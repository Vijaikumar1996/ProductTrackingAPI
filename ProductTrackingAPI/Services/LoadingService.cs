using Microsoft.EntityFrameworkCore;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Entities;
using ProductTrackingAPI.Interface;
using System.ComponentModel.DataAnnotations;

namespace ProductTrackingAPI.Services;

public class LoadingService : ILoadingService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;


    public LoadingService(
        ApplicationDbContext context, IEmailService emailService, IAuditService auditService)
    {
        _context = context;
        _emailService = emailService;
        _auditService = auditService;
    }

    public async Task<LoadingScanResponseDto> ScanAsync(
      LoadingScanRequestDto request,
      long userId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var saleOrder =
                await _context.SaleOrders
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.SaleOrderId);

            if (saleOrder == null)
            {
                throw new Exception(
                    "Sale Order not found");
            }

            var huItem =
                await _context.HuItems
                    .FirstOrDefaultAsync(x =>
                        x.SaleOrderId ==
                        request.SaleOrderId &&
                        x.HuNumberBarcode ==
                        request.HuNumber);

            if (huItem == null)
            {
                throw new Exception(
                    "Invalid HU Number");
            }

            if (huItem.Status == "LOADED")
            {
                throw new Exception(
                    "HU already loaded");
            }

            if (huItem.Status == "LOADING")
            {
                throw new Exception(
                    "HU already scanned");
            }

            if (huItem.Status != "RECEIVED")
            {
                throw new Exception(
                    "HU is not received at hub");
            }

            var scanTransaction =
                new ScanTransaction
                {
                    HuItemId =
                        huItem.Id,

                    Stage =
                        "VEHICLE_LOADING",

                    ScannedBy =
                        userId,

                    ScannedAt =
                        DateTime.UtcNow
                };

            _context.ScanTransactions
                .Add(scanTransaction);

            huItem.Status =
                "LOADING";

            huItem.UpdatedAt =
                DateTime.UtcNow;

            saleOrder.LoadedHuCount += 1;

            saleOrder.Status =
                saleOrder.LoadedHuCount ==
                saleOrder.ReceivedHuCount
                    ? "LOADED"
                    : "LOADING";

            saleOrder.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new LoadingScanResponseDto
            {
                ExpectedCount =
                    saleOrder.ReceivedHuCount,

                ActualCount =
                    saleOrder.LoadedHuCount,

                Status =
                    saleOrder.Status
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CompleteAsync(
    LoadingCompleteRequestDto request,
    long userId)
    {
        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var saleOrder =
                await _context.SaleOrders
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.SaleOrderId);

            if (saleOrder == null)
            {
                throw new ValidationException(
                    "Sale Order not found");
            }

            var dispatch =
                new VehicleDispatch
                {
                    SaleOrderId =
                        saleOrder.Id,

                    VehicleNumber =
                        request.VehicleNumber,

                    DriverName =
                        request.DriverName,

                    DriverMobile =
                        request.MobileNumber,

                    CreatedAt =
                        DateTime.UtcNow,

                    CreatedBy =
                        userId
                };

            _context.VehicleDispatches
                .Add(dispatch);

            var loadedHuItems =
                await _context.HuItems
                    .Where(x =>
                        x.SaleOrderId ==
                        saleOrder.Id &&
                        x.Status ==
                        "LOADING")
                    .ToListAsync();

            foreach (var hu in loadedHuItems)
            {
                hu.Status = "LOADED";

                hu.UpdatedAt =
                    DateTime.UtcNow;
            }

            if (saleOrder.ReceivedHuCount ==
                saleOrder.LoadedHuCount)
            {
                saleOrder.Status =
                    "LOADED";

                dispatch.Status =
                    "LOADED";
            }
            else
            {
                var missingHuNumbers =
                    await _context.HuItems
                        .Where(x =>
                            x.SaleOrderId ==
                            saleOrder.Id &&
                            x.Status ==
                            "RECEIVED")
                        .Select(x =>
                            x.HuNumberBarcode)
                        .ToListAsync();

                saleOrder.Status =
                    "MISMATCH";

                dispatch.Status =
                    "MISMATCH";

                var mismatchLog =
                    new MismatchLog
                    {
                        SaleOrderId =
                            saleOrder.Id,

                        Stage =
                            "VEHICLE_LOADING",

                        ExpectedCount =
                            saleOrder.ReceivedHuCount,

                        ActualCount =
                            saleOrder.LoadedHuCount,

                        MissingCount =
                            missingHuNumbers.Count,

                        MissingHuNumbers =
                            string.Join(
                                ",",
                                missingHuNumbers),

                        CreatedAt =
                            DateTime.UtcNow
                    };

                _context.MismatchLogs
                    .Add(mismatchLog);

                await _emailService
                    .SendMismatchEmailAsync(
                        saleOrder.Id,
                        saleOrder.SaleOrderNo,
                        "VEHICLE_LOADING",
                        saleOrder.ReceivedHuCount,
                        saleOrder.LoadedHuCount,
                        missingHuNumbers);
            }

            saleOrder.UpdatedBy =
                userId;

            saleOrder.UpdatedAt =
                DateTime.UtcNow;

            saleOrder.LoadedBy = userId;
            saleOrder.LoadedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            
             await _auditService.LogAsync(
                    saleOrder.Status == "MISMATCH"
                        ? "LOADING_MISMATCH"
                        : "LOADING_COMPLETED",

                    "SaleOrder",

                    saleOrder.Id.ToString(),

                    $"Sale Order: {saleOrder.SaleOrderNo}, " +
                    $"Expected: {saleOrder.ReceivedHuCount}, " +
                    $"Loaded: {saleOrder.LoadedHuCount}, " +
                    $"Vehicle: {request.VehicleNumber}, " +
                    $"Status: {saleOrder.Status}",

                    userId);
            
            
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
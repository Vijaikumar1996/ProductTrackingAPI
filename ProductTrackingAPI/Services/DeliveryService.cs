using Microsoft.EntityFrameworkCore;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Entities;
using ProductTrackingAPI.Interface;
using System.ComponentModel.DataAnnotations;

namespace ProductTrackingAPI.Services;

public class DeliveryService : IDeliveryService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;

    public DeliveryService(
        ApplicationDbContext context,
        IEmailService emailService,
        IAuditService auditService)
    {
        _context = context;
        _emailService = emailService;
        _auditService = auditService;
    }

    public async Task<DeliveryScanResponseDto> ScanAsync(
    DeliveryScanRequestDto request,
    long userId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var dispatch =
                await _context.VehicleDispatches
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.DispatchId);

            if (dispatch == null)
                throw new Exception(
                    "Dispatch not found");

            var huItem =
                await _context.HuItems
                    .FirstOrDefaultAsync(x =>
                        x.SaleOrderId ==
                        dispatch.SaleOrderId &&
                        x.HuNumberBarcode ==
                        request.HuNumber);

            if (huItem == null)
                throw new Exception(
                    "Invalid HU Number");

            if (huItem.Status == "DELIVERED")
                throw new Exception(
                    "HU already delivered");

            if (huItem.Status == "DELIVERING")
                throw new Exception(
                    "HU already scanned");

            if (huItem.Status != "LOADED")
                throw new Exception(
                    "HU not loaded");

            var saleOrder =
                await _context.SaleOrders
                    .FirstOrDefaultAsync(x =>
                        x.Id ==
                        dispatch.SaleOrderId);

            if (saleOrder == null)
                throw new Exception(
                    "Sale Order not found");

            var scan =
                new ScanTransaction
                {
                    HuItemId = huItem.Id,

                    VehicleDispatchId =
                        dispatch.Id,

                    Stage = "DELIVERY",

                    ScannedBy = userId,

                    ScannedAt =
                        DateTime.UtcNow
                };

            _context.ScanTransactions.Add(scan);

            huItem.Status = "DELIVERING";

            huItem.UpdatedAt =
                DateTime.UtcNow;

            saleOrder.DeliveredHuCount += 1;

            saleOrder.Status =
                saleOrder.DeliveredHuCount ==
                saleOrder.LoadedHuCount
                    ? "DELIVERED"
                    : "DELIVERING";

            saleOrder.UpdatedAt =
                DateTime.UtcNow;

            dispatch.Status =
                saleOrder.DeliveredHuCount ==
                saleOrder.LoadedHuCount
                    ? "DELIVERED"
                    : "DELIVERING";

            dispatch.UpdatedAt =
                DateTime.UtcNow;

            dispatch.UpdatedBy =
                userId;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new DeliveryScanResponseDto
            {
                ExpectedCount =
                    saleOrder.LoadedHuCount,

                ActualCount =
                    saleOrder.DeliveredHuCount,

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
    DeliveryCompleteRequestDto request,
    long userId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var dispatch =
                await _context.VehicleDispatches
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.DispatchId);

            if (dispatch == null)
            {
                throw new ValidationException(
                    "Dispatch not found");
            }

            var saleOrder =
                await _context.SaleOrders
                    .FirstOrDefaultAsync(x =>
                        x.Id == dispatch.SaleOrderId);

            if (saleOrder == null)
            {
                throw new ValidationException(
                    "Sale Order not found");
            }

            /* ---------------- Mark Delivered HUs ---------------- */

            var deliveredHuItems =
                await _context.HuItems
                    .Where(x =>
                        x.SaleOrderId == saleOrder.Id &&
                        x.Status == "DELIVERING")
                    .ToListAsync();

            foreach (var hu in deliveredHuItems)
            {
                hu.Status = "DELIVERED";

                hu.UpdatedAt =
                    DateTime.UtcNow;
            }

            /* ---------------- Complete Delivery ---------------- */

            if (saleOrder.LoadedHuCount ==
                saleOrder.DeliveredHuCount)
            {
                saleOrder.Status =
                    "DELIVERED";

                dispatch.Status =
                    "DELIVERED";
            }
            else
            {
                var missingHuNumbers =
                    await _context.HuItems
                        .Where(x =>
                            x.SaleOrderId == saleOrder.Id &&
                            x.Status == "LOADED")
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
                            "DELIVERY",

                        ExpectedCount =
                            saleOrder.LoadedHuCount,

                        ActualCount =
                            saleOrder.DeliveredHuCount,

                        MissingCount =
                            missingHuNumbers.Count,

                        MissingHuNumbers =
                            string.Join(
                                ",",
                                missingHuNumbers),

                        Remarks =
                            "Delivery mismatch",

                        CreatedAt =
                            DateTime.UtcNow
                    };

                _context.MismatchLogs
                    .Add(mismatchLog);

                await _emailService
                    .SendMismatchEmailAsync(
                        saleOrder.Id,
                        saleOrder.SaleOrderNo,
                        "DELIVERY",
                        saleOrder.LoadedHuCount,
                        saleOrder.DeliveredHuCount,
                        missingHuNumbers);
            }

            saleOrder.UpdatedBy =
                userId;

            saleOrder.UpdatedAt =
                DateTime.UtcNow;

            dispatch.UpdatedBy =
                userId;

            dispatch.UpdatedAt =
                DateTime.UtcNow;

            saleOrder.DeliveredBy = userId;
            saleOrder.DeliveredAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            
            await _auditService.LogAsync(
                    saleOrder.Status == "MISMATCH"
                        ? "DELIVERY_MISMATCH"
                        : "DELIVERY_COMPLETED",

                    "SaleOrder",

                    saleOrder.Id.ToString(),

                    $"Sale Order: {saleOrder.SaleOrderNo}, " +
                    $"Loaded: {saleOrder.LoadedHuCount}, " +
                    $"Delivered: {saleOrder.DeliveredHuCount}, " +
                    $"Vehicle: {dispatch.VehicleNumber}, " +
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
using Microsoft.EntityFrameworkCore;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Entities;
using ProductTrackingAPI.Interface;
using System.ComponentModel.DataAnnotations;

namespace ProductTrackingAPI.Services;

public class HubService : IHubService
{
    private readonly ApplicationDbContext _context;

    private readonly IEmailService _emailService;

    private readonly IAuditService _auditService;

    public HubService(
        ApplicationDbContext context, IEmailService emailService, IAuditService auditService)
    {
        _context = context;
        _emailService = emailService;  
        _auditService = auditService;
    }

    public async Task<HubScanResponseDto> ScanAsync(
     HubScanRequestDto request,
     long userId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var huItem =
                await _context.HuItems
                    .FirstOrDefaultAsync(x =>
                        x.SaleOrderId == request.SaleOrderId &&
                        x.HuNumberBarcode == request.HuNumber);

            if (huItem == null)
            {
                throw new Exception(
                    "Invalid HU Number");
            }

            if (huItem.Status == "RECEIVED")
            {
                throw new Exception(
                    "HU already received");
            }

            var saleOrder =
                await _context.SaleOrders
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.SaleOrderId);

            if (saleOrder == null)
            {
                throw new Exception(
                    "Sale Order not found");
            }

            var scanTransaction =
                new ScanTransaction
                {
                    HuItemId = huItem.Id,
                    Stage = "HUB_RECEIVING",
                    ScannedBy = userId,
                    ScannedAt = DateTime.UtcNow
                };

            _context.ScanTransactions
                .Add(scanTransaction);

            huItem.Status = "RECEIVED";
            huItem.UpdatedAt = DateTime.UtcNow;

            saleOrder.ReceivedHuCount += 1;

            saleOrder.Status =
                saleOrder.ReceivedHuCount ==
                saleOrder.ExpectedHuCount
                    ? "RECEIVED"
                    : "RECEIVING";

            saleOrder.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new HubScanResponseDto
            {
                ExpectedCount =
                    saleOrder.ExpectedHuCount,

                ActualCount =
                    saleOrder.ReceivedHuCount,

                //PendingCount =
                //    saleOrder.ExpectedHuCount -
                //    saleOrder.ReceivedHuCount,

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

    public async Task<HubSaleOrderDetailsDto> GetSaleOrderAsync(
    long saleOrderId)
    {
        var saleOrder = await _context.SaleOrders
            .FirstOrDefaultAsync(x => x.Id == saleOrderId);

        if (saleOrder == null)
        {
            throw new Exception("Sale Order not found");
        }

        return new HubSaleOrderDetailsDto
        {
            SaleOrderId = saleOrder.Id,

            SaleOrderNo = saleOrder.SaleOrderNo,

            ExpectedHuCount = saleOrder.ExpectedHuCount,

            ReceivedHuCount = saleOrder.ReceivedHuCount,

            PendingHuCount =
                saleOrder.ExpectedHuCount -
                saleOrder.ReceivedHuCount,

            Status = saleOrder.Status
        };
    }

    public async Task<List<HubSaleOrderListDto>> GetSaleOrdersAsync()
    {
        return await _context.SaleOrders
            .Where(x =>
             x.Status == "UPLOADED" ||
             x.Status == "RECEIVING")
            .OrderByDescending(x => x.Id)
            .Select(x => new HubSaleOrderListDto
            {
                Id = x.Id,

                SaleOrderNo = x.SaleOrderNo,

                ExpectedHuCount = x.ExpectedHuCount,

                ReceivedHuCount = x.ReceivedHuCount,

                Status = x.Status
            })
            .ToListAsync();
    }

    public async Task CompleteReceivingAsync(
     long saleOrderId,
     long userId)
    {
        var saleOrder =
            await _context.SaleOrders
                .FirstOrDefaultAsync(x =>
                    x.Id == saleOrderId);

        if (saleOrder == null)
        {
            throw new ValidationException(
                "Sale Order not found");
        }

        if (saleOrder.ExpectedHuCount ==
            saleOrder.ReceivedHuCount)
        {
            saleOrder.Status = "RECEIVED";
        }
        else
        {
            var missingCount =
                saleOrder.ExpectedHuCount -
                saleOrder.ReceivedHuCount;

            var missingHuNumbers =
                await _context.HuItems
                    .Where(x =>
                        x.SaleOrderId == saleOrderId &&
                        x.Status != "RECEIVED")
                    .Select(x =>
                        x.HuNumberBarcode)
                    .ToListAsync();

            saleOrder.Status = "MISMATCH";

            var mismatchLog =
                new MismatchLog
                {
                    SaleOrderId = saleOrder.Id,

                    Stage = "HUB_RECEIVING",

                    ExpectedCount =
                        saleOrder.ExpectedHuCount,

                    ActualCount =
                        saleOrder.ReceivedHuCount,

                    MissingCount =
                        missingCount,

                    MissingHuNumbers =
                        string.Join(",", missingHuNumbers),

                    CreatedAt =
                        DateTime.UtcNow
                };

            _context.MismatchLogs.Add(
                mismatchLog);

            await _emailService
                .SendMismatchEmailAsync(
                    saleOrder.Id,
                    saleOrder.SaleOrderNo,
                    "HUB_RECEIVING",
                    saleOrder.ExpectedHuCount,
                    saleOrder.ReceivedHuCount,
                    missingHuNumbers);
        }

        saleOrder.UpdatedBy = userId;
        saleOrder.UpdatedAt = DateTime.UtcNow;
        saleOrder.ReceivedBy = userId;
        saleOrder.ReceivedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            saleOrder.Status == "MISMATCH"
                ? "RECEIVING_MISMATCH"
                : "RECEIVING_COMPLETED",
            "SaleOrder",
            saleOrder.Id.ToString(),
            $"Sale Order: {saleOrder.SaleOrderNo}, " +
            $"Expected: {saleOrder.ExpectedHuCount}, " +
            $"Received: {saleOrder.ReceivedHuCount}, " +
            $"Status: {saleOrder.Status}",
            userId);
    }
}
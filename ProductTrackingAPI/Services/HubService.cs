using Microsoft.EntityFrameworkCore;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Entities;
using ProductTrackingAPI.Interface;
using System.ComponentModel.DataAnnotations;
using static ProductTrackingAPI.DTOs.HubTruckDTO;

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


    public async Task<HubTruckScanResponse> ScanHubHuAsync(
    HubTruckScanRequest request,
    long userId)
    {
        if (string.IsNullOrWhiteSpace(request.VehicleNumber))
            throw new Exception("Vehicle number is required.");

        if (string.IsNullOrWhiteSpace(request.HuNumber))
            throw new Exception("HU number is required.");


        var vehicleNumber =
            request.VehicleNumber
                .Trim()
                .ToUpper();

        var huNumber =
            request.HuNumber
                .Trim()
                .ToUpper();


        // Find HU and its Sale Order
        var huItem = await _context.HuItems
            .Include(x => x.SaleOrder)
            .FirstOrDefaultAsync(x =>
                x.HuNumberBarcode
                    .Trim()
                    .ToUpper() == huNumber);


        if (huItem == null)
            throw new Exception(
                $"HU number '{request.HuNumber}' was not found.");


        var saleOrder = huItem.SaleOrder;


        if (saleOrder == null)
            throw new Exception(
                "Sale Order was not found for this HU.");


      


        if (saleOrder.ShipmentDate != request.ShipmentDate)
        {
            throw new Exception(
                $"HU belongs to shipment date " +
                $"{saleOrder.ShipmentDate:dd-MMM-yyyy}, " +
                $"not {request.ShipmentDate:dd-MMM-yyyy}.");
        }


        // Verify vehicle belongs to this Sale Order
        var belongsToVehicle =
            await _context.SaleOrderTransportDetails
                .AnyAsync(x =>
                    x.SaleOrderId == saleOrder.Id &&
                    x.InboundVehicleNumber != null &&
                    x.InboundVehicleNumber
                        .Trim()
                        .ToUpper() == vehicleNumber);


        if (!belongsToVehicle)
        {
            throw new Exception(
                $"HU '{request.HuNumber}' belongs to Sale Order " +
                $"{saleOrder.SaleOrderNo}, which is not part of " +
                $"vehicle {vehicleNumber}.");
        }


        // Check duplicate HUB_RECEIVING scan
        var alreadyScanned =
            await _context.ScanTransactions
                .AnyAsync(x =>
                    x.HuItemId == huItem.Id &&
                    x.Stage == "HUB_RECEIVING");


        if (alreadyScanned)
        {
            throw new Exception(
                $"HU '{request.HuNumber}' has already been received.");
        }


        // Create scan transaction
        var scanTransaction = new ScanTransaction
        {
            HuItemId = huItem.Id,

            Stage = "HUB_RECEIVING",

            ScannedBy = userId,

            ScannedAt = DateTime.UtcNow
        };


        _context.ScanTransactions.Add(scanTransaction);


        // Update Sale Order received count
        saleOrder.ReceivedHuCount =
            saleOrder.ReceivedHuCount + 1;


        if (saleOrder.ReceivedHuCount >=
            saleOrder.ExpectedHuCount)
        {
            saleOrder.Status = "RECEIVED";
        }
        else
        {
            saleOrder.Status = "RECEIVING";
        }


        saleOrder.ReceivedBy = userId;

        saleOrder.ReceivedAt = DateTime.UtcNow;

        saleOrder.UpdatedBy = userId;

        saleOrder.UpdatedAt = DateTime.UtcNow;


        await _context.SaveChangesAsync();


        // Get all Sale Orders belonging to this truck shipment
        var truckSaleOrders =
            await _context.SaleOrderTransportDetails
                .Where(x =>
                    x.InboundVehicleNumber != null &&
                    x.InboundVehicleNumber
                        .Trim()
                        .ToUpper() == vehicleNumber &&                   
                    x.SaleOrder.ShipmentDate ==
                        request.ShipmentDate)
                .Select(x => x.SaleOrder)
                .ToListAsync();


        var truckExpectedCount =
            truckSaleOrders.Sum(x =>
                x.ExpectedHuCount);


        var truckReceivedCount =
            truckSaleOrders.Sum(x =>
                x.ReceivedHuCount);


        var truckPendingCount =
            Math.Max(
                truckExpectedCount -
                truckReceivedCount,
                0);


        return new HubTruckScanResponse
        {
            VehicleNumber = vehicleNumber,

            ShipmentDate = request.ShipmentDate,

            SaleOrderId = saleOrder.Id,

            SaleOrderNo = saleOrder.SaleOrderNo,

            HuNumber = huNumber,

            TruckExpectedCount = truckExpectedCount,

            TruckReceivedCount = truckReceivedCount,

            TruckPendingCount = truckPendingCount,

            SaleOrderExpectedCount =
                saleOrder.ExpectedHuCount,

            SaleOrderReceivedCount =
                saleOrder.ReceivedHuCount,

            SaleOrderPendingCount =
                Math.Max(
                    saleOrder.ExpectedHuCount -
                    saleOrder.ReceivedHuCount,
                    0),

            Message =
                $"HU {huNumber} received successfully."
        };
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

    public async Task<List<HubTruckListDto>> GetHubTrucksAsync()
    {
        var data = await _context.SaleOrderTransportDetails
            .Where(x =>
                x.InboundVehicleNumber != null &&               
                (
                    x.SaleOrder.Status == "UPLOADED" ||
                    x.SaleOrder.Status == "RECEIVING"
                ))
            .Select(x => new
            {
                SaleOrderId = x.SaleOrderId,

                SaleOrderNo = x.SaleOrder.SaleOrderNo,

                ShipmentDate = x.SaleOrder.ShipmentDate,

                VehicleNumber = x.InboundVehicleNumber!,

                DriverName = x.InboundDriverName,

                DriverMobile = x.InboundDriverMobile,

                ExpectedHuCount = x.SaleOrder.ExpectedHuCount,

                ReceivedHuCount = x.SaleOrder.ReceivedHuCount,

                Status = x.SaleOrder.Status
            })
            .ToListAsync();


        var result = data
            .GroupBy(x => new
            {
                VehicleNumber = x.VehicleNumber.Trim().ToUpper(),

                x.ShipmentDate
            })
            .Select(group =>
            {
                var saleOrders = group
                    .GroupBy(x => x.SaleOrderId)
                    .Select(soGroup =>
                    {
                        var so = soGroup.First();

                        return new HubTruckSaleOrderDto
                        {
                            SaleOrderId = so.SaleOrderId,

                            SaleOrderNo = so.SaleOrderNo,

                            ExpectedHuCount = so.ExpectedHuCount,

                            ReceivedHuCount = so.ReceivedHuCount,

                            PendingHuCount =
                                Math.Max(
                                    so.ExpectedHuCount -
                                    so.ReceivedHuCount,
                                    0),

                            Status = so.Status
                        };
                    })
                    .OrderBy(x => x.SaleOrderNo)
                    .ToList();


                var expected = saleOrders.Sum(x => x.ExpectedHuCount);

                var received = saleOrders.Sum(x => x.ReceivedHuCount);

                var pending = Math.Max(expected - received, 0);


                string status;

                if (received == 0)
                {
                    status = "UPLOADED";
                }
                else if (received < expected)
                {
                    status = "RECEIVING";
                }
                else
                {
                    status = "RECEIVED";
                }


                var first = group.First();


                return new HubTruckListDto
                {
                    VehicleNumber = group.Key.VehicleNumber,

                    ShipmentDate = group.Key.ShipmentDate,

                    DriverName = first.DriverName,

                    DriverMobile = first.DriverMobile,

                    ExpectedHuCount = expected,

                    ReceivedHuCount = received,

                    PendingHuCount = pending,

                    Status = status,

                    SaleOrders = saleOrders
                };
            })
            .OrderByDescending(x => x.ShipmentDate)
            .ThenBy(x => x.VehicleNumber)
            .ToList();


        return result;
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

    public async Task CompleteHubReceiveAsync(
     CompleteHubTruckRequest request,
     long userId)
    {
        if (string.IsNullOrWhiteSpace(request.VehicleNumber))
        {
            throw new ValidationException(
                "Vehicle number is required");
        }

        var vehicleNumber =
            request.VehicleNumber
                .Trim()
                .ToUpper();


        // Get all Sale Orders belonging to
        // this Vehicle + Shipment Date
        var saleOrders =
            await _context.SaleOrderTransportDetails
                .Where(x =>
                    x.InboundVehicleNumber != null &&
                    x.InboundVehicleNumber
                        .Trim()
                        .ToUpper() == vehicleNumber &&                   
                    x.SaleOrder.ShipmentDate ==
                        request.ShipmentDate &&

                    (
                        x.SaleOrder.Status == "UPLOADED" ||
                        x.SaleOrder.Status == "RECEIVING"
                    ))
                .Select(x => x.SaleOrder)
                .Distinct()
                .ToListAsync();


        if (!saleOrders.Any())
        {
            throw new ValidationException(
                "No active Sale Orders found for this vehicle and shipment date.");
        }


        var expectedCount =
            saleOrders.Sum(x =>
                x.ExpectedHuCount);


        var actualCount =
            saleOrders.Sum(x =>
                x.ReceivedHuCount);


        var missingCount =
            Math.Max(
                expectedCount - actualCount,
                0);


        var hasMismatch =
            expectedCount != actualCount;


        // If there is a mismatch and the user has not
        // confirmed "Continue Anyway", stop here.
        if (hasMismatch &&
            !request.ContinueWithMismatch)
        {
            throw new ValidationException(
                $"HU mismatch. Expected: {expectedCount}, " +
                $"Received: {actualCount}, " +
                $"Missing: {missingCount}.");
        }


        foreach (var saleOrder in saleOrders)
        {
            var saleOrderMismatch =
                saleOrder.ExpectedHuCount !=
                saleOrder.ReceivedHuCount;


            if (saleOrderMismatch)
            {
                var missingHuNumbers =
                    await _context.HuItems
                        .Where(x =>
                            x.SaleOrderId == saleOrder.Id &&

                            !_context.ScanTransactions
                                .Any(scan =>
                                    scan.HuItemId == x.Id &&
                                    scan.Stage ==
                                        "HUB_RECEIVING"))
                        .Select(x =>
                            x.HuNumberBarcode)
                        .ToListAsync();


                var saleOrderMissingCount =
                    Math.Max(
                        saleOrder.ExpectedHuCount -
                        saleOrder.ReceivedHuCount,
                        0);


                // -----------------------------------------
                // UPDATE SALE ORDER
                // -----------------------------------------

                saleOrder.Status = "MISMATCH";


                saleOrder.UpdatedBy = userId;

                saleOrder.UpdatedAt =
                    DateTime.UtcNow;

                saleOrder.ReceivedBy = userId;

                saleOrder.ReceivedAt =
                    DateTime.UtcNow;


                // -----------------------------------------
                // CREATE MISMATCH LOG
                // -----------------------------------------

                var mismatchLog =
                    new MismatchLog
                    {
                        SaleOrderId =
                            saleOrder.Id,

                        Stage =
                            "HUB_RECEIVING",

                        ExpectedCount =
                            saleOrder.ExpectedHuCount,

                        ActualCount =
                            saleOrder.ReceivedHuCount,

                        MissingCount =
                            saleOrderMissingCount,

                        MissingHuNumbers =
                            string.Join(
                                ",",
                                missingHuNumbers),

                        CreatedAt =
                            DateTime.UtcNow,

                        Remarks =
                            $"Vehicle: {vehicleNumber}, " +
                            $"Shipment Date: " +
                            $"{request.ShipmentDate:dd-MMM-yyyy}"
                    };


                _context.MismatchLogs.Add(
                    mismatchLog);


                // -----------------------------------------
                // SEND MISMATCH EMAIL
                // -----------------------------------------

                await _emailService
                    .SendMismatchEmailAsync(
                        saleOrder.Id,
                        saleOrder.SaleOrderNo,
                        "HUB_RECEIVING",
                        saleOrder.ExpectedHuCount,
                        saleOrder.ReceivedHuCount,
                        missingHuNumbers);
            }
            else
            {
                // -----------------------------------------
                // FULLY RECEIVED
                // -----------------------------------------

                saleOrder.Status =
                    "RECEIVED";


                saleOrder.UpdatedBy =
                    userId;

                saleOrder.UpdatedAt =
                    DateTime.UtcNow;

                saleOrder.ReceivedBy =
                    userId;

                saleOrder.ReceivedAt =
                    DateTime.UtcNow;
            }


            // -----------------------------------------
            // AUDIT LOG
            // -----------------------------------------

            await _auditService.LogAsync(

                saleOrder.Status == "MISMATCH"
                    ? "RECEIVING_MISMATCH"
                    : "RECEIVING_COMPLETED",

                "SaleOrder",

                saleOrder.Id.ToString(),

                $"Sale Order: {saleOrder.SaleOrderNo}, " +
                $"Vehicle: {vehicleNumber}, " +
                $"Shipment Date: {request.ShipmentDate:dd-MMM-yyyy}, " +
                $"Expected: {saleOrder.ExpectedHuCount}, " +
                $"Received: {saleOrder.ReceivedHuCount}, " +
                $"Status: {saleOrder.Status}",

                userId);
        }


        // Save all Sale Order updates and mismatch logs
        await _context.SaveChangesAsync();
    }
}
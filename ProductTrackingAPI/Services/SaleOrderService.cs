using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Entities;
using ProductTrackingAPI.Exceptions;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Services;

public class SaleOrderService : ISaleOrderService
{
    private readonly ApplicationDbContext _context;

    private readonly IAuditService _auditService;
   
    public SaleOrderService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task UploadAsync(
    UploadSaleOrderRequest request,
    long userId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var saleOrderExists = await _context.SaleOrders
                .AnyAsync(x => x.SaleOrderNo == request.SaleOrderNo);

            if (saleOrderExists)
            {
                throw new ValidationException(
                $"Sale Order '{request.SaleOrderNo}' already exists.");
            }   

            var saleOrder = new SaleOrder
            {
                SaleOrderNo = request.SaleOrderNo,
                ShipmentDate = request.ShipmentDate,
                FileName = request.File.FileName,
                Status = "UPLOADED",
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                ExpectedHuCount = 0,
                ReceivedHuCount = 0,
                LoadedHuCount = 0,
                DeliveredHuCount = 0
            };

            _context.SaleOrders.Add(saleOrder);

            await _context.SaveChangesAsync();

                                       
                var huItems = new List<HuItem>();

                var validationErrors = new List<string>();

                var uploadedHuNumbers = new HashSet<string>();

                using var stream = request.File.OpenReadStream();

                using var workbook = new XLWorkbook(stream);

                var worksheet = workbook.Worksheet(1);

                var rows = worksheet.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    var rowNumber = row.RowNumber();

                    var huNumber =
                        row.Cell(1).GetString()?.Trim();

                    var equipmentNumber =
                        row.Cell(2).GetString()?.Trim();

                    var materialId =
                        row.Cell(4).GetString()?.Trim();

                    if (string.IsNullOrWhiteSpace(huNumber))
                    {
                        validationErrors.Add(
                            $"Row {rowNumber}: HU Number is required");

                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(equipmentNumber))
                    {
                        validationErrors.Add(
                            $"Row {rowNumber}: Equipment Number is required");
                    }

                    if (string.IsNullOrWhiteSpace(materialId))
                    {
                        validationErrors.Add(
                            $"Row {rowNumber}: Material Id is required");
                    }

                    if (!uploadedHuNumbers.Add(huNumber))
                    {
                        validationErrors.Add(
                            $"Row {rowNumber}: Duplicate HU Number '{huNumber}' found in file");
                    }

                    huItems.Add(new HuItem
                    {
                        SaleOrderId = saleOrder.Id,

                        HuNumberBarcode = huNumber,

                        EquipmentNumber = equipmentNumber,                       

                        MaterialId = materialId,

                        OrderItemNumber = row.Cell(3).GetString().Trim(),

                        MaterialDescription = row.Cell(5).GetString().Trim(),

                        PackageDescription = row.Cell(6).GetString().Trim(),

                        MaterialQuantity = row.Cell(7).GetValue<decimal?>(),

                        Uom = row.Cell(8).GetString().Trim(),
                       
                        Length = row.Cell(9).GetValue<decimal?>(),

                        Width = row.Cell(10).GetValue<decimal?>(),

                        Height = row.Cell(11).GetValue<decimal?>(),

                        NetWeight = row.Cell(12).GetValue<decimal?>(),

                        GrossWeight = row.Cell(13).GetValue<decimal?>(),

                        Volume = row.Cell(14).GetValue<decimal?>(),

                        Remarks = row.Cell(15).GetString().Trim(),

                        Status = "UPLOADED",

                        CreatedAt = DateTime.UtcNow,

                        UpdatedAt = DateTime.UtcNow
                    });
                }

            if (!huItems.Any())
            {
                throw new ValidationException(
                    "No HU records found in uploaded file.");
            }

                var duplicateHuNumbers = await _context.HuItems
                    .Where(x =>
                        uploadedHuNumbers.Contains(
                            x.HuNumberBarcode))
                    .Select(x => x.HuNumberBarcode)
                    .ToListAsync();

                if (duplicateHuNumbers.Any())
                {
                    validationErrors.Add(
                        $"Duplicate HU Numbers already exist in system: {string.Join(", ", duplicateHuNumbers)}");
                }

            if (validationErrors.Any())
            {
                
                throw new ValidationException(
                    string.Join(
                    Environment.NewLine,
                    validationErrors));
            }

            saleOrder.ExpectedHuCount = huItems.Count;

            _context.HuItems.AddRange(huItems);

           

            var transport = new SaleOrderTransportDetail
            {
                SaleOrderId = saleOrder.Id,
                InboundVehicleNumber = request.InboundVehicleNumber,
                InboundDriverName = request.InboundDriverName,
                InboundDriverMobile = request.InboundDriverMobile,
                CreatedAt = DateTime.UtcNow
            };

            _context.SaleOrderTransportDetails.Add(transport);

            var delivery = new SaleOrderDeliveryDetail
            {
                SaleOrderId = saleOrder.Id,
                CustomerName = request.CustomerName,
                DeliveryAddress = request.DeliveryAddress,
                ContactPerson = request.ContactPerson,
                ContactNumber = request.ContactNumber,
                // ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                // Remarks = request.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            _context.SaleOrderDeliveryDetails.Add(delivery);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();            

            await _auditService.LogAsync(
                action: "Shipment Uploaded",
                entityName: "SaleOrder",
                entityId: saleOrder.Id.ToString(),
                description:
                    $"Sale Order {saleOrder.SaleOrderNo} uploaded with {saleOrder.ExpectedHuCount} HU items",
                userId: userId);

        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<SaleOrderListDto>>
GetOrdersAsync(
    string? saleOrderNo,
    DateTime? fromDate,
    DateTime? toDate,
    string? status)
    {
        var query =
            from so in _context.SaleOrders

            join u in _context.Users
                on so.CreatedBy equals u.Id

            select new
            {
                SaleOrder = so,
                User = u
            };

        if (!string.IsNullOrWhiteSpace(saleOrderNo))
        {
            query = query.Where(x =>
                x.SaleOrder.SaleOrderNo.Contains(
                    saleOrderNo));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x =>
                x.SaleOrder.Status == status);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x =>
                x.SaleOrder.ShipmentDate.Date >=
                fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x =>
                x.SaleOrder.ShipmentDate.Date <=
                toDate.Value.Date);
        }

        return await query

            .OrderByDescending(x =>
                x.SaleOrder.CreatedAt)

            .Select(x =>
                new SaleOrderListDto
                {
                    Id =
                        x.SaleOrder.Id,

                    SaleOrderNo =
                        x.SaleOrder.SaleOrderNo,

                    ShipmentDate =
                        x.SaleOrder.ShipmentDate,

                    Status =
                        x.SaleOrder.Status,

                    ExpectedHuCount =
                        x.SaleOrder.ExpectedHuCount,

                    CreatedByName =
                        x.User.Name,

                    CreatedAt =
                        x.SaleOrder.CreatedAt
                })

            .ToListAsync();
    }

    public async Task<SaleOrderDetailDto?>
GetOrderDetailAsync(long saleOrderId)
    {
        return await (
            from so in _context.SaleOrders

            join u in _context.Users
                on so.CreatedBy equals u.Id

            join td in
                _context.SaleOrderTransportDetails
                on so.Id equals td.SaleOrderId
                into transportGroup

            from td in transportGroup.DefaultIfEmpty()

            join dd in
                _context.SaleOrderDeliveryDetails
                on so.Id equals dd.SaleOrderId
                into deliveryGroup

            from dd in deliveryGroup.DefaultIfEmpty()

            where so.Id == saleOrderId

            select new SaleOrderDetailDto
            {
                Id = so.Id,

                SaleOrderNo = so.SaleOrderNo,

                ShipmentDate = so.ShipmentDate,

                Status = so.Status,

                ExpectedHuCount = so.ExpectedHuCount,

                ReceivedHuCount = so.ReceivedHuCount,

                LoadedHuCount = so.LoadedHuCount,

                DeliveredHuCount = so.DeliveredHuCount,

                CustomerName = dd.CustomerName,

                ContactPerson = dd.ContactPerson,

                ContactNumber = dd.ContactNumber,

                DeliveryAddress = dd.DeliveryAddress,

                VehicleNumber = td.InboundVehicleNumber,

                InboundDriverName = td.InboundDriverName,

                InboundDriverMobile = td.InboundDriverMobile,

                CreatedByName = u.Name,

                CreatedAt = so.CreatedAt
            })
            .FirstOrDefaultAsync();
    }
}
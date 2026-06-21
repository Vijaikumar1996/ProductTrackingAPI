using Microsoft.EntityFrameworkCore;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Entities;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Services;

public class VehicleDispatchService : IVehicleDispatchService
{
    private readonly ApplicationDbContext _context;

    public VehicleDispatchService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(
        CreateVehicleDispatchRequestDto request,
        long userId)
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

        if (saleOrder.Status != "RECEIVED")
        {
            throw new Exception(
                "Sale Order is not ready for dispatch");
        }

        var dispatch = new VehicleDispatch
        {
            SaleOrderId = request.SaleOrderId,

            VehicleNumber = request.VehicleNumber,

            DriverName = request.DriverName,

            DriverMobile = request.DriverMobile,

            Status = "CREATED",

            CreatedBy = userId,

            CreatedAt = DateTime.UtcNow
        };

        _context.VehicleDispatches.Add(dispatch);

        await _context.SaveChangesAsync();
    }

    public async Task<List<VehicleDispatchListDto>>
        GetDispatchesAsync()
    {
        return await
            (from vd in _context.VehicleDispatches
             join so in _context.SaleOrders
                on vd.SaleOrderId equals so.Id
             orderby vd.Id descending
             select new VehicleDispatchListDto
             {
                 Id = vd.Id,

                 SaleOrderNo = so.SaleOrderNo,

                 VehicleNumber = vd.VehicleNumber,

                 Status = vd.Status
             })
             .ToListAsync();
    }
}
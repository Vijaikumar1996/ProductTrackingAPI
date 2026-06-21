using Microsoft.EntityFrameworkCore;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Services;

public class WorkflowService : IWorkflowService
{
    private readonly ApplicationDbContext _context;

    public WorkflowService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkflowListDto>> GetListAsync(
        WorkflowStage stage)
    {
        switch (stage)
        {
            case WorkflowStage.Hub:

                return await _context.SaleOrders
                    .Where(x =>
                        x.Status == "UPLOADED" ||
                        x.Status == "RECEIVING")
                    .OrderByDescending(x => x.Id)
                    .Select(x => new WorkflowListDto
                    {
                        Id = x.Id,

                        SaleOrderNo = x.SaleOrderNo,

                        Status = x.Status,

                        ExpectedCount =
                            x.ExpectedHuCount,

                        ActualCount =
                            x.ReceivedHuCount
                    })
                    .ToListAsync();

            case WorkflowStage.Loading:

                return await _context.SaleOrders
                    .Where(x =>
                       x.Status == "RECEIVED" ||
                        x.Status == "LOADING")
                    .OrderByDescending(x => x.Id)
                    .Select(x => new WorkflowListDto
                    {
                        Id = x.Id,

                        SaleOrderNo = x.SaleOrderNo,

                        Status = x.Status,

                        ExpectedCount =
                            x.ReceivedHuCount,

                        ActualCount =
                            x.LoadedHuCount
                    })
                    .ToListAsync();

            case WorkflowStage.Delivery:

                return await
                    (from vd in _context.VehicleDispatches

                     join so in _context.SaleOrders
                         on vd.SaleOrderId equals so.Id

                     join dd in _context.SaleOrderDeliveryDetails
                         on so.Id equals dd.SaleOrderId

                     where vd.Status == "LOADED" || vd.Status == "DELIVERING"

                     orderby vd.Id descending

                     select new WorkflowListDto
                     {
                         Id = so.Id,

                         DispatchId = vd.Id,                        

                         SaleOrderNo = so.SaleOrderNo,

                         VehicleNumber = vd.VehicleNumber,

                         Status = vd.Status,

                         CustomerName = dd.CustomerName,

                         DeliveryAddress = dd.DeliveryAddress,

                         ContactPerson = dd.ContactPerson,

                         ContactNumber = dd.ContactNumber,

                         ExpectedCount = so.LoadedHuCount,

                         ActualCount = so.DeliveredHuCount
                     })
                    .ToListAsync();

            default:
                throw new Exception(
                    "Invalid workflow stage");
        }
    }
}
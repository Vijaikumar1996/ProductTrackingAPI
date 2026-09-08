using Microsoft.EntityFrameworkCore;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<MisReportDto>>
    GetMisReportAsync(MisReportRequestDto request)
        {
            var query = _context.SaleOrders
                .AsNoTracking()
                .Select(x => new MisReportDto
                {
                    SaleOrderId = x.Id,
                    SaleOrderNo = x.SaleOrderNo,
                    ShipmentDate = x.ShipmentDate,

                    ExpectedHuCount = x.ExpectedHuCount,
                    ReceivedHuCount = x.ReceivedHuCount,
                    LoadedHuCount = x.LoadedHuCount,
                    DeliveredHuCount = x.DeliveredHuCount,

                    HubMissingCount =
                        _context.MismatchLogs
                            .Where(m =>
                                m.SaleOrderId == x.Id &&
                                m.Stage == "HUB_RECEIVE")
                            .Select(m => (int?)m.MissingCount)
                            .FirstOrDefault() ?? 0,

                    LoadingMissingCount =
                        _context.MismatchLogs
                            .Where(m =>
                                m.SaleOrderId == x.Id &&
                                m.Stage == "VEHICLE_LOADING")
                            .Select(m => (int?)m.MissingCount)
                            .FirstOrDefault() ?? 0,

                    DeliveryMissingCount =
                        _context.MismatchLogs
                            .Where(m =>
                                m.SaleOrderId == x.Id &&
                                m.Stage == "DELIVERY")
                            .Select(m => (int?)m.MissingCount)
                            .FirstOrDefault() ?? 0,

                    TotalMissingCount =
                        _context.MismatchLogs
                            .Where(m =>
                                m.SaleOrderId == x.Id)
                            .Sum(m => (int?)m.MissingCount) ?? 0,

                    Status = x.Status
                });

            // Sale Order Filter
            if (!string.IsNullOrWhiteSpace(request.SaleOrderNo))
            {
                query = query.Where(x =>
                    x.SaleOrderNo.Contains(request.SaleOrderNo));
            }

            // Status Filter
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x =>
                    x.Status == request.Status);
            }

            // From Date Filter
            if (request.FromDate.HasValue)
            {              

                query = query.Where(x =>
                    x.ShipmentDate >= request.FromDate);
            }

            // To Date Filter
            if (request.ToDate.HasValue)
            {              
                query = query.Where(x =>
                    x.ShipmentDate < request.ToDate);
            }

            // Mismatch Only Filter
            if (request.MismatchOnly == true)
            {
                query = query.Where(x =>
                    x.TotalMissingCount > 0);
            }

            var totalRecords =
                await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.ShipmentDate)
                .Skip(
                    (request.PageNumber - 1)
                    * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<MisReportDto>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}

using Microsoft.EntityFrameworkCore;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;
using System.Security.Claims;

namespace ProductTrackingAPI.Services;

public class ScanHistoryService : IScanHistoryService
{
    private readonly ApplicationDbContext _context;

    public ScanHistoryService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ScanHistoryDto>>
        GetScanHistoryAsync(
            ScanHistoryRequestDto request,
            ClaimsPrincipal user)
    {
        var currentUserId =
            long.Parse(
                user.FindFirst(
                    ClaimTypes.NameIdentifier)!
                .Value);

        var roles = user.Claims
            .Where(x =>
                x.Type == ClaimTypes.Role)
            .Select(x => x.Value)
            .ToList();

        var isWebUser =
            roles.Contains("WEB_USER");

        var query =
            _context.ScanTransactions
                .Include(x => x.User)
                .Include(x => x.HuItem)
                    .ThenInclude(h => h.SaleOrder)
                .AsQueryable();

        /* ---------------- Scanner User Restriction ---------------- */

        if (!isWebUser)
        {
            var allowedStages =
                new List<string>();

            if (roles.Contains(
                "HUB_RECEIVE"))
            {
                allowedStages.Add(
                    "HUB_RECEIVING");
            }

            if (roles.Contains(
                "VEHICLE_LOAD"))
            {
                allowedStages.Add(
                    "VEHICLE_LOADING");
            }

            if (roles.Contains(
                "DELIVERY_SCAN"))
            {
                allowedStages.Add(
                    "DELIVERY");
            }

            query = query.Where(x =>
                x.ScannedBy ==
                currentUserId &&
                allowedStages.Contains(
                    x.Stage));
        }

        /* ---------------- WEB USER Filters ---------------- */

        if (isWebUser)
        {
            if (!string.IsNullOrWhiteSpace(
                request.ScanStage))
            {
                query = query.Where(x =>
                    x.Stage ==
                    request.ScanStage);
            }

            if (request.UserId.HasValue)
            {
                query = query.Where(x =>
                    x.ScannedBy ==
                    request.UserId.Value);
            }
        }

        /* ---------------- Common Filters ---------------- */

        if (!string.IsNullOrWhiteSpace(
            request.HuNumber))
        {
            query = query.Where(x =>
                x.HuItem
                    .HuNumberBarcode
                    .Contains(
                        request.HuNumber));
        }

        if (!string.IsNullOrWhiteSpace(
            request.SaleOrderNo))
        {
            query = query.Where(x =>
                x.HuItem
                    .SaleOrder
                    .SaleOrderNo
                    .Contains(
                        request.SaleOrderNo));
        }

        if (request.FromDate.HasValue)
        {
            var fromDate =
            DateTime.SpecifyKind(
          request.FromDate.Value.Date,
          DateTimeKind.Utc);

            query = query.Where(x =>
                x.ScannedAt >= fromDate);

        }

        if (request.ToDate.HasValue)
        {
            var toDate =
                DateTime.SpecifyKind(
                    request.ToDate.Value
                        .Date
                        .AddDays(1),
                    DateTimeKind.Utc);

            query = query.Where(x =>
                x.ScannedAt < toDate);
        }

        return await query
            .OrderByDescending(x =>
                x.ScannedAt)
            .Select(x =>
                new ScanHistoryDto
                {
                    Id = x.Id,

                    SaleOrderNo =
                        x.HuItem
                            .SaleOrder
                            .SaleOrderNo,

                    HuNumber =
                        x.HuItem
                            .HuNumberBarcode,

                    ScanStage =
                        x.Stage,

                    ScannedBy =
                        x.User
                            .Name,

                    ScanTime =
                        x.ScannedAt
                })
            .ToListAsync();
    }
}
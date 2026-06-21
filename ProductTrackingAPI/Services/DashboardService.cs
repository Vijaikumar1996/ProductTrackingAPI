using Microsoft.EntityFrameworkCore;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;
using System.Security.Claims;

namespace ProductTrackingAPI.Services;

public class DashboardService
    : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<
        ScannerDashboardResponseDto>
        GetScannerDashboardAsync(
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

        var today =
            DateTime.UtcNow.Date;

        var cards =
            new List<DashboardCardDto>();

        /* ---------------- HUB RECEIVE ---------------- */

        if (roles.Contains(
            "HUB_RECEIVE"))
        {
            var receivedOrders =
     await _context.SaleOrders
         .CountAsync(x =>
             x.ReceivedAt >= today);

            var pendingReceive =
                await _context.SaleOrders
                    .CountAsync(x =>
                        x.ReceivedHuCount <
                        x.ExpectedHuCount);

            cards.Add(
                new DashboardCardDto
                {
                    Title =
                        "Received Orders",

                    Count =
                        receivedOrders
                });

            cards.Add(
                new DashboardCardDto
                {
                    Title =
                        "Pending Receive",

                    Count =
                        pendingReceive
                });
        }

        /* ---------------- VEHICLE LOADING ---------------- */

        if (roles.Contains(
            "VEHICLE_LOAD"))
        {
            var loadedOrders =
    await _context.SaleOrders
        .CountAsync(x =>
            x.LoadedAt >= today);

            var pendingLoading =
                await _context.SaleOrders
                    .CountAsync(x =>
                        x.LoadedHuCount <
                        x.ReceivedHuCount);

            cards.Add(
                new DashboardCardDto
                {
                    Title =
                        "Loaded Orders",

                    Count =
                        loadedOrders
                });

            cards.Add(
                new DashboardCardDto
                {
                    Title =
                        "Pending Loading",

                    Count =
                        pendingLoading
                });
        }

        /* ---------------- DELIVERY ---------------- */

        if (roles.Contains(
            "DELIVERY_SCAN"))
        {
            var deliveredOrders =
    await _context.SaleOrders
        .CountAsync(x =>
            x.DeliveredAt >= today);

            var pendingDelivery =
                await _context.SaleOrders
                    .CountAsync(x =>
                        x.DeliveredHuCount <
                        x.LoadedHuCount);

            cards.Add(
                new DashboardCardDto
                {
                    Title =
                        "Delivered Orders",

                    Count =
                        deliveredOrders
                });

            cards.Add(
                new DashboardCardDto
                {
                    Title =
                        "Pending Delivery",

                    Count =
                        pendingDelivery
                });
        }

        /* ---------------- SCANNED HU COUNT ---------------- */

        var scannedHuCount =
            await _context.ScanTransactions
                .CountAsync(x =>
                    x.ScannedBy ==
                    currentUserId &&
                    x.ScannedAt >=
                    today);

        /* ---------------- LAST 5 SCANS ---------------- */

        var recentScans =
            await _context.ScanTransactions
                .Include(x =>
                    x.HuItem)
                    .ThenInclude(h =>
                        h.SaleOrder)
                .Where(x =>
                    x.ScannedBy ==
                    currentUserId)
                .OrderByDescending(x =>
                    x.ScannedAt)
                .Take(5)
                .Select(x =>
                    new RecentScanDto
                    {
                        SaleOrderNo =
                            x.HuItem
                                .SaleOrder
                                .SaleOrderNo,

                        HuNumber =
                            x.HuItem
                                .HuNumberBarcode,

                        ScanStage =
                            x.Stage,

                        ScanTime =
                            x.ScannedAt
                    })
                .ToListAsync();

        return new
            ScannerDashboardResponseDto
        {
            Cards =
                cards,

            ScannedHuCount =
                scannedHuCount,

            RecentScans =
                recentScans
        };
    }

    public async Task<AdminDashboardResponseDto>
     GetAdminDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;

        var todayUploads =
            await _context.SaleOrders
                .CountAsync(x =>
                    x.CreatedAt >= today);

        var pendingHubReceive =
            await _context.SaleOrders
                .CountAsync(x =>
                    x.Status == "UPLOADED");

        var productsInHub =
            await _context.SaleOrders
                .CountAsync(x =>
                    x.Status == "RECEIVED");

        var outForDelivery =
            await _context.SaleOrders
                .CountAsync(x =>
                    x.Status == "LOADED");

        var deliveredToday =
            await _context.SaleOrders
                .CountAsync(x =>
                    x.Status == "DELIVERED" &&
                    x.UpdatedAt >= today);

        var stageCounts =
            await _context.MismatchLogs
                .GroupBy(x => x.Stage)
                .Select(g => new
                {
                    Stage = g.Key,
                    MissingCount =
                        g.Sum(x => x.MissingCount)
                })
                .ToListAsync();

        var recentOrders =
            await _context.SaleOrders
                .OrderByDescending(x =>
                    x.CreatedAt)
                .Take(5)
                .Select(x =>
                    new RecentOrderDto
                    {
                        SaleOrderNo =
                            x.SaleOrderNo,

                        ExpectedCount =
                            x.ExpectedHuCount,

                        ScannedCount =
                            x.Status == "UPLOADED"
                                ? 0
                            : x.Status == "RECEIVED"
                                ? x.ReceivedHuCount
                            : x.Status == "LOADED"
                                ? x.LoadedHuCount
                            : x.DeliveredHuCount,

                        Status = x.Status
                    })
                .ToListAsync();

        var missingPackages =
            new MissingPackageSummaryDto
            {
                HubReceiveMissing =
                    stageCounts
                        .FirstOrDefault(x =>
                            x.Stage ==
                            "HUB_RECEIVING")
                        ?.MissingCount ?? 0,

                VehicleLoadingMissing =
                    stageCounts
                        .FirstOrDefault(x =>
                            x.Stage ==
                            "VEHICLE_LOADING")
                        ?.MissingCount ?? 0,

                DeliveryMissing =
                    stageCounts
                        .FirstOrDefault(x =>
                            x.Stage ==
                            "DELIVERY")
                        ?.MissingCount ?? 0
            };

        missingPackages.TotalMissingPackages =
            missingPackages.HubReceiveMissing +
            missingPackages.VehicleLoadingMissing +
            missingPackages.DeliveryMissing;

        return new AdminDashboardResponseDto
        {
            TodayUploads =
                todayUploads,

            PendingHubReceive =
                pendingHubReceive,

            ProductsInHub =
                productsInHub,

            OutForDelivery =
                outForDelivery,

            DeliveredToday =
                deliveredToday,

            MissingPackages =
                missingPackages,

            RecentOrders =
                recentOrders
        };
    }
}
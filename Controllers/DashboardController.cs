
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubWatch.Api.data;
using SubWatch.Api.DTOs;

namespace SubWatch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _appDbContext;
    public DashboardController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    //helper
    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = GetUserId();

        var subscriptions = _appDbContext.Subscriptions.Where(s => s.UserId == userId);

        var totalSubscriptions = await subscriptions.CountAsync();
        var activeSubscriptions = await subscriptions.CountAsync(s => s.IsActive);

        var monthlySpend =  await subscriptions
                            .Where(s => s.IsActive)
                            .SumAsync(s => s.BillingCycle == "Monthly" ? s.Amount : s.Amount / 12m);

        var yearlySpend =  await subscriptions
                           .Where(s => s.IsActive)
                           .SumAsync(s => s.BillingCycle == "Monthly" ? s.Amount * 12 : s.Amount);

        return Ok(new DashboardSummaryDto
        {
            TotalSubscriptions = totalSubscriptions,
            ActiveSubscriptions = activeSubscriptions,
            MonthlySpend = monthlySpend,
            YearlySpend = yearlySpend
        });
    }

    [HttpGet("upcoming-payments")]
    public async Task<IActionResult> GetUpcomingPayments()
    {
        var userId = GetUserId();
        var today = DateTime.Today;

        var payments =  await _appDbContext.Subscriptions
                        .Where(s => s.UserId == userId && s.IsActive && s.NextBillingDate >= today)
                        .OrderBy(s => s.NextBillingDate)
                        .Take(5)
                        .Select(s => new UpcomingPaymentDto
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Amount = s.Amount,
                            Currency = s.Currency,
                            BillingDate = s.NextBillingDate
                        }).ToListAsync();


        return Ok(payments);
    }

    [HttpGet("spending-overview")]
public async Task<IActionResult> GetSpendingOverview()
{
    var userId = GetUserId();
    var today = DateTime.Today;

    var subscriptions = await _appDbContext.Subscriptions
        .Where(s => s.UserId == userId && s.IsActive)
        .Select(s => new
        {
            s.Amount,
            s.BillingCycle,
            s.NextBillingDate
        })
        .ToListAsync();

    var result = new List<SpendingOverviewDto>();

    var firstMonth = new DateTime(today.Year, today.Month, 1);

    for (int i = 0; i < 6; i++)
    {
        var monthStart = firstMonth.AddMonths(i);
        var nextMonth = monthStart.AddMonths(1);

        decimal monthlyAmount = 0;

        foreach (var subscription in subscriptions)
        {
            var billingDate = subscription.NextBillingDate;

            while (billingDate < monthStart)
            {
                billingDate = subscription.BillingCycle == "Monthly"
                    ? billingDate.AddMonths(1)
                    : billingDate.AddYears(1);
            }

            if (billingDate >= monthStart && billingDate < nextMonth)
            {
                monthlyAmount += subscription.Amount;
            }
        }

        result.Add(new SpendingOverviewDto
        {
            Month = monthStart,
            Amount = monthlyAmount
        });
    }

    return Ok(result);
}
}
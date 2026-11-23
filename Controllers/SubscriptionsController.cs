

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubWatch.Api.data;
using SubWatch.Api.DTOs;
using SubWatch.Api.models;

namespace SubWatch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : Controller
{
    private readonly AppDbContext _appDbContext;

    public SubscriptionsController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    [HttpGet()]
    public async Task<IActionResult> GetSubscriptions([FromQuery]string? searchText = null, [FromQuery]int page = 1, [FromQuery]int pageSize = 4)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        
        var query = _appDbContext.Subscriptions.AsQueryable().Where(s => s.UserId == userId);
                 

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(s => s.Name.Contains(searchText));
        }

        var totalCount = await query.CountAsync();


        var subs =  await query
                    .OrderBy(s => s.Id)
                    .Skip((page - 1)* pageSize)
                    .Take(pageSize)
                    .Select(s => new SubscriptionResponseDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Amount = s.Amount,
                        Currency = s.Currency,
                        BillingCycle = s.BillingCycle,
                        StartDate = s.StartDate,
                        NextBillingDate = s.NextBillingDate,
                        Category = s.Category,
                        IsActive = s.IsActive
                    })    
                    .ToListAsync();

        return Ok(new
        {
            Items = subs,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount /(double)pageSize)
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionDto createSubscriptionDto)
    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var sub = new Subscription
        {
            Name = createSubscriptionDto.Name,
            Amount = createSubscriptionDto.Amount,
            Currency = createSubscriptionDto.Currency,
            BillingCycle = createSubscriptionDto.BillingCycle,
            StartDate = createSubscriptionDto.StartDate,
            NextBillingDate = createSubscriptionDto.NextBillingDate,
            UserId = userId
        };

        _appDbContext.Subscriptions.Add(sub);

        await _appDbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubscriptions), new {id = sub.Id}, sub);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSubscription([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var sub = await _appDbContext.Subscriptions.Where(s => s.Id == id && s.UserId == userId).Select(s => new SubscriptionResponseDto
        {
            Id = s.Id,
            Name = s.Name,
            Amount = s.Amount,
            Currency = s.Currency,
            BillingCycle = s.BillingCycle,
            StartDate = s.StartDate,
            NextBillingDate = s.NextBillingDate,
            IsActive = s.IsActive,
            Category = s.Category
        })
        .FirstOrDefaultAsync();

        if(sub == null) return NotFound();

        return Ok(sub);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubscription([FromRoute] int id,[FromBody] UpdateSubscriptionDto updateSubscription)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var sub = await _appDbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);


        if(sub == null)
            return NotFound();

        sub.Name = updateSubscription.Name;
        sub.Amount = updateSubscription.Amount;
        sub.Currency = updateSubscription.Currency;
        sub.BillingCycle = updateSubscription.BillingCycle;
        sub.BillingCycle = updateSubscription.BillingCycle;
        sub.StartDate = updateSubscription.StartDate;
        sub.NextBillingDate = updateSubscription.NextBillingDate;
        sub.IsActive = updateSubscription.IsActive;

        await _appDbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var sub = await _appDbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if(sub == null) return NotFound();

        _appDbContext.Subscriptions.Remove(sub);

        await _appDbContext.SaveChangesAsync();

        return NoContent();
    }
}
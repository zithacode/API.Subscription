

namespace SubWatch.Api.DTOs;

public class SubscriptionResponseDto
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public decimal Amount {get; set;}
    public string Currency {get;set;} = "ZAR";
    public string BillingCycle {get; set;} = "Monthly";
    public DateTime StartDate {get; set;}

    public DateTime NextBillingDate {get; set;}
    public string? Category {get;set;}

    public bool IsActive {get; set;} = true;
}
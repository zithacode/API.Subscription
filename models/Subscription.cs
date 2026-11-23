

using Microsoft.AspNetCore.Identity;

namespace SubWatch.Api.models;

public class Subscription
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public decimal Amount {get; set;}
    public string Currency {get;set;} = "ZAR";
    public string BillingCycle {get; set;} = "Monthly";
    public DateTime StartDate {get; set;}

    public DateTime NextBillingDate {get; set;}

    public string? Category {get; set;} = string.Empty;

    public bool IsActive {get; set;} = true;

    //relationships
    public string UserId {get; set;} = string.Empty;

    //Nav property
    public ApplicationUser User {get; set;} = null!;
}
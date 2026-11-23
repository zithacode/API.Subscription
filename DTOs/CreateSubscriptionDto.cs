

using System.ComponentModel.DataAnnotations;

namespace SubWatch.Api.DTOs;

public class CreateSubscriptionDto
{   
    [Required]
    [StringLength(100, MinimumLength =2)]
    public string Name {get; set;} = string.Empty;
    [Range(0.01, 1000000)]
    public decimal Amount {get; set;}
    [Required]
    public string Currency {get;set;} = "ZAR";
    [Required]
    [RegularExpression("^(Monthly|Yearly)$", ErrorMessage ="Billing cycle muste be Monthly or Yearly.")]

    public string BillingCycle {get; set;} = "Monthly";
    public DateTime StartDate {get; set;}

    public DateTime NextBillingDate {get; set;}

}
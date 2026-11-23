
namespace SubWatch.Api.DTOs;


public class UpcomingPaymentDto
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public decimal Amount {get; set;}
    public string Currency {get; set;} = "ZAR";
    public DateTime BillingDate {get; set;}
}
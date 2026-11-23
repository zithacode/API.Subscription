
namespace SubWatch.Api.DTOs;

public class DashboardSummaryDto
{
    public int TotalSubscriptions {get; set;}
    public int ActiveSubscriptions {get; set;}
    public decimal MonthlySpend {get; set;}
    public decimal YearlySpend {get; set;}
}
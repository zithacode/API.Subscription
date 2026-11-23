

using Microsoft.AspNetCore.Identity;

namespace SubWatch.Api.models;


public class ApplicationUser : IdentityUser
{
    public string? FullName {get; set;} = string.Empty;
}
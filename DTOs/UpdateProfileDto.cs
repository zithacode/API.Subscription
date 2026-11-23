
using System.ComponentModel.DataAnnotations;

namespace SubWatch.Api.DTOs;


public class UpdateProfileDto{
    [Required]
    [MinLength(4)]
    [MaxLength(30)]
    public string FullName {get;set;} = string.Empty;
    [EmailAddress]
    public string? Email {get; set;} = string.Empty;
}
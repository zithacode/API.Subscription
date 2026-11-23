

using System.ComponentModel.DataAnnotations;

namespace SubWatch.Api.DTOs;

public class DeleteAccountDto
{
    [Required]
    public string Password {get; set;} = string.Empty;
}
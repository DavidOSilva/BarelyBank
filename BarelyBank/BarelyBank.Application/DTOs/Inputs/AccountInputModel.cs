using BarelyBank.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BarelyBank.Application.DTOs.Inputs
{
    public class AccountInputModel
    {
        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public AccountType Type { get; set; }

        [Required]
        public AccountStatus Status { get; set; }
    }
}
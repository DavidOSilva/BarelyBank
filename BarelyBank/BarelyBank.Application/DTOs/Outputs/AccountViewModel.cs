using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Enums;
using System;

namespace BarelyBank.Application.DTOs.Outputs
{
    public class AccountViewModel
    {
        public Guid Id { get; set; }
        public int Number { get; set; }
        public decimal Balance { get; set; }
        public decimal Fee { get; set; }
        public AccountStatus Status { get; set; }
        public AccountType Type { get; set; }
        public ClientViewModel Holder { get; set; } = null!;

        public static AccountViewModel ToViewModel(Account account)
        {
            return new AccountViewModel
            {
                Id = account.Id,
                Number = account.Number,
                Balance = account.Balance,
                Fee = account.Fee,
                Status = account.Status,
                Type = account.Type,
                Holder = ClientViewModel.ToViewModel(account.Holder)
            };
        }
    }
}
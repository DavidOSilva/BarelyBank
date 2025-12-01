using BarelyBank.Domain.Enums;
using BarelyBank.Domain.Utils;
using System;

namespace BarelyBank.Domain.Entities
{
    public class CheckingAccount : Account
    {

        public CheckingAccount(int number, Guid clientId, AccountStatus status, decimal fee=0.005m) : base(number, clientId, status, fee)
        {
            Type = AccountType.Checking;
        }

    }
}
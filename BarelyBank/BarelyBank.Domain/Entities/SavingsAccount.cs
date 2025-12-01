using BarelyBank.Domain.Enums;

namespace BarelyBank.Domain.Entities
{
    public class SavingsAccount : Account
    {

        public SavingsAccount(int number, Guid clientId, AccountStatus status, decimal fee = 0.00m) : base(number, clientId, status, fee)
        {
            Type = AccountType.Savings;
        }

    }
}
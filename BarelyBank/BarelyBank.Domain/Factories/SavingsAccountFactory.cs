using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Enums;

namespace BarelyBank.Domain.Factories
{
    public class SavingsAccountFactory : IAccountFactory
    {
        public Account Create(int accountNumber, Guid clientId, AccountStatus status)
        {
            return new SavingsAccount(accountNumber, clientId, status);
        }
    }
}
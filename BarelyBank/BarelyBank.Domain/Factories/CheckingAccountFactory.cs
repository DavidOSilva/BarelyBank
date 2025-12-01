using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Enums;

namespace BarelyBank.Domain.Factories
{
    public class CheckingAccountFactory : IAccountFactory
    {
        public Account Create(int accountNumber, Guid clientId, AccountStatus status)
        {
                return new CheckingAccount(accountNumber, clientId, status);
        }

    }

}

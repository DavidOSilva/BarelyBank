using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Enums;

namespace BarelyBank.Domain.Factories
{
    public interface IAccountFactory
    {
        Account Create(int accountNumber, Guid clientId, AccountStatus status);
    }
}

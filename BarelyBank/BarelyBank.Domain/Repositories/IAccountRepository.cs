using BarelyBank.Domain.Entities;

namespace BarelyBank.Domain.Repositories
{
    public interface IAccountRepository : IEntityRepository<Account>
    {
        Task<int> GetLastAccountNumberAsync();
        Task<Account?> GetAccountByIdWithHolderAsync(Guid id);
        Task<IEnumerable<Account>> GetAccountsByClientIdAsync(Guid clientId);
    }
}

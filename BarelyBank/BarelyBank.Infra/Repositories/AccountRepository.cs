using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Repositories;
using BarelyBank.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace BarelyBank.Infra.Repositories
{
    public class AccountRepository : EntityRepository<Account>, IAccountRepository
    {
        public AccountRepository(BBContext context) : base(context)
        {
        }

        public async Task<Account?> GetAccountByIdWithHolderAsync(Guid id)
        {
            return await Context.Accounts
                                .Include(a => a.Holder)
                                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<int> GetLastAccountNumberAsync()
        {
            return await Context.Accounts
                                .MaxAsync(a => (int?)a.Number) ?? 10000;
        }

        public async Task<IEnumerable<Account>> GetAccountsByClientIdAsync(Guid clientId)
        {
            return await Context.Accounts
                                .Where(a => a.ClientId == clientId)
                                .Include(a => a.Holder)
                                .ToListAsync();
        }

    }

}

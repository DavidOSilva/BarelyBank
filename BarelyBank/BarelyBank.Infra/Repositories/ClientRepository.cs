using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Repositories;
using BarelyBank.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace BarelyBank.Infra.Repositories
{
    public class ClientRepository : EntityRepository<Client>, IClientRepository
    {
        public ClientRepository(BBContext context) : base(context)
        {
        }

        public Task<Client?> GetClientByDocumentNumberAsync(string documentNumber)
        {
            return Context.Clients
                          .AsNoTracking()
                          .FirstOrDefaultAsync(c => c.DocumentNumber == documentNumber);
        }

        public Task<Client?> GetClientByEmailAsync(string email)
        {
            return Context.Clients
                          .AsNoTracking()
                          .FirstOrDefaultAsync(c => c.Email == email);
        }
    }

}

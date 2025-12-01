using BarelyBank.Domain.Entities;

namespace BarelyBank.Domain.Repositories
{
    public interface IClientRepository : IEntityRepository<Client>
    {

        Task<Client?> GetClientByDocumentNumberAsync(string documentNumber);
        Task<Client?> GetClientByEmailAsync(string email);

    }
}

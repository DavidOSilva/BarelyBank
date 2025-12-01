using BarelyBank.Domain.Entities;

namespace BarelyBank.Domain.Repositories
{
    public interface ITransactionRepository // Não herda.s
    {
        Task<IEnumerable<Transaction>> GetTransactionsByIdAsync(Guid accountId, DateTime? startDate, DateTime? endDate, string sortBy, string sortOrder);
        void Add(Transaction transaction);
    }
}
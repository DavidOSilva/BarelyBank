using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Repositories;
using BarelyBank.Infra.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BarelyBank.Infra.Repositories
{
    public class TransactionRepository(BBContext context) : ITransactionRepository
    {
        public void Add(Transaction transaction)
        {
            context.Transactions.Add(transaction);
        }
        public async Task<IEnumerable<Transaction>> GetTransactionsByIdAsync(
            Guid accountId,
            DateTime? startDate, DateTime? endDate,
            string sortBy, string sortOrder)
        {
            var query = context.Transactions.AsQueryable();

            // Filtrando
            query = query.Where(t =>
                (t.SourceAccountId == accountId || t.TargetAccountId == accountId));
            if (startDate.HasValue) query = query.Where(t => t.Timestamp >= startDate.Value);
            if (endDate.HasValue) query = query.Where(t => t.Timestamp <= endDate.Value);

            // Ordenando
            var isDescending = string.Equals(sortOrder, "Desc", StringComparison.OrdinalIgnoreCase);
            Expression<Func<Transaction, object>> keySelector = sortBy.ToLowerInvariant() switch
            {
                "amount" => t => t.Amount,
                _ => t => t.Timestamp,
            };

            query = isDescending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

            return await query.ToListAsync();

        }
    }

}

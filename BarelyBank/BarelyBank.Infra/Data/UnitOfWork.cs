using BarelyBank.Domain.Repositories;
using BarelyBank.Infra.Data;
using BarelyBank.Infra.Repositories;

public class UnitOfWork(BBContext context) : IUnitOfWork
{
    public IAccountRepository AccountRepository { get; } = new AccountRepository(context);
    public IClientRepository ClientRepository { get; } = new ClientRepository(context);
    public ITransactionRepository TransactionRepository { get; } = new TransactionRepository(context);

    public async Task<int> CompleteAsync()
    {
        return await context.SaveChangesAsync();
    }
}

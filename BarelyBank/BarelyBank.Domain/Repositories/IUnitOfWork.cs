using BarelyBank.Domain.Repositories;

public interface IUnitOfWork
{
    IAccountRepository AccountRepository { get; }
    IClientRepository ClientRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    Task<int> CompleteAsync();
}

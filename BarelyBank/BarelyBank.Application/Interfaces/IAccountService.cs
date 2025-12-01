using BarelyBank.Application.DTOs.Inputs;
using BarelyBank.Application.DTOs.Outputs;

namespace BarelyBank.Application.Interfaces
{
    public interface IAccountService
    {
        Task<AccountViewModel> CreateAccountAsync(AccountInputModel input);
        Task<AccountViewModel> GetAccountByIdAsync(Guid id);
        Task<AccountViewModel> DepositAsync(Guid accountId, decimal amount);
        Task<AccountViewModel> WithdrawAsync(Guid accountId, decimal amount);
        Task<TransferViewModel> TransferAsync(TransferInputModel input);
        Task<IEnumerable<TransactionViewModel>> GetAccountStatementAsync(Guid accountId, StatementInputModel input);

    }
}
using BarelyBank.Application.DTOs.Inputs;
using BarelyBank.Application.DTOs.Outputs;
using BarelyBank.Application.Interfaces;
using BarelyBank.Domain.Enums;
using BarelyBank.Domain.Exceptions;
using BarelyBank.Domain.Factories;

namespace BarelyBank.Application.Services
{
    public class AccountService(
        IUnitOfWork unitOfWork,
        IDictionary<AccountType, IAccountFactory> accountFactories) : IAccountService
    {
        public async Task<AccountViewModel> CreateAccountAsync(AccountInputModel input)
        {
            var client = await unitOfWork.ClientRepository.GetAsync(input.ClientId);
            if (client == null) throw new NotFoundException("Cliente não encontrado.");

            if (!accountFactories.TryGetValue(input.Type, out var factory)) throw new ValidationException("Tipo de conta inválido."); // Obtem a fábrica se tipo for válido.
            var lastAccountNumber = await unitOfWork.AccountRepository.GetLastAccountNumberAsync();
            var newNumber = lastAccountNumber + 1;
            var account = factory.Create(newNumber, input.ClientId, input.Status);

            unitOfWork.AccountRepository.Add(account);
            await unitOfWork.CompleteAsync();
            return AccountViewModel.ToViewModel(account);
        }

        public async Task<AccountViewModel> GetAccountByIdAsync(Guid id)
        {
            var account = await unitOfWork.AccountRepository.GetAccountByIdWithHolderAsync(id);
            if (account == null) throw new NotFoundException("Conta não encontrada.");
            return AccountViewModel.ToViewModel(account);
        }

        public async Task<AccountViewModel> DepositAsync(Guid accountId, decimal amount)
        {
            var account = await unitOfWork.AccountRepository.GetAccountByIdWithHolderAsync(accountId);
            if (account == null) throw new NotFoundException("Conta não encontrada.");

            var deposit = account.Deposit(amount);
            unitOfWork.TransactionRepository.Add(deposit);

            unitOfWork.AccountRepository.Update(account);
            await unitOfWork.CompleteAsync();
            return AccountViewModel.ToViewModel(account);
        }

        public async Task<AccountViewModel> WithdrawAsync(Guid accountId, decimal amount)
        {
            var account = await unitOfWork.AccountRepository.GetAccountByIdWithHolderAsync(accountId);
            if (account == null) throw new NotFoundException("Conta não encontrada.");

            var transactions = account.Withdraw(amount);
            foreach (var transaction in transactions) unitOfWork.TransactionRepository.Add(transaction);

            unitOfWork.AccountRepository.Update(account);
            await unitOfWork.CompleteAsync();
            return AccountViewModel.ToViewModel(account);
        }

        public async Task<TransferViewModel> TransferAsync(TransferInputModel input)
        {
            if (input.SourceAccountId == input.TargetAccountId) throw new ValidationException("A conta de origem e destino não podem ser a mesma.");

            var sourceAccount = await unitOfWork.AccountRepository.GetAccountByIdWithHolderAsync(input.SourceAccountId);
            if (sourceAccount == null) throw new NotFoundException("Conta de origem não encontrada.");

            var targetAccount = await unitOfWork.AccountRepository.GetAccountByIdWithHolderAsync(input.TargetAccountId);
            if (targetAccount == null) throw new NotFoundException("Conta de destino não encontrada.");

            if (sourceAccount.Status != AccountStatus.Active || targetAccount.Status != AccountStatus.Active) throw new ValidationException("Ambas as contas devem estar ativas para realizar a transferência.");

            var withdrawalTransactions = sourceAccount.Withdraw(input.Amount);
            foreach (var transaction in withdrawalTransactions) unitOfWork.TransactionRepository.Add(transaction);
            var depositTransaction = targetAccount.Deposit(input.Amount);
            unitOfWork.TransactionRepository.Add(depositTransaction);

            unitOfWork.AccountRepository.Update(sourceAccount);
            unitOfWork.AccountRepository.Update(targetAccount);
            await unitOfWork.CompleteAsync();
            return TransferViewModel.ToViewModel(sourceAccount, targetAccount, input.Amount);
        }

        public async Task<IEnumerable<TransactionViewModel>> GetAccountStatementAsync(Guid accountId, StatementInputModel input)
        {
            var account = await unitOfWork.AccountRepository.GetAccountByIdWithHolderAsync(accountId);
            if (account == null) throw new NotFoundException("Conta não encontrada.");
            if (input.StartDate > input.EndDate) throw new ValidationException("A data de início não pode ser maior que a data de fim.");

            var validSortBy = new[] { "timestamp", "amount" };
            var validSortOrder = new[] { "asc", "desc" };

            if (!validSortBy.Contains(input.SortBy.ToLowerInvariant())) throw new ValidationException("Parâmetro de ordenação 'sortBy' inválido. Use 'Timestamp' ou 'Amount'.");

            if (!validSortOrder.Contains(input.SortOrder.ToLowerInvariant())) throw new ValidationException("Parâmetro de ordenação 'sortOrder' inválido. Use 'Asc' ou 'Desc'.");

            var transactions = await unitOfWork.TransactionRepository.GetTransactionsByIdAsync(
                accountId,
                input.StartDate,
                input.EndDate,
                input.SortBy,
                input.SortOrder);
            return transactions.Select(TransactionViewModel.ToViewModel);
        }

    }

}

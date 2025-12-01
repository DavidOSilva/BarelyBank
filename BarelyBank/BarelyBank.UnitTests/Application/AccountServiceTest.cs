using BarelyBank.Application.DTOs.Inputs;
using BarelyBank.Application.Services;
using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Enums;
using BarelyBank.Domain.Exceptions;
using BarelyBank.Domain.Factories;
using BarelyBank.Domain.Repositories;
using Moq;
using System.Reflection;

namespace BarelyBank.UnitTests.Application
{
    public class AccountServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly Mock<IClientRepository> _mockClientRepository;
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly Dictionary<AccountType, IAccountFactory> _factories;
        private readonly Mock<IAccountFactory> _mockCheckingFactory;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockClientRepository = new Mock<IClientRepository>();
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockCheckingFactory = new Mock<IAccountFactory>();

            _mockUnitOfWork.Setup(u => u.AccountRepository).Returns(_mockAccountRepository.Object);
            _mockUnitOfWork.Setup(u => u.ClientRepository).Returns(_mockClientRepository.Object);
            _mockUnitOfWork.Setup(u => u.TransactionRepository).Returns(_mockTransactionRepository.Object);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _factories = new Dictionary<AccountType, IAccountFactory>
            {
                { AccountType.Checking, _mockCheckingFactory.Object }
            };

            _accountService = new AccountService(_mockUnitOfWork.Object, _factories);
        }

        private static void SetEntityId<T>(T entity, Guid id)
        {
            var prop = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(entity, id);
                return;
            }

            var backing = typeof(T)
                .GetField($"<{prop!.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

            backing?.SetValue(entity, id);
        }

        private static Client CreateDefaultClient()
        {
            return new Client(
                "David Testevaldo",
                "123.456.789-01",
                DateTime.Now.AddYears(-25),
                "david@test.com",
                "senhahashfake"
            );
        }

        [Fact]
        public async Task CreateAccountAsync_WhenClientExists_ThenCreatesAccountWithIncrementedNumber()
        {
            var client = CreateDefaultClient();
            var clientId = Guid.NewGuid();
            SetEntityId(client, clientId);

            var input = new AccountInputModel
            {
                ClientId = clientId,
                Type = AccountType.Checking,
                Status = AccountStatus.Active
            };

            var lastNumber = 1500;
            var createdAccount = new CheckingAccount(lastNumber + 1, clientId, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(createdAccount, client);

            _mockClientRepository
                .Setup(r => r.GetAsync(clientId))
                .ReturnsAsync(client);

            _mockAccountRepository
                .Setup(r => r.GetLastAccountNumberAsync())
                .ReturnsAsync(lastNumber);

            _mockCheckingFactory
                .Setup(f => f.Create(lastNumber + 1, clientId, AccountStatus.Active))
                .Returns(createdAccount);

            var result = await _accountService.CreateAccountAsync(input);

            Assert.NotNull(result);
            Assert.Equal(lastNumber + 1, result.Number);
            _mockAccountRepository.Verify(r => r.Add(It.IsAny<Account>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_WhenClientDoesNotExist_ThenThrowsNotFoundException()
        {
            var input = new AccountInputModel
            {
                ClientId = Guid.NewGuid(),
                Type = AccountType.Checking,
                Status = AccountStatus.Active
            };

            _mockClientRepository
                .Setup(r => r.GetAsync(input.ClientId))
                .ReturnsAsync((Client?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _accountService.CreateAccountAsync(input)
            );
        }

        [Fact]
        public async Task CreateAccountAsync_WhenAccountTypeFactoryNotFound_ThenThrowsValidationException()
        {
            var client = CreateDefaultClient();
            var clientId = Guid.NewGuid();
            SetEntityId(client, clientId);

            var input = new AccountInputModel
            {
                ClientId = clientId,
                Type = AccountType.Savings, // Não existe no dicionário de fábricas.
                Status = AccountStatus.Active
            };

            _mockClientRepository
                .Setup(r => r.GetAsync(clientId))
                .ReturnsAsync(client);

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                _accountService.CreateAccountAsync(input)
            );

            Assert.Equal("Tipo de conta inválido.", ex.Message);
        }

        [Fact]
        public async Task GetAccountByIdAsync_WhenAccountExists_ThenReturnsViewModel()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());

            var account = new CheckingAccount(1001, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(account, client);
            SetEntityId(account, Guid.NewGuid());

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(account.Id))
                .ReturnsAsync(account);

            var result = await _accountService.GetAccountByIdAsync(account.Id);

            Assert.NotNull(result);
            Assert.Equal(account.Id, result.Id);
        }

        [Fact]
        public async Task GetAccountByIdAsync_WhenAccountDoesNotExist_ThenThrowsNotFoundException()
        {
            var id = Guid.NewGuid();
            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(id))
                .ReturnsAsync((Account?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _accountService.GetAccountByIdAsync(id)
            );
        }

        [Fact]
        public async Task DepositAsync_WhenAccountExists_ThenAddsTransactionAndUpdatesBalance()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());
            var account = new CheckingAccount(2001, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(account, client);
            SetEntityId(account, Guid.NewGuid());

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(account.Id))
                .ReturnsAsync(account);

            var previousBalance = account.Balance;
            var amount = 500m;

            var result = await _accountService.DepositAsync(account.Id, amount);

            Assert.Equal(previousBalance + amount, result.Balance);
            _mockTransactionRepository.Verify(t => t.Add(It.IsAny<Transaction>()), Times.Once);
            _mockAccountRepository.Verify(r => r.Update(account), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DepositAsync_WhenAccountDoesNotExist_ThenThrowsNotFoundException()
        {
            var id = Guid.NewGuid();
            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(id))
                .ReturnsAsync((Account?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _accountService.DepositAsync(id, 100m)
            );
        }

        [Fact]
        public async Task DepositAsync_WhenAmountIsZeroOrNegative_ThenThrowsArgumentException()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());
            var account = new CheckingAccount(3001, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(account, client);
            SetEntityId(account, Guid.NewGuid());

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(account.Id))
                .ReturnsAsync(account);

            // Zero
            var exZero = await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountService.DepositAsync(account.Id, 0m)
            );
            Assert.Equal("O valor da operação não pode ser negativo ou zero.", exZero.Message);

            // Negativo
            var exNeg = await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountService.DepositAsync(account.Id, -10m)
            );
            Assert.Equal("O valor da operação não pode ser negativo ou zero.", exNeg.Message);

            _mockTransactionRepository.Verify(t => t.Add(It.IsAny<Transaction>()), Times.Never);
            _mockAccountRepository.Verify(r => r.Update(It.IsAny<Account>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task TransferAsync_WhenInsufficientFunds_ThenThrowsInsufficientFundsException()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());

            var source = new CheckingAccount(7201, client.Id, AccountStatus.Active);
            var target = new CheckingAccount(7202, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(source, client);
            typeof(Account).GetProperty("Holder")!.SetValue(target, client);
            SetEntityId(source, Guid.NewGuid());
            SetEntityId(target, Guid.NewGuid());

            source.Deposit(20m);

            var input = new TransferInputModel
            {
                SourceAccountId = source.Id,
                TargetAccountId = target.Id,
                Amount = 100m
            };

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(source.Id))
                .ReturnsAsync(source);
            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(target.Id))
                .ReturnsAsync(target);

            var ex = await Assert.ThrowsAsync<InsufficientFundsException>(() =>
                _accountService.TransferAsync(input)
            );
            Assert.Equal("Saldo insuficiente.", ex.Message);

            _mockAccountRepository.Verify(r => r.Update(It.IsAny<Account>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task WithdrawAsync_WhenInsufficientFunds_ThenThrowsInsufficientFundsException()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());
            var account = new CheckingAccount(5001, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(account, client);
            SetEntityId(account, Guid.NewGuid());

            // Saldo baixo
            account.Deposit(50m);

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(account.Id))
                .ReturnsAsync(account);

            var ex = await Assert.ThrowsAsync<InsufficientFundsException>(() =>
                _accountService.WithdrawAsync(account.Id, 200m)
            );
            Assert.Equal("Saldo insuficiente.", ex.Message);

            _mockTransactionRepository.Verify(t => t.Add(It.IsAny<Transaction>()), Times.Never);
            _mockAccountRepository.Verify(r => r.Update(It.IsAny<Account>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task DepositAsync_WhenAccountInactive_ThenThrowsAccountNotActiveException()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());
            var account = new CheckingAccount(9001, client.Id, AccountStatus.Inactive);
            typeof(Account).GetProperty("Holder")!.SetValue(account, client);
            SetEntityId(account, Guid.NewGuid());

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(account.Id))
                .ReturnsAsync(account);

            var ex = await Assert.ThrowsAsync<AccountNotActiveException>(() =>
                _accountService.DepositAsync(account.Id, 100m)
            );
            Assert.Equal("A conta precisa está ativa.", ex.Message);

            _mockTransactionRepository.Verify(t => t.Add(It.IsAny<Transaction>()), Times.Never);
            _mockAccountRepository.Verify(r => r.Update(It.IsAny<Account>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task WithdrawAsync_WhenAccountInactive_ThenThrowsAccountNotActiveException()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());
            var account = new CheckingAccount(9002, client.Id, AccountStatus.Inactive);
            typeof(Account).GetProperty("Holder")!.SetValue(account, client);
            SetEntityId(account, Guid.NewGuid());

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(account.Id))
                .ReturnsAsync(account);

            var ex = await Assert.ThrowsAsync<AccountNotActiveException>(() =>
                _accountService.WithdrawAsync(account.Id, 50m)
            );
            Assert.Equal("A conta precisa está ativa.", ex.Message);

            _mockTransactionRepository.Verify(t => t.Add(It.IsAny<Transaction>()), Times.Never);
            _mockAccountRepository.Verify(r => r.Update(It.IsAny<Account>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task TransferAsync_WhenValid_ThenWithdrawsFromSourceAndDepositsInTarget()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());

            var source = new CheckingAccount(6001, client.Id, AccountStatus.Active);
            var target = new CheckingAccount(6002, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(source, client);
            typeof(Account).GetProperty("Holder")!.SetValue(target, client);
            SetEntityId(source, Guid.NewGuid());
            SetEntityId(target, Guid.NewGuid());

            source.Deposit(1000m);

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(source.Id))
                .ReturnsAsync(source);

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(target.Id))
                .ReturnsAsync(target);

            var input = new TransferInputModel
            {
                SourceAccountId = source.Id,
                TargetAccountId = target.Id,
                Amount = 300m
            };

            var result = await _accountService.TransferAsync(input);

            Assert.NotNull(result);
            Assert.Equal(source.Id, result.SourceAccountId);
            Assert.Equal(target.Id, result.TargetAccountId);
            Assert.Equal(300m, result.Amount);

            var expectedSourceBalance = 1000m - input.Amount - (source.Fee * input.Amount);
            Assert.Equal(expectedSourceBalance, source.Balance);
            Assert.Equal(300m, target.Balance);

            _mockAccountRepository.Verify(r => r.Update(source), Times.Once);
            _mockAccountRepository.Verify(r => r.Update(target), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task TransferAsync_WhenSourceEqualsTarget_ThenThrowsValidationException()
        {
            var id = Guid.NewGuid();
            var input = new TransferInputModel
            {
                SourceAccountId = id,
                TargetAccountId = id,
                Amount = 100m
            };

            await Assert.ThrowsAsync<ValidationException>(() =>
                _accountService.TransferAsync(input)
            );
        }

        [Fact]
        public async Task TransferAsync_WhenSourceNotFound_ThenThrowsNotFoundException()
        {
            var input = new TransferInputModel
            {
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                Amount = 50m
            };

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(input.SourceAccountId))
                .ReturnsAsync((Account?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _accountService.TransferAsync(input)
            );
        }

        [Fact]
        public async Task TransferAsync_WhenTargetNotFound_ThenThrowsNotFoundException()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());

            var source = new CheckingAccount(7001, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(source, client);
            SetEntityId(source, Guid.NewGuid());

            var input = new TransferInputModel
            {
                SourceAccountId = source.Id,
                TargetAccountId = Guid.NewGuid(),
                Amount = 50m
            };

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(source.Id))
                .ReturnsAsync(source);

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(input.TargetAccountId))
                .ReturnsAsync((Account?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _accountService.TransferAsync(input)
            );
        }

        [Fact]
        public async Task TransferAsync_WhenAnyAccountInactive_ThenThrowsValidationException()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());

            var source = new CheckingAccount(7101, client.Id, AccountStatus.Inactive);
            var target = new CheckingAccount(7102, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(source, client);
            typeof(Account).GetProperty("Holder")!.SetValue(target, client);
            SetEntityId(source, Guid.NewGuid());
            SetEntityId(target, Guid.NewGuid());

            var input = new TransferInputModel
            {
                SourceAccountId = source.Id,
                TargetAccountId = target.Id,
                Amount = 10m
            };

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(source.Id))
                .ReturnsAsync(source);
            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(target.Id))
                .ReturnsAsync(target);

            await Assert.ThrowsAsync<ValidationException>(() =>
                _accountService.TransferAsync(input)
            );
        }

        [Fact]
        public async Task GetAccountStatementAsync_WhenAccountExists_ThenReturnsTransactions()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());
            var account = new CheckingAccount(8001, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(account, client);
            SetEntityId(account, Guid.NewGuid());

            // Cria uma transação real via depósito
            var tx = account.Deposit(250m);

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(account.Id))
                .ReturnsAsync(account);

            _mockTransactionRepository
                .Setup(r => r.GetTransactionsByIdAsync(
                    account.Id,
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new List<Transaction> { tx });

            var input = new StatementInputModel
            {
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow,
                SortBy = "Timestamp",
                SortOrder = "Desc"
            };

            var result = await _accountService.GetAccountStatementAsync(account.Id, input);

            Assert.Single(result);
            _mockTransactionRepository.Verify(t => t.GetTransactionsByIdAsync(
                account.Id,
                input.StartDate,
                input.EndDate,
                input.SortBy,
                input.SortOrder), Times.Once);
        }

        [Fact]
        public async Task GetAccountStatementAsync_WhenAccountNotFound_ThenThrowsNotFoundException()
        {
            var input = new StatementInputModel
            {
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow,
                SortBy = "Timestamp",
                SortOrder = "Asc"
            };

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Account?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _accountService.GetAccountStatementAsync(Guid.NewGuid(), input)
            );
        }

        [Fact]
        public async Task GetAccountStatementAsync_WhenStartDateGreaterThanEndDate_ThenThrowsValidationException()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());
            var account = new CheckingAccount(8101, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(account, client);
            SetEntityId(account, Guid.NewGuid());

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(account.Id))
                .ReturnsAsync(account);

            var input = new StatementInputModel
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-1),
                SortBy = "Timestamp",
                SortOrder = "Asc"
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                _accountService.GetAccountStatementAsync(account.Id, input)
            );

            Assert.Equal("A data de início não pode ser maior que a data de fim.", ex.Message);
        }

        [Fact]
        public async Task GetAccountStatementAsync_WhenInvalidSortBy_ThenThrowsValidationException()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());
            var account = new CheckingAccount(8201, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(account, client);
            SetEntityId(account, Guid.NewGuid());

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(account.Id))
                .ReturnsAsync(account);

            var input = new StatementInputModel
            {
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow,
                SortBy = "ValorInvalido",
                SortOrder = "Asc"
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                _accountService.GetAccountStatementAsync(account.Id, input)
            );

            Assert.Equal("Parâmetro de ordenação 'sortBy' inválido. Use 'Timestamp' ou 'Amount'.", ex.Message);
        }

        [Fact]
        public async Task GetAccountStatementAsync_WhenInvalidSortOrder_ThenThrowsValidationException()
        {
            var client = CreateDefaultClient();
            SetEntityId(client, Guid.NewGuid());
            var account = new CheckingAccount(8301, client.Id, AccountStatus.Active);
            typeof(Account).GetProperty("Holder")!.SetValue(account, client);
            SetEntityId(account, Guid.NewGuid());

            _mockAccountRepository
                .Setup(r => r.GetAccountByIdWithHolderAsync(account.Id))
                .ReturnsAsync(account);

            var input = new StatementInputModel
            {
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow,
                SortBy = "Timestamp",
                SortOrder = "OrdemInvalida"
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                _accountService.GetAccountStatementAsync(account.Id, input)
            );

            Assert.Equal("Parâmetro de ordenação 'sortOrder' inválido. Use 'Asc' ou 'Desc'.", ex.Message);
        }
    }
}
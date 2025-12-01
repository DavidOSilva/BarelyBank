using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Enums;
using BarelyBank.Domain.Exceptions;
using Xunit;

namespace BarelyBank.UnitTests.Domain
{
    public class AccountTests
    {
        private static Client CreateClient()
        {
            return new Client(
                "David Testevaldo",
                "123.456.789-01",
                DateTime.Now.AddYears(-25),
                "david@test.com",
                "senhahashfake"
            );
        }

        private static CheckingAccount CreateActiveChecking(Guid clientId, decimal initialDeposit = 0m)
        {
            var acc = new CheckingAccount(1000, clientId, AccountStatus.Active);
            if (initialDeposit > 0) acc.Deposit(initialDeposit);
            return acc;
        }

        [Fact]
        public void Deposit_WhenAccountInactive_ThenThrowsAccountNotActiveException()
        {
            var client = CreateClient();
            var account = new CheckingAccount(1001, client.Id, AccountStatus.Inactive);

            var ex = Assert.Throws<AccountNotActiveException>(() =>
                account.Deposit(100m)
            );

            Assert.Equal("A conta precisa está ativa.", ex.Message);
        }

        [Fact]
        public void Withdraw_WhenAccountInactive_ThenThrowsAccountNotActiveException()
        {
            var client = CreateClient();
            var account = new CheckingAccount(1002, client.Id, AccountStatus.Inactive);

            var ex = Assert.Throws<AccountNotActiveException>(() =>
                account.Withdraw(50m).ToList()
            );

            Assert.Equal("A conta precisa está ativa.", ex.Message);
        }

        [Fact]
        public void Deposit_WhenAmountIsZeroOrNegative_ThenThrowsArgumentException()
        {
            var client = CreateClient();
            var account = CreateActiveChecking(client.Id);

            var exZero = Assert.Throws<ArgumentException>(() => account.Deposit(0m));
            Assert.Equal("O valor da operação não pode ser negativo ou zero.", exZero.Message);

            var exNeg = Assert.Throws<ArgumentException>(() => account.Deposit(-5m));
            Assert.Equal("O valor da operação não pode ser negativo ou zero.", exNeg.Message);
        }

        [Fact]
        public void Withdraw_WhenAmountIsZeroOrNegative_ThenThrowsArgumentException()
        {
            var client = CreateClient();
            var account = CreateActiveChecking(client.Id, initialDeposit: 100m);

            var exZero = Assert.Throws<ArgumentException>(() => account.Withdraw(0m).ToList());
            Assert.Equal("O valor da operação não pode ser negativo ou zero.", exZero.Message);

            var exNeg = Assert.Throws<ArgumentException>(() => account.Withdraw(-1m).ToList());
            Assert.Equal("O valor da operação não pode ser negativo ou zero.", exNeg.Message);
        }

        [Fact]
        public void Withdraw_WhenInsufficientFunds_ThenThrowsInsufficientFundsException()
        {
            var client = CreateClient();
            var account = CreateActiveChecking(client.Id, initialDeposit: 50m);

            var ex = Assert.Throws<InsufficientFundsException>(() =>
                account.Withdraw(100m).ToList()
            );

            Assert.Equal("Saldo insuficiente.", ex.Message);
        }

        [Fact]
        public void Withdraw_WhenSufficientFunds_ThenCreatesWithdrawalAndFeeTransactionsAndUpdatesBalance()
        {
            var client = CreateClient();
            var account = CreateActiveChecking(client.Id);

            // Deposita para ter saldo suficiente
            var depositTx = account.Deposit(1000m);
            Assert.Equal(1000m, account.Balance);

            var amount = 300m;
            var transactions = account.Withdraw(amount).ToList();

            // Deve conter pelo menos a transação de saque e possivelmente a taxa
            Assert.True(transactions.Count >= 1);
            Assert.Contains(transactions, t => t.Type == TransactionType.Withdraw);

            if (account.Fee > 0)
            {
                Assert.Contains(transactions, t => t.Type == TransactionType.Fee);
                var expectedBalance = 1000m - amount - (account.Fee * amount);
                Assert.Equal(expectedBalance, account.Balance);
            }
            else
            {
                var expectedBalance = 1000m - amount;
                Assert.Equal(expectedBalance, account.Balance);
            }
        }

        [Fact]
        public void Deposit_WhenValid_ThenCreatesDepositTransactionAndUpdatesBalance()
        {
            var client = CreateClient();
            var account = CreateActiveChecking(client.Id);

            var tx = account.Deposit(250m);

            Assert.Equal(TransactionType.Deposit, tx.Type);
            Assert.Equal(250m, tx.Amount);
            Assert.Equal(250m, account.Balance);
        }
    }
}
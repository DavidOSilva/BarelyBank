using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Enums;
using BarelyBank.Domain.Factories;
using Xunit;

namespace BarelyBank.UnitTests.Domain
{
    public class AccountFactoryTests
    {
        private static Guid CreateClientId() => Guid.NewGuid();

        [Fact]
        public void CheckingAccountFactory_Create_WhenValidParams_ThenCreatesCheckingAccountWithDefaults()
        {
            // Arrange
            var factory = new CheckingAccountFactory();
            var clientId = CreateClientId();
            var number = 1234;

            // Act
            var account = factory.Create(number, clientId, AccountStatus.Active);

            // Assert
            Assert.NotNull(account);
            Assert.IsType<CheckingAccount>(account);
            Assert.Equal(AccountType.Checking, account.Type);
            Assert.Equal(number, account.Number);
            Assert.Equal(clientId, account.ClientId);
            Assert.Equal(AccountStatus.Active, account.Status);

            // Checking default fee (as per constructor default: 0.005m)
            Assert.Equal(0.005m, account.Fee);
            Assert.Equal(0m, account.Balance);
        }

        [Fact]
        public void CheckingAccountFactory_Create_WhenInactiveStatus_ThenCreatesInactiveAccount()
        {
            // Arrange
            var factory = new CheckingAccountFactory();
            var clientId = CreateClientId();

            // Act
            var account = factory.Create(5678, clientId, AccountStatus.Inactive);

            // Assert
            Assert.NotNull(account);
            Assert.Equal(AccountStatus.Inactive, account.Status);
            Assert.Equal(AccountType.Checking, account.Type);
        }

        [Fact]
        public void SavingsAccountFactory_Create_WhenValidParams_ThenCreatesSavingsAccount()
        {
            // Arrange
            var factory = new SavingsAccountFactory();
            var clientId = CreateClientId();
            var number = 4321;

            // Act
            var account = factory.Create(number, clientId, AccountStatus.Active);

            // Assert
            Assert.NotNull(account);
            Assert.Equal(AccountType.Savings, account.Type);
            Assert.Equal(number, account.Number);
            Assert.Equal(clientId, account.ClientId);
            Assert.Equal(AccountStatus.Active, account.Status);

            // Valide a taxa conforme implementação da Savings (ex.: 0.01m)
            Assert.True(account.Fee >= 0m);
            Assert.Equal(0m, account.Balance);
        }
    }
}
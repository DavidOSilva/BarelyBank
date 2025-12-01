using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Enums;
using BarelyBank.Domain.Exceptions;
using Xunit;

namespace BarelyBank.UnitTests.Domain
{
    public class TransactionTests
    {
        [Fact]
        public void Constructor_DepositWithoutTarget_ThenThrowsInvalidTransactionException()
        {
            var ex = Assert.Throws<InvalidTransactionException>(() =>
                new Transaction(TransactionType.Deposit, 100m, targetAccountId: null)
            );

            Assert.Equal("Depósitos exigem uma conta de destino.", ex.Message);
        }

        [Fact]
        public void Constructor_DepositWithTarget_ThenCreatesTransaction()
        {
            var targetId = Guid.NewGuid();

            var tx = new Transaction(TransactionType.Deposit, 250m, targetAccountId: targetId);

            Assert.NotEqual(Guid.Empty, tx.Id);
            Assert.Equal(TransactionType.Deposit, tx.Type);
            Assert.Equal(250m, tx.Amount);
            Assert.NotEqual(default, tx.Timestamp);
            Assert.Null(tx.SourceAccountId);
            Assert.Equal(targetId, tx.TargetAccountId);
        }

        [Fact]
        public void Constructor_WithdrawWithoutSource_ThenThrowsInvalidTransactionException()
        {
            var ex = Assert.Throws<InvalidTransactionException>(() =>
                new Transaction(TransactionType.Withdraw, 50m, sourceAccountId: null)
            );

            Assert.Equal("Saques exigem uma conta de origem.", ex.Message);
        }

        [Fact]
        public void Constructor_WithdrawWithSource_ThenCreatesTransaction()
        {
            var sourceId = Guid.NewGuid();

            var tx = new Transaction(TransactionType.Withdraw, 75m, sourceAccountId: sourceId);

            Assert.NotEqual(Guid.Empty, tx.Id);
            Assert.Equal(TransactionType.Withdraw, tx.Type);
            Assert.Equal(75m, tx.Amount);
            Assert.Equal(sourceId, tx.SourceAccountId);
            Assert.Null(tx.TargetAccountId);
        }

        [Fact]
        public void Constructor_FeeWithoutSource_ThenThrowsInvalidTransactionException()
        {
            var ex = Assert.Throws<InvalidTransactionException>(() =>
                new Transaction(TransactionType.Fee, 2.5m, sourceAccountId: null)
            );

            Assert.Equal("Taxas exigem uma conta de origem.", ex.Message);
        }

        [Fact]
        public void Constructor_FeeWithSource_ThenCreatesTransaction()
        {
            var sourceId = Guid.NewGuid();

            var tx = new Transaction(TransactionType.Fee, 1.5m, sourceAccountId: sourceId);

            Assert.NotEqual(Guid.Empty, tx.Id);
            Assert.Equal(TransactionType.Fee, tx.Type);
            Assert.Equal(1.5m, tx.Amount);
            Assert.Equal(sourceId, tx.SourceAccountId);
            Assert.Null(tx.TargetAccountId);
        }

        [Fact]
        public void Constructor_UnknownType_ThenThrowsInvalidTransactionException()
        {
            // Simula um tipo inválido fora do enum conhecido
            var invalidType = (TransactionType)999;

            var ex = Assert.Throws<InvalidTransactionException>(() =>
                new Transaction(invalidType, 10m, sourceAccountId: Guid.NewGuid())
            );

            Assert.Equal("Tipo de transação desconhecido.", ex.Message);
        }

        [Fact]
        public void Constructor_TimestampIsUtcNowish_ThenSetsRecentTimestamp()
        {
            var targetId = Guid.NewGuid();
            var before = DateTime.UtcNow.AddSeconds(-2);

            var tx = new Transaction(TransactionType.Deposit, 10m, targetAccountId: targetId);

            var after = DateTime.UtcNow.AddSeconds(2);
            Assert.True(tx.Timestamp >= before && tx.Timestamp <= after);
        }
    }
}
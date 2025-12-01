using BarelyBank.Application.DTOs.Outputs;
using BarelyBank.Application.Interfaces;
using BarelyBank.Application.Services;
using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Enums;
using BarelyBank.Domain.Exceptions;
using BarelyBank.Domain.Repositories;
using Moq;

namespace BarelyBank.UnitTests.Application
{
    public class ClientServiceTests
    {
        private readonly Mock<IClientRepository> _mockClientRepository;
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly ClientService _clientService;

        public ClientServiceTests()
        {
            _mockClientRepository = new Mock<IClientRepository>();
            _mockAccountRepository = new Mock<IAccountRepository>();
            _clientService = new ClientService(_mockAccountRepository.Object, _mockClientRepository.Object);
        }

        [Fact]
        public async Task GetClientByIdAsync_WhenClientExists_ThenReturnsClientViewModel()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var client = new Client(
                "David Testevaldo",
                "123.456.789-01",
                DateTime.Now.AddYears(-30),
                "teste@example.com",
                "senhahashfake"
            );

            _mockClientRepository
                .Setup(repo => repo.GetAsync(clientId))
                .ReturnsAsync(client);

            // Act
            var result = await _clientService.GetClientByIdAsync(clientId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ClientViewModel>(result);
            Assert.Equal(client.Name, result.Name);
        }

        [Fact]
        public async Task GetClientByIdAsync_WhenClientDoesNotExist_ThenThrowsNotFoundException()
        {
            // Arrange
            var clientId = Guid.NewGuid();

            _mockClientRepository
                .Setup(repo => repo.GetAsync(clientId))
                .ReturnsAsync((Client?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _clientService.GetClientByIdAsync(clientId)
            );
        }

        [Fact]
        public async Task SearchAccountsAsync_WhenNoParametersProvided_ThenThrowsValidationException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _clientService.SearchAccountsAsync(null, null)
            );

            Assert.Equal(
                "É necessário informar o ID do cliente ou o CPF para realizar a busca.",
                exception.Message
            );
        }

        [Fact]
        public async Task SearchAccountsAsync_WhenValidClientId_ThenReturnsAccounts()
        {
            // Arrange
            var clientId = Guid.NewGuid();

            var client = new Client(
                "David Testevaldo",
                "123.456.789-01",
                DateTime.Now.AddYears(-30),
                "teste@example.com",
                "senhahashfake"
            );

            typeof(Client)
                .GetProperty("Id")!
                .SetValue(client, clientId);

            var accounts = new List<Account>
            {
                new CheckingAccount(1001, clientId, AccountStatus.Active),
                new CheckingAccount(1002, clientId, AccountStatus.Active)
            };

            foreach (var account in accounts)
            {
                typeof(Account)
                    .GetProperty("Holder")!
                    .SetValue(account, client);
            }

            _mockClientRepository
                .Setup(repo => repo.GetAsync(clientId))
                .ReturnsAsync(client);

            _mockAccountRepository
                .Setup(repo => repo.GetAccountsByClientIdAsync(clientId))
                .ReturnsAsync(accounts);

            // Act
            var result = await _clientService.SearchAccountsAsync(clientId, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(accounts.Select(a => a.Id), result.Select(r => r.Id));
        }

        [Fact]
        public async Task SearchAccountsAsync_WhenValidDocument_ThenReturnsAccounts()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var document = "123.456.789-01";

            var client = new Client(
                "David Testevaldo",
                "123.456.789-01",
                DateTime.Now.AddYears(-30),
                "teste@example.com",
                "senhahashfake"
            );

            typeof(Client)
                .GetProperty("Id")!
                .SetValue(client, clientId);

            var accounts = new List<Account>
            {
                new CheckingAccount(2001, clientId, AccountStatus.Active)
            };

            foreach (var account in accounts)
            {
                typeof(Account)
                    .GetProperty("Holder")!
                    .SetValue(account, client);
            }

            _mockClientRepository
                .Setup(repo => repo.GetClientByDocumentNumberAsync(document))
                .ReturnsAsync(client);

            _mockAccountRepository
                .Setup(repo => repo.GetAccountsByClientIdAsync(clientId))
                .ReturnsAsync(accounts);

            // Act
            var result = await _clientService.SearchAccountsAsync(null, document);

            // Assert
            Assert.Single(result);
            Assert.Equal(accounts[0].Id, result[0].Id);
        }

        [Fact]
        public async Task SearchAccountsAsync_WhenInvalidClientId_ThenThrowsNotFoundException()
        {
            // Arrange
            var clientId = Guid.NewGuid();

            _mockClientRepository
                .Setup(repo => repo.GetAsync(clientId))
                .ReturnsAsync((Client?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _clientService.SearchAccountsAsync(clientId, null)
            );
        }

        [Fact]
        public async Task SearchAccountsAsync_WhenClientHasNoAccounts_ThenReturnsEmptyList()
        {
            // Arrange
            var clientId = Guid.NewGuid();

            var client = new Client(
                "David Testevaldo",
                "123.456.789-01",
                DateTime.Now.AddYears(-30),
                "teste@example.com",
                "senhahashfake"
            );

            typeof(Client)
                .GetProperty("Id")!
                .SetValue(client, clientId);

            _mockClientRepository
                .Setup(repo => repo.GetAsync(clientId))
                .ReturnsAsync(client);

            _mockAccountRepository
                .Setup(repo => repo.GetAccountsByClientIdAsync(clientId))
                .ReturnsAsync(new List<Account>());

            // Act
            var result = await _clientService.SearchAccountsAsync(clientId, null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}

using BarelyBank.Application.DTOs.Inputs;
using BarelyBank.Application.DTOs.Outputs;
using BarelyBank.Application.Services;
using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Exceptions;
using BarelyBank.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using Xunit;

namespace BarelyBank.UnitTests.Application
{
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<IClientRepository> mockClientRepository;
        private readonly Mock<IPasswordHasher<Client>> mockPasswordHasher;
        private readonly Mock<IConfiguration> mockConfiguration;
        private readonly AuthService authService;

        public AuthServiceTests()
        {
            mockUnitOfWork = new Mock<IUnitOfWork>();
            mockClientRepository = new Mock<IClientRepository>();
            mockPasswordHasher = new Mock<IPasswordHasher<Client>>();
            mockConfiguration = new Mock<IConfiguration>();

            mockUnitOfWork
                .Setup(uow => uow.ClientRepository)
                .Returns(mockClientRepository.Object);

            var jwtSettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "a-very-secret-key-that-is-long-enough"},
                {"Jwt:Issuer", "BarelyBank.Api"},
                {"Jwt:Audience", "BarelyBank.Users"}
            };

            mockConfiguration
                .Setup(config => config[It.IsAny<string>()])
                .Returns((string key) =>
                    jwtSettings.ContainsKey(key) ? jwtSettings[key] : null
                );

            authService = new AuthService(
                mockUnitOfWork.Object,
                mockPasswordHasher.Object,
                mockConfiguration.Object
            );
        }

        private static ClientInputModel DefaultClientInput => new ClientInputModel
        {
            Name = "David Silva",
            DocumentNumber = "123.456.789-01",
            BirthDate = DateTime.Now.AddYears(-25),
            Email = "david@test.com",
            Password = "Senha123@",
            ConfirmPassword = "Senha123@"
        };

        private void SetEntityId<T>(T entity, Guid id)
        {
            var propertyInfo = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(entity, id, null);
            }
            else
            {
                var fieldInfo = typeof(T)
                    .BaseType?
                    .GetField($"<{propertyInfo!.Name}>k__BackingField",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                fieldInfo?.SetValue(entity, id);
            }
        }

        [Fact]
        public async Task RegisterClientAsync_WhenValidInput_ThenCreatesClient()
        {
            var input = DefaultClientInput;

            mockClientRepository
                .Setup(repo => repo.GetClientByEmailAsync(input.Email))
                .ReturnsAsync((Client?)null);

            mockClientRepository
                .Setup(repo => repo.GetClientByDocumentNumberAsync(input.DocumentNumber))
                .ReturnsAsync((Client?)null);

            mockPasswordHasher
                .Setup(ph => ph.HashPassword(It.IsAny<Client>(), input.Password))
                .Returns("hashed_password");

            var result = await authService.RegisterClientAsync(input);

            Assert.NotNull(result);
            Assert.Equal(input.Name, result.Name);

            mockClientRepository.Verify(repo => repo.Add(It.IsAny<Client>()), Times.Once);
            mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterClientAsync_WhenEmailAlreadyExists_ThenDoesNotPersistAndThrowsConflictException()
        {
            var input = DefaultClientInput;

            var existingClient = new Client(
                "David",
                input.DocumentNumber,
                input.BirthDate,
                input.Email,
                "hash"
            );

            mockClientRepository
                .Setup(repo => repo.GetClientByEmailAsync(input.Email))
                .ReturnsAsync(existingClient);

            await Assert.ThrowsAsync<ConflictException>(() =>
                authService.RegisterClientAsync(input)
            );

            mockClientRepository.Verify(repo => repo.Add(It.IsAny<Client>()), Times.Never);
            mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task RegisterClientAsync_WhenPasswordInvalid_ThenDoesNotPersistAndThrowsValidationException()
        {
            var input = DefaultClientInput;
            input.ConfirmPassword = "SenhaDiferente@";

            mockClientRepository
                .Setup(repo => repo.GetClientByEmailAsync(input.Email))
                .ReturnsAsync((Client?)null);

            mockClientRepository
                .Setup(repo => repo.GetClientByDocumentNumberAsync(input.DocumentNumber))
                .ReturnsAsync((Client?)null);

            await Assert.ThrowsAsync<ValidationException>(() =>
                authService.RegisterClientAsync(input)
            );

            mockClientRepository.Verify(repo => repo.Add(It.IsAny<Client>()), Times.Never);
            mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WhenClientDoesNotExist_ThenThrowsAuthenticationException()
        {
            var loginInput = new LoginInputModel
            {
                Email = "inexistente@email.com",
                Password = "Senha123@"
            };

            mockClientRepository
                .Setup(repo => repo.GetClientByEmailAsync(loginInput.Email))
                .ReturnsAsync((Client?)null);

            await Assert.ThrowsAsync<AuthenticationException>(() =>
                authService.ValidateCredentialsAsync(loginInput)
            );
        }

        [Fact]
        public void GenerateToken_WhenCalledTwiceForSameClient_ThenReturnsDifferentTokens()
        {
            var client = new Client(
                "David",
                "123456789",
                DateTime.Now.AddYears(-20),
                "token@test.com",
                "hash"
            );

            SetEntityId(client, Guid.NewGuid());

            var token1 = authService.GenerateToken(client);
            Thread.Sleep(1000);
            var token2 = authService.GenerateToken(client);

            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GenerateToken_WhenJwtKeyIsInvalid_ThenThrowsException()
        {
            var invalidConfig = new Mock<IConfiguration>();
            invalidConfig
                .Setup(c => c["Jwt:Key"])
                .Returns(string.Empty);

            invalidConfig
                .Setup(c => c["Jwt:Issuer"])
                .Returns("Issuer");

            invalidConfig
                .Setup(c => c["Jwt:Audience"])
                .Returns("Audience");

            Assert.ThrowsAny<Exception>(() =>
                new AuthService(
                    mockUnitOfWork.Object,
                    mockPasswordHasher.Object,
                    invalidConfig.Object)
            );
        }

        [Fact]
        public async Task RegisterClientAsync_WhenSuccessful_ThenDoesNotExposePasswordHashInViewModel()
        {
            var input = DefaultClientInput;

            mockClientRepository
                .Setup(repo => repo.GetClientByEmailAsync(input.Email))
                .ReturnsAsync((Client?)null);

            mockClientRepository
                .Setup(repo => repo.GetClientByDocumentNumberAsync(input.DocumentNumber))
                .ReturnsAsync((Client?)null);

            mockPasswordHasher
                .Setup(ph => ph.HashPassword(It.IsAny<Client>(), input.Password))
                .Returns("hash_super_secreto");

            var result = await authService.RegisterClientAsync(input);

            var resultProperties = result.GetType().GetProperties();
            Assert.DoesNotContain(resultProperties, p => p.Name.Contains("Password"));
        }

        [Fact]
        public async Task FullFlow_RegisterValidateAndGenerateToken_ThenAllStepsSucceed()
        {
            var input = DefaultClientInput;

            mockClientRepository
                .Setup(repo => repo.GetClientByEmailAsync(input.Email))
                .ReturnsAsync((Client?)null);

            mockClientRepository
                .Setup(repo => repo.GetClientByDocumentNumberAsync(input.DocumentNumber))
                .ReturnsAsync((Client?)null);

            mockPasswordHasher
                .Setup(ph => ph.HashPassword(It.IsAny<Client>(), input.Password))
                .Returns("hash");

            mockPasswordHasher
                .Setup(ph => ph.VerifyHashedPassword(
                    It.IsAny<Client>(),
                    It.IsAny<string>(),
                    input.Password))
                .Returns(PasswordVerificationResult.Success);

            var clientViewModel = await authService.RegisterClientAsync(input);

            var loginInput = new LoginInputModel
            {
                Email = input.Email,
                Password = input.Password
            };

            var client = new Client(
                input.Name,
                input.DocumentNumber,
                input.BirthDate,
                input.Email,
                "hash"
            );

            SetEntityId(client, Guid.NewGuid());

            mockClientRepository
                .Setup(repo => repo.GetClientByEmailAsync(input.Email))
                .ReturnsAsync(client);

            var validatedClient = await authService.ValidateCredentialsAsync(loginInput);
            var token = authService.GenerateToken(validatedClient);

            Assert.NotNull(clientViewModel);
            Assert.NotNull(validatedClient);
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void AuthResponse_WhenComposed_ThenContainsTokenAndClient()
        {
            var client = new Client(
                "Response User",
                "987.654.321-00",
                DateTime.Now.AddYears(-25),
                "response@test.com",
                "hash"
            );
            SetEntityId(client, Guid.NewGuid());

            var token = authService.GenerateToken(client);
            var response = new AuthResponse
            {
                Token = token,
                Client = ClientViewModel.ToViewModel(client)
            };

            Assert.False(string.IsNullOrWhiteSpace(response.Token));
            Assert.NotNull(response.Client);
            Assert.Equal(client.Id, response.Client.Id);
            Assert.Equal(client.Email, response.Client.Email);
        }
    }
}

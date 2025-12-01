using BarelyBank.Application.DTOs.Inputs;
using BarelyBank.Application.DTOs.Outputs;
using BarelyBank.Application.Interfaces;
using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Exceptions;
using BarelyBank.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using BarelyBank.Domain.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BarelyBank.Application.Services
{
    public class AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher<Client> passwordHasher,
        IConfiguration config) : IAuthService
    {
        private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

        public string GenerateToken(Client client)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.NameId, client.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, client.Email)
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = creds,
                Issuer = config["Jwt:Issuer"]!,
                Audience = config["Jwt:Audience"]!
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<ClientViewModel> RegisterClientAsync(ClientInputModel input)
        {

            if (await unitOfWork.ClientRepository.GetClientByEmailAsync(input.Email) != null)
                throw new ConflictException("Este e-mail já foi registrado.");

            if (await unitOfWork.ClientRepository.GetClientByDocumentNumberAsync(input.DocumentNumber) != null)
                throw new ConflictException("Esse documento já foi cadastrado.");

            PasswordValidator.Validate(input.Password, input.ConfirmPassword);
            var passwordHash = passwordHasher.HashPassword(null, input.Password);

            var client = new Client(
                input.Name,
                input.DocumentNumber,
                input.BirthDate,
                input.Email,
                passwordHash
            );

            unitOfWork.ClientRepository.Add(client);
            await unitOfWork.CompleteAsync();
            return ClientViewModel.ToViewModel(client);
        }

        public async Task<Client> ValidateCredentialsAsync(LoginInputModel input)
        {
            var client = await unitOfWork.ClientRepository.GetClientByEmailAsync(input.Email);
            if (client == null) throw new AuthenticationException("Credenciais inválidas.");

            var result = passwordHasher.VerifyHashedPassword(null, client.PasswordHash, input.Password);
            if (result == PasswordVerificationResult.Failed) throw new AuthenticationException("Credenciais inválidas.");

            return client;
        }


    }
}
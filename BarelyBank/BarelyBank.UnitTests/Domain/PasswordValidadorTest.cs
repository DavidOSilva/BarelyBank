using BarelyBank.Domain.Exceptions;
using BarelyBank.Domain.Utils;
using Xunit;

namespace BarelyBank.UnitTests.Domain
{
    public class PasswordValidatorTests
    {
        [Fact]
        public void Validate_WhenPasswordsDoNotMatch_ThenThrowsValidationException()
        {
            var ex = Assert.Throws<ValidationException>(() =>
                PasswordValidator.Validate("Senha123@", "SenhaDiferente@")
            );

            Assert.Equal("As senhas não são iguais.", ex.Message);
        }

        [Fact]
        public void Validate_WhenMissingUpperOrLower_ThenThrowsValidationException()
        {
            // Falta maiúscula
            var exNoUpper = Assert.Throws<ValidationException>(() =>
                PasswordValidator.Validate("senha123@", "senha123@")
            );
            Assert.Equal("A senha deve ter letras maiúsculas e minúscula.", exNoUpper.Message);

            // Falta minúscula
            var exNoLower = Assert.Throws<ValidationException>(() =>
                PasswordValidator.Validate("SENHA123@", "SENHA123@")
            );
            Assert.Equal("A senha deve ter letras maiúsculas e minúscula.", exNoLower.Message);
        }

        [Fact]
        public void Validate_WhenMissingDigit_ThenThrowsValidationException()
        {
            var ex = Assert.Throws<ValidationException>(() =>
                PasswordValidator.Validate("SenhaSemNumero@", "SenhaSemNumero@")
            );

            Assert.Equal("A senha deve ter números.", ex.Message);
        }

        [Fact]
        public void Validate_WhenMissingSpecialChar_ThenThrowsValidationException()
        {
            var ex = Assert.Throws<ValidationException>(() =>
                PasswordValidator.Validate("Senha1234", "Senha1234")
            );

            Assert.Equal("A senha deve ter ao menos um caractere especial", ex.Message);
        }

        [Fact]
        public void Validate_WhenAllRulesSatisfied_ThenDoesNotThrow()
        {
            var valid = "Senha123@";
            var exception = Record.Exception(() =>
                PasswordValidator.Validate(valid, valid)
            );

            Assert.Null(exception);
        }
    }
}
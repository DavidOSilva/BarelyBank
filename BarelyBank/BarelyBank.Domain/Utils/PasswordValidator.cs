using BarelyBank.Domain.Exceptions;

namespace BarelyBank.Domain.Utils
{
    public class PasswordValidator
    {

        public static void Validate(string password, string confirmPass)
        {
            if (password != confirmPass)
                throw new ValidationException("As senhas não são iguais.");

            if (!password.Any(char.IsUpper) || !password.Any(char.IsLower))
                throw new ValidationException("A senha deve ter letras maiúsculas e minúscula.");

            if (!password.Any(char.IsDigit))
                throw new ValidationException("A senha deve ter números.");

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                throw new ValidationException("A senha deve ter ao menos um caractere especial");
        }


    }

}

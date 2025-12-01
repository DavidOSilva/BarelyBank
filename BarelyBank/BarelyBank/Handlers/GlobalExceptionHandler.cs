using BarelyBank.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BarelyBank.Api.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            switch (exception)
            {
                case NotFoundException:
                    problemDetails.Title = "Recurso não encontrado";
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    problemDetails.Detail = exception.Message;
                    break;
                case ValidationException:
                case InsufficientFundsException:
                case AccountNotActiveException:
                case InvalidTransactionException:
                case ArgumentException:
                    problemDetails.Title = "Requisição inválida";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Detail = exception.Message;
                    break;
                case ConflictException:
                    problemDetails.Title = "Conflito de recurso";
                    problemDetails.Status = (int)HttpStatusCode.Conflict;
                    problemDetails.Detail = exception.Message;
                    break;
                case AuthenticationException:
                    problemDetails.Title = "Autenticação falhou";
                    problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                    problemDetails.Detail = exception.Message;
                    break;
                default:
                    problemDetails.Title = "Um erro inesperado ocorreu";
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Detail = "Não foi possível processar sua solicitação. Tente novamente mais tarde.";
                    break;
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
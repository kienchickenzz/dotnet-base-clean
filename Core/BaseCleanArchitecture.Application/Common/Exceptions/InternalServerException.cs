namespace BaseCleanArchitecture.Application.Common.Exceptions;

using System.Net;

using BaseCleanArchitecture.Domain.Common;


public class InternalServerException : DomainException
{
    public InternalServerException(string message, List<Error>? errors = default)
        : base(message, errors, HttpStatusCode.InternalServerError)
    {
    }
}

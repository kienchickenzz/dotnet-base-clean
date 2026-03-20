namespace BaseCleanArchitecture.Application.Common.Messaging;

using MediatR;

using BaseCleanArchitecture.Domain.Common;


public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

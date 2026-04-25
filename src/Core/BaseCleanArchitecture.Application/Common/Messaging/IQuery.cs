namespace BaseCleanArchitecture.Application.Common.Messaging;

using MediatR;

using BaseCleanArchitecture.Domain.Primitives;


public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

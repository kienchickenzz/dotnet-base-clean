namespace BaseCleanArchitecture.Application.Common.Messaging;

using BaseCleanArchitecture.Domain.Primitives;

using MediatR;


public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}

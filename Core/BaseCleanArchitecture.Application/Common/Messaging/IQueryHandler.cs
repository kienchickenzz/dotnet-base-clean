namespace BaseCleanArchitecture.Application.Common.Messaging;

using BaseCleanArchitecture.Domain.Common;

using MediatR;


public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}

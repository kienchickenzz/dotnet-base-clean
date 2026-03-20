namespace BaseCleanArchitecture.Application.Common.Messaging;

using MediatR;

using BaseCleanArchitecture.Domain.Common;


public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

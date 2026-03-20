namespace BaseCleanArchitecture.Application.Common.Messaging;

using MediatR;

using BaseCleanArchitecture.Domain.Common;


public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}

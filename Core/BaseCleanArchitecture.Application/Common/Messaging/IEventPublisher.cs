namespace BaseCleanArchitecture.Application.Common.Messaging;

using BaseCleanArchitecture.Domain.Common;


public interface IEventPublisher
{
    Task PublishAsync(IDomainEvent @event);
}

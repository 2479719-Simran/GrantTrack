using System;

namespace GrantTrack.Utility;

public class InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync<T>(T @event) where T : class
    {
        logger.LogInformation("[EVENT] {EventType} published: {@Event}",
            typeof(T).Name, @event);
        return Task.CompletedTask;
    }
}

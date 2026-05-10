using System;
namespace GrantTrack.Utility;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event) where T : class;
}


using CleanArchitecture.Shared.Helpers;

namespace CleanArchitecture.Domain.Common;

public abstract class BaseEntity
{
    private readonly List<BaseEvent> _domainEvents = [];

    public Guid Id { get; set; } = GuidHelper.NewGuidId();

    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

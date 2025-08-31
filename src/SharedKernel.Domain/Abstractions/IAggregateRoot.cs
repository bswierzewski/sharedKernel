using SharedKernel.Domain.Abstractions;

namespace SharedKernel.Domain.Abstractions;

/// <summary>
/// Interface for aggregate root entities that manage domain events.
/// Aggregate roots are the entry points to aggregates and ensure consistency boundaries.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Gets the read-only collection of domain events
    /// </summary>
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    /// <summary>
    /// Adds a domain event to the aggregate's event collection
    /// </summary>
    /// <param name="domainEvent">Domain event to add</param>
    void AddDomainEvent(DomainEvent domainEvent);

    /// <summary>
    /// Removes a specific domain event from the aggregate's event collection
    /// </summary>
    /// <param name="domainEvent">Domain event to remove</param>
    void RemoveDomainEvent(DomainEvent domainEvent);

    /// <summary>
    /// Clears all domain events from the aggregate's event collection
    /// </summary>
    void ClearDomainEvents();
}
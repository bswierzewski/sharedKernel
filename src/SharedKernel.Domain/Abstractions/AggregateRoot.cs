using System.ComponentModel.DataAnnotations.Schema;

namespace SharedKernel.Domain.Abstractions;

/// <summary>
/// Base aggregate root class providing domain events functionality and audit capabilities.
/// Aggregate roots are the entry points to aggregates and ensure consistency boundaries.
/// </summary>
/// <typeparam name="TId">Type of the entity identifier</typeparam>
/// <remarks>
/// Key characteristics of Aggregate Roots:
/// - Entry point to an aggregate - external access only through aggregate root
/// - Ensures consistency and business invariants within aggregate boundary
/// - Manages domain events for the aggregate
/// - Has global identity (unlike entities inside aggregate)
/// - Controls access to internal entities
/// </remarks>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    /// <summary>
    /// Private collection storing domain events for this aggregate
    /// </summary>
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the read-only collection of domain events. NotMapped to exclude from database persistence
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the aggregate's event collection
    /// </summary>
    /// <param name="domainEvent">Domain event to add</param>
    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a specific domain event from the aggregate's event collection
    /// </summary>
    /// <param name="domainEvent">Domain event to remove</param>
    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the aggregate's event collection
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

using SharedKernel.Domain.Interfaces;

namespace SharedKernel.Domain.Abstractions;

/// <summary>
/// Base entity class providing identity functionality and audit capabilities.
/// All entities in the domain inherit from this class and automatically include audit fields.
/// </summary>
/// <typeparam name="TId">Type of the entity identifier</typeparam>
/// <remarks>
/// Key characteristics of Entities:
/// - Has unique identity (Id) that persists through object lifecycle
/// - Identity is more important than attributes
/// - Can change state while maintaining identity
/// - Mutable (unlike Value Objects)
/// - Includes audit fields for tracking creation and modification
/// </remarks>
public abstract class Entity<TId> : IAuditable
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Date and time when the entity was created
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Identifier of the user who created the entity
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// Date and time when the entity was last modified
    /// </summary>
    public DateTimeOffset LastModified { get; set; }

    /// <summary>
    /// Identifier of the user who last modified the entity
    /// </summary>
    public int? LastModifiedBy { get; set; }
}

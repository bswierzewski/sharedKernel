namespace SharedKernel.Domain.Interfaces;

/// <summary>
/// Interface for entities that support audit tracking.
/// Provides automatic population of creation and modification timestamps and user information.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Date and time when the entity was created
    /// </summary>
    DateTimeOffset Created { get; set; }

    /// <summary>
    /// Identifier of the user who created the entity
    /// </summary>
    int? CreatedBy { get; set; }

    /// <summary>
    /// Date and time when the entity was last modified
    /// </summary>
    DateTimeOffset LastModified { get; set; }

    /// <summary>
    /// Identifier of the user who last modified the entity
    /// </summary>
    int? LastModifiedBy { get; set; }
}
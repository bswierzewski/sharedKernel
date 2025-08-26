namespace SharedKernel.Application.Abstractions;

public interface IUser
{
    /// <summary>
    /// Gets the unique identifier of the user from the external identity provider (Clerk, Auto0, etc.).
    /// Returns null if the user is not authenticated or the claim is not present.
    /// </summary>
    string ExternalId { get; }

    /// <summary>
    /// Gets the unique identifier of the corresponding domain user.
    /// This ID is typically populated after the UserProvisioningMiddleware has run.
    /// Returns null if the user is not authenticated or not yet provisioned in the domain.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks whether the current user has the specified permission.
    /// </summary>
    /// <param name="permission">The permission to check for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the user has the permission; otherwise, false.</returns>
    Task<bool> HasPermissionAsync(string permission);

    /// <summary>
    /// Checks whether the current user is in the specified role.
    /// </summary>
    /// <param name="role">The role to check for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the user is in the role; otherwise, false.</returns>
    Task<bool> IsInRoleAsync(string role);
}
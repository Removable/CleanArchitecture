namespace CleanArchitecture.Application.Common.Security;

/// <summary>
///     Specifies the class this attribute is applied to requires authorization.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizeAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorizeAttribute" /> class.
    /// </summary>
    public AuthorizeAttribute()
    {
    }

    /// <summary>
    /// Gets or sets the roles that define the authorization requirements for accessing a resource.
    /// </summary>
    public string[] Roles { get; set; } = [];

    /// <summary>
    ///     Gets or sets the policy name that determines access to the resource.
    /// </summary>
    public string Policy { get; set; } = string.Empty;
}

namespace Kongroo.Identity.Infrastructure;

/// <summary>
/// SNS topic names used with the Amazon SQS transport. <see cref="UserCreated"/> is shared by
/// contract with the Kongroo.Notifications SAM template — change both together.
/// </summary>
public static class MessagingTopics
{
    public const string UserCreated = "kongroo-user-created";
    public const string UserRoleChanged = "kongroo-user-role-changed";
}

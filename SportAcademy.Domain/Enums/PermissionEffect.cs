namespace SportAcademy.Domain.Enums
{
    // Deny always wins over Allow, and both always win over whatever the user's role(s) grant
    // by default - see PermissionResolver for the resolution order.
    public enum PermissionEffect
    {
        Allow = 0,
        Deny = 1
    }
}

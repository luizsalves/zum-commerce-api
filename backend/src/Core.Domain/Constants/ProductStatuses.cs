namespace Core.Domain.Constants;

public static class ProductStatuses
{
    public const string Active = "ACTIVE";
    public const string Draft = "DRAFT";
    public const string Archived = "ARCHIVED";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Active, Draft, Archived };
}

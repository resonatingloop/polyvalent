namespace Polyvalent.Domain;

public readonly record struct ResearchLineageSlug
{
    private ResearchLineageSlug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ResearchLineageSlug Parse(string value)
    {
        var canonical = value?.Trim() ?? string.Empty;
        if (canonical.Length is < 1 or > 80
            || canonical.Any(character =>
                !(character is >= 'a' and <= 'z'
                    || character is >= '0' and <= '9'
                    || character == '-'))
            || canonical[0] == '-'
            || canonical[^1] == '-'
            || canonical.Contains("--", StringComparison.Ordinal))
        {
            throw new FormatException(
                "research lineage slugs use lowercase ascii letters, digits, and single interior hyphens.");
        }

        return new ResearchLineageSlug(canonical);
    }

    public override string ToString() => Value;
}

public enum DirectRelationDirectionality
{
    Directed,
    Symmetric,
}

public static class DirectRelationDirectionalityExtensions
{
    public static string ToStorage(this DirectRelationDirectionality directionality) =>
        directionality switch
        {
            DirectRelationDirectionality.Directed => "directed",
            DirectRelationDirectionality.Symmetric => "symmetric",
            _ => throw new ArgumentOutOfRangeException(nameof(directionality)),
        };

    public static DirectRelationDirectionality ParseStorage(string value) =>
        value switch
        {
            "directed" => DirectRelationDirectionality.Directed,
            "symmetric" => DirectRelationDirectionality.Symmetric,
            _ => throw new FormatException($"unknown direct-relation directionality '{value}'."),
        };
}

public enum AuthoredRevisionStatus
{
    Draft,
    Accepted,
    Withdrawn,
}

public static class AuthoredRevisionStatusExtensions
{
    public static string ToStorage(this AuthoredRevisionStatus status) =>
        status switch
        {
            AuthoredRevisionStatus.Draft => "draft",
            AuthoredRevisionStatus.Accepted => "accepted",
            AuthoredRevisionStatus.Withdrawn => "withdrawn",
            _ => throw new ArgumentOutOfRangeException(nameof(status)),
        };

    public static AuthoredRevisionStatus ParseStorage(string value) =>
        value switch
        {
            "draft" => AuthoredRevisionStatus.Draft,
            "accepted" => AuthoredRevisionStatus.Accepted,
            "withdrawn" => AuthoredRevisionStatus.Withdrawn,
            _ => throw new FormatException($"unknown authored revision status '{value}'."),
        };
}

public sealed record DirectRelationParticipant(
    IntegerValue Integer,
    string Role,
    int Ordinal);

public sealed record ConstellationMember(
    IntegerValue Integer,
    string Role,
    string? MemberNotes,
    int Ordinal);

public static class AuthoredRelationalValidator
{
    public static void ValidateDirectRelationForAcceptance(
        DirectRelationDirectionality directionality,
        string proposition,
        string warrant,
        string epistemicRegister,
        IReadOnlyList<DirectRelationParticipant> participants)
    {
        _ = directionality.ToStorage();
        RequireText(proposition, "a direct relation requires a proposition before acceptance.");
        RequireText(warrant, "a direct relation requires a warrant before acceptance.");
        RequireText(epistemicRegister, "a direct relation requires an epistemic register before acceptance.");

        if (participants.Count != 2)
        {
            throw new InvalidOperationException("an accepted direct relation requires exactly two participants.");
        }

        ValidateOrdinalsAndRoles(participants.Select(item => (item.Integer, item.Role, item.Ordinal)));
    }

    public static void ValidateConstellationForAcceptance(
        string name,
        string account,
        string epistemicRegister,
        IReadOnlyList<ConstellationMember> members)
    {
        RequireText(name, "a constellation requires a name before acceptance.");
        RequireText(account, "a constellation requires a collective account before acceptance.");
        RequireText(epistemicRegister, "a constellation requires an epistemic register before acceptance.");

        if (members.Count < 2)
        {
            throw new InvalidOperationException("an accepted constellation requires at least two members.");
        }

        ValidateOrdinalsAndRoles(members.Select(item => (item.Integer, item.Role, item.Ordinal)));
    }

    public static void ValidateDraftParticipants(IReadOnlyList<DirectRelationParticipant> participants) =>
        ValidateOrdinalsAndRoles(participants.Select(item => (item.Integer, item.Role, item.Ordinal)));

    public static void ValidateDraftMembers(IReadOnlyList<ConstellationMember> members) =>
        ValidateOrdinalsAndRoles(members.Select(item => (item.Integer, item.Role, item.Ordinal)));

    private static void ValidateOrdinalsAndRoles(
        IEnumerable<(IntegerValue Integer, string Role, int Ordinal)> items)
    {
        var materialized = items.ToArray();
        if (materialized.Any(item => item.Ordinal < 0))
        {
            throw new InvalidOperationException("participant and member ordinals cannot be negative.");
        }

        if (materialized.Any(item => string.IsNullOrWhiteSpace(item.Role)))
        {
            throw new InvalidOperationException("every participant and member requires an explicit role.");
        }

        if (materialized.Select(item => item.Ordinal).Distinct().Count() != materialized.Length)
        {
            throw new InvalidOperationException("participant and member ordinals must be unique within a revision.");
        }

        if (materialized.Select(item => item.Integer).Distinct().Count() != materialized.Length)
        {
            throw new InvalidOperationException("an integer may appear only once in one authored revision.");
        }
    }

    private static void RequireText(string value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }
    }
}

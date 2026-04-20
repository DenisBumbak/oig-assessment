using System.Text.RegularExpressions;
using OIG.Domain.Common;

namespace OIG.Domain.Users;

public sealed record Email
{
    private static readonly Regex SimpleEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    public string Value { get; }
    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (value is null)
            throw new DomainException("Email is required.");

        value = value.Trim();

        if (value.Length == 0)
            throw new DomainException("Email is required.");

        if (!SimpleEmail.IsMatch(value))
            throw new DomainException("Email format is invalid.");

        return new Email(value.ToLowerInvariant());
    }
    public override string ToString() => Value;
}

using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Catalyst.Domain;

public sealed partial class MacAddress : IEquatable<MacAddress>, IComparable<MacAddress>, IComparable
{
    public MacAddress(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || !ParseExpression().IsMatch(raw))
            throw new ArgumentException("Invalid MAC format", nameof(raw));

        // Normalize to upper‐case, colon‐separated (e.g. "AA:BB:CC:DD:EE:FF")
        var digits = NormalizeExpression().Replace(raw, "");
        var pairs = Enumerable
            .Range(0, digits.Length / 2)
            .Select(i => digits.Substring(i * 2, 2).ToUpperInvariant());
        var normalized = string.Join(":", pairs);

        Address = PhysicalAddress.Parse(digits);
        Value = normalized;
    }

    public MacAddress(PhysicalAddress addr)
    {
        Address = addr;
        // ToString() on PhysicalAddress yields hex with no separators; insert colons if you want:
        Value = string.Join(":", BitConverter
            .ToString(addr.GetAddressBytes())
            .Split('-'));
    }

    public PhysicalAddress Address { get; }

    public string Value { get; }

    int IComparable.CompareTo(object? obj)
        => obj is MacAddress m
            ? CompareTo(m)
            : throw new ArgumentException("Not a MacAddress", nameof(obj));


    // Order by the normalized string
    public int CompareTo(MacAddress? other)
        => other is null
            ? 1
            : StringComparer.Ordinal.Compare(Value, other.Value);

    public bool Equals(MacAddress? other) =>
        other is not null && Address.Equals(other.Address);

    public override string ToString() => Value;

    public override bool Equals(object? obj) =>
        obj is MacAddress m && Equals(m);

    public override int GetHashCode() => Address.GetHashCode();

    // Implicit conversion so EF Core can read/write string
    public static implicit operator PhysicalAddress(MacAddress m) => m.Address;
    public static explicit operator MacAddress(string s) => new(s);

    [GeneratedRegex("[-:]")]
    private static partial Regex NormalizeExpression();

    [GeneratedRegex(@"^([0-9A-Fa-f]{2}([-:]?)){5}[0-9A-Fa-f]{2}$", RegexOptions.Compiled)]
    private static partial Regex ParseExpression();

    public static bool IsMac(string textTerm) => ParseExpression().IsMatch(textTerm);
}
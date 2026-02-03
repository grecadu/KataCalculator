namespace CalculatorChallenge.Core.Parsing;

public sealed record DelimiterSpec(IReadOnlyList<string> Delimiters)
{
    public static readonly DelimiterSpec Default =
        new(new[] { ",", "\n" });
}

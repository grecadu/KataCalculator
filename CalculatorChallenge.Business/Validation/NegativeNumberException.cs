namespace CalculatorChallenge.Core.Validation;

public sealed class NegativeNumberException : Exception
{
    public IReadOnlyList<int> Negatives { get; }

    public NegativeNumberException(IEnumerable<int> negatives)
        : base($"Negatives not allowed: {string.Join(", ", negatives)}")
    {
        Negatives = negatives.ToArray();
    }
}

public sealed class CalculatorOptions
{
    public bool LimitToTwoNumbers { get; init; } = false;

    public bool DenyNegatives { get; init; } = true;

    public int UpperBound { get; init; } = 1000;

    public string[] BaseDelimiters { get; init; } = new[] { ",", "\n" };

    public bool ShowFormula { get; init; } = false;

    public Operation Operation { get; init; } = Operation.Add;
}

public enum Operation
{
    Add,
    Subtract,
    Multiply,
    Divide
}

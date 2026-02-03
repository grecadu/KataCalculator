public sealed record CalcResult(int Value, string? Formula);

public sealed class StringCalculator
{
    private readonly CalculatorOptions _options;

    public StringCalculator(CalculatorOptions options)
    {
        _options = options;
    }

    public int Add(string? input) => Calculate(input).Value;

    public CalcResult Calculate(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return new CalcResult(0, _options.ShowFormula ? "0 = 0" : null);

        input = NormalizeNewlines(input);

        var delimiters = new List<string>(_options.BaseDelimiters);
        var numbersPart = input;

        if (input.StartsWith("//"))
        {
            var headerEnd = input.IndexOf('\n');
            if (headerEnd > 1)
            {
                var header = input.Substring(2, headerEnd - 2);
                numbersPart = input[(headerEnd + 1)..];

                if (header.StartsWith("["))
                {
                    var matches = System.Text.RegularExpressions.Regex.Matches(header, @"\[(.*?)\]");
                    foreach (System.Text.RegularExpressions.Match m in matches)
                        delimiters.Add(m.Groups[1].Value);
                }
                else
                {
                    delimiters.Add(header); // single char
                }
            }
        }

        delimiters = delimiters
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct()
            .OrderByDescending(d => d.Length)
            .ToList();

        var tokens = Tokenize(numbersPart, delimiters);

        // Step 1: limitar a 2 números si está encendido
        if (_options.LimitToTwoNumbers && tokens.Count > 2)
            throw new Exception("Only up to 2 numbers are allowed in this mode.");

        var normalizedValues = new List<int>(tokens.Count);
        var formulaParts = new List<string>(tokens.Count);
        var negatives = new List<int>();

        foreach (var token in tokens)
        {
            int value;

            if (!int.TryParse(token, out value))
            {
                value = 0;
            }

            if (value < 0)
                negatives.Add(value);

            if (value > _options.UpperBound)
                value = 0;

            normalizedValues.Add(value);
            if (_options.ShowFormula)
                formulaParts.Add(value.ToString());
        }

        if (_options.DenyNegatives && negatives.Count > 0)
            throw new Exception($"Negatives not allowed: {string.Join(", ", negatives)}");

        var result = ApplyOperation(normalizedValues, _options.Operation);

        if (_options.ShowFormula)
        {
            var opSymbol = _options.Operation switch
            {
                Operation.Add => "+",
                Operation.Subtract => "-",
                Operation.Multiply => "*",
                Operation.Divide => "/",
                _ => "+"
            };

            var formula = $"{string.Join(opSymbol, formulaParts)} = {result}";
            return new CalcResult(result, formula);
        }

        return new CalcResult(result, null);
    }

    private static int ApplyOperation(List<int> values, Operation op)
    {
        if (values.Count == 0) return 0;

        return op switch
        {
            Operation.Add => values.Sum(),
            Operation.Subtract => values.Skip(1).Aggregate(values[0], (acc, v) => acc - v),
            Operation.Multiply => values.Aggregate(1, (acc, v) => acc * v),
            Operation.Divide => DivideLeftToRight(values),
            _ => values.Sum()
        };
    }

    private static int DivideLeftToRight(List<int> values)
    {
        int acc = values[0];
        for (int i = 1; i < values.Count; i++)
        {
            var v = values[i];
            if (v == 0) throw new DivideByZeroException("Division by zero in expression.");
            acc /= v;
        }
        return acc;
    }

    private static string NormalizeNewlines(string s)
        => s.Replace("\\r\\n", "\n").Replace("\\n", "\n");

    private static List<string> Tokenize(string input, List<string> delimiters)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();

        int i = 0;
        while (i < input.Length)
        {
            bool matched = false;

            foreach (var d in delimiters)
            {
                if (i + d.Length <= input.Length &&
                    string.CompareOrdinal(input, i, d, 0, d.Length) == 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    i += d.Length;
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                current.Append(input[i]);
                i++;
            }
        }

        result.Add(current.ToString());
        return result;
    }
}

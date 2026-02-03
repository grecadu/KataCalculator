namespace CalculatorChallenge.Core.Parsing;

public sealed class Tokenizer
{
    public IReadOnlyList<string> Tokenize(string input, IReadOnlyList<string> delimiters)
    {
        if (string.IsNullOrEmpty(input))
            return new[] { "" };

        var ordered = delimiters
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct()
            .OrderByDescending(d => d.Length)
            .ToArray();

        var tokens = new List<string>();
        var current = new System.Text.StringBuilder();

        int i = 0;
        while (i < input.Length)
        {
            var matched = false;

            foreach (var d in ordered)
            {
                if (d.Length == 0) continue;
                if (i + d.Length <= input.Length &&
                    string.CompareOrdinal(input, i, d, 0, d.Length) == 0)
                {
                    tokens.Add(current.ToString());
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

        tokens.Add(current.ToString());
        return tokens;
    }
}

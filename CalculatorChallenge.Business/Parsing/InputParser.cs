using System.Text.RegularExpressions;

namespace CalculatorChallenge.Core.Parsing;

public sealed class InputParser
{
    private static readonly Regex MultiDelimiterHeader =
        new(@"^//(?<spec>\[.*\])\n", RegexOptions.Compiled);

    private static readonly Regex BracketedDelimiters =
        new(@"\[(?<d>.*?)\]", RegexOptions.Compiled);

    public ParseResult Parse(string? input)
    {
        input ??= string.Empty;

        var delimiters = DelimiterSpec.Default.Delimiters.ToList();
        var body = input;

        if (body.StartsWith("//"))
        {
            // Case A: //{delimiter}\n...
            // Case B: //[{delimiter}]\n...
            // Case C: //[{d1}][{d2}]...\n...
            var m = MultiDelimiterHeader.Match(body);
            if (m.Success)
            {
                var spec = m.Groups["spec"].Value;
                var ds = new List<string>();

                foreach (Match dm in BracketedDelimiters.Matches(spec))
                    ds.Add(dm.Groups["d"].Value);

                delimiters.AddRange(ds);
                body = body[(m.Length)..]; 
            }
            else
            {
                if (body.Length >= 4 && body[3] == '\n')
                {
                    var d = body[2].ToString();
                    delimiters.Add(d);
                    body = body[4..]; 
                }
                else
                {
                    body = input;
                }
            }
        }

        var tokenizer = new Tokenizer();
        var tokens = tokenizer.Tokenize(body, delimiters);

        return new ParseResult(tokens);
    }
}

public sealed record ParseResult(IReadOnlyList<string> Tokens);

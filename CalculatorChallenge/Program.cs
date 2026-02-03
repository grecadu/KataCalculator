using Microsoft.Extensions.DependencyInjection;

static Dictionary<string, string> ParseArgs(string[] args)
{
    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    for (int i = 0; i < args.Length; i++)
    {
        if (!args[i].StartsWith("--")) continue;

        var key = args[i][2..];
        var value = (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
            ? args[++i]
            : "true";

        dict[key] = value;
    }

    return dict;
}

static bool BoolArg(Dictionary<string, string> p, string key, bool def)
    => p.TryGetValue(key, out var v) && bool.TryParse(v, out var b) ? b : def;

static int IntArg(Dictionary<string, string> p, string key, int def)
    => p.TryGetValue(key, out var v) && int.TryParse(v, out var i) ? i : def;

static string? StringArg(Dictionary<string, string> p, string key)
    => p.TryGetValue(key, out var v) ? v : null;

static Operation OperationArg(Dictionary<string, string> p)
{
    return StringArg(p, "op")?.ToLowerInvariant() switch
    {
        "sub" => Operation.Subtract,
        "mul" => Operation.Multiply,
        "div" => Operation.Divide,
        _ => Operation.Add
    };
}

static int StepArg(Dictionary<string, string> p)
{
    var s = StringArg(p, "step")?.ToLowerInvariant();
    if (s == "final") return 999;
    return int.TryParse(s, out var step) ? step : 999;
}

static void PrintHelp()
{
    Console.WriteLine("""
String Calculator

Usage:
  dotnet run -- [options]

Options:
  --step <1|2|3|4|5|final>
  --denyNegatives <true|false>
  --upperBound <int>
  --newlineDelimiter <string>
  --formula <true|false>
  --op <add|sub|mul|div>
  --help

Examples:
  dotnet run --
  dotnet run -- --step 1
  dotnet run -- --formula true
  dotnet run -- --op mul --formula true
""");
}

var argsMap = ParseArgs(args);

if (argsMap.ContainsKey("help"))
{
    PrintHelp();
    return;
}

var step = StepArg(argsMap);

var baseDelimiters = new List<string> { ",", "\n" };
var altNewline = StringArg(argsMap, "newlineDelimiter");
if (!string.IsNullOrEmpty(altNewline))
{
    baseDelimiters.Remove("\n");
    baseDelimiters.Add(altNewline);
}

var options = new CalculatorOptions
{
    LimitToTwoNumbers = step == 1,
    DenyNegatives = BoolArg(argsMap, "denyNegatives", step >= 4 || step == 999),
    UpperBound = IntArg(argsMap, "upperBound", 1000),
    ShowFormula = BoolArg(argsMap, "formula", false),
    BaseDelimiters = baseDelimiters.ToArray(),
    Operation = OperationArg(argsMap)
};

var services = new ServiceCollection();
services.AddSingleton(options);
services.AddSingleton<StringCalculator>();

using var provider = services.BuildServiceProvider();
var calculator = provider.GetRequiredService<StringCalculator>();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Environment.Exit(0);
};

Console.WriteLine("String Calculator (Ctrl+C to exit)");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    if (input is null) break;

    try
    {
        var result = calculator.Calculate(input);
        Console.WriteLine(options.ShowFormula ? result.Formula : result.Value.ToString());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: {ex.Message}");
    }
}

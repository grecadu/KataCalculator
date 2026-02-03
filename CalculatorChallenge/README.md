# String Calculator – C# Console Application

This project implements the classic **String Calculator Kata**, including all required steps and stretch goals, using clean, extensible .NET design.

---

## Features

### Core Kata
- Comma and newline delimiters
- Unlimited numbers
- Invalid values treated as `0`
- Custom delimiters:
  - Single character: `//;\n1;2`
  - Any length: `//[***]\n1***2`
  - Multiple delimiters: `//[*][!!]\n1*2!!3`
- Ignore values greater than an upper bound (default `1000`)
- Optional negative number validation

### Stretch Goals
- Display calculation formula
- Run continuously until `Ctrl+C`
- Runtime configuration via CLI arguments
- Dependency Injection
- Support for addition, subtraction, multiplication, and division

---

## Usage

```bash
dotnet run -- [options]

| Option               | Description                            | Default |
| -------------------- | -------------------------------------- | ------- |
| `--step`             | Simulate kata step (`1–5` or `final`)  | `final` |
| `--denyNegatives`    | Throw on negative numbers              | `true`  |
| `--upperBound`       | Max allowed number                     | `1000`  |
| `--newlineDelimiter` | Replace newline delimiter              | `\n`    |
| `--formula`          | Show calculation formula               | `false` |
| `--op`               | Operation (`add`, `sub`, `mul`, `div`) | `add`   |
| `--help`             | Show help                              |         |

Examples

dotnet run --

dotnet run -- --formula true

dotnet run -- --step 1

dotnet run -- --op mul --formula true

dotnet run -- --upperBound 500

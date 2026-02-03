using System;
using Xunit;

namespace CalculatorChallenge.Tests;
public sealed class StringCalculatorTests
{
    private static StringCalculator Create(CalculatorOptions? options = null)
        => new StringCalculator(options ?? new CalculatorOptions());

    [Fact]
    public void EmptyInput_ReturnsZero()
    {
        var calc = Create();
        Assert.Equal(0, calc.Add(""));
    }

    [Fact]
    public void SingleNumber_ReturnsValue()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false });
        Assert.Equal(20, calc.Add("20"));
    }

    [Fact]
    public void CommaSeparated_Sums()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false });
        Assert.Equal(5001, calc.Add("1,5000"));
    }

    [Fact]
    public void MissingNumbers_TreatedAsZero()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false });
        Assert.Equal(6, calc.Add("2,,4"));
        Assert.Equal(2, calc.Add(",2"));
        Assert.Equal(2, calc.Add("2,"));
    }

    [Fact]
    public void InvalidTokens_TreatedAsZero()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false });
        Assert.Equal(5, calc.Add("5,tytyt"));
        Assert.Equal(102, calc.Add("//,\n2,ff,100"));
    }

    [Fact]
    public void UnlimitedNumbers_Supported()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false });
        Assert.Equal(78, calc.Add("1,2,3,4,5,6,7,8,9,10,11,12"));
    }

    [Fact]
    public void NewlineDelimiter_Supported()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false });
        Assert.Equal(6, calc.Add("1\n2,3"));
    }

    [Fact]
    public void Step1_LimitToTwoNumbers_WhenEnabled()
    {
        var calc = Create(new CalculatorOptions { LimitToTwoNumbers = true, DenyNegatives = false });

        Assert.Equal(3, calc.Add("1,2"));

        var ex = Assert.Throws<Exception>(() => calc.Add("1,2,3"));
        Assert.Contains("2 numbers", ex.Message, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DenyNegatives_WhenEnabled_ThrowsAndListsAll()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = true });

        var ex = Assert.Throws<Exception>(() => calc.Add("1,-2,-3"));
        Assert.Contains("-2", ex.Message);
        Assert.Contains("-3", ex.Message);
    }

    [Fact]
    public void AllowNegatives_WhenDisabled_DoesNotThrow()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false });
        Assert.Equal(-4, calc.Add("1,-2,-3"));
    }

    [Fact]
    public void ValuesGreaterThanUpperBound_AreInvalid_TreatedAsZero()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false, UpperBound = 1000 });

        Assert.Equal(8, calc.Add("2,1001,6"));
        Assert.Equal(1000, calc.Add("1000,1001"));
    }

    [Fact]
    public void CustomDelimiter_SingleCharacter_Supported()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false });

        Assert.Equal(7, calc.Add("//#\n2#5"));
        Assert.Equal(7, calc.Add("//#\\n2#5")); // console typed \n support
    }

    [Fact]
    public void CustomDelimiter_AnyLength_Supported()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false });

        Assert.Equal(66, calc.Add("//[***]\n11***22***33"));
        Assert.Equal(66, calc.Add("//[***]\\n11***22***33")); // console typed \n support
    }

    [Fact]
    public void CustomDelimiter_Multiple_AnyLength_Supported()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false });

        Assert.Equal(110, calc.Add("//[*][!!][r9r]\n11r9r22*hh*33!!44"));
        Assert.Equal(110, calc.Add("//[*][!!][r9r]\\n11r9r22*hh*33!!44"));
    }

    [Fact]
    public void AlternateNewlineDelimiter_CanBeConfigured()
    {
        var calc = Create(new CalculatorOptions
        {
            DenyNegatives = false,
            BaseDelimiters = new[] { ",", "|" } // replaces newline with |
        });

        Assert.Equal(6, calc.Add("1|2,3"));
    }

    [Fact]
    public void ShowFormula_ReturnsExpandedFormula()
    {
        var calc = Create(new CalculatorOptions
        {
            DenyNegatives = false,
            UpperBound = 1000,
            ShowFormula = true,
            Operation = Operation.Add
        });

        var r = calc.Calculate("2,,4,rrrr,1001,6");
        Assert.Equal(12, r.Value);
        Assert.Equal("2+0+4+0+0+6 = 12", r.Formula);
    }

    [Theory]
    [InlineData("add", "2,3,4", 9)]
    [InlineData("sub", "10,3,2", 5)]   // 10 - 3 - 2
    [InlineData("mul", "2,3,4", 24)]
    [InlineData("div", "100,2,5", 10)] // 100 / 2 / 5
    public void Operations_AreSupported(string op, string input, int expected)
    {
        var operation = op switch
        {
            "add" => Operation.Add,
            "sub" => Operation.Subtract,
            "mul" => Operation.Multiply,
            "div" => Operation.Divide,
            _ => Operation.Add
        };

        var calc = Create(new CalculatorOptions { DenyNegatives = false, Operation = operation });
        Assert.Equal(expected, calc.Add(input));
    }

    [Fact]
    public void DivisionByZero_Throws()
    {
        var calc = Create(new CalculatorOptions { DenyNegatives = false, Operation = Operation.Divide });

        Assert.Throws<DivideByZeroException>(() => calc.Add("10,0"));
    }
}

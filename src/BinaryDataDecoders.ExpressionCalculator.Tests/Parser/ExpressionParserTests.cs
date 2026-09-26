using Antlr4.Runtime.Misc;
using BinaryDataDecoders.ExpressionCalculator.Evaluators;
using BinaryDataDecoders.ExpressionCalculator.Expressions;
using BinaryDataDecoders.ExpressionCalculator.Parser;
using BinaryDataDecoders.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace BinaryDataDecoders.ExpressionCalculator.Tests.Parser;

#region Per Numeric Type
[TestClass]
public class DecimalExpressionParserTests : ExpressionParserTests<decimal>
{
}
[TestClass]
public class DoubleExpressionParserTests : ExpressionParserTests<double>
{
}
[TestClass]
public class FloatExpressionParserTests : ExpressionParserTests<float>
{
}

[TestClass]
public class Int8ExpressionParserTests : ExpressionParserTests<sbyte>
{
    public Int8ExpressionParserTests() : base(skipDecimal: true) { }
}
[TestClass]
public class Int16ExpressionParserTests : ExpressionParserTests<short>
{
    public Int16ExpressionParserTests() : base(skipDecimal: true) { }
}
[TestClass]
public class Int32ExpressionParserTests : ExpressionParserTests<int>
{
    public Int32ExpressionParserTests() : base(skipDecimal: true) { }
}
[TestClass]
public class Int64ExpressionParserTests : ExpressionParserTests<long>
{
    public Int64ExpressionParserTests() : base(skipDecimal: true) { }
}

[TestClass]
public class UInt8ExpressionParserTests : ExpressionParserTests<byte>
{
    public UInt8ExpressionParserTests() : base(skipDecimal: true, skipNegative: true) { }
}
[TestClass]
public class UInt16ExpressionParserTests : ExpressionParserTests<ushort>
{
    public UInt16ExpressionParserTests() : base(skipDecimal: true, skipNegative: true) { }
}
[TestClass]
public class UInt32ExpressionParserTests : ExpressionParserTests<uint>
{
    public UInt32ExpressionParserTests() : base(skipDecimal: true, skipNegative: true) { }
}
[TestClass]
public class UInt64ExpressionParserTests : ExpressionParserTests<ulong>
{
    public UInt64ExpressionParserTests() : base(skipDecimal: true, skipNegative: true) { }
}

#endregion Per Numeric Type

public abstract class ExpressionParserTests<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    private readonly bool _skipDecimal;
    private readonly bool _skipNegative;
    private static readonly IExpressionEvaluator<T> _evaluator = ExpressionEvaluatorFactory.Create<T>();

    protected ExpressionParserTests(
        bool skipDecimal = false,
        bool skipNegative = false
        )
    {
        _skipDecimal = skipDecimal;
        _skipNegative = skipNegative;
    }

    public TestContext TestContext { get; set; }

    private void AssertAreEqual(T expected, T actual)
    {
        // Use epsilon tolerance for floating-point types to handle rounding differences
        // that occur when the optimizer reorders operations
        if (typeof(T) == typeof(double))
        {
            var exp = Convert.ToDouble(expected);
            var act = Convert.ToDouble(actual);
            var epsilon = Math.Max(Math.Abs(exp), Math.Abs(act)) * 1e-10;
            Assert.AreEqual(exp, act, epsilon, $"Expected: {expected}, Actual: {actual}");
        }
        else if (typeof(T) == typeof(float))
        {
            var exp = Convert.ToSingle(expected);
            var act = Convert.ToSingle(actual);
            var epsilon = Math.Max(Math.Abs(exp), Math.Abs(act)) * 1e-5f;
            Assert.AreEqual(exp, act, epsilon, $"Expected: {expected}, Actual: {actual}");
        }
        else if (typeof(T) == typeof(decimal))
        {
            var exp = Convert.ToDecimal(expected);
            var act = Convert.ToDecimal(actual);
            var epsilon = Math.Max(Math.Abs(exp), Math.Abs(act)) * 0.0000000001m;
            Assert.AreEqual(exp, act, epsilon, $"Expected: {expected}, Actual: {actual}");
        }
        else
        {
            // For integer types, use exact equality
            Assert.AreEqual(expected, actual);
        }
    }


    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("-A!")]
    [DataRow("B/*1")]
    [DataRow("B**")]
    [DataRow("B**A")]
    [DataRow("B***A")]
    [DataRow("B*+*A")]
    [DataRow("B*-*A")]
    [DataRow("")]
    [DataRow("b")]
    [DataRow("b+1")]
    [TestTarget(typeof(ExpressionParser<>), Member = nameof(ExpressionParser<double>.Parse))]
    //TODO: this should throw a parse error !!![DataRow("1e")]
    public void PoorlyFormedExpressions(string input)
    {
        TestContext.WriteLine($"Input: {input}");
        var ex = Assert.ThrowsExactly<ParseCanceledException>(() => new ExpressionParser<T>().Parse(input));
        TestContext.WriteLine(ex.Message);
        TestContext.WriteLine(ex.GetType().ToString());
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("2+3*4^5%6/7-8", "2 + 3 * 4 ^ 5 % 6 / 7 - 8", DisplayName = "Parse all operators test")]
    [DataRow("A+B+1", "A + B + 1", DisplayName = "Simple test with variable")]
    [DataRow("Abc1", "Abc1", DisplayName = "Just variable")]
    [DataRow("1.34", "1.34", DisplayName = "Just decimal value")]
    [DataRow("A!", "A!", DisplayName = "Simple factorial")]
    [DataRow("-(A!)", "-(A!)", DisplayName = "Negative factorial")]
    [DataRow("((((((A-B-1*E-06)/C)+0.999999)/1000000)*1000000)*((D*E)*F)*G)+H", "((((((A - B - 1 * E - 6) / C) + 0.999999) / 1000000) * 1000000) * ((D * E) * F) * G) + H", DisplayName = "Parse Complex Expression")]
    [DataRow("B*--A", "B * --A")]
    [TestTarget(typeof(ExpressionParser<>), Member = nameof(ExpressionParser<double>.Parse))]
    public void SimpleParserTests(string input, string result)
    {
        if (_skipDecimal && input.Contains("."))
        {
            Assert.Inconclusive("Decimals not supported");
        }
        else
        {
            TestContext.WriteLine($"Input: {input}");
            var parsed = new ExpressionParser<T>().Parse(input);
            TestContext.WriteLine($"As Parsed: {parsed}");
            Assert.AreEqual(result, parsed.ToString());
        }
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("(A)", "A")]
    [DataRow("(A+(B))", "A + B")]
    [DataRow("B^1", "B")]
    [DataRow("B*1", "B")]
    [DataRow("1*B", "B")]
    [DataRow("B*-1", "-B")]
    [DataRow("-1*B", "-B")]
    [DataRow("B/1", "B")]
    [DataRow("B/-1", "-B")]
    [DataRow("B+0", "B")]
    [DataRow("0+B", "B")]
    [DataRow("B-0", "B")]
    [DataRow("0-B", "-B")]

    [DataRow("2+3*4^5%6/7-8", "-6")]

    [DataRow("2+3", "5")]
    [DataRow("B/B", "1")]
    [DataRow("B%B", "0")]
    [DataRow("B^0", "1")]
    [DataRow("0^B", "0")]
    [DataRow("0*B", "0")]
    [DataRow("B*0", "0")]
    [DataRow("0/B", "0")]
    [DataRow("0%B", "0")]
    [DataRow("B%1", "0")]
    [DataRow("B%-1", "0")]

    [DataRow("-(B)", "-B")]
    [DataRow("-(3)", "-3")]
    [DataRow("-3", "-3")]
    [DataRow("--B", "B")]
    [DataRow("---B", "-B")]
    [DataRow("-1*(A*B)", "-(A * B)")]
    [DataRow("A!", "A!")]
    [DataRow("(A)!", "A!")]
    [DataRow("3!!", "720")]
    [DataRow("2!!!", "2")]
    [DataRow("N!!", "N!!")]
    [DataRow("N!!!", "N!!!")]
    [TestTarget(typeof(ExpressionBaseExtensions), Member = nameof(ExpressionBaseExtensions.Optimize))]
    public void OptimizerTests(string input, string result)
    {
        if (Marshal.SizeOf<T>() == 1 && input.Contains("!!"))
        {
            Assert.Inconclusive($"{nameof(input)}: \"{input}\" not supported");
            return;
        }
        try
        {
            TestContext.WriteLine($"Input: {input}");
            var parsed = new ExpressionParser<T>().Parse(input);
            TestContext.WriteLine($"As Parsed: {parsed}");
            var optimized = parsed.Optimize();
            TestContext.WriteLine($"As Optimized: {optimized}");

            if (_skipNegative && result.StartsWith("-"))
            {
                Assert.Inconclusive($"Negative not supported");
            }
            else
            {
                Assert.AreEqual(result, optimized.ToString());
            }
        }
        catch (NotSupportedException nse) when (_skipNegative && nse.Message == nameof(IExpressionEvaluator<T>.Negate))
        {
            Assert.Inconclusive($"{nse.Message} not supported");
        }
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("B/0")]
    [DataRow("B%0")]
    [TestTarget(typeof(ExpressionBaseExtensions), Member = nameof(ExpressionBaseExtensions.Optimize))]
    public void OptimizerTests_WithExceptions(string input)
    {
        Assert.ThrowsExactly<DivideByZeroException>(() =>
        {
            TestContext.WriteLine($"Input: {input}");
            var parsed = new ExpressionParser<T>().Parse(input);
            TestContext.WriteLine($"As Parsed: {parsed}");
            var optimized = parsed.Optimize();
            TestContext.WriteLine($"As Optimized: {optimized}");
        });
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("A+B+C", "A, B, C")]
    //[DataRow("A+B+B", "A, B")]
    //[DataRow("Abc+XyW1", "Abc, XyW1")]
    [TestTarget(typeof(ExpressionBaseExtensions), Member = nameof(ExpressionBaseExtensions.GetDistinctVariableNames))]
    public void GetDistinctVariablesTests(string input, string result)
    {
        TestContext.WriteLine($"Input: {input}");
        var parsed = new ExpressionParser<T>().Parse(input);
        TestContext.WriteLine($"As Parsed: {parsed}");
        var variables = string.Join(", ", parsed.GetDistinctVariableNames());
        TestContext.WriteLine($"Variables: {variables}");
        Assert.AreEqual(result, variables.ToString());
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("A", DisplayName = "Check Expressions \"A\"")]
    [DataRow("A*1", DisplayName = "Check Expressions \"A*1\"")]
    [DataRow("(A*B)+C", DisplayName = "Check Expressions \"(A*B)+C\"")]
    [DataRow("(A*B)", DisplayName = "Check Expressions \"(A*B)\"")]
    [DataRow("(A-B)/A", DisplayName = "Check Expressions \"(A-B)/A\"")]
    [DataRow("(A*((B+(-1*C))+((D*(B+(-1*C)))+E)+((F*G)+H))+((D*(B+(-1*C)))+E))", DisplayName = "Check Expressions \"(A*((B+(-1*C))+((D*(B+(-1*C)))+E)+((F*G)+H))+((D*(B+(-1*C)))+E))\"")]
    [DataRow("((A+B+((C*D)+E))*F)+G+((D/H)*I)+J", DisplayName = "Check Expressions \"((A+B+((C*D)+E))*F)+G+((D/H)*I)+J\"")]
    [DataRow("A*(B/C)+D", DisplayName = "Check Expressions \"A*(B/C)+D\"")]
    [DataRow("A*B+C", DisplayName = "Check Expressions \"A*B+C\"")]
    [DataRow("(((A-(B*C))/D)*E)+F", DisplayName = "Check Expressions \"(((A-(B*C))/D)*E)+F\"")]
    [DataRow("(A/B)-(C+D+E+F)", DisplayName = "Check Expressions \"(A/B)-(C+D+E+F)\"")]
    [DataRow("((A*B)*C)+D", DisplayName = "Check Expressions \"((A*B)*C)+D\"")]
    [DataRow("(((A*B)*C)*D)+E", DisplayName = "Check Expressions \"(((A*B)*C)*D)+E\"")]
    [DataRow("(((A/B)+(-1*C))*(D*E)*F)+G", DisplayName = "Check Expressions \"(((A/B)+(-1*C))*(D*E)*F)+G\"")]
    [DataRow("(A*B*(C/D))+E", DisplayName = "Check Expressions \"(A*B*(C/D))+E\"")]
    [DataRow("(A*(B/C)+D)", DisplayName = "Check Expressions \"(A*(B/C)+D)\"")]
    [DataRow("A+B", DisplayName = "Check Expressions \"A+B\"")]
    [DataRow("((A/B)*(C*D)*E)+F", DisplayName = "Check Expressions \"((A/B)*(C*D)*E)+F\"")]
    [DataRow("((A+((B*C)+D))*E)+F+(C*G)+H", DisplayName = "Check Expressions \"((A+((B*C)+D))*E)+F+(C*G)+H\"")]
    [DataRow("(A+((B*C)+D))*E+F+(A+((B*C)+D))*G+H", DisplayName = "Check Expressions \"(A+((B*C)+D))*E+F+(A+((B*C)+D))*G+H\"")]
    [DataRow("(((((A*B)+C)+((D*E)+F))*G)+H)+(-1*((A+(-1*(A*B)))+C))", DisplayName = "Check Expressions \"(((((A*B)+C)+((D*E)+F))*G)+H)+(-1*((A+(-1*(A*B)))+C))\"")]
    [DataRow("((A*B)+C)+((D*B)+E)", DisplayName = "Check Expressions \"((A*B)+C)+((D*B)+E)\"")]
    [DataRow("(A*(((B*C)+D)+((E*F)+G))+H)+(-1*(B+(-1*((B*C)+D))))+(I*(((B*J)+K)+((E*F)+G))+L)", DisplayName = "Check Expressions \"(A*(((B*C)+D)+((E*F)+G))+H)+(-1*(B+(-1*((B*C)+D))))+(I*(((B*J)+K)+((E*F)+G))+L)\"")]
    [DataRow("(A*(((B*C)+D)+((E*((F*G)+H))+I))+J)+(-1*(B+(-1*((B*C)+D))))", DisplayName = "Check Expressions \"(A*(((B*C)+D)+((E*((F*G)+H))+I))+J)+(-1*(B+(-1*((B*C)+D))))\"")]
    [DataRow("(A*(B+((C*D)+E))+F)+(G*(B+((C*D)+E)+(A*(B+((C*D)+E))+F))+H)", DisplayName = "Check Expressions \"(A*(B+((C*D)+E))+F)+(G*(B+((C*D)+E)+(A*(B+((C*D)+E))+F))+H)\"")]
    [DataRow("(A*(((B*C)+D)+((E*F)+G)))+(-1*(B+(-1*((B*C)+D))))+(H*(((B*I)+J)+((E*F)+G)+(A*(((B*C)+D)+((E*F)+G)))+(-1*(B+(-1*((B*C)+D)))))+K)", DisplayName = "Check Expressions \"(A*(((B*C)+D)+((E*F)+G)))+(-1*(B+(-1*((B*C)+D))))+(H*(((B*I)+J)+((E*F)+G)+(A*(((B*C)+D)+((E*F)+G)))+(-1*(B+(-1*((B*C)+D)))))+K)\"")]
    [DataRow("((A*B)+C)+(-1*A)", DisplayName = "Check Expressions \"((A*B)+C)+(-1*A)\"")]
    [DataRow("(((A+(-1*B))+((C*D)+E))*F)+G+(D*H)+I", DisplayName = "Check Expressions \"(((A+(-1*B))+((C*D)+E))*F)+G+(D*H)+I\"")]
    [DataRow("((A+(-1*B))+((C*D)+E))*F+G+((A+(-1*B))+((C*D)+E))*H+I", DisplayName = "Check Expressions \"((A+(-1*B))+((C*D)+E))*F+G+((A+(-1*B))+((C*D)+E))*H+I\"")]
    [DataRow("((A*(B+(-1*C)))+D)+((E*(B+(-1*C)))+F)", DisplayName = "Check Expressions \"((A*(B+(-1*C)))+D)+((E*(B+(-1*C)))+F)\"")]
    [DataRow("(A*((((B+(-1*C))*D)+E)+((F*((G*H)+I))+J))+K)+(-1*((B+(-1*C))+(-1*(((B+(-1*C))*D)+E))))", DisplayName = "Check Expressions \"(A*((((B+(-1*C))*D)+E)+((F*((G*H)+I))+J))+K)+(-1*((B+(-1*C))+(-1*(((B+(-1*C))*D)+E))))\"")]
    [DataRow("(A*((B+(-1*C))+((D*E)+F))+G)+(H*((B+(-1*C))+((D*E)+F)+(A*((B+(-1*C))+((D*E)+F))+G))+I)", DisplayName = "Check Expressions \"(A*((B+(-1*C))+((D*E)+F))+G)+(H*((B+(-1*C))+((D*E)+F)+(A*((B+(-1*C))+((D*E)+F))+G))+I)\"")]
    [DataRow("(A*((B+(-1*C))*D+E+(F*G)+H))+(-1*(B+(-1*C)+(-1*((B+(-1*C))*D+E))))+I*((B+(-1*C))*J+K+(F*G)+H+(A*((((B+(-1*C))*D)+E)+(F*G)+H))+(-1*(B+(-1*C)+(-1*((B+(-1*C))*D+E)))))+L", DisplayName = "Check Expressions \"(A*((B+(-1*C))*D+E+(F*G)+H))+(-1*(B+(-1*C)+(-1*((B+(-1*C))*D+E))))+I*((B+(-1*C))*J+K+(F*G)+H+(A*((((B+(-1*C))*D)+E)+(F*G)+H))+(-1*(B+(-1*C)+(-1*((B+(-1*C))*D+E)))))+L\"")]
    [DataRow("(((A+(-1*B))*C)+D)+(B+(-1*A))", DisplayName = "Check Expressions \"(((A+(-1*B))*C)+D)+(B+(-1*A))\"")]
    [DataRow("((A*(((B*C)+D)+(E*F)))+(-1*(B*(1+(-1*C))+D)+G))", DisplayName = "Check Expressions \"((A*(((B*C)+D)+(E*F)))+(-1*(B*(1+(-1*C))+D)+G))\"")]
    [DataRow("((A+((B*C)+D))*E)+F+((C/G)*H)+I", DisplayName = "Check Expressions \"((A+((B*C)+D))*E)+F+((C/G)*H)+I\"")]
    [DataRow("(A+B)*C+D+(A+B)*E+F", DisplayName = "Check Expressions \"(A+B)*C+D+(A+B)*E+F\"")]
    [DataRow("A*B", DisplayName = "Check Expressions \"A*B\"")]
    [DataRow("((A+((-1*B)*(C/D)))*(E/C))+(-1*F)+G", DisplayName = "Check Expressions \"((A+((-1*B)*(C/D)))*(E/C))+(-1*F)+G\"")]
    [DataRow("((A/(1-B))*C)+D", DisplayName = "Check Expressions \"((A/(1-B))*C)+D\"")]
    [DataRow("(((A-(B*C))/(1-D))*E)+F", DisplayName = "Check Expressions \"(((A-(B*C))/(1-D))*E)+F\"")]
    [DataRow("(A*(1+(-1*B))+C)*D+E", DisplayName = "Check Expressions \"(A*(1+(-1*B))+C)*D+E\"")]
    [DataRow("(A+B)*C+D", DisplayName = "Check Expressions \"(A+B)*C+D\"")]
    [DataRow("(((A/B)*C*D)*E*F)+G", DisplayName = "Check Expressions \"(((A/B)*C*D)*E*F)+G\"")]
    [DataRow("((A-B)*C)+D", DisplayName = "Check Expressions \"((A-B)*C)+D\"")]
    [DataRow("((A/B)*C)+D", DisplayName = "Check Expressions \"((A/B)*C)+D\"")]
    [DataRow("((((A*B)+C)/D)*E)+F", DisplayName = "Check Expressions \"((((A*B)+C)/D)*E)+F\"")]
    [DataRow("((A/B)*C*D*E)+F", DisplayName = "Check Expressions \"((A/B)*C*D*E)+F\"")]
    [DataRow("(A*B*C)+D", DisplayName = "Check Expressions \"(A*B*C)+D\"")]
    [DataRow("(((A*B)+C)*D)+E", DisplayName = "Check Expressions \"(((A*B)+C)*D)+E\"")]
    [DataRow("(((A/B)*C*D)*E)+F", DisplayName = "Check Expressions \"(((A/B)*C*D)*E)+F\"")]
    [DataRow("(A*1)", DisplayName = "Check Expressions \"(A*1)\"")]
    [DataRow("(((A/B)+(-1*C))*D*E*F)+G", DisplayName = "Check Expressions \"(((A/B)+(-1*C))*D*E*F)+G\"")]
    [DataRow("((A*B)+C)+(D*E*F)", DisplayName = "Check Expressions \"((A*B)+C)+(D*E*F)\"")]
    [DataRow("((A/(1+(-1*(B+C))))*D)+E", DisplayName = "Check Expressions \"((A/(1+(-1*(B+C))))*D)+E\"")]
    [DataRow("((A-(B*C))*D)+E", DisplayName = "Check Expressions \"((A-(B*C))*D)+E\"")]
    [DataRow("(((A+(-1*B))/C)*(D*E)*F)+G", DisplayName = "Check Expressions \"(((A+(-1*B))/C)*(D*E)*F)+G\"")]
    [DataRow("((A/B)*((C*D)*E)*F)+G", DisplayName = "Check Expressions \"((A/B)*((C*D)*E)*F)+G\"")]
    [DataRow("((((A/B)*C)+D)*((E*F)*G))+H", DisplayName = "Check Expressions \"((((A/B)*C)+D)*((E*F)*G))+H\"")]
    [DataRow("(((A/B)+(-1*C))*((D*E)*F)*G)+H", DisplayName = "Check Expressions \"(((A/B)+(-1*C))*((D*E)*F)*G)+H\"")]
    [DataRow("(((A+(-1*B))/C)*((D*E)*F)*G)+H", DisplayName = "Check Expressions \"(((A+(-1*B))/C)*((D*E)*F)*G)+H\"")]
    [DataRow("(((A/B)*(((C*D)*E)*F))+(C*D*G))+H", DisplayName = "Check Expressions \"(((A/B)*(((C*D)*E)*F))+(C*D*G))+H\"")]
    [DataRow("((A*B)+((A*C)-A)*D)+E", DisplayName = "Check Expressions \"((A*B)+((A*C)-A)*D)+E\"")]
    [DataRow("((((((A-B-1*E-06)/C)+0.999999)/1000000)*1000000)*((D*E)*F)*G)+H", DisplayName = "Check Expressions \"((((((A-B-1E-06)/C)+0.999999)/1000000)*1000000)*((D*E)*F)*G)+H\"")]
    [DataRow("(((A/B)*((C*D)*E)*F)*G)+H", DisplayName = "Check Expressions \"(((A/B)*((C*D)*E)*F)*G)+H\"")]
    [DataRow("((A/B)*(((C*D)*E)*F))+G", DisplayName = "Check Expressions \"((A/B)*(((C*D)*E)*F))+G\"")]
    [DataRow("((A/B)*((C*D)*E))+F", DisplayName = "Check Expressions \"((A/B)*((C*D)*E))+F\"")]
    [DataRow("(A/(A+B))", DisplayName = "Check Expressions \"(A/(A+B))\"")]
    [DataRow("A!")]
    [TestTarget(typeof(ExpressionBaseExtensions), Member = nameof(ExpressionBaseExtensions.Optimize))]
    public void VerifyOptimizerForComplexExpressions(string input)
    {
        const int MaxRetryAttempts = 10;
        var includesFactorial = input.Contains("!");

        for (int attempt = 0; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                if (_skipDecimal && input.Contains("."))
                {
                    Assert.Inconclusive("Decimals not supported");
                }
                else
                {
                    TestContext.WriteLine($"Input: {input}");
                    var parsed = new ExpressionParser<T>().Parse(input);
                    TestContext.WriteLine($"As Parsed: {parsed}");
                    var optimized = parsed.Optimize();
                    TestContext.WriteLine($"As Optimized: {optimized}");

                    var testValues = parsed.GenerateTestValues(includeNegatives: !includesFactorial, scale: includesFactorial ? 1 : 4);

                    var variables = string.Join(", ", testValues.Select(kvp => (Name: kvp.Key, kvp.Value)));
                    TestContext.WriteLine($"Variables: {variables}");

                    var resultAsParsed = parsed.Evaluate(testValues);
                    var resultAsOptimized = optimized.Evaluate(testValues);

                    TestContext.WriteLine($"Parsed Result: {resultAsParsed}");
                    TestContext.WriteLine($"Optimized Result: {resultAsOptimized}");

                    AssertAreEqual(resultAsParsed, resultAsOptimized);
                }
                return; // Test passed, exit the retry loop
            }
            catch (NotSupportedException nse) when (_skipNegative && nse.Message == nameof(IExpressionEvaluator<T>.Negate))
            {
                Assert.Inconclusive($"{nse.Message} not supported");
            }
            catch (DivideByZeroException) when (attempt < MaxRetryAttempts)
            {
                // Retry with new random values
                continue;
            }
        }
    }
}

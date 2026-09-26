using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BinaryDataDecoders.TestUtilities;

[AttributeUsage(AttributeTargets.Method)]
public class ContextualTestMethodAttribute : TestMethodAttribute
{
    public const string CurrentTestMethod = nameof(CurrentTestMethod);
    public const string CurrentTestInstance = nameof(CurrentTestInstance);

    private readonly static AsyncLocal<ITestMethod?> _current = new();
    private readonly static AsyncLocal<object?> _instance = new();

    public static ITestMethod? Current => _current.Value;
    public static object? Instance
    {
        get => _instance.Value;
        set => _instance.Value = value;
    }

    public ContextualTestMethodAttribute()
    {
    }

    public ContextualTestMethodAttribute(string? displayName) : base(displayName)
    {
    }

    public override async Task<TestResult[]> ExecuteAsync(ITestMethod testMethod)
    {
        _current.Value = testMethod;
        var ret = await base.ExecuteAsync(testMethod);
        _current.Value = null;
        return ret;
    }
}

using BinaryDataDecoders.ExpressionCalculator.Expressions;
using BinaryDataDecoders.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BinaryDataDecoders.ExpressionCalculator.Tests.Expressions;

[TestClass]
public class VariableExpressionTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(VariableExpression<>), Member = nameof(VariableExpression<double>.Equals))]
    public void Equals_SameReference_Test()
    {
        var exp = new VariableExpression<double>("Test");
        Assert.IsTrue(exp.Equals(exp));
    }
    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(VariableExpression<>), Member = nameof(VariableExpression<double>.Equals))]
    public void Equals_SameValue_Test()
    {
        var var1 = new VariableExpression<double>("Test1");
        var var2 = new VariableExpression<double>("Test1");
        Assert.IsTrue(var1.Equals(var2));
    }
    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(VariableExpression<>), Member = nameof(VariableExpression<double>.Equals))]
    public void Equals_DifferentValue_Test()
    {
        var var1 = new VariableExpression<double>("Test1");
        var var2 = new VariableExpression<double>("Test2");
        Assert.IsFalse(var1.Equals(var2));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(VariableExpression<>), Member = nameof(VariableExpression<double>.Equals))]
    public void Equals_SameString_Test()
    {
        var var1 = new VariableExpression<double>("Test1");
        var var2 = "Test1";
        Assert.IsTrue(var1.Equals(var2));
    }
    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(VariableExpression<>), Member = nameof(VariableExpression<double>.Equals))]
    public void Equals_DifferentString_Test()
    {
        var var1 = new VariableExpression<double>("Test1");
        var var2 = "Test2";
        Assert.IsFalse(var1.Equals(var2));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(VariableExpression<>))]
    public void NullVariableName_Test()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => new VariableExpression<double>(null));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(VariableExpression<>))]
    public void EmptyVariableName_Test()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => new VariableExpression<double>(""));
    }
}

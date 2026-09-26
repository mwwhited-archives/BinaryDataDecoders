using BinaryDataDecoders.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryDataDecoders.Cryptography.Tests;

[TestClass]
public class CaesarTests
{
    public TestContext TestContext { get; set; }

    [DataTestMethod]
    [DataRow("Hello World", 'H', "Olssv Dvysk")]
    [DataRow("Hello, World", 'H', "Olssv, Dvysk")]
    [DataRow("hello, world", 'h', "olssv, dvysk")]
    [DataRow("hello world", 'C', "jgnnq yqtnf")]
    [TestCategory(TestCategories.Unit)]
    public void EncodeTest(string message, char key, string expected)
    {
        var result = new Caesar().Encode(message, key);
        this.TestContext.WriteLine($"{message} -> {result}");
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow("Olssv Dvysk", 'H', "Hello World")]
    [DataRow("Olssv, Dvysk", 'H', "Hello, World")]
    [DataRow("olssv, dvysk", 'h', "hello, world")]
    [DataRow("jgnnq yqtnf", 'C', "hello world")]
    [TestCategory(TestCategories.Unit)]
    public void DecodeTest(string message, char key, string expected)
    {
        var result = new Caesar().Decode(message, key);
        this.TestContext.WriteLine($"{message} -> {result}");
        Assert.AreEqual(expected, result);
    }
}

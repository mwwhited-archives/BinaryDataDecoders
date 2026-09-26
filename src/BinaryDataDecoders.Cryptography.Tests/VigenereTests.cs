using BinaryDataDecoders.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryDataDecoders.Cryptography.Tests;

[TestClass]
public class VigenereTests
{
    public TestContext TestContext { get; set; }

    [DataTestMethod]
    [DataRow("Hello World", "World", "Dscwr Kfcoz")]
    [DataRow("Hello, World", "world", "Dscwr, Nzuhr")]
    [DataRow("hello, world", "World", "dscwr, nzuhr")]
    [DataRow("hello world", "Hello", "oiwwc azczk")]
    [TestCategory(TestCategories.Unit)]
    public void EncodeTest(string message, string key, string expected)
    {
        var result = new Vigenere().Encode(message, key);
        this.TestContext.WriteLine($"{message} -> {result}");
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow("Dscwr Kfcoz", "World", "Hello World")]
    [DataRow("Dscwr, Nzuhr", "World", "Hello, World")]
    [DataRow("dscwr, nzuhr", "World", "hello, world")]
    [DataRow("oiwwc azczk", "Hello", "hello world")]
    [TestCategory(TestCategories.Unit)]
    public void DecodeTest(string message, string key, string expected)
    {
        var result = new Vigenere().Decode(message, key);
        this.TestContext.WriteLine($"{message} -> {result}");
        Assert.AreEqual(expected, result);
    }
}

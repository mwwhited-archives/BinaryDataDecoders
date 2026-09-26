using BinaryDataDecoders.TestUtilities;
using BinaryDataDecoders.ToolKit.Codecs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryDataDecoders.ToolKit.Tests.Codecs;

[TestClass]
public class MorseCodeTests
{
    public TestContext TestContext { get; set; }

    [DataTestMethod]
    [DataRow("Hello, World!", ".... . .-.. .-.. ---  .-- --- .-. .-.. -..")]
    [DataRow("hello world", ".... . .-.. .-.. ---  .-- --- .-. .-.. -..")]
    [DataRow("abcdefghijklmnopqrstuvwxyz1234567890", ".- -... -.-. -.. . ..-. --. .... .. .--- -.- .-.. -- -. --- .--. --.- .-. ... - ..- ...- .-- -..- -.-- --.. .---- ..--- ...-- ....- ..... -.... --... ---.. ----. -----")]
    [TestCategory(TestCategories.Unit)]
    public void EncodeTest(string message, string expected)
    {
        var result = new MorseCode().Encode(message);
        this.TestContext.WriteLine($"{message} -> {result}");
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(".... . .-.. .-.. ---  .-- --- .-. .-.. -..", "HELLO WORLD")]
    [DataRow(".- -... -.-. -.. . ..-. --. .... .. .--- -.- .-.. -- -. --- .--. --.- .-. ... - ..- ...- .-- -..- -.-- --..  .---- ..--- ...-- ....- ..... -.... --... ---.. ----. -----", "ABCDEFGHIJKLMNOPQRSTUVWXYZ 1234567890")]
    [TestCategory(TestCategories.Unit)]
    public void DecodeTest(string message, string expected)
    {
        var result = new MorseCode().Decode(message);
        this.TestContext.WriteLine($"{message} -> {result}");
        Assert.AreEqual(expected, result);
    }
}

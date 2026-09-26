using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryDataDecoders.TestUtilities;

/// <summary>
/// this is a base class to handle setting up a TestContext and bind up the ContextualTestMethod
/// </summary>
public abstract class ContextualTestClassBase
{
    // https://github.com/MicrosoftDocs/visualstudio-docs/blob/main/docs/test/using-microsoft-visualstudio-testtools-unittesting-members-in-unit-tests.md
    // https://github.com/dotnet/docs/blob/main/docs/core/tutorials/testing-library-with-visual-studio.md
    public virtual TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public virtual void TestInitialize()
    {
        if (ContextualTestMethodAttribute.Current == null)
        {
            if (TestContext.Properties.ContainsKey(ContextualTestMethodAttribute.CurrentTestMethod))
                TestContext.Properties.Remove(ContextualTestMethodAttribute.CurrentTestMethod);
        }
        else
        {
            TestContext.Properties[ContextualTestMethodAttribute.CurrentTestMethod] = ContextualTestMethodAttribute.Current;
        }
        TestContext.Properties[ContextualTestMethodAttribute.CurrentTestInstance] = ContextualTestMethodAttribute.Instance = this;
    }
    [TestCleanup]
    public virtual void TestCleanup()
    {
        if (TestContext.Properties.ContainsKey(ContextualTestMethodAttribute.CurrentTestMethod))
            TestContext.Properties.Remove(ContextualTestMethodAttribute.CurrentTestMethod);
        if (TestContext.Properties.ContainsKey(ContextualTestMethodAttribute.CurrentTestInstance))
            TestContext.Properties.Remove(ContextualTestMethodAttribute.CurrentTestInstance);
    }
}

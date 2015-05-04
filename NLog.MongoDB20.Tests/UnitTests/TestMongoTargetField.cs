using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.Layouts;


namespace NLog.MongoDB20.Tests
{
    [TestClass]
    public class TestMongoTargetField
    {
        [TestMethod]
        public void TestConstructor()
        {
            const string name = "SomeName!";
            var layout = new SimpleLayout
                             {
                                 Text = "SomeText"
                             };

            var field = new MongoDBTargetField(name, layout);

            field.Name
                .Should().Be(name);
            field.Layout
                .Should().Be(layout);
        }
    }
}
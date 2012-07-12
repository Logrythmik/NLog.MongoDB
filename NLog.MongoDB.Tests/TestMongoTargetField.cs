using FluentAssertions;
using NLog.Layouts;
using NUnit.Framework;

namespace NLog.MongoDB.Tests
{
    [TestFixture]
    public class TestMongoTargetField
    {
        [Test]
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
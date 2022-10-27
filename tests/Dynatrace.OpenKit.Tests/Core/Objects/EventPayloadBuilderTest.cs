using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Util.Json.Objects;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Objects
{
    class EventPayloadBuilderTest
    {
        private ILogger mockLogger;

        [SetUp]
        public void Setup()
        {
            mockLogger = Substitute.For<ILogger>();
        }

        [Test]
        public void CreateEmptyPayloadBuilder()
        {
            EventPayloadBuilder builder = new EventPayloadBuilder(new Dictionary<string, JsonValue>(), mockLogger);
            Assert.That(builder.Build(), Is.EqualTo("{}"));
        }

        [Test]
        public void CreateEmptyPayloadBuilderWithNullAttributes()
        {
            EventPayloadBuilder builder = new EventPayloadBuilder(null, mockLogger);
            Assert.That(builder.Build(), Is.EqualTo("{}"));
        }

        [Test]
        public void RemovingReservedInternalValues()
        {
            Dictionary<string, JsonValue> dict = new Dictionary<string, JsonValue>();
            dict.Add("dt", JsonStringValue.FromString("Removed"));
            dict.Add("dt.hello", JsonStringValue.FromString("Removed"));
            dict.Add("event.kind", JsonStringValue.FromString("Okay"));

            EventPayloadBuilder builder = new EventPayloadBuilder(dict, mockLogger);
            builder.CleanReservedInternalAttributes();
            Assert.That(builder.Build(), Is.EqualTo("{\"event.kind\":\"Okay\"}"));
        }

        [Test]
        public void AddNonOverridableAttributeWhichIsAlreadyAvailable()
        {
            Dictionary<string, JsonValue> dict = new Dictionary<string, JsonValue>();
            dict.Add("dt.rum.sid", JsonStringValue.FromString("MySession"));

            EventPayloadBuilder builder = new EventPayloadBuilder(dict, mockLogger);
            builder.AddNonOverridableAttribute("dt.rum.sid", JsonStringValue.FromString("ComingFromAgent"));

            Assert.That(builder.Build(), Is.EqualTo("{\"dt.rum.sid\":\"ComingFromAgent\"}"));
        }

        [Test]
        public void AddNonOverridableAttributeWhichIsNotAvailable()
        {
            EventPayloadBuilder builder = new EventPayloadBuilder(new Dictionary<string, JsonValue>(), mockLogger);
            builder.AddNonOverridableAttribute("NonOverridable", JsonStringValue.FromString("Changed"));

            Assert.That(builder.Build(), Is.EqualTo("{\"NonOverridable\":\"Changed\"}"));
        }

        [Test]
        public void AddOverridableAttributeWhichIsAlreadyAvailable()
        {
            Dictionary<string, JsonValue> dict = new Dictionary<string, JsonValue>();
            dict.Add("timestamp", JsonStringValue.FromString("Changed"));

            EventPayloadBuilder builder = new EventPayloadBuilder(dict, mockLogger);
            builder.AddOverridableAttribute("timestamp", JsonStringValue.FromString("ComingFromAgent"));

            Assert.That(builder.Build(), Is.EqualTo("{\"timestamp\":\"Changed\"}"));
        }

        [Test]
        public void AddOverridableAttributeWhichIsNotAvailable()
        {
            EventPayloadBuilder builder = new EventPayloadBuilder(new Dictionary<string, JsonValue>(), mockLogger);
            builder.AddOverridableAttribute("Overridable", JsonStringValue.FromString("Changed"));

            Assert.That(builder.Build(), Is.EqualTo("{\"Overridable\":\"Changed\"}"));
        }
    }
}

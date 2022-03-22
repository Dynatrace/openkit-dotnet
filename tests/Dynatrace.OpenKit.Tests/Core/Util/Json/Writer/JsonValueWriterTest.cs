using Dynatrace.OpenKit.Util.Json.Writer;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Util.Json.Writer
{
    public class JsonValueWriterTest
    {
		[Test]
		public void CheckOpenArrayCharacter()
		{
			JsonValueWriter writer = new JsonValueWriter();
			writer.OpenArray();

			// then
			Assert.That(writer.ToString(), Is.EqualTo("["));
		}

		[Test]
	public void checkCloseArrayCharacter()
		{
			JsonValueWriter writer = new JsonValueWriter();
			writer.CloseArray();

			// then
			Assert.That(writer.ToString(), Is.EqualTo("]"));
		}

		[Test]
	public void checkOpenObjectCharacter()
		{
			JsonValueWriter writer = new JsonValueWriter();
			writer.OpenObject();

			// then
			Assert.That(writer.ToString(), Is.EqualTo("{"));
		}

		[Test]
	public void checkCloseObjectCharacter()
		{
			JsonValueWriter writer = new JsonValueWriter();
			writer.CloseObject();

			// then
			Assert.That(writer.ToString(), Is.EqualTo("}"));
		}

		[Test]
	public void checkElementSeperatorCharacter()
		{
			JsonValueWriter writer = new JsonValueWriter();
			writer.InsertElementSeperator();

			// then
			Assert.That(writer.ToString(), Is.EqualTo(","));
		}

		[Test]
	public void checkKeyValueSeperatorCharacter()
		{
			JsonValueWriter writer = new JsonValueWriter();
			writer.InsertKeyValueSeperator();

			// then
			Assert.That(writer.ToString(), Is.EqualTo(":"));
		}

		[Test]
	public void checkValueFormatting()
		{
			JsonValueWriter writer = new JsonValueWriter();
			writer.InsertValue("false");

			// then
			Assert.That(writer.ToString(), Is.EqualTo("false"));
		}

		[Test]
	public void checkStringValueFormatting()
		{
			JsonValueWriter writer = new JsonValueWriter();
			writer.InsertStringValue("false");

			// then
			Assert.That(writer.ToString(), Is.EqualTo("\"false\""));
		}

		[Test]
	public void checkKeyFormatting()
		{
			JsonValueWriter writer = new JsonValueWriter();
			writer.InsertKey("Key");

			// then
			Assert.That(writer.ToString(), Is.EqualTo("\"Key\""));
		}
	}
}

using Xunit;

namespace Decorator.Tests {

	public class CanDeserializeProperly {

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanDeserialize")]
		public void TooShortMessage()
			=> Assert.False(Deserializer.CanDeserialize<TestMessage>(Setup.TooShort));

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanDeserialize")]
		public void TooLongMessage()
			=> Assert.False(Deserializer.CanDeserialize<TestMessage>(Setup.TooLong));

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanDeserialize")]
		public void InvalidBaseMessage()
			=> Assert.False(Deserializer.CanDeserialize<TestMessage>(Setup.InvalidBase));

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanDeserialize")]
		public void IncorrectTypesMessage()
			=> Assert.False(Deserializer.CanDeserialize<TestMessage>(Setup.IncorrectTypes));

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanDeserialize")]
		public void NullValuesMessage()
			=> Assert.False(Deserializer.CanDeserialize<TestMessage>(Setup.NullValues));

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanDeserialize")]
		public void NullTypeMessage()
			=> Assert.False(Deserializer.CanDeserialize<TestMessage>(Setup.NullType));

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanDeserialize")]
		public void AllNullMessage()
			=> Assert.False(Deserializer.CanDeserialize<TestMessage>(Setup.AllNull));

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanDeserialize")]
		public void CorrectMessage()
			=> Assert.True(Deserializer.CanDeserialize<TestMessage>(Setup.Correct));

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanDeserialize")]
		public void OnlyNullMessage()
			=> Assert.True(Deserializer.CanDeserialize<NullType>(Setup.OnlyNullType));
	}
}
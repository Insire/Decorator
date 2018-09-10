﻿using System.Collections.Generic;

using Xunit;

namespace Decorator.Tests {

	public class DesHandlers {

		[Fact]
		[Trait("Category", "HandlerDeserialization")]
		public void DesLotsLotsLOTS() {
			var setup = Setup.GetSetup();
			for (int i = 0; i < 100_000; i++)
				setup.Deserializer.DeserializeMessageToMethod(setup.Instance, Setup.Correct);
		}

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "HandlerDeserialization")]
		public void DeserializesToHandlerTestMessage() {
			var setup = Setup.GetSetup();

			setup.Deserializer.DeserializeItemToMethod(setup.Instance, new TestMessage {
				PositionZeroItem = "",
				PositionOneItem = 1337
			});

			Assert.True(setup.Instance.Invoked);
		}

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "HandlerDeserialization")]
		public void DeserializeToHandlerMessage() {
			var setup = Setup.GetSetup();

			setup.Deserializer.DeserializeMessageToMethod(setup.Instance, Setup.Correct);

			Assert.True(setup.Instance.Invoked);
		}

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "HandlerDeserialization")]
		public void DeserializesEnumerable() {
			var setup = Setup.GetSetup();

			var args = new List<object>();

			var msg = Setup.Correct;

			// 4 is arbitrary here
			for (int i = 0; i < 4; i++) {
				msg.Arguments[1] = i;
				args.AddRange(msg.Arguments);
			}

			setup.Deserializer.DeserializeMessageToMethod(setup.Instance, new BasicMessage("test", args.ToArray()));

			Assert.True(setup.Instance.Invoked);
		}

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "HandlerDeserialization")]
		public void DoesntDeserializesNonEnumerable() {
			var setup = Setup.GetSetup();

			var args = new List<object>();

			var msg = Setup.NonRepeatable;

			// 4 is arbitrary here
			for (int i = 0; i < 4; i++) {
				msg.Arguments[0] = i;
				args.AddRange(msg.Arguments);
			}

			//msg.Arguments = args.ToArray();

			var t = args.ToArray();

			Assert.False(setup.Deserializer.CanDeserializeRepeats<NonRepeatable>(new BasicMessage("rep", t)));
		}
	}
}
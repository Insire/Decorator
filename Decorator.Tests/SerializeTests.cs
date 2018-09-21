﻿using Decorator.Exceptions;
using System;
using System.Collections.Generic;

using Xunit;

namespace Decorator.Tests {

	public class SerializeTests {

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanSerialize")]
		public void SerializeExample() {
			Assert.Equal(new BasicMessage("test", "frog", 8), Serializer<TestMessage>.Serialize(new TestMessage {
				PositionOneItem = 8,
				PositionZeroItem = "frog"
			}));
		}

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanSerialize")]
		public void SerializeEmpty() => Assert.Equal(new BasicMessage(null), Serializer<NullType>.Serialize(new NullType()));

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanSerialize")]
		public void SerializeEnumerable() {
			var args = new List<object>();

			var msg = Setup.Correct;

			// 4 is arbitrary here
			for (var i = 0; i < 4; i++) {
				msg.Arguments[1] = i;
				args.AddRange(msg.Arguments);
			}

			var testMe = new BasicMessage("test", args.ToArray());

			Assert.Equal(testMe, Serializer<TestMessage>.Serialize(new TestMessage[] {
				new TestMessage {
					PositionZeroItem = "just right",
					PositionOneItem = 0
				},
				new TestMessage {
					PositionZeroItem = "just right",
					PositionOneItem = 1
				},
				new TestMessage {
					PositionZeroItem = "just right",
					PositionOneItem = 2
				},
				new TestMessage {
					PositionZeroItem = "just right",
					PositionOneItem = 3
				}
			}));
		}

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanSerialize")]
		public void NeedsAttributes1() {
			// technically a MissingAttributeException
			Assert.Throws<TypeInitializationException>(delegate () {
				Serializer<NeedsAttribute>.Serialize(new NeedsAttribute {
					WOWOW = "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEE no my secret comment EEEEEEEEEEEEEEEE"
				});
			});
		}

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanSerialize")]
		public void NeedsAttributes2() {
			Assert.Throws<MissingAttributeException>(delegate () {
				Serializer.Serialize<NeedsAttribute>(new NeedsAttribute {
					WOWOW = "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEE no my secret comment EEEEEEEEEEEEEEEE"
				});
			});
		}

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanSerialize")]
		public void NoProperties() {
			Assert.Equal((BaseMessage)(new BasicMessage("noprop")), Serializer.Serialize(new NoProperties()));
		}

		[Fact, Trait("Project", "Decorator.Tests")]
		[Trait("Category", "CanSerialize")]
		public void NoItems() {
			Assert.Equal((BaseMessage)(new BasicMessage("noprop")), Serializer.Serialize((IEnumerable<NoProperties>)new NoProperties[0] { }));
		}
	}
}
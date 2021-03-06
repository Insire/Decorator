﻿using BenchmarkDotNet.Attributes;

using Decorator.Attributes;

using System;

namespace Decorator.Benchmarks
{
	[Message("say")]
	public class Chat
	{
		[Position(0), Required]
		public int PlayerId { get; set; }

		[Position(1), Required]
		public string Message { get; set; }
	}

	public class Benchmarks
	{
		private readonly object[] _goodArgs = new object[] { 10, "hello world" };
		private readonly object[] _badArgsAt0 = new object[] { "10", "hello world" };
		private readonly object[] _badArgsAt1 = new object[] { 10, 3f };

		private BaseMessage _goodMsg;
		private BaseMessage _badMsgAt0;
		private BaseMessage _badMsgAt1;
		private BaseMessage _badType;

		private Chat _chat;

		private ProtocolMessage.ProtocolMessageManager _pm;

		private readonly Type _type = typeof(Decorator.Benchmarks.Chat);

		[GlobalSetup]
		public void Setup()
		{
			_pm = new ProtocolMessage.ProtocolMessageManager();
			_goodMsg = new BasicMessage("say", _goodArgs);
			_badMsgAt0 = new BasicMessage("say", _badArgsAt0);
			_badMsgAt1 = new BasicMessage("say", _badArgsAt1);
			_badType = new BasicMessage("sya", _goodArgs);
			_chat = new Chat {
				Message = "hello world",
				PlayerId = 10
			};

			// dry run, ensure caching in decorator is fine
			BasicDeserialize();

			for (var i = 0; i < 2; i++)
			{
				BasicDeserialize();
				ProtocolMessage();
				DeserializeWithType();
				InvalidChat_At0();
				InvalidChat_At1();
				InvalidChat_Type();
			}
		}

		[Benchmark(Description = "Simple TryDeserialize", Baseline = true)]
		public bool BasicDeserialize()
			=> Deserializer.TryDeserializeItem<Chat>(_goodMsg, out var _);

		[Benchmark(Description = "ProtocolMessage alternative")]
		public ProtocolMessage.Chat ProtocolMessage()
			=> _pm.Convert<ProtocolMessage.Chat>(_goodArgs);

		[Benchmark(Description = "TryDeserialize with Type")]
		public bool DeserializeWithType()
			=> Deserializer.TryDeserializeItem(_type, _goodMsg, out var _);

		[Benchmark(Description = "Invalid @ 0")]
		public bool InvalidChat_At0()
			=> Deserializer.TryDeserializeItem<Chat>(_badMsgAt0, out var _);

		[Benchmark(Description = "Invalid @ 1")]
		public bool InvalidChat_At1()
			=> Deserializer.TryDeserializeItem<Chat>(_badMsgAt1, out var _);

		[Benchmark(Description = "Invalid Type")]
		public bool InvalidChat_Type()
			=> Deserializer.TryDeserializeItem<Chat>(_badType, out var _);

		[Benchmark(Description = "Deserialize Message to Method")]
		public void InvokeMethodMessage()
			=> Deserializer<Benchmarks>.InvokeMethodFromMessage(this, _goodMsg);

		[Benchmark(Description = "Deserialize Item to Method")]
		public void InvokeMethodItem()
			=> Deserializer<Benchmarks>.InvokeMethodFromItem(this, _chat);

		[DeserializedHandler]
		public void HandleItem(Chat chat)
		{
			// Allow the deserializer to discover this method
		}
	}
}
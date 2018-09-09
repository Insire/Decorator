﻿namespace Decorator {

	public abstract class BaseMessage {
		public abstract string Type { get; }
		public abstract object[] Arguments { get; }

		public uint Count => (uint)this.Arguments.Length;
		public object this[uint index] => this.Arguments[index];
	}

	public class BasicMessage : BaseMessage {

		public BasicMessage(string type, params object[] args) {
			this.Type = type;
			this.Arguments = args;
		}

		public override string Type { get; }
		public override object[] Arguments { get; }
	}
}
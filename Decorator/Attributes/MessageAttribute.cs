﻿using System;

namespace Decorator.Attributes {

	/// <summary>
	/// Allows a class to be used as a valid Message, ready for serialization/deserialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class MessageAttribute : Attribute {

		/// <summary>Initializes a new instance of the <see cref="MessageAttribute"/> class.</summary>
		/// <param name="type">The type.</param>
		/// <autogeneratedoc />
		public MessageAttribute(string type) => this.Type = type;

		/// <summary>Gets or sets the type.</summary>
		/// <value>The type.</value>
		/// <autogeneratedoc />
		public string Type { get; set; }
	}
}
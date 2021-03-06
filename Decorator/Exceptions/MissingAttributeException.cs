﻿using System;

namespace Decorator.Exceptions
{
	/// <summary>Gets thrown whenever an attribute is missing that should be on it</summary>
	/// <seealso cref="Decorator.Exceptions.DecoratorException" />
	public sealed class MissingAttributeException : DecoratorException
	{
		public MissingAttributeException(Type attribute, Type notOn) : base($"The attribute [{attribute}] must be put on [{notOn}]")
		{
		}
	}
}
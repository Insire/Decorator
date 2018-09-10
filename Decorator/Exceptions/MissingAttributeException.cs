﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Decorator.Exceptions
{
	/// <summary>
	/// When an attribute is missing, this exception will be thrown.
	/// </summary>
	/// <seealso cref="Decorator.Exceptions.DecoratorException" />
	/// <autogeneratedoc />
	public class MissingAttributeException : DecoratorException
    {
		public MissingAttributeException(Type attribute, Type notOn) : base($"The attribute [{attribute}] must be put on [{notOn}]") {

		}
    }
}
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Decorator.Exceptions {
	/// <summary>Gets thrown whenever there are no methods in a class that belong to type T</summary>
	/// <seealso cref="Decorator.Exceptions.DecoratorException" />
	public class LackingMethodsException : DecoratorException {
		public LackingMethodsException(Type t) : base($"Unable to find any methods that are associated with [{t}]") {

		}
	}
}

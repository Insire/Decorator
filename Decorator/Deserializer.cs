﻿using Decorator.Attributes;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Decorator {

	internal interface IMatchClass {
		object InnerClass { get; }
		int MatchAmount { get; }
	}

	public static class Deserializer {

		public static T Deserialize<T>(Message msg) {
			return InternalDeserialize<T>(msg).InnerClass;
		}

		public static void DeserializeToEvent<T>(T eventClass, Message msg, params object[] additionalInfo)
					where T : class {
			// if eventClass is null, it's a probably a static method.

			var matches = new List<MatchResult>(8);

			foreach (var i in typeof(T).GetMethods()) {
				// make sure that if it's a static method, we're being passed null
				// or, make sure that if it's not a static method, we're being passed a value
				if ((i.IsStatic && eventClass == default(T)) ||
					!i.IsStatic && eventClass != default(T)) {
					var dhAttrib = (DeserializedHandlerAttribute)i.GetCustomAttribute(typeof(DeserializedHandlerAttribute), true);

					if (dhAttrib != default(DeserializedHandlerAttribute)) {
						var args = i.GetParameters();
						if (args?.Length == 1 + additionalInfo?.Length) {
							var type = args[0].ParameterType;

							if (TryDeserializeGenerically(msg, type, out var genRes)) {
								var res = (IMatchClass)genRes;

								matches.Add(new MatchResult(res, type, i));
							}
						} // else throw new CustomAttributeFormatException($"Any method with the {nameof(DeserializedHandlerAttribute)} Attribute must have the right amount of parameters.");
					}
				}
			}

			if (matches.Count > 0) {
				//TODO: make sure that the parameters match the additionalInfo
				MatchResult winner = default;
				bool duplicate = false;
				foreach (var i in matches) {
					if (winner == default(MatchResult)) {
						winner = i;
					} else if (i.MatchClass.MatchAmount > winner.MatchClass.MatchAmount) {
						duplicate = false;
						winner = i;
					} else if (i.MatchClass.MatchAmount == winner.MatchClass.MatchAmount) {
						duplicate = true;
					}
				}

				if (duplicate)
					throw new AmbiguousMatchException($"More then one canidates were found during deserialization.");

				object[] invokeEvents;

				if (additionalInfo != null) {
					invokeEvents = new object[additionalInfo.Length + 1];

					for (int i = 0; i < additionalInfo.Length; i++)
						invokeEvents[i + 1] = additionalInfo[i];
				} else invokeEvents = new object[1];

				invokeEvents[0] = Convert.ChangeType(winner.MatchClass.InnerClass, winner.Type);

				winner.MethodInfo.Invoke(eventClass, invokeEvents);
				return;
			} else {
				var help = "";
				if (eventClass == default(T))
					help = "If you intended to target a public method, ensure that the target is not null.";
				throw new NotSupportedException($"Unable to find a viable method. {help}");
			}
		}

		public static bool TryDeserialize<T>(Message msg, out T result) {
			var res = InternalTryDeserialize<T>(msg, out var test);
			result = test.InnerClass;
			return res;
		}

		private static MatchClass<T> InternalDeserialize<T>(Message msg) {
			var t = typeof(T);
			var item = (T)Activator.CreateInstance(t);

			if (msg == null) throw new ArgumentNullException(nameof(msg), "Message is null.");

			//TODO: wrap this in it's own function
			var msgAttrib = (MessageAttribute)t.GetCustomAttribute(typeof(MessageAttribute), true);
			if (msgAttrib == default(MessageAttribute)) throw new CustomAttributeFormatException($"The type {t} doesn't have a [{nameof(MessageAttribute)}] attribute modifier defined on it.");
			if (msgAttrib.Type != msg.Type) throw new ArgumentException($"The message types don't match.");

			var limiter = (ArgumentLimitAttribute)t.GetCustomAttribute(typeof(ArgumentLimitAttribute), true);
			if (limiter != default(ArgumentLimitAttribute)) if (msg.Args.Length > limiter.ArgLimit) throw new MessageException($"Too many arguments specified");

			var matchAmount = 0;

			foreach (var i in t.GetProperties()) {
				var attribs = i.GetCustomAttributes();

				var posAttrib = (PositionAttribute)i.GetCustomAttribute(typeof(PositionAttribute), true);

				if (posAttrib != default(PositionAttribute)) {
					var setPropValue = true;

					foreach (var k in attribs) {
						if (k.GetType().GetInterface(nameof(Attributes.IPropertyAttributeBase)) != default)
							setPropValue = ((IPropertyAttributeBase)k).CheckRequirements<T>(i, msg, item, posAttrib) && setPropValue;
					}

					if (setPropValue) {
						// can't determine type, so we'll just have to try set it to null and see if it fails.
						if (msg.Args[posAttrib.Position] != null)
							if (i.PropertyType != msg.Args[posAttrib.Position].GetType() &&
								!i.PropertyType.IsInstanceOfType(msg.Args[posAttrib.Position].GetType()))
								throw new MessageException($"The property type of {i.Name} doesn't match the property type of the argument at {posAttrib.Position} ({msg.Args[posAttrib.Position].GetType()})");

						matchAmount++;
						i.SetValue(item, msg.Args[posAttrib.Position]);
					}
				}
			}

			return new MatchClass<T>(item, matchAmount);
		}

		private static bool InternalTryDeserialize<T>(Message msg, out MatchClass<T> result) {
			try {
				result = InternalDeserialize<T>(msg);
				return true;
			} catch (Exception ex) when (
				ex is ArgumentException ||
				ex is ArgumentNullException ||
				ex is CustomAttributeFormatException ||
				ex is NullReferenceException ||
				ex is MessageException) {
				result = default;
				return false;
			}
		}

		private static bool TryDeserializeGenerically(Message msg, Type msgType, out object result) {
			var args = new object[] { msg, null };

			var generic = typeof(Deserializer).GetMethod(nameof(InternalTryDeserialize), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(msgType);
			var gmo = (bool)generic.Invoke(null, args);

			result = args[1];
			return gmo;
		}
	}

	internal static class ReflectionCacher {
		private static ConcurrentDictionary<Type, IEnumerable<Attribute>> AttributeCache = new ConcurrentDictionary<Type, IEnumerable<Attribute>>();
		private static ConcurrentDictionary<Type, MethodInfo> DeserializeMethodHandlers = new ConcurrentDictionary<Type, MethodInfo>();

		public static T GetAttributeOf<T>(Type t) {
			foreach (var i in GetAttributesOf(t))
				if (i is T attrib)
					return attrib;
			return default(T);
		}

		public static IEnumerable<Attribute> GetAttributesOf(Type t) {
			if (AttributeCache.TryGetValue(t, out var attribs)) return attribs;

			attribs = GetFrom(t.GetCustomAttributes(true));
			AttributeCache.TryAdd(t, attribs.ToArray());

			return attribs;
		}

		public static IEnumerable<T> GetAttributesOf<T>(Type t) {
			foreach (var i in GetAttributesOf(t))
				if (i is T attrib)
					yield return attrib;
		}

		private static IEnumerable<Attribute> GetFrom(object[] attribs) {
			foreach (var i in attribs)
				if (i is Attribute attrib)
					yield return attrib;
		}
	}
	internal class MatchClass<T> : IMatchClass {

		public MatchClass(T @class, int matchAmount) {
			this.InnerClass = @class;
			this.MatchAmount = matchAmount;
		}

		public T InnerClass { get; }
		object IMatchClass.InnerClass => this.InnerClass;
		public int MatchAmount { get; set; }
		int IMatchClass.MatchAmount => this.MatchAmount;
	}
}
﻿using Decorator.Exceptions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Decorator {

	/// <summary>
	/// Deserializes any message to a type.
	/// </summary>
	/// <typeparam name="TClass">The type of the class.</typeparam>
	/// <autogeneratedoc />
	public static class Deserializer {
		static Deserializer() => TypeManager = new MessageManager();

		public static MessageManager TypeManager { get; }

		public static bool CanDeserialize<TItem>(BaseMessage m)
			=> CanDeserialize(typeof(TItem), m);

		public static bool CanDeserializeRepeats<TItem>(BaseMessage m)
			=> CanDeserializeRepeats(typeof(TItem), m);

		public static TItem Deserialize<TItem>(BaseMessage m)
					where TItem : new()
			=> (TItem)Deserialize(typeof(TItem), m);

		public static IEnumerable<TItem> DeserializeRepeats<TItem>(BaseMessage m) where TItem : new()
			=> FromObj<TItem>(DeserializeRepeats(typeof(TItem), m)).ToArray();

		public static bool TryDeserialize<TItem>(BaseMessage m, out TItem result) {
			var pass = TryDeserialize(typeof(TItem), m, out var tryDesRes);

			result = pass ?
				(TItem)tryDesRes
				: default;

			return pass;
		}

		public static bool TryDeserializeRepeats<TItem>(BaseMessage m, out IEnumerable<TItem> result) {
			var pass = TryDeserializeRepeats(typeof(TItem), m, out var tryDesRes);

			result = pass ?
				FromObj<TItem>(tryDesRes)
				: default;

			return pass;
		}

		public static bool CanDeserialize(Type t, BaseMessage m)
			=> TypeManager.QualifiesAsType(t, m);

		public static bool CanDeserializeRepeats(Type t, BaseMessage m)
			=> TypeManager.QualifiesAsRepeatableType(t, m);

		public static object Deserialize(Type t, BaseMessage m) {
			if (CanDeserialize(t, m))
				return TypeManager.DeserializeToType(t, m);

			throw new InvalidDeserializationAttemptException();
		}

		public static IEnumerable<object> DeserializeRepeats(Type t, BaseMessage m) {
			if (TypeManager.QualifiesAsRepeatableType(t, m))
				return TypeManager.DeserializeRepeatableToType(t, m).ToArray();

			throw new InvalidDeserializationAttemptException();
		}

		public static bool TryDeserialize(Type t, BaseMessage m, out object result) {
			if (TypeManager.QualifiesAsType(t, m)) {
				result = TypeManager.DeserializeToType(t, m);
				return true;
			}
			result = default;
			return false;
		}

		public static bool TryDeserializeRepeats(Type t, BaseMessage m, out IEnumerable<object> result) {
			if (TypeManager.QualifiesAsRepeatableType(t, m)) {
				result = TypeManager.DeserializeRepeatableToType(t, m).ToArray();
				return true;
			}
			result = default;
			return false;
		}

		private static IEnumerable<T> FromObj<T>(IEnumerable<object> objs) {
			foreach (var i in objs)
				yield return (T)i;
		}
	}

	/// <summary>Deserializes any message to a method in the TClass</summary>
	/// <typeparam name="TClass">The type of the class.</typeparam>
	public static class Deserializer<TClass>
		where TClass : class {

		static Deserializer() {
			MethodDeserializerManager = new MethodDeserializerManager<TClass>();
			_objToArrays = new Cache<Type, Func<object, object[], object>>();
			_objToArray = typeof(Deserializer<TClass>)
								.GetMethod(nameof(FromObjToArray), BindingFlags.Static | BindingFlags.NonPublic);
		}

		private static readonly MethodInfo _objToArray;

		private static readonly Cache<Type, Func<object, object[], object>> _objToArrays;

		public static MethodDeserializerManager<TClass> MethodDeserializerManager { get; }

		#region imitate Deserializer
		public static bool CanDeserialize<TItem>(BaseMessage m)
			=> Deserializer.CanDeserialize<TItem>(m);

		public static bool CanDeserializeRepeats<TItem>(BaseMessage m)
			=> Deserializer.CanDeserializeRepeats<TItem>(m);

		public static TItem Deserialize<TItem>(BaseMessage m)
					where TItem : new()
			=> Deserializer.Deserialize<TItem>(m);

		public static IEnumerable<TItem> DeserializeRepeats<TItem>(BaseMessage m) where TItem : new()
			=> Deserializer.DeserializeRepeats<TItem>(m);

		public static bool TryDeserialize<TItem>(BaseMessage m, out TItem result)
			=> Deserializer.TryDeserialize<TItem>(m, out result);

		public static bool TryDeserializeRepeats<TItem>(BaseMessage m, out IEnumerable<TItem> result)
			=> Deserializer.TryDeserializeRepeats<TItem>(m, out result);

		public static bool CanDeserialize(Type t, BaseMessage m)
			=> Deserializer.CanDeserialize(t, m);

		public static bool CanDeserializeRepeats(Type t, BaseMessage m)
			=> Deserializer.CanDeserializeRepeats(t, m);

		public static object Deserialize(Type t, BaseMessage m)
			=> Deserializer.Deserialize(t, m);

		public static IEnumerable<object> DeserializeRepeats(Type t, BaseMessage m)
			=> Deserializer.DeserializeRepeats(t, m);

		public static bool TryDeserialize(Type t, BaseMessage m, out object result)
			=> Deserializer.TryDeserialize(t, m, out result);

		public static bool TryDeserializeRepeats(Type t, BaseMessage m, out IEnumerable<object> result)
			=> Deserializer.TryDeserializeRepeats(t, m, out result);
		#endregion

		public static void DeserializeItemToMethod<TItem>(TClass instance, TItem item) {
			foreach (var i in MethodDeserializerManager.GetMethodsFor<TItem>()) {
				MethodDeserializerManager.InvokeMethod<TItem>(i, instance, item);
			}
		}

		public static void DeserializeMessageToMethod(TClass instance, BaseMessage msg) {
			foreach (var i in MethodDeserializerManager.Cache) {
				if (Deserializer.TypeManager.QualifiesAsType(i.Key, msg)) {
					var des = Deserializer.TypeManager.DeserializeToType(i.Key, msg);

					foreach (var k in i.Value)
						MethodDeserializerManager.InvokeMethod(k, instance, des);
				}

					// if it works as whatevever the key is, it ***certainly*** won't work as an IEnuemrable<>
					else

					if (i.Key.GenericTypeArguments.Length > 0) {
					var genArg = i.Key.GenericTypeArguments[0];

					if (typeof(IEnumerable).IsAssignableFrom(i.Key) &&
						Deserializer.TypeManager.QualifiesAsRepeatableType(genArg, msg)) {
						var des = Deserializer.TypeManager.DeserializeRepeatableToType(genArg, msg);

						var result = _objToArrays.Retrieve(genArg, () =>
							IL.Wrap(_objToArray.MakeGenericMethod(genArg)))
							(null, new[] { des });

						foreach (var k in i.Value)
							MethodDeserializerManager.InvokeMethod(k, (object)instance, result);
					}
				}
			}
		}

		private static IEnumerable<T> FromObjToArray<T>(IEnumerable<object> objs)
			=> FromObj<T>(objs).ToArray();

		private static IEnumerable<T> FromObj<T>(IEnumerable<object> objs) {
			foreach (var i in objs)
				yield return (T)i;
		}
	}
}
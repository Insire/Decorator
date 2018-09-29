﻿using Decorator.Helpers;

using System;
using System.Reflection;

namespace Decorator
{
	/// <summary>
	/// Deserializes any message to a type.
	/// </summary>
	/// <typeparam name="TClass">The type of the class.</typeparam>
	/// <autogeneratedoc />
	public static class Deserializer
	{
		private static FunctionWrapper _tryDeserialize;
		private static FunctionWrapper _tryDeserializeRepeatable;

		static Deserializer()
		{
			var methods = typeof(Deserializer)
							.GetMethods();

			var genericTType = typeof(object).MakeByRefType();
			var genericIEnumerableTType = typeof(object[]).MakeByRefType();

			// TODO: put the 'foreach' precursor stuff in a function, not the HandleParam stuff

			foreach (var method in methods)
				if (method.IsGenericMethodDefinition)
					foreach (var parameter in method.MakeGenericMethod(typeof(object))
										.GetParameters())
						HandleParam(genericTType, genericIEnumerableTType, method, parameter);
		}

		private static void HandleParam(Type genericTType, Type genericIEnumerableTType, MethodInfo method, ParameterInfo parameter)
		{
			if (parameter.IsOut)
				if (parameter.ParameterType == genericTType)
					_tryDeserialize = new FunctionWrapper(method);
				else if (parameter.ParameterType == genericIEnumerableTType)
					_tryDeserializeRepeatable = new FunctionWrapper(method);
		}

		/// <summary>Attempts to deserialize the <see cref="BaseMessage" /><paramref name="m" /> to a <typeparamref name="TItem" /></summary>
		/// <typeparam name="TItem">The message class type</typeparam>
		/// <param name="m">The message to deserialize into a <typeparamref name="TItem" /></param>
		/// <param name="result">The result of the deserialization</param>
		/// <example><code>
		/// [Message("12o")]
		/// public class Oatmeal
		/// {
		///		[Position(0), Required]
		///		public int One { get; set; }
		///
		///		[Position(1), Required]
		///		public int Two { get; set; }
		///
		///		[Position(2), Required]
		///		public string Oatmeal { get; set; }
		/// }
		///
		/// var result = Deserializer.TryDeserializeItem<Oatmeal>(new BasicMessage("12o", 1, 2, "oatmeal", out var oatmeal);
		///
		/// if (result)
		/// {
		///		Console.WriteLine($"{oatmeal.One}, {oatmeal.Two}, {oatmeal.Oatmeal}\nKirby is a pink guy");
		/// }
		///
		/// // should output:
		///
		/// // 1, 2, oatmeal
		/// // Kirby is a pink guy
		/// </code></example>
		/// <returns>
		/// <para><c>true</c> if it was able to deserialize <paramref name="m" /> into a <typeparamref name="TItem" />, with <paramref name="result" /> containing the valid result, or</para>
		/// <para><c>false</c> if it was unable to deserialize <paramref name="m" /> into a <typeparamref name="TItem" /></para></returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="m> <c>is</c> <c>null</c></exception>
		/// <seealso cref="TryDeserializeItem(Type, BaseMessage, out object)"/>
		public static bool TryDeserializeItem<TItem>(BaseMessage m, out TItem result)
		{
			if (m is null) throw new ArgumentNullException(nameof(m));

			var def = MessageManager.GetDefinitionFor<TItem>();

			if (def is null ||
				!EnsureAttributesOn(m, def) ||
				m.Count != def.MaxCount) return TryMethodHelpers.EndTryMethod(false, default, out result);

			return TryDeserializeValue<TItem>(m, def, out result);
		}

		/// <summary>
		/// Attempts to deserialize the <paramref name="m"/> to a <see cref="IEnumerable{TItem}"/>, and returns whether or not it can.
		/// </summary>
		/// <typeparam name="TItem">The type of the item.</typeparam>
		/// <param name="m">The message.</param>
		/// <param name="result">The result after deserialization</param>
		/// <example><code>
		/// [Message("example"), Repeatable]
		/// public class TestyClass
		/// {
		///		[Position(0), Required]
		///		public int Id { get; set; }
		///
		///		[Position(2), Required]
		///		public string Name { get; set; }
		/// }
		///
		/// var result = Deserializer.TryDeserializeItems<TestyClass>(new BasicMessage("example", 0, "John", 1, "Mac", 2, "Mark", 3, "Zuccy", 4, "Johnny"), out var items);
		///
		/// if (result)
		/// {
		///		foreach (var i in items)
		///		{
		///			Console.WriteLine($"{i.Id} - {i.Name}");
		///		}
		/// }
		///
		/// // should output:
		/// // 0 - John
		/// // 1 - Mac
		/// // 2 - Mark
		/// // 3 - Zuccy
		/// // 4 - Johnny
		/// </code></example>
		/// <returns><c>true</c> if it can deserialize it, <c>false</c> if it can't</returns>
		public static bool TryDeserializeItems<TItem>(BaseMessage m, out TItem[] result)
		{
			if (m is null) throw new ArgumentNullException(nameof(m));

			var def = MessageManager.GetDefinitionFor<TItem>();

			if (def is null ||
				!EnsureAttributesOn(m, def) ||
				!def.Repeatable) return TryMethodHelpers.EndTryMethod(false, default, out result);

			return TryDeserializeValues<TItem>(m, def, out result);
		}

		#region reflectionified

		/// <summary>
		/// Invokes <seealso cref="TryDeserializeItem{TItem}(BaseMessage, out TItem)"/> by using <seealso cref="System.Type"/> <paramref name="t"/> as the generic argument.
		/// </summary>
		/// <see cref="TryDeserializeItem{TItem}(BaseMessage, out TItem)"/>
		public static bool TryDeserializeItem(Type t, BaseMessage m, out object result)
		{
			if (t is null) throw new ArgumentNullException(nameof(t));
			if (m is null) throw new ArgumentNullException(nameof(m));

			var args = new object[] { m, null };

			var method = _tryDeserialize.GetMethodFor(t);

			if (!(bool)(method(null, args))) return TryMethodHelpers.EndTryMethod(false, default, out result);

			return TryMethodHelpers.EndTryMethod(true, args[1], out result);
		}

		/// <summary>
		/// Invokes <seealso cref="TryDeserializeItems{TItem}(BaseMessage, out TItem[])"/>
		/// </summary>
		/// <param name="t"></param>
		/// <param name="m"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryDeserializeItems(Type t, BaseMessage m, out object[] result)
		{
			if (t is null) throw new ArgumentNullException(nameof(t));
			if (m is null) throw new ArgumentNullException(nameof(m));

			var args = new object[] { m, null };

			if (!((bool)_tryDeserializeRepeatable.GetMethodFor(t)(null, args))) return TryMethodHelpers.EndTryMethod(false, default, out result);

			return TryMethodHelpers.EndTryMethod(true, (object[])args[1], out result);
		}

		#endregion reflectionified

		private static bool TryDeserializeValue<T>(BaseMessage m, MessageDefinition def, out T result)
		{
			// prevent boxing calls
			var instance = (object)InstanceOf<T>.Create();

			foreach (var i in def.Properties)
				if (PropertyQualifies(i, m)) i.Set(instance, m.Arguments[i.IntPos]);
				else if (i.State != TypeRequiredness.Optional) return TryMethodHelpers.EndTryMethod(false, default, out result);

			return TryMethodHelpers.EndTryMethod(true, (T)instance, out result);
		}

		private static bool TryDeserializeValues<T>(BaseMessage m, MessageDefinition def, out T[] result)
		{
			var max = m.Count / def.IntMaxCount;

			var itms = new T[max];

			for (var i = 0; i < max; i++)
			{
				var messageItems = new object[def.IntMaxCount];

				Array.Copy(m.Arguments, i * def.IntMaxCount, messageItems, 0, def.IntMaxCount);

				if (!TryDeserializeValue<T>(new BasicMessage(null, messageItems), def, out var item)) return TryMethodHelpers.EndTryMethod(false, default, out result);

				itms[i] = item;
			}

			return TryMethodHelpers.EndTryMethod(true, itms, out result);
		}

		private static bool PropertyQualifies(MessageProperty prop, BaseMessage m)
		{
			if (!(m.Arguments.Length > prop.IntPos)) return false;

			var item = m.Arguments[prop.IntPos];
			if (item is null) return false;

			return prop.Type == item.GetType();
		}

		private static bool EnsureAttributesOn(BaseMessage m, MessageDefinition def)
			=> m.Type == def.Type;
	}
}
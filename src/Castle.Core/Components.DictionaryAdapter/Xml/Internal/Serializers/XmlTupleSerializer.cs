// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.f
// See the License for the specific language governing permissions and
// limitations under the License.

#if DOTNET40
namespace Castle.Components.DictionaryAdapter.Xml
{
	using System;
	using System.Linq.Expressions;

	// HACK: I never originally intended for serializers to contain accessors.
	public class XmlTupleSerializer : XmlTypeSerializer
	{
		private readonly Type                     type;
		private readonly Type[]                   itemTypes;
		private readonly Func<object[], object  > entupler;
		private readonly Func<object,   object[]> detupler;

		public XmlTupleSerializer(Type type)
		{
			if (type == null)
				throw Error.ArgumentNull("type");
			if (!IsTuple(type))
				throw Error.ArgumentOutOfRange("type");

			this.type = type;
			itemTypes = type.GetGenericArguments();
			entupler  = CreateEntupler(type, itemTypes);
			detupler  = CreateDetupler(type, itemTypes);
		}

		public override XmlTypeKind Kind
		{
			get { return XmlTypeKind.Complex; }
		}

		public override object GetValue(IXmlNode node, IDictionaryAdapter parent, IXmlAccessor accessor)
		{
			var items      = new object[itemTypes.Length];
			var accessors  = GetItemAccessors(parent, accessor);
			var references = XmlAdapter.For(parent).References;

			for (var i = 0; i < accessors.Length; i++)
				items[i] = accessors[i].GetPropertyValue(node, parent, references, true);

			return entupler(items);
		}

		public override void SetValue(IXmlNode node, IDictionaryAdapter parent, IXmlAccessor accessor, object oldValue, ref object value)
		{
			var originalItems = (oldValue != null) ? detupler(oldValue) : new object[itemTypes.Length];
			var providedItems = detupler(value);
			var assignedItems = null as object[];
			var accessors     = GetItemAccessors(parent, accessor);
			var references    = XmlAdapter.For(parent).References;

			for (var i = 0; i < accessors.Length; i++)
			{
				var originalItem = originalItems[i];
				var providedItem = providedItems[i];
				var assignedItem = providedItem;

				accessors[i].SetPropertyValue(node, parent, references, originalItem, ref assignedItem);

				if (assignedItems != null)
				{
					assignedItems[i] = assignedItem;
				}
				else if (!Equals(assignedItem, providedItem))
				{
					assignedItems = new object[accessors.Length];
					Array.Copy(providedItems, assignedItems, i);
					assignedItems[i] = assignedItem;
				}
			}

			if (assignedItems != null)
				value = assignedItems;
		}

		private static Func<object, object[]> CreateDetupler(Type tupleType, Type[] itemTypes)
		{
			var value = Expression.Parameter(typeof(object), "value");
			var tuple = Expression.Variable (tupleType,      "tuple");
			var args  = new Expression[itemTypes.Length];

			for (var i = 0; i < itemTypes.Length; i++)
				args[i] = Expression.TypeAs(
					Expression.Property(tuple, "Item" + (i + 1).ToString()),
					typeof(object));

			return Expression.Lambda<Func<object, object[]>>
			(
				Expression.Block(
					new[] { tuple },
					Expression.Assign(tuple, Expression.Convert(value, tupleType)),
					Expression.NewArrayInit(typeof(object), args)),
				value
			)
			.Compile();
		}

		private Func<object[], object> CreateEntupler(Type tupleType, Type[] itemTypes)
		{
			var items = Expression.Parameter(typeof(object[]), "items");
			var args  = new Expression[itemTypes.Length];

			for (var i = 0; i < itemTypes.Length; i++)
				args[i] = Expression.Convert(
					Expression.ArrayIndex(items, Expression.Constant(i)),
					itemTypes[i]);

			return Expression
				.Lambda<Func<object[], object>>(
					Expression.New(tupleType.GetConstructor(itemTypes), args),
					items)
				.Compile();
		}

		private static bool IsTuple(Type type)
		{
			return type.Namespace == "System"
				&& type.Name.StartsWith("Tuple`")
				&& type.IsGenericType
				&& type.IsGenericTypeDefinition == false;
		}

		private ItemAccessor[] GetItemAccessors(IDictionaryAdapter parent, IXmlAccessor accessor)
		{
			var cache = parent.Meta.ExtendedProperties;
			ItemAccessor[] accessors;

			if (cache.Contains(accessor))
			{
				accessors = cache[accessor] as ItemAccessor[];

				if (accessors != null && accessors.Length == itemTypes.Length)
					return accessors;
			}

			accessors = new ItemAccessor[itemTypes.Length];

			for (var i = 0; i < itemTypes.Length; i++)
				accessors[i] = new ItemAccessor(i, itemTypes[i], accessor);

			cache[accessor] = accessors;
			return accessors;
		}

		private sealed class ItemAccessor : XmlNodeAccessor
		{
			public ItemAccessor(int index, Type type, IXmlAccessor parent)
				: base(GetName(index), type, parent.Context)
			{
				ConfigureNillable (true);
				ConfigureReference(parent.IsReference);
			}

			private static string GetName(int index)
			{
				return "Item" + (index + 1).ToString();
			}

			public override IXmlCursor SelectPropertyNode(IXmlNode parentNode, bool mutable)
			{
				return parentNode.SelectChildren(KnownTypes, Context, CursorFlags.Elements.MutableIf(mutable));
			}
		}
	}
}
#endif

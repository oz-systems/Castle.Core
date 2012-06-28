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
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Components.DictionaryAdapter.Xml
{
	using System.Xml;
	using System.Xml.XPath;

	internal struct CompiledXPathWriter
	{
		private readonly XmlWriter      writer;
		private readonly XPathNavigator evaluator;

		public CompiledXPathWriter(XmlWriter writer, XPathNavigator evaluator)
		{
			this.writer    = writer;
			this.evaluator = evaluator;
		}

		public void WriteNode(CompiledXPathNode node)
		{
			if (node.IsAttribute)
				WriteAttribute(node);
			else if (node.IsSimple)
				WriteSimpleElement(node);
			else
				WriteComplexElement(node);
		}

		private void WriteAttribute(CompiledXPathNode node)
		{
			writer.WriteStartAttribute(node.Prefix, node.LocalName, null);
			WriteValue(node);
			writer.WriteEndAttribute();
		}

		private void WriteSimpleElement(CompiledXPathNode node)
		{
			writer.WriteStartElement(node.Prefix, node.LocalName, null);
			WriteValue(node);
			writer.WriteEndElement();
		}

		private void WriteComplexElement(CompiledXPathNode node)
		{
			writer.WriteStartElement(node.Prefix, node.LocalName, null);
			WriteSubnodes(node, true);
			WriteSubnodes(node, false);
			writer.WriteEndElement();
		}

		private void WriteSubnodes(CompiledXPathNode parent, bool attributes)
		{
			var next = parent.NextNode;
			if (next != null && next.IsAttribute == attributes)
				WriteNode(next);

			foreach (var node in parent.Dependencies)
				if (node.IsAttribute == attributes)
					WriteNode(node);
		}

		private void WriteValue(CompiledXPathNode node)
		{
			if (node.Value == null)
				return;

			var value = evaluator.Evaluate(node.Value);
			writer.WriteValue(value);
		}
	}
}

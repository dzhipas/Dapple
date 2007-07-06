//
// OnlineResourceType.cs
//
// This file was generated by XMLSpy 2007r3 Enterprise Edition.
//
// YOU SHOULD NOT MODIFY THIS FILE, BECAUSE IT WILL BE
// OVERWRITTEN WHEN YOU RE-RUN CODE GENERATION.
//
// Refer to the XMLSpy Documentation for further details.
// http://www.altova.com/xmlspy
//


using System;
using System.Collections;
using System.Xml;
using Altova.Types;

namespace WMS_MS_Capabilities
{
	public class OnlineResourceType : Altova.Xml.Node
	{
		#region Documentation
		public static string GetAnnoDocumentation() { return ""; }
		#endregion

		#region Forward constructors

		public OnlineResourceType() : base() { SetCollectionParents(); }

		public OnlineResourceType(XmlDocument doc) : base(doc) { SetCollectionParents(); }
		public OnlineResourceType(XmlNode node) : base(node) { SetCollectionParents(); }
		public OnlineResourceType(Altova.Xml.Node node) : base(node) { SetCollectionParents(); }
		public OnlineResourceType(Altova.Xml.Document doc, string namespaceURI, string prefix, string name) : base(doc, namespaceURI, prefix, name) { SetCollectionParents(); }
		#endregion // Forward constructors

		public override void AdjustPrefix()
		{

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Attribute, "", "xmlns:xlink" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Attribute, "", "xmlns:xlink", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Attribute, "", "xlink:type" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Attribute, "", "xlink:type", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Attribute, "", "xlink:href" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Attribute, "", "xlink:href", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
			}
		}

		public void SetXsiType()
		{
 			XmlElement el = (XmlElement) domNode;
			el.SetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", "OnlineResource");
		}


		#region xmlns_xlink Documentation
		public static string Getxmlns_xlinkAnnoDocumentation()
		{
			return "";		
		}
		public static string Getxmlns_xlinkDefault()
		{
			return "";		
		}
		#endregion

		#region xmlns_xlink accessor methods
		public static int Getxmlns_xlinkMinCount()
		{
			return 0;
		}

		public static int xmlns_xlinkMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int Getxmlns_xlinkMaxCount()
		{
			return 1;
		}

		public static int xmlns_xlinkMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int Getxmlns_xlinkCount()
		{
			return DomChildCount(NodeType.Attribute, "", "xmlns:xlink");
		}

		public int xmlns_xlinkCount
		{
			get
			{
				return DomChildCount(NodeType.Attribute, "", "xmlns:xlink");
			}
		}

		public bool Hasxmlns_xlink()
		{
			return HasDomChild(NodeType.Attribute, "", "xmlns:xlink");
		}

		public SchemaString Newxmlns_xlink()
		{
			return new SchemaString();
		}

		public SchemaString Getxmlns_xlinkAt(int index)
		{
			return new SchemaString(GetDomNodeValue(GetDomChildAt(NodeType.Attribute, "", "xmlns:xlink", index)));
		}

		public XmlNode GetStartingxmlns_xlinkCursor()
		{
			return GetDomFirstChild( NodeType.Attribute, "", "xmlns:xlink" );
		}

		public XmlNode GetAdvancedxmlns_xlinkCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Attribute, "", "xmlns:xlink", curNode );
		}

		public SchemaString Getxmlns_xlinkValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new SchemaString( curNode.Value );
		}


		public SchemaString Getxmlns_xlink()
		{
			return Getxmlns_xlinkAt(0);
		}

		public SchemaString xmlns_xlink
		{
			get
			{
				return Getxmlns_xlinkAt(0);
			}
		}

		public void Removexmlns_xlinkAt(int index)
		{
			RemoveDomChildAt(NodeType.Attribute, "", "xmlns:xlink", index);
		}

		public void Removexmlns_xlink()
		{
			Removexmlns_xlinkAt(0);
		}

		public XmlNode Addxmlns_xlink(SchemaString newValue)
		{
			if( newValue.IsNull() == false )
				return AppendDomChild(NodeType.Attribute, "", "xmlns:xlink", newValue.ToString());
			return null;
		}

		public void Insertxmlns_xlinkAt(SchemaString newValue, int index)
		{
			if( newValue.IsNull() == false )
				InsertDomChildAt(NodeType.Attribute, "", "xmlns:xlink", index, newValue.ToString());
		}

		public void Replacexmlns_xlinkAt(SchemaString newValue, int index)
		{
			ReplaceDomChildAt(NodeType.Attribute, "", "xmlns:xlink", index, newValue.ToString());
		}
		#endregion // xmlns_xlink accessor methods

		#region xmlns_xlink collection
        public xmlns_xlinkCollection	Myxmlns_xlinks = new xmlns_xlinkCollection( );

        public class xmlns_xlinkCollection: IEnumerable
        {
            OnlineResourceType parent;
            public OnlineResourceType Parent
			{
				set
				{
					parent = value;
				}
			}
			public xmlns_xlinkEnumerator GetEnumerator() 
			{
				return new xmlns_xlinkEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class xmlns_xlinkEnumerator: IEnumerator 
        {
			int nIndex;
			OnlineResourceType parent;
			public xmlns_xlinkEnumerator(OnlineResourceType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.xmlns_xlinkCount );
			}
			public SchemaString  Current 
			{
				get 
				{
					return(parent.Getxmlns_xlinkAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // xmlns_xlink collection

		#region xlink_type Documentation
		public static string Getxlink_typeAnnoDocumentation()
		{
			return "";		
		}
		public static string Getxlink_typeDefault()
		{
			return "";		
		}
		#endregion

		#region xlink_type accessor methods
		public static int Getxlink_typeMinCount()
		{
			return 0;
		}

		public static int xlink_typeMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int Getxlink_typeMaxCount()
		{
			return 1;
		}

		public static int xlink_typeMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int Getxlink_typeCount()
		{
			return DomChildCount(NodeType.Attribute, "", "xlink:type");
		}

		public int xlink_typeCount
		{
			get
			{
				return DomChildCount(NodeType.Attribute, "", "xlink:type");
			}
		}

		public bool Hasxlink_type()
		{
			return HasDomChild(NodeType.Attribute, "", "xlink:type");
		}

		public SchemaString Newxlink_type()
		{
			return new SchemaString();
		}

		public SchemaString Getxlink_typeAt(int index)
		{
			return new SchemaString(GetDomNodeValue(GetDomChildAt(NodeType.Attribute, "", "xlink:type", index)));
		}

		public XmlNode GetStartingxlink_typeCursor()
		{
			return GetDomFirstChild( NodeType.Attribute, "", "xlink:type" );
		}

		public XmlNode GetAdvancedxlink_typeCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Attribute, "", "xlink:type", curNode );
		}

		public SchemaString Getxlink_typeValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new SchemaString( curNode.Value );
		}


		public SchemaString Getxlink_type()
		{
			return Getxlink_typeAt(0);
		}

		public SchemaString xlink_type
		{
			get
			{
				return Getxlink_typeAt(0);
			}
		}

		public void Removexlink_typeAt(int index)
		{
			RemoveDomChildAt(NodeType.Attribute, "", "xlink:type", index);
		}

		public void Removexlink_type()
		{
			Removexlink_typeAt(0);
		}

		public XmlNode Addxlink_type(SchemaString newValue)
		{
			if( newValue.IsNull() == false )
				return AppendDomChild(NodeType.Attribute, "", "xlink:type", newValue.ToString());
			return null;
		}

		public void Insertxlink_typeAt(SchemaString newValue, int index)
		{
			if( newValue.IsNull() == false )
				InsertDomChildAt(NodeType.Attribute, "", "xlink:type", index, newValue.ToString());
		}

		public void Replacexlink_typeAt(SchemaString newValue, int index)
		{
			ReplaceDomChildAt(NodeType.Attribute, "", "xlink:type", index, newValue.ToString());
		}
		#endregion // xlink_type accessor methods

		#region xlink_type collection
        public xlink_typeCollection	Myxlink_types = new xlink_typeCollection( );

        public class xlink_typeCollection: IEnumerable
        {
            OnlineResourceType parent;
            public OnlineResourceType Parent
			{
				set
				{
					parent = value;
				}
			}
			public xlink_typeEnumerator GetEnumerator() 
			{
				return new xlink_typeEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class xlink_typeEnumerator: IEnumerator 
        {
			int nIndex;
			OnlineResourceType parent;
			public xlink_typeEnumerator(OnlineResourceType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.xlink_typeCount );
			}
			public SchemaString  Current 
			{
				get 
				{
					return(parent.Getxlink_typeAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // xlink_type collection

		#region xlink_href Documentation
		public static string Getxlink_hrefAnnoDocumentation()
		{
			return "";		
		}
		public static string Getxlink_hrefDefault()
		{
			return "";		
		}
		#endregion

		#region xlink_href accessor methods
		public static int Getxlink_hrefMinCount()
		{
			return 1;
		}

		public static int xlink_hrefMinCount
		{
			get
			{
				return 1;
			}
		}

		public static int Getxlink_hrefMaxCount()
		{
			return 1;
		}

		public static int xlink_hrefMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int Getxlink_hrefCount()
		{
			return DomChildCount(NodeType.Attribute, "", "xlink:href");
		}

		public int xlink_hrefCount
		{
			get
			{
				return DomChildCount(NodeType.Attribute, "", "xlink:href");
			}
		}

		public bool Hasxlink_href()
		{
			return HasDomChild(NodeType.Attribute, "", "xlink:href");
		}

		public SchemaString Newxlink_href()
		{
			return new SchemaString();
		}

		public SchemaString Getxlink_hrefAt(int index)
		{
			return new SchemaString(GetDomNodeValue(GetDomChildAt(NodeType.Attribute, "", "xlink:href", index)));
		}

		public XmlNode GetStartingxlink_hrefCursor()
		{
			return GetDomFirstChild( NodeType.Attribute, "", "xlink:href" );
		}

		public XmlNode GetAdvancedxlink_hrefCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Attribute, "", "xlink:href", curNode );
		}

		public SchemaString Getxlink_hrefValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new SchemaString( curNode.Value );
		}


		public SchemaString Getxlink_href()
		{
			return Getxlink_hrefAt(0);
		}

		public SchemaString xlink_href
		{
			get
			{
				return Getxlink_hrefAt(0);
			}
		}

		public void Removexlink_hrefAt(int index)
		{
			RemoveDomChildAt(NodeType.Attribute, "", "xlink:href", index);
		}

		public void Removexlink_href()
		{
			Removexlink_hrefAt(0);
		}

		public XmlNode Addxlink_href(SchemaString newValue)
		{
			if( newValue.IsNull() == false )
				return AppendDomChild(NodeType.Attribute, "", "xlink:href", newValue.ToString());
			return null;
		}

		public void Insertxlink_hrefAt(SchemaString newValue, int index)
		{
			if( newValue.IsNull() == false )
				InsertDomChildAt(NodeType.Attribute, "", "xlink:href", index, newValue.ToString());
		}

		public void Replacexlink_hrefAt(SchemaString newValue, int index)
		{
			ReplaceDomChildAt(NodeType.Attribute, "", "xlink:href", index, newValue.ToString());
		}
		#endregion // xlink_href accessor methods

		#region xlink_href collection
        public xlink_hrefCollection	Myxlink_hrefs = new xlink_hrefCollection( );

        public class xlink_hrefCollection: IEnumerable
        {
            OnlineResourceType parent;
            public OnlineResourceType Parent
			{
				set
				{
					parent = value;
				}
			}
			public xlink_hrefEnumerator GetEnumerator() 
			{
				return new xlink_hrefEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class xlink_hrefEnumerator: IEnumerator 
        {
			int nIndex;
			OnlineResourceType parent;
			public xlink_hrefEnumerator(OnlineResourceType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.xlink_hrefCount );
			}
			public SchemaString  Current 
			{
				get 
				{
					return(parent.Getxlink_hrefAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // xlink_href collection

        private void SetCollectionParents()
        {
            Myxmlns_xlinks.Parent = this; 
            Myxlink_types.Parent = this; 
            Myxlink_hrefs.Parent = this; 
	}
}
}
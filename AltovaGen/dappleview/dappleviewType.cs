//
// dappleviewType.cs
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

namespace dappleview
{
	public class dappleviewType : Altova.Xml.Node
	{
		#region Documentation
		public static string GetAnnoDocumentation() { return ""; }
		#endregion

		#region Forward constructors

		public dappleviewType() : base() { SetCollectionParents(); }

		public dappleviewType(XmlDocument doc) : base(doc) { SetCollectionParents(); }
		public dappleviewType(XmlNode node) : base(node) { SetCollectionParents(); }
		public dappleviewType(Altova.Xml.Node node) : base(node) { SetCollectionParents(); }
		public dappleviewType(Altova.Xml.Document doc, string namespaceURI, string prefix, string name) : base(doc, namespaceURI, prefix, name) { SetCollectionParents(); }
		#endregion // Forward constructors

		public override void AdjustPrefix()
		{

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Attribute, "", "favouriteserverurl" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Attribute, "", "favouriteserverurl", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Attribute, "", "showbluemarble" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Attribute, "", "showbluemarble", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "servers" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "servers", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, true);
				new serversType(DOMNode).AdjustPrefix();
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "activelayers" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "activelayers", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, true);
				new activelayersType(DOMNode).AdjustPrefix();
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "cameraorientation" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "cameraorientation", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, true);
				new cameraorientationType(DOMNode).AdjustPrefix();
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "notes" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "notes", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, true);
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "preview" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "preview", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, true);
			}
		}

		public void SetXsiType()
		{
 			XmlElement el = (XmlElement) domNode;
			el.SetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", "dappleview");
		}


		#region favouriteserverurl Documentation
		public static string GetfavouriteserverurlAnnoDocumentation()
		{
			return "";		
		}
		public static string GetfavouriteserverurlDefault()
		{
			return "";		
		}
		#endregion

		#region favouriteserverurl accessor methods
		public static int GetfavouriteserverurlMinCount()
		{
			return 0;
		}

		public static int favouriteserverurlMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetfavouriteserverurlMaxCount()
		{
			return 1;
		}

		public static int favouriteserverurlMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetfavouriteserverurlCount()
		{
			return DomChildCount(NodeType.Attribute, "", "favouriteserverurl");
		}

		public int favouriteserverurlCount
		{
			get
			{
				return DomChildCount(NodeType.Attribute, "", "favouriteserverurl");
			}
		}

		public bool Hasfavouriteserverurl()
		{
			return HasDomChild(NodeType.Attribute, "", "favouriteserverurl");
		}

		public SchemaString Newfavouriteserverurl()
		{
			return new SchemaString();
		}

		public SchemaString GetfavouriteserverurlAt(int index)
		{
			return new SchemaString(GetDomNodeValue(GetDomChildAt(NodeType.Attribute, "", "favouriteserverurl", index)));
		}

		public XmlNode GetStartingfavouriteserverurlCursor()
		{
			return GetDomFirstChild( NodeType.Attribute, "", "favouriteserverurl" );
		}

		public XmlNode GetAdvancedfavouriteserverurlCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Attribute, "", "favouriteserverurl", curNode );
		}

		public SchemaString GetfavouriteserverurlValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new SchemaString( curNode.Value );
		}


		public SchemaString Getfavouriteserverurl()
		{
			return GetfavouriteserverurlAt(0);
		}

		public SchemaString favouriteserverurl
		{
			get
			{
				return GetfavouriteserverurlAt(0);
			}
		}

		public void RemovefavouriteserverurlAt(int index)
		{
			RemoveDomChildAt(NodeType.Attribute, "", "favouriteserverurl", index);
		}

		public void Removefavouriteserverurl()
		{
			RemovefavouriteserverurlAt(0);
		}

		public XmlNode Addfavouriteserverurl(SchemaString newValue)
		{
			if( newValue.IsNull() == false )
				return AppendDomChild(NodeType.Attribute, "", "favouriteserverurl", newValue.ToString());
			return null;
		}

		public void InsertfavouriteserverurlAt(SchemaString newValue, int index)
		{
			if( newValue.IsNull() == false )
				InsertDomChildAt(NodeType.Attribute, "", "favouriteserverurl", index, newValue.ToString());
		}

		public void ReplacefavouriteserverurlAt(SchemaString newValue, int index)
		{
			ReplaceDomChildAt(NodeType.Attribute, "", "favouriteserverurl", index, newValue.ToString());
		}
		#endregion // favouriteserverurl accessor methods

		#region favouriteserverurl collection
        public favouriteserverurlCollection	Myfavouriteserverurls = new favouriteserverurlCollection( );

        public class favouriteserverurlCollection: IEnumerable
        {
            dappleviewType parent;
            public dappleviewType Parent
			{
				set
				{
					parent = value;
				}
			}
			public favouriteserverurlEnumerator GetEnumerator() 
			{
				return new favouriteserverurlEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class favouriteserverurlEnumerator: IEnumerator 
        {
			int nIndex;
			dappleviewType parent;
			public favouriteserverurlEnumerator(dappleviewType par) 
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
				return(nIndex < parent.favouriteserverurlCount );
			}
			public SchemaString  Current 
			{
				get 
				{
					return(parent.GetfavouriteserverurlAt(nIndex));
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

        #endregion // favouriteserverurl collection

		#region showbluemarble Documentation
		public static string GetshowbluemarbleAnnoDocumentation()
		{
			return "";		
		}
		public static string GetshowbluemarbleDefault()
		{
			return "true";		
		}
		#endregion

		#region showbluemarble accessor methods
		public static int GetshowbluemarbleMinCount()
		{
			return 0;
		}

		public static int showbluemarbleMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetshowbluemarbleMaxCount()
		{
			return 1;
		}

		public static int showbluemarbleMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetshowbluemarbleCount()
		{
			return DomChildCount(NodeType.Attribute, "", "showbluemarble");
		}

		public int showbluemarbleCount
		{
			get
			{
				return DomChildCount(NodeType.Attribute, "", "showbluemarble");
			}
		}

		public bool Hasshowbluemarble()
		{
			return HasDomChild(NodeType.Attribute, "", "showbluemarble");
		}

		public SchemaBoolean Newshowbluemarble()
		{
			return new SchemaBoolean();
		}

		public SchemaBoolean GetshowbluemarbleAt(int index)
		{
			return new SchemaBoolean(GetDomNodeValue(GetDomChildAt(NodeType.Attribute, "", "showbluemarble", index)));
		}

		public XmlNode GetStartingshowbluemarbleCursor()
		{
			return GetDomFirstChild( NodeType.Attribute, "", "showbluemarble" );
		}

		public XmlNode GetAdvancedshowbluemarbleCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Attribute, "", "showbluemarble", curNode );
		}

		public SchemaBoolean GetshowbluemarbleValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new SchemaBoolean( curNode.Value );
		}


		public SchemaBoolean Getshowbluemarble()
		{
			return GetshowbluemarbleAt(0);
		}

		public SchemaBoolean showbluemarble
		{
			get
			{
				return GetshowbluemarbleAt(0);
			}
		}

		public void RemoveshowbluemarbleAt(int index)
		{
			RemoveDomChildAt(NodeType.Attribute, "", "showbluemarble", index);
		}

		public void Removeshowbluemarble()
		{
			RemoveshowbluemarbleAt(0);
		}

		public XmlNode Addshowbluemarble(SchemaBoolean newValue)
		{
			if( newValue.IsNull() == false )
				return AppendDomChild(NodeType.Attribute, "", "showbluemarble", newValue.ToString());
			return null;
		}

		public void InsertshowbluemarbleAt(SchemaBoolean newValue, int index)
		{
			if( newValue.IsNull() == false )
				InsertDomChildAt(NodeType.Attribute, "", "showbluemarble", index, newValue.ToString());
		}

		public void ReplaceshowbluemarbleAt(SchemaBoolean newValue, int index)
		{
			ReplaceDomChildAt(NodeType.Attribute, "", "showbluemarble", index, newValue.ToString());
		}
		#endregion // showbluemarble accessor methods

		#region showbluemarble collection
        public showbluemarbleCollection	Myshowbluemarbles = new showbluemarbleCollection( );

        public class showbluemarbleCollection: IEnumerable
        {
            dappleviewType parent;
            public dappleviewType Parent
			{
				set
				{
					parent = value;
				}
			}
			public showbluemarbleEnumerator GetEnumerator() 
			{
				return new showbluemarbleEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class showbluemarbleEnumerator: IEnumerator 
        {
			int nIndex;
			dappleviewType parent;
			public showbluemarbleEnumerator(dappleviewType par) 
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
				return(nIndex < parent.showbluemarbleCount );
			}
			public SchemaBoolean  Current 
			{
				get 
				{
					return(parent.GetshowbluemarbleAt(nIndex));
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

        #endregion // showbluemarble collection

		#region servers Documentation
		public static string GetserversAnnoDocumentation()
		{
			return "";		
		}
		public static string GetserversDefault()
		{
			return "";		
		}
		#endregion

		#region servers accessor methods
		public static int GetserversMinCount()
		{
			return 0;
		}

		public static int serversMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetserversMaxCount()
		{
			return 1;
		}

		public static int serversMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetserversCount()
		{
			return DomChildCount(NodeType.Element, "", "servers");
		}

		public int serversCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "servers");
			}
		}

		public bool Hasservers()
		{
			return HasDomChild(NodeType.Element, "", "servers");
		}

		public serversType Newservers()
		{
			return new serversType(domNode.OwnerDocument.CreateElement("servers", ""));
		}

		public serversType GetserversAt(int index)
		{
			return new serversType(GetDomChildAt(NodeType.Element, "", "servers", index));
		}

		public XmlNode GetStartingserversCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "servers" );
		}

		public XmlNode GetAdvancedserversCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "servers", curNode );
		}

		public serversType GetserversValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new serversType( curNode );
		}


		public serversType Getservers()
		{
			return GetserversAt(0);
		}

		public serversType servers
		{
			get
			{
				return GetserversAt(0);
			}
		}

		public void RemoveserversAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "servers", index);
		}

		public void Removeservers()
		{
			RemoveserversAt(0);
		}

		public XmlNode Addservers(serversType newValue)
		{
			return AppendDomElement("", "servers", newValue);
		}

		public void InsertserversAt(serversType newValue, int index)
		{
			InsertDomElementAt("", "servers", index, newValue);
		}

		public void ReplaceserversAt(serversType newValue, int index)
		{
			ReplaceDomElementAt("", "servers", index, newValue);
		}
		#endregion // servers accessor methods

		#region servers collection
        public serversCollection	Myserverss = new serversCollection( );

        public class serversCollection: IEnumerable
        {
            dappleviewType parent;
            public dappleviewType Parent
			{
				set
				{
					parent = value;
				}
			}
			public serversEnumerator GetEnumerator() 
			{
				return new serversEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class serversEnumerator: IEnumerator 
        {
			int nIndex;
			dappleviewType parent;
			public serversEnumerator(dappleviewType par) 
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
				return(nIndex < parent.serversCount );
			}
			public serversType  Current 
			{
				get 
				{
					return(parent.GetserversAt(nIndex));
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

        #endregion // servers collection

		#region activelayers Documentation
		public static string GetactivelayersAnnoDocumentation()
		{
			return "";		
		}
		public static string GetactivelayersDefault()
		{
			return "";		
		}
		#endregion

		#region activelayers accessor methods
		public static int GetactivelayersMinCount()
		{
			return 0;
		}

		public static int activelayersMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetactivelayersMaxCount()
		{
			return 1;
		}

		public static int activelayersMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetactivelayersCount()
		{
			return DomChildCount(NodeType.Element, "", "activelayers");
		}

		public int activelayersCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "activelayers");
			}
		}

		public bool Hasactivelayers()
		{
			return HasDomChild(NodeType.Element, "", "activelayers");
		}

		public activelayersType Newactivelayers()
		{
			return new activelayersType(domNode.OwnerDocument.CreateElement("activelayers", ""));
		}

		public activelayersType GetactivelayersAt(int index)
		{
			return new activelayersType(GetDomChildAt(NodeType.Element, "", "activelayers", index));
		}

		public XmlNode GetStartingactivelayersCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "activelayers" );
		}

		public XmlNode GetAdvancedactivelayersCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "activelayers", curNode );
		}

		public activelayersType GetactivelayersValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new activelayersType( curNode );
		}


		public activelayersType Getactivelayers()
		{
			return GetactivelayersAt(0);
		}

		public activelayersType activelayers
		{
			get
			{
				return GetactivelayersAt(0);
			}
		}

		public void RemoveactivelayersAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "activelayers", index);
		}

		public void Removeactivelayers()
		{
			RemoveactivelayersAt(0);
		}

		public XmlNode Addactivelayers(activelayersType newValue)
		{
			return AppendDomElement("", "activelayers", newValue);
		}

		public void InsertactivelayersAt(activelayersType newValue, int index)
		{
			InsertDomElementAt("", "activelayers", index, newValue);
		}

		public void ReplaceactivelayersAt(activelayersType newValue, int index)
		{
			ReplaceDomElementAt("", "activelayers", index, newValue);
		}
		#endregion // activelayers accessor methods

		#region activelayers collection
        public activelayersCollection	Myactivelayerss = new activelayersCollection( );

        public class activelayersCollection: IEnumerable
        {
            dappleviewType parent;
            public dappleviewType Parent
			{
				set
				{
					parent = value;
				}
			}
			public activelayersEnumerator GetEnumerator() 
			{
				return new activelayersEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class activelayersEnumerator: IEnumerator 
        {
			int nIndex;
			dappleviewType parent;
			public activelayersEnumerator(dappleviewType par) 
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
				return(nIndex < parent.activelayersCount );
			}
			public activelayersType  Current 
			{
				get 
				{
					return(parent.GetactivelayersAt(nIndex));
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

        #endregion // activelayers collection

		#region cameraorientation Documentation
		public static string GetcameraorientationAnnoDocumentation()
		{
			return "";		
		}
		public static string GetcameraorientationDefault()
		{
			return "";		
		}
		#endregion

		#region cameraorientation accessor methods
		public static int GetcameraorientationMinCount()
		{
			return 0;
		}

		public static int cameraorientationMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetcameraorientationMaxCount()
		{
			return 1;
		}

		public static int cameraorientationMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetcameraorientationCount()
		{
			return DomChildCount(NodeType.Element, "", "cameraorientation");
		}

		public int cameraorientationCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "cameraorientation");
			}
		}

		public bool Hascameraorientation()
		{
			return HasDomChild(NodeType.Element, "", "cameraorientation");
		}

		public cameraorientationType Newcameraorientation()
		{
			return new cameraorientationType(domNode.OwnerDocument.CreateElement("cameraorientation", ""));
		}

		public cameraorientationType GetcameraorientationAt(int index)
		{
			return new cameraorientationType(GetDomChildAt(NodeType.Element, "", "cameraorientation", index));
		}

		public XmlNode GetStartingcameraorientationCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "cameraorientation" );
		}

		public XmlNode GetAdvancedcameraorientationCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "cameraorientation", curNode );
		}

		public cameraorientationType GetcameraorientationValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new cameraorientationType( curNode );
		}


		public cameraorientationType Getcameraorientation()
		{
			return GetcameraorientationAt(0);
		}

		public cameraorientationType cameraorientation
		{
			get
			{
				return GetcameraorientationAt(0);
			}
		}

		public void RemovecameraorientationAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "cameraorientation", index);
		}

		public void Removecameraorientation()
		{
			RemovecameraorientationAt(0);
		}

		public XmlNode Addcameraorientation(cameraorientationType newValue)
		{
			return AppendDomElement("", "cameraorientation", newValue);
		}

		public void InsertcameraorientationAt(cameraorientationType newValue, int index)
		{
			InsertDomElementAt("", "cameraorientation", index, newValue);
		}

		public void ReplacecameraorientationAt(cameraorientationType newValue, int index)
		{
			ReplaceDomElementAt("", "cameraorientation", index, newValue);
		}
		#endregion // cameraorientation accessor methods

		#region cameraorientation collection
        public cameraorientationCollection	Mycameraorientations = new cameraorientationCollection( );

        public class cameraorientationCollection: IEnumerable
        {
            dappleviewType parent;
            public dappleviewType Parent
			{
				set
				{
					parent = value;
				}
			}
			public cameraorientationEnumerator GetEnumerator() 
			{
				return new cameraorientationEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class cameraorientationEnumerator: IEnumerator 
        {
			int nIndex;
			dappleviewType parent;
			public cameraorientationEnumerator(dappleviewType par) 
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
				return(nIndex < parent.cameraorientationCount );
			}
			public cameraorientationType  Current 
			{
				get 
				{
					return(parent.GetcameraorientationAt(nIndex));
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

        #endregion // cameraorientation collection

		#region notes Documentation
		public static string GetnotesAnnoDocumentation()
		{
			return "";		
		}
		public static string GetnotesDefault()
		{
			return "";		
		}
		#endregion

		#region notes accessor methods
		public static int GetnotesMinCount()
		{
			return 0;
		}

		public static int notesMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetnotesMaxCount()
		{
			return 1;
		}

		public static int notesMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetnotesCount()
		{
			return DomChildCount(NodeType.Element, "", "notes");
		}

		public int notesCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "notes");
			}
		}

		public bool Hasnotes()
		{
			return HasDomChild(NodeType.Element, "", "notes");
		}

		public SchemaString Newnotes()
		{
			return new SchemaString();
		}

		public SchemaString GetnotesAt(int index)
		{
			return new SchemaString(GetDomNodeValue(GetDomChildAt(NodeType.Element, "", "notes", index)));
		}

		public XmlNode GetStartingnotesCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "notes" );
		}

		public XmlNode GetAdvancednotesCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "notes", curNode );
		}

		public SchemaString GetnotesValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new SchemaString( curNode.InnerText );
		}


		public SchemaString Getnotes()
		{
			return GetnotesAt(0);
		}

		public SchemaString notes
		{
			get
			{
				return GetnotesAt(0);
			}
		}

		public void RemovenotesAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "notes", index);
		}

		public void Removenotes()
		{
			RemovenotesAt(0);
		}

		public XmlNode Addnotes(SchemaString newValue)
		{
			if( newValue.IsNull() == false )
				return AppendDomChild(NodeType.Element, "", "notes", newValue.ToString());
			return null;
		}

		public void InsertnotesAt(SchemaString newValue, int index)
		{
			if( newValue.IsNull() == false )
				InsertDomChildAt(NodeType.Element, "", "notes", index, newValue.ToString());
		}

		public void ReplacenotesAt(SchemaString newValue, int index)
		{
			ReplaceDomChildAt(NodeType.Element, "", "notes", index, newValue.ToString());
		}
		#endregion // notes accessor methods

		#region notes collection
        public notesCollection	Mynotess = new notesCollection( );

        public class notesCollection: IEnumerable
        {
            dappleviewType parent;
            public dappleviewType Parent
			{
				set
				{
					parent = value;
				}
			}
			public notesEnumerator GetEnumerator() 
			{
				return new notesEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class notesEnumerator: IEnumerator 
        {
			int nIndex;
			dappleviewType parent;
			public notesEnumerator(dappleviewType par) 
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
				return(nIndex < parent.notesCount );
			}
			public SchemaString  Current 
			{
				get 
				{
					return(parent.GetnotesAt(nIndex));
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

        #endregion // notes collection

		#region preview Documentation
		public static string GetpreviewAnnoDocumentation()
		{
			return "";		
		}
		public static string GetpreviewDefault()
		{
			return "";		
		}
		#endregion

		#region preview accessor methods
		public static int GetpreviewMinCount()
		{
			return 0;
		}

		public static int previewMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetpreviewMaxCount()
		{
			return 1;
		}

		public static int previewMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetpreviewCount()
		{
			return DomChildCount(NodeType.Element, "", "preview");
		}

		public int previewCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "preview");
			}
		}

		public bool Haspreview()
		{
			return HasDomChild(NodeType.Element, "", "preview");
		}

		public SchemaBase64Binary Newpreview()
		{
			return new SchemaBase64Binary();
		}

		public SchemaBase64Binary GetpreviewAt(int index)
		{
			return new SchemaBase64Binary(GetDomNodeValue(GetDomChildAt(NodeType.Element, "", "preview", index)));
		}

		public XmlNode GetStartingpreviewCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "preview" );
		}

		public XmlNode GetAdvancedpreviewCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "preview", curNode );
		}

		public SchemaBase64Binary GetpreviewValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new SchemaBase64Binary( curNode.InnerText );
		}


		public SchemaBase64Binary Getpreview()
		{
			return GetpreviewAt(0);
		}

		public SchemaBase64Binary preview
		{
			get
			{
				return GetpreviewAt(0);
			}
		}

		public void RemovepreviewAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "preview", index);
		}

		public void Removepreview()
		{
			RemovepreviewAt(0);
		}

		public XmlNode Addpreview(SchemaBase64Binary newValue)
		{
			if( newValue.IsNull() == false )
				return AppendDomChild(NodeType.Element, "", "preview", newValue.ToString());
			return null;
		}

		public void InsertpreviewAt(SchemaBase64Binary newValue, int index)
		{
			if( newValue.IsNull() == false )
				InsertDomChildAt(NodeType.Element, "", "preview", index, newValue.ToString());
		}

		public void ReplacepreviewAt(SchemaBase64Binary newValue, int index)
		{
			ReplaceDomChildAt(NodeType.Element, "", "preview", index, newValue.ToString());
		}
		#endregion // preview accessor methods

		#region preview collection
        public previewCollection	Mypreviews = new previewCollection( );

        public class previewCollection: IEnumerable
        {
            dappleviewType parent;
            public dappleviewType Parent
			{
				set
				{
					parent = value;
				}
			}
			public previewEnumerator GetEnumerator() 
			{
				return new previewEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class previewEnumerator: IEnumerator 
        {
			int nIndex;
			dappleviewType parent;
			public previewEnumerator(dappleviewType par) 
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
				return(nIndex < parent.previewCount );
			}
			public SchemaBase64Binary  Current 
			{
				get 
				{
					return(parent.GetpreviewAt(nIndex));
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

        #endregion // preview collection

        private void SetCollectionParents()
        {
            Myfavouriteserverurls.Parent = this; 
            Myshowbluemarbles.Parent = this; 
            Myserverss.Parent = this; 
            Myactivelayerss.Parent = this; 
            Mycameraorientations.Parent = this; 
            Mynotess.Parent = this; 
            Mypreviews.Parent = this; 
	}
}
}

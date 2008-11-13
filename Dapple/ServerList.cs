using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using Geosoft.GX.DAPGetData;
using Dapple.LayerGeneration;
using WorldWind;

namespace Dapple
{
   public partial class ServerList : UserControl
   {
      #region Constants

      private const int LAYERS_PER_PAGE = 10;
      private const int MAX_DAP_RESULTS = 1000;

      #endregion

      #region Member variables

      private String m_strSearchString;
      private GeographicBoundingBox m_oSearchBox;

      private ArrayList m_oServerList;

      private int m_iCurrPage;
      private int m_iNumPages;
      private List<LayerBuilder> m_oCurrServerLayers;

      private Point m_oDragDropStartPoint;

      private Object m_oSelectedServer;

      private LayerList m_hLayerList;

      public event Dapple.MainForm.ViewMetadataHandler ViewMetadata;
		public event EventHandler LayerSelectionChanged;

      #endregion

      #region Constructors

      /// <summary>
      /// Constructor.
      /// </summary>
      public ServerList()
      {
         InitializeComponent();
         c_oPageNavigator.PageBack += new System.Threading.ThreadStart(BackPage);
         c_oPageNavigator.PageForward += new System.Threading.ThreadStart(ForwardPage);

         m_strSearchString = String.Empty;
         m_oSearchBox = null;

         m_oServerList = null;

         m_oSelectedServer = null;
         m_oDragDropStartPoint = Point.Empty;

         m_oServerList = new ArrayList();
         SetNoServer();
      }

      #endregion

      #region Properties

      /// <summary>
      /// Sets the ImageLists for the layer list.
      /// </summary>
      public ImageList ImageList
      {
         set
         {
            c_lvLayers.SmallImageList = value;
            c_lvLayers.LargeImageList = value;
         }
      }

      /// <summary>
      /// The list of servers to display in the drop-down list.
      /// </summary>
      public ArrayList Servers
      {
         set
         {
            checkArrayList(value);
            m_oServerList = value;

            if (m_oSelectedServer != null && m_oServerList.Contains(m_oSelectedServer))
            {
               c_cbServers.SelectedIndex = m_oServerList.IndexOf(m_oSelectedServer);
            }
            else
            {
               SetNoServer();
               FillServerList();
            }
         }
      }

      /// <summary>
      /// Set the selected server
      /// </summary>
      public Object SelectedServer
      {
         get
         {
            return m_oSelectedServer;
         }
         set
         {
            int iSelectedIndex = m_oServerList.IndexOf(value);

            if (iSelectedIndex != c_cbServers.SelectedIndex)
            {
               c_cbServers.SelectedIndex = iSelectedIndex;
               c_cbServers_SelectedIndexChanged(this, new EventArgs());
            }
         }
      }

      /// <summary>
      /// Whether any of the layers in the layer list are selected.
      /// </summary>
      public Boolean HasLayersSelected
      {
         get 
         {
            return c_lvLayers.SelectedIndices.Count > 0;
         }
      }

      /// <summary>
      /// A List of the selected layers.
      /// </summary>
      public List<LayerBuilder> SelectedLayers
      {
         get
         {
            List<LayerBuilder> result = new List<LayerBuilder>();

            foreach (int index in c_lvLayers.SelectedIndices)
            {
               result.Add(m_oCurrServerLayers[index + m_iCurrPage * LAYERS_PER_PAGE]);
            }

            return result;
         }
      }

      /// <summary>
      /// The layer list that this will add to on an 'add' action.
      /// </summary>
      public LayerList LayerList
      {
         set
         {
            if (m_hLayerList != null)
            {
               m_hLayerList.ActiveLayersChanged -= new LayerList.ActiveLayersChangedHandler(UpdateActiveLayers); 
            }

            m_hLayerList = value;
            m_hLayerList.ActiveLayersChanged += new LayerList.ActiveLayersChangedHandler(UpdateActiveLayers);
            UpdateActiveLayers();
         }
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Sets the search criteria for the search filter.  Performs a search if the values sent are different
      /// from the values that are currently posessed.
      /// </summary>
      /// <param name="strKeywords">The keywords to search for.</param>
      /// <param name="oBounds">The bounding box to search for.</param>
      public void setSearchCriteria(String strKeywords, GeographicBoundingBox oBounds)
      {
         bool blBoundsEqual = false;
         if (oBounds == null && m_oSearchBox == null) blBoundsEqual = true;
         if (oBounds != null && m_oSearchBox != null && oBounds.Equals(m_oSearchBox)) blBoundsEqual = true;

         if (!strKeywords.Equals(m_strSearchString) || !blBoundsEqual)
         {
            m_strSearchString = strKeywords;
            m_oSearchBox = oBounds;
            if (c_cbServers.SelectedIndex != -1)
            {
               InitLayerList();
            }
         }
      }

      /// <summary>
      /// Makes the text color of those layers that have been added to the layer list green.
      /// </summary>
      public void UpdateActiveLayers()
      {
         c_lvLayers.SuspendLayout();

         if (m_oCurrServerLayers != null)
         {
            for (int count = m_iCurrPage * LAYERS_PER_PAGE; count < m_iCurrPage * LAYERS_PER_PAGE + LAYERS_PER_PAGE; count++)
            {
               if (count < m_oCurrServerLayers.Count)
               {
                  if (m_hLayerList.AllLayers.Contains(m_oCurrServerLayers[count]))
                  {
                     c_lvLayers.Items[count % LAYERS_PER_PAGE].ForeColor = Color.ForestGreen;
                  }
                  else
                  {
                     c_lvLayers.Items[count % LAYERS_PER_PAGE].ForeColor = c_lvLayers.ForeColor;
                  }
               }
            }
         }

         c_lvLayers.ResumeLayout();
      }

		/// <summary>
		/// Clear the current search and execute it again.
		/// </summary>
		public void ReSearch()
		{
			if (c_cbServers.SelectedIndex == -1)
			{
				SetNoServer();
			}
			else
			{
				SetSearching();
				InitLayerList();
			}
		}

      #endregion

      #region Event handlers

      /// <summary>
      /// Event handler for picking a server from the server drop-down box.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void c_cbServers_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (c_cbServers.SelectedIndex == -1)
         {
            SetNoServer();
            m_oSelectedServer = null;
         }
         else
         {
            Object oNewSelectedServer = m_oServerList[c_cbServers.SelectedIndex];
            if (!oNewSelectedServer.Equals(m_oSelectedServer))
            {
               SetSearching();
               InitLayerList();
               m_oSelectedServer = oNewSelectedServer;
            }
         }
      }

      private void c_cbServers_DrawItem(object sender, DrawItemEventArgs e)
      {
         e.DrawBackground();

         if (e.Index >= 0)
         {
            e.Graphics.DrawIcon(getServerIcon(m_oServerList[e.Index]), new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Height, e.Bounds.Height));
            e.Graphics.DrawString(getServerName(m_oServerList[e.Index]), e.Font, Brushes.Black, new PointF(e.Bounds.X + e.Bounds.Height, e.Bounds.Y));
         }
         else
         {
            e.Graphics.DrawString("Select a server", e.Font, Brushes.Black, e.Bounds.Location);
         }
      }

      private void cLayersListView_DoubleClick(object sender, EventArgs e)
      {
         if (m_hLayerList != null)
         {
            m_hLayerList.AddLayers(this.SelectedLayers);
         }
      }

      /// <summary>
      /// User mouses down on the layer list view.  They may be wanting to start a drag & drop.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cLayersListView_MouseDown(object sender, MouseEventArgs e)
      {
         m_oDragDropStartPoint = e.Location;
      }

      /// <summary>
      /// User moves the mouse.  If the drag flag is set, initiate the drag & drop.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cLayersListView_MouseMove(object sender, MouseEventArgs e)
      {
         if (m_oDragDropStartPoint != Point.Empty && e.Location.Equals(m_oDragDropStartPoint)) return; // The mouse didn't really move

         if (m_oDragDropStartPoint != Point.Empty && HasLayersSelected && (e.Button & MouseButtons.Left) == MouseButtons.Left)
         {
            DoDragDrop(SelectedLayers, DragDropEffects.Copy);
            m_oDragDropStartPoint = Point.Empty;
         }
      }

      /// <summary>
      /// The layer list has resized.  Resize the (invisible) column.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cLayersListView_Resize(object sender, EventArgs e)
      {
         ResizeColumn();
      }

      /// <summary>
      /// The control loads.  Set the size of the (invisible) layer list column.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ServerList_Load(object sender, EventArgs e)
      {
         ResizeColumn();
      }

      /// <summary>
      /// Cancel the context menu if no layers are selected.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cLayerContextMenu_Opening(object sender, CancelEventArgs e)
      {
         if (c_lvLayers.SelectedIndices.Count == 0) e.Cancel = true;

         c_miViewLegend.Enabled = (c_lvLayers.SelectedIndices.Count == 1 && m_oCurrServerLayers[c_lvLayers.SelectedIndices[0]].SupportsLegend);
      }

      /// <summary>
      /// Add the selected layers to the visible layers set.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void addToLayersToolStripMenuItem_Click(object sender, EventArgs e)
      {
         if (m_hLayerList != null)
         {
            m_hLayerList.AddLayers(this.SelectedLayers);
         }
      }

      /// <summary>
      /// View the legend for the selected layer.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void viewLegendToolStripMenuItem_Click(object sender, EventArgs e)
      {
         LayerBuilder oBuilder = m_oCurrServerLayers[c_lvLayers.SelectedIndices[0]];

         string[] aLegends = oBuilder.GetLegendURLs();
         foreach (string szLegend in aLegends)
         {
            if (!String.IsNullOrEmpty(szLegend)) MainForm.BrowseTo(szLegend);
         }
      }

      /// <summary>
      /// View the metadata if there is one layer selected.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cLayersListView_SelectedIndexChanged(object sender, EventArgs e)
      {
			if (c_lvLayers.SelectedIndices.Count == 1)
			{
				if (ViewMetadata != null) ViewMetadata(m_oCurrServerLayers[c_lvLayers.SelectedIndices[0] + LAYERS_PER_PAGE * m_iCurrPage]);
			}
			else
			{
				if (ViewMetadata != null) ViewMetadata(null);
			}

			if (LayerSelectionChanged != null) LayerSelectionChanged(this, new EventArgs());
      }

      #endregion

      #region Private helper methods

      /// <summary>
      /// User presses the "back a page" button.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void BackPage()
      {
         m_iCurrPage--;
         DrawCurrentPage();
      }

      /// <summary>
      /// User presses the "forward a page" button.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ForwardPage()
      {
         m_iCurrPage++;
         DrawCurrentPage();
      }

      #region Enabling/disabling controls

      /// <summary>
      /// Call to deselect servers.
      /// </summary>
      private void SetNoServer()
      {
         c_cbServers.SelectedIndex = -1;
         c_lvLayers.Items.Clear();
         c_oPageNavigator.SetState(String.Empty);

         m_oCurrServerLayers = new List<LayerBuilder>();
      }

      /// <summary>
      /// Call when a search is underway.
      /// </summary>
      private void SetSearching()
      {
         c_lvLayers.Items.Clear();
         c_oPageNavigator.SetState("Searching...");

         Refresh();
      }

      /// <summary>
      /// Resets the layer list and the navigation buttons and the label.
      /// </summary>
      private void DrawCurrentPage()
      {
         FillLayerList();
         UpdatePageNavigation();
      }

      /// <summary>
      /// Call to update the forward, back buttons, page label, and layers in list when the current page changes.
      /// </summary>
		private void UpdatePageNavigation()
		{
			if (m_oCurrServerLayers.Count > 0)
			{
				c_oPageNavigator.SetState(m_iCurrPage, m_oCurrServerLayers.Count);
			}
			else
			{
				c_oPageNavigator.SetState("No results");
			}
		}

      /// <summary>
      /// Call to fill in the layer list.  Takes into account the current page.
      /// </summary>
      private void FillLayerList()
      {
         c_lvLayers.SuspendLayout();
         c_lvLayers.Items.Clear();

         if (m_oCurrServerLayers != null)
         {
            for (int count = m_iCurrPage * LAYERS_PER_PAGE; count < m_iCurrPage * LAYERS_PER_PAGE + LAYERS_PER_PAGE; count++)
            {
               if (count < m_oCurrServerLayers.Count) c_lvLayers.Items.Add(getLayerTitle(m_oCurrServerLayers[count]), c_lvLayers.SmallImageList.Images.IndexOfKey(m_oCurrServerLayers[count].DisplayIconKey));
            }
         }

         c_lvLayers.ResumeLayout();

         UpdateActiveLayers();
      }

      /// <summary>
      /// Sets the width of the (invisible) layer list column equal to the width of the layer list.
      /// </summary>
      private void ResizeColumn()
      {
         c_lvLayers.Columns[0].Width = c_lvLayers.ClientSize.Width;
      }

      /// <summary>
      /// Call to fill in the server list.
      /// </summary>
      private void FillServerList()
      {
         c_cbServers.SuspendLayout();
         c_cbServers.Items.Clear();

         foreach (Object obj in m_oServerList)
         {
            c_cbServers.Items.Add(getServerName(obj));
         }

         c_cbServers.ResumeLayout();
      }

      #endregion

      #region Smelly code

      /// <summary>
      /// Gets the list of layers from the server (according to the search criteria) and populates the
      /// local list of layers, and resets the currently viewed page to the first page.
		/// Finally, updates the UI to show the first page.
      /// </summary>
      private void InitLayerList()
      {
         Object obj = m_oServerList[c_cbServers.SelectedIndex];

         try
         {
            if (obj is Server)
            {
               ArrayList oDapLayers = new ArrayList();
               m_oCurrServerLayers = new List<LayerBuilder>();
               Geosoft.Dap.Common.BoundingBox oConvertedBox = m_oSearchBox == null ? null : new Geosoft.Dap.Common.BoundingBox(m_oSearchBox.East, m_oSearchBox.North, m_oSearchBox.West, m_oSearchBox.South);
               ((Server)obj).Command.GetCatalog(null, 0, 0, MAX_DAP_RESULTS, m_strSearchString, oConvertedBox, out oDapLayers);

               foreach (Geosoft.Dap.Common.DataSet oDataSet in oDapLayers)
               {
                  m_oCurrServerLayers.Add(new DAPQuadLayerBuilder(oDataSet, MainForm.WorldWindowSingleton, obj as Server, null));
               }
            }
            else if (obj is AsyncBuilder)
            {
               m_oCurrServerLayers = new List<LayerBuilder>();
               ((AsyncBuilder)obj).getLayerBuilders(ref m_oCurrServerLayers);
					for (int index = m_oCurrServerLayers.Count - 1; index > 0; index--)
					{
						if (!String.IsNullOrEmpty(m_strSearchString) && m_oCurrServerLayers[index].Title.IndexOf(m_strSearchString, StringComparison.InvariantCultureIgnoreCase) == -1
							|| m_oSearchBox != null && !m_oCurrServerLayers[index].Extents.Intersects(m_oSearchBox))
						{
							m_oCurrServerLayers.RemoveAt(index);
						}
					}
            }
            else
               throw new ArgumentException("obj is unknown type " + obj.GetType());

            m_iCurrPage = 0;
            m_iNumPages = m_oCurrServerLayers.Count / LAYERS_PER_PAGE;
            if (m_oCurrServerLayers.Count % LAYERS_PER_PAGE != 0) m_iNumPages++;
				DrawCurrentPage();
         }
         catch (Exception)
         {
            m_oCurrServerLayers = null;
            m_iCurrPage = 0;
            m_iNumPages = -1;
            c_oPageNavigator.SetState("Error occurred");
				FillLayerList();
         }
      }

      /// <summary>
      /// Get the server name for the Server/ServerBuilder.
      /// </summary>
      /// Breaking polymorphic design principles since August 2007!
      /// <remarks>
      /// </remarks>
      /// <param name="obj">The server to get the name of.</param>
      /// <returns>The server name.</returns>
      private String getServerName(Object obj)
      {
         if (obj is Server)
            return ((Server)obj).Name;
         else if (obj is ServerBuilder)
            return ((ServerBuilder)obj).Title;
         else
            throw new ArgumentException("obj is unknown type " + obj.GetType());
      }

      private Icon getServerIcon(Object obj)
      {
         if (obj is Server)
            return Dapple.Properties.Resources.dap;
         else if (obj is ServerBuilder)
            return ((ServerBuilder)obj).Icon;
         else
            throw new ArgumentException("obj is unknown type " + obj.GetType());
      }

      /// <summary>
      /// Get the layer title for the LayerBuilder/DataSet.
      /// </summary>
      /// <param name="obj">The layer to get the title of.</param>
      /// <returns></returns>
      private String getLayerTitle(Object obj)
      {
         if (obj is Geosoft.Dap.Common.DataSet)
            return ((Geosoft.Dap.Common.DataSet)obj).Title;
         else if (obj is LayerBuilder)
            return ((LayerBuilder)obj).Title;
         else
            throw new ArgumentException("obj is unknown type " + obj.GetType());
      }

      /// <summary>
      /// Get the image of the icon to display in the list for this layer.
      /// </summary>
      /// <param name="obj">The layer to get the icon of.</param>
      /// <returns></returns>
      private int getImageIndex(Object obj)
      {
         if (obj is Geosoft.Dap.Common.DataSet)
            return c_lvLayers.SmallImageList.Images.IndexOfKey("dap_" + ((Geosoft.Dap.Common.DataSet)obj).Type.ToLower());
         else if (obj is LayerBuilder)
            return c_lvLayers.SmallImageList.Images.IndexOfKey("layer");
         else
            throw new ArgumentException("obj is unknown type " + obj.GetType());
      }
      
      /// <summary>
      /// Throws an ArgumentException if an ArrayList contains something other than ServerBuilders and Servers.
      /// </summary>
      /// <param name="oList"></param>
      private void checkArrayList(ArrayList oList)
      {
         foreach (Object obj in oList)
         {
            if (!(obj is Server || obj is ServerBuilder)) throw new ArgumentException("oServers contains an invalid object (Type " + obj.GetType().ToString() + ")");
         }
      }

      #endregion

      #endregion
   }
}

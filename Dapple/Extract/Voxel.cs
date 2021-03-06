using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace Dapple.Extract
{
   /// <summary>
   /// Set voxel download options
   /// </summary>
   internal partial class Voxel : DownloadOptions
   {
      #region Constants
      private readonly string MAP_EXT = ".map";
      #endregion

      /// <summary>
      /// Control where the resolution can be changed
      /// </summary>
      internal override bool ResolutionEnabled
      {
         set { oResolution.Enabled = value; }
      }

		internal override bool OpenInMap
		{
			get { return true; }
		}

		internal override ErrorProvider ErrorProvider
		{
			get
			{
				return base.ErrorProvider;
			}
			set
			{
				base.ErrorProvider = value;
				oResolution.ErrorProvider = value;
			}
		}

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      internal Voxel(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : base(oDAPbuilder)
      {
         InitializeComponent();

         tbFilename.Text = System.IO.Path.ChangeExtension(oDAPbuilder.Title, MAP_EXT);
         tbGroupName.Text = oDAPbuilder.Title;         

         oResolution.SetDownloadOptions(this);         
         SetDefaultResolution();
      }

      /// <summary>
      /// Set the default resolution
      /// </summary>
      internal override void SetDefaultResolution()
      {
         double dMinX, dMaxX, dMinY, dMaxY;
         SortedList<double, int> oResolutionList;
         SortedList<double, int> oX;
         SortedList<double, int> oY;
         SortedList<double, int> oZ;

         string strCoordinateSystem = m_strLayerProjection;
         MainForm.MontajInterface.GetExtents(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName, out dMaxX, out dMinX, out dMaxY, out dMinY);
         MainForm.MontajInterface.GetVoxelInfo(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName, out oResolutionList, out oX, out oY, out oZ);

         oResolution.Setup(strCoordinateSystem, dMinX, dMinY, dMaxX, dMaxY, oResolutionList, oX, oY, oZ);
      }

      internal override void SetNativeResolution()
      {
         oResolution.SetNativeResolution();
      }

      /// <summary>
      /// Write out settings for the voxel dataset
      /// </summary>
      /// <param name="oDatasetElement"></param>
      /// <param name="strDestFolder"></param>
      /// <param name="bDefaultResolution"></param>
      /// <returns></returns>
		internal override ExtractSaveResult Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         ExtractSaveResult result = base.Save(oDatasetElement, strDestFolder, eCS);

         System.Xml.XmlAttribute oPathAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
         oPathAttr.Value = System.IO.Path.Combine(strDestFolder, System.IO.Path.ChangeExtension(Utility.FileSystem.SanitizeFilename(tbFilename.Text), MAP_EXT));
         oDatasetElement.Attributes.Append(oPathAttr);

         System.Xml.XmlAttribute oResolutionAttr = oDatasetElement.OwnerDocument.CreateAttribute("resolution");
         oResolutionAttr.Value = oResolution.ResolutionValueSpecific(eCS).ToString(CultureInfo.InvariantCulture);
         oDatasetElement.Attributes.Append(oResolutionAttr);

         System.Xml.XmlAttribute oGroupAttribute = oDatasetElement.OwnerDocument.CreateAttribute("group");
         oGroupAttribute.Value = tbGroupName.Text;
         oDatasetElement.Attributes.Append(oGroupAttribute);

			return result;
      }

		internal override DownloadOptions.DuplicateFileCheckResult CheckForDuplicateFiles(String szExtractDirectory, Form hExtractForm)
		{
			String szFilename = System.IO.Path.Combine(szExtractDirectory, System.IO.Path.ChangeExtension(tbFilename.Text, MAP_EXT));
			if (System.IO.File.Exists(szFilename))
			{
				return QueryOverwriteFile("The file \"" + szFilename + "\" already exists.  Overwrite?", hExtractForm);
			}
			else
			{
				return DuplicateFileCheckResult.Yes;
			}
		}

		private void tbFilename_Validating(object sender, CancelEventArgs e)
		{
			if (String.IsNullOrEmpty(tbFilename.Text))
			{
				m_oErrorProvider.SetError(tbFilename, "Field cannot be empty.");
				e.Cancel = true;
			}
			else
			{
				m_oErrorProvider.SetError(tbFilename, String.Empty);
			}
		}

		private void tbGroupName_Validating(object sender, CancelEventArgs e)
		{
			if (String.IsNullOrEmpty(tbGroupName.Text))
			{
				m_oErrorProvider.SetError(tbGroupName, "Field cannot be empty.");
				e.Cancel = true;
			}
			else
			{
				m_oErrorProvider.SetError(tbGroupName, String.Empty);
			}
		}
   }
}

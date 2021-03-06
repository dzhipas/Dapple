using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using WorldWind;
using WorldWind.Renderable;
using System.Xml;
using Dapple;
using System.Globalization;

namespace Dapple.LayerGeneration
{
   internal class GeorefImageLayerBuilder : LayerBuilder
   {
      #region Statics

      internal static readonly string URLProtocolName = "gxtif:///";
      internal static readonly string CacheSubDir = "Local Image Cache";

      #endregion

		#region Member Variables

		ImageLayer m_Layer;
		string m_strFileName;
		string m_strCacheFileName;
		bool m_bIsTmp;
		bool m_blnIsChanged = true;
		bool m_blMissingFile;

		#endregion

      #region Constructor

      internal GeorefImageLayerBuilder(string strDisplayName, string strFileName, bool bTmp, WorldWindow oWorldWindow, IBuilder parent)
         : base(strDisplayName.Length > 0 ? strDisplayName : Path.GetFileName(strFileName), oWorldWindow, parent)
      {
         if (strFileName == null) throw new ArgumentNullException("strFileName");
         m_strFileName = strFileName;
         m_bIsTmp = bTmp;
         m_strCacheFileName = Path.Combine(GetCachePath(), Path.GetFileNameWithoutExtension(strFileName) + ".png");
			m_blMissingFile = !File.Exists(m_strFileName);
      }

      internal GeorefImageLayerBuilder(string strFileName, bool bTmp, WorldWindow oWorldWindow, IBuilder parent)
         : base(Path.GetFileName(strFileName), oWorldWindow, parent)
      {
         if (strFileName == null) throw new ArgumentNullException("strFileName");
         m_strFileName = strFileName;
         m_bIsTmp = bTmp;
         m_strCacheFileName = Path.Combine(GetCachePath(), Path.GetFileNameWithoutExtension(strFileName) + ".png");
			m_blMissingFile = !File.Exists(m_strFileName);
      }

      #endregion

		#region Properties

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The opacity of the image (255 = opaque, 0 = transparent)")]
		public override byte Opacity
		{
			get
			{
				if (m_Layer != null)
					return m_Layer.Opacity;
				return m_bOpacity;
			}
			set
			{
				bool bChanged = false;
				if (m_bOpacity != value)
				{
					m_bOpacity = value;
					bChanged = true;
				}
				if (m_Layer != null && m_Layer.Opacity != value)
				{
					m_Layer.Opacity = value;
					bChanged = true;
				}
				if (bChanged)
					SendBuilderChanged(BuilderChangeType.OpacityChanged);
			}
		}

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether this data layer is visible on the globe")]
		public override bool Visible
		{
			get
			{
				if (m_Layer != null)
					return m_Layer.IsOn;
				return m_IsOn;
			}
			set
			{
				bool bChanged = false;
				if (m_IsOn != value)
				{
					m_IsOn = value;
					bChanged = true;
				}
				if (m_Layer != null && m_Layer.IsOn != value)
				{
					m_Layer.IsOn = value;
					bChanged = true;
				}

				if (bChanged)
					SendBuilderChanged(BuilderChangeType.VisibilityChanged);
			}
		}

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The extents of this data layer, in WGS 84")]
		public override GeographicBoundingBox Extents
		{
			get
			{
				if (m_blMissingFile)
				{
					return new GeographicBoundingBox(90, -90, -180, 180);
				}
				else
				{
					if (m_Layer == null)
						GetLayer();
					return new GeographicBoundingBox(m_Layer.MaxLat, m_Layer.MinLat, m_Layer.MinLon, m_Layer.MaxLon);
				}
			}
		}

		[System.ComponentModel.Category("GeoTIFF")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The filename of the GeoTIFF")]
		internal string FileName
		{
			get { return m_strFileName; }
		}

		[System.ComponentModel.Browsable(false)]
		public override bool IsChanged
		{
			get { return m_blnIsChanged; }
		}

		[System.ComponentModel.Browsable(false)]
		internal override string ServerTypeIconKey
		{
			get { return MainForm.GeorefImageIconKey; }
		}

		[System.ComponentModel.Browsable(false)]
		public override string DisplayIconKey
		{
			get { return MainForm.GeorefImageIconKey; }
		}

		#endregion

		#region ImageBuilder Implementations

		internal override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         iBytesRead = 0;
         iTotalBytes = 0;
         return false;
      }

      internal override RenderableObject GetLayer()
      {
         if (m_blnIsChanged)
         {
            try
            {
               GeographicBoundingBox extents = GeorefImageLayerBuilder.GetExtentsFromGeotif(m_strFileName);
               if (extents != null)
               {
                  // Convert Geotif right here to save lockup in update thread due to slow GDI+ to DirectX texture stream
                  if (!File.Exists(m_strCacheFileName) || File.GetLastWriteTime(m_strCacheFileName) < File.GetLastWriteTime(m_strFileName))
                  {
                     using (Image img = Image.FromFile(m_strFileName))
                     {
                        Directory.CreateDirectory(Path.GetDirectoryName(m_strCacheFileName));
                        img.Save(m_strCacheFileName, System.Drawing.Imaging.ImageFormat.Png);
                     }
                  }

                  m_Layer = new ImageLayer(m_szTreeNodeText, m_oWorldWindow.CurrentWorld, 0.0, m_strCacheFileName, extents.South, extents.North, extents.West, extents.East, m_bOpacity, m_oWorldWindow.CurrentWorld.TerrainAccessor);
                  m_Layer.IsOn = m_IsOn;
                  m_Layer.Opacity = m_bOpacity;
                  m_Layer.RenderPriority = RenderPriority.TerrainMappedImages;
               }
            }
            catch
            {
               if (File.Exists(m_strCacheFileName))
                  File.Delete(m_strCacheFileName);
               m_Layer = null;
            }
            m_blnIsChanged = false;
         }
         return m_Layer;
      }

      internal override string GetURI()
      {
         return URLProtocolName + m_strFileName.Replace(Path.DirectorySeparatorChar, '/');
      }

      internal override string GetCachePath()
      {
			return Path.Combine(Path.Combine(s_strCacheRoot, CacheSubDir), m_strFileName.GetHashCode().ToString(CultureInfo.InvariantCulture));
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_Layer != null)
            m_Layer.Dispose();
         if (File.Exists(m_strCacheFileName))
            File.Delete(m_strCacheFileName);
         if (bFinal && m_bIsTmp && File.Exists(m_strFileName))
            File.Delete(m_strFileName);
         m_Layer = null;
         m_blnIsChanged = true;
      }

      internal override object CloneSpecific()
      {
         return new GeorefImageLayerBuilder(m_strFileName, m_bIsTmp, m_oWorldWindow, m_Parent);
      }

		public override bool Equals(object obj)
      {
         if (!(obj is GeorefImageLayerBuilder)) return false;
         GeorefImageLayerBuilder castObj = obj as GeorefImageLayerBuilder;

         // -- Equal if they're the same local file --
         return this.m_strFileName.Equals(castObj.m_strFileName);
      }

		public override int GetHashCode()
		{
			return m_strFileName.GetHashCode();
		}

      #endregion

      #region Other internal Methods

      /// <summary>
      /// Obtain geographic extents from GeoTIF file
      /// </summary>
      /// <param name="strFile"></param>
      /// <returns>null if not WGS84 or other failure</returns>
      internal static GeographicBoundingBox GetExtentsFromGeotif(string strFile)
      {
         string strTemp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

         try
         {
            ProcessStartInfo psi = new ProcessStartInfo(Path.GetDirectoryName(Application.ExecutablePath) + @"\System\listgeo.exe");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            psi.Arguments = "-d \"" + strFile + "\"";

            using (Process p = Process.Start(psi))
            {
               using (StreamWriter sw = new StreamWriter(strTemp))
               {
                  sw.WriteLine(p.StandardOutput.ReadToEnd());
               }
               p.WaitForExit();
            }

            if (File.Exists(strTemp))
            {
               int iWGS84Check = 0;
               string strUL = "";
               string strLL = "";
               string strUR = "";
               string strLR = "";
               using (StreamReader sr = new StreamReader(strTemp))
               {
                  string strLine;
                  while ((strLine = sr.ReadLine()) != null)
                  {
                     if (strLine.Contains("GTModelTypeGeoKey (Short,1): ModelTypeGeographic"))
                        iWGS84Check++;

                     if (strLine.Contains("GeographicTypeGeoKey (Short,1): "))
                     {
                        foreach (string strGCS in Utility.GCSMappings.GeoTiffWGS84Equivalents)
                        {
                           if (strLine.Contains(strGCS))
                           {
                              iWGS84Check++;
                              break;
                           }
                        }
                     }
                     if (strLine.Contains("Upper Left"))
                        strUL = strLine;
                     if (strLine.Contains("Upper Right"))
                        strUR = strLine;
                     if (strLine.Contains("Lower Left"))
                        strLL = strLine;
                     if (strLine.Contains("Lower Right"))
                        strLR = strLine;
                  }
               }

               if (iWGS84Check >= 2 && strUL.Length > 0 && strLL.Length > 0 && strUR.Length > 0 && strLR.Length > 0)
               {
                  double dWest = Convert.ToDouble(strUL.Substring(strUL.IndexOf('(') + 1).Substring(0, strUL.IndexOf(',') - strUL.IndexOf('(') - 2).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                  double dEast = Convert.ToDouble(strUR.Substring(strUR.IndexOf('(') + 1).Substring(0, strUR.IndexOf(',') - strUR.IndexOf('(') - 2).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                  double dNorth = Convert.ToDouble(strUL.Substring(strUL.IndexOf(',') + 1).Substring(0, strUL.IndexOf(')') - strUL.IndexOf(',') - 2).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                  double dSouth = Convert.ToDouble(strLL.Substring(strLL.IndexOf(',') + 1).Substring(0, strLL.IndexOf(')') - strLL.IndexOf(',') - 2).Trim(), System.Globalization.CultureInfo.InvariantCulture);

                  if (dWest < dEast && dSouth < dNorth)
                     return new GeographicBoundingBox(dNorth, dSouth, dWest, dEast);
               }
            }
         }
         finally
         {
            if (File.Exists(strTemp))
               File.Delete(strTemp);
         }

         return null;
      }

      /// <summary>
      /// Obtain geographic information string from GeoTIF file
      /// </summary>
      /// <param name="strFile"></param>
      /// <returns>empty null if not WGS84 or other failure</returns>
      internal static string GetGeorefInfoFromGeotif(string strFile)
      {
         string strTemp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
         string strReturn = "";
         
         try
         {
            ProcessStartInfo psi = new ProcessStartInfo(Path.GetDirectoryName(Application.ExecutablePath) + @"\System\listgeo.exe");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            psi.Arguments = "-d \"" + strFile + "\"";

            using (Process p = Process.Start(psi))
            {
               using (StreamWriter sw = new StreamWriter(strTemp))
               {
                  sw.WriteLine(p.StandardOutput.ReadToEnd());
               }
               p.WaitForExit();
            }

            if (File.Exists(strTemp))
            {
               string strGCS = "";
               string strProjection = "";
               using (StreamReader sr = new StreamReader(strTemp))
               {
                  string strLine;
                  while ((strLine = sr.ReadLine()) != null)
                  {
                     if (strLine.Contains("GeographicTypeGeoKey (Short,1):"))
                        strGCS = strLine.Replace("GeographicTypeGeoKey (Short,1):", "").Trim();
                     
                     if (String.Compare(strLine, 0, "Projection Method:", 0, "Projection Method:".Length) == 0)
                        strProjection = strLine;
                  }
               }
               if (strGCS.Length > 0)
                  strReturn += strGCS + "\n";
               strReturn += strProjection;
            }
         }
         catch
         {
         }
         finally
         {
            if (File.Exists(strTemp))
               File.Delete(strTemp);
         }

         return strReturn;
      }

      internal override void GetOMMetadata(out String szDownloadType, out String szServerURL, out String szLayerId)
      {
         szDownloadType = "file";
         szServerURL = String.Empty;
         szLayerId = String.Empty;
      }

      #endregion
   }
}

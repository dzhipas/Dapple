using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System;
using WorldWind;
using WorldWind.Renderable;
using WorldWind.VisualControl;
using WorldWind.Net;
using WorldWind.Net.Wms;

namespace WorldWind
{
	public class WMSList
   {
      #region Private Members
      WMSLayer[] _layers;
      string _serverGetCapabilitiesUrl;
      string _serverGetMapUrl;
      string _version;
      string _name;
      #endregion

      #region internal Methods
		public WMSList(string serverGetCapabilitiesUrl, string capabilitiesFilePath)
      {
         bool bConvert = false;

         this._serverGetCapabilitiesUrl = serverGetCapabilitiesUrl;

         try
         {
            this.load_version_1_1_1(capabilitiesFilePath);
            return;
         }
         catch (Exception e)
         {
            if (e is XmlException)
            {
               // Last out convert the catalog to UTF8 and try again
               bConvert = true;
            }
         }
         try
         {
            this.load_version_1_3_0(capabilitiesFilePath);
            return;
         }
         catch (Exception e)
         {
            if (e is XmlException)
            {
               // Last out convert the catalog to UTF8 and try again
               bConvert = true;
            }
            else
               throw e;

         }

         if (bConvert)
         {
            // Convert the catalog to UTF8 and try again
            string strLine;
            string strTemp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            File.Move(capabilitiesFilePath, strTemp);
            using (StreamReader sr = new StreamReader(strTemp, System.Text.Encoding.Default))
            {
               using (StreamWriter sw = new StreamWriter(capabilitiesFilePath, false, System.Text.Encoding.UTF8))
               {
                  for (;;)
                  {
                     strLine = sr.ReadLine();
                     if (strLine == null)
                        break;
                     sw.WriteLine(strLine);
                  }
               }
            }
            File.Delete(strTemp);
         }

         try
         {
            this.load_version_1_1_1(capabilitiesFilePath);
            return;
         }
         catch
         {
         }
         this.load_version_1_3_0(capabilitiesFilePath);
         return;
      }

      internal static string[] GetDatesFromDateTimeString(string dateTimeString)
      {
         System.Collections.ArrayList dates = new ArrayList();
         string[] parsedTimeValues = dateTimeString.Split(',');
         foreach (string s in parsedTimeValues)
         {
            if (s.IndexOf("/") < 0)
            {
               dates.Add(s);
               continue;
            }

            try
            {
               string[] dateList = WMSList.GetTimeValuesFromTimePeriodString(s);
               foreach (string curDate in dateList)
                  dates.Add(curDate + "Z");
            }
            catch //(Exception caught)
            {
               //Utility.Log.Write( caught );
            }
         }

         string[] returnDates = new string[dates.Count];
         for (int i = 0; i < dates.Count; i++)
         {
            returnDates[i] = (string)dates[i];
         }
         return returnDates;
      }

      internal static string[] GetTimeValuesFromTimePeriodString(string timePeriodString)
      {
         string temp = "";
         int numYears = 0;
         int numMonths = 0;
         int numDays = 0;
         double numHours = 0;
         double numMins = 0;
         double numSecs = 0;
         bool isTime = false;

         string[] timePeriodParts = timePeriodString.Split('/');
         if (timePeriodParts.Length != 3)
            throw new ArgumentException("Unexpected time period string: " + timePeriodString);

         string startDateString = timePeriodParts[0];
         string endDateString = timePeriodParts[1];
         DateTime startDate = WMSList.GetDateTimeFromWMSDate(startDateString);
         DateTime endDate = WMSList.GetDateTimeFromWMSDate(endDateString);
         string interval = timePeriodParts[2];
         for (int i = 0; i < interval.Length; i++)
         {
            if (interval[i] == 'Y')
            {
               numYears = Int32.Parse(temp, CultureInfo.InvariantCulture);
               temp = "";
            }
            else if (interval[i] == 'M' && !isTime)
            {
               numMonths = Int32.Parse(temp, CultureInfo.InvariantCulture);
               temp = "";
            }
            else if (interval[i] == 'D')
            {
               numDays = Int32.Parse(temp, CultureInfo.InvariantCulture);
               temp = "";
            }
            else if (interval[i] == 'H')
            {
               numHours = Double.Parse(temp, CultureInfo.InvariantCulture);
               temp = "";
            }
            else if (interval[i] == 'M' && isTime)
            {
               numMins = Double.Parse(temp, CultureInfo.InvariantCulture);
               temp = "";
            }
            else if (interval[i] == 'S')
            {
               numSecs = Double.Parse(temp, CultureInfo.InvariantCulture);
               temp = "";
            }
            else if (interval[i] == 'T')
            {
               isTime = true;
            }
            else if (interval[i] != 'P')
            {
               temp += interval[i];
            }
         }

         StringCollection dateList = new StringCollection();
         while (startDate <= endDate)
         {
            // Year-Month
            string curDate = string.Format("{0:0000}-", startDate.Year);
            if (startDateString.Length > 5)
               curDate += string.Format(startDate.Month.ToString("00"));
            if (startDateString.Length > 7)
               // Add day
               curDate += "-" + startDate.Day.ToString("00");
            if (startDateString.Length > 10)
            {
               // Add hours+minutes
               curDate += string.Format("T{0:00}:{1:00}",
                  startDate.Hour,
                  startDate.Minute);
               if (startDateString.Length > 17)
                  // Add seconds
                  curDate += ":" + startDate.Second.ToString("00");
            }
            dateList.Add(curDate);
            startDate = startDate.AddYears(numYears);
            startDate = startDate.AddMonths(numMonths);
            startDate = startDate.AddDays(numDays);
            startDate = startDate.AddHours(numHours);
            startDate = startDate.AddMinutes(numMins);
            startDate = startDate.AddSeconds(numSecs);
         }

         string[] res = new string[dateList.Count];
         dateList.CopyTo(res, 0);
         return res;
      }

      /// <summary>
      /// Parses WMS string dates.
      /// </summary>
      /// <param name="wmsDate">Input WMS date string.</param>
      /// <returns>Time converted to DateTime or DateTime.MinValue if date string is incorrect format.</returns>
      internal static DateTime GetDateTimeFromWMSDate(string wmsDate)
      {
         // result = UTC (not local)
         DateTime result = DateTime.ParseExact(wmsDate,
            new string[] {
											 "yyyy-MM-ddTHH:mm:ssZ",
											 "yyyy-MM-ddTHH:mmZ",
											 "yyyy-MM-ddTHHZ",
											 "yyyy-MM-dd",
											 "yyyy-MM",
											 "yyyy-"},
            null, DateTimeStyles.AdjustToUniversal);
         return result;
      }

      #endregion

      #region Private Methods
      private void load_version_1_1_1(string capabilitiesFilePath)
      {
         WMS_MS_Capabilities.WMS_MS_CapabilitiesDoc doc = new WMS_MS_Capabilities.WMS_MS_CapabilitiesDoc();
         XmlReaderSettings oSettings = new System.Xml.XmlReaderSettings();
         oSettings.IgnoreWhitespace = true;
         oSettings.ProhibitDtd = false;
         oSettings.XmlResolver = null;
         oSettings.ValidationType = ValidationType.None;
         using (XmlReader oResponseXmlStream = XmlReader.Create(capabilitiesFilePath, oSettings))
         {
            WMS_MS_Capabilities.WMT_MS_CapabilitiesType root = new WMS_MS_Capabilities.WMT_MS_CapabilitiesType(doc.Load(oResponseXmlStream));

				if (root.versionCount > 0)
				{
					this._version = root.version.Value;
				}
				else
				{
					this._version = "1.1.1";
				}
            this._name = root.Service.Title.Value.Value;

            if (!root.HasCapability())
               return;

            if (!root.Capability.HasLayer())
               return;

            string[] imageFormats = null;
            if (root.Capability.HasRequest() &&
               root.Capability.Request.HasGetMap())
            {
               if (root.Capability.Request.GetMap.DCPType.HTTP.Get.HasOnlineResource())
               {
                  System.Xml.XmlNode hrefnode = root.Capability.Request.GetMap.DCPType.HTTP.Get.OnlineResource.getDOMNode();
                  System.Xml.XmlAttribute attr = hrefnode.Attributes["xlink:href"];
                  if (attr != null)
                     this._serverGetMapUrl = attr.InnerText;
                  else
                     this._serverGetMapUrl = this._serverGetCapabilitiesUrl;
               }
               else
                  this._serverGetMapUrl = this._serverGetCapabilitiesUrl;

               if (root.Capability.Request.GetMap.HasFormat())
               {
                  imageFormats = new string[root.Capability.Request.GetMap.FormatCount];
                  for (int i = 0; i < root.Capability.Request.GetMap.FormatCount; i++)
                  {
                     imageFormats[i] = root.Capability.Request.GetMap.GetFormatAt(i).Value.Value;
                  }
               }
            }
            else
               this._serverGetMapUrl = this._serverGetCapabilitiesUrl;

            this.Layers = new WMSLayer[root.Capability.LayerCount];

            for (int i = 0; i < root.Capability.LayerCount; i++)
            {
               WMS_MS_Capabilities.LayerType curLayer = (WMS_MS_Capabilities.LayerType)root.Capability.GetLayerAt(i);
               this.Layers[i] = this.getWMSLayer(curLayer, null, imageFormats);
            }
         }
      }

      private WMSLayer getWMSLayer(WMS_MS_Capabilities.LayerType layer, WMS_MS_Capabilities.LatLonBoundingBoxType parentLatLonBoundingBox, string[] imageFormats)
      {
         WMSLayer wmsLayer = new WMSLayer();
         wmsLayer.ParentWMSList = this;
         wmsLayer.ImageFormats = imageFormats;

         if (layer.HasName())
            wmsLayer.Name = layer.Name.Value.Value;

         if (layer.HasTitle())
            wmsLayer.Title = layer.Title.Value.Value;

         if (layer.HasAbstract2())
            wmsLayer.Description = layer.Abstract2.Value.Value;

         if (layer.HasSRS())
            wmsLayer.SRS = layer.SRS.Value.Value;

         if (layer.HasExtent())
         {
            for (int i = WMS_MS_Capabilities.LayerType.ExtentMinCount; i < layer.ExtentCount; i++)
            {
               WMS_MS_Capabilities.ExtentType curExtent = layer.GetExtentAt(i);
               if (curExtent.Hasname())
               {
                  if (String.Compare(curExtent.name.Value, "time", true) == 0)
                  {
                     WMSList.GetDatesFromDateTimeString(curExtent.getDOMNode().InnerText);
                  }
               }
            }
         }

         if (layer.HasLatLonBoundingBox())
         {
            wmsLayer.North = Convert.ToDecimal(Double.Parse(layer.LatLonBoundingBox.maxy.Value, CultureInfo.InvariantCulture));
            wmsLayer.South = Convert.ToDecimal(Double.Parse(layer.LatLonBoundingBox.miny.Value, CultureInfo.InvariantCulture));
            wmsLayer.West = Convert.ToDecimal(Double.Parse(layer.LatLonBoundingBox.minx.Value, CultureInfo.InvariantCulture));
            wmsLayer.East = Convert.ToDecimal(Double.Parse(layer.LatLonBoundingBox.maxx.Value, CultureInfo.InvariantCulture));
            parentLatLonBoundingBox = layer.LatLonBoundingBox;
         }
         else if (parentLatLonBoundingBox != null)
         {
            wmsLayer.North = Convert.ToDecimal(Double.Parse(parentLatLonBoundingBox.maxy.Value, CultureInfo.InvariantCulture));
            wmsLayer.South = Convert.ToDecimal(Double.Parse(parentLatLonBoundingBox.miny.Value, CultureInfo.InvariantCulture));
            wmsLayer.West = Convert.ToDecimal(Double.Parse(parentLatLonBoundingBox.minx.Value, CultureInfo.InvariantCulture));
            wmsLayer.East = Convert.ToDecimal(Double.Parse(parentLatLonBoundingBox.maxx.Value, CultureInfo.InvariantCulture));
         }

         if (layer.HasStyle())
         {
            wmsLayer.Styles = new WMSLayerStyle[layer.StyleCount];
            for (int i = 0; i < layer.StyleCount; i++)
            {
               WMS_MS_Capabilities.StyleType curStyle = layer.GetStyleAt(i);

               wmsLayer.Styles[i] = new WMSLayerStyle();

               if (curStyle.HasTitle())
                  wmsLayer.Styles[i].title = curStyle.Title.Value.Value;

               if (curStyle.HasLegendURL())
               {
                  wmsLayer.Styles[i].legendURL = new WMSLayerStyleLegendURL[curStyle.LegendURLCount];

                  for (int j = 0; j < curStyle.LegendURLCount; j++)
                  {
                     WMS_MS_Capabilities.LegendURLType curLegend = curStyle.GetLegendURLAt(j);

                     wmsLayer.Styles[i].legendURL[j] = new WMSLayerStyleLegendURL();

                     if (curLegend.HasOnlineResource())
                     {
                        System.Xml.XmlNode n = curLegend.OnlineResource.getDOMNode();

                        foreach (System.Xml.XmlAttribute attr in n.Attributes)
                        {
                           if (attr.Name.IndexOf("href") >= 0)
                           {
                              wmsLayer.Styles[i].legendURL[j].href = attr.InnerText;
                           }
                        }
                     }
                  }
               }
            }
         }

         if (layer.HasLayer())
         {
            wmsLayer.ChildLayers = new WMSLayer[layer.LayerCount];
            for (int i = 0; i < layer.LayerCount; i++)
            {
               wmsLayer.ChildLayers[i] = this.getWMSLayer((WMS_MS_Capabilities.LayerType)layer.GetLayerAt(i), parentLatLonBoundingBox, imageFormats);
            }
         }

         return wmsLayer;
      }

      private void load_version_1_3_0(string capabilitiesFilePath)
      {
         capabilities_1_3_0.capabilities_1_3_0Doc doc = new capabilities_1_3_0.capabilities_1_3_0Doc();
         XmlReaderSettings oSettings = new System.Xml.XmlReaderSettings();
         oSettings.IgnoreWhitespace = true;
         oSettings.ProhibitDtd = false;
         oSettings.XmlResolver = null;
         oSettings.ValidationType = ValidationType.None;
         using (XmlReader oResponseXmlStream = XmlReader.Create(capabilitiesFilePath, oSettings))
         {
            capabilities_1_3_0.wms.WMS_CapabilitiesType root = new capabilities_1_3_0.wms.WMS_CapabilitiesType(doc.Load(oResponseXmlStream));

				if (root.versionCount > 0)
				{
					this._version = root.version.Value;
				}
				else
				{
					this._version = "1.3.0";
				}
            this._name = root.Service.Title.Value;

            if (!root.HasCapability())
               return;

            if (!root.Capability.HasLayer())
               return;

            string[] imageFormats = null;
            if (root.Capability.HasRequest() &&
               root.Capability.Request.HasGetMap())
            {
               if (root.Capability.Request.GetMap.HasDCPType())
               {
                  this._serverGetMapUrl = root.Capability.Request.GetMap.DCPType.HTTP.Get.OnlineResource.href.Value;
               }
               else
                  this._serverGetMapUrl = this._serverGetCapabilitiesUrl;

               if (root.Capability.Request.GetMap.HasFormat())
               {
                  imageFormats = new string[root.Capability.Request.GetMap.FormatCount];
                  for (int i = 0; i < root.Capability.Request.GetMap.FormatCount; i++)
                  {
                     imageFormats[i] = root.Capability.Request.GetMap.GetFormatAt(i).Value;
                  }
               }
            }

            this.Layers = new WMSLayer[root.Capability.LayerCount];

            for (int i = 0; i < root.Capability.LayerCount; i++)
            {
               capabilities_1_3_0.wms.LayerType curLayer = (capabilities_1_3_0.wms.LayerType)root.Capability.GetLayerAt(i);
               this.Layers[i] = this.getWMSLayer(curLayer, null, imageFormats);
            }
         }
      }

      private WMSLayer getWMSLayer(capabilities_1_3_0.wms.LayerType layer, capabilities_1_3_0.wms.EX_GeographicBoundingBoxType parentLatLonBoundingBox, string[] imageFormats)
      {
         WMSLayer wmsLayer = new WMSLayer();

         wmsLayer.ParentWMSList = this;
         wmsLayer.ImageFormats = imageFormats;

         if (layer.HasName())
            wmsLayer.Name = layer.Name.Value;

         if (layer.HasTitle())
            wmsLayer.Title = layer.Title.Value;

         if (layer.HasAbstract2())
            wmsLayer.Description = layer.Abstract2.Value;

         if (layer.HasCRS())
            wmsLayer.CRS = layer.CRS.Value;

         if (layer.HasDimension())
         {
            for (int i = capabilities_1_3_0.wms.LayerType.DimensionMinCount; i < layer.DimensionCount; i++)
            {
               capabilities_1_3_0.wms.DimensionType curDimension = layer.GetDimensionAt(i);
               if (curDimension.Hasname())
               {
                  if (String.Compare(layer.Dimension.name.Value, "time", true, CultureInfo.InvariantCulture) == 0)
                  {
                     WMSList.GetDatesFromDateTimeString(curDimension.Value.Value);
                  }
               }
            }
         }

         if (layer.HasEX_GeographicBoundingBox())
         {
            wmsLayer.North = (decimal)layer.EX_GeographicBoundingBox.northBoundLatitude.Value;
            wmsLayer.South = (decimal)layer.EX_GeographicBoundingBox.southBoundLatitude.Value;
            wmsLayer.East = (decimal)layer.EX_GeographicBoundingBox.eastBoundLongitude.Value;
            wmsLayer.West = (decimal)layer.EX_GeographicBoundingBox.westBoundLongitude.Value;
         }
         else if (parentLatLonBoundingBox != null)
         {
            wmsLayer.North = (decimal)parentLatLonBoundingBox.northBoundLatitude.Value;
            wmsLayer.South = (decimal)parentLatLonBoundingBox.southBoundLatitude.Value;
            wmsLayer.West = (decimal)parentLatLonBoundingBox.westBoundLongitude.Value;
            wmsLayer.East = (decimal)parentLatLonBoundingBox.eastBoundLongitude.Value;
         }

         if (layer.HasStyle())
         {
            wmsLayer.Styles = new WMSLayerStyle[layer.StyleCount];
            for (int i = capabilities_1_3_0.wms.LayerType.StyleMinCount; i < layer.StyleCount; i++)
            {
               capabilities_1_3_0.wms.StyleType curStyle = layer.GetStyleAt(i);

               wmsLayer.Styles[i] = new WMSLayerStyle();

               if (curStyle.HasTitle())
                  wmsLayer.Styles[i].title = curStyle.Title.Value;

               if (curStyle.HasLegendURL())
               {
                  wmsLayer.Styles[i].legendURL = new WMSLayerStyleLegendURL[curStyle.LegendURLCount];

                  for (int j = 0; j < curStyle.LegendURLCount; j++)
                  {
                     capabilities_1_3_0.wms.LegendURLType curLegend = curStyle.GetLegendURLAt(j);
                     wmsLayer.Styles[i].legendURL[j] = new WMSLayerStyleLegendURL();

                     if (curLegend.HasOnlineResource())
                     {
                        if (curLegend.OnlineResource.Hashref())
                           wmsLayer.Styles[i].legendURL[j].href = curLegend.OnlineResource.href.Value;
                     }
                  }
               }
            }
         }

         if (layer.HasLayer())
         {
            wmsLayer.ChildLayers = new WMSLayer[layer.LayerCount];
            for (int i = 0; i < layer.LayerCount; i++)
            {
               wmsLayer.ChildLayers[i] = this.getWMSLayer((capabilities_1_3_0.wms.LayerType)layer.GetLayerAt(i), parentLatLonBoundingBox, imageFormats);
            }
         }

         return wmsLayer;
      }

      #endregion

      #region Properties
		public WMSLayer[] Layers
      {
         get
         {
            return this._layers;
         }
         set
         {
            this._layers = value;
         }
      }
		public string Name
      {
         get
         {
            return this._name;
         }
         set
         {
            this._name = value;
         }
      }
		public string ServerGetCapabilitiesUrl
      {
         get
         {
            return this._serverGetCapabilitiesUrl;
         }
         set
         {
            this._serverGetCapabilitiesUrl = value;
         }
      }
		public string ServerGetMapUrl
      {
         get
         {
            return this._serverGetMapUrl;
         }
         set
         {
            this._serverGetMapUrl = value;
         }
      }
		public string Version
      {
         get
         {
            return this._version;
         }
         set
         {
            this._version = value;
         }
      }
      #endregion
   }
}

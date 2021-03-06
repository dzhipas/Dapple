using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;
using WorldWind.Camera;
using WorldWind.Configuration;
using WorldWind.Net;
using WorldWind.Terrain;
using WorldWind.VisualControl;
using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using Utility;

namespace WorldWind.Renderable
{
   /// <summary>
   /// Main class for image tile rendering.  Uses the Terrain Manager to query height values for 3D 
   /// terrain rendering.
   /// Relies on an Update thread to refresh the "tiles" based on lat/lon/view range
   /// </summary>
	public class QuadTileSet : RenderableObject, IGeoSpatialDownloadTileSet
   {
		readonly Color DownloadQueuedColor = Color.FromArgb(50, 128, 168, 128);
		readonly Color DownloadLogoColor = Color.FromArgb(180, 255, 255, 255);
		const int MaxSimultaneousDownloads = 1;

      #region Private Members

      protected string m_ServerLogoFilePath;
      protected Image m_ServerLogoImage;

      protected Dictionary<long, QuadTile> m_topmostTiles = new Dictionary<long, QuadTile>();

      protected double m_north;
      protected double m_south;
      protected double m_west;
      protected double m_east;

      protected Texture m_iconTexture;
      protected Sprite sprite;
      protected Rectangle m_spriteSize;
      protected ProgressBar progressBar;

      protected Blend m_sourceBlend = Blend.BlendFactor;
      protected Blend m_destinationBlend = Blend.InvBlendFactor;

      // If this value equals CurrentFrameStartTicks the Z buffer needs to be cleared
      protected static long lastRenderTime;

      //internal static int MaxConcurrentDownloads = 3;
      protected double m_layerRadius;
      protected bool m_alwaysRenderBaseTiles;
      protected float m_tileDrawSpread;
      protected float m_tileDrawDistance;
      protected bool m_isDownloadingElevation;
      protected int m_numberRetries;
      protected Dictionary<IGeoSpatialDownloadTile, GeoSpatialDownloadRequest> m_downloadRequests = new Dictionary<IGeoSpatialDownloadTile, GeoSpatialDownloadRequest>();
      protected int m_maxQueueSize = 400;
      protected bool m_terrainMapped;
      protected ImageStore[] m_imageStores;
      protected Camera.CameraBase m_camera;
      protected GeoSpatialDownloadRequest[] m_activeDownloads = new GeoSpatialDownloadRequest[20];
      protected DateTime[] m_downloadStarted = new DateTime[20];
      protected TimeSpan m_connectionWaitTime = TimeSpan.FromMinutes(2);
      protected DateTime m_connectionWaitStart;
      protected bool m_isConnectionWaiting;
      protected bool m_enableColorKeying;

      protected Effect m_effect = null;
      protected bool m_effectEnabled = true;
      protected string m_effectPath = null;
      protected string m_effectTechnique = null;
      protected Dictionary<string, EffectHandle> m_effectHandles = null;
      static protected EffectPool m_effectPool = new EffectPool();


      protected TimeSpan m_cacheExpirationTime = TimeSpan.MaxValue;

      #endregion

      #region internal members

      /// <summary>
      /// Texture showing download in progress
      /// </summary>
      internal static Texture DownloadInProgressTexture;

      /// <summary>
      /// Texture showing queued download
      /// </summary>
      internal static Texture DownloadQueuedTexture;

      /// <summary>
      /// Texture showing terrain download in progress
      /// </summary>
      internal static Texture DownloadTerrainTexture;


      private int colorKey;
      private int colorKeyMax;

      /// <summary>
      /// If a color range is to be transparent this specifies the brightest transparent color.
      /// The darkest transparent color is set using ColorKey.
      /// </summary>
		public int ColorKey
      {
         get
         {
            return colorKey;
         }
         set
         {
            colorKey = value;
         }
      }

      /// <summary>
      /// default: 100% transparent black = transparent
      /// </summary>
		public int ColorKeyMax
      {
         get
         {
            return colorKeyMax;
         }
         set
         {
            colorKeyMax = value;
         }
      }


      #endregion

      internal float GrayscaleBrightness
      {
         get { return 0.0f; }
      }

      internal const bool RenderGrayscale = false;

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.QuadTileSet"/> class.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="parentWorld"></param>
      /// <param name="distanceAboveSurface"></param>
      /// <param name="north"></param>
      /// <param name="south"></param>
      /// <param name="west"></param>
      /// <param name="east"></param>
      /// <param name="terrainAccessor"></param>
      /// <param name="imageAccessor"></param>
		public QuadTileSet(
         string name,
         World parentWorld,
         double distanceAboveSurface,
         double north,
         double south,
         double west,
         double east,
         bool terrainMapped,
                  ImageStore[] imageStores)
         : base(name, parentWorld)
      {
         float layerRadius = (float)(parentWorld.EquatorialRadius + distanceAboveSurface);
         m_north = north;
         m_south = south;
         m_west = west;
         m_east = east;

         // Layer center position
         Position = MathEngine.SphericalToCartesian(
            (north + south) * 0.5f,
            (west + east) * 0.5f,
            layerRadius);

         m_layerRadius = layerRadius;
         m_tileDrawDistance = 3.5f;
         m_tileDrawSpread = 2.9f;
         m_imageStores = imageStores;
         m_terrainMapped = terrainMapped;

			// Default terrain mapped imagery to terrain mapped priority 
			if (terrainMapped)
				m_renderPriority = RenderPriority.TerrainMappedImages;
      }

      #region internal properties

      /// <summary>
      /// If images in cache are older than expration time a refresh
      /// from server will be attempted.
      /// </summary>
		public TimeSpan CacheExpirationTime
      {
         get
         {
            return this.m_cacheExpirationTime;
         }
         set
         {
            this.m_cacheExpirationTime = value;
         }
      }

      /// <summary>
      /// Path to a thumbnail image (e.g. for use as a download indicator).
      /// </summary>
		public virtual string ServerLogoFilePath
      {
         get
         {
            return m_ServerLogoFilePath;
         }
         set
         {
            m_ServerLogoFilePath = value;
         }
      }

      /// <summary>
      /// The image referenced by ServerLogoFilePath.
      /// </summary>
      internal virtual Image ServerLogoImage
      {
         get
         {
            if (m_ServerLogoImage == null)
            {
               if (m_ServerLogoFilePath == null)
                  return null;
               try
               {
                  if (File.Exists(m_ServerLogoFilePath))
                     m_ServerLogoImage = ImageHelper.LoadImage(m_ServerLogoFilePath);
               }
               catch { }
            }
            return m_ServerLogoImage;
         }
      }

      /// <summary>
      /// Path to a thumbnail image (or download indicator if none available)
      /// </summary>
      internal override Image ThumbnailImage
      {
         get
         {
            if (base.ThumbnailImage != null)
               return base.ThumbnailImage;

            return ServerLogoImage;
         }
      }

      /// <summary>
      /// North bound for this QuadTileSet
      /// </summary>
      public double North
      {
         get
         {
            return m_north;
         }
      }

      /// <summary>
      /// West bound for this QuadTileSet
      /// </summary>
		public double West
      {
         get
         {
            return m_west;
         }
      }

      /// <summary>
      /// South bound for this QuadTileSet
      /// </summary>
		public double South
      {
         get
         {
            return m_south;
         }
      }

      /// <summary>
      /// East bound for this QuadTileSet
      /// </summary>
		public double East
      {
         get
         {
            return m_east;
         }
      }

      internal DateTime ConnectionWaitStart
      {
         get
         {
            return m_connectionWaitStart;
         }
      }

      internal bool IsConnectionWaiting
      {
         get
         {
            return m_isConnectionWaiting;
         }
      }

      internal double LayerRadius
      {
         get
         {
            return m_layerRadius;
         }
      }

		public bool AlwaysRenderBaseTiles
      {
         get
         {
            return m_alwaysRenderBaseTiles;
         }
         set
         {
            m_alwaysRenderBaseTiles = value;
         }
      }

      internal float TileDrawSpread
      {
         get
         {
            return m_tileDrawSpread;
         }
      }

      internal float TileDrawDistance
      {
         get
         {
            return m_tileDrawDistance;
         }
      }

      internal bool IsDownloadingElevation
      {
         set
         {
            m_isDownloadingElevation = value;
         }
      }

		public int NumberRetries
      {
         get
         {
            return m_numberRetries;
         }
         set
         {
            m_numberRetries = value;
         }
      }

      /// <summary>
      /// Controls rendering (flat or terrain mapped)
      /// </summary>
      internal bool TerrainMapped
      {
         get { return m_terrainMapped; }
      }

      internal ImageStore[] ImageStores
      {
         get
         {
            return m_imageStores;
         }
      }

      /// <summary>
      /// The camera controlling the layers update logic
      /// </summary>
		public CameraBase Camera
      {
         get
         {
            return m_camera;
         }
         set
         {
            m_camera = value;
         }
      }

      /// <summary>
      /// The effect used to render this tileset.
      /// </summary>
      internal Effect Effect
      {
         get
         {
            return m_effect;
         }
         set
         {
            m_effect = value;
         }
      }

      internal Dictionary<string, EffectHandle> EffectParameters
      {
         get
         {
            return m_effectHandles;
         }
      }

      #endregion

      public override void Initialize(DrawArgs drawArgs)
      {
         Camera = DrawArgs.Camera;

         // Initialize download rectangles
         if (DownloadInProgressTexture == null)
            DownloadInProgressTexture = CreateDownloadRectangle(
                  DrawArgs.Device, World.Settings.DownloadProgressColor, 0);
         if (DownloadQueuedTexture == null)
            DownloadQueuedTexture = CreateDownloadRectangle(
                  DrawArgs.Device, DownloadQueuedColor, 0);
         if (DownloadTerrainTexture == null)
            DownloadTerrainTexture = CreateDownloadRectangle(
                  DrawArgs.Device, World.Settings.DownloadTerrainRectangleColor, 0);

         try
         {
            lock (((System.Collections.IDictionary)m_topmostTiles).SyncRoot)
            {
               foreach (QuadTile qt in m_topmostTiles.Values)
                  qt.Initialize();
            }
         }
         catch
         {
         }
         isInitialized = true;


         if (MetaData.ContainsKey("EffectPath"))
         {
            m_effectPath = MetaData["EffectPath"] as string;
         }
         else
         {
            m_effectPath = null;
         }
         m_effect = null;
         if (m_effectHandles == null)
            m_effectHandles = new Dictionary<string, EffectHandle>();
      }

		public override void Update(DrawArgs drawArgs)
		{
			if (!System.Threading.Thread.CurrentThread.Name.Equals(ThreadNames.WorldWindowBackground))
				throw new System.InvalidOperationException("QTS.Update() must be called from WorkerThread!");

			if (!isInitialized)
				Initialize(drawArgs);

			ServiceDownloadQueue();

			if (m_effectEnabled && (m_effectPath != null) && !File.Exists(m_effectPath))
			{
				Log.Write(Log.Levels.Warning, string.Format("Effect {0} not found - disabled", m_effectPath));
				m_effectEnabled = false;
			}

			if (m_effectEnabled && m_effectPath != null && m_effect == null)
			{
				string errs = string.Empty;

				m_effectHandles.Clear();

				try
				{
					Log.Write(Log.Levels.Warning, string.Format("Loading effect from {0}", m_effectPath));
					m_effect = Effect.FromFile(DrawArgs.Device, m_effectPath, null, "", ShaderFlags.None, m_effectPool, out errs);

					// locate effect handles and store for rendering.
					m_effectHandles.Add("WorldViewProj", m_effect.GetParameter(null, "WorldViewProj"));
					m_effectHandles.Add("World", m_effect.GetParameter(null, "World"));
					m_effectHandles.Add("ViewInverse", m_effect.GetParameter(null, "ViewInverse"));
					for (int i = 0; i < 8; i++)
					{
						string name = string.Format("Tex{0}", i);
						m_effectHandles.Add(name, m_effect.GetParameter(null, name));
					}
					m_effectHandles.Add("Brightness", m_effect.GetParameter(null, "Brightness"));
					m_effectHandles.Add("Opacity", m_effect.GetParameter(null, "Opacity"));
					m_effectHandles.Add("TileLevel", m_effect.GetParameter(null, "TileLevel"));
					m_effectHandles.Add("LightDirection", m_effect.GetParameter(null, "LightDirection"));
					m_effectHandles.Add("LocalOrigin", m_effect.GetParameter(null, "LocalOrigin"));
					m_effectHandles.Add("LayerRadius", m_effect.GetParameter(null, "LayerRadius"));
					m_effectHandles.Add("LocalFrameOrigin", m_effect.GetParameter(null, "LocalFrameOrigin"));
					m_effectHandles.Add("LocalFrameXAxis", m_effect.GetParameter(null, "LocalFrameXAxis"));
					m_effectHandles.Add("LocalFrameYAxis", m_effect.GetParameter(null, "LocalFrameYAxis"));
					m_effectHandles.Add("LocalFrameZAxis", m_effect.GetParameter(null, "LocalFrameZAxis"));
				}
				catch (Exception ex)
				{
					Log.Write(Log.Levels.Error, "Effect load caused exception:" + ex.ToString());
					Log.Write(Log.Levels.Warning, "Effect has been disabled.");
					m_effectEnabled = false;
				}

				if (errs != null && errs != string.Empty)
				{
					Log.Write(Log.Levels.Warning, "Could not load effect " + m_effectPath + ": " + errs);
					Log.Write(Log.Levels.Warning, "Effect has been disabled.");
					m_effectEnabled = false;
					m_effect = null;
				}
			}

			if (ImageStores[0].LevelZeroTileSizeDegrees < 180)
			{
				// Check for layer outside view
				double vrd = DrawArgs.Camera.ViewRange.Degrees;
				double latitudeMax = DrawArgs.Camera.Latitude.Degrees + vrd;
				double latitudeMin = DrawArgs.Camera.Latitude.Degrees - vrd;
				double longitudeMax = DrawArgs.Camera.Longitude.Degrees + vrd;
				double longitudeMin = DrawArgs.Camera.Longitude.Degrees - vrd;
				if (latitudeMax < m_south || latitudeMin > m_north || longitudeMax < m_west || longitudeMin > m_east)
					return;
			}

			if (!m_alwaysRenderBaseTiles && DrawArgs.Camera.ViewRange * 0.5f >
					Angle.FromDegrees(TileDrawDistance * ImageStores[0].LevelZeroTileSizeDegrees))
			{
				lock (((System.Collections.IDictionary)m_topmostTiles).SyncRoot)
				{
					// Don't dispose of the quadtiles like WorldWind does here (they may be nice to look at)
					// Do however clear the download requests
					/*
					foreach (QuadTile qt in m_topmostTiles.Values)
						qt.Dispose();
					m_topmostTiles.Clear();
					 */
					ClearDownloadRequests();
				}

				return;
			}

			// 'Spiral' from the centre tile outward adding tiles that's in the view
			// Defer the updates to after the loop to prevent tiles from updating twice
			// If the tilespread is huge we are likely looking at a small dataset in the view 
			// so just test all the tiles in the dataset.
			int iTileSpread = Math.Max(5, (int)Math.Ceiling(drawArgs.WorldCamera.TrueViewRange.Degrees / (2.0 * ImageStores[0].LevelZeroTileSizeDegrees)));

			int iMiddleRow, iMiddleCol;
			double dRowInc = ImageStores[0].LevelZeroTileSizeDegrees;
			double dColInc = ImageStores[0].LevelZeroTileSizeDegrees;

			if (iTileSpread > 10)
			{
				iTileSpread = Math.Max(5, (int)Math.Ceiling(Math.Max(North - South, East - West) / (2.0 * ImageStores[0].LevelZeroTileSizeDegrees)));
				iMiddleRow = MathEngine.GetRowFromLatitude(South + (North - South) / 2.0, ImageStores[0].LevelZeroTileSizeDegrees);
				iMiddleCol = MathEngine.GetColFromLongitude(West + (East - West) / 2.0, ImageStores[0].LevelZeroTileSizeDegrees);
			}
			else
			{
				iMiddleRow = MathEngine.GetRowFromLatitude(drawArgs.WorldCamera.Latitude, ImageStores[0].LevelZeroTileSizeDegrees);
				iMiddleCol = MathEngine.GetColFromLongitude(drawArgs.WorldCamera.Longitude, ImageStores[0].LevelZeroTileSizeDegrees);
			}


			// --- Calculate the bounding box of the middle tile, and from this, its latitude and longitude size ---
			double dMiddleSouth = -90.0f + iMiddleRow * dRowInc;
			double dMiddleNorth = -90.0f + iMiddleRow * dRowInc + dRowInc;
			double dMiddleWest = -180.0f + iMiddleCol * dColInc;
			double dMiddleEast = -180.0f + iMiddleCol * dColInc + dColInc;

			double dMiddleCenterLat = 0.5f * (dMiddleNorth + dMiddleSouth);
			double dMiddleCenterLon = 0.5f * (dMiddleWest + dMiddleEast);

			Dictionary<long, QuadTile> oTilesToUpdate = new Dictionary<long, QuadTile>();

			// --- Create tiles radially outward from the center tile ---

			for (int iSpread = 0; iSpread < iTileSpread; iSpread++)
			{
				for (double dNewTileCenterLat = dMiddleCenterLat - iSpread * dRowInc; dNewTileCenterLat < dMiddleCenterLat + iSpread * dRowInc; dNewTileCenterLat += dRowInc)
				{
					for (double dNewTileCenterLon = dMiddleCenterLon - iSpread * dColInc; dNewTileCenterLon < dMiddleCenterLon + iSpread * dColInc; dNewTileCenterLon += dColInc)
					{
						QuadTile qt;
						int iCurRow = MathEngine.GetRowFromLatitude(Angle.FromDegrees(dNewTileCenterLat), dRowInc);
						int iCurCol = MathEngine.GetColFromLongitude(Angle.FromDegrees(dNewTileCenterLon), dColInc);

						long lKey = ((long)iCurRow << 32) + iCurCol;  // Index keys by row-col packed into a single long

						if (m_topmostTiles.ContainsKey(lKey))
						{
							qt = m_topmostTiles[lKey];
							if (!oTilesToUpdate.ContainsKey(lKey))
								oTilesToUpdate.Add(lKey, qt);
							continue;
						}

						// Check for tile outside layer boundaries
						double west = -180.0 + iCurCol * dColInc;
						if (west > m_east)
							continue;

						double east = west + dColInc;
						if (east < m_west)
							continue;

						double south = -90.0 + iCurRow * dRowInc;
						if (south > m_north)
							continue;

						double north = south + dRowInc;
						if (north < m_south)
							continue;

						qt = new QuadTile(south, north, west, east, 0, this);
						if (DrawArgs.Camera.ViewFrustum.Intersects(qt.BoundingBox))
						{
							lock (((System.Collections.IDictionary)m_topmostTiles).SyncRoot)
							{
								m_topmostTiles.Add(lKey, qt);
							}

							if (!oTilesToUpdate.ContainsKey(lKey))
							{
								oTilesToUpdate.Add(lKey, qt);
							}
						}
					}
				}
			}

			List<long> oTileIndicesToDelete = new List<long>();
			foreach (long key in m_topmostTiles.Keys)
			{
				QuadTile qt = (QuadTile)m_topmostTiles[key];
				if (!drawArgs.WorldCamera.ViewFrustum.Intersects(qt.BoundingBox))
				{
					if (oTilesToUpdate.ContainsKey(key))
						oTilesToUpdate.Remove(key);
					oTileIndicesToDelete.Add(key);
				}
			}
			// Do updates before cleanup for performance reasons.

			foreach (long key in oTilesToUpdate.Keys)
				m_topmostTiles[key].Update(drawArgs);

			lock (((System.Collections.IDictionary)m_topmostTiles).SyncRoot)
			{
				foreach (long key in oTileIndicesToDelete)
				{
					if (m_topmostTiles.ContainsKey(key))
					{
						QuadTile qt = (QuadTile)m_topmostTiles[key];
						m_topmostTiles.Remove(key);
						qt.Dispose();
					}
				}
			}
		}

      public override void Render(DrawArgs drawArgs)
      {
         try
         {
            lock (((System.Collections.IDictionary)m_topmostTiles).SyncRoot)
            {
               if (m_topmostTiles.Count <= 0)
               {
                  return;
               }

               Device device = DrawArgs.Device;

               // Temporary fix: Clear Z buffer between rendering
               // terrain mapped layers to avoid Z buffer fighting
               //if (lastRenderTime == DrawArgs.CurrentFrameStartTicks)
               device.Clear(ClearFlags.ZBuffer, 0, 1.0f, 0);
               device.RenderState.ZBufferEnable = true;
               lastRenderTime = DrawArgs.CurrentFrameStartTicks;

               //							  if (m_renderPriority < RenderPriority.TerrainMappedImages)
               //									  // No Z buffering needed for "flat" layers
               //									  device.RenderState.ZBufferEnable = false;


               /*	  if (m_opacity < 255 && device.DeviceCaps.DestinationBlendCaps.SupportsBlendFactor)
                     {
                           // Blend
                           device.RenderState.AlphaBlendEnable = true;
                           device.RenderState.SourceBlend = m_sourceBlend;
                           device.RenderState.DestinationBlend = m_destinationBlend;
                           // Set Red, Green and Blue = opacity
                           device.RenderState.BlendFactorColor = (m_opacity << 16) | (m_opacity << 8) | m_opacity;
                     }
                     else if (EnableColorKeying && device.DeviceCaps.TextureCaps.SupportsAlpha)
                     {
                           device.RenderState.AlphaBlendEnable = true;
                           device.RenderState.SourceBlend = Blend.SourceAlpha;
                           device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
                     }
      */
               if (!World.Settings.EnableSunShading)
               {
                  // Set the render states for rendering of quad tiles.
                  // Any quad tile rendering code that adjusts the state should restore it to below values afterwards.
                  device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
                  device.SetTextureStageState(0, TextureStageStates.ColorOperation, (int)TextureOperation.SelectArg1);
                  device.SetTextureStageState(0, TextureStageStates.ColorArgument1, (int)TextureArgument.TextureColor);
                  device.SetTextureStageState(0, TextureStageStates.AlphaArgument1, (int)TextureArgument.TextureColor);
                  device.SetTextureStageState(0, TextureStageStates.AlphaOperation, (int)TextureOperation.SelectArg1);

                  // Be prepared for multi-texturing
                  device.SetTextureStageState(1, TextureStageStates.ColorArgument2, (int)TextureArgument.Current);
                  device.SetTextureStageState(1, TextureStageStates.ColorArgument1, (int)TextureArgument.TextureColor);
                  device.SetTextureStageState(1, TextureStageStates.TextureCoordinateIndex, 0);
               }
               device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
					lock (((System.Collections.IDictionary)m_topmostTiles).SyncRoot)
					{
						foreach (QuadTile qt in m_topmostTiles.Values)
							qt.Render(drawArgs);
					}

               // Restore device states
               device.SetTextureStageState(1, TextureStageStates.TextureCoordinateIndex, 1);

               if (m_renderPriority < RenderPriority.TerrainMappedImages)
                  device.RenderState.ZBufferEnable = true;

               /*
                                             if (m_opacity < 255 || EnableColorKeying)
                                             {
                                                   // Restore alpha blend state
                                                   device.RenderState.SourceBlend = Blend.SourceAlpha;
                                                   device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
                                             }*/
            }
         }
         finally
         {
            if (IsConnectionWaiting)
            {
               if (DateTime.Now.Subtract(TimeSpan.FromSeconds(15)) < ConnectionWaitStart)
               {
                  string s = "Problem connecting to server... Trying again in 2 minutes.\n";
                  drawArgs.UpperLeftCornerText += s;
               }
            }

            int i = 0;
            foreach (GeoSpatialDownloadRequest request in m_activeDownloads)
            {
               if (request != null && !request.IsComplete && i < 10)
               {
                  RenderDownloadProgress(drawArgs, request, i++);
                  // Only render the first
                  //break;
               }
            }
         }
      }

      internal void RenderDownloadProgress(DrawArgs drawArgs, GeoSpatialDownloadRequest request, int offset)
      {
         int halfIconHeight = 24;
         int halfIconWidth = 24;

         Vector3 projectedPoint = new Vector3(DrawArgs.ParentControl.Width - halfIconWidth - 10, DrawArgs.ParentControl.Height - 34 - 4 * offset, 0.5f);

         // Render progress bar
         if (progressBar == null)
            progressBar = new ProgressBar(40, 4);
         progressBar.Draw(drawArgs, projectedPoint.X, projectedPoint.Y + 24, request.ProgressPercent, World.Settings.DownloadProgressColor.ToArgb());
         DrawArgs.Device.RenderState.ZBufferEnable = true;

         // Render server logo
         if (ServerLogoFilePath == null)
            return;

         if (m_iconTexture == null)
            m_iconTexture = ImageHelper.LoadIconTexture(ServerLogoFilePath);

         if (sprite == null)
         {
            using (Surface s = m_iconTexture.GetSurfaceLevel(0))
            {
               SurfaceDescription desc = s.Description;
               m_spriteSize = new Rectangle(0, 0, desc.Width, desc.Height);
            }

            this.sprite = new Sprite(DrawArgs.Device);
         }

         float scaleWidth = (float)2.0f * halfIconWidth / m_spriteSize.Width;
         float scaleHeight = (float)2.0f * halfIconHeight / m_spriteSize.Height;

         this.sprite.Begin(SpriteFlags.AlphaBlend);
         this.sprite.Transform = Matrix.Transformation2D(new Vector2(0.0f, 0.0f), 0.0f, new Vector2(scaleWidth, scaleHeight),
               new Vector2(0, 0),
               0.0f, new Vector2(projectedPoint.X, projectedPoint.Y));

         this.sprite.Draw(m_iconTexture, m_spriteSize,
               new Vector3(1.32f * 48, 1.32f * 48, 0), new Vector3(0, 0, 0),
               DownloadLogoColor);
         this.sprite.End();
      }

		public override void Dispose()
		{
			isInitialized = false;

			// flush downloads
			lock (((System.Collections.IDictionary)m_downloadRequests).SyncRoot)
			{
				m_downloadRequests.Clear();
				for (int i = 0; i < m_activeDownloads.Length; i++)
				{
					if (m_activeDownloads[i] != null)
					{
						m_activeDownloads[i].Dispose();
						m_activeDownloads[i] = null;
					}
				}
			}

			lock (((System.Collections.IDictionary)m_topmostTiles).SyncRoot)
			{
				foreach (QuadTile qt in m_topmostTiles.Values)
					qt.Dispose();
				m_topmostTiles.Clear();
			}
			if (m_iconTexture != null)
			{
				m_iconTexture.Dispose();
				m_iconTexture = null;
			}

			if (this.sprite != null)
			{
				this.sprite.Dispose();
				this.sprite = null;
			}
		}

      internal virtual void ResetCacheForCurrentView(WorldWind.Camera.CameraBase camera)
      {
         //					  if (!ImageStore.IsDownloadableLayer)
         //							  return;

         List<long> deletionList = new List<long>();
         //reset "root" tiles that intersect current view
         lock (((System.Collections.IDictionary)m_topmostTiles).SyncRoot)
         {
            foreach (long key in m_topmostTiles.Keys)
            {
               QuadTile qt = (QuadTile)m_topmostTiles[key];
               if (camera.ViewFrustum.Intersects(qt.BoundingBox))
               {
                  qt.ResetCache();
                  deletionList.Add(key);
               }
            }

            foreach (long deletionKey in deletionList)
               m_topmostTiles.Remove(deletionKey);
         }
      }

      internal void ClearDownloadRequests()
      {
         lock (((System.Collections.IDictionary)m_downloadRequests).SyncRoot)
         {
            m_downloadRequests.Clear();
         }
      }

		public virtual void AddToDownloadQueue(CameraBase camera, GeoSpatialDownloadRequest newRequest)
      {
         IGeoSpatialDownloadTile key = newRequest.Tile;
         key.WaitingForDownload = true;
         lock (((System.Collections.IDictionary)m_downloadRequests).SyncRoot)
         {
            if (m_downloadRequests.ContainsKey(key))
               return;

            m_downloadRequests.Add(key, newRequest);

            if (m_downloadRequests.Count >= m_maxQueueSize)
            {
               //remove spatially farthest request
               GeoSpatialDownloadRequest farthestRequest = null;
               Angle curDistance = Angle.Zero;
               Angle farthestDistance = Angle.Zero;
               foreach (GeoSpatialDownloadRequest curRequest in m_downloadRequests.Values)
               {
                  curDistance = MathEngine.SphericalDistance(
                              curRequest.Tile.CenterLatitude,
                              curRequest.Tile.CenterLongitude,
                              camera.Latitude,
                              camera.Longitude);

                  if (curDistance > farthestDistance)
                  {
                     farthestRequest = curRequest;
                     farthestDistance = curDistance;
                  }
               }

               farthestRequest.Dispose();
               farthestRequest.Tile.DownloadRequests.Remove(farthestRequest);
               m_downloadRequests.Remove(farthestRequest.Tile);
            }
         }

         ServiceDownloadQueue();
      }

      /// <summary>
      /// Removes a request from the download queue.
      /// </summary>
		public virtual void RemoveFromDownloadQueue(GeoSpatialDownloadRequest removeRequest, bool serviceQueue)
      {
         lock (((System.Collections.IDictionary)m_downloadRequests).SyncRoot)
         {
            IGeoSpatialDownloadTile key = removeRequest.Tile;
				if (m_downloadRequests.ContainsKey(key))
				{
					GeoSpatialDownloadRequest request = m_downloadRequests[key];
					if (request != null)
					{
						m_downloadRequests.Remove(key);
						request.Tile.DownloadRequests.Remove(request);
					}
				}

				if (serviceQueue)
					ServiceDownloadQueue();
         }
      }

      /// <summary>
      /// Starts downloads when there are threads available
      /// </summary>
		public virtual void ServiceDownloadQueue()
      {
         if (m_downloadRequests.Count > 0)
            Log.Write(Log.Levels.Verbose, "QTS", "ServiceDownloadQueue: " + m_downloadRequests.Count + " requests waiting");

         lock (((System.Collections.IDictionary)m_downloadRequests).SyncRoot)
         {
            for (int i = 0; i < MaxSimultaneousDownloads; i++)
            {
               if (m_activeDownloads[i] == null)
                  continue;

               if (!m_activeDownloads[i].IsComplete)
                  continue;

                    // remove from request queue
                    m_downloadRequests.Remove(m_activeDownloads[i].Tile);
               m_activeDownloads[i].Cancel();
               m_activeDownloads[i].Dispose();
               m_activeDownloads[i] = null;
            }

            if (NumberRetries >= 5 || m_isConnectionWaiting)
            {
               // Anti hammer in effect
               if (!m_isConnectionWaiting)
               {
                  m_connectionWaitStart = DateTime.Now;
                  m_isConnectionWaiting = true;
               }

               if (DateTime.Now.Subtract(m_connectionWaitTime) > m_connectionWaitStart)
               {
                  NumberRetries = 0;
                  m_isConnectionWaiting = false;
               }
               return;
            }

            // Queue new downloads
            for (int i = 0; i < MaxSimultaneousDownloads; i++)
            {
               if (m_activeDownloads[i] != null)
                  continue;

               if (m_downloadRequests.Count <= 0)
                  continue;

               m_activeDownloads[i] = GetClosestDownloadRequest();
               if (m_activeDownloads[i] != null)
               {
                  m_downloadStarted[i] = DateTime.Now;
                  m_activeDownloads[i].StartDownload();
               }
            }
         }
      }

      /// <summary>
      /// Finds the "best" tile from queue
      /// </summary>
      internal virtual GeoSpatialDownloadRequest GetClosestDownloadRequest()
      {
         GeoSpatialDownloadRequest closestRequest = null;
			double smallestDist = double.MaxValue;

         lock (((System.Collections.IDictionary)m_downloadRequests).SyncRoot)
         {
				List<IGeoSpatialDownloadTile> oOutOfViewTiles = new List<IGeoSpatialDownloadTile>();

				foreach (IGeoSpatialDownloadTile oTile in m_downloadRequests.Keys)
				//foreach (GeoSpatialDownloadRequest curRequest in m_downloadRequests.Values)
            {
					GeoSpatialDownloadRequest curRequest = m_downloadRequests[oTile];

               if (curRequest.IsDownloading)
                  continue;

               QuadTile qt = (QuadTile)curRequest.Tile;
					if (!m_camera.ViewFrustum.Intersects(qt.BoundingBox))
					{
						oOutOfViewTiles.Add(oTile);
						continue;
					}

               double dLatDist = (oTile.South + oTile.North) / 2.0 - m_camera.Latitude.Degrees;
					double dLonDist = (oTile.East + oTile.West) / 2.0 - m_camera.Longitude.Degrees;
					double dTileDist = Math.Sqrt(dLatDist * dLatDist + dLonDist * dLonDist);

               if (dTileDist < smallestDist)
					{
						smallestDist = dTileDist;
						closestRequest = curRequest;
					}
            }

				foreach (IGeoSpatialDownloadTile oOOVTile in oOutOfViewTiles)
				{
					m_downloadRequests.Remove(oOOVTile);
				}
         }

         return closestRequest;
      }

      /// <summary>
      /// Creates a tile download indication texture
      /// </summary>
      static protected Texture CreateDownloadRectangle(Device device, Color color, int padding)
      {
         int mid = 128;
         using (Bitmap i = new Bitmap(2 * mid, 2 * mid))
         using (Graphics g = Graphics.FromImage(i))
         using (Pen pen = new Pen(color))
         {
            int width = mid - 1 - 2 * padding;
            g.DrawRectangle(pen, padding, padding, width, width);
            g.DrawRectangle(pen, mid + padding, padding, width, width);
            g.DrawRectangle(pen, padding, mid + padding, width, width);
            g.DrawRectangle(pen, mid + padding, mid + padding, width, width);

            Texture texture = new Texture(device, i, Usage.None, Pool.Managed);
            return texture;
         }
      }

		public bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         bool bIsDownloading = false;

         iBytesRead = 0; iTotalBytes = 0;
         foreach (GeoSpatialDownloadRequest request in m_activeDownloads)
         {
            if (request != null && !request.IsComplete)
            {
               iBytesRead += request.DownloadPos;
               iTotalBytes += request.DownloadTotal;
               bIsDownloading = true;
            }
         }

         return bIsDownloading;
      }

      public override void InitExportInfo(DrawArgs drawArgs, ExportInfo info)
      {
         //Update(drawArgs);
         lock (((System.Collections.IDictionary)m_topmostTiles).SyncRoot)
         {
            foreach (long key in m_topmostTiles.Keys)
            {
               QuadTile qt = (QuadTile)m_topmostTiles[key];
               qt.InitExportInfo(drawArgs, info);
            }
         }
      }

		public override void ExportProcess(DrawArgs drawArgs, ExportInfo expInfo)
      {
         //Update(drawArgs);
         lock (((System.Collections.IDictionary)m_topmostTiles).SyncRoot)
         {
            foreach (long key in m_topmostTiles.Keys)
            {
               QuadTile qt = (QuadTile)m_topmostTiles[key];
               qt.ExportProcess(drawArgs, expInfo);
            }
         }
      }
   }
}

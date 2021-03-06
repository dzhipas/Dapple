using System;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind.Renderable;
using WorldWind;
using WorldWind.Configuration;
using WorldWind.Terrain;
using Utility;

namespace WorldWind
{
	/// <summary>
	///
	/// </summary>
	public class World : RenderableObject
	{
		/// <summary>
		/// Persisted user adjustable settings.
		/// </summary>
		public static WorldSettings Settings = new WorldSettings();

		#region Private Members

		double equatorialRadius;
		// TODO: Add ellipsoid parameters to world.
		const double flattening = 6378.135;
		const double SemiMajorAxis = 6378137.0;
		const double SemiMinorAxis = 6356752.31425;
		TerrainAccessor _terrainAccessor;
		RenderableObjectList _renderableObjects;

		#endregion

		#region Properties

		/// <summary>
		/// Whether this world is planet Earth.
		/// </summary>
		internal bool IsEarth
		{
			get
			{
				// HACK
				return this.Name == "Earth";
			}
		}
		#endregion

		ProjectedVectorRenderer m_projectedVectorRenderer = null;

		static World()
		{
			// Don't load settings here - use LoadSettings explicitly
			//LoadSettings();
		}


		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.World"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="position"></param>
		/// <param name="orientation"></param>
		/// <param name="equatorialRadius"></param>
		/// <param name="cacheDirectory"></param>
		/// <param name="terrainAccessor"></param>
		public World(string name, Point3d position, Quaternion4d orientation, double equatorialRadius,
			string cacheDirectory,
			TerrainAccessor terrainAccessor)
			: base(name, position, orientation)
		{
			this.equatorialRadius = equatorialRadius;

			this._terrainAccessor = terrainAccessor;
			this._renderableObjects = new RenderableObjectList(this.Name);
			this.MetaData.Add("CacheDirectory", cacheDirectory);

			//	this.m_WorldSurfaceRenderer = new WorldSurfaceRenderer(32, 0, this);
			this.m_projectedVectorRenderer = new ProjectedVectorRenderer("World Default ProjectedVectorRenderer", this);

			m_outerSphere = new AtmosphericScatteringSphere();
			AtmosphericScatteringSphere.m_fInnerRadius = (float)equatorialRadius;
			AtmosphericScatteringSphere.m_fOuterRadius = (float)equatorialRadius * 1.025f;

			m_outerSphere.Init((float)equatorialRadius * 1.025f);
		}

		internal AtmosphericScatteringSphere m_outerSphere = null;
		public void SetLayerOpacity(string category, string name, float opacity)
		{
			this.setLayerOpacity(this._renderableObjects, category, name, opacity);
		}

		private static string getRenderablePathString(RenderableObject renderable)
		{
			if (renderable.ParentList == null)
			{
				return renderable.Name;
			}
			else
			{
				return getRenderablePathString(renderable.ParentList) + Path.DirectorySeparatorChar + renderable.Name;
			}
		}

		private void setLayerOpacity(RenderableObject ro, string category, string name, float opacity)
		{
			foreach (string key in ro.MetaData.Keys)
			{
				if (String.Compare(key, category, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
				{
					if (ro.MetaData[key].GetType() == typeof(String))
					{
						string curValue = ro.MetaData[key] as string;
						if (String.Compare(curValue, name, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
						{
							ro.Opacity = (byte)(255 * opacity);
						}
					}
					break;
				}
			}

			RenderableObjectList rol = ro as RenderableObjectList;
			if (rol != null)
			{
				foreach (RenderableObject childRo in rol.ChildObjects)
					setLayerOpacity(childRo, category, name, opacity);
			}
		}

		/// <summary>
		/// Deserializes settings from default location
		/// </summary>
		public static void LoadSettings()
		{
			try
			{
				Settings = (WorldSettings)SettingsBase.Load(Settings);
			}
			catch (Exception caught)
			{
				Log.Write(caught);
			}
		}

		/// Deserializes settings from specified location
		/// </summary>
		public static void LoadSettings(string directory)
		{
			try
			{
				Settings = (WorldSettings)SettingsBase.LoadFromPath(Settings, directory);
			}
			catch (Exception caught)
			{
				Log.Write(caught);
			}
		}

		public TerrainAccessor TerrainAccessor
		{
			get
			{
				return this._terrainAccessor;
			}
			set
			{
				this._terrainAccessor = value;
			}
		}

		public double EquatorialRadius
		{
			get
			{
				return this.equatorialRadius;
			}
		}

		public RenderableObjectList RenderableObjects
		{
			get
			{
				return this._renderableObjects;
			}
			set
			{
				this._renderableObjects = value;
			}
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			try
			{
				if (this.isInitialized)
					return;

				this.RenderableObjects.Initialize(drawArgs);
			}
			catch (Exception caught)
			{
				Log.DebugWrite(caught);
			}
			finally
			{
				this.isInitialized = true;
			}
		}

		public override void Update(DrawArgs drawArgs)
		{
			if (!this.isInitialized)
			{
				this.Initialize(drawArgs);
			}

			if (this.RenderableObjects != null)
			{
				this.RenderableObjects.Update(drawArgs);
			}

			if (this.m_projectedVectorRenderer != null)
			{
				this.m_projectedVectorRenderer.Update(drawArgs);
			}
			// Update camera terrain elevation
			// drawArgs.WorldCamera.UpdateTerrainElevation(this.TerrainAccessor); // Moved to WorldWindow.Render()

			if (World.Settings.EnableAtmosphericScattering && m_outerSphere != null)
				m_outerSphere.Update();
		}

		private void RenderSun(DrawArgs drawArgs)
		{
			Point3d sunPosition = -SunCalculator.GetGeocentricPosition(TimeKeeper.CurrentTimeUtc);

			Point3d sunSpherical = MathEngine.CartesianToSpherical(sunPosition.X, sunPosition.Y, sunPosition.Z);
			sunPosition = MathEngine.SphericalToCartesian(
				Angle.FromRadians(sunSpherical.Y),
				Angle.FromRadians(sunSpherical.Z),
				150000000000);

			Point3d sunVector = new Point3d(sunPosition.X, sunPosition.Y, sunPosition.Z);

			Frustum viewFrustum = new Frustum();

			float aspectRatio = (float)drawArgs.WorldCamera.Viewport.Width / drawArgs.WorldCamera.Viewport.Height;
			Matrix4d projectionMatrix = Matrix4d.PerspectiveFovRH(drawArgs.WorldCamera.Fov.Radians, aspectRatio, 1000.0, 300000000000);

			viewFrustum.Update(
				Matrix4d.Multiply(drawArgs.WorldCamera.AbsoluteWorldMatrix,
				Matrix4d.Multiply(drawArgs.WorldCamera.AbsoluteViewMatrix,
					projectionMatrix)));

			if (!viewFrustum.ContainsPoint(sunVector))
				return;

			Point3d translationVector = new Point3d(
				(sunPosition.X - drawArgs.WorldCamera.ReferenceCenter.X),
				(sunPosition.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
				(sunPosition.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

			Point3d projectedPoint = drawArgs.WorldCamera.Project(translationVector);

			if (m_sunTexture == null)
			{
				m_sunTexture = ImageHelper.LoadTexture(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Data\\sun.dds");
				m_sunSurfaceDescription = m_sunTexture.GetLevelDescription(0);
			}

			if (m_sprite == null)
			{
				m_sprite = new Sprite(drawArgs.device);
			}

			m_sprite.Begin(SpriteFlags.AlphaBlend);

			// Render icon
			float xscale = (float)m_sunWidth / m_sunSurfaceDescription.Width;
			float yscale = (float)m_sunHeight / m_sunSurfaceDescription.Height;
			m_sprite.Transform = Matrix.Scaling(xscale, yscale, 0);

			m_sprite.Transform *= Matrix.Translation((float)projectedPoint.X, (float)projectedPoint.Y, 0);
			m_sprite.Draw(m_sunTexture,
				new Vector3(m_sunSurfaceDescription.Width >> 1, m_sunSurfaceDescription.Height >> 1, 0),
				Vector3.Empty,
				System.Drawing.Color.FromArgb(253, 253, 200).ToArgb());

			// Reset transform to prepare for text rendering later
			m_sprite.Transform = Matrix.Identity;
			m_sprite.End();
		}

		int m_sunWidth = 72;
		int m_sunHeight = 72;

		Sprite m_sprite = null;
		Texture m_sunTexture = null;
		SurfaceDescription m_sunSurfaceDescription;

		public override void Render(DrawArgs drawArgs)
		{
			using (new DirectXProfilerEvent("World::Render"))
			{
				try
				{
					//  Old method -- problems with RenderPriority sorting
					//RenderableObjects.Render(drawArgs);

					// allow fog and other renderstate setup changers to get in front of everything else
					Render(RenderableObjects, (WorldWind.Renderable.RenderPriority)(-1), drawArgs);

					//RenderStars(drawArgs, RenderableObjects);

					if (drawArgs.CurrentWorld.IsEarth && World.Settings.EnableAtmosphericScattering)
					{
						// Render atmospheric scattering
						bool origFog = drawArgs.device.RenderState.FogEnable;
						drawArgs.device.RenderState.FogEnable = false;
						float aspectRatio = (float)drawArgs.WorldCamera.Viewport.Width / drawArgs.WorldCamera.Viewport.Height;
						float zNear = (float)drawArgs.WorldCamera.Altitude * 0.1f;
						double distToCenterOfPlanet = (drawArgs.WorldCamera.Altitude + equatorialRadius);
						double tangentalDistance = Math.Sqrt(distToCenterOfPlanet * distToCenterOfPlanet - equatorialRadius * equatorialRadius);
						double amosphereThickness = Math.Sqrt(m_outerSphere.m_radius * m_outerSphere.m_radius + equatorialRadius * equatorialRadius);
						Matrix proj = drawArgs.device.Transform.Projection;
						drawArgs.device.Transform.Projection = ConvertDX.FromMatrix4d(Matrix4d.PerspectiveFovRH(drawArgs.WorldCamera.Fov.Radians, aspectRatio, zNear, tangentalDistance + amosphereThickness));
						drawArgs.device.RenderState.ZBufferEnable = false;
						drawArgs.device.RenderState.CullMode = Cull.CounterClockwise;
						m_outerSphere.Render(drawArgs);
						drawArgs.device.RenderState.CullMode = Cull.Clockwise;
						drawArgs.device.RenderState.ZBufferEnable = true;
						drawArgs.device.Transform.Projection = proj;
						drawArgs.device.RenderState.FogEnable = origFog;
					}

					DirectXProfiler.BeginEvent("render stars");
					//render SurfaceImages
					Render(RenderableObjects, WorldWind.Renderable.RenderPriority.Stars, drawArgs);
					DirectXProfiler.EndEvent();

					if (World.Settings.EnableSunShading)
						RenderSun(drawArgs);

					DirectXProfiler.BeginEvent("render surface images");
					//render SurfaceImages
					Render(RenderableObjects, WorldWind.Renderable.RenderPriority.TerrainMappedImages, drawArgs);
					DirectXProfiler.EndEvent();

					if (m_projectedVectorRenderer != null)
						m_projectedVectorRenderer.Render(drawArgs);

					DirectXProfiler.BeginEvent("render atmospheric images");
					//render AtmosphericImages
					Render(RenderableObjects, WorldWind.Renderable.RenderPriority.AtmosphericImages, drawArgs);
					DirectXProfiler.EndEvent();

					DirectXProfiler.BeginEvent("render line paths");
					//render LinePaths
					Render(RenderableObjects, WorldWind.Renderable.RenderPriority.LinePaths, drawArgs);
					DirectXProfiler.EndEvent();

					DirectXProfiler.BeginEvent("render placenames");
					//render Placenames
					Render(RenderableObjects, WorldWind.Renderable.RenderPriority.Placenames, drawArgs);
					DirectXProfiler.EndEvent();

					DirectXProfiler.BeginEvent("render icons");
					//render Icons
					Render(RenderableObjects, WorldWind.Renderable.RenderPriority.Icons, drawArgs);
					DirectXProfiler.EndEvent();

					DirectXProfiler.BeginEvent("render custom");
					//render Custom
					Render(RenderableObjects, WorldWind.Renderable.RenderPriority.Custom, drawArgs);
					DirectXProfiler.EndEvent();
				}
				catch (Exception ex)
				{
					Log.Write(ex);
				}
			}
		}

		private void Render(WorldWind.Renderable.RenderableObject renderable, WorldWind.Renderable.RenderPriority priority, DrawArgs drawArgs)
		{
			if (!renderable.IsOn || (renderable.Name != null && renderable.Name.Equals("Starfield")))
				return;

			try
			{
				if (priority == WorldWind.Renderable.RenderPriority.Icons && renderable is Icons)
				{
					renderable.Render(drawArgs);
				}
				else if (renderable is WorldWind.Renderable.RenderableObjectList)
				{
					WorldWind.Renderable.RenderableObjectList rol = (WorldWind.Renderable.RenderableObjectList)renderable;
					for (int i = 0; i < rol.ChildObjects.Count; i++)
					{
						Render((WorldWind.Renderable.RenderableObject)rol.ChildObjects[i], priority, drawArgs);
					}
				}
				// hack at the moment
				else if (priority == WorldWind.Renderable.RenderPriority.TerrainMappedImages)
				{
					if (renderable.RenderPriority == WorldWind.Renderable.RenderPriority.SurfaceImages || renderable.RenderPriority == WorldWind.Renderable.RenderPriority.TerrainMappedImages)
					{
						renderable.Render(drawArgs);
					}
				}
				else if (renderable.RenderPriority == priority)
				{
					renderable.Render(drawArgs);
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private void saveRenderableState(RenderableObject ro)
		{
			string path = getRenderablePathString(ro);
			bool found = false;
			for (int i = 0; i < World.Settings.LoadedLayers.Count; i++)
			{
				string s = (string)World.Settings.LoadedLayers[i];
				if (s.Equals(path))
				{
					if (!ro.IsOn)
					{
						World.Settings.LoadedLayers.RemoveAt(i);
						break;

					}
					else
					{
						found = true;
					}
				}
			}

			if (!found && ro.IsOn)
			{
				World.Settings.LoadedLayers.Add(path);
			}
		}

		private void saveRenderableStates(RenderableObjectList rol)
		{
			saveRenderableState(rol);

			foreach (RenderableObject ro in rol.ChildObjects)
			{
				if (ro is RenderableObjectList)
				{
					RenderableObjectList childRol = (RenderableObjectList)ro;
					saveRenderableStates(childRol);
				}
				else
				{
					saveRenderableState(ro);
				}
			}
		}

		public override void Dispose()
		{
			saveRenderableStates(RenderableObjects);

			if (this.RenderableObjects != null)
			{
				this.RenderableObjects.Dispose();
				this.RenderableObjects = null;
			}

			if (m_outerSphere != null)
			{
				m_outerSphere.Dispose();
			}
		}

		/// <summary>
		/// Computes the great circle distance between two pairs of lat/longs.
		/// TODO: Compute distance using ellipsoid.
		/// </summary>
		public static Angle ApproxAngularDistance(Angle latA, Angle lonA, Angle latB, Angle lonB)
		{
			Angle dlon = lonB - lonA;
			Angle dlat = latB - latA;
			double k = Math.Sin(dlat.Radians * 0.5);
			double l = Math.Sin(dlon.Radians * 0.5);
			double a = k * k + Math.Cos(latA.Radians) * Math.Cos(latB.Radians) * l * l;
			double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
			return Angle.FromRadians(c);
		}

		/// <summary>
		/// Intermediate points on a great circle
		/// In previous sections we have found intermediate points on a great circle given either
		/// the crossing latitude or longitude. Here we find points (lat,lon) a given fraction of the
		/// distance (d) between them. Suppose the starting point is (lat1,lon1) and the final point
		/// (lat2,lon2) and we want the point a fraction f along the great circle route. f=0 is
		/// point 1. f=1 is point 2. The two points cannot be antipodal ( i.e. lat1+lat2=0 and
		/// abs(lon1-lon2)=pi) because then the route is undefined.
		/// </summary>
		/// <param name="f">Fraction of the distance for intermediate point (0..1)</param>
		public static void IntermediateGCPoint(float f, Angle lat1, Angle lon1, Angle lat2, Angle lon2, Angle d,
			out Angle lat, out Angle lon)
		{
			double sind = Math.Sin(d.Radians);
			double cosLat1 = Math.Cos(lat1.Radians);
			double cosLat2 = Math.Cos(lat2.Radians);
			double A = Math.Sin((1 - f) * d.Radians) / sind;
			double B = Math.Sin(f * d.Radians) / sind;
			double x = A * cosLat1 * Math.Cos(lon1.Radians) + B * cosLat2 * Math.Cos(lon2.Radians);
			double y = A * cosLat1 * Math.Sin(lon1.Radians) + B * cosLat2 * Math.Sin(lon2.Radians);
			double z = A * Math.Sin(lat1.Radians) + B * Math.Sin(lat2.Radians);
			lat = Angle.FromRadians(Math.Atan2(z, Math.Sqrt(x * x + y * y)));
			lon = Angle.FromRadians(Math.Atan2(y, x));
		}

		/// <summary>
		/// Intermediate points on a great circle
		/// In previous sections we have found intermediate points on a great circle given either
		/// the crossing latitude or longitude. Here we find points (lat,lon) a given fraction of the
		/// distance (d) between them. Suppose the starting point is (lat1,lon1) and the final point
		/// (lat2,lon2) and we want the point a fraction f along the great circle route. f=0 is
		/// point 1. f=1 is point 2. The two points cannot be antipodal ( i.e. lat1+lat2=0 and
		/// abs(lon1-lon2)=pi) because then the route is undefined.
		/// </summary>
		/// <param name="f">Fraction of the distance for intermediate point (0..1)</param>
		public Point3d IntermediateGCPoint(double f, Angle lat1, Angle lon1, Angle lat2, Angle lon2, Angle d)
		{
			double sind = Math.Sin(d.Radians);
			double cosLat1 = Math.Cos(lat1.Radians);
			double cosLat2 = Math.Cos(lat2.Radians);
			double A = Math.Sin((1 - f) * d.Radians) / sind;
			double B = Math.Sin(f * d.Radians) / sind;
			double x = A * cosLat1 * Math.Cos(lon1.Radians) + B * cosLat2 * Math.Cos(lon2.Radians);
			double y = A * cosLat1 * Math.Sin(lon1.Radians) + B * cosLat2 * Math.Sin(lon2.Radians);
			double z = A * Math.Sin(lat1.Radians) + B * Math.Sin(lat2.Radians);
			Angle lat = Angle.FromRadians(Math.Atan2(z, Math.Sqrt(x * x + y * y)));
			Angle lon = Angle.FromRadians(Math.Atan2(y, x));

			Point3d v = MathEngine.SphericalToCartesian(lat, lon, equatorialRadius);
			return v;
		}
	}

	internal class AtmosphericScatteringSphere
	{
		internal float m_radius;

		internal static float m_fInnerRadius;
		internal static float m_fOuterRadius;
		internal static int TilesHigh = 4;
		internal static int TilesWide = 8;

		internal void Init(float radius)
		{
			try
			{
				m_radius = radius;

				Point3d sunPosition = SunCalculator.GetGeocentricPosition(TimeKeeper.CurrentTimeUtc);
				Point3d sunVector = new Point3d(
					-sunPosition.X,
					-sunPosition.Y,
					-sunPosition.Z);

				m_vLight = sunVector * 100000000.0;
				m_vLightDirection = new Point3d(
					m_vLight.X / m_vLight.Length,
					m_vLight.Y / m_vLight.Length,
					m_vLight.Z / m_vLight.Length
					);

				m_fScale = 1 / (m_fOuterRadius - m_fInnerRadius);

				m_meshList.Clear();

				for (int y = 0; y < TilesHigh; y++)
				{
					for (int x = 0; x < TilesWide; x++)
					{
						m_meshList.Add(new MeshSubset());
					}
				}

				m_nSamples = 4;		// Number of sample rays to use in integral equation
				m_Kr = 0.0025f;		// Rayleigh scattering constant
				m_Kr4PI = m_Kr * 4.0f * (float)Math.PI;
				m_Km = 0.0015f;		// Mie scattering constant
				m_Km4PI = m_Km * 4.0f * (float)Math.PI;
				m_ESun = 15.0f;		// Sun brightness constant
				m_g = -0.85f;		// The Mie phase asymmetry factor

				m_fWavelength[0] = 0.650f;		// 650 nm for red
				m_fWavelength[1] = 0.570f;		// 570 nm for green
				m_fWavelength[2] = 0.475f;		// 475 nm for blue
				m_fWavelength4[0] = (float)Math.Pow(m_fWavelength[0], 4.0f);
				m_fWavelength4[1] = (float)Math.Pow(m_fWavelength[1], 4.0f);
				m_fWavelength4[2] = (float)Math.Pow(m_fWavelength[2], 4.0f);

				m_fRayleighScaleDepth = 0.25f;
				m_fMieScaleDepth = 0.1f;

			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		internal void Dispose()
		{
			active = false;
		}

		class MeshSubset
		{
		}
		System.Collections.Generic.List<MeshSubset> m_meshList = new System.Collections.Generic.List<MeshSubset>();

		System.Threading.Thread m_backgroundThread = null;
		bool active;
		System.DateTime m_lastOpticalUpdate = System.DateTime.MinValue;

		private void Updater()
		{
			while (active)
			{
				if (World.Settings.EnableAtmosphericScattering && m_meshList.Count > 0)
				{
					System.DateTime currentTime = TimeKeeper.CurrentTimeUtc;
					// Update Sun
					UpdateLightVector();

					m_nSamples = 4;		// Number of sample rays to use in integral equation
					m_Kr = 0.0025f;		// Rayleigh scattering constant
					m_Kr4PI = m_Kr * 4.0f * (float)Math.PI;
					m_Km = 0.0015f;		// Mie scattering constant
					m_Km4PI = m_Km * 4.0f * (float)Math.PI;
					m_ESun = 15.0f;		// Sun brightness constant
					m_g = -0.85f;		// The Mie phase asymmetry factor

					m_fWavelength[0] = 0.650f;		// 650 nm for red
					m_fWavelength[1] = 0.570f;		// 570 nm for green
					m_fWavelength[2] = 0.475f;		// 475 nm for blue
					m_fWavelength4[0] = (float)Math.Pow(m_fWavelength[0], 4.0f);
					m_fWavelength4[1] = (float)Math.Pow(m_fWavelength[1], 4.0f);
					m_fWavelength4[2] = (float)Math.Pow(m_fWavelength[2], 4.0f);

					m_fRayleighScaleDepth = 0.25f;
					m_fMieScaleDepth = 0.1f;

					if (currentTime.Subtract(m_lastOpticalUpdate) > TimeSpan.FromSeconds(100))
					{
						MakeOpticalDepthBuffer(m_fInnerRadius, m_fOuterRadius, m_fRayleighScaleDepth, m_fMieScaleDepth);
						m_lastOpticalUpdate = currentTime;
					}
				}
				System.Threading.Thread.Sleep(500);
			}
		}

		internal void Update()
		{
			if (m_backgroundThread == null)
			{
				active = true;
				m_backgroundThread = new System.Threading.Thread(new System.Threading.ThreadStart(Updater));
				m_backgroundThread.Name = ThreadNames.AtmosphericScatteringBackground;
				m_backgroundThread.Priority = System.Threading.ThreadPriority.Lowest;
				m_backgroundThread.IsBackground = true;
				m_backgroundThread.Start();
			}
		}

		float[] fCameraDepth = new float[4] { 0, 0, 0, 0 };
		float[] fLightDepth = new float[4];
		float[] fSampleDepth = new float[4];
		float[] fRayleighSum = new float[] { 0, 0, 0 };
		float[] fMieSum = new float[] { 0, 0, 0 };
		Point3d vPos = new Point3d();
		float[] fAttenuation = new float[3];
		Point3d vCamera = new Point3d();

		internal void SetColor(ref CustomVertex.PositionColored pVertex, DrawArgs drawArgs)
		{
			vPos.X = pVertex.X;
			vPos.Y = pVertex.Y;
			vPos.Z = pVertex.Z;

			// Get the ray from the camera to the vertex, and its length (which is the far point of the ray passing through the atmosphere)
			vCamera.X = drawArgs.WorldCamera.Position.X;
			vCamera.Y = drawArgs.WorldCamera.Position.Y;
			vCamera.Z = drawArgs.WorldCamera.Position.Z;

			Point3d vRay = vPos - vCamera;
			float fFar = (float)vRay.Length;

			vRay.normalize();

			// Calculate the closest intersection of the ray with the outer atmosphere (which is the near point of the ray passing through the atmosphere)
			float B = 2.0f * (float)Point3d.dot(vCamera, vRay);
			float C = (float)Point3d.dot(vCamera, vCamera) - m_fOuterRadius * m_fOuterRadius;
			float fDet = (float)Math.Max(0.0f, B * B - 4.0f * C);
			float fNear = 0.5f * (-B - (float)Math.Sqrt(fDet));

			bool bCameraAbove = true;

			for (int i = 0; i < fCameraDepth.Length; i++)
				fCameraDepth[i] = 0;

			for (int i = 0; i < fLightDepth.Length; i++)
				fLightDepth[i] = 0;

			for (int i = 0; i < fSampleDepth.Length; i++)
				fSampleDepth[i] = 0;

			if (fNear <= 0)
			{
				// If the near point is behind the camera, it means the camera is inside the atmosphere
				fNear = 0;
				float fCameraHeight = (float)vCamera.Length;
				float fCameraAltitude = (fCameraHeight - m_fInnerRadius) * m_fScale;
				bCameraAbove = fCameraHeight >= vPos.Length;
				float fCameraAngle = (float)Point3d.dot((bCameraAbove ? -vRay : vRay), vCamera) / fCameraHeight;
				Interpolate(ref fCameraDepth, fCameraAltitude, 0.5f - fCameraAngle * 0.5f);
			}
			else
			{
				// Otherwise, move the camera up to the near intersection point
				vCamera += vRay * fNear;
				fFar -= fNear;
				fNear = 0;
			}

			// If the distance between the points on the ray is negligible, don't bother to calculate anything
			if (fFar <= DELTA)
			{
				pVertex.Color = System.Drawing.Color.FromArgb(255, 0, 0, 0).ToArgb();
				return;
			}

			// Initialize a few variables to use inside the loop
			for (int i = 0; i < fRayleighSum.Length; i++)
				fRayleighSum[i] = 0;
			for (int i = 0; i < fMieSum.Length; i++)
				fMieSum[i] = 0;

			float fSampleLength = fFar / m_nSamples;
			float fScaledLength = fSampleLength * m_fScale;
			Point3d vSampleRay = vRay * fSampleLength;

			// Start at the center of the first sample ray, and loop through each of the others
			vPos = vCamera + vSampleRay * 0.5f;
			for (int i = 0; i < m_nSamples; i++)
			{
				float fHeight = (float)vPos.Length;

				// Start by looking up the optical depth coming from the light source to this point
				float fLightAngle = (float)Point3d.dot(m_vLightDirection, vPos) / fHeight;
				float fAltitude = (fHeight - m_fInnerRadius) * m_fScale;
				Interpolate(ref fLightDepth, fAltitude, 0.5f - fLightAngle * 0.5f);

				// If no light light reaches this part of the atmosphere, no light is scattered in at this point
				if (fLightDepth[0] < DELTA)
					continue;

				// Get the density at this point, along with the optical depth from the light source to this point
				float fRayleighDensity = fScaledLength * fLightDepth[0];
				float fRayleighDepth = fLightDepth[1];
				float fMieDensity = fScaledLength * fLightDepth[2];
				float fMieDepth = fLightDepth[3];

				// If the camera is above the point we're shading, we calculate the optical depth from the sample point to the camera
				// Otherwise, we calculate the optical depth from the camera to the sample point
				if (bCameraAbove)
				{
					float fSampleAngle = (float)Point3d.dot(-vRay, vPos) / fHeight;
					Interpolate(ref fSampleDepth, fAltitude, 0.5f - fSampleAngle * 0.5f);
					fRayleighDepth += fSampleDepth[1] - fCameraDepth[1];
					fMieDepth += fSampleDepth[3] - fCameraDepth[3];
				}
				else
				{
					float fSampleAngle = (float)Point3d.dot(vRay, vPos) / fHeight;
					Interpolate(ref fSampleDepth, fAltitude, 0.5f - fSampleAngle * 0.5f);
					fRayleighDepth += fCameraDepth[1] - fSampleDepth[1];
					fMieDepth += fCameraDepth[3] - fSampleDepth[3];
				}

				// Now multiply the optical depth by the attenuation factor for the sample ray
				fRayleighDepth *= m_Kr4PI;
				fMieDepth *= m_Km4PI;

				// Calculate the attenuation factor for the sample ray
				fAttenuation[0] = (float)Math.Exp(-fRayleighDepth / m_fWavelength4[0] - fMieDepth);
				fAttenuation[1] = (float)Math.Exp(-fRayleighDepth / m_fWavelength4[1] - fMieDepth);
				fAttenuation[2] = (float)Math.Exp(-fRayleighDepth / m_fWavelength4[2] - fMieDepth);

				fRayleighSum[0] += fRayleighDensity * fAttenuation[0];
				fRayleighSum[1] += fRayleighDensity * fAttenuation[1];
				fRayleighSum[2] += fRayleighDensity * fAttenuation[2];

				fMieSum[0] += fMieDensity * fAttenuation[0];
				fMieSum[1] += fMieDensity * fAttenuation[1];
				fMieSum[2] += fMieDensity * fAttenuation[2];

				// Move the position to the center of the next sample ray
				vPos += vSampleRay;
			}

			// Calculate the angle and phase values (this block of code could be handled by a small 1D lookup table, or a 1D texture lookup in a pixel shader)
			float fAngle = (float)Point3d.dot(-vRay, m_vLightDirection);
			float[] fPhase = new float[2];
			float fAngle2 = fAngle * fAngle;
			float g2 = m_g * m_g;
			fPhase[0] = 0.75f * (1.0f + fAngle2);
			fPhase[1] = 1.5f * ((1 - g2) / (2 + g2)) * (1.0f + fAngle2) / (float)Math.Pow(1 + g2 - 2 * m_g * fAngle, 1.5f);
			fPhase[0] *= m_Kr * m_ESun;
			fPhase[1] *= m_Km * m_ESun;
			// Calculate the in-scattering color and clamp it to the max color value
			float[] fColor = new float[3] { 0, 0, 0 };
			fColor[0] = fRayleighSum[0] * fPhase[0] / m_fWavelength4[0] + fMieSum[0] * fPhase[1];
			fColor[1] = fRayleighSum[1] * fPhase[0] / m_fWavelength4[1] + fMieSum[1] * fPhase[1];
			fColor[2] = fRayleighSum[2] * fPhase[0] / m_fWavelength4[2] + fMieSum[2] * fPhase[1];
			fColor[0] = (float)Math.Min(fColor[0], 1.0f);
			fColor[1] = (float)Math.Min(fColor[1], 1.0f);
			fColor[2] = (float)Math.Min(fColor[2], 1.0f);

			// Compute alpha transparency (PM 2006-11-19)
			float alpha = (fColor[0] + fColor[1] + fColor[2]) / 3;  // Average luminosity
			alpha = (float)Math.Min(alpha + 0.50, 1f);			  // increase opacity

			// Last but not least, set the color
			pVertex.Color = System.Drawing.Color.FromArgb((byte)(alpha * 255), (byte)(fColor[0] * 255), (byte)(fColor[1] * 255), (byte)(fColor[2] * 255)).ToArgb();

		}

		void Interpolate(ref float[] p, float x, float y)
		{
			float fX = x * (m_nWidth - 1);
			float fY = y * (m_nHeight - 1);
			int nX = Math.Min(m_nWidth - 2, Math.Max(0, (int)fX));
			int nY = Math.Min(m_nHeight - 2, Math.Max(0, (int)fY));
			float fRatioX = fX - nX;
			float fRatioY = fY - nY;

			//float *pValue = (float *)((unsigned long)m_pBuffer + m_nElementSize * (m_nWidth * nY + nX));
			//float pValue = m_opticalDepthBuffer[m_nWidth * nY + nX];
			int pValueOffset = (m_nWidth * nY + nX) * 4;

			for (int i = 0; i < m_nChannels; i++)
			{
				if (m_currentOpticalBuffer == 1)
				{
					p[i] = m_opticalDepthBuffer1[pValueOffset] * (1 - fRatioX) * (1 - fRatioY) +
						m_opticalDepthBuffer1[pValueOffset + m_nChannels * 1] * (fRatioX) * (1 - fRatioY) +
						m_opticalDepthBuffer1[pValueOffset + m_nChannels * m_nWidth] * (1 - fRatioX) * (fRatioY) +
						m_opticalDepthBuffer1[pValueOffset + m_nChannels * (m_nWidth + 1)] * (fRatioX) * (fRatioY);
				}
				else
				{
					p[i] = m_opticalDepthBuffer2[pValueOffset] * (1 - fRatioX) * (1 - fRatioY) +
						m_opticalDepthBuffer2[pValueOffset + m_nChannels * 1] * (fRatioX) * (1 - fRatioY) +
						m_opticalDepthBuffer2[pValueOffset + m_nChannels * m_nWidth] * (1 - fRatioX) * (fRatioY) +
						m_opticalDepthBuffer2[pValueOffset + m_nChannels * (m_nWidth + 1)] * (fRatioX) * (fRatioY);
				}
				pValueOffset++;
			}
		}

		float DELTA = 1e-6f;
		static int m_nChannels = 4;
		static int m_nWidth;				// The width of the buffer (x axis)
		static int m_nHeight;				// The height of the buffer (y axis)
		float m_fScale;
		float[] m_fWavelength = new float[3];
		float[] m_fWavelength4 = new float[3];
		float m_fRayleighScaleDepth;
		float m_fMieScaleDepth;

		int m_nSamples;
		float m_Kr, m_Kr4PI;
		float m_Km, m_Km4PI;
		float m_ESun;
		float m_g;

		Point3d m_vLight;
		Point3d m_vLightDirection;

		// -- Addition from Sky Gradient geometry (PM 2006-11-27)
		// Rebuild sky mesh with updated colors
		double thickness;
		Mesh skyMesh;
		private void UpdateSkyMesh(DrawArgs drawArgs, double horizonSpan)
		{
			WorldWind.Camera.CameraBase camera = drawArgs.WorldCamera;

			// Use world atmospheric scattering sphere radius for thickness
			thickness = m_radius - camera.WorldRadius;

			double distToCenterOfPlanet = (camera.Altitude + camera.WorldRadius);
			// Compute distance to horizon and dome radius
			double tangentalDistance = Math.Sqrt(distToCenterOfPlanet * distToCenterOfPlanet - camera.WorldRadius * camera.WorldRadius);
			double domeRadius = tangentalDistance;

			// horizon latitude
			double horizonLat = (-Math.PI / 2 + Math.Acos(tangentalDistance / distToCenterOfPlanet)) * 180 / Math.PI;

			// zenith latitude
			double zenithLat = 90;
			if (camera.Altitude >= thickness)
			{
				double tangentalDistanceZenith = Math.Sqrt(distToCenterOfPlanet * distToCenterOfPlanet - (camera.WorldRadius + thickness) * (camera.WorldRadius + thickness));
				zenithLat = (-Math.PI / 2 + Math.Acos(tangentalDistanceZenith / distToCenterOfPlanet)) * 180 / Math.PI;
			}
			if (camera.Altitude < thickness && camera.Altitude > thickness * 0.8)
			{
				zenithLat = (thickness - camera.Altitude) / (thickness - thickness * 0.8) * 90;
			}
			// new mesh
			if (skyMesh != null) skyMesh.Dispose();
			int res = horizonSpan > 180 ? 64 : 128;
			skyMesh = ColoredSpherePartial(drawArgs, (float)domeRadius, horizonLat, zenithLat, res, res / 2, horizonSpan);

		}

		/// <summary>
		/// Creates a partial PositionColored sphere pre-transformed to the camera position
		/// modified to provide a sky/atmosphere gradient dome
		/// </summary>
		/// <param name="device">The current direct3D drawing device.</param>
		/// <param name="radius">The sphere's radius</param>
		/// <param name="slices">Number of slices (Horizontal resolution).</param>
		/// <param name="stacks">Number of stacks. (Vertical resolution)</param>
		/// <returns></returns>
		/// <remarks>
		/// Number of vertices in the sphere will be (slices+1)*(stacks+1)<br/>
		/// Number of faces	:slices*stacks*2
		/// Number of Indexes	: Number of faces * 3;
		/// </remarks>
		private Mesh ColoredSpherePartial(DrawArgs drawArgs, float radius, double startLat, double endLat, int slices, int stacks, double lonSpan)
		{
			slices = (int)((double)slices * lonSpan / 360);

			int numVertices = (slices + 1) * (stacks + 1);
			int numFaces = slices * stacks * 2;
			int indexCount = numFaces * 3;
			Device device = drawArgs.device;

			// SkyGradient dome transform
			WorldWind.Camera.CameraBase camera = drawArgs.WorldCamera;
			Point3d cameraPos = camera.Position;
			double distToCenterOfPlanet = (camera.Altitude + camera.WorldRadius);
			Point3d cameraCoord = MathEngine.CartesianToSpherical(cameraPos.X, cameraPos.Y, cameraPos.Z);
			double camLat = cameraCoord.Y;
			double camLon = cameraCoord.Z;
			Matrix4d SkyGradientTrans = Matrix4d.Identity;
			SkyGradientTrans *= Matrix4d.Translation(0, 0, distToCenterOfPlanet);
			SkyGradientTrans *= Matrix4d.RotationY(-camLat + Math.PI / 2);
			SkyGradientTrans *= Matrix4d.RotationZ(camLon);

			// Find camera heading from the camera lat/lon to the target
			double d = MathEngine.SphericalDistance(Angle.FromRadians(camLat), Angle.FromRadians(camLon), camera.Latitude, camera.Longitude).Radians;
			double camHead = Math.Acos((Math.Sin(camera.Latitude.Radians) - Math.Sin(camLat) * Math.Cos(d)) / (Math.Sin(d) * Math.Cos(camLat)));
			if (Math.Sign(camera.Longitude.Radians - camLon) < 0) camHead = Math.PI * 2 - camHead;
			if (double.IsNaN(camHead)) camHead = 0;
			camHead = MathEngine.RadiansToDegrees(camHead);
			double startLon = -camHead - 180 + (lonSpan / 2);

			Mesh mesh = new Mesh(numFaces, numVertices, MeshFlags.Managed, CustomVertex.PositionColored.Format, device);

			// Get the original sphere's vertex buffer.
			int[] ranks = new int[1];
			ranks[0] = mesh.NumberVertices;
			System.Array arr = mesh.VertexBuffer.Lock(0, typeof(CustomVertex.PositionColored), LockFlags.None, ranks);

			// Set the vertex buffer
			int vertIndex = 0;
			CustomVertex.PositionColored pnt;
			Point3d v;

			// bottom fade
			double latitude = startLat - ((endLat - startLat) / 10);
			if (latitude < startLat - 1) latitude = startLat - 1;
			for (int slice = 0; slice <= slices; slice++)
			{
				pnt = new CustomVertex.PositionColored();
				double longitude = startLon - ((float)slice / slices * lonSpan);
				v = MathEngine.SphericalToCartesian(latitude, longitude, radius);
				v.TransformCoordinate(SkyGradientTrans);
				pnt.X = (float)v.X;
				pnt.Y = (float)v.Y;
				pnt.Z = (float)v.Z;
				pnt.Color = System.Drawing.Color.FromArgb(0, 0, 0, 0).ToArgb();
				arr.SetValue(pnt, vertIndex++);
			}
			// stacks and slices
			for (int stack = 1; stack < stacks; stack++)
			{
				//latitude = startLat + ((float)(stack-1)/(stacks-1f)*(float)(endLat - startLat));
				double linear = (float)(stack - 1) / (stacks - 1f);
				double k = 1 - Math.Cos((float)(stack - 1) / (stacks - 1f) * Math.PI / 2);
				latitude = startLat + (k * k * k * (float)(endLat - startLat));
				double alphaFactor = 1 - (linear * linear * linear);	// coef alpha transparency
				if (alphaFactor > .8) alphaFactor = .8f;
				for (int slice = 0; slice <= slices; slice++)
				{
					pnt = new CustomVertex.PositionColored();
					double longitude = startLon - ((float)slice / slices * lonSpan);
					v = MathEngine.SphericalToCartesian(latitude, longitude, radius);
					v.TransformCoordinate(SkyGradientTrans);
					pnt.X = (float)v.X;
					pnt.Y = (float)v.Y;
					pnt.Z = (float)v.Z;
					pnt.Color = getAtmosphereColor(drawArgs, pnt);
					arr.SetValue(pnt, vertIndex++);
				}
			}
			// top fade
			latitude = endLat + ((endLat - startLat) / 10);
			for (int slice = 0; slice <= slices; slice++)
			{
				pnt = new CustomVertex.PositionColored();
				double longitude = startLon - ((float)slice / slices * lonSpan);
				v = MathEngine.SphericalToCartesian(latitude, longitude, radius);
				v.TransformCoordinate(SkyGradientTrans);
				pnt.X = (float)v.X;
				pnt.Y = (float)v.Y;
				pnt.Z = (float)v.Z;
				pnt.Color = System.Drawing.Color.FromArgb(0, 0, 0, 0).ToArgb();
				arr.SetValue(pnt, vertIndex++);
			}

			mesh.VertexBuffer.Unlock();
			ranks[0] = indexCount;
			arr = mesh.LockIndexBuffer(typeof(short), LockFlags.None, ranks);
			int i = 0;
			short bottomVertex = 0;
			short topVertex = 0;

			// stacks and slices
			for (short x = 0; x < stacks; x++)
			{
				bottomVertex = (short)((slices + 1) * x);
				topVertex = (short)(bottomVertex + slices + 1);
				for (int y = 0; y < slices; y++)
				{
					arr.SetValue(bottomVertex, i++);
					arr.SetValue((short)(topVertex + 1), i++);
					arr.SetValue(topVertex, i++);
					arr.SetValue(bottomVertex, i++);
					arr.SetValue((short)(bottomVertex + 1), i++);
					arr.SetValue((short)(topVertex + 1), i++);
					bottomVertex++;
					topVertex++;
				}
			}

			mesh.UnlockIndexBuffer();
			mesh.IndexBuffer.SetData(arr, 0, LockFlags.None);

			return mesh;
		}

		CustomVertex.PositionColored p2 = new CustomVertex.PositionColored();
		/// <summary>
		/// Compute sky vertex color using atmospheric scattering
		/// </summary>
		int getAtmosphereColor(DrawArgs drawArgs, CustomVertex.PositionColored pVertex)
		{
			// Find out intersection point on world scattering sphere
			//Vector3 vPos = new Vector3();
			vPos.X = pVertex.X;
			vPos.Y = pVertex.Y;
			vPos.Z = pVertex.Z;

			// Get the ray from the camera to the vertex
			//Vector3 vCamera = new Vector3();
			vCamera.X = (float)drawArgs.WorldCamera.Position.X;
			vCamera.Y = (float)drawArgs.WorldCamera.Position.Y;
			vCamera.Z = (float)drawArgs.WorldCamera.Position.Z;

			Point3d vRay = vPos - vCamera;
			vRay.normalize();

			// Calculate the closest intersection of the ray with the outer atmosphere 
			float B = (float)(2.0 * Point3d.dot(vCamera, vRay));
			float C = (float)(Point3d.dot(vCamera, vCamera) - m_radius * m_radius);
			float fDet = B * B - 4.0f * C;

			p2.Color = System.Drawing.Color.FromArgb(0, 0, 0, 0).ToArgb();
			if (fDet >= 0)
			{
				// Camera ray intersect atmosphere
				float fNear1 = 0.5f * (-B - (float)Math.Sqrt(fDet));
				float fNear2 = 0.5f * (-B + (float)Math.Sqrt(fDet));
				if (fNear1 >= 0 || fNear2 >= 0)
				{
					// biggest distance - not sure why...
					float fNear = (float)Math.Max(fNear1, fNear2);
					vPos = vCamera + vRay * fNear;
					p2.X = (float)vPos.X;
					p2.Y = (float)vPos.Y;
					p2.Z = (float)vPos.Z;
					SetColor(ref p2, drawArgs);
				}
			}
			return p2.Color;
		}
		// -- End SkyGradiant geometry addition

		static Effect skyFromSpaceEffect = null;

		internal void Render(DrawArgs drawArgs)
		{
			try
			{
				if (m_meshList.Count > 0 && m_opticalDepthBuffer1 != null)
				{
					double horizonSpan = HorizonSpan(drawArgs);
					if (horizonSpan == 0) return;   // Check if horizon visible (PM 2006-11-28)

					if (skyFromSpaceEffect == null)
					{
						drawArgs.device.DeviceReset += new EventHandler(device_DeviceReset);
						device_DeviceReset(drawArgs.device, null);
					}

					vCamera.X = (float)drawArgs.WorldCamera.Position.X;
					vCamera.Y = (float)drawArgs.WorldCamera.Position.Y;
					vCamera.Z = (float)drawArgs.WorldCamera.Position.Z;

					drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
					drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
					if (drawArgs.device.RenderState.Lighting)
						drawArgs.device.RenderState.Lighting = false;

					drawArgs.device.Transform.World = Matrix.Translation(
						(float)-drawArgs.WorldCamera.ReferenceCenter.X,
						(float)-drawArgs.WorldCamera.ReferenceCenter.Y,
						(float)-drawArgs.WorldCamera.ReferenceCenter.Z
						);

					Frustum frustum = new Frustum();

					frustum.Update(
						Matrix4d.Multiply(ConvertDX.ToMatrix4d(drawArgs.device.Transform.World),
						Matrix4d.Multiply(ConvertDX.ToMatrix4d(drawArgs.device.Transform.View), ConvertDX.ToMatrix4d(drawArgs.device.Transform.Projection))));

					// Update Sun
					UpdateLightVector();
					// Use SkyGradient geometry
					UpdateSkyMesh(drawArgs, horizonSpan);
					drawArgs.device.RenderState.CullMode = Cull.Clockwise;
					skyMesh.DrawSubset(0);
					
					drawArgs.device.Transform.World = ConvertDX.FromMatrix4d(drawArgs.WorldCamera.WorldMatrix);
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		// Check if horizon is visible in camera viewport (PM 2006-11-28)
		// Returns the horizon span angle or 0 if not visible
		double HorizonSpan(DrawArgs drawArgs)
		{
			// Camera & Viewport shortcuts
			Camera.CameraBase camera = drawArgs.WorldCamera;
			Viewport viewport = camera.Viewport;

			// Compute camera absolute field of view (to the horizon)
			double fovH = Math.Abs(Math.Asin(camera.WorldRadius / (camera.WorldRadius + camera.Altitude))) * 2;

			// Compute viewport diagonal field of view
			int h = viewport.Height;
			int w = viewport.Width;
			double fovV = camera.Fov.Radians;
			double fovD = Math.Abs(Math.Atan(Math.Sqrt(h * h + w * w) * Math.Tan(fovV / 2) / h)) * 2;

			// Compute camera tilt from vertical at the camera position
			double tilt = camera.Tilt.Radians * 2;
			if (camera.Altitude > 10000)
			{
				double a = camera.WorldRadius;					  // World radius
				double b = camera.WorldRadius + camera.Altitude;	 // Camera to center of planet
				double c = camera.Distance;						 // Distance to target
				tilt = Math.Abs(Math.Acos((a * a - c * c - b * b) / (-2 * c * b))) * 2;
				if (double.IsNaN(tilt)) tilt = 0;
			}

			// Check if cones intersect
			double span = 0;
			if (fovD + tilt > fovH)
			{
				span = fovD < fovH ? Math.Abs(Math.Asin(Math.Sin(fovD / 2) / Math.Sin(fovH / 2))) * 2 : Math.PI * 2;
				span *= 180 / Math.PI;
			}

			return span;
		}

		void device_DeviceReset(object sender, EventArgs e)
		{
			Device device = (Device)sender;

			try
			{
				string outerrors = "";

				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
				Stream skyFromSpaceStream = assembly.GetManifestResourceStream("WorldWind.Shaders.SkyFromSpace.fx");

				skyFromSpaceEffect =
					Effect.FromStream(
					device,
					skyFromSpaceStream,
					null,
					null,
					ShaderFlags.None,
					null,
					out outerrors);

				if (outerrors != null && outerrors.Length > 0)
					Log.Write(Log.Levels.Error, outerrors);

				if (outerrors != null && outerrors.Length > 0)
					Log.Write(Log.Levels.Error, outerrors);

			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		// Updates vLight and vLightDirection according to Sun position
		void UpdateLightVector()
		{
			System.DateTime currentTime = TimeKeeper.CurrentTimeUtc;
			Point3d sunPosition = SunCalculator.GetGeocentricPosition(currentTime);
			Point3d sunVector = new Point3d(
				-sunPosition.X,
				-sunPosition.Y,
				-sunPosition.Z);

			m_vLight = sunVector * 100000000.0;
			m_vLightDirection = new Point3d(
				m_vLight.X / m_vLight.Length,
				m_vLight.Y / m_vLight.Length,
				m_vLight.Z / m_vLight.Length
				);
		}

		void MakeOpticalDepthBuffer(float fInnerRadius, float fOuterRadius, float fRayleighScaleHeight, float fMieScaleHeight)
		{
			int nSize = 128;
			int nSamples = 50;
			float fScale = 1.0f / (fOuterRadius - fInnerRadius);

			if (m_opticalDepthBuffer1 == null)
				m_opticalDepthBuffer1 = new float[nSize * nSize * 4];

			if (m_opticalDepthBuffer2 == null)
				m_opticalDepthBuffer2 = new float[nSize * nSize * 4];

			if (m_currentOpticalBuffer == 1)
			{
				for (int i = 0; i < m_opticalDepthBuffer2.Length; i++)
				{
					m_opticalDepthBuffer2[i] = 0;
				}
			}
			else
			{
				for (int i = 0; i < m_opticalDepthBuffer1.Length; i++)
				{
					m_opticalDepthBuffer1[i] = 0;
				}
			}

			m_nWidth = nSize;
			m_nHeight = nSize;
			//m_nDepth = 1;
			//m_nDataType = 4;
			m_nChannels = 4;
			int nIndex = 0;
			for (int nAngle = 0; nAngle < nSize; nAngle++)
			{
				// As the y tex coord goes from 0 to 1, the angle goes from 0 to 180 degrees
				float fCos = 1.0f - (nAngle + nAngle) / (float)nSize;
				float fAngle = (float)Math.Acos(fCos);

				Point3d vRay = new Point3d(Math.Sin(fAngle), Math.Cos(fAngle), 0);	// Ray pointing to the viewpoint
				for (int nHeight = 0; nHeight < nSize; nHeight++)
				{
					// As the x tex coord goes from 0 to 1, the height goes from the bottom of the atmosphere to the top
					float fHeight = DELTA + fInnerRadius + ((fOuterRadius - fInnerRadius) * nHeight) / nSize;
					Point3d vPos = new Point3d(0, fHeight, 0);				// The position of the camera

					// If the ray from vPos heading in the vRay direction intersects the inner radius (i.e. the planet), then this spot is not visible from the viewpoint
					float B = 2.0f * (float)Point3d.dot(vPos, vRay);
					float Bsq = B * B;
					float Cpart = (float)Point3d.dot(vPos, vPos);
					float C = Cpart - fInnerRadius * fInnerRadius;
					float fDet = Bsq - 4.0f * C;
					bool bVisible = (fDet < 0 || (0.5f * (-B - (float)Math.Sqrt(fDet)) <= 0) && (0.5f * (-B + (float)Math.Sqrt(fDet)) <= 0));
					float fRayleighDensityRatio;
					float fMieDensityRatio;
					if (bVisible)
					{
						fRayleighDensityRatio = (float)Math.Exp(-(fHeight - fInnerRadius) * fScale / fRayleighScaleHeight);
						fMieDensityRatio = (float)Math.Exp(-(fHeight - fInnerRadius) * fScale / fMieScaleHeight);
					}
					else
					{
						if (m_currentOpticalBuffer == 1)
						{
							// Smooth the transition from light to shadow (it is a soft shadow after all)
							fRayleighDensityRatio = m_opticalDepthBuffer2[nIndex - nSize * m_nChannels] * 0.75f;
							fMieDensityRatio = m_opticalDepthBuffer2[nIndex + 2 - nSize * m_nChannels] * 0.75f;
						}
						else
						{
							// Smooth the transition from light to shadow (it is a soft shadow after all)
							fRayleighDensityRatio = m_opticalDepthBuffer1[nIndex - nSize * m_nChannels] * 0.75f;
							fMieDensityRatio = m_opticalDepthBuffer1[nIndex + 2 - nSize * m_nChannels] * 0.75f;
						}

					}

					// Determine where the ray intersects the outer radius (the top of the atmosphere)
					// This is the end of our ray for determining the optical depth (vPos is the start)
					C = Cpart - fOuterRadius * fOuterRadius;
					fDet = Bsq - 4.0f * C;
					float fFar = 0.5f * (-B + (float)Math.Sqrt(fDet));

					// Next determine the length of each sample, scale the sample ray, and make sure position checks are at the center of a sample ray
					float fSampleLength = fFar / nSamples;
					float fScaledLength = fSampleLength * fScale;
					Point3d vSampleRay = vRay * fSampleLength;
					vPos += vSampleRay * 0.5f;

					// Iterate through the samples to sum up the optical depth for the distance the ray travels through the atmosphere
					float fRayleighDepth = 0;
					float fMieDepth = 0;
					for (int i = 0; i < nSamples; i++)
					{
						fHeight = (float)vPos.Length;
						float fAltitude = (fHeight - fInnerRadius) * fScale;
						fAltitude = (float)Math.Max(fAltitude, 0.0f);
						fRayleighDepth += (float)Math.Exp(-fAltitude / fRayleighScaleHeight);
						fMieDepth += (float)Math.Exp(-fAltitude / fMieScaleHeight);
						vPos += vSampleRay;
					}

					// Multiply the sums by the length the ray traveled
					fRayleighDepth *= fScaledLength;
					fMieDepth *= fScaledLength;

					// Store the results for Rayleigh to the light source, Rayleigh to the camera, Mie to the light source, and Mie to the camera
					if (m_currentOpticalBuffer == 1)
					{
						m_opticalDepthBuffer2[nIndex++] = fRayleighDensityRatio;
						m_opticalDepthBuffer2[nIndex++] = fRayleighDepth;
						m_opticalDepthBuffer2[nIndex++] = fMieDensityRatio;
						m_opticalDepthBuffer2[nIndex++] = fMieDepth;
					}
					else
					{
						m_opticalDepthBuffer1[nIndex++] = fRayleighDensityRatio;
						m_opticalDepthBuffer1[nIndex++] = fRayleighDepth;
						m_opticalDepthBuffer1[nIndex++] = fMieDensityRatio;
						m_opticalDepthBuffer1[nIndex++] = fMieDepth;
					}
				}
			}

			if (m_currentOpticalBuffer == 1)
				m_currentOpticalBuffer = 2;
			else
				m_currentOpticalBuffer = 1;

		}
		int m_currentOpticalBuffer = 2;
		float[] m_opticalDepthBuffer1 = null;
		float[] m_opticalDepthBuffer2 = null;
	}
}

//----------------------------------------------------------------------------
// NAME: 3Dconnexion
// DESCRIPTION:  Driver for 3- dim input device from 3dconnexion,  1.0.3
// VERSION: 1.0.3
// DEVELOPER: Wolfgang Roessl
// WEBSITE: http://www.3dconnexion.com
// LASTUPDATE: 10. march 2007  W.Roessl
//----------------------------------------------------------------------------
// 1.0.1  07/02/26 first beta Release Wolfgang Roessl; 
// 1.0.2  07/03/08 transform earth position in one step;
//                 release Com- classes at "Unload()";
//                 bugfix: no rotational DOF when switching between planets;
// 1.0.3  07/03/09 code cleaning; TDxInput wrapper completed; smaller bugfixes;
//----------------------------------------------------------------------------
//
// This file is in the internal Domain, and comes with no warranty. 
//

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

//needed by 3Dconnexion SDK
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Win32;
using System.Diagnostics;

using WorldWind;
using WorldWind.Camera;
using WorldWind.Interop;

using WorldWind.PluginEngine;
using System.Globalization;

namespace ThreeDconnexion.Plugin
{

    /// <summary>
    /// interface of the Plugin used by the Plugin dialog
    /// </summary>
    internal interface I3DxPlugin
    {
        void SetObjectMode();
        void SetCameraMode();
        void GetMode(out int nAxisSet, out bool bDynamic_p);
    }

    /// <summary>
    /// The dialog form of the 3Dconnexion Plugin.
    /// </summary>
    internal class CTDxWWWInputDialog : Form
    {
        ///<value>the plugin interface, used to set the behaviour of the plugin</value>
        I3DxPlugin m_IPlugin = null;

        //GUI member
        private GroupBox GrBxAxisSetting;
        private RadioButton RaBuObjectMode;
        private Button ButOK;
        private RadioButton RaBuCameraMode;


        //constructor/destructor
        internal CTDxWWWInputDialog(ref I3DxPlugin ThePlugin_p)
            : base()
        {
            InitializeComponent();

            m_IPlugin = ThePlugin_p;

            int AxisSetting;
            bool bDynamic;
            m_IPlugin.GetMode(out AxisSetting, out bDynamic);

            if (AxisSetting == 0)
                RaBuObjectMode.Checked = true;
            else
                RaBuCameraMode.Checked = true;

        }


        private void InitializeComponent()
        {
            this.GrBxAxisSetting = new System.Windows.Forms.GroupBox();
            this.RaBuCameraMode = new System.Windows.Forms.RadioButton();
            this.RaBuObjectMode = new System.Windows.Forms.RadioButton();
            this.ButOK = new System.Windows.Forms.Button();
            this.GrBxAxisSetting.SuspendLayout();
            this.SuspendLayout();
            // 
            // GrBxAxisSetting
            // 
            this.GrBxAxisSetting.Controls.Add(this.RaBuCameraMode);
            this.GrBxAxisSetting.Controls.Add(this.RaBuObjectMode);
            this.GrBxAxisSetting.Location = new System.Drawing.Point(12, 12);
            this.GrBxAxisSetting.Name = "GrBxAxisSetting";
            this.GrBxAxisSetting.Size = new System.Drawing.Size(270, 122);
            this.GrBxAxisSetting.TabIndex = 0;
            this.GrBxAxisSetting.TabStop = false;
            this.GrBxAxisSetting.Text = "Axis Setting";
            // 
            // RaBuCameraMode
            // 
            this.RaBuCameraMode.AutoSize = true;
            this.RaBuCameraMode.Location = new System.Drawing.Point(20, 41);
            this.RaBuCameraMode.Name = "RaBuCameraMode";
            this.RaBuCameraMode.Size = new System.Drawing.Size(154, 17);
            this.RaBuCameraMode.TabIndex = 2;
            this.RaBuCameraMode.TabStop = true;
            this.RaBuCameraMode.Text = "User Based (move yourself)";
            this.RaBuCameraMode.UseVisualStyleBackColor = true;
            // 
            // RaBuObjectMode
            // 
            this.RaBuObjectMode.AutoSize = true;
            this.RaBuObjectMode.Location = new System.Drawing.Point(20, 77);
            this.RaBuObjectMode.Name = "RaBuObjectMode";
            this.RaBuObjectMode.Size = new System.Drawing.Size(145, 17);
            this.RaBuObjectMode.TabIndex = 1;
            this.RaBuObjectMode.TabStop = true;
            this.RaBuObjectMode.Text = "Earth Based (move earth)";
            this.RaBuObjectMode.UseVisualStyleBackColor = true;
            // 
            // ButOK
            // 
            this.ButOK.Location = new System.Drawing.Point(299, 22);
            this.ButOK.Name = "ButOK";
            this.ButOK.Size = new System.Drawing.Size(75, 23);
            this.ButOK.TabIndex = 1;
            this.ButOK.Text = "OK";
            this.ButOK.UseVisualStyleBackColor = true;
            this.ButOK.Click += new System.EventHandler(this.OK_Button_Click);
            // 
            // CTDxWWWInputDialog
            // 
            this.ClientSize = new System.Drawing.Size(391, 157);
            this.Controls.Add(this.ButOK);
            this.Controls.Add(this.GrBxAxisSetting);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CSpaceMouseDialog";
            this.Text = "3Dconnexion WorldWind Axis Settings";
            this.GrBxAxisSetting.ResumeLayout(false);
            this.GrBxAxisSetting.PerformLayout();
            this.ResumeLayout(false);

            this.Icon = Dapple.Properties.Resources._3dxicon;

        }//InitializeComponent

        //Event handler
        private void OK_Button_Click(object sender, EventArgs e)
        {
            if (m_IPlugin == null)
                return;

            if (RaBuObjectMode.Checked)
                m_IPlugin.SetObjectMode();
            else
                m_IPlugin.SetCameraMode();

            this.Hide();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Class:       TDxWWInput formerly SpaceMouse 
    // description: the main plugin class 
    // Last Update: 2007-03-09
    //////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// The 3DConnexion Plugin for WorldWind 
    /// </summary>
    internal class TDxWWInput : WorldWind.PluginEngine.Plugin, I3DxPlugin
    {
        private enum AxisMapping
        {
            TDXOBJECTMODE = 0,
            TDXCAMERAMODE = 1,
            TDXUNKNOWN = 8
        }

        //increment this values if you want to decrement the sensitivity of the Input device
        //for single axis
        private const double NormTilt = 1.0E5;
        private const double NormHeading = 2.0E5;
        private const double NormDistance = 3.5E4;
        private const double NormLatLon = 3.0E4;

        System.Windows.Forms.ToolStripMenuItem menuItem;

        AxisMapping m_AxisMode = AxisMapping.TDXUNKNOWN;
        TDconnexion.TDxDeviceWrapper m_TheInputDevice = null;
        static TDconnexion.I3DxSensor s_TheSensor = null;
        static TDconnexion.I3DxKeyboard s_TheKeyBoard = null;
        static WorldWindow s_WW = null;
        static CameraBase s_TheCamera = null;
        static Point3d s_Position;
        TDconnexion.TDxSensorInputEvent m_SensorEventHandler = null;
        TDconnexion.TDxKeyboardEvent m_KeyEventHandler = null;
        static double s_dLastHeight = 0;
        static double s_dDeltaHeight = 0;
        static private Stopwatch s_stopWatch;  //check event interval of the input device

        internal TDxWWInput()
        {
        }

		  public override void Load()
        {
            base.Load();

            // Add our plugin to the World Wind Tools menu 
            menuItem = new System.Windows.Forms.ToolStripMenuItem();
            menuItem.Text = "3Dconnexion Input Device";
            menuItem.Click += new System.EventHandler(menuItem_Click);
            s_stopWatch = new Stopwatch();
            s_WW = m_Application.WorldWindow;

            s_TheCamera = s_WW.DrawArgs.WorldCamera;

            m_TheInputDevice = new TDconnexion.TDxDeviceWrapper();
            if (m_TheInputDevice != null)
            {
					if (m_TheInputDevice.InitializationSuccessful)
					{
						// JBTODO: m_app.MenuStrip.Items.Add(menuItem);

						s_TheSensor = m_TheInputDevice.Sensor;
						s_TheKeyBoard = m_TheInputDevice.Keyboard;
						s_Position = new Point3d();
						SetCameraMode();
						m_KeyEventHandler = new TDconnexion.TDxKeyboardEvent(KeyboardEventHandler);
						s_TheKeyBoard.KeyboardEventDOWN += m_KeyEventHandler;
						m_TheInputDevice.Connect();
					}
					else
					{
						m_TheInputDevice = null;
					}
            }
        }

		  public override void Unload()
        {
            if (m_TheInputDevice != null)
                m_TheInputDevice.Disconnect();


            if ((m_KeyEventHandler != null) && (s_TheKeyBoard != null))
                s_TheKeyBoard.KeyboardEventDOWN -= m_KeyEventHandler;
            if ((m_SensorEventHandler != null) && (s_TheSensor != null))
                s_TheSensor.SensorInput -= m_SensorEventHandler;

            m_KeyEventHandler = null;
            m_SensorEventHandler = null;
            s_TheSensor = null;
            s_TheCamera = null;

            s_dLastHeight = 0;
            s_dDeltaHeight = 0;
            s_stopWatch = null;  //check event interval of the input device
            s_TheCamera = null;
            s_WW = null;

            if (m_TheInputDevice != null)
                m_TheInputDevice.Release();

            m_TheInputDevice = null;

            // Clean up, remove menu item
            // JBTODO: m_app.MenuStrip.Items.Remove(menuItem);
            base.Unload();
        }

        void menuItem_Click(object sender, EventArgs e)
        {
            // Fired when user clicks our main menu item.
            I3DxPlugin IPlugin = (I3DxPlugin)this;
            CTDxWWWInputDialog Diag = new CTDxWWWInputDialog(ref IPlugin);
            Diag.ShowDialog();
        }

        #region I3DxPlugin implementation
		  public void SetObjectMode()
        {
            if (s_TheSensor != null)
            {
                if (m_SensorEventHandler != null)
                    s_TheSensor.SensorInput -= m_SensorEventHandler;

                m_SensorEventHandler = new TDconnexion.TDxSensorInputEvent(ObjectMode);
                s_TheSensor.SensorInput += m_SensorEventHandler;
                m_AxisMode = AxisMapping.TDXOBJECTMODE;
            }
        }//SetObjectMode

		  public void SetCameraMode()
        {
            if (s_TheSensor != null)
            {
                if (m_SensorEventHandler != null)
                    s_TheSensor.SensorInput -= m_SensorEventHandler;

                m_SensorEventHandler = new TDconnexion.TDxSensorInputEvent(CameraMode);
                s_TheSensor.SensorInput += m_SensorEventHandler;
                m_AxisMode = AxisMapping.TDXCAMERAMODE;
            }
        }//SetHelicopterMode

		  public void GetMode(out int nAxisSet, out bool bDynamic_p)
        {
            nAxisSet = (int)m_AxisMode; //TODO: remove cast replace parameter with enum
            bDynamic_p = false;
        }//GetMode

        #endregion //I3DxPlugin implementation

        /// <summary>
        /// used as delegate for the "Object Mode" -axis mapping, moving the earth
        /// </summary>
        private static void ObjectMode()
        {
            long delta = 1;
            if (s_stopWatch.IsRunning)
            {
                delta = s_stopWatch.ElapsedMilliseconds;

                //ToDo: remove following two lines;
                //just a performance improvement for 3DxWare Version < 6.1.8.
                if (delta < s_TheSensor.Period)
                    return;

                s_stopWatch.Stop();

                if (delta > (10 * s_TheSensor.Period))
                    return;
            }

            //get data from the Input Device
            TDconnexion.C3DxVector TranslVector = s_TheSensor.Translation as TDconnexion.C3DxVector;
            TDconnexion.C3DxRotation AngleAxis = s_TheSensor.Rotation as TDconnexion.C3DxRotation;

            double dNormTime = (double)delta / s_TheSensor.Period;
            TranslVector *= dNormTime;
            AngleAxis *= dNormTime;

            //0- event sent when cap released
            if ((AngleAxis.Angle == 0)
                && (TranslVector.X == 0)
                && (TranslVector.Y == 0)
                && (TranslVector.Z == 0)
                )
            {
                s_stopWatch.Stop();
                return;
            }
            s_TheCamera = s_WW.DrawArgs.WorldCamera;
            //set CameraSmooth to false to disable movement after releasing the cap
            bool bBackUpSmooth = World.Settings.CameraSmooth;
            World.Settings.CameraSmooth = false;

            //// Tilt////////////////////////////
            s_TheCamera.Tilt -= Angle.FromRadians(AngleAxis.X * AngleAxis.Angle * WorldSettings.CameraRotationSpeed / NormTilt);

            //// Heading/////////////////////////
            double dHeading = -AngleAxis.Y * AngleAxis.Angle * WorldSettings.CameraRotationSpeed / NormHeading;

            //// Distance///////////////////////
            s_TheCamera.TargetDistance *= (1 + TranslVector.Y / NormDistance);

            double factor = (s_TheCamera.Altitude) / (-NormLatLon * s_WW.CurrentWorld.EquatorialRadius);

             Quaternion4d Orientation
                = Quaternion4d.RotationYawPitchRoll(s_TheCamera.Longitude.Radians + (TranslVector.X * factor)
                                                 , s_TheCamera.Latitude.Radians + (TranslVector.Z * factor)
                                                 , dHeading);

             s_TheCamera.CurrentOrientation = Orientation;
             s_Position = Quaternion4d.QuaternionToEuler(Orientation);


             s_WW.SetViewPosition(Angle.FromRadians(s_Position.Y).Degrees
                                      , Angle.FromRadians(s_Position.X).Degrees
                                      , s_TheCamera.Altitude);

            World.Settings.CameraSmooth = bBackUpSmooth;

            s_stopWatch.Reset();
            s_stopWatch.Start();
        }//ObjectMode()


        /// <summary>
        /// calculates the altitude from the distance and tilt angle for a given radius of the planet; 
        /// </summary>
        /// <remarks> 
        /// using law of cosine; copied from WorldWind
        /// </remarks>
        protected double DDDxCpteAlti(double distance, Angle tilt, double Radius)
        {
            double dfromeq = Math.Sqrt(Radius * Radius + distance * distance -
               2 * Radius * distance * Math.Cos(Math.PI - tilt.Radians));
            double alt = dfromeq - Radius;
            return (alt);
        }//DDDxCpteAlti

        /// <summary>
        /// calculates the distance from the altitude and tilt angle for a given radius of the planet; 
        /// </summary>
        /// <remarks> 
        /// using law of cosine; copied from WorldWind
        /// </remarks>
        protected static double DDxCpteDist(double altitude, Angle tilt, double Radius)
        {
            double cos = Math.Cos(Math.PI - tilt.Radians);
            double x = Radius * cos;
            double hyp = Radius + altitude;
            double y = Math.Sqrt(Radius * Radius * cos * cos + hyp * hyp - Radius * Radius);
            double res = x - y;
            if (res < 0)
                res = x + y;

            return (res);
        }//DDxCpteDist


        /// <summary>
        /// used as delegate for the "Camera Mode" -axis mapping, moving the Camera/Yourself
        /// </summary>
        private static void CameraMode()
        {
            long delta = 1;
            if (s_stopWatch.IsRunning)
            {
                delta = s_stopWatch.ElapsedMilliseconds;

                //ToDo: remove just a performance improvement for 3DxWare Version < 6.1.8
                if (delta < s_TheSensor.Period)
                    return;

                s_stopWatch.Stop();

                if (delta > (10 * s_TheSensor.Period))
                    return;
                //Console.WriteLine(delta.ToString());
                //Debug.Print(delta.ToString());
            }

            //get data from the Input Device
            TDconnexion.C3DxVector TranslVector = s_TheSensor.Translation as TDconnexion.C3DxVector;
            TDconnexion.C3DxRotation AngleAxis = s_TheSensor.Rotation as TDconnexion.C3DxRotation;

            double dNormTime = (double)delta / s_TheSensor.Period;
            TranslVector *= dNormTime;
            AngleAxis *= dNormTime;

            //0- event sent when cap released
            if ((AngleAxis.Angle == 0)
                && (TranslVector.X == 0)
                && (TranslVector.Y == 0)
                && (TranslVector.Z == 0)
                )
            {
                s_dLastHeight = s_TheCamera.Altitude;
                s_dDeltaHeight = 0;
                s_stopWatch.Stop();
                return;
            }

            s_TheCamera = s_WW.DrawArgs.WorldCamera;

            //set CameraSmooth to false to disable movement after releasing the cap
            bool bBackUpSmooth = World.Settings.CameraSmooth;
            World.Settings.CameraSmooth = false;


            //variables to move the earth via Quaternion4d
            double dRadius = s_TheCamera.WorldRadius;
            double dAltitude = s_TheCamera.Altitude;         //height above sealevel of the camera
            Angle rTilt = s_TheCamera.Tilt;                  //angle between line [center of earth ... crosshairs] and line [crosshairs ... camera]
            double dDeltaLatitude = 0;                       //change of Latitude
            double dDeltaLongitude = 0;                      //change of Longitude
            double dSinus = Math.Sin(rTilt.Radians);
            double dLength = dAltitude + dRadius;            //distance earth center - camera
            double dSinusLaw = dSinus / dLength;               //const of law of sines
            double dAlpha = rTilt.Radians - Math.Asin(dRadius * dSinusLaw); //angle between crosshairs and intersection of line [camera ... center of earth] and earth- face

            //////////////////////////////////////////////////////////////////////////////
            /// Tilt, camera position == center of rotation -> adjust latitude/distance///
            //////////////////////////////////////////////////////////////////////////////
            double dDeltaTiltRad = AngleAxis.X * AngleAxis.Angle * WorldSettings.CameraRotationSpeed / NormTilt; //150000.0;
            if (Math.Abs(dDeltaTiltRad) > double.Epsilon)
            {
                rTilt += Angle.FromRadians(dDeltaTiltRad);
                if (rTilt.Degrees > 85.0)
                    rTilt.Degrees = 85.0;

                if (Math.Abs(rTilt.Radians) > double.Epsilon)
                {
                    dSinus = Math.Sin(rTilt.Radians);
                    dSinusLaw = dSinus / dLength;
                    dDeltaLatitude = -dAlpha;
                    dAlpha = rTilt.Radians - Math.Asin(dRadius * dSinusLaw); // == dAlpha after Tilt
                    dDeltaLatitude += dAlpha;
                }
            }

            ///////////////////////////////////////////////////////////////////////////////////////
            /// Camera height: change Distance and Position (Tilt != 0) in order to move radial ///
            ///////////////////////////////////////////////////////////////////////////////////////
            bool bPosOK = false;
            if (s_dDeltaHeight < -double.Epsilon)
            {
                if (dAltitude < (s_dLastHeight + (s_dDeltaHeight * 0.5)))
                    bPosOK = true;
            }
            else if (s_dDeltaHeight > double.Epsilon)
            {
                if (dAltitude > (s_dLastHeight + (s_dDeltaHeight * 0.5)))
                    bPosOK = true;
            }
            else
                bPosOK = true;

            s_dLastHeight = dAltitude;
            s_dDeltaHeight = TranslVector.Z * dAltitude / NormDistance; //50000.0;

            if (Math.Abs(dSinus) > double.Epsilon)
            {
                dAltitude += s_dDeltaHeight;

                if (bPosOK)
                {
                    dLength = dAltitude + dRadius;
                    if (rTilt.Degrees < 85.0)
                    {
                        double dTilt = Math.Asin(dLength * dSinusLaw); //==new tilt                 
                        if (Angle.FromRadians(dTilt).Degrees <= 85.0)
                        {
                            dDeltaLatitude += dTilt - rTilt.Radians;
                            rTilt.Radians = dTilt;
                        }
                        else
                            rTilt.Degrees = 85.0;
                    }
                    else
                    {//only change latitude, tilt is fixed Tilt = 85.0
                        dDeltaLatitude += Math.Asin(dRadius * dSinusLaw);
                        dSinusLaw = dSinus / dLength;
                        dDeltaLatitude -= Math.Asin(dRadius * dSinusLaw);
                    }
                }
            }
            else
                dAltitude += s_dDeltaHeight;

            s_TheCamera.Tilt = rTilt;
            s_TheCamera.TargetDistance = DDxCpteDist(dAltitude, s_TheCamera.Tilt, dRadius);


            ///////////////////////////////////////////////////////////////////////////////////////
            /// Latitude/Longitude                                                              ///
            ///////////////////////////////////////////////////////////////////////////////////////
            double factor = s_TheCamera.Altitude / (s_WW.CurrentWorld.EquatorialRadius * NormLatLon);//50000.0);
            dDeltaLatitude += TranslVector.Y * factor;
            dDeltaLongitude += TranslVector.X * factor;



            ///////////////////////////////////////////////////////////////////////////////////////
            /// Heading                                                                         ///
            ///////////////////////////////////////////////////////////////////////////////////////
            double dHeading = -AngleAxis.Z * AngleAxis.Angle * WorldSettings.CameraRotationSpeed / NormHeading; // 250000.0;

            ///////////////////////////////////////////////////////////////////////////////////////
            /// set the new View/Camera - position                                              ///
            /////////////////////////////////////////////////////////////////////////////////////// 
            Quaternion4d TiltQuat
               = Quaternion4d.RotationYawPitchRoll(0
                                                , -dAlpha
                                                , 0);

            Quaternion4d TiltQuatInv
               = Quaternion4d.RotationYawPitchRoll(0
                                                , dAlpha
                                                , 0);

            Quaternion4d SpMoQuat
               = Quaternion4d.RotationYawPitchRoll(dDeltaLongitude
                                                , dDeltaLatitude
                                                , dHeading);

            Quaternion4d OrientNew = TiltQuatInv * SpMoQuat * TiltQuat;
            OrientNew *= s_TheCamera.CurrentOrientation;
            s_TheCamera.CurrentOrientation = OrientNew;

            s_Position = Quaternion4d.QuaternionToEuler(OrientNew);
            s_TheCamera.Heading = Angle.FromRadians(s_Position.Z);
            s_WW.SetViewPosition(Angle.FromRadians(s_Position.Y).Degrees
                                     , Angle.FromRadians(s_Position.X).Degrees
                                     , dAltitude);


            World.Settings.CameraSmooth = bBackUpSmooth;
            s_stopWatch.Reset();
            s_stopWatch.Start();
        }//CameraMode

        /// <summary>
        /// used as delegate for the Keyboard Event Handler 
        /// </summary>
        /// <param name="nKey_p"></param>
        private static void KeyboardEventHandler(Int32 nKey_p)
        {
            switch (nKey_p)
            {
                case 31:
                    s_TheCamera.Heading = Angle.FromRadians(0.0);
                    s_WW.SetViewPosition(s_TheCamera.Latitude.Degrees
                                              , s_TheCamera.Longitude.Degrees
                                              , s_TheCamera.Altitude);
                    s_TheCamera.Bank = Angle.FromDegrees(0);
                    s_TheCamera.Tilt = Angle.FromDegrees(0);
                    break;
                default:
                    break;
            }
        }//KeyboardEventHandler
    }

    /// <summary>
    /// 3DConnexion SDK Com- Co class wrapper
    /// </summary>
    /// <remarks>
    /// Late binding will be used to connect WorldWind to the 3Dconnexion SDK, which is COM based.
    /// Therefore each 3Dconnexion Com- Class will be wrapped by a C# class using "Reflection"
    /// </remarks>
    #region TDconnexion SDK Com-Co wrapper

    namespace TDconnexion
    {
        /// <summary>
        /// typedefs of the delegates used for Event- handling  
        /// </summary>
        internal delegate void TDxSensorInputEvent();
        internal delegate void TDxKeyboardEvent(Int32 nKey_p);

        #region COM- Class- Interfaces

        /// <summary>
        /// Interface of the Translation Com- class, 
        /// </summary>
        internal interface I3DxVector
        {
            double X
            {
                get;
                set;
            }

            double Y
            {
                get;
                set;
            }

            double Z
            {
                get;
                set;
            }
        }

        /// <summary>
        /// Interface of the Rotation Com- class, 
        /// 4 dim "Vector": 1 unit-vector with 3dim and the rotation angle in arbitrary units; 
        /// </summary>
        internal interface I3DxRotation
        {
            double X
            {
                get;
                set;
            }

            double Y
            {
                get;
                set;
            }

            double Z
            {
                get;
                set;
            }

            double Angle
            {
                get;
                set;
            }
        }

        /// <summary>
        /// Interface of the Sensor Com- class, acts as event source
        /// </summary>
        internal interface I3DxSensor
        {
            I3DxVector Translation
            {
                get;
            }

            I3DxRotation Rotation
            {
                get;
            }

            double Period
            {
                get;
            }

            event TDxSensorInputEvent SensorInput;
        }

        /// <summary>
        /// Interface of the Keyboard (Keyboard from the 3Dconnexion Device) Com- class; 
        /// </summary>
        internal interface I3DxKeyboard
        {
            Int32 NumOfKeys
            {
                get;
            }

            Int32 NumOfProgKeys
            {
                get;
            }

            string GetKeyLabel(Int32 nKey_p);
            string GetKeyName(Int32 nKey_p);

            event TDxKeyboardEvent KeyboardEventUP;
            event TDxKeyboardEvent KeyboardEventDOWN;
        }

        #endregion

        #region Public Wrapper Classes

        /// <summary>
        /// wrapper class for the Com-Co class TDxInput.Vector3D
        /// </summary>
        internal sealed class C3DxVector : I3DxVector
        {
            internal C3DxVector(ref double X_p, ref double Y_p, ref double Z_p)
            {
                m_nX = X_p;
                m_nY = Y_p;
                m_nZ = Z_p;
            }

            internal C3DxVector()
            {
                m_nX = 0.0;
                m_nY = 0.0;
                m_nZ = 0.0;
            }

            internal C3DxVector(ref C3DxVector rCopy_p)
            {
                m_nX = rCopy_p.m_nX;
                m_nY = rCopy_p.m_nY;
                m_nZ = rCopy_p.m_nZ;
            }

				public double X
            {
                get
                {
                    return m_nX;
                }
                set
                {
                    m_nX = value;
                }
            }
            double m_nX;

				public double Y
            {
                get
                {
                    return m_nY;
                }
                set
                {
                    m_nY = value;
                }
            }
            double m_nY;

				public double Z
            {
                get
                {
                    return m_nZ;
                }
                set
                {
                    m_nZ = value;
                }
            }
            double m_nZ;

				public static C3DxVector operator *(C3DxVector oVector_p, double dScalar_p)
            {
                C3DxVector oReturn = new C3DxVector(ref oVector_p);
                oReturn.m_nX *= dScalar_p;
                oReturn.m_nY *= dScalar_p;
                oReturn.m_nZ *= dScalar_p;
                return (oReturn);
            }

				public static C3DxVector operator *(double dScalar_p, C3DxVector oVector_p)
            {
                return (oVector_p * dScalar_p);
            }
        }


        /// <summary>
        /// wrapper class for the Com-Co class TDxInput.AngleAxis
        /// </summary>
        internal sealed class C3DxRotation : I3DxRotation
        {
            internal C3DxRotation(C3DxRotation rCopy_p)
            {
                m_dX = rCopy_p.m_dX;
                m_dY = rCopy_p.m_dY;
                m_dZ = rCopy_p.m_dZ;
                m_dAng = rCopy_p.m_dAng;
            }

            internal C3DxRotation()
            {
                m_dX = 0.0;
                m_dY = 0.0;
                m_dZ = 0.0;
                m_dAng = 0.0;
            }

				public double X
            {
                get
                {
                    return m_dX;
                }
                set
                {
                    m_dX = value;
                }
            }
            double m_dX;

				public double Y
            {
                get
                {
                    return m_dY;
                }
                set
                {
                    m_dY = value;
                }
            }
            double m_dY;

				public double Z
            {
                get
                {
                    return m_dZ;
                }
                set
                {
                    m_dZ = value;
                }
            }
            double m_dZ;

				public double Angle
            {
                get
                {
                    return m_dAng;
                }
                set
                {
                    m_dAng = value;
                }
            }
            double m_dAng;

				public static C3DxRotation operator *(C3DxRotation oAngleAxis_p, double dScalar_p)
            {
                C3DxRotation oReturn = new C3DxRotation(oAngleAxis_p);
                oReturn.m_dAng *= dScalar_p;
                return (oReturn);
            }

				public static C3DxRotation operator *(double dScalar_p, C3DxRotation oAngleAxis_p)
            {
                return (oAngleAxis_p * dScalar_p);
            }

        }

        /// <summary>
        /// wrapper class for the Com-Co class TDxInput.Sensor
        /// </summary>
        internal sealed class C3DxSensor : I3DxSensor
        {
            #region attributes
            static object s_oComSensor;
            Type m_tyComSensor;
            static MethodInfo s_miTranslation;
            static MethodInfo s_miRotation;
            static C3DxVector s_TranslVec = new C3DxVector();
            static C3DxRotation s_AngleAxis = new C3DxRotation();
            static MethodInfo s_miGetX;
            static MethodInfo s_miGetY;
            static MethodInfo s_miGetZ;

            static MethodInfo s_miRotGetX;
            static MethodInfo s_miRotGetY;
            static MethodInfo s_miRotGetZ;
            static MethodInfo s_miRotGetAngle;
            #endregion //attributes

            /// <summary>
            /// wrapper internal Event handler, which updates the attributes
            /// </summary>
            static internal void UpdateData()
            {
                UpdateVector();
                UpdateAngleAxis();
            }


            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="r3DxComSensor_p"></param>
            internal C3DxSensor(ref object r3DxComSensor_p)
            {
                s_oComSensor = r3DxComSensor_p;

                m_tyComSensor = s_oComSensor.GetType();
                //MethodInfo[] AllSensorMethods = m_tyComSensor.GetMethods();

                //Translation property
                s_miTranslation = m_tyComSensor.GetMethod("get_Translation");
                object result = s_miTranslation.Invoke(s_oComSensor, null);
                Type ty3DxVector = result.GetType();
                //MethodInfo[] TheMethods = ty3DxVector.GetMethods();
                s_miGetX = ty3DxVector.GetMethod("get_X");
                s_miGetY = ty3DxVector.GetMethod("get_Y");
                s_miGetZ = ty3DxVector.GetMethod("get_Z");

                //average time between two Sensor-Events when cap is moved
                MethodInfo Period = m_tyComSensor.GetMethod("get_Period");
                m_dPeriod = (double)Period.Invoke(s_oComSensor, null);

                //Rotation property
                //m_miRotation = m_tyComSensor.GetMethod("get_Rotation", new Type[0]);
                s_miRotation = m_tyComSensor.GetMethod("get_Rotation");
                result = s_miRotation.Invoke(s_oComSensor, null);
                Type ty3DxRotation = result.GetType();
                //TheMethods = ty3DxRotation.GetMethods();
                s_miRotGetX = ty3DxRotation.GetMethod("get_X");
                s_miRotGetY = ty3DxRotation.GetMethod("get_Y");
                s_miRotGetZ = ty3DxRotation.GetMethod("get_Z");
                s_miRotGetAngle = ty3DxRotation.GetMethod("get_Angle");

                SensorInput += new TDxSensorInputEvent(UpdateData);
            }

				public event TDxSensorInputEvent SensorInput
            {
                add
                {
                    try
                    {
                    		EventInfo TheSensorEvent = m_tyComSensor.GetEvent("SensorInput");
								if (TheSensorEvent != null && TheSensorEvent.EventHandlerType != null)
								{
									Delegate TheDelegate = Delegate.CreateDelegate(TheSensorEvent.EventHandlerType
																								  , value.Method);
									TheSensorEvent.AddEventHandler(s_oComSensor, TheDelegate);
								}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0} Exception caught.", e);
                    }
                }

                remove
                {
                    try
                    {
                        EventInfo TheSensorEvent = m_tyComSensor.GetEvent("SensorInput");
								if (TheSensorEvent != null && TheSensorEvent.EventHandlerType != null)
								{
									Delegate TheDelegate = Delegate.CreateDelegate(TheSensorEvent.EventHandlerType
									                                               , value.Method);
									TheSensorEvent.RemoveEventHandler(s_oComSensor, TheDelegate);
								}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0} Exception caught.", e);
                    }
                }
            }

            private static void UpdateVector()
            {
                //object[] ZeroParameter = new object[0];
                //object Vector = m_miTranslation.Invoke(m_oComSensor, ZeroParameter);
                object Vector = s_miTranslation.Invoke(s_oComSensor, null);
                s_TranslVec.X = (double)s_miGetX.Invoke(Vector, null);
                s_TranslVec.Y = (double)s_miGetY.Invoke(Vector, null);
                s_TranslVec.Z = (double)s_miGetZ.Invoke(Vector, null);
                while (Marshal.ReleaseComObject(Vector) > 0) ;
            }

            private static void UpdateAngleAxis()
            {
                object AngleAxis = s_miRotation.Invoke(s_oComSensor, null);
                s_AngleAxis.X = (double)s_miRotGetX.Invoke(AngleAxis, null);
                s_AngleAxis.Y = (double)s_miRotGetY.Invoke(AngleAxis, null);
                s_AngleAxis.Z = (double)s_miRotGetZ.Invoke(AngleAxis, null);
                s_AngleAxis.Angle = (double)s_miRotGetAngle.Invoke(AngleAxis, null);
                while (Marshal.ReleaseComObject(AngleAxis) > 0) ;
            }

				public I3DxVector Translation
            {
                get
                {
                    I3DxVector Return = s_TranslVec;
                    s_TranslVec = new C3DxVector(ref s_TranslVec);
                    return Return;
                }
            }

				public I3DxRotation Rotation
            {
                get
                {
                    I3DxRotation Return = s_AngleAxis;
                    s_AngleAxis = new C3DxRotation(s_AngleAxis);
                    return Return;
                }
            }

				public double Period
            {
                get
                {
                    return m_dPeriod;
                }
            }
            double m_dPeriod;
        }

        /// <summary>
        /// wrapper class for the Com-Co class TDxInput.Keyboard
        /// </summary>
        internal sealed class C3DxKeyboard : I3DxKeyboard
        {
            #region attributes
            static object s_oComKeyboard;
            Type m_tyComKeyboard;
            //static MethodInfo m_miTranslation;
            #endregion

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="r3DxComSensor_p"></param>
            internal C3DxKeyboard(ref object r3DxComKeyboard_p)
            {
                s_oComKeyboard = r3DxComKeyboard_p;
                m_tyComKeyboard = s_oComKeyboard.GetType();
                //MethodInfo[] AllSensorMethods = m_tyComKeyboard.GetMethods();

            }

				public Int32 NumOfKeys
            {
                get
                {
                    Int32 nRet = 0;
                    /*   TODO implementation at Driver
                                   MethodInfo MInfo = m_tyComKeyboard.GetMethod("get_Keys");
                                   if (MInfo != null)
                                   {
                                      nRet = (Int32)MInfo.Invoke(m_oComKeyboard, null);
                                   }
                    */
                    return nRet;
                }
            }

				public Int32 NumOfProgKeys
            {
                get
                {
                    Int32 nRet = 0;
                    /* TODO implementation at Driver ??
                                   MethodInfo MInfo = m_tyComKeyboard.GetMethod("get_ProgrammableKeys");
                                   if (MInfo != null)
                                   {
                                      nRet = (Int32)MInfo.Invoke(m_oComKeyboard, null);
                                   }
                    */
                    return nRet;
                }
            }

				public string GetKeyLabel(Int32 nKey_p)
            {
                string sReturn = "";
                /* TODO implementation at Driver??
                            MethodInfo MInfo = m_tyComKeyboard.GetMethod("GetKeyLabel");
                            if (MInfo != null)
                            {
                               object[] Param ={nKey_p};
                               sReturn = (string)MInfo.Invoke(m_oComKeyboard, Param);
                            }
                */
                return sReturn;
            }
				public string GetKeyName(Int32 nKey_p)
            {
                return nKey_p.ToString(CultureInfo.InvariantCulture);
            }

				public event TDxKeyboardEvent KeyboardEventUP
            {
                add
                {
                    try
                    {
                        //EventInfo[] AllEvents = m_tyComKeyboard.GetEvents();
                        EventInfo TheKeyboardEvent = m_tyComKeyboard.GetEvent("KeyUp");

								if (TheKeyboardEvent != null && TheKeyboardEvent.EventHandlerType != null)
								{
									Delegate TheDelegate = Delegate.CreateDelegate(TheKeyboardEvent.EventHandlerType
									                                               , value.Method);
									TheKeyboardEvent.AddEventHandler(s_oComKeyboard, TheDelegate);
								}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0} Exception caught.", e);
                    }
                }

                remove
                {
                    try
                    {
                        //EventInfo[] AllEvents = m_tyComKeyboard.GetEvents();
                        EventInfo TheKeyboardEvent = m_tyComKeyboard.GetEvent("KeyUp");
								if (TheKeyboardEvent != null && TheKeyboardEvent.EventHandlerType != null)
								{
									Delegate TheDelegate = Delegate.CreateDelegate(TheKeyboardEvent.EventHandlerType
									                                               , value.Method);
									TheKeyboardEvent.RemoveEventHandler(s_oComKeyboard, TheDelegate);
								}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0} Exception caught.", e);
                    }
                }
            }//KeyboardEventUP

            public event TDxKeyboardEvent KeyboardEventDOWN
            {
                add
                {
                    try
                    {
                        //EventInfo[] AllEvents = m_tyComKeyboard.GetEvents();
                        EventInfo TheKeyDownEvent = m_tyComKeyboard.GetEvent("KeyDown");
								if (TheKeyDownEvent != null && TheKeyDownEvent.EventHandlerType != null)
								{
									Delegate TheDelegate = Delegate.CreateDelegate(TheKeyDownEvent.EventHandlerType
									                                               , value.Method);
									TheKeyDownEvent.AddEventHandler(s_oComKeyboard, TheDelegate);
								}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0} Exception caught.", e);
                    }
                }

                remove
                {
                    try
                    {
                        EventInfo TheKeyboardEvent = m_tyComKeyboard.GetEvent("KeyDown");
                        Delegate TheDelegate = Delegate.CreateDelegate(TheKeyboardEvent.EventHandlerType
                                                                       , value.Method);
                        TheKeyboardEvent.RemoveEventHandler(s_oComKeyboard, TheDelegate);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0} Exception caught.", e);
                    }
                }
            }//KeyboardEventDOWN
        }

        #endregion


        /// <summary>
        /// wrapper class for the COM - CSimpleDeviceClass
        /// </summary>
        internal class TDxDeviceWrapper
        {
            private enum RegKind
            {
                RegKind_Default = 0,
                RegKind_Register = 1,
                RegKind_None = 2
            }

            [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
            private static extern void LoadTypeLibEx(String strTypeLibName, RegKind regKind,
                [MarshalAs(UnmanagedType.Interface)] out Object typeLib);

            static readonly object s_Lock = new object();

            private AssemblyBuilder m_Assembly;
            private object m_OSimpleDevice;
            private object m_OSensor;
            private object m_OKeyboard;
            private C3DxSensor m_C3dxSensor;
            private C3DxKeyboard m_C3dxKeyboard;
				private bool m_blInitializationSuccessful;

            private class ConversionEventHandler : ITypeLibImporterNotifySink
            {
					public void ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg)
                {
                    // handle warning event here...
                    Console.WriteLine(eventMsg);
                }

					public Assembly ResolveRef(object typeLib)
                {
                    // resolve reference here and return a correct assembly...
                    Console.WriteLine("resolve refernce");
                    return null;
                }
            }

            internal TDxDeviceWrapper()
            {
                m_blInitializationSuccessful = InitializeTDxInputWrapper();
            }

            private bool InitializeTDxInputWrapper()
            {
                bool bReturn = false;
                Object typeLib = null;
                try
                {
                    //get Com-Server from Registry
                    RegistryKey regKey
                       = Registry.LocalMachine.OpenSubKey("Software\\Classes\\CLSID\\{82C5AB54-C92C-4D52-AAC5-27E25E22604C}\\InprocServer32", false);

						  if (regKey != null)
						  {
							  string strTypeLibName = regKey.GetValue("").ToString();
							  regKey.Close();

							  LoadTypeLibEx(strTypeLibName
												 , RegKind.RegKind_None
												 , out typeLib);
						  }
						  else
						  {
							  // --- No registry set, there must be no 3DConnexion ---
							  return false;
						  }
                }
                catch (Exception)
                {
                }

                if (typeLib == null)
                {
                    Console.WriteLine("LoadTypeLibEx failed.");
                    throw new NotSupportedException("Com- Server not found.");
                }

                TypeLibConverter converter = new TypeLibConverter();
                ConversionEventHandler eventHandler = new ConversionEventHandler();
                m_Assembly = converter.ConvertTypeLibToAssembly(typeLib, "Import3DxInputAssembly.dll", 0, eventHandler, null, null, null, null);

                //Type[] ExpTypes = m_Assembly.GetTypes();
                try
                {
                    m_OSimpleDevice = m_Assembly.CreateInstance("Import3DxInputAssembly.DeviceClass");
                }
                catch
                {
                    //int nTest;
                }

                if (m_OSimpleDevice != null)
                {
                    Type TheType = m_OSimpleDevice.GetType();
                    //MethodInfo[] TheMethods = TheType.GetMethods();
                    MethodInfo method = TheType.GetMethod("get_Sensor");
                    m_OSensor = method.Invoke(m_OSimpleDevice, null);  // kein Parameter

                    method = TheType.GetMethod("get_Keyboard");
                    m_OKeyboard = method.Invoke(m_OSimpleDevice, null);

                    TheType = m_OKeyboard.GetType();
                    //TheMethods = TheType.GetMethods();
                    bReturn = true;
                }

                return bReturn;
            }//constructor


            internal void Connect()
            {
                if (m_OSimpleDevice == null)
                {
						 m_blInitializationSuccessful = InitializeTDxInputWrapper();
                }

                if (m_OSimpleDevice != null)
                {
                    Type TheType = m_OSimpleDevice.GetType();
                    MethodInfo method = TheType.GetMethod("Connect");  // kein Parameter
                    //method.Invoke(m_OSimpleDevice, BindingFlags.InvokeMethod, new TDxBinder(), new object[0], CultureInfo.CurrentCulture );
                    method.Invoke(m_OSimpleDevice, null);  // kein Parameter
                }
            }

            internal void Disconnect()
            {
                if (m_OSimpleDevice != null)
                {
                    Type TheType = m_OSimpleDevice.GetType();
                    MethodInfo method = TheType.GetMethod("Disconnect");  // kein Parameter
                    method.Invoke(m_OSimpleDevice, null);  // kein Parameter
                }
            }

				internal bool InitializationSuccessful
				{
					get { return m_blInitializationSuccessful; }
				}


            /// <summary>
            /// Property "Sensor"; implemented as Singelton
            /// </summary>
            internal I3DxSensor Sensor
            {
                get
                {
                    lock (s_Lock)
                    {
                        if ((m_C3dxSensor == null)
                            && (m_OSimpleDevice != null))
                        {
                            Type TheType = m_OSimpleDevice.GetType();
                            MethodInfo method = TheType.GetMethod("get_Sensor");
                            m_OSensor = method.Invoke(m_OSimpleDevice, null);
                            m_C3dxSensor = new C3DxSensor(ref m_OSensor);
                        }
                        return m_C3dxSensor;
                    }
                }
            }

            internal I3DxKeyboard Keyboard
            {
                get
                {
                    lock (s_Lock)
                    {
                        if ((m_C3dxKeyboard == null)
                            && (m_OSimpleDevice != null))
                        {
                            Type TheType = m_OSimpleDevice.GetType();
                            MethodInfo method = TheType.GetMethod("get_Keyboard");
                            m_OKeyboard = method.Invoke(m_OSimpleDevice, null);
                            m_C3dxKeyboard = new C3DxKeyboard(ref m_OKeyboard);
                        }
                        return m_C3dxKeyboard;
                    }
                }
            }

            internal void Release()
            {
                Marshal.ReleaseComObject(m_OKeyboard);
                m_C3dxKeyboard = null;
                Marshal.ReleaseComObject(m_OSensor);
                m_C3dxSensor = null;
                Marshal.ReleaseComObject(m_OSimpleDevice);
                m_OSimpleDevice = null;
            }

        }//class TDxDeviceWrapper 


    }//end namespace



    #endregion //TDconnexion SDK Com-Co wrapper


}//end namespace "ThreeDconnexion.Plugin"
///////////////////////////////////////////////////////////////////////////////
//
// END
//
///////////////////////////////////////////////////////////////////////////////

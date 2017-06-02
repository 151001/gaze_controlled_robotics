using GOTSDK;
using GOTSDK.Master;
using GOTSDK.Master.Master2XTypes;
using GOTSDK.Position;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Robot
{
    class GameOnTrack
    {
        public static string calibPath = AppDomain.CurrentDomain.BaseDirectory + "Calibration.xml";

        static public bool LoadCalib()
        {
            if(File.Exists(calibPath))
            {
                var doc = XDocument.Load(calibPath);
                var loadedScenarios = Scenario3DPersistence.Load(doc);

                foreach (var scenario in loadedScenarios)
                    MainWindow.scenarios.Add(scenario);
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static void Close()
        {
            // Close the Master USB connection
            if (MainWindow.master != null)
                MainWindow.master.Close();
        }

        internal static bool Connect()
        {
            // Try to connect. This will return false immediately if it fails to find the proper USB port.
            // Otherwise, it will begin connecting (async) and the "Master2X.OnMasterStatusChanged" event will be fired soon. 
            return MainWindow.master.BeginConnect();
        }

        static public void Init()
        {
            MainWindow.master = new Master2X(SynchronizationContext.Current);
            MainWindow.master.OnMasterStatusChanged += OnMasterStatusChanged;
            MainWindow.master.OnNewReceiverConnected += OnNewReceiverConnected;
            MainWindow.master.OnNewTransmitterConnected += OnNewTransmitterConnected;
            MainWindow.master.OnMasterInfoReceived += OnMasterInfoReceived;
            MainWindow.master.OnMeasurementReceived += OnMeasurementReceived;
        }

        // Called when Master info has been received
        private static void OnMasterInfoReceived(IMaster master, string swVersion, string serialNumber)
        {
            MainWindow.masterVersion = string.Format("{0}, Serial: {1}", swVersion, serialNumber);
        }

        // This is called whenever the master connection status has been changed.
        private static void OnMasterStatusChanged(IMaster master, MasterStatus newStatus)
        {
            MainWindow.masterConnectionStatus = newStatus.ToString();

            if (newStatus == MasterStatus.Connected)
            {
                // Display the name of the virtual COM port
                MainWindow.masterConnectionStatus += string.Format(" ({0})", master.CurrentPortName);

                // Request firmware version and serial.
                master.RequestMasterInfo();

                // The current air temperature
                int temperatureDegrees = 22;
                ushort speedOfSoundInMeters = (ushort)(Master2X.GetSpeedOfSoundInMeters(temperatureDegrees) + 0.5);
                ((Master2X)master).Setup(110, speedOfSoundInMeters, 16, 0, TPCLINK_ULTRASONIC_LEVEL.LEVEL_4);

                // The very last thing: Request all currently connected units. They will show up in the OnNewTransmitter/OnNewReceiver connected events.
                master.RequestUnits();
            }
        }

        // Called when a new transmitter has been connected to Master. This includes the ones used in the calibrator triangle.
        private static void OnNewTransmitterConnected(Transmitter transmitter)
        {
            if (!MainWindow.connectedTransmitters.Any(t => t.GOTAddress == transmitter.GOTAddress))
            {
                MainWindow.connectedTransmitters.Add(transmitter);
                MainWindow.master.SetTransmitterState(transmitter.GOTAddress, GetTransmitterState(transmitter.GOTAddress), Transmitter.UltraSonicLevel.High);
            }
        }

        private static Transmitter.TransmitterState GetTransmitterState(GOTAddress gOTAddress)
        {
            if (CalibratorTriangle.IsCalibratorTriangleAddress(gOTAddress))
                return Transmitter.TransmitterState.Deactivated;

            return Transmitter.TransmitterState.ActiveHigh;
        }

        // Called when a new receiver has been connected to Master
        private static void OnNewReceiverConnected(Receiver receiver)
        {
            if (!MainWindow.connectedReceivers.Any(r => r.GOTAddress == receiver.GOTAddress))
                MainWindow.connectedReceivers.Add(receiver);
        }

        // Called when a measurement is received from the master
        private static void OnMeasurementReceived(Measurement measurement)
        {
            if (MainWindow.scenarios.Count == 0)
            {
                Log.SetLog("GameOnTrack: No scenario found. ");
            }
            else if (measurement.RSSI == 0)
            {
                Log.SetLog("GameOnTrack: Transmitter radio lost. ");
            }
            else if (measurement.RxMeasurements.Count(dist => dist.Distance > 0) < 3)
            {
                Log.SetLog("GameOnTrack: The number of receivers is less than 3. ");
            }
            else
            {
                CalculatedPosition pos;
                if (PositionCalculator.TryCalculatePosition(measurement, MainWindow.scenarios.ToArray(), out pos))
                {
                    if( pos.TxAddress.ToString() == Wheelchair.sensor1.address.ToString() )
                    {
                        Wheelchair.sensor1.x = pos.Position.X / 10;
                        Wheelchair.sensor1.y = pos.Position.Y / 10;
                    }
                    else if(pos.TxAddress.ToString() == Wheelchair.sensor2.address.ToString())
                    {
                        Wheelchair.sensor2.x = pos.Position.X / 10;
                        Wheelchair.sensor2.y = pos.Position.Y / 10;
                    }
                }
                else
                {
                    Log.SetLog("GameOnTrack: Can not calculate the position. ");
                }
            }

            if (Wheelchair.state != 3 && Wheelchair.state != 0)
            {
                Wheelchair.Move();
            }

        }       
    }
}

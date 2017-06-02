using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Robot
{
    class Wheelchair
    {
        #region Variables

        // Wheelchair configuration, all in CM
        static public double maxSpeed = 167;        // Max speed of the wheelchair
        static public double acceleration = 0.05;   // acceleration for each wheel
        static public double speedLimit = 100;      // speed limitation for each wheel when running
        static public double wheelBase = 50;
        static public int GOTAddress1 = 41269;
        static public int GOTAdderss2 = 41272;
        static public double speed_l = 0;
        static public double speed_r = 0;

        static public int l_obstacle = 0;
        static public int ml_obstacle = 0;
        static public int m_obstacle = 0;
        static public int mr_obstacle = 0;
        static public int r_obstacle = 0;

        static public double x = 0;
        static public double y = 0;
        static public double theta = 0;     // In degree
        static public GOTData sensor1 = new GOTData();
        static public GOTData sensor2 = new GOTData();

        // Move control
        static public int state = 0;    // 0:stop, 1:turn2point, 2: fwd2point, 3: turn2dir
        static public double fwdThreshold = 5;
        static public double angleThreshold = 5;
        static public int currentPoint = 0;
        static public int currentDir = 0;
        static public int targetPoint = 0; // index of the waypoint
        static public int targetDir = 0; // 1-4 represent 0,90,180,-90 degree
        static public string cmdDone = "DTU1"; // tell the user the command is finished
        static public string obstacleDetected = "DTU1"; // tell the user the command is finished
        // Timer
        static public DispatcherTimer delayTimer = new DispatcherTimer();
 
        #endregion

        #region Initialise

        internal static void Init()
        {
            // init the address of the sensor on the wheelchair
            sensor1.address = GOTAddress1;
            sensor1.x = 0;
            sensor1.y = 10;
            sensor2.address = GOTAdderss2;
            sensor2.x = 0;
            sensor2.y = -10;
        }

        #endregion

        #region Wheelchair control

        internal static void Move()
        {
            int pointIndex = 0;
            if (Map.pointsPath.Count > 0)
            {
                pointIndex = int.Parse(Map.pointsPath[0].ToString());
            }

            switch (state)
            {
                
                case 1:     // turn to the point
                    TurnToPoint(Map.waypoints[pointIndex].x, Map.waypoints[pointIndex].y);
                    break;
                case 2:     // head to the point
                    MoveToPoint(Map.waypoints[pointIndex].x, Map.waypoints[pointIndex].y);
                    break;
                case 3:     // turn to direction
                    TurnToDir(targetDir);
                    break;
                default:
                    Stop();
                    break;
            }
        }

        internal static void Reactive(string v)
        {
            if (v.Substring(0, 3) == "DTU")
            {
                l_obstacle = int.Parse(v.Substring(3, 1));
                ml_obstacle = int.Parse(v.Substring(4, 1));
                m_obstacle = int.Parse(v.Substring(5, 1));
                mr_obstacle = int.Parse(v.Substring(6, 1));
                r_obstacle = int.Parse(v.Substring(7, 1));

                if (m_obstacle == 1)
                {
                    Stop();
                    MainWindow.SendCmd(obstacleDetected);
                }
            }
        }

        // Update wheelchair position according to sensor data
        public static void UpdatePosition()
        {
            x = (sensor1.x + sensor2.x) / 2;
            y = (sensor1.y + sensor2.y) / 2;
            var sensorTh = Math.Atan2(sensor2.y - sensor1.y, sensor2.x - sensor1.x); // -pi < result <= pi
            var robTh = sensorTh + Math.PI / 2;
            robTh = FitInRadius(robTh);
            theta = RadiusToDegree(robTh); 
        }

        // Stop the wheelchair
        internal static void Stop()
        {
            if (MainWindow.HWPort.IsOpen)
            {
                try
                {
                    MainWindow.HWDataSend("DTU100000000");                 
                }
                catch (Exception ex)
                {
                    Log.SetLog("Serial port: Communication failed: " + ex.Message.ToString());
                }
            }
            UpdatePosition();
            speed_l = 0;
            speed_r = 0;
        }

        internal static void TurnToPoint(double pointX, double pointY)
        {
            double targetTheta = RadiusToDegree(Math.Atan2(pointY - y, pointX - x));
            TurnTo(targetTheta);
        }

        internal static void MoveToPoint(double pointX, double pointY)
        {
            double deltaDist = Distance(pointX, pointY);
            double speed = (Math.Abs(speed_l) + Math.Abs(speed_r)) / 2;
            while (deltaDist > fwdThreshold )
            {               
                // if deceleration
                if (deltaDist < ( (speedLimit*speedLimit)/(2000*acceleration)) )   // need to be adjusted
                {
                    speed = Math.Sqrt(2 * acceleration * deltaDist);
                }
                // else, max speed or acceleration
                else
                {
                    speed = Math.Min(speed + acceleration, speedLimit);
                }

                double targetTheta = RadiusToDegree(Math.Atan2(pointY - y, pointX - x));
                double deltaTheta = FitInDegree(targetTheta - theta);
                speed_l = Math.Min(speed - deltaTheta, maxSpeed);   // need to be adjusted
                speed_r = Math.Min(speed + deltaTheta, maxSpeed);   // need to be adjusted

                try
                {
                    MainWindow.HWDataSend("DTU0" + wheelCmd(speed_l) + wheelCmd(speed_r));
                }
                catch (Exception ex)
                {
                    Log.SetLog("Serial port: Communication failed: " + ex.Message.ToString());
                }              
                Delay(1);
                deltaDist = Math.Sqrt((pointX - x) * (pointX - x) + (pointY - y) * (pointX - y));
            }
            Stop();
        }

        internal static void TurnToDir(int targetDir)
        {
            switch (targetDir)
            {
                case 1:
                    TurnTo(0.0);
                    break;
                case 2:
                    TurnTo(90.0);
                    break;
                case 3:
                    TurnTo(180.0);
                    break;
                case 4:
                    TurnTo(-90.0);
                    break;
                default:
                    break;
            }
        }

        internal static void TurnTo(double targetD)
        {
            targetD = FitInDegree(targetD);
            while (Math.Abs(targetD-theta) > angleThreshold)
            {
                Log.SetLog("Turning");
                // Decide which direction to turn, counterclockwise: 1, clockwise: -1
                int turnDir = 1;
                double deltaTheta = FitInDegree(targetD - theta);
                if (deltaTheta >= 0)
                {
                    turnDir = 1;
                }
                else
                {
                    turnDir = -1;
                }

                // Decide the turning speed
                double speed = (Math.Abs(speed_l)+Math.Abs(speed_r)) / 2;
                // if deceleration
                if (turnDir * deltaTheta < ((speedLimit * speedLimit) / (acceleration * wheelBase)))
                {
                    speed = Math.Sqrt(2*acceleration*wheelBase*turnDir*deltaTheta);
                }
                // else, max speed or acceleration
                else
                {
                    speed = Math.Min(speed+acceleration,speedLimit);
                }
                Log.SetLog("" + turnDir.ToString() + "," + speed.ToString());
                Turn(turnDir,speed);
                Delay(1);
            }
            Stop();
        }

        internal static void Turn(int turnDir, double speed)
        {
            speed_l = -turnDir * speed;
            speed_r = turnDir * speed;
            try
            {
                MainWindow.HWDataSend("DTU0" + wheelCmd(speed_l) + wheelCmd(speed_r));
            }
            catch (Exception ex)
            {
                Log.SetLog("Serial port: Communication failed: " + ex.Message.ToString());
            }         
        }

        internal static string wheelCmd(double s)
        {
            string cmd = "";
            string dir = "";
            string value = "";

            if (s > 0)
            {
                dir = "1";
            }
            else if (s < 0)
            {
                dir = "2";
            }
            else
            {
                dir = "0";
            }

            value = Math.Ceiling(Math.Abs(s * 255.0 / maxSpeed)).ToString("000");

            cmd = dir + value;
            return cmd;
        }

        #endregion

        #region Delay

        public static void Delay(int mm)
        {
            delayTimer.Interval = new TimeSpan(0, 0, 0, 0, mm);
            delayTimer.Tick += new EventHandler(OnTimedEvent);
            delayTimer.Start();
        }

        public static void OnTimedEvent(object sender, EventArgs e)
        {
            delayTimer.Stop();
        }

        #endregion

        #region Math function

        private static double RadiusToDegree(double r)
        {
            var d = (r / Math.PI) * 180;
            return d;
        }

        private static double DegreeToRadius(double d)
        {
            var r = (d / 180) * Math.PI;
            return r;
        }

        private static double FitInRadius(double r)
        {
            while (r < -Math.PI)
            {
                r = r + 2 * Math.PI;
            }
            while (r > Math.PI)
            {
                r = r - 2 * Math.PI;
            }
            return r;
        }

        private static double FitInDegree(double d)
        {
            while (d < -180)
            {
                d = d + 360;
            }
            while (d > 180)
            {
                d = d - 360;
            }
            return d;
        }

        // Calculate the distance from robot to the point
        internal static double Distance(double px, double py)
        {
            double dis;
            dis = Math.Sqrt((px - x) * (px - x) + (py - y) * (py - y));
            return dis;
        }

        #endregion

    }
}

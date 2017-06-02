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
        static public double maxSpeed = 255;       // Max speed of the wheelchair
        static public double speedLimit = 50;      // speed limitation for each wheel when running
        static public int GOTAddress1 = 41269;
        static public int GOTAdderss2 = 41272;
        static public double speed_l = 0;
        static public double speed_r = 0;
        static public double movingSpeed = 30;

        static public double x = 0;
        static public double y = 0;
        static public double theta = 0;             // In degree
        static public GOTData sensor1 = new GOTData();
        static public GOTData sensor2 = new GOTData();

        // Move control
        static public int state = 0;                // 0:stop, 1:turn2point, 2: fwd2point, 3: direct control
        static public double fwdThreshold = 30;     // centimeter
        static public double angleThreshold = 30;    // degree
        static public double controlPara = 0.3;
        static public int currentPoint = 0;
        static public int targetPoint = 0;          // index of the waypoint

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
                    Log.SetLog("Turning to point " + pointIndex.ToString());
                    TurnToPoint(Map.waypoints[pointIndex].x, Map.waypoints[pointIndex].y);
                    break;
                case 2:     // head to the point
                    Log.SetLog("Heading to point " + pointIndex.ToString());
                    MoveToPoint(Map.waypoints[pointIndex].x, Map.waypoints[pointIndex].y);
                    break;
                default:
                    Stop();
                    break;
            }
        }

        // Stop the wheelchair
        internal static void Stop()
        {      
            if (MainWindow.HWPort.IsOpen)
            {
                try
                {
                    MainWindow.HWDataSend("DK0110001000");
                }
                catch (Exception ex)
                {
                    Log.SetLog("Serial port: Communication failed: " + ex.Message.ToString());
                }
            }
            UpdatePosition();
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
            if (deltaDist > fwdThreshold )
            {
                speed = movingSpeed;
                double targetTheta = RadiusToDegree(Math.Atan2(pointY - y, pointX - x));
                double deltaTheta = FitInDegree(targetTheta - theta);
                speed_l = Math.Min(speed - controlPara*deltaTheta, speedLimit);   // need to be adjusted
                speed_r = Math.Min(speed + controlPara*deltaTheta, speedLimit);   // need to be adjusted

                try
                {
                    MainWindow.HWDataSend("DK10" + wheelCmd(speed_l) + wheelCmd(speed_r));
                }
                catch (Exception ex)
                {
                    Log.SetLog("Serial port: Communication failed: " + ex.Message.ToString());
                }              
            }
            else
            {
                currentPoint = Int32.Parse(Map.pointsPath[0].ToString());
                Map.pointsPath.Remove(Map.pointsPath[0]);
                Log.SetLog(currentPoint.ToString() + " reached");                    
                if (Map.pointsPath.Count>0)
                {
                    state = 1;
                    MainWindow.fbCmd();
                }
                else
                {
                    state = 0;
                    MainWindow.fbCmd();
                }
                Stop();
            }
        }

        internal static void TurnTo(double targetD)
        {
            targetD = FitInDegree(targetD);
            double deltaTheta = FitInDegree(targetD - theta);

            if (Math.Abs(deltaTheta) > angleThreshold)
            {
                int turnDir = 1;              
                Turn(turnDir, movingSpeed);
            }
            else
            {
                Log.SetLog("Faced to " + Map.pointsPath[0].ToString() );
                state = 2;              
                Stop();
                MainWindow.fbCmd();
            }
        }

        internal static void Turn(int turnDir, double speed)
        {
            speed_l = -turnDir * speed;
            speed_r = turnDir * speed;
            try
            {
                MainWindow.HWDataSend("DK10" + wheelCmd(speed_l) + wheelCmd(speed_r));
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
            double v = 0.0;
            string value = "";

            if (s > 0)
            {
                dir = "2";
            }
            else if (s < 0)
            {
                dir = "0";
            }
            else
            {
                dir = "1";
            }

            v = Math.Ceiling(Math.Abs(s * 255.0 / maxSpeed));

            if (v > 0 && v < 10)
            {
                v = 10.0;
            }

            value = v.ToString("000");
            cmd = dir + value;
            return cmd;
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

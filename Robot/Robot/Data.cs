using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Media;

namespace Robot
{
    public class Data
    {
        public static string dataPath = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToFileTime().ToString()+".csv";

        public static void Init()
        {
            // Init log file
            if (File.Exists(dataPath))
            {
                File.Delete(dataPath);
            }
            string titles = string.Format("Time,l-dir,l-spd,r-dir,r-spd,Left, Left-Front,Front,Right-Front,Right,X,Y,Theta\n");
            File.WriteAllText(dataPath,titles);
        }

        public static void SetData(string data)
        {
            try
            {
                File.AppendAllText(dataPath, data + "\r\n");
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
            }
        }
    }
}

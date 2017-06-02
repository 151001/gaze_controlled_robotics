using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User
{
    class Data
    {
        public static string dataPath = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToFileTime().ToString() + ".csv";

        public static void Init()
        {
            // Init log file
            if (File.Exists(dataPath))
            {
                File.Delete(dataPath);
            }
            string titles = string.Format("Time,GazeX,GazeY,GazeState,selectObj,triggerObj\n");
            File.WriteAllText(dataPath, titles);
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

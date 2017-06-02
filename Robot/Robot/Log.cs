﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Media;

namespace Robot
{
    public class Log
    {
        public static string logPath = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToFileTime().ToString() + ".log";

        public static void Init()
        {
            // Init log file
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
        }

        public static void SetLog(string log)
        {
            try
            {
                File.AppendAllText(logPath, "[" + DateTime.Now.ToString() + "]" + log + "\r\n");
            }
            catch (Exception)
            {
            }
        }
    }
}

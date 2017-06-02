using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot
{
    class Serial
    {
        static public void Init(SerialPort port, string portName, int baudRate, int dataBits)
        {
            // Init serial port           
            port.PortName = portName;
            port.BaudRate = baudRate;          
            port.Handshake = Handshake.None;
            port.Parity = Parity.None;
            port.DataBits = dataBits;
            port.StopBits = StopBits.One;
            port.ReadTimeout = 200;
            port.WriteTimeout = 50;
            port.Open();
            port.DataReceived += new SerialDataReceivedEventHandler (MainWindow.HWDataReceived);
        }

        static public void Close(SerialPort port)
        {
            port.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ANYCHATAPI;
using System.IO;
using System.IO.Ports;
using System.Windows.Interop;

namespace Robot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables
        
        // Server information
        public string address = "demo.anychat.cn";
        public int port = 8906;
        public int roomNum = 151001;

        // User information
        public string userName = "s151001-user";
        public string userPassword = "s151001";
        private int myUserID = -1;

        // Local camera configuration
        public int localCamLeft = 1300;
        public int localCamRight = 1880;
        public int localCamTop = 60;
        public int localCamBottom = 370;
        public int localCamIndex = 0;

        // Remote camera configuration
        public int remoteCamLeft = 70;
        public int remoteCamRight = 1200;
        public int remoteCamTop = 100;
        public int remoteCamBottom = 920;
        public int remoteCamIndex = 0;

        // Serial port configuration
        SerialPort HWPort = new SerialPort();
        public string portName = "COM7";
        public int baudRate = 9600;
        public int dataBits = 8;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            // Init serial port           
            HWPort.PortName = portName;
            HWPort.BaudRate = baudRate;
            HWPort.Handshake = System.IO.Ports.Handshake.None;
            HWPort.Parity = Parity.None;
            HWPort.DataBits = dataBits;
            HWPort.StopBits = StopBits.Two;
            HWPort.ReadTimeout = 200;
            HWPort.WriteTimeout = 50;
            HWPort.Open();
            HWPort.DataReceived += new SerialDataReceivedEventHandler(HWDataReceived);
        }

        #endregion

        #region Window Events

        // Load window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                IntPtr windowHdl = new WindowInteropHelper(Application.Current.MainWindow).Handle;
               
                // Init log file
                if (File.Exists(Log.logPath))
                {
                    File.Delete(Log.logPath);
                }

                // Init AnychatSDK
                string path = AppDomain.CurrentDomain.BaseDirectory;
                SystemSetting.Text_OnReceive = new TextReceivedHandler(receivedCmd);//文本回调涵数
                AnyChatCoreSDK.SetSDKOption(AnyChatCoreSDK.BRAC_SO_CORESDK_PATH, path, path.Length);
                SystemSetting.Init(windowHdl);

                // Maximize the main window and bring to front
                this.WindowState = WindowState.Maximized;
                this.Topmost = true;
                this.Activate();
            }
            catch (Exception ex)
            {
                Log.SetLog("Window_Loaded: " + ex.Message.ToString());
            }
            
        }

        // Close window
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                HWPort.Close();
                AnyChatCoreSDK.LeaveRoom(roomNum);
                AnyChatCoreSDK.Logout();
                AnyChatCoreSDK.Release();
            }
            catch (Exception ex)
            {
                Log.SetLog("Window_closed: " + ex.Message.ToString());
            }
        }      

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            int ret = AnyChatCoreSDK.Connect(address, port);
            ret = AnyChatCoreSDK.Login(userName, userPassword, 0);

            IntPtr windowHdl = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            HwndSource hwndSource = HwndSource.FromHwnd(windowHdl);
            hwndSource.AddHook(new HwndSourceHook(WndProc));    
        }

        #endregion

        #region WndProc

        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case AnyChatCoreSDK.WM_GV_CONNECT:
                    /// Connect
                    int succeed = wParam.ToInt32();
                    if (succeed == 1)
                    {
                        Log.SetLog("Connected to server");
                    }
                    else
                    {
                        Log.SetLog("Connected to server failed");
                    }
                    break;
                case AnyChatCoreSDK.WM_GV_LOGINSYSTEM:
                    /// Login system
                    if (lParam.ToInt32() == 0)
                    {
                        Log.SetLog("Success login");
                        myUserID = wParam.ToInt32();
                        AnyChatCoreSDK.EnterRoom(roomNum, "", 0);
                    }
                    else
                    {
                        Log.SetLog("Login failed：Error=" + lParam.ToString());
                    }
                    break;
                case AnyChatCoreSDK.WM_GV_ENTERROOM:
                    // Enter room
                    int lparam = lParam.ToInt32();
                    if (lparam == 0)
                    {
                        int roomid = wParam.ToInt32();
                        Log.SetLog("Success enter room, the room number is：" + roomid.ToString());
                        roomNum = roomid;
                        // Open local video camera on the interface
                        Video.openLocalVideo(Video.getLocalVideoDeivceName(), hwnd, localCamLeft,  localCamTop, localCamRight, localCamBottom, localCamIndex);
                    }
                    else
                    {
                        Log.SetLog("Enter room failed，Error：" + lparam.ToString());
                    }
                    break;
                case AnyChatCoreSDK.WM_GV_ONLINEUSER:
                    /// Get users list
                    int cnt = 0;    // Number of online users
                    AnyChatCoreSDK.GetOnlineUser(null, ref cnt);    // Get the number of online users
                    int[] usersID = new int[cnt];   // Online users ID list
                    AnyChatCoreSDK.GetOnlineUser(usersID, ref cnt); // Get the ID list of online users
                    for (int idx = 0; idx < cnt; idx++)
                    {
                        if (usersID[idx] != myUserID)
                        {
                            Video.controlRemoteVideo(usersID[idx], true, hwnd, remoteCamLeft, remoteCamTop, remoteCamRight, remoteCamBottom, remoteCamIndex);
                            Log.SetLog("Robot video get");
                        }
                    }              
                    break;
                case AnyChatCoreSDK.WM_GV_USERATROOM:
                    /// New user enter room
                    int userID = wParam.ToInt32();
                    int boEntered = lParam.ToInt32();
                    if (boEntered == 0)
                    {
                        if (userID != myUserID)
                        {
                            Video.controlRemoteVideo(userID, false, hwnd, remoteCamLeft, remoteCamTop, remoteCamRight, remoteCamBottom, remoteCamIndex);
                            Log.SetLog("Robot leave room");
                        }
                    }
                    else
                    {
                        if (userID != myUserID)
                        {
                            Video.controlRemoteVideo(userID, true, hwnd, remoteCamLeft, remoteCamTop, remoteCamRight, remoteCamBottom, remoteCamIndex);
                            Log.SetLog("Robot Enter room");
                        }
                    }
                    break;
                case AnyChatCoreSDK.WM_GV_CAMERASTATE:
                    // State of the camera
                    break;
                case AnyChatCoreSDK.WM_GV_LINKCLOSE:
                    // Lose connection
                    AnyChatCoreSDK.LeaveRoom(-1);
                    int wpara = wParam.ToInt32();
                    int lpara = lParam.ToInt32();
                    Log.SetLog("Lose connection，ErrorCode = " + wpara.ToString());
                    break;
            }
            return IntPtr.Zero;
        }

        #endregion

        #region Command handle

        // receive command from user side
        private void receivedCmd(int fromUID, int toUID, string Text, bool isserect)
        {
            
        }

        // receive command from serial port
        private void HWDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //
        }

        // send command through serial port
        private void sendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (HWPort.IsOpen)
            {
                try
                {
                    // Send the binary data out the port
                    HWPort.WriteLine(sendTxt.Text);
                    cmdTxt.Content = sendTxt.Text;
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                cmdTxt.Content = "Serial port is not open!";
            }
        }

        #endregion
    }
}

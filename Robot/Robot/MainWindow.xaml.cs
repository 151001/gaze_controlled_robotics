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
using GOTSDK;
using GOTSDK.Master;
using GOTSDK.Position;
using HelixToolkit.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Collections;

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
        public string userName = "s151001-robot";
        public string userPassword = "s151001";
        private int myUserID = -1;

        static public PresentationSource source;

        // Local camera configuration
        public int localCamIndex = 0;
        static public double localCamLeftP = 0.006;
        static public double localCamRightP = 0.3285;
        static public double localCamTopP = 0.732;
        static public double localCamBottomP = 0.955;

        public int localCamLeft = 0;
        public int localCamRight = 0;
        public int localCamTop = 0;
        public int localCamBottom = 0;

        // Remote camera configuration
        public int remoteCamIndex = 0;
        static public double remoteCamLeftP = 0.006;
        static public double remoteCamRightP = 0.992;
        static public double remoteCamTopP = 0.01;
        static public double remoteCamBottomP = 0.72;

        public int remoteCamLeft = 0;
        public int remoteCamRight = 0;
        public int remoteCamTop = 0;
        public int remoteCamBottom = 0;

        // Serial port configuration, port name could be different on different device
        static public SerialPort HWPort = new SerialPort();
        public string portName = "COM6";
        public int baudRate = 9600;
        public int dataBits = 8;

        // GameOnTrack configuration
        static public ObservableCollection<Transmitter> connectedTransmitters = new ObservableCollection<Transmitter>();
        static public ObservableCollection<Receiver> connectedReceivers = new ObservableCollection<Receiver>();
        static public ObservableCollection<Scenario3D> scenarios = new ObservableCollection<Scenario3D>();
        static public Master2X master;
        static public string masterConnectionStatus = "Offline";
        static public string masterVersion = "Unknown";           

        // Timer
        DispatcherTimer timer = new DispatcherTimer();

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            Log.Init();
            Data.Init();
        }

        #endregion

        #region Window Events

        // Load window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Open serial port
            try
            {
                Serial.Init(HWPort, portName, baudRate, dataBits);
                ShowInfo("Success open serial port. ");
                serialTxt.Text = "Open";
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
                ShowInfo("Error: " + ex.Message.ToString());
            }

            // Init web video
            try
            {
                IntPtr windowHdl = new WindowInteropHelper(Application.Current.MainWindow).Handle;

                SystemSetting.Text_OnReceive = new TextReceivedHandler(ReceivedCmd); // Text callback
                WebVideo.Init(windowHdl);
                ShowInfo("Video server has been initialised. ");

                int ret = AnyChatCoreSDK.Connect(address, port);
                ret = AnyChatCoreSDK.Login(userName, userPassword, 0);
                HwndSource hwndSource = HwndSource.FromHwnd(windowHdl);
                hwndSource.AddHook(new HwndSourceHook(WndProc));
                ShowInfo("Connecting to video server... ");
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
                ShowInfo("Error: " + ex.Message.ToString());
            }

            // Fit camera stream to window size
            try
            {
                source = PresentationSource.FromVisual(this);
                if (source != null)
                {
                    localCamLeft = Convert.ToInt32(localCamLeftP * SystemParameters.WorkArea.Width * source.CompositionTarget.TransformToDevice.M11);
                    localCamRight = Convert.ToInt32(localCamRightP * SystemParameters.WorkArea.Width * source.CompositionTarget.TransformToDevice.M11);
                    localCamTop = Convert.ToInt32(localCamTopP * SystemParameters.WorkArea.Height * source.CompositionTarget.TransformToDevice.M22);
                    localCamBottom = Convert.ToInt32(localCamBottomP * SystemParameters.WorkArea.Height * source.CompositionTarget.TransformToDevice.M22);

                    remoteCamLeft = Convert.ToInt32(remoteCamLeftP * SystemParameters.WorkArea.Width * source.CompositionTarget.TransformToDevice.M11);
                    remoteCamRight = Convert.ToInt32(remoteCamRightP * SystemParameters.WorkArea.Width * source.CompositionTarget.TransformToDevice.M11);
                    remoteCamTop = Convert.ToInt32(remoteCamTopP * SystemParameters.WorkArea.Height * source.CompositionTarget.TransformToDevice.M22);
                    remoteCamBottom = Convert.ToInt32(remoteCamBottomP * SystemParameters.WorkArea.Height * source.CompositionTarget.TransformToDevice.M22);
                }
                else
                {
                    localCamLeft = Convert.ToInt32(localCamLeftP * SystemParameters.WorkArea.Width);
                    localCamRight = Convert.ToInt32(localCamRightP * SystemParameters.WorkArea.Width);
                    localCamTop = Convert.ToInt32(localCamTopP * SystemParameters.WorkArea.Height);
                    localCamBottom = Convert.ToInt32(localCamBottomP * SystemParameters.WorkArea.Height);

                    remoteCamLeft = Convert.ToInt32(remoteCamLeftP * SystemParameters.WorkArea.Width);
                    remoteCamRight = Convert.ToInt32(remoteCamRightP * SystemParameters.WorkArea.Width);
                    remoteCamTop = Convert.ToInt32(remoteCamTopP * SystemParameters.WorkArea.Height);
                    remoteCamBottom = Convert.ToInt32(remoteCamBottomP * SystemParameters.WorkArea.Height);
                }
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
                ShowInfo("Error: " + ex.Message.ToString());
            }

            // Init GameOnTrack
            try
            {
                if (GameOnTrack.LoadCalib())
                {
                    ShowInfo("GameOnTrack calibration file found. ");
                    GameOnTrack.Init();
                    if (GameOnTrack.Connect())
                    {
                        ShowInfo("GameOnTrack master detected. ");
                        GOTTxt.Text = "Connected";
                    }
                    else
                    {
                        Log.SetLog("Error: Master can not be detected. ");
                        ShowInfo("GameOnTrack master can not be detected. ");
                    }
                }
                else
                {
                    Log.SetLog("Error: Can not find GameOnTrack Calibration file. ");
                    ShowInfo("Can not find GameOnTrack Calibration file. ");
                }
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
                ShowInfo("Error: " + ex.Message.ToString());
            }

            // Init wheelchair, configuration need to be changed
            try
            {
                Wheelchair.Init();
                ShowInfo("Wheelchair has been initialised. ");
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
                ShowInfo("Error: " + ex.Message.ToString());
            }

            // Init map, configuration need to be changed
            try
            {
                Map.Init();
                ShowInfo("Map has been initialised. ");
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
                ShowInfo("Error: " + ex.Message.ToString());
            }

            // Start the timer, every 1 ms.
            try
            {
                timer.Tick += new EventHandler(Timer_tick);
                timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
                timer.Start();
                ShowInfo("Timer starts to update robot information. ");
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
                ShowInfo("Error: " + ex.Message.ToString());
            }

            // Maximize the main window and bring to front
            WindowState = WindowState.Maximized;
            this.Activate();          
        }

        // Close window
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                Wheelchair.Stop();
                timer.Stop();
                GameOnTrack.Close();
                WebVideo.Close(roomNum);                
                Serial.Close(HWPort);
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
            }
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
                        ShowInfo("Connected to video server. ");
                    }
                    else
                    {
                        Log.SetLog("Error: Connect to video server failed. ");
                        ShowInfo("Error: Connect to video server failed. ");
                    }
                    break;
                case AnyChatCoreSDK.WM_GV_LOGINSYSTEM:
                    /// Login system
                    if (lParam.ToInt32() == 0)
                    {
                        ShowInfo("Success login video server. ");
                        myUserID = wParam.ToInt32();
                        AnyChatCoreSDK.EnterRoom(roomNum, "", 0);
                    }
                    else
                    {
                        Log.SetLog("Login video server failed, Error=" + lParam.ToString());
                        ShowInfo("Login video server failed. ");
                    }
                    break;
                case AnyChatCoreSDK.WM_GV_ENTERROOM:
                    // Enter room
                    int lparam = lParam.ToInt32();
                    if (lparam == 0)
                    {
                        int roomid = wParam.ToInt32();
                        ShowInfo("Success entered video server room. ");
                        roomNum = roomid;
                        // Open local video camera on the interface
                        WebVideo.OpenLocalVideo(WebVideo.GetLocalVideoDeivceName(), hwnd, localCamLeft, localCamTop, localCamRight, localCamBottom, localCamIndex);
                    }
                    else
                    {
                        Log.SetLog("Enter video server room failed，" + lparam.ToString());
                        ShowInfo("Enter video server room failed. ");
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
                            SetRemoteCamPos(usersID[idx], true);
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
                            SetRemoteCamPos(userID, false);
                        }
                    }
                    else
                    {
                        if (userID != myUserID)
                        {
                            SetRemoteCamPos(userID, true);
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
                    ShowInfo("Lose  video connection. ");
                    break;
            }
            return IntPtr.Zero;
        }
        private void SetRemoteCamPos(int id, bool en)
        {
            IntPtr windowHdl = new WindowInteropHelper(Application.Current.MainWindow).Handle;

            byte[] userNameByte = new byte[255];
            int ret = AnyChatCoreSDK.GetUserName(id, ref userNameByte[0], 30);
            string userName = ByteToString(userNameByte);

            if (string.Equals(userName, "s151001-Gaze"))
            {
                WebVideo.ControlRemoteVideo(id, en, windowHdl, remoteCamLeft, remoteCamTop, remoteCamRight, remoteCamBottom, remoteCamIndex);
            }
        }

        private string ByteToString(byte[] byteStr)
        {
            string retVal = "";
            try
            {
                retVal = System.Text.Encoding.GetEncoding("GB18030").GetString(byteStr, 0, byteStr.Length);
            }
            catch (Exception exp)
            {
                Console.Write(exp.Message);
            }
            return retVal.TrimEnd('\0');
        }

        #endregion

        #region Command handle

        // Send command to PC
        static public void SendCmd(string cmd)
        {
            bool secret = false;
            int userID = -1; // -1 means to all users
            int ret = -1;
            ret = AnyChatCoreSDK.SendTextMessage(userID, secret, cmd, cmd.Length);
        }

        internal static void fbCmd()
        {
            if (Map.pointsPath.Count>0)
            {
                int index = int.Parse(Map.pointsPath[0].ToString());
                SendCmd("DTU" + Wheelchair.state.ToString() + index.ToString());
            }
            else
            {
                SendCmd("DTU"+ Wheelchair.state.ToString() + Wheelchair.targetPoint.ToString());
            }
        }

        // Receive cmd from PC
        private void ReceivedCmd(int fromUID, int toUID, string Text, bool isserect)
        {
            // Show received command
            ShowInfo("Received command: " + Text);
            Log.SetLog("Received command: " + Text + " at " + DateTime.Now.ToFileTime().ToString());

            // Check the identification characters
            if (Text.Substring(0, 4) == "GAZE")
            {              
                // Whether user want to stop the wheelchair       
                if (int.Parse(Text.Substring(7, 1)) == 0)
                {
                    if (Wheelchair.targetPoint != int.Parse(Text.Substring(4, 3)))
                    {
                        Wheelchair.targetPoint = int.Parse(Text.Substring(4, 3));

                        // Generate path
                        ShowInfo("New target point is " + Wheelchair.targetPoint.ToString() );                       
                        Map.CreatePath(Wheelchair.currentPoint, Wheelchair.targetPoint);

                        // Print the path information
                        string pathStr = Map.pointsPath[0].ToString();
                        int pointsNum = Map.pointsPath.Count;
                        for (var index = 1; index< pointsNum; index++)
                        {
                            pathStr = pathStr + "-" + Map.pointsPath[index].ToString() ;
                        }
                        ShowInfo("The path is: " + pathStr);
                               
                        Wheelchair.state = 1;                        
                    }
                    fbCmd();
                    Wheelchair.Move();
                }
                else
                {
                    // Stop the wheelchair
                    Wheelchair.Stop();
                    ShowInfo("Wheelchair stopped");                
                }
            }
            else if (Text.Substring(0,2) == "DK")
            {
                Wheelchair.state = 3;
                // Send the command direct to the hardware
                if (HWPort.IsOpen)
                {
                    try
                    {
                        HWDataSend(Text);
                    }
                    catch (Exception)
                    {
                        ShowInfo("Serial port sent command failed. ");
                    }
                }
                else
                {
                    ShowInfo("Error: Serial port is not open!");
                }
            }
        }

        // Serial port send
        internal static void HWDataSend(string c)
        {
            Log.SetLog("Sendcmd: " + c);
            HWPort.Write(c);
        }

        internal static void HWDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string serialFB = HWPort.ReadLine();
            if (serialFB.Substring(0, 4) == "DATA")
            {
                string ldir = serialFB.Substring(4, 1);
                string lspd = serialFB.Substring(5, 3);
                string rdir = serialFB.Substring(8, 1);
                string rspd = serialFB.Substring(9, 3);
                string s1 = serialFB.Substring(12, 3);
                string s2 = serialFB.Substring(15, 3);
                string s3 = serialFB.Substring(18, 3);
                string s4 = serialFB.Substring(21, 3);
                string s5 = serialFB.Substring(24, 3);
                Data.SetData((DateTime.Now.ToFileTime()/1000).ToString() +','+ ldir + ',' + lspd + ',' + rdir + ',' + rspd + ',' + s1 + ',' +s2 + ',' +s3 + ',' +s4 + ',' + s5 + ',' + Wheelchair.x.ToString("000") + ',' +  Wheelchair.y.ToString("000") + ',' +  Wheelchair.theta.ToString("000") );
            }
        }

        #endregion

        #region Show information

        public void ShowInfo(string text)
        {
            sysInfoTxt.AppendText(DateTime.Now.ToString() + ": " + text + "\r\n");
            sysInfoTxt.ScrollToEnd();
        }
        
        #endregion

        #region Timer Click

        private void Timer_tick(object sender, EventArgs e)
        {
            // Get robot current position
            Wheelchair.UpdatePosition();
            RobXTxt.Text = Wheelchair.x.ToString("000");
            RobYTxt.Text = Wheelchair.y.ToString("000");
            RobThTxt.Text = Wheelchair.theta.ToString("000");
        }     

        #endregion
    }
}

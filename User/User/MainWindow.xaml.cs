using ANYCHATAPI;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace User
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IGazeListener
    {
        #region Variables

        // Window size
        static public int width = 0;
        static public int height = 0;

        // Server information
        public string address = "demo.anychat.cn";
        public int port = 8906;
        public int roomNum = 151001;

        // User information
        public string userName = "s151001-Gaze";
        public string userPassword = "s151001";
        private int myUserID = -1;

        // Local camera configuration
        static public PresentationSource source;

        public int localCamIndex = 0;
        static public double localCamLeftP = 0.0;
        static public double localCamRightP = 0.0;
        static public double localCamTopP = 0.0;
        static public double localCamBottomP = 0.0;

        public int localCamLeft = 0;
        public int localCamRight = 0;
        public int localCamTop = 0;
        public int localCamBottom = 0;

        // Remote camera configuration
        public int[] remoteCamIndex = { 0, 2 };
        static public double[] remoteCamLeftP = { 0.203, 0.007 };
        static public double[] remoteCamRightP = { 0.797, 0.196 };
        static public double[] remoteCamTopP = { 0.201, 0.784 };
        static public double[] remoteCamBottomP = { 0.771, 0.959 };

        public int[] remoteCamLeft = { 0, 0 };
        public int[] remoteCamRight = { 0, 0 };
        public int[] remoteCamTop = { 0, 0 };
        public int[] remoteCamBottom = { 0, 0 };

        // Gaze configuration
        static public int gazeX = 1000;
        static public int gazeY = 500;
        public int gazeState = 1;   // 1:not select, 2:selected, 3:triggered
        public int triggerObj = 0;
        public int selectObj = 0;
        public int triggerCount = 0;
        public int nontriggerCount = 0;
        public int totalCount = 100;       // duration to trigger a command, in ms
        public int triggerthres = 80;      // threshold of triggering a command

        // Timer
        DispatcherTimer gazeTimer = new DispatcherTimer();
        DispatcherTimer dataTimer = new DispatcherTimer();

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            Log.Init();
            Data.Init();
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            

            // Maximize the main window and bring to front
            WindowState = WindowState.Maximized;
            Activate();

            // Start eye tribe server
            try
            {
                if (!GazeManager.Instance.IsActivated)
                {
                    // Process.Start(AppDomain.CurrentDomain.BaseDirectory + "EyeTribe.exe", "");
                }
                else if (!GazeManager.Instance.IsCalibrated)
                {
                    ShowInfo("Error: Gaze tracker has not been calibrated");
                }
                #pragma warning disable CS0618 // Type or member is obsolete
                GazeManager.Instance.Activate(GazeManagerCore.ApiVersion.VERSION_1_0, GazeManagerCore.ClientMode.Push);
                #pragma warning restore CS0618 // Type or member is obsolete
                GazeManager.Instance.AddGazeListener(this);
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
                    width = Convert.ToInt32(SystemParameters.WorkArea.Width * source.CompositionTarget.TransformToDevice.M11);
                    height = Convert.ToInt32(SystemParameters.WorkArea.Height * source.CompositionTarget.TransformToDevice.M22);

                    localCamLeft = Convert.ToInt32(localCamLeftP * SystemParameters.WorkArea.Width * source.CompositionTarget.TransformToDevice.M11);
                    localCamRight = Convert.ToInt32(localCamRightP * SystemParameters.WorkArea.Width * source.CompositionTarget.TransformToDevice.M11);
                    localCamTop = Convert.ToInt32(localCamTopP * SystemParameters.WorkArea.Height * source.CompositionTarget.TransformToDevice.M22);
                    localCamBottom = Convert.ToInt32(localCamBottomP * SystemParameters.WorkArea.Height * source.CompositionTarget.TransformToDevice.M22);

                    for (var i = 0; i< remoteCamIndex.Length; i++)
                    {
                        remoteCamLeft[i] = Convert.ToInt32(remoteCamLeftP[i] * SystemParameters.WorkArea.Width * source.CompositionTarget.TransformToDevice.M11);
                        remoteCamRight[i] = Convert.ToInt32(remoteCamRightP[i] * SystemParameters.WorkArea.Width * source.CompositionTarget.TransformToDevice.M11);
                        remoteCamTop[i] = Convert.ToInt32(remoteCamTopP[i] * SystemParameters.WorkArea.Height * source.CompositionTarget.TransformToDevice.M22);
                        remoteCamBottom[i] = Convert.ToInt32(remoteCamBottomP[i] * SystemParameters.WorkArea.Height * source.CompositionTarget.TransformToDevice.M22);
                    }
                }
                else
                {
                    width = Convert.ToInt32(SystemParameters.WorkArea.Width);
                    height = Convert.ToInt32(SystemParameters.WorkArea.Height);

                    localCamLeft = Convert.ToInt32(localCamLeftP * SystemParameters.WorkArea.Width);
                    localCamRight = Convert.ToInt32(localCamRightP * SystemParameters.WorkArea.Width);
                    localCamTop = Convert.ToInt32(localCamTopP * SystemParameters.WorkArea.Height);
                    localCamBottom = Convert.ToInt32(localCamBottomP * SystemParameters.WorkArea.Height);

                    for (var i = 0; i < remoteCamIndex.Length; i++)
                    {
                        remoteCamLeft[i] = Convert.ToInt32(remoteCamLeftP[i] * SystemParameters.WorkArea.Width);
                        remoteCamRight[i] = Convert.ToInt32(remoteCamRightP[i] * SystemParameters.WorkArea.Width);
                        remoteCamTop[i] = Convert.ToInt32(remoteCamTopP[i] * SystemParameters.WorkArea.Height);
                        remoteCamBottom[i] = Convert.ToInt32(remoteCamBottomP[i] * SystemParameters.WorkArea.Height);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
                ShowInfo("Error: " + ex.Message.ToString());
            }

            // Start the timer, every 1 ms.
            try
            {
                gazeTimer.Tick += new EventHandler(gaze_tick);
                gazeTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
                gazeTimer.Start();
                ShowInfo("Gaze timer started");
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
                ShowInfo("Error: " + ex.Message.ToString());
            }

            // Start the timer, every 1 ms.
            try
            {
                dataTimer.Tick += new EventHandler(log_data);
                dataTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                dataTimer.Start();
                ShowInfo("Gaze timer started");
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
                ShowInfo("Error: " + ex.Message.ToString());
            }

            Activate();
        }

        private void log_data(object sender, EventArgs e)
        {
            if (IsActive)
            {
                Data.SetData((DateTime.Now.ToFileTime() / 1000).ToString() + ',' + gazeX + ',' + gazeY + ',' + gazeState + ',' + selectObj + ',' + triggerObj);
            }          
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                WebVideo.Close(roomNum);
                gazeTimer.Stop();
                GazeManager.Instance.Deactivate();
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
            }
        }

        #endregion

        #region WndProc

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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
                            SetRemoteCamPos(usersID[idx],true);
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
                    ShowInfo("Lose video connection. ");
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

            if (string.Equals(userName, "s151001-robot"))
            {
                WebVideo.ControlRemoteVideo(id, en, windowHdl, remoteCamLeft[0], remoteCamTop[0], remoteCamRight[0], remoteCamBottom[0], remoteCamIndex[0]);
            }

            if (string.Equals(userName, "s151001-usbcam"))
            {
                WebVideo.ControlRemoteVideo(id, en, windowHdl, remoteCamLeft[1], remoteCamTop[1], remoteCamRight[1], remoteCamBottom[1], remoteCamIndex[1]);
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

        static public void SendCmd(string cmd)
        {
            bool secret = false;
            int userID = -1; // -1 means to all users
            int ret = -1;
            ret = AnyChatCoreSDK.SendTextMessage(userID, secret, cmd, cmd.Length);
        }

        private void ReceivedCmd(int fromUID, int toUID, string Text, bool isserect)
        {
            if (Text.Substring(0, 3) == "DTU")
            {
                string s = Text.Substring(3, 1);
                string p = Text.Substring(4, 3);

                switch (s)
                {
                    case "0":
                        ShowInfo("Robot stopped!");
                        break;
                    case "1":
                        ShowInfo("Robot is turning to point "+p);
                        break;
                    case "2":
                        ShowInfo("Robot is heading to point "+p);
                        break;
                    case "3":
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Show information

        public void ShowInfo(string text)
        {
            sysInfoTxt.AppendText(DateTime.Now.ToString("HH:mm:ss") + ": " + text + "\r\n");
            sysInfoTxt.ScrollToEnd();
        }

        #endregion

        #region User interaction

        static public BitmapImage setSource(string url)
        {
            return new BitmapImage(new Uri(url, UriKind.Relative));
        }

        private void fwdImg_MouseEnter(object sender, MouseEventArgs e)
        {
            fwdImg.Source = setSource("images/abtnbg.png");
        }

        private void fwdImg_MouseLeave(object sender, MouseEventArgs e)
        {
            fwdImg.Source = setSource("images/btnbg.png");
        }

        private void backImg_MouseEnter(object sender, MouseEventArgs e)
        {
            backImg.Source = setSource("images/abtnbg.png");
        }

        private void backImg_MouseLeave(object sender, MouseEventArgs e)
        {
            backImg.Source = setSource("images/btnbg.png");
        }

        private void viewImg_MouseEnter(object sender, MouseEventArgs e)
        {
            viewImg.Source = setSource("images/abtnbg.png");
        }

        private void viewImg_MouseLeave(object sender, MouseEventArgs e)
        {
            viewImg.Source = setSource("images/btnbg.png");
        }

        private void leftImg_MouseEnter(object sender, MouseEventArgs e)
        {
            leftImg.Source = setSource("images/abtnbg.png");
        }

        private void leftImg_MouseLeave(object sender, MouseEventArgs e)
        {
            leftImg.Source = setSource("images/btnbg.png");
        }

        private void rightImg_MouseEnter(object sender, MouseEventArgs e)
        {
            rightImg.Source = setSource("images/abtnbg.png");
        }

        private void rightImg_MouseLeave(object sender, MouseEventArgs e)
        {
            rightImg.Source = setSource("images/btnbg.png");
        }

        private void stopImg_MouseEnter(object sender, MouseEventArgs e)
        {
            stopImg.Source = setSource("images/abtnbg.png");
        }

        private void stopImg_MouseLeave(object sender, MouseEventArgs e)
        {
            stopImg.Source = setSource("images/btnbg.png");
        }

        private void viewImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Waypoint window        
            SubWindow wayPointsView = new SubWindow();
            wayPointsView.Show();
            wayPointsView.WindowState = WindowState.Maximized;
            wayPointsView.Activate();
        }

        private void fwdImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCmd("DK1020302030");
            ShowInfo("Send forward command");
        }

        private void leftImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCmd("DK1010002030");
            ShowInfo("Send left turn command");
        }

        private void rightImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCmd("DK1020301000");
            ShowInfo("Send right turn command");
        }

        private void backImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCmd("DK0000300030");
            ShowInfo("Send backward command");
        }

        private void stopImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCmd("DK0110001000");
            ShowInfo("Send stop command");
        }

        private void mapview_MouseEnter(object sender, MouseEventArgs e)
        {
            viewImg.Source = setSource("images/abtnbg.png");
        }

        private void fwd_arrow_MouseEnter(object sender, MouseEventArgs e)
        {
            fwdImg.Source = setSource("images/abtnbg.png");
        }

        private void stop_icon_MouseEnter(object sender, MouseEventArgs e)
        {
            stopImg.Source = setSource("images/abtnbg.png");
        }

        private void left_arrow_MouseEnter(object sender, MouseEventArgs e)
        {
            leftImg.Source = setSource("images/abtnbg.png");
        }

        private void right_arrow_MouseEnter(object sender, MouseEventArgs e)
        {
            rightImg.Source = setSource("images/abtnbg.png");
        }

        private void back_arrow_MouseEnter(object sender, MouseEventArgs e)
        {
            backImg.Source = setSource("images/abtnbg.png");
        }

        private void mapview_MouseLeave(object sender, MouseEventArgs e)
        {
            viewImg.Source = setSource("images/btnbg.png");
        }

        private void fwd_arrow_MouseLeave(object sender, MouseEventArgs e)
        {
            fwdImg.Source = setSource("images/btnbg.png");
        }

        private void stop_icon_MouseLeave(object sender, MouseEventArgs e)
        {
            stopImg.Source = setSource("images/btnbg.png");
        }

        private void left_arrow_MouseLeave(object sender, MouseEventArgs e)
        {
            leftImg.Source = setSource("images/btnbg.png");
        }

        private void right_arrow_MouseLeave(object sender, MouseEventArgs e)
        {
            rightImg.Source = setSource("images/btnbg.png");
        }

        private void back_arrow_MouseLeave(object sender, MouseEventArgs e)
        {
            backImg.Source = setSource("images/btnbg.png");
        }

        private void mapview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Waypoint window        
            SubWindow wayPointsView = new SubWindow();
            wayPointsView.Show();
            wayPointsView.WindowState = WindowState.Maximized;
            wayPointsView.Activate();
        }

        private void fwd_arrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCmd("DK1020302030");
            ShowInfo("Send forward command");
        }

        private void stop_icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCmd("DK0110001000");
            ShowInfo("Send stop command");
        }

        private void left_arrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCmd("DK1010002030");
            ShowInfo("Send left turn command");
        }

        private void right_arrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCmd("DK1020301000");
            ShowInfo("Send right turn command");
        }

        private void back_arrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCmd("DK0000300030");
            ShowInfo("Send backward command");
        }

        #endregion

        #region Gaze Control

        public void OnGazeUpdate(GazeData gazeData)
        {
            gazeX = (int)Math.Round(gazeData.SmoothedCoordinates.X, 0);
            gazeY = (int)Math.Round(gazeData.SmoothedCoordinates.Y, 0);
        }

        private void gaze_tick(object sender, EventArgs e)
        {
            double x = gazeX;
            double y = gazeY;

            if (IsActive)
            {
                Canvas.SetLeft(gaze, x);
                Canvas.SetTop(gaze, y);

                selectObj = CheckHit(x, y);

                switch (gazeState)
                {
                    case 1:
                        if (selectObj > 0)
                        {
                            triggerObj = selectObj;
                            gazeState = 2;
                            triggerCount = 0;
                            nontriggerCount = 0;
                            gaze.Source = setSource("images/Gaze-select.png");
                        }                       
                        break;
                    case 2:
                        if (selectObj == triggerObj)
                        {
                            triggerCount = triggerCount + 1;
                        }
                        else
                        {
                            nontriggerCount = nontriggerCount + 1;
                        }

                        if (triggerCount > triggerthres)
                        {
                            triggerCmd(triggerObj);
                            gazeState = 3;
                            triggerCount = 0;
                            nontriggerCount = 0;
                            gaze.Source = setSource("images/Gaze-trigger.png");
                        }
                        if (nontriggerCount > totalCount - triggerthres)
                        {
                            ResetGaze();
                        }
                        break;
                    case 3:
                        if (selectObj == triggerObj)
                        {
                            triggerCount = triggerCount + 1;
                        }
                        else
                        {
                            nontriggerCount = nontriggerCount + 1;
                        }
                        if (triggerCount > triggerthres)
                        {
                            triggerCount = 0;
                            nontriggerCount = 0;
                        }
                        if (nontriggerCount > totalCount - triggerthres)
                        {
                            SendCmd("DK0110001000");
                            ShowInfo("Send stop command");
                            ResetGaze();                           
                        }
                        break;
                    default:
                        break;
                }
            }       
        }

        private void ResetGaze()
        {
            triggerCount = 0;
            nontriggerCount = 0;
            gazeState = 1;
            gaze.Source = setSource("images/Gaze.png");
        }

        private void triggerCmd(int obj)
        {
            switch (obj)
            {
                case 1:
                    // Waypoint window        
                    SubWindow wayPointsView = new SubWindow();
                    wayPointsView.Show();
                    wayPointsView.WindowState = WindowState.Maximized;
                    wayPointsView.Activate();
                    break;
                case 2:
                    SendCmd("DK1020202020");
                    ShowInfo("Send forward command");
                    break;
                case 3:
                    SendCmd("DK0110001000");
                    ShowInfo("Send stop command");
                    break;
                case 4:
                    SendCmd("DK1010002020");
                    ShowInfo("Send left turn command");
                    break;
                case 5:
                    SendCmd("DK1020201000");
                    ShowInfo("Send right turn command");
                    break;
                case 6:
                    SendCmd("DK1000200020");
                    ShowInfo("Send backward command");
                    break;
                default:
                    break;
            }
        }

        private int CheckHit(double x, double y)
        {
            int obj = 0;
            if ( x <0.2*width && y < 0.2*height)
            {
                obj = 1;    // view btn
            }
            if (x > 0.2 * width && x<0.8*width && y < 0.2 * height)
            {
                obj = 2;    // fwd btn
            }
            if (x > 0.8 * width && y < 0.2 * height)
            {
                obj = 3;    // stop btn
            }
            if (x < 0.2 * width && y>0.2*height && y < 0.8 * height)
            {
                obj = 4;    // left btn
            }
            if (x > 0.8 * width && y > 0.2 * height && y < 0.8 * height)
            {
                obj = 5;    // right btn
            }
            if ( x> 0.2 * width && x < 0.8 * width && y > 0.8 * height)
            {
                obj = 6;    // back btn
            }
            return obj;
        }
        #endregion
    }
}

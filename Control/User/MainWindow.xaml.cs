using ANYCHATAPI;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace User
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
        public string userName = "s151001-student";
        public string userPassword = "s151001";
        private int myUserID = -1;

        // Remote camera configuration
        static public PresentationSource source;

        public int remoteCamIndex = 0;
        static public double remoteCamLeftP = 0.007;
        static public double remoteCamRightP = 0.328;
        static public double remoteCamTopP = 0.654;
        static public double remoteCamBottomP = 0.959;

        public int remoteCamLeft = 0;
        public int remoteCamRight = 0;
        public int remoteCamTop = 0;
        public int remoteCamBottom = 0;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            Log.Init();
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

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
                    remoteCamLeft = Convert.ToInt32(remoteCamLeftP * SystemParameters.WorkArea.Width * source.CompositionTarget.TransformToDevice.M11);
                    remoteCamRight = Convert.ToInt32(remoteCamRightP * SystemParameters.WorkArea.Width * source.CompositionTarget.TransformToDevice.M11);
                    remoteCamTop = Convert.ToInt32(remoteCamTopP * SystemParameters.WorkArea.Height * source.CompositionTarget.TransformToDevice.M22);
                    remoteCamBottom = Convert.ToInt32(remoteCamBottomP * SystemParameters.WorkArea.Height * source.CompositionTarget.TransformToDevice.M22);
                }
                else
                {
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

            // Maximize the main window and bring to front
            WindowState = WindowState.Maximized;
            Activate();         
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                WebVideo.Close(roomNum);
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

        static public void SendCmd(string cmd)
        {
            bool secret = false;
            int userID = -1; // -1 means to all users
            int ret = -1;
            ret = AnyChatCoreSDK.SendTextMessage(userID, secret, cmd, cmd.Length);
        }

        private void ReceivedCmd(int fromUID, int toUID, string Text, bool isserect)
        {
            //
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


        // Front left
        private void lfImg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lfImg.Source = setSource("images/abtnbg.png");
        }

        private void lfImg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lfImg.Source = setSource("images/btnbg.png");
        }

        private void lfImg_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1020602070");
            ShowInfo("Send front left command");
        }

        private void lficon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lfImg.Source = setSource("images/abtnbg.png");
        }

        private void lficon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lfImg.Source = setSource("images/btnbg.png");
        }

        private void lficon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1020602070");
            ShowInfo("Send front left command");
        }

        // Forward
        private void fImg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            fImg.Source = setSource("images/abtnbg.png");
        }

        private void fImg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            fImg.Source = setSource("images/btnbg.png");
        }

        private void fImg_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1020602060");
            ShowInfo("Send forward command");
        }

        private void ficon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            fImg.Source = setSource("images/abtnbg.png");
        }

        private void ficon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            fImg.Source = setSource("images/btnbg.png");
        }

        private void ficon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1020602060");
            ShowInfo("Send forward command");
        }

        // Front right
        private void rfImg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rfImg.Source = setSource("images/abtnbg.png");
        }

        private void rfImg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rfImg.Source = setSource("images/btnbg.png");
        }

        private void rfImg_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1020702060");
            ShowInfo("Send front right command");
        }

        private void rficon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rfImg.Source = setSource("images/abtnbg.png");
        }

        private void rficon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1020702060");
            ShowInfo("Send front right command");
        }

        // Left
        private void lImg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lImg.Source = setSource("images/abtnbg.png");
        }

        private void lImg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lImg.Source = setSource("images/btnbg.png");
        }

        private void lImg_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1010002030");
            ShowInfo("Send left turn command");
        }

        private void licon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lImg.Source = setSource("images/abtnbg.png");
        }

        private void licon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lImg.Source = setSource("images/btnbg.png");
        }

        private void licon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1010002030");
            ShowInfo("Send left turn command");
        }

        // Stop
        private void stopImg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            stopImg.Source = setSource("images/abtnbg.png");
        }

        private void stopImg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            stopImg.Source = setSource("images/btnbg.png");
        }

        private void stopImg_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK0010001000");
            ShowInfo("Send stop command");
        }

        private void stopicon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            stopImg.Source = setSource("images/abtnbg.png");
        }

        private void stopicon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            stopImg.Source = setSource("images/btnbg.png");
        }

        private void stopicon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK0010001000");
            ShowInfo("Send stop command");
        }

        // Right
        private void rImg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rImg.Source = setSource("images/abtnbg.png");
        }

        private void rImg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rImg.Source = setSource("images/btnbg.png");
        }

        private void rImg_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1020301000");
            ShowInfo("Send right turn command");
        }

        private void ricon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rImg.Source = setSource("images/abtnbg.png");
        }

        private void rficon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rfImg.Source = setSource("images/btnbg.png");
        }

        private void ricon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rImg.Source = setSource("images/btnbg.png");
        }

        private void ricon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1020301000");
            ShowInfo("Send right turn command");
        }

        // Backwards
        private void bImg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            bImg.Source = setSource("images/abtnbg.png");
        }

        private void bImg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            bImg.Source = setSource("images/btnbg.png");
        }

        private void bImg_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1000300030");
            ShowInfo("Send backward command");
        }

        private void bicon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            bImg.Source = setSource("images/abtnbg.png");
        }

        private void bicon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            bImg.Source = setSource("images/btnbg.png");
        }

        private void bicon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SendCmd("DK1000300030");
            ShowInfo("Send backward command");
        }

        #endregion
    }
}

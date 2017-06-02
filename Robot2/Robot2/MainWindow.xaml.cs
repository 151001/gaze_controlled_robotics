using ANYCHATAPI;
using System;
using System.Windows;
using System.Windows.Interop;

namespace Robot2
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
        public string userName = "s151001-usbcam";
        public string userPassword = "s151001";
        private int myUserID = -1;

        // Local camera configuration
        public int localCamIndex = 2;
        
        public int localCamLeft = 20;
        public int localCamRight = 755;
        public int localCamTop = 40;
        public int localCamBottom = 530;

        static public PresentationSource source;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            Log.Init();
        }

        #endregion

        #region Window Events

        // Load window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // Init web video
            try
            {
                IntPtr windowHdl = new WindowInteropHelper(Application.Current.MainWindow).Handle;

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
        }

        // Close window
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
                    break;
                case AnyChatCoreSDK.WM_GV_USERATROOM:
                    /// New user enter room
                    int userID = wParam.ToInt32();
                    int boEntered = lParam.ToInt32();
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

        #endregion

        #region Show information

        public void ShowInfo(string text)
        {
            sysInfoTxt.AppendText(DateTime.Now.ToString() + ": " + text + "\r\n");
            sysInfoTxt.ScrollToEnd();
        }

        #endregion
    }
}

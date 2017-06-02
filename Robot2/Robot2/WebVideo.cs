using ANYCHATAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Robot2
{
    class WebVideo
    {
        static public void Init(IntPtr hdl)
        {
            // Init AnychatSDK
            string path = AppDomain.CurrentDomain.BaseDirectory;
            AnyChatCoreSDK.SetSDKOption(AnyChatCoreSDK.BRAC_SO_CORESDK_PATH, path, path.Length);
            SystemSetting.Init(hdl);
        }

        static public void Close(int roomNum)
        {
            AnyChatCoreSDK.LeaveRoom(roomNum);
            AnyChatCoreSDK.Logout();
            AnyChatCoreSDK.Release();
        }

        #region Local camera

        static public void OpenLocalVideo(string[] videoDeviceName, IntPtr hwnd, int left, int right, int top, int bottom, int index)
        {
            try
            {
                int videoDeviceNum = videoDeviceName.Length;

                AnyChatCoreSDK.SetUserStreamInfo(-1, index, AnyChatCoreSDK.BRAC_SO_LOCALVIDEO_DEVICENAME, videoDeviceName[index], videoDeviceName[index].ToCharArray().Length);
                AnyChatCoreSDK.SetVideoPosEx(-1, hwnd, left, right, top, bottom, index, 0);
                AnyChatCoreSDK.UserCameraControlEx(-1, true, index, 0, string.Empty);
                AnyChatCoreSDK.UserSpeakControlEx(-1, false, index, 0, string.Empty);
            }
            catch (Exception ex)
            {
                Log.SetLog("Local video open error: " + ex.Message.ToString());
            }
        }

        static public string[] GetLocalVideoDeivceName()
        {
            string[] retVal = null;

            int deviceNum = 0;
            AnyChatCoreSDK.EnumVideoCapture(null, ref deviceNum);
            IntPtr[] deviceList = new IntPtr[deviceNum];
            retVal = new string[deviceNum];

            AnyChatCoreSDK.EnumVideoCapture(deviceList, ref deviceNum);
            for (int idx = 0; idx < deviceNum; idx++)
            {
                IntPtr intPtr = deviceList[idx];
                int len = 100;
                byte[] byteArray = new byte[len];
                Marshal.Copy(intPtr, byteArray, 0, len);
                string DeviceName = Encoding.Default.GetString(byteArray);
                DeviceName = DeviceName.Substring(0, DeviceName.IndexOf('\0'));
                retVal[idx] = DeviceName;
            }
            return retVal;
        }

        #endregion
    }
}

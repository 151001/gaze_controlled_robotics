using EyeTribe.ClientSdk;
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
using System.Windows.Shapes;
using EyeTribe.ClientSdk.Data;
using System.Windows.Threading;
using System.Windows.Interop;

namespace User
{
    /// <summary>
    /// Interaction logic for SubWindow.xaml
    /// </summary>
    public partial class SubWindow : Window
    {
        
        // Gaze configuration
        public int gazeState = 1;   // 1:not select, 2:selected, 3:triggered
        public int triggerObj = 0;
        public int selectObj = 0;
        public int triggerCount = 0;
        public int nontriggerCount = 0;
        public int totalCount = 100;       // duration to trigger a command, in ms
        public int triggerthres = 80;      // threshold of triggering a command

        // Timer
        DispatcherTimer gazeTimer = new DispatcherTimer();

        public SubWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                gazeTimer.Tick += new EventHandler(gaze_tick);
                gazeTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
                gazeTimer.Start();
            }
            catch (Exception ex)
            {
                Log.SetLog("Exception: " + ex.Message.ToString());
            }
        }

        private void gaze_tick(object sender, EventArgs e)
        {
            double x = MainWindow.gazeX - gaze.ActualWidth / 2;
            double y = MainWindow.gazeY - gaze.ActualHeight / 2;
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
                case 1:     // 0 btn
                    // Waypoint window        
                    MainWindow.SendCmd("GAZE0000");
                    break;
                case 2:     // 1 btn
                    MainWindow.SendCmd("GAZE0010");
                    break;
                case 3:     // 2 btn
                    MainWindow.SendCmd("GAZE0020");
                    break;
                case 4:     // 3 btn
                    MainWindow.SendCmd("GAZE0030");
                    break;
                case 5:     // control btn
                    Close();
                    break;
                default:
                    break;
            }
        }

        private int CheckHit(double x, double y)
        {
            int obj = 0;
            if (HitButton(x, y, 0.29, 0.39, 0.5, 0.6))
            {
                obj = 1;    // 0 btn
            }
            if (HitButton(x, y, 0.47, 0.57, 0.5, 0.6))
            {
                obj = 2;    // 1 btn
            }
            if (HitButton(x, y, 0.7, 0.8, 0.5, 0.6))
            {
                obj = 3;    // 2 btn
            }
            if (HitButton(x, y, 0.47, 0.57, 0.7, 0.8))
            {
                obj = 4;    // 3 btn
            }
            if (HitButton(x, y, 0.8, 1, 0, 0.2))
            {
                obj = 5;    // control btn
            }
            return obj;
        }

        private bool HitButton(double x, double y, double l, double r, double t, double b)
        {
            if (x>l*MainWindow.width && x<r*MainWindow.width && y>t * MainWindow.height && y < b*MainWindow.height)
            {
                return true;
            }
            return false;
        }

        #region User Interactions

        static public BitmapImage setSource(string url)
        {
            return new BitmapImage(new Uri(url, UriKind.Relative));
        }

        private void viewImg_MouseEnter(object sender, MouseEventArgs e)
        {
            viewImg.Source = setSource("images/abtnbg.png");
        }

        private void viewImg_MouseLeave(object sender, MouseEventArgs e)
        {
            viewImg.Source = setSource("images/btnbg.png");
        }

        private void viewImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void controlImg_MouseEnter(object sender, MouseEventArgs e)
        {
            viewImg.Source = setSource("images/abtnbg.png");
        }

        private void controlImg_MouseLeave(object sender, MouseEventArgs e)
        {
            viewImg.Source = setSource("images/btnbg.png");
        }

        private void controlImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void zeroImg_MouseEnter(object sender, MouseEventArgs e)
        {
            zeroImg.Source = setSource("images/azero.png");
        }

        private void zeroImg_MouseLeave(object sender, MouseEventArgs e)
        {
            zeroImg.Source = setSource("images/zero.png");
        }

        private void zeroImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SendCmd("GAZE0000");
        }

        private void oneImg_MouseEnter(object sender, MouseEventArgs e)
        {
            oneImg.Source = setSource("images/aone.png");
        }

        private void oneImg_MouseLeave(object sender, MouseEventArgs e)
        {
            oneImg.Source = setSource("images/one.png");
        }

        private void oneImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SendCmd("GAZE0010");
        }

        private void twoImg_MouseEnter(object sender, MouseEventArgs e)
        {
            twoImg.Source = setSource("images/atwo.png");
        }

        private void twoImg_MouseLeave(object sender, MouseEventArgs e)
        {
            twoImg.Source = setSource("images/two.png");
        }

        private void twoImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SendCmd("GAZE0020");
        }

        private void threeImg_MouseEnter(object sender, MouseEventArgs e)
        {
            threeImg.Source = setSource("images/athree.png");
        }

        private void threeImg_MouseLeave(object sender, MouseEventArgs e)
        {
            threeImg.Source = setSource("images/three.png");
        }

        private void threeImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SendCmd("GAZE0030");
        }

        #endregion
    }
}

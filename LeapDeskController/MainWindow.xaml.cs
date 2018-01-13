using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using WinApi;

namespace LeapDeskController
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        static extern void mouse_event(MouseFlags dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);

        [Flags]
        enum MouseFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            MouseWheel = 0x0800
        };


        public MainWindow()
        {
            InitializeComponent();

            Init();
        }

        void Init()
        {
            try
            {
                Controller c = new Controller();
                c.FrameTimestamp(1);
                c.FrameReady += NewFrame;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        double FirstFingerState = -1;
        bool cenMove = true;
        Tuple<double, double> mousePos = new Tuple<double, double>(0, 0);
        void NewFrame(object sender, FrameEventArgs e)
        {
            if(e.frame.Hands.Count > 1)
            {
                if(Include.GetAsyncKeyState(System.Windows.Forms.Keys.LWin) >= 0)
                {
                    Include.keybd_event((byte)System.Windows.Forms.Keys.LWin, 0, Include.KEYEVENTF_EXTENDEDKEY, 0);
                    Include.keybd_event((byte)System.Windows.Forms.Keys.Tab, 0, Include.KEYEVENTF_EXTENDEDKEY, 0);
                    Include.keybd_event((byte)System.Windows.Forms.Keys.Tab, 0, Include.KEYEVENTF_KEYUP, 0);
                }
            }
            if (e.frame.Hands.Count == 1)
            {
                if (Include.GetAsyncKeyState(System.Windows.Forms.Keys.LWin) < 0)
                {
                    Include.keybd_event((byte)System.Windows.Forms.Keys.LWin, 0, Include.KEYEVENTF_KEYUP, 0);
                }

                


                foreach (var h in e.frame.Hands)
                {
                    if (h.IsRight)
                    {
                        if (h.StabilizedPalmPosition.DistanceTo(h.Fingers[0].StabilizedTipPosition) < 72.5)
                        {
                            if (h.PalmPosition.y > mousePos.Item2)
                            {
                                mouse_event(MouseFlags.MouseWheel, 0, 0, (int)((mousePos.Item2 - h.PalmPosition.y) * 10), UIntPtr.Zero);
                            }
                            if (h.PalmPosition.y < mousePos.Item2)
                            {
                                mouse_event(MouseFlags.MouseWheel, 0, 0, (int)((mousePos.Item2 - h.PalmPosition.y) * 10), UIntPtr.Zero);
                            }
                        }
                        else
                        {
                            //mouse move
                            if(h.Fingers[0].StabilizedTipPosition.DistanceTo(h.StabilizedPalmPosition) < 95)
                            {
                                if (h.PalmPosition.x > mousePos.Item1 - 6)
                                {
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point(
                                        System.Windows.Forms.Cursor.Position.X + (int)((h.PalmPosition.x - mousePos.Item1) * 5),
                                        System.Windows.Forms.Cursor.Position.Y
                                    );
                                }
                                if (h.PalmPosition.x < mousePos.Item1 + 6)
                                {
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point(
                                        System.Windows.Forms.Cursor.Position.X - (int)((mousePos.Item1 - h.PalmPosition.x) * 5),
                                        System.Windows.Forms.Cursor.Position.Y
                                    );
                                }
                                if (h.PalmPosition.y > mousePos.Item2 - 6)
                                {
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point(
                                        System.Windows.Forms.Cursor.Position.X,
                                        System.Windows.Forms.Cursor.Position.Y - (int)((h.PalmPosition.y - mousePos.Item2) * 5)
                                    );
                                }
                                if (h.PalmPosition.y < mousePos.Item2 + 6)
                                {
                                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point(
                                        System.Windows.Forms.Cursor.Position.X,
                                        System.Windows.Forms.Cursor.Position.Y + (int)((mousePos.Item2 - h.PalmPosition.y) * 5)
                                    );
                                }
                            }


                            //mouse L R btn clics
                            if (h.StabilizedPalmPosition.DistanceTo(h.Fingers[1].StabilizedTipPosition) < 92.3)
                            {
                                if (Include.GetAsyncKeyState(System.Windows.Forms.Keys.LButton) >= 0)
                                {
                                    mouse_event(MouseFlags.Absolute | MouseFlags.LeftDown, 0, 0, 0, UIntPtr.Zero);
                                }
                            }
                            else
                            {
                                if (Include.GetAsyncKeyState(System.Windows.Forms.Keys.LButton) < 0)
                                {
                                    mouse_event(MouseFlags.Absolute | MouseFlags.LeftUp, 0, 0, 0, UIntPtr.Zero);
                                }
                            }

                            if (h.StabilizedPalmPosition.DistanceTo(h.Fingers[2].StabilizedTipPosition) < 92.3)
                            {
                                if (Include.GetAsyncKeyState(System.Windows.Forms.Keys.RButton) >= 0)
                                {
                                    mouse_event(MouseFlags.Absolute | MouseFlags.RightDown, 0, 0, 0, UIntPtr.Zero);
                                }
                            }
                            else
                            {
                                if (Include.GetAsyncKeyState(System.Windows.Forms.Keys.RButton) < 0)
                                {
                                    mouse_event(MouseFlags.Absolute | MouseFlags.RightUp, 0, 0, 0, UIntPtr.Zero);
                                }
                            }

                        }


                        mousePos = new Tuple<double, double>(
                            h.PalmPosition.x, 
                            h.PalmPosition.y
                        );
                    }
                }



            }
            if(e.frame.Hands.Count == 2)
            {
                foreach (var h in e.frame.Hands)
                {
                    if(h.IsRight)
                    {
                        if (h.Fingers[1].StabilizedTipPosition.x > FirstFingerState + 5)
                        {
                            if(cenMove)
                            {
                                Include.keybd_event((byte)System.Windows.Forms.Keys.Tab, 0, Include.KEYEVENTF_EXTENDEDKEY, 0);
                                Include.keybd_event((byte)System.Windows.Forms.Keys.Tab, 0, Include.KEYEVENTF_KEYUP, 0);
                                cenMove = false;
                            }
                        }
                        if (h.Fingers[1].StabilizedTipPosition.x < FirstFingerState)
                        {
                            cenMove = true;
                        }
                        FirstFingerState = h.Fingers[1].StabilizedTipPosition.x;
                    }
                }
            }
        }
    }
}

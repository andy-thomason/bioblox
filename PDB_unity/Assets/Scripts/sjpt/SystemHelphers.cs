//This file copied in from QuickHouse along with CSG for Unity. 
//Will try to avoid using it / gradually change to more appropriate things... 
//starting by removing using Windows.Forms & Drawing...

// This is a set of functions from Win32 and above that it is convenient to have together.
// The set of available functions is based entirely on what I have needed from time to time,
// with some convenience functions based above them.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using System.Windows.Forms;
using System.Threading;
using System.IO;

//using System.Drawing;

using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace SystemHelpers {
    
    // High performance Timer
    // from http://www.codeproject.com/KB/cs/highperformancetimercshar.aspx
    public class HiPerfTimer {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        private long startTime, stopTime;
        private long freq;
        
        // Constructor
        public HiPerfTimer() {
            startTime = 0;
            stopTime = 0;
            
            if (QueryPerformanceFrequency(out freq) == false) {
                throw new Exception("high-performance counter not supported");
            }
        }
        
        // Start the timer
        public void Start() {
            // lets do the waiting threads there work
            Thread.Sleep(0);
            QueryPerformanceCounter(out startTime);
        }
        
        // Stop the timer
        public void Stop() {
            QueryPerformanceCounter(out stopTime);
        }
        
        // Returns the duration of the timer (in seconds)
        public double Duration {
            get {
                return (double)(stopTime - startTime) / (double)freq;
            }
        }

        public double time(string message) {
            return time(message, 1);
        }

        public string lastmsg;

        public double time(string message, int n) {
            long etime;
            QueryPerformanceCounter(out etime);
            double t = (etime - startTime) / (double)freq;
            if (n == 1) {
                lastmsg = String.Format(">>> time: {0} time={1}", message, t);
            } else {
                lastmsg = String.Format(">>> time: {0} time={1}  n={2} each={3}", message, t, n, t / n);
            }
            Console.WriteLine(lastmsg);
            Start();
            return t;
        }
        
    }
    
    // could be static, but then I can't derive subclasses for convenient use ~ check alternatives
    public class Win32Helpers {
        
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")] public static extern ushort GetAsyncKeyState(uint vKey);

        public const uint VK_LCONTROL = 0xA2;
        public const uint VK_RCONTROL = 0xA3;
        public const uint VK_ESCAPE = 0x1B;

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(uint hWnd, out RECT lpRect);

        public static RECT GetClientRect(uint hWnd) {
            RECT r = new RECT();
            GetClientRect(hWnd, out r);
            return r;
        }

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(uint hWnd, out RECT lpRect);

        public static RECT GetWindowRect(uint hWnd) {
            RECT r = new RECT();
            GetWindowRect(hWnd, out r);
            return r;
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(
            uint hWnd,      // handle to destination window
            uint Msg,       // message
            uint wParam,  // first message parameter
            uint lParam   // second message parameter
        );

        [DllImport("user32.dll")]
        public static extern int PostMessage(
            uint hWnd,      // handle to destination window
            uint Msg,       // message
            uint wParam,  // first message parameter
            uint lParam   // second message parameter
        );

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessageAny(
            uint hWnd,      // handle to destination window
            uint Msg,       // message
            uint wParam,  // first message parameter
            object lParam   // second message parameter
        );

        [DllImport("User32.dll")]
        public static extern uint FindWindow(String lpClassName, String lpWindowName);
        //[DllImport("User32.dll")]
        //static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("User32.dll")]
        public static extern uint SetForegroundWindow(uint hWnd);

        [DllImport("User32.dll")]
        public static extern Boolean EnumChildWindows(uint hWndParent, Delegate lpEnumFunc, int lParam);

        [DllImport("User32.dll")]
        public static extern uint GetWindowText(uint hWnd, StringBuilder s, int nMaxCount);

        [DllImport("User32.dll")]
        public static extern uint GetWindowTextLength(uint hwnd);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern uint GetDesktopWindow();

        [DllImport("user32")]
        private static extern uint GetWindow(uint hwnd, int wCmd);

        [DllImport("user32", EntryPoint = "GetClassName")]
        public static extern uint GetClassNameA(uint hwnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern uint GetDC(uint ptr);

        [DllImport("user32")]
        public static extern uint GetParent(uint hwnd);

        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern uint SetParent(uint hWndChild, uint hWndNewParent);

        [System.Runtime.InteropServices.DllImport("user32", EntryPoint = "ReleaseDC", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        public static extern long ReleaseDC(uint hwnd, uint hdc);

        [System.Runtime.InteropServices.DllImport("gdi32", EntryPoint = "StretchBlt", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        public static extern bool StretchBlt(uint hdc, int x, int Y, int nWidth, int nHeight, uint hSrcDC, int xSrc, int ySrc, int nSrcWidth, int nSrcHeight, int dwRop);

        public const int SRCCOPY = 0XCC0020;
        // (DWORD) dest = source
        
        /* pjt Unity ::: type Point could not be found, and I very much doubt I want these functions...
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(Point lpPoint);
        
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point lpPoint);
        public static Point GetCursorPos() {
            Point p = new Point();
            GetCursorPos(ref p);
            return p;
        }
*/
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
                                              int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern uint WindowFromPoint(int x, int y);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(uint hWnd, int hWndInsertAfter, int X,
                                               int Y, int cx, int cy, uint uFlags);

        [DllImport("user32", EntryPoint = "MapVirtualKey")]
        public static extern int MapVirtualKey(int wCode, int wMapType);
        
        //        [DllImport("user32.dll")]
        //        public static extern bool InvalidateRect(uint hwnd, ref Rectangle r, bool bErase); //pjt Unity Rectangle unkown
        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(uint hwnd, IntPtr r, bool bErase);
        /* pjt Unity unwanted...
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT {
            uint length;
            uint flags;
            uint showCmd;
            Point ptMinPosition;
            Point ptMaxPosition;
            RECT rcNormalPosition;
        };
        */
        // todo ShowWindow(): http://msdn.microsoft.com/en-us/library/ms633548(VS.85).aspx
        // todo FindWindow(): http://msdn.microsoft.com/en-us/library/ms633499(VS.85).aspx (easier than FindWindowLike)
        // todo GetWindowPlacement(): http://msdn.microsoft.com/en-us/library/ms633518(VS.85).aspx
        
        //[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        //static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        //public const int WM_KEYDOWN = 0x0100;
        //public const int WM_KEYUP = 0x0101;
        //public const int WM_SYSKEYDOWN = 0x0104;
        //public const int WM_SYSKEYUP = 0x0105;
        //public const int WM_LBUTTONDOWN = 0x0201;
        //public const int WM_LBUTTONUP = 0x0202;
        
        //public const int WM_CHAR = 0x0102;
        //public const int WM_SETTEXT = 0x000c;
        //public const uint WM_PASTE = 0x0302;
        public const int GW_HWNDFIRST = 0;
        public const int GW_HWNDLAST = 1;
        public const int GW_HWNDNEXT = 2;
        public const int GW_HWNDPREV = 3;
        public const int GW_OWNER = 4;
        public const int GW_CHILD = 5;
        
        //public const int WM_KEYFIRST = 0X100;
        //public const int WM_DEADCHAR = 0x103;
        //public const int WM_SYSCHAR = 0x106;
        //public const int WM_SYSDEADCHAR = 0x107;
        //public const int WM_MOUSEFIRST = 0x200;
        //public const int WM_MOUSEMOVE = 0x200;
        //public const int WM_LBUTTONDBLCLK = 0x203;
        //public const int WM_RBUTTONDOWN = 0x204;
        //public const int WM_RBUTTONUP = 0x205;
        //public const int WM_RBUTTONDBLCLK = 0x206;
        //public const int WM_MBUTTONDOWN = 0x207;
        //public const int WM_MBUTTONUP = 0x208;
        //public const int WM_MBUTTONDBLCLK = 0x209;
        //public const int WM_MOUSEWHEEL = 0x20A;
        
        
        
        // WM_KEYUP/DOWN/CHAR HIWORD(lParam) flags
        public const int KF_EXTENDED = 0X100;
        public const int KF_DLGMODE = 0X800;
        public const int KF_MENUMODE = 0X1000;
        public const int KF_ALTDOWN = 0X2000;
        public const int KF_REPEAT = 0X4000;
        public const int KF_UP = 0X8000;
        
        
        //public const int WM_CLOSE = 0X10;
        //public const int WM_COMMAND = 0X111;
        //public const int WM_QUIT = 0X12;
        public const int BM_CLICK = 0XF5;
        
        public static readonly int HWND_TOPMOST = (-1);
        public static readonly int HWND_NOTOPMOST = (-2);
        public static readonly int HWND_TOP = (0);
        public static readonly int HWND_BOTTOM = (1);
        // From winuser.h
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 SWP_NOZORDER = 0x0004;
        public const UInt32 SWP_NOREDRAW = 0x0008;
        public const UInt32 SWP_NOACTIVATE = 0x0010;
        public const UInt32 SWP_FRAMECHANGED = 0x0020;
        /* The frame changed: send WM_NCCALCSIZE */
        public const UInt32 SWP_SHOWWINDOW = 0x0040;
        public const UInt32 SWP_HIDEWINDOW = 0x0080;
        public const UInt32 SWP_NOCOPYBITS = 0x0100;
        public const UInt32 SWP_NOOWNERZORDER = 0x0200;
        /* Don't do owner Z ordering */
        public const UInt32 SWP_NOSENDCHANGING = 0x0400;
        /* Don't send WM_WINDOWPOSCHANGING */
        public const UInt32 SWP_ASYNCWINDOWPOS = 0x4000;

        
        public enum MouseEventTFlags {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData,
                                              UIntPtr dwExtraInfo);

        
        public static int lastWaitTime = 0;
        // for debug, lenght of last waiting
        
        public static void trace(string msg) {
            System.Console.WriteLine(msg);
        }

        public static void Sleep(int k) {
            Thread.Sleep(k);
        }
        //public static void DoEvents() { Application.DoEvents(); } //pjt Unity::: Don't.
        public static int TimeGetTime() {
            return Environment.TickCount;
        }

        public static string GetWindowText(uint hwnd) {
            //Get the window text
            StringBuilder sb = new StringBuilder(255);
            /** uint r = **/
            GetWindowText(hwnd, sb, 255);
            return sb.ToString();
        }

        public static string GetClassName(uint hwnd) {
            //Get the class name
            StringBuilder sb = new StringBuilder(255);
            /** uint r = **/
            GetClassNameA(hwnd, sb, 255);
            return sb.ToString();
        }
        
        //
        // http://bytes.com/groups/net/124681-sendmessage-handle-wm_char-virtualkeys-vk_a-0-does-not-trigger-keydown
        public const uint GUpLParam = 0xC0220001;
        public const uint CtrlDownLParam = 0x11D0001;
        public const uint CtrlUpLParam = 0xC11D0001;
        public const uint GDownLParam = 0x220001;
        
        /* pjt Unity Keys not found
        public static void SendControl(uint hwnd, Keys k) {
            SendControl(hwnd, (uint)k);
        }
        */
        
        public static void SendControl(uint hwnd, uint k) {
            //SendMessage(hwnd, WM_KEYDOWN, 0x11, CtrlDownLParam);
            //SendMessage(hwnd, WM_KEYDOWN, k, GDownLParam);
            //SendMessage(hwnd, WM_CHAR, 20, GDownLParam);
            //SendMessage(hwnd, WM_KEYUP, k, GUpLParam);
            //SendMessage(hwnd, WM_KEYUP, 0x11, CtrlUpLParam);
            // System.Windows.Forms.KeysConverter
            
            SetForegroundWindow(hwnd);
            // by experiment
            Sleep(200);
            PostMessage(hwnd, WM_KEYDOWN, 0x00000011, 0x001D0001);
            Sleep(200);
            PostMessage(hwnd, WM_KEYDOWN, 0x00000054, 0x00140001);
            // PostMessage(hwnd, WM_CHAR, 0x00000014, 0x00140001); 
        }

        /** pjt Unity::: there'll be no key sending here...


        // feeble version of SendKeys, but able to send to non-front window
        public static void SendKeysAV(uint hwnd, string keys) {
            
            if (keys.Length == 0) return;
            uint k;
            string rest = "";
            switch (keys) {
            case "{ESC}": k = (uint)Keys.Escape; break;
            case "{ENTER}": k = (uint)Keys.Return; break;
            case "{TAB}": k = (uint)Keys.Tab; break;
            case "{DELETE}": k = (uint)Keys.Delete; break;
            case "{HOME}": k = (uint)Keys.Home; break;
            case "{LEFT}": k = (uint)Keys.Left; break;
            case "{RIGHT}": k = (uint)Keys.Right; break;
            case "{ALT}": k = (uint)Keys.Alt; break;
            case "{CTRL}": k = (uint)Keys.Control; break;
            default:
                k = keys[0];
                rest = keys.Substring(1);
                break;
            }
            int rc = SendMessage(hwnd, WM_CHAR, k, 0);
            SendKeysAV(hwnd, rest);
        }
        
         */

        /* pjt Unity ::: there'll be no window finding here...
        // find window
        // copied initially from http://vbnet.mvps.org/index.html?code/system/findwindowlikesimple.htm
        
        public static uint FindWindowLike(uint hWndStart, string WindowText, string Classname) {
            return FindWindowLike(hWndStart, new Regex(WindowText), new Regex(Classname));
        }
        
        public static uint FindWindowLike(uint hWndStart, string WindowText, string Classname, int Count) {
            return FindWindowLike(hWndStart, new Regex(WindowText), new Regex(Classname), false, Count);
        }
        
        public static uint FindWindowLike(uint hWndStart, Regex WindowText, Regex Classname, bool dotrace, int Count) {
            return FindWindowLike(hWndStart, WindowText, Classname, dotrace, Count, 0);
        }
        
        public static uint FindWindowLike(uint hWndStart, Regex WindowText, Regex Classname, bool dotrace) {
            return FindWindowLike(hWndStart, WindowText, Classname, dotrace, 1, 0);
        }
        
        public static uint FindWindowLike(uint hWndStart, Regex WindowText, Regex Classname) {
            return FindWindowLike(hWndStart, WindowText, Classname, false, 1, 0);
        }
        
        // display rect intfo
        public static string showrect(RECT rect) {
            string s = null;
            long b = 0;
            long t = 0;
            long l = 0;
            long r = 0;
            b = rect.bottom;
            t = rect.top;
            l = rect.left;
            r = rect.right;
            s = "rect " + (r - l) + "x" + (b - t) + "(" + l + ".." + r + ", " + t + ".." + b + ")";
            return s;
        }
        
        public static string foundWindowText; //  side effect of findWindowLike sets full window name
        public static uint FindWindowLike(uint hWndStart, Regex WindowText, Regex Classname, bool dotrace, int Count, int level) {
            uint tempFindWindowLike = 0;
            
            uint hwnd = 0;
            String sWindowText = null;
            String sClassname = null;
            //#uint r = 0;
            
            //Initialize if necessary. This is only executed
            //when level = 0 and hWndStart = 0, normally
            //only on the first call to the routine.
            if (level == 0) {
                if (hWndStart == 0) {
                    hWndStart = GetDesktopWindow();
                }
            }
            
            //Get first child window
            hwnd = GetWindow(hWndStart, GW_CHILD);
            
            while (!(hwnd == 0)) {
                
                //Search children by recursion
                uint shwnd = 0;
                shwnd = FindWindowLike(hwnd, WindowText, Classname, dotrace, Count, level + 1);
                if (shwnd != 0) {
                    tempFindWindowLike = shwnd;
                    if (Count == 0) {
                        return tempFindWindowLike;
                    }
                }
                
                //Get the window text and class name
                sWindowText = GetWindowText(hwnd);
                sClassname = GetClassName(hwnd);
                
                //Check if window found matches the search parameters
                if (Classname.Match(sClassname).Success && WindowText.Match(sWindowText).Success) {
                    
                    if (dotrace) {
                        RECT rect = GetWindowRect(hwnd);
                        if (true) // rect.Right - rect.left > 1500 Then
                        {
                            trace(new string(' ', level * 4) + " ... " + hwnd + "\t" + "text=" + sWindowText + "\t" + "class=" + sClassname + "\t" + showrect(rect));
                        }
                    }
                    tempFindWindowLike = hwnd;
                    foundWindowText = sWindowText;
                    Count = Count - 1;
                    if (Count == 0) {
                        return tempFindWindowLike;
                    }
                }
                
                //Get next child window
                hwnd = GetWindow(hwnd, GW_HWNDNEXT);
                
            }
            return tempFindWindowLike;
        }
        
        public static uint FindWindowAtMouse() {
            Point p = new Point();
            GetCursorPos(ref p);
            System.Console.WriteLine("point {0}", p);
            return WindowFromPoint(p.X, p.Y);
        }
        
        // find a window based on position
        public static uint FindWindowAt(uint hWndStart, int x, int y, int level) {
            uint hwnd = 0;
            
            
            //Initialize if necessary. This is only executed
            //when level = 0 and hWndStart = 0, normally
            //only on the first call to the routine.
            if (level == 0) {
                if (hWndStart == 0) {
                    hWndStart = GetDesktopWindow();
                }
            }
            
            //Get first child window
            hwnd = GetWindow(hWndStart, GW_CHILD);
            
            while (!(hwnd == 0)) {
                RECT rect = GetWindowRect(hwnd);
                if (rect.left <= x && x <= rect.right && rect.top <= y && y <= rect.bottom) {
                    //Search children by recursion
                    uint shwnd = 0;
                    shwnd = FindWindowAt(hwnd, x - rect.left, y - rect.top, level + 1);
                    if (shwnd == 0) return hwnd;
                }
                
                //Get next child window
                hwnd = GetWindow(hwnd, GW_HWNDNEXT);
                
            }
            return 0;
        }
        
        // check if we need to show a message, and if show show it
        public static bool checkMbox(bool bad, string msg) {
            bool tempcheckMbox = false;
            tempcheckMbox = bad;
            if (bad) {
                MessageBox.Show("Contraint failed:" + msg);
            }
            return tempcheckMbox;
        }
        
        // send keys of a string to a specific window, Vista safe
        //
        public static void SendKeysAVName(string proc, string win, string keys) {
            uint hwnd = 0;
            trace("> > >SendKeys " + proc + ": '" + keys + "'");
            hwnd = FindWindowLike(0, proc, win);
            if (hwnd == 0) {
                trace("no proc/win pair " + proc + " " + win + " to send " + keys);
            }
            else {
                SendKeysAV(hwnd, keys);
            }
        }
        
        // wait for file to exist
        // return 0 when it does, of code if it doesn't after wait
        
        public static int waitdone(string dir, string donefid) {
            return waitdone(dir, donefid, 100);
        }
        
        //INSTANT C# NOTE: C# does not support optional parameters. Overloaded method(s) are created above.
        //ORIGINAL LINE: Public Function waitdone(donefid As String, Optional times = 100) As Integer
        public static int waitdone(string donedir, string donefid, int times) {
            // wait till Done
            int ttry = 0;
            int rc = 0;
            int st = 0;
            int tt = 0;
            st = TimeGetTime();
            for (ttry = 1; ttry <= times; ttry++) {
                if (Directory.GetFiles(donedir, donefid).Length != 0) break;
                // trace try & " " & donefid & "  open error " & Err.Description & " " & Err.Number & "  ff=" & ffdone
                Sleep(100);
                DoEvents();
            }
            tt = TimeGetTime() - st;
            //If rc <> 0 Then
            //    trace donefid & "  Uncorrectable open error " & rc & " " & Err.Description & " millesec=" & tt
            //Else
            //    trace donefid & " wait millesecs=" & tt
            //End If
            lastWaitTime = tt;
            if (rc == 55) {
                rc = 0;
            }
            return rc;
        }
        
        public static uint FindWindowLikeAfter(uint hWndStart, string WindowTextK, string ClassnameK, string WindowTextMe, string ClassnameMe) {
            return FindWindowLikeAfter(hWndStart, new Regex(WindowTextK), new Regex(ClassnameK), new Regex(WindowTextMe), new Regex(ClassnameMe));
        }
        
        // find first window matching WindowTextME/ClassnameME AFTER window matching WindowTextK/ClassnameK
        // copied initially from http://vbnet.mvps.org/index.html?code/system/findwindowlikesimple.htm
        // NO recursion
        public static uint FindWindowLikeAfter(uint hWndStart, Regex WindowTextK, Regex ClassnameK, Regex WindowTextMe, Regex ClassnameMe) {
            
            uint hwnd = 0;
            string sWindowText = null;
            string sClassname = null;
            //#long r = 0;
            bool scanK = false;
            Regex WindowText = null;
            Regex Classname = null;
            
            //Get first child window
            hwnd = GetWindow(hWndStart, GW_CHILD);
            
            scanK = true;
            WindowText = WindowTextK;
            Classname = ClassnameK;
            
            while (!(hwnd == 0)) {
                
                //Get the window text and class name
                sWindowText = GetWindowText(hwnd);
                sClassname = GetClassName(hwnd);
                
                //Check if window found matches the search parameters
                if (WindowText.Match(sWindowText).Success && Classname.Match(sClassname).Success) {
                    
                    //'''List1.AddItem hwnd & vbTab & _
                    //'''              sClassname & vbTab & _
                    //'''              sWindowText
                    //rect rect = new rect();
                    //long rc = 0;
                    //rc = GetWindowRect(hwnd, rect);
                    
                    //trace(" ... " + hwnd + "\t" + sWindowText + "\t" + sClassname + showrect(rect));
                    if (scanK) {
                        scanK = false;
                        WindowText = WindowTextMe;
                        Classname = ClassnameMe;
                    }
                    else {
                        return hwnd;
                    }
                    
                }
                
                //Get next child window
                hwnd = GetWindow(hwnd, GW_HWNDNEXT);
                
            }
            
            //INSTANT C# NOTE: Inserted the following 'return' since all code paths must return a value in C#:
            return 0;
        }
        */
        /* pjt Unity not using Control
        // steal the main image from whereever: given an optional trim from around the window
        // keep as large as possible, preserve aspect ratio and center
        
        public static void copyImage(Control pic, uint whwnd, int tt, int tl, int tb) {
            copyImage(pic, whwnd, tt, tl, tb, 0);
        }
        
        public static void copyImage(Control pic, uint whwnd, int tt, int tl) {
            copyImage(pic, whwnd, tt, tl, 0, 0);
        }
        
        public static void copyImage(Control pic, uint whwnd, int tt) {
            copyImage(pic, whwnd, tt, 0, 0, 0);
        }
        
        public static void copyImage(Control pic, uint whwnd) {
            copyImage(pic, whwnd, 0, 0, 0, 0);
        }
        
        public static void copyImage(Control pic, uint whwnd, int tt, int tl, int tb, int tr) {
            uint tohwnd = 0;
            uint whdc = 0; // window hdc
            uint tohdc = 0; // to hdc
            //#bool rc = false;
            
            // pic.Picture = null;
            tohwnd = (uint)pic.Handle;
            if (tohwnd != 0 & whwnd != 0) {
                RECT rect = GetClientRect(whwnd);
                int t = rect.top + tt;
                int l = rect.left + tl;
                int r = rect.right - tr;
                int b = rect.bottom - tb;
                
                rect = GetClientRect(tohwnd);
                int tob = rect.bottom;
                int tot = rect.top;
                int tol = rect.left;
                int tor = rect.right;
                
                if (r == 0 || b == 0) {
                    trace("unexpected 0 size window");
                    return;
                }
                // top and right should both be 0 for both as using client, this simplifies below as well
                double str = Math.Min(tor / (double)(r - l), tob / (double)(b - t)); // stretch
                
                whdc = GetDC(whwnd);
                tohdc = GetDC(tohwnd);
                // Debug.Print "copy image " & whdc & " to " & tohdc
                //rc = StretchBlt(tohdc, 0, 0, tor, tob, whdc, 0, 0, 1, 1, SRCCOPY) ' fill with what I hope is background
                bool rrc = StretchBlt(tohdc, (int)((tor - r * str) / 2), (int)((tob - b * str) / 2), (int)(r * str), (int)(b * str), whdc, l, t, r, b, SRCCOPY);
                ReleaseDC(tohwnd, tohdc);
                ReleaseDC(whwnd, whdc);
                //pic.Picture = pic.Image;
                //pic.AutoRedraw = true;
                
                // Debug.Print (tor - tol) & " " & toc.ScaleWidth
            }
            
        }
        */
        
        public const int WM_ACTIVATE = 0x6;
        public const int WM_ACTIVATEAPP = 0x1C;
        public const int WM_ADSPROP_NOTIFY_APPLY = (WM_USER + 1104);
        public const int WM_ADSPROP_NOTIFY_CHANGE = (WM_USER + 1103);
        public const int WM_ADSPROP_NOTIFY_ERROR = (WM_USER + 1110);
        public const int WM_ADSPROP_NOTIFY_EXIT = (WM_USER + 1107);
        public const int WM_ADSPROP_NOTIFY_FOREGROUND = (WM_USER + 1106);
        public const int WM_ADSPROP_NOTIFY_PAGEHWND = (WM_USER + 1102);
        public const int WM_ADSPROP_NOTIFY_PAGEINIT = (WM_USER + 1101);
        public const int WM_ADSPROP_NOTIFY_SETFOCUS = (WM_USER + 1105);
        public const int WM_ADSPROP_NOTIFY_SHOW_ERROR_DIALOG = (WM_USER + 1111);
        public const int WM_AFXFIRST = 0x360;
        public const int WM_AFXLAST = 0x37F;
        public const int WM_APP = 0x8000;
        public const int WM_APPCOMMAND = 0x319;
        public const int WM_ASKCBFORMATNAME = 0x30C;
        public const int WM_CANCELJOURNAL = 0x4B;
        public const int WM_CANCELMODE = 0x1F;
        public const int WM_CAP_ABORT = (WM_CAP_START + 69);
        public const int WM_CAP_DLG_VIDEOCOMPRESSION = (WM_CAP_START + 46);
        public const int WM_CAP_DLG_VIDEODISPLAY = (WM_CAP_START + 43);
        public const int WM_CAP_DLG_VIDEOFORMAT = (WM_CAP_START + 41);
        public const int WM_CAP_DLG_VIDEOSOURCE = (WM_CAP_START + 42);
        public const int WM_CAP_DRIVER_CONNECT = (WM_CAP_START + 10);
        public const int WM_CAP_DRIVER_DISCONNECT = (WM_CAP_START + 11);
        public const int WM_CAP_DRIVER_GET_CAPS = (WM_CAP_START + 14);
        public const int WM_CAP_DRIVER_GET_NAMEA = (WM_CAP_START + 12);
        public const int WM_CAP_DRIVER_GET_NAMEW = (WM_CAP_UNICODE_START + 12);
        public const int WM_CAP_DRIVER_GET_VERSIONA = (WM_CAP_START + 13);
        public const int WM_CAP_DRIVER_GET_VERSIONW = (WM_CAP_UNICODE_START + 13);
        public const int WM_CAP_EDIT_COPY = (WM_CAP_START + 30);
        public const int WM_CAP_END = WM_CAP_UNICODE_END;
        public const int WM_CAP_FILE_ALLOCATE = (WM_CAP_START + 22);
        public const int WM_CAP_FILE_GET_CAPTURE_FILEA = (WM_CAP_START + 21);
        public const int WM_CAP_FILE_GET_CAPTURE_FILEW = (WM_CAP_UNICODE_START + 21);
        public const int WM_CAP_FILE_SAVEASA = (WM_CAP_START + 23);
        public const int WM_CAP_FILE_SAVEASW = (WM_CAP_UNICODE_START + 23);
        public const int WM_CAP_FILE_SAVEDIBA = (WM_CAP_START + 25);
        public const int WM_CAP_FILE_SAVEDIBW = (WM_CAP_UNICODE_START + 25);
        public const int WM_CAP_FILE_SET_CAPTURE_FILEA = (WM_CAP_START + 20);
        public const int WM_CAP_FILE_SET_CAPTURE_FILEW = (WM_CAP_UNICODE_START + 20);
        public const int WM_CAP_FILE_SET_INFOCHUNK = (WM_CAP_START + 24);
        public const int WM_CAP_GET_AUDIOFORMAT = (WM_CAP_START + 36);
        public const int WM_CAP_GET_CAPSTREAMPTR = (WM_CAP_START + 1);
        public const int WM_CAP_GET_MCI_DEVICEA = (WM_CAP_START + 67);
        public const int WM_CAP_GET_MCI_DEVICEW = (WM_CAP_UNICODE_START + 67);
        public const int WM_CAP_GET_SEQUENCE_SETUP = (WM_CAP_START + 65);
        public const int WM_CAP_GET_STATUS = (WM_CAP_START + 54);
        public const int WM_CAP_GET_USER_DATA = (WM_CAP_START + 8);
        public const int WM_CAP_GET_VIDEOFORMAT = (WM_CAP_START + 44);
        public const int WM_CAP_GRAB_FRAME = (WM_CAP_START + 60);
        public const int WM_CAP_GRAB_FRAME_NOSTOP = (WM_CAP_START + 61);
        public const int WM_CAP_PAL_AUTOCREATE = (WM_CAP_START + 83);
        public const int WM_CAP_PAL_MANUALCREATE = (WM_CAP_START + 84);
        public const int WM_CAP_PAL_OPENA = (WM_CAP_START + 80);
        public const int WM_CAP_PAL_OPENW = (WM_CAP_UNICODE_START + 80);
        public const int WM_CAP_PAL_PASTE = (WM_CAP_START + 82);
        public const int WM_CAP_PAL_SAVEA = (WM_CAP_START + 81);
        public const int WM_CAP_PAL_SAVEW = (WM_CAP_UNICODE_START + 81);
        public const int WM_CAP_SEQUENCE = (WM_CAP_START + 62);
        public const int WM_CAP_SEQUENCE_NOFILE = (WM_CAP_START + 63);
        public const int WM_CAP_SET_AUDIOFORMAT = (WM_CAP_START + 35);
        public const int WM_CAP_SET_CALLBACK_CAPCONTROL = (WM_CAP_START + 85);
        public const int WM_CAP_SET_CALLBACK_ERRORA = (WM_CAP_START + 2);
        public const int WM_CAP_SET_CALLBACK_ERRORW = (WM_CAP_UNICODE_START + 2);
        public const int WM_CAP_SET_CALLBACK_FRAME = (WM_CAP_START + 5);
        public const int WM_CAP_SET_CALLBACK_STATUSA = (WM_CAP_START + 3);
        public const int WM_CAP_SET_CALLBACK_STATUSW = (WM_CAP_UNICODE_START + 3);
        public const int WM_CAP_SET_CALLBACK_VIDEOSTREAM = (WM_CAP_START + 6);
        public const int WM_CAP_SET_CALLBACK_WAVESTREAM = (WM_CAP_START + 7);
        public const int WM_CAP_SET_CALLBACK_YIELD = (WM_CAP_START + 4);
        public const int WM_CAP_SET_MCI_DEVICEA = (WM_CAP_START + 66);
        public const int WM_CAP_SET_MCI_DEVICEW = (WM_CAP_UNICODE_START + 66);
        public const int WM_CAP_SET_OVERLAY = (WM_CAP_START + 51);
        public const int WM_CAP_SET_PREVIEW = (WM_CAP_START + 50);
        public const int WM_CAP_SET_PREVIEWRATE = (WM_CAP_START + 52);
        public const int WM_CAP_SET_SCALE = (WM_CAP_START + 53);
        public const int WM_CAP_SET_SCROLL = (WM_CAP_START + 55);
        public const int WM_CAP_SET_SEQUENCE_SETUP = (WM_CAP_START + 64);
        public const int WM_CAP_SET_USER_DATA = (WM_CAP_START + 9);
        public const int WM_CAP_SET_VIDEOFORMAT = (WM_CAP_START + 45);
        public const int WM_CAP_SINGLE_FRAME = (WM_CAP_START + 72);
        public const int WM_CAP_SINGLE_FRAME_CLOSE = (WM_CAP_START + 71);
        public const int WM_CAP_SINGLE_FRAME_OPEN = (WM_CAP_START + 70);
        public const int WM_CAP_START = WM_USER;
        public const int WM_CAP_STOP = (WM_CAP_START + 68);
        public const int WM_CAP_UNICODE_END = WM_CAP_PAL_SAVEW;
        public const int WM_CAP_UNICODE_START = WM_USER + 100;
        public const int WM_CAPTURECHANGED = 0x215;
        public const int WM_CHANGECBCHAIN = 0x30D;
        public const int WM_CHANGEUISTATE = 0x127;
        public const int WM_CHAR = 0x102;
        public const int WM_CHARTOITEM = 0x2F;
        public const int WM_CHILDACTIVATE = 0x22;
        public const int WM_CHOOSEFONT_GETLOGFONT = (WM_USER + 1);
        public const int WM_CHOOSEFONT_SETFLAGS = (WM_USER + 102);
        public const int WM_CHOOSEFONT_SETLOGFONT = (WM_USER + 101);
        public const int WM_CLEAR = 0x303;
        public const int WM_CLOSE = 0x10;
        public const int WM_COMMAND = 0x111;
        public const int WM_COMMNOTIFY = 0x44;
        public const int WM_COMPACTING = 0x41;
        public const int WM_COMPAREITEM = 0x39;
        public const int WM_CONTEXTMENU = 0x7B;
        public const int WM_CONVERTREQUEST = 0x10A;
        public const int WM_CONVERTREQUESTEX = 0x108;
        public const int WM_CONVERTRESULT = 0x10B;
        public const int WM_COPY = 0x301;
        public const int WM_COPYDATA = 0x4A;
        public const int WM_CPL_LAUNCH = (WM_USER + 1000);
        public const int WM_CPL_LAUNCHED = (WM_USER + 1001);
        public const int WM_CREATE = 0x1;
        public const int WM_CTLCOLOR = 0x19;
        public const int WM_CTLCOLORBTN = 0x135;
        public const int WM_CTLCOLORDLG = 0x136;
        public const int WM_CTLCOLOREDIT = 0x133;
        public const int WM_CTLCOLORLISTBOX = 0x134;
        public const int WM_CTLCOLORMSGBOX = 0x132;
        public const int WM_CTLCOLORSCROLLBAR = 0x137;
        public const int WM_CTLCOLORSTATIC = 0x138;
        public const int WM_CUT = 0x300;
        public const int WM_DDE_ACK = (WM_DDE_FIRST + 4);
        public const int WM_DDE_ADVISE = (WM_DDE_FIRST + 2);
        public const int WM_DDE_DATA = (WM_DDE_FIRST + 5);
        public const int WM_DDE_EXECUTE = (WM_DDE_FIRST + 8);
        public const int WM_DDE_FIRST = 0x3E0;
        public const int WM_DDE_INITIATE = (WM_DDE_FIRST);
        public const int WM_DDE_LAST = (WM_DDE_FIRST + 8);
        public const int WM_DDE_POKE = (WM_DDE_FIRST + 7);
        public const int WM_DDE_REQUEST = (WM_DDE_FIRST + 6);
        public const int WM_DDE_TERMINATE = (WM_DDE_FIRST + 1);
        public const int WM_DDE_UNADVISE = (WM_DDE_FIRST + 3);
        public const int WM_DEADCHAR = 0x103;
        public const int WM_DELETEITEM = 0x2D;
        public const int WM_DESTROY = 0x2;
        public const int WM_DESTROYCLIPBOARD = 0x307;
        public const int WM_DEVICECHANGE = 0x219;
        public const int WM_DEVMODECHANGE = 0x1B;
        public const int WM_DISPLAYCHANGE = 0x7E;
        public const int WM_DRAWCLIPBOARD = 0x308;
        public const int WM_DRAWITEM = 0x2B;
        public const int WM_DROPFILES = 0x233;
        public const int WM_ENABLE = 0xA;
        public const int WM_ENDSESSION = 0x16;
        public const int WM_ENTERIDLE = 0x121;
        public const int WM_ENTERMENULOOP = 0x211;
        public const int WM_ENTERSIZEMOVE = 0x231;
        public const int WM_ERASEBKGND = 0x14;
        public const int WM_EXITMENULOOP = 0x212;
        public const int WM_EXITSIZEMOVE = 0x232;
        public const int WM_FONTCHANGE = 0x1D;
        public const int WM_FORWARDMSG = 0x37F;
        public const int WM_GETDLGCODE = 0x87;
        public const int WM_GETFONT = 0x31;
        public const int WM_GETHOTKEY = 0x33;
        public const int WM_GETICON = 0x7F;
        public const int WM_GETMINMAXINFO = 0x24;
        public const int WM_GETOBJECT = 0x3D;
        public const int WM_GETTEXT = 0xD;
        public const int WM_GETTEXTLENGTH = 0xE;
        public const int WM_HANDHELDFIRST = 0x358;
        public const int WM_HANDHELDLAST = 0x35F;
        public const int WM_HELP = 0x53;
        public const int WM_HOTKEY = 0x312;
        public const int WM_HSCROLL = 0x114;
        public const int WM_HSCROLLCLIPBOARD = 0x30E;
        public const int WM_ICONERASEBKGND = 0x27;
        public const int WM_IME_CHAR = 0x286;
        public const int WM_IME_COMPOSITION = 0x10F;
        public const int WM_IME_COMPOSITIONFULL = 0x284;
        public const int WM_IME_CONTROL = 0x283;
        public const int WM_IME_ENDCOMPOSITION = 0x10E;
        public const int WM_IME_KEYDOWN = 0x290;
        public const int WM_IME_KEYLAST = 0x10F;
        public const int WM_IME_KEYUP = 0x291;
        public const int WM_IME_NOTIFY = 0x282;
        public const int WM_IME_REPORT = 0x280;
        public const int WM_IME_REQUEST = 0x288;
        public const int WM_IME_SELECT = 0x285;
        public const int WM_IME_SETCONTEXT = 0x281;
        public const int WM_IME_STARTCOMPOSITION = 0x10D;
        public const int WM_IMEKEYDOWN = 0x290;
        public const int WM_IMEKEYUP = 0x291;
        public const int WM_INITDIALOG = 0x110;
        public const int WM_INITMENU = 0x116;
        public const int WM_INITMENUPOPUP = 0x117;
        public const int WM_INPUTLANGCHANGE = 0x51;
        public const int WM_INPUTLANGCHANGEREQUEST = 0x50;
        public const int WM_INTERIM = 0x10C;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYFIRST = 0x100;
        public const int WM_KEYLAST = 0x108;
        public const int WM_KEYUP = 0x101;
        public const int WM_KILLFOCUS = 0x8;
        public const int WM_LBUTTONDBLCLK = 0x203;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_MBUTTONDBLCLK = 0x209;
        public const int WM_MBUTTONDOWN = 0x207;
        public const int WM_MBUTTONUP = 0x208;
        public const int WM_MDIACTIVATE = 0x222;
        public const int WM_MDICASCADE = 0x227;
        public const int WM_MDICREATE = 0x220;
        public const int WM_MDIDESTROY = 0x221;
        public const int WM_MDIGETACTIVE = 0x229;
        public const int WM_MDIICONARRANGE = 0x228;
        public const int WM_MDIMAXIMIZE = 0x225;
        public const int WM_MDINEXT = 0x224;
        public const int WM_MDIREFRESHMENU = 0x234;
        public const int WM_MDIRESTORE = 0x223;
        public const int WM_MDISETMENU = 0x230;
        public const int WM_MDITILE = 0x226;
        public const int WM_MEASUREITEM = 0x2C;
        public const int WM_MENUCHAR = 0x120;
        public const int WM_MENUCOMMAND = 0x126;
        public const int WM_MENUDRAG = 0x123;
        public const int WM_MENUGETOBJECT = 0x124;
        public const int WM_MENURBUTTONUP = 0x122;
        public const int WM_MENUSELECT = 0x11F;
        public const int WM_MOUSEACTIVATE = 0x21;
        public const int WM_MOUSEFIRST = 0x200;
        public const int WM_MOUSEHOVER = 0x2A1;
        public const int WM_MOUSELAST = 0x209;
        public const int WM_MOUSELEAVE = 0x2A3;
        public const int WM_MOUSEMOVE = 0x200;
        public const int WM_MOUSEWHEEL = 0x20A;
        public const int WM_MOVE = 0x3;
        public const int WM_MOVING = 0x216;
        public const int WM_NCACTIVATE = 0x86;
        public const int WM_NCCALCSIZE = 0x83;
        public const int WM_NCCREATE = 0x81;
        public const int WM_NCDESTROY = 0x82;
        public const int WM_NCHITTEST = 0x84;
        public const int WM_NCLBUTTONDBLCLK = 0xA3;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int WM_NCLBUTTONUP = 0xA2;
        public const int WM_NCMBUTTONDBLCLK = 0xA9;
        public const int WM_NCMBUTTONDOWN = 0xA7;
        public const int WM_NCMBUTTONUP = 0xA8;
        public const int WM_NCMOUSEHOVER = 0x2A0;
        public const int WM_NCMOUSELEAVE = 0x2A2;
        public const int WM_NCMOUSEMOVE = 0xA0;
        public const int WM_NCPAINT = 0x85;
        public const int WM_NCRBUTTONDBLCLK = 0xA6;
        public const int WM_NCRBUTTONDOWN = 0xA4;
        public const int WM_NCRBUTTONUP = 0xA5;
        public const int WM_NCXBUTTONDBLCLK = 0xAD;
        public const int WM_NCXBUTTONDOWN = 0xAB;
        public const int WM_NCXBUTTONUP = 0xAC;
        public const int WM_NEXTDLGCTL = 0x28;
        public const int WM_NEXTMENU = 0x213;
        public const int WM_NOTIFY = 0x4E;
        public const int WM_NOTIFYFORMAT = 0x55;
        public const int WM_NULL = 0x0;
        public const int WM_OTHERWINDOWCREATED = 0x42;
        public const int WM_OTHERWINDOWDESTROYED = 0x43;
        public const int WM_PAINT = 0xF;
        public const int WM_PAINTCLIPBOARD = 0x309;
        public const int WM_PAINTICON = 0x26;
        public const int WM_PALETTECHANGED = 0x311;
        public const int WM_PALETTEISCHANGING = 0x310;
        public const int WM_PARENTNOTIFY = 0x210;
        public const int WM_PASTE = 0x302;
        public const int WM_PENWINFIRST = 0x380;
        public const int WM_PENWINLAST = 0x38F;
        public const int WM_POWER = 0x48;
        public const int WM_POWERBROADCAST = 0x218;
        public const int WM_PRINT = 0x317;
        public const int WM_PRINTCLIENT = 0x318;
        public const int WM_PSD_ENVSTAMPRECT = (WM_USER + 5);
        public const int WM_PSD_FULLPAGERECT = (WM_USER + 1);
        public const int WM_PSD_GREEKTEXTRECT = (WM_USER + 4);
        public const int WM_PSD_MARGINRECT = (WM_USER + 3);
        public const int WM_PSD_MINMARGINRECT = (WM_USER + 2);
        public const int WM_PSD_PAGESETUPDLG = (WM_USER);
        public const int WM_PSD_YAFULLPAGERECT = (WM_USER + 6);
        public const int WM_QUERYDRAGICON = 0x37;
        public const int WM_QUERYENDSESSION = 0x11;
        public const int WM_QUERYNEWPALETTE = 0x30F;
        public const int WM_QUERYOPEN = 0x13;
        public const int WM_QUERYUISTATE = 0x129;
        public const int WM_QUEUESYNC = 0x23;
        public const int WM_QUIT = 0x12;
        public const int WM_RASDIALEVENT = 0xCCCD;
        public const int WM_RBUTTONDBLCLK = 0x206;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_RENDERALLFORMATS = 0x306;
        public const int WM_RENDERFORMAT = 0x305;
        public const int WM_SETCURSOR = 0x20;
        public const int WM_SETFOCUS = 0x7;
        public const int WM_SETFONT = 0x30;
        public const int WM_SETHOTKEY = 0x32;
        public const int WM_SETICON = 0x80;
        public const int WM_SETREDRAW = 0xB;
        public const int WM_SETTEXT = 0xC;
        public const int WM_SETTINGCHANGE = WM_WININICHANGE;
        public const int WM_SHOWWINDOW = 0x18;
        public const int WM_SIZE = 0x5;
        public const int WM_SIZECLIPBOARD = 0x30B;
        public const int WM_SIZING = 0x214;
        public const int WM_SPOOLERSTATUS = 0x2A;
        public const int WM_STYLECHANGED = 0x7D;
        public const int WM_STYLECHANGING = 0x7C;
        public const int WM_SYNCPAINT = 0x88;
        public const int WM_SYSCHAR = 0x106;
        public const int WM_SYSCOLORCHANGE = 0x15;
        public const int WM_SYSCOMMAND = 0x112;
        public const int WM_SYSDEADCHAR = 0x107;
        public const int WM_SYSKEYDOWN = 0x104;
        public const int WM_SYSKEYUP = 0x105;
        public const int WM_TCARD = 0x52;
        public const int WM_TIMECHANGE = 0x1E;
        public const int WM_TIMER = 0x113;
        public const int WM_UNDO = 0x304;
        public const int WM_UNINITMENUPOPUP = 0x125;
        public const int WM_UPDATEUISTATE = 0x128;
        public const int WM_USER = 0x400;
        public const int WM_USERCHANGED = 0x54;
        public const int WM_VKEYTOITEM = 0x2E;
        public const int WM_VSCROLL = 0x115;
        public const int WM_VSCROLLCLIPBOARD = 0x30A;
        public const int WM_WINDOWPOSCHANGED = 0x47;
        public const int WM_WINDOWPOSCHANGING = 0x46;
        public const int WM_WININICHANGE = 0x1A;
        public const int WM_WNT_CONVERTREQUESTEX = 0x109;
        public const int WM_XBUTTONDBLCLK = 0x20D;
        public const int WM_XBUTTONDOWN = 0x20B;
        public const int WM_XBUTTONUP = 0x20C;
        
        
        /* cmd for HSHELL_APPCOMMAND and WM_APPCOMMAND */
        public const int APPCOMMAND_BROWSER_BACKWARD = 1;
        public const int APPCOMMAND_BROWSER_FORWARD = 2;
        public const int APPCOMMAND_BROWSER_REFRESH = 3;
        public const int APPCOMMAND_BROWSER_STOP = 4;
        public const int APPCOMMAND_BROWSER_SEARCH = 5;
        public const int APPCOMMAND_BROWSER_FAVORITES = 6;
        public const int APPCOMMAND_BROWSER_HOME = 7;
        public const int APPCOMMAND_VOLUME_MUTE = 8;
        public const int APPCOMMAND_VOLUME_DOWN = 9;
        public const int APPCOMMAND_VOLUME_UP = 10;
        public const int APPCOMMAND_MEDIA_NEXTTRACK = 11;
        public const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 12;
        public const int APPCOMMAND_MEDIA_STOP = 13;
        public const int APPCOMMAND_MEDIA_PLAY_PAUSE = 14;
        public const int APPCOMMAND_LAUNCH_MAIL = 15;
        public const int APPCOMMAND_LAUNCH_MEDIA_SELECT = 16;
        public const int APPCOMMAND_LAUNCH_APP1 = 17;
        public const int APPCOMMAND_LAUNCH_APP2 = 18;
        public const int APPCOMMAND_BASS_DOWN = 19;
        public const int APPCOMMAND_BASS_BOOST = 20;
        public const int APPCOMMAND_BASS_UP = 21;
        public const int APPCOMMAND_TREBLE_DOWN = 22;
        public const int APPCOMMAND_TREBLE_UP = 23;
        
        
    }
}


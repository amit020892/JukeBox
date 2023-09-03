using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;


namespace JukeBoxSolutions.AudioSources
{
    public class SpotifyControls
    {
        private IntPtr _spotifyHwnd;
        public const string SpotifyPausedWindowTitle = "Spotify";
        private const string SpotifyWindowClassName = "Chrome_WidgetWin_0"; // Last checked spotify version 1.0.88.353

        public string GetSpotifyWindowTitle()
        {
            // find spotify window by class name since the title changes depending on if a song is playing
            var hwnd = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, SpotifyWindowClassName, null);
            if (hwnd == IntPtr.Zero)
                return null;

            // We are looking for the topmost window with the classname
            // We keep looking upwards by calling findwindow again and setting the child as the previous value
            // Those other windows will have an empty window title
            while (GetTitle(hwnd).Length == 0)
            {
                // At the top, this call should return null
                var nextHwnd = NativeMethods.FindWindowEx(IntPtr.Zero, hwnd, SpotifyWindowClassName, null);
                if (nextHwnd == IntPtr.Zero)
                    break;
                hwnd = nextHwnd;
            }

            _spotifyHwnd = hwnd;
            return hwnd == IntPtr.Zero ? null : GetTitle(hwnd);
        }

        public void Play()
        {
            if (_spotifyHwnd == IntPtr.Zero)
            {
                return;
            }

            NativeMethods.SendMessage(_spotifyHwnd, NativeMethods.WM_APPCOMMAND, 0, new IntPtr((int)NativeMethods.AppCommand.Play));
        }

        public void Pause()
        {
            if (_spotifyHwnd == IntPtr.Zero)
            {
                return;
            }

            NativeMethods.SendMessage(_spotifyHwnd, NativeMethods.WM_APPCOMMAND, 0, new IntPtr((int)NativeMethods.AppCommand.Pause));
        }

        public void Previous()
        {
            if (_spotifyHwnd == IntPtr.Zero)
            {
                return;
            }

            NativeMethods.SendMessage(_spotifyHwnd, NativeMethods.WM_APPCOMMAND, 0, new IntPtr((int)NativeMethods.AppCommand.Previous));
        }

        public void Next()
        {
            if (_spotifyHwnd == IntPtr.Zero)
            {
                return;
            }

            NativeMethods.SendMessage(_spotifyHwnd, NativeMethods.WM_APPCOMMAND, 0, new IntPtr((int)NativeMethods.AppCommand.Next));
        }

        public bool IsPaused()
        {
            return GetSpotifyWindowTitle() == SpotifyPausedWindowTitle;
        }

        private static string GetTitle(IntPtr hwnd)
        {
            var titleLength = NativeMethods.GetWindowTextLength(hwnd);
            var title = new StringBuilder(titleLength + 1);
            NativeMethods.GetWindowText(hwnd, title, title.Capacity);

            return title.ToString();
        }
    }

    public static class NativeMethods
    {
        public const int WM_APPCOMMAND = 0x0319;

        public enum AppCommand
        {
            Pause = 47 * 65536,
            Play = 46 * 65536,
            Next = 11 * 65536,
            Previous = 12 * 65536,
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr childAfter, string className, string windowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
    }
}

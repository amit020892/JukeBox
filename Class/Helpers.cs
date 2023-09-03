using JukeBoxSolutions.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace JukeBoxSolutions
{
    class Helpers
    {

        public void AddNextPlaylist(JukeboxBrainsDBEntities db, int TrackId, int PlaylistDetailId)
        {

        }

        internal static char[] bracketsOpen = { '(', '[', '{' };
        internal static char[] bracketsClose = { ')', ']', '}' };
        public static string StringRemoveBrackets(string text)
        {
            for (int i = 0; i < bracketsOpen.Count(); i++)
            {
                if (text.Contains(bracketsOpen[i]))
                {
                    int iStart = text.IndexOf(bracketsOpen[i]);
                    int iEnd = text.LastIndexOf(bracketsClose[i]);
                    //middle = text.Substring(iStart, iEnd - iStart + 1);
                    return text.Remove(iStart, iEnd - iStart + 1).Trim();
                }
            }
            return text;
        }
    }

    internal static class UIHelpers
    {

        static List<ButtonQueIndex> buttonQue = new List<ButtonQueIndex>();
        internal class ButtonQueIndex
        {
            internal int index { get; set; }
            internal bool isToggleButton { get { return ToggleButton != null; } }
            internal ToggleButton ToggleButton { get; set; }
            internal Button Button { get; set; }
        }
        internal static int SetButtonBusy(ref ToggleButton toggleButton, object partentPage = null)
        {
            toggleButton.IsEnabled = false;
            if (partentPage != null)
                switch (partentPage.GetType().Name)
                {
                    case "FileManagerV3": ((FileManagerV3)partentPage).isPleaseWait = true; break;
                }

            int i = buttonQue.Count();
            buttonQue.Add(new ButtonQueIndex() { index = i, ToggleButton = toggleButton });

            return i;
        }

        internal static void ReleaseButtonBusy(int ButtonQueIndex, object partentPage = null)
        {
            var b = buttonQue.Find(f => f.index == ButtonQueIndex);
            if (b.isToggleButton)
                b.ToggleButton.IsEnabled = true;
            else
                b.Button.IsEnabled = true;
        }
        internal static void ReleaseButtonBusy(ref ToggleButton toggleButton, object partentPage = null)
        {
            toggleButton.IsEnabled = false;
            if (partentPage != null)
                switch (partentPage.GetType().Name)
                {
                    case "FileManagerV3": ((FileManagerV3)partentPage).isPleaseWait = true; break;
                }
        }
    }


}

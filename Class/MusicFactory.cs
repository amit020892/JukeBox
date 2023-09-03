using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JukeBoxSolutions
{
    class MusicFactory
    {
        internal static string[] commonDiliminators = { "-" };
        public void ExtractArtist(string filename)
        {
            foreach(string d in commonDiliminators)
            {
                if(filename.Contains(d))
                {
                    // extract pre-delim
                    string testartist = filename.Substring(0, filename.IndexOf(d)).Trim();

                }
            }
        }
    }

    class UltimateMusicFile
    {
    }
}

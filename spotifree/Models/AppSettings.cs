using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotifree.Models
{
    public class AppSettings
    {
        public List<string> MusicFolderPaths { get; set; } = new();
        public bool IsDarkTheme { get; set; }
        public bool HasCompletedFirstRunTour { get; set; } = false;
    }
}

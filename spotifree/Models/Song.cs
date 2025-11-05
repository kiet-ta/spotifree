using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace spotifree.Models
{
    public class Song : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private string _artist;
        public string Artist
        {
            get => _artist;
            set { _artist = value; OnPropertyChanged(); }
        }

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(); }
        }

        // Đường dẫn đến ảnh bìa (có thể là URL web hoặc một file local khác)
        private string _coverArtUrl;
        public string CoverArtUrl
        {
            get => _coverArtUrl;
            set { _coverArtUrl = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

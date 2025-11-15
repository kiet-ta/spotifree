using Spotifree.ViewModels;
using System.Windows.Media.Imaging;

namespace Spotifree.Models;

public class LocalTrack : BaseViewModel
{
    private string _title = string.Empty;
    private string _artist = string.Empty;
    private string _album = string.Empty;
    private string _filePath = string.Empty;
    private int _duration;
    private BitmapImage? _coverArt;

    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Artist
    {
        get => _artist;
        set => SetProperty(ref _artist, value);
    }

    public string Album
    {
        get => _album;
        set => SetProperty(ref _album, value);
    }

    public int Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }

    public BitmapImage? CoverArt
    {
        get => _coverArt;
        set => SetProperty(ref _coverArt, value);
    }
}

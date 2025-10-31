namespace spotifree.Models
{
    public class AppSettings
    {
        public string Language { get; set; } = "en-US";
        public bool Autoplay { get; set; } = false;
        public int ZoomPercent { get; set; } = 100; // 70..130
        public string OfflineStoragePath { get; set; } =
            System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Spotifree", "Storage");
    }
}

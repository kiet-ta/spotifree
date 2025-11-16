using System.Windows;

namespace Spotifree.Models
{
    public class TourStep
    {
        public string Text { get; set; } = string.Empty;

        public HorizontalAlignment PopupAlignmentH { get; set; } = HorizontalAlignment.Center;
        public VerticalAlignment PopupAlignmentV { get; set; } = VerticalAlignment.Center;
        public Thickness PopupMargin { get; set; } = new Thickness(0);

        public bool ShowHighlight { get; set; } = false; 
        public HorizontalAlignment HighlightAlignmentH { get; set; } = HorizontalAlignment.Left;
        public VerticalAlignment HighlightAlignmentV { get; set; } = VerticalAlignment.Top;
        public Thickness HighlightMargin { get; set; } = new Thickness(0);
        public double HighlightWidth { get; set; } = 100; 
        public double HighlightHeight { get; set; } = 50;  
    }
}
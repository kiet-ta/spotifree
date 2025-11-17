using Spotifree.ViewModels; // Cần using BaseViewModel của bạn

namespace Spotifree.ViewModels
{
    public class FrequencyBandViewModel : BaseViewModel
    {
        private double _level;

        public double Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }
    }
}
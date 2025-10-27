using System.Windows;
using System.Windows.Input; // Cần cho MouseButtonEventArgs
using spotifree.ViewModels; // Cần cho ViewModel

namespace spotifree
{
    public partial class MusicDetail : Window
    {
        // Sửa hàm khởi tạo để NHẬN ViewModel (Dependency Injection)
        public MusicDetail(MusicDetailViewModel viewModel)
        {
            InitializeComponent();

            // Đây là bước kết nối View với ViewModel
            DataContext = viewModel;
        }

        // XÓA HẾT các hàm BtnMaximize_Click, BtnMinimize_Click, BtnClose_Click

        // Thêm hàm này để kéo thả cửa sổ
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
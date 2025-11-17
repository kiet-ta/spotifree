using System.Windows;

namespace Spotifree.Views
{
    public partial class SimpleTextDialog : Window
    {
        public string? Value => InputBox.Text;

        public SimpleTextDialog(string message, string defaultValue)
        {
            InitializeComponent();
            MessageText.Text = message;
            InputBox.Text = defaultValue;
            InputBox.SelectAll();
            Loaded += (_, __) => InputBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}

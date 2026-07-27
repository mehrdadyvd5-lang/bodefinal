using System.Windows;

namespace BodyComposition.App.Views
{
    public partial class BluetoothConnectWindow : Window
    {
        public BluetoothConnectWindow()
        {
            InitializeComponent();
            FlowDirection = App.Localization.FlowDirection;
            MissingText.Visibility = Visibility.Visible;
            DevicesList.Items.Clear();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            // TODO: replace with a real Bluetooth LE device scan once the
            // analyzer hardware + dongle are available for testing.
            DevicesList.Items.Clear();
            MissingText.Visibility = Visibility.Visible;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            MissingText.Visibility = Visibility.Visible;
        }

        private void Stop_Click(object sender, RoutedEventArgs e) { }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}

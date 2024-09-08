using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace AIChat_Project
{
    public partial class SettingsWindow : Window
    {
        public string ServerUrl { get; private set; }
        private static readonly HttpClient client = new HttpClient();

        public SettingsWindow()
        {
            InitializeComponent();
        }

        // 서버 주소 저장
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ServerUrl = UrlTextBox.Text;
            if (!string.IsNullOrWhiteSpace(ServerUrl))
            {
                LoadingProgressBar.Visibility = Visibility.Visible;

                if (await TestServerConnection(ServerUrl))
                {
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Unable to connect to the server. Please check the URL.");
                }

                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Please enter a valid URL.");
            }
        }

        // 서버 연결 확인
        private async Task<bool> TestServerConnection(string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}

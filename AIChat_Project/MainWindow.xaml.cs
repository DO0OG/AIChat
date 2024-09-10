using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AIChat_Project
{
    public partial class MainWindow : Window
    {
        private static HttpClient client;
        private string serverUrl;

        public MainWindow()
        {
            InitializeComponent();
            ShowSettingsWindow();
        }

        private void ShowSettingsWindow()
        {
            var settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                serverUrl = settingsWindow.ServerUrl;
                client = new HttpClient();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        // ChatMessage 클래스 생성
        public class ChatMessage
        {
            public string Message { get; set; }
            public int Column { get; set; }
            public HorizontalAlignment Alignment { get; set; }
            public Brush BackgroundColor { get; set; }
            public ContextMenu ContextMenu { get; set; }
        }

        // 사용자 메시지 추가
        private void AddUserMessage(string message)
        {
            ChatHistory.Items.Add(new ChatMessage
            {
                Message = message,
                Column = 2,
                Alignment = HorizontalAlignment.Right,
                BackgroundColor = Brushes.LightBlue
            });
            ScrollToEnd(); // 새로운 메시지 추가 후 스크롤
        }

        // AI 메시지 추가
        private void AddAIMessage(string message)
        {
            ChatHistory.Items.Add(new ChatMessage
            {
                Message = message,
                Column = 0,
                Alignment = HorizontalAlignment.Left,
                BackgroundColor = Brushes.LightGray
            });
            ScrollToEnd(); // 새로운 메시지 추가 후 스크롤
        }

        // 사용자 입력을 서버에 보내고 응답을 처리하는 함수
        private async void SendMessage(string message)
        {
            try
            {
                AddUserMessage(message);
                UserInput.Clear();

                AddAIMessage("답변 생성중...");

                var response = await SendToLlamaServer(message);

                // "답변 생성중..." 메시지를 제거하고 실제 응답 추가
                ChatHistory.Items.RemoveAt(ChatHistory.Items.Count - 1);
                AddAIMessage(response);
            }
            catch (Exception ex)
            {
                AddAIMessage($"Error: {ex.Message}");
            }
        }

        // 서버로 메시지 전송
        private async Task<string> SendToLlamaServer(string message)
        {
            var contentJson = new
            {
                prompt = $"user: {message}\nAI: ",
                n_predict = 300,
                temperature = 0.7,
                repeat_penalty = 1.3,
                repeat_last_n = 256,
                penalize_nl = true,
                top_k = 50,
                top_p = 0.9,
                min_p = 0.05,
                stop = new[] { "user", "\n\n" }
            };

            var jsonContent = JsonConvert.SerializeObject(contentJson);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var fullUrl = $"{serverUrl.TrimEnd('/')}/completion";

            try
            {
                var response = await client.PostAsync(fullUrl, content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<dynamic>(responseBody);
                return responseJson?.content?.ToString() ?? "No response content found.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Enter 키로 메시지 전송
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var message = UserInput.Text;
                if (!string.IsNullOrWhiteSpace(message))
                {
                    SendMessage(message);
                }
            }
        }

        // 전송 버튼 클릭 시 메시지 전송
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var message = UserInput.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                SendMessage(message);
            }
        }

        // 우클릭 시 채팅 복사
        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is ChatMessage chatMessage)
            {
                Clipboard.SetText(chatMessage.Message);
            }
        }

        // 마지막 항목으로 스크롤
        private void ScrollToEnd()
        {
            ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
        }
    }
}

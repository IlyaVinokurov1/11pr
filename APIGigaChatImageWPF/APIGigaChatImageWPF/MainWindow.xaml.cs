using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace APIGigaChatImageWPF
{
    public partial class MainWindow : Window
    {
        private const string CLIENT_ID = "019b287d-4c6f-7695-97bd-095b75ac26a5";
        private const string AUTH_KEY = "MDE5YjI4N2QtNGM2Zi03Njk1LTk3YmQtMDk1Yjc1YWMyNmE1OmJkMjI4NGU2LWFlYzctNDg0Ny1hM2FkLTg0NGViZjY2NzFlNA==";

        private string currentToken;
        private string currentImagePath;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeApplication();
        }

        private async Task InitializeApplication()
        {
            currentToken = await GetAccessToken();
        }

        private async Task<string> GetAccessToken()
        {
            try
            {
                string url = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";

                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback =
                        (sender, cert, chain, sslPolicyErrors) => true;

                    using (HttpClient client = new HttpClient(handler))
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, url);
                        request.Headers.Add("Accept", "application/json");
                        request.Headers.Add("RqUID", CLIENT_ID);
                        request.Headers.Add("Authorization", $"Bearer {AUTH_KEY}");

                        var content = new Dictionary<string, string>
                        {
                            { "scope", "GIGACHAT_API_PERS" }
                        };

                        request.Content = new FormUrlEncodedContent(content);
                        client.Timeout = TimeSpan.FromSeconds(30);

                        var response = await client.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            var data = JsonConvert.DeserializeObject<TokenData>(json);
                            return data.access_token;
                        }
                        else
                        {
                            string error = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Ошибка API: {response.StatusCode}\n{error}");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения токена: {ex.Message}");
                return null;
            }
        }

        private class TokenData
        {
            public string access_token { get; set; }
            public string expires_at { get; set; }
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            await GenerateImage();
        }

        private void HolidayButton_Click(object sender, RoutedEventArgs e)
        {
            // Просто вставляем праздничный промпт в TextBox
            DescriptionTextBox.Text = GetHolidayPrompt();
        }

        private async Task GenerateImage()
        {
            if (currentToken == null)
            {
                MessageBox.Show("Токен не получен. Перезапустите приложение.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ||
                DescriptionTextBox.Text == "Введите описание для генерации")
            {
                MessageBox.Show("Введите описание изображения",
                                "Внимание",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            string prompt = DescriptionTextBox.Text;
            string style = GetSelectedStyle();
            string palette = GetSelectedPalette();
            string aspect = GetSelectedAspect();
            string fullPrompt = BuildPromptWithParameters(prompt, style, palette, aspect);

            try
            {
                string imagePath = await CreateImage(fullPrompt);

                if (imagePath != null)
                {
                    currentImagePath = imagePath;
                    ShowImagePreview(imagePath);
                    SetWallpaperButton.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Не удалось создать изображение. Попробуйте другой запрос.",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                                "Исключение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private string GetSelectedStyle()
        {
            return (StyleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Реалистичный";
        }

        private string GetSelectedPalette()
        {
            return (PaletteComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Теплые тона";
        }

        private string GetSelectedAspect()
        {
            return (AspectComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "16:9";
        }

        private string BuildPromptWithParameters(string basePrompt, string style, string palette, string aspect)
        {
            string styleText = style.ToLower();
            string paletteText = palette.ToLower();
            string aspectText = GetAspectDescription(aspect);

            return $"{basePrompt}, стиль: {styleText}, палитра: {paletteText}, {aspectText}, подходит для обоев рабочего стола";
        }

        private string GetAspectDescription(string aspect)
        {
            if (aspect.Contains("16:9")) return "широкоформатное изображение 16:9";
            if (aspect.Contains("4:3")) return "стандартное соотношение 4:3";
            return "широкоформатное изображение";
        }

        private async Task<string> CreateImage(string prompt)
        {
            try
            {
                string apiUrl = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";

                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback =
                        (sender, cert, chain, sslPolicyErrors) => true;

                    using (HttpClient client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {currentToken}");
                        client.DefaultRequestHeaders.Add("X-Client-ID", CLIENT_ID);
                        client.Timeout = TimeSpan.FromSeconds(120);

                        var requestData = new
                        {
                            model = "GigaChat",
                            messages = new[]
                            {
                        new { role = "user", content = prompt }
                    },
                            function_call = "auto"
                        };

                        string json = JsonConvert.SerializeObject(requestData);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync(apiUrl, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            string errorContent = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Ошибка API: {response.StatusCode}\n{errorContent}");
                            return null;
                        }

                        string responseJson = await response.Content.ReadAsStringAsync();
                        var data = JObject.Parse(responseJson);
                        string htmlContent = data["choices"]?[0]?["message"]?["content"]?.ToString();

                        if (string.IsNullOrEmpty(htmlContent))
                        {
                            Console.WriteLine("Пустой ответ от нейросети");
                            return null;
                        }

                        var match = Regex.Match(htmlContent, @"src=""([^""]+)""");

                        if (!match.Success)
                        {
                            match = Regex.Match(htmlContent, @"<img[^>]+src=[""']([^""']+)[""']");

                            if (!match.Success)
                            {
                                Console.WriteLine($"Не найдено изображение в ответе: {htmlContent.Substring(0, Math.Min(200, htmlContent.Length))}");
                                return null;
                            }
                        }

                        string imageId = match.Groups[1].Value;
                        string imageUrl = $"https://gigachat.devices.sberbank.ru/api/v1/files/{imageId}/content";
                        var imageResponse = await client.GetAsync(imageUrl);

                        if (!imageResponse.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Ошибка загрузки изображения: {imageResponse.StatusCode}");
                            return null;
                        }

                        byte[] imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
                        string wallpaperFolder = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                            "GigaChat Wallpapers");

                        if (!Directory.Exists(wallpaperFolder))
                        {
                            Directory.CreateDirectory(wallpaperFolder);
                        }

                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string fileName = $"wallpaper_{timestamp}.jpg";
                        string filePath = Path.Combine(wallpaperFolder, fileName);

                        await File.WriteAllBytesAsync(filePath, imageBytes);
                        Console.WriteLine($"Изображение сохранено: {filePath}");
                        return filePath;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show("Превышено время ожидания. Попробуйте упростить запрос.",
                              "Таймаут",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания изображения: {ex.Message}");
                return null;
            }
        }

        private void ShowImagePreview(string imagePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imagePath);
                bitmap.EndInit();

                PreviewImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки предпросмотра: {ex.Message}");
            }
        }

        private string GetHolidayPrompt()
        {
            DateTime today = DateTime.Today;

            if (today.Month == 12 && today.Day >= 20 && today.Day <= 31)
                return "зима перед новым годом";
            if (today.Month == 1 && today.Day == 1)
                return "новый год праздник веселье";
            if (today.Month == 2 && today.Day == 23)
                return "23 февраля защитник отечества военная тема флаг";

            return "красивые обои для рабочего стола";
        }

        private void SetWallpaperButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentImagePath) && File.Exists(currentImagePath))
            {
                try
                {
                    WallpaperHelper.SetAsWallpaper(currentImagePath);
                    MessageBox.Show("Изображение успешно установлено как фон рабочего стола",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка установки обоев: {ex.Message}",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Сначала создайте изображение",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private static class WallpaperHelper
        {
            private const int SPI_SETDESKWALLPAPER = 20;
            private const int SPIF_UPDATEINIFILE = 0x01;
            private const int SPIF_SENDWININICHANGE = 0x02;

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

            public static void SetAsWallpaper(string imagePath)
            {
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
        }
    }
}
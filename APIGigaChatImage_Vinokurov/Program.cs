using APIGigaChatImage_Vinokurov.Classes;
using APIGigaChatImage_Vinokurov.Models;
using APIGigaChatImage_Vinokurov.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace APIGigaChatImage_Vinokurov
{

    internal class Program
    {
       
        static string ClientId = "019b038f-be32-7728-9ba8-86b03afb5efc";
       
        static string AuthorizationKey = "MDE5YjAzOGYtYmUzMi03NzI4LTliYTgtODZiMDNhZmI1ZWZjOjdlZTk5Y2MzLTFlMGEtNGFjMC1hMjI0LWMxY2ZiZjY1ZmNiMA==";

        static async Task Main(string[] args)
        {
            // Переменная для хранения токена
            string Token = await GetToken(ClientId, AuthorizationKey);

            Token = "eyJjdHkiOiJqd3QiLCJlbmMiOiJBMjU2Q0JDLUhTNTEyIiwiYWxnIjoiUlNBLU9BRVAifQ.q68pHkavp7mt0Gqr";

            Console.WriteLine($"Токен получен: {Token}");

            // Здесь должен быть вызов методов для генерации и скачивания изображения
            // ...

            // Тестирование установки обоев (пример из задания)
            string imagePath = @"C:\Users\aooschepkov\Pictures\CHumok 3kpaha 2625-11-01 152414.png";
            WallpaperSetter.SetWallpaper(imagePath);
        }

        /// <summary>
        /// Метод получения токена пользователя
        /// </summary>
        /// <param name="RqUID">Клиент ID</param>
        /// <param name="Bearer">Ключ авторизации</param>
        /// <returns>Токен для выполнения запросов</returns>
        public static async Task<string> GetToken(string RqUID, string Bearer)
        {
            string ReturnToken = null; // Переменная для хранения полученного токена
            string Url = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth"; // URL endpoint для получения токена

            // Создаем обработчик HTTP-клиента с настройками SSL
            using (HttpClientHandler Handler = new HttpClientHandler())
            {
                // Отключаем проверку SSL-сертификатов
                Handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

                // Создаем HTTP-клиент с использованием кастомного обработчика
                using (HttpClient Client = new HttpClient(Handler))
                {
                    // Создаем POST-запрос к указанному URL
                    HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, Url);

                    // Добавляем заголовки запроса
                    Request.Headers.Add("Accept", "application/json"); // Ожидаем JSON в ответе
                    Request.Headers.Add("RqUID", RqUID); // Уникальный идентификатор запроса
                    Request.Headers.Add("Authorization", $"Bearer {Bearer}"); // Токен авторизации

                    // Подготавливаем данные для формы (application/x-www-form-urlencoded)
                    var Data = new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS") // Запрашиваемые разрешения
                    };

                    // Создаем содержимое запроса в формате form-urlencoded
                    Request.Content = new FormUrlEncodedContent(Data);

                    // Отправляем асинхронный запрос и получаем ответ
                    HttpResponseMessage Response = await Client.SendAsync(Request);

                    // Проверяем успешность HTTP-запроса
                    if (Response.IsSuccessStatusCode)
                    {
                        // Читаем содержимое ответа как строку
                        string ResponseContent = await Response.Content.ReadAsStringAsync();

                        // Десериализуем JSON-ответ в объект ResponseToken
                        ResponseToken Token = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseToken>(ResponseContent);

                        // Извлекаем access_token из объекта ответа
                        ReturnToken = Token.access_token;
                    }
                }
            }

            // Возвращаем полученный токен (или null, если запрос не удался)
            return ReturnToken;
        }
    }
}
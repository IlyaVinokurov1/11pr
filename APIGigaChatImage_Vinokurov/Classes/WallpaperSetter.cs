using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace APIGigaChatImage_Vinokurov.Classes
{
    public class WallpaperSetter
    {
        // Константы для функции SystemParametersInfo из WinAPI

        // SPI_SETDESKWALLPAPER - код операции для установки обоев рабочего стола
        // Значение 0x0014 соответствует установке фонового изображения
        private const int SPI_SETDESKWALLPAPER = 0x0014;

        // SPIF_UPDATEINIFILE - флаг, указывающий на необходимость обновления файла конфигурации
        // Изменения будут сохранены в системных настройках
        private const int SPIF_UPDATEINIFILE = 0x01;

        // SPIF_SENDWININICHANGE - флаг, отправляющий уведомление об изменении настроек
        // Это заставляет Windows перерисовать рабочий стол и обновить все окна
        private const int SPIF_SENDWININICHANGE = 0x02;

        // Импорт функции SystemParametersInfo из библиотеки user32.dll
        // Эта функция является частью Windows API и позволяет изменять системные параметры
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(
            int uAction,    // Код операции (например, SPI_SETDESKWALLPAPER)
            int uParam,    // Дополнительный параметр (для установки обоев обычно 0)
            string lpvParam,    // Путь к файлу обоев
            int fuWinIni    // Комбинация флагов (SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE)
        );

        // Публичный метод для установки обоев рабочего стола
        // imagePath - полный путь к файлу изображения
        public static void SetWallpaper(string imagePath)
        {
            try
            {
                // Вызов WinAPI функции для установки обоев
                SystemParametersInfo(
                    SPI_SETDESKWALLPAPER, // Код операции: установить обои
                    0,    // Второй параметр не используется (должен быть 0)
                    imagePath,    // Путь к файлу обоев
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE // Флаги: сохранить настройки и уведомить систему
                );
                // Вывод сообщения об успешной установке
                Console.WriteLine($"Обои установлены: {imagePath}");
            }
            catch (Exception ex)
            {
                // Обработка возможных исключений:
                // 1. Файл не найден
                // 2. Нет прав доступа
                // 3. Неподдерживаемый формат изображения
                // 4. Ошибка вызова WinAPI функции
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
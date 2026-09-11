using System.Security.Cryptography;
using System.Text;

namespace MarketplaceManagerWeb.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            // Проверяем, что пароль не null и не пустой
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                // Преобразуем пароль в байты используя UTF-8
                byte[] bytes = Encoding.UTF8.GetBytes(password);

                // Вычисляем хеш
                byte[] hash = sha256.ComputeHash(bytes);

                // Преобразуем хеш в Base64 строку
                string result = Convert.ToBase64String(hash);

                // ОТЛАДКА: выводим результат в консоль
                System.Diagnostics.Debug.WriteLine($"[PasswordHelper] Хеш для '{password}': {result}");
                System.Diagnostics.Debug.WriteLine($"[PasswordHelper] Длина хеша: {result.Length}");

                return result;
            }
        }
    }
}
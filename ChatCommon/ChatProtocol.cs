using System.Linq;

namespace ChatCommon
{
    /// <summary>Sunucu ve istemcinin ortak kullandığı sabitler ve doğrulama kuralları.</summary>
    public static class ChatProtocol
    {
        public const int DefaultPort = 5050;
        public const int MaxUserNameLength = 20;
        public const int MaxMessageLength = 1000;

        /// <summary>Kullanıcı adı geçerliyse null, değilse hata açıklamasını döner.</summary>
        public static string ValidateUserName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Kullanıcı adı boş olamaz.";
            if (name.Length > MaxUserNameLength)
                return "Kullanıcı adı en fazla " + MaxUserNameLength + " karakter olabilir.";
            if (name.Any(c => c == ',' || c == '|' || char.IsControl(c)))
                return "Kullanıcı adında virgül, '|' veya kontrol karakteri olamaz.";
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ChatCommon
{
    /// <summary>Sunucu ile istemci arasında gidip gelen mesaj türleri.</summary>
    public enum MessageType
    {
        Join,       // istemci -> sunucu : "bu kullanıcı adıyla katılmak istiyorum"
        JoinAck,    // sunucu  -> istemci: katılım kabul edildi
        Text,       // genel sohbet mesajı
        Private,    // özel mesaj (To alanı dolu)
        UserList,   // sunucu  -> istemci: çevrimiçi kullanıcılar (Text alanında, virgülle)
        System,     // sunucu bilgilendirmesi (katıldı/ayrıldı/duyuru)
        Error,      // sunucu  -> istemci: hata (ör. kullanıcı adı kullanımda)
        Leave       // istemci -> sunucu : ayrılıyorum
    }

    /// <summary>
    /// Ağ üzerinden gönderilen tek bir mesaj.
    /// Her mesaj TCP akışında tek bir satır olarak taşınır:
    ///   TUR|gönderen|alıcı|zaman(ticks)|metin
    /// Alanlardaki '|', '\' ve satır sonu karakterleri kaçış (escape) ile kodlanır,
    /// böylece mesaj metni protokolü bozamaz.
    /// </summary>
    public class ChatMessage
    {
        private const char Separator = '|';

        public MessageType Type { get; private set; }
        public string From { get; private set; }
        public string To { get; private set; }
        public string Text { get; private set; }
        public DateTime Timestamp { get; private set; }

        public ChatMessage(MessageType type, string from, string to, string text, DateTime timestamp)
        {
            Type = type;
            From = from ?? "";
            To = to ?? "";
            Text = text ?? "";
            Timestamp = timestamp;
        }

        public ChatMessage(MessageType type, string from, string to, string text)
            : this(type, from, to, text, DateTime.Now)
        {
        }

        // ---------------------------------------------------------------- kodlama

        public string Encode()
        {
            var sb = new StringBuilder();
            sb.Append(Type.ToString()).Append(Separator);
            sb.Append(Escape(From)).Append(Separator);
            sb.Append(Escape(To)).Append(Separator);
            sb.Append(Timestamp.Ticks.ToString(CultureInfo.InvariantCulture)).Append(Separator);
            sb.Append(Escape(Text));
            return sb.ToString();
        }

        /// <summary>Bir satırı mesaja çevirir. Biçim bozuksa FormatException fırlatır.</summary>
        public static ChatMessage Decode(string line)
        {
            if (line == null) throw new ArgumentNullException("line");

            List<string> parts = SplitEscaped(line);
            if (parts.Count != 5)
                throw new FormatException("Beklenmeyen alan sayısı: " + parts.Count);

            MessageType type;
            if (!Enum.TryParse(parts[0], out type))
                throw new FormatException("Bilinmeyen mesaj türü: " + parts[0]);

            long ticks;
            if (!long.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks)
                || ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                throw new FormatException("Geçersiz zaman bilgisi.");

            return new ChatMessage(type, parts[1], parts[2], parts[4], new DateTime(ticks));
        }

        private static string Escape(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '|': sb.Append("\\p"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': break; // \r\n -> \n olarak taşınır
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        /// <summary>Kaçış karakterlerini çözerek satırı '|' ile ayırır.</summary>
        private static List<string> SplitEscaped(string line)
        {
            var parts = new List<string>();
            var current = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\\' && i + 1 < line.Length)
                {
                    char next = line[++i];
                    if (next == 'p') current.Append('|');
                    else if (next == 'n') current.Append('\n');
                    else current.Append(next);
                }
                else if (c == Separator)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            parts.Add(current.ToString());
            return parts;
        }

        // ---------------------------------------------------------------- görüntüleme

        /// <summary>Arayüzde/geçmiş dosyasında kullanılan okunur biçim.</summary>
        public string ToDisplayString()
        {
            string time = "[" + Timestamp.ToString("HH:mm:ss") + "] ";
            switch (Type)
            {
                case MessageType.Text: return time + From + ": " + Text;
                case MessageType.Private: return time + "(özel) " + From + " → " + To + ": " + Text;
                case MessageType.System: return time + "* " + Text;
                case MessageType.Error: return time + "HATA: " + Text;
                default: return time + Type + " " + Text;
            }
        }

        public override string ToString()
        {
            return ToDisplayString();
        }
    }
}

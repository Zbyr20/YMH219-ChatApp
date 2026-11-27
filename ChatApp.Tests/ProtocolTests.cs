using System;
using ChatCommon;

namespace ChatApp.Tests
{
    /// <summary>ChatMessage.Encode/Decode ve ChatProtocol kuralları (ağ yok).</summary>
    public static class ProtocolTests
    {
        private static readonly DateTime Time = new DateTime(2026, 11, 27, 14, 30, 5);

        private static ChatMessage RoundTrip(ChatMessage m)
        {
            string line = m.Encode();
            Assert.True(line.IndexOf('\n') < 0 && line.IndexOf('\r') < 0, "kodlanmış mesaj tek satır olmalı: " + line);
            return ChatMessage.Decode(line);
        }

        public static void RoundTripKeepsAllFields()
        {
            var m = new ChatMessage(MessageType.Private, "ali", "veli", "merhaba", Time);
            ChatMessage d = RoundTrip(m);
            Assert.Equal(MessageType.Private, d.Type, "Type");
            Assert.Equal("ali", d.From, "From");
            Assert.Equal("veli", d.To, "To");
            Assert.Equal("merhaba", d.Text, "Text");
            Assert.Equal(Time, d.Timestamp, "Timestamp");
        }

        public static void SpecialCharactersSurvive()
        {
            string[] texts =
            {
                "a|b|c", "ters\\bölü", "\\p gerçek metin", "satır1\nsatır2", "sonda ters bölü \\",
                "||||", "", "Çağrı şöyle öğütür: İı Ğğ Üü Şş Öö Çç", "emoji 🚀 ve ✅",
                "TUR|ali||0|sahte mesaj"   // protokol taklidi
            };
            foreach (string t in texts)
                Assert.Equal(t, RoundTrip(new ChatMessage(MessageType.Text, "x", "", t, Time)).Text, "metin '" + t + "'");
        }

        public static void CarriageReturnIsDropped()
        {
            var m = new ChatMessage(MessageType.Text, "x", "", "a\r\nb", Time);
            Assert.Equal("a\nb", RoundTrip(m).Text, "\\r\\n -> \\n");
        }

        public static void NullFieldsBecomeEmpty()
        {
            ChatMessage d = RoundTrip(new ChatMessage(MessageType.Leave, null, null, null, Time));
            Assert.Equal("", d.From, "From");
            Assert.Equal("", d.To, "To");
            Assert.Equal("", d.Text, "Text");
        }

        public static void DecodeRejectsMalformedLines()
        {
            string[] bad =
            {
                "",                                  // alan yok
                "Text|a|b|0",                        // 4 alan
                "Text|a|b|0|x|fazla",                // 6 alan
                "Bilinmeyen|a|b|0|x",                // bilinmeyen tür
                "Text|a|b|abc|x",                    // sayı değil
                "Text|a|b|-1|x",                     // DateTime aralığı dışı
                "Text|a|b|999999999999999999999|x",  // long taşması
            };
            foreach (string line in bad)
                Assert.Throws<FormatException>(() => ChatMessage.Decode(line), "Decode(\"" + line + "\")");

            Assert.Throws<ArgumentNullException>(() => ChatMessage.Decode(null), "Decode(null)");
        }

        public static void DisplayStringFormats()
        {
            Assert.Equal("[14:30:05] ali: selam", new ChatMessage(MessageType.Text, "ali", "", "selam", Time).ToDisplayString(), "Text");
            Assert.Equal("[14:30:05] (özel) ali → veli: gizli", new ChatMessage(MessageType.Private, "ali", "veli", "gizli", Time).ToDisplayString(), "Private");
            Assert.Equal("[14:30:05] * ali katıldı", new ChatMessage(MessageType.System, "Sunucu", "", "ali katıldı", Time).ToDisplayString(), "System");
        }

        public static void UserNameValidation()
        {
            Assert.Equal(null, ChatProtocol.ValidateUserName("Zübeyir"), "Türkçe karakterli ad geçerli");
            Assert.Equal(null, ChatProtocol.ValidateUserName(new string('a', ChatProtocol.MaxUserNameLength)), "tam sınırda ad geçerli");

            string[] invalid = { null, "", "   ", new string('a', ChatProtocol.MaxUserNameLength + 1), "a,b", "a|b", "a\nb", "a\tb" };
            foreach (string name in invalid)
                Assert.True(ChatProtocol.ValidateUserName(name) != null, "geçersiz sayılmalı: '" + name + "'");
        }

        public static void LongestValidMessageFitsInLineLimit()
        {
            // En kötü durum: her karakter kaçışla iki karaktere çıkar.
            string text = new string('|', ChatProtocol.MaxMessageLength);
            string line = new ChatMessage(MessageType.Private, new string('a', 20), new string('b', 20), text, DateTime.MaxValue).Encode();
            Assert.True(line.Length < ChatProtocol.MaxLineLength, "en uzun geçerli mesaj (" + line.Length + ") satır sınırını aşmamalı");
        }
    }
}

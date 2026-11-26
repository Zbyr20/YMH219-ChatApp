# YMH219 – İstemci-Sunucu Chat Uygulaması

**TCP/IP** üzerinden çalışan, bir sunucu ve birden fazla istemciden oluşan Windows Forms sohbet uygulaması. Ağ işlemleri `async/await` ile yapıldığı için arayüz donmaz; bağlantı hataları uygulamayı kapatmadan kullanıcıya bildirilir.

## Kullanılan Teknolojiler

| Teknoloji | Açıklama |
|---|---|
| C# / .NET 8 | Sunucu ve istemci: `net8.0-windows`, ortak kütüphane: `net8.0` |
| `TcpListener` / `TcpClient` | TCP bağlantısı |
| `NetworkStream` + `StreamReader` / `StreamWriter` | Satır tabanlı mesajlaşma (UTF-8) |
| `async / await`, `SemaphoreSlim` | Engellemeyen okuma/yazma, eşzamanlı yazma koruması |
| Windows Forms | ListView, ListBox, RichTextBox (renkli mesajlar) |
| NuGet paketi | **Yok** |

## Proje Yapısı

```
YMH219-ChatApp.sln
├── ChatCommon/    (sınıf kütüphanesi – iki tarafın ortak kodu)
│   ├── ChatMessage.cs        mesaj modeli + Encode()/Decode() (protokol)
│   ├── ChatProtocol.cs       varsayılan port, kullanıcı adı doğrulama
│   └── MessageConnection.cs  TcpClient sarmalayıcı: ReadAsync(), SendAsync()
├── ChatServer/    (Windows Forms)
│   ├── ChatServerCore.cs     dinleme, istemci yönetimi, yayın (broadcast), özel mesaj
│   ├── ClientSession.cs      bağlı istemci bilgisi (ad, adres, bağlanma zamanı, mesaj sayısı)
│   └── ServerForm.cs         arayüz
└── ChatClient/    (Windows Forms)
    ├── ChatClientCore.cs     bağlanma, gönderme, dinleme döngüsü
    └── ClientForm.cs         arayüz
```

## Kurulum ve Çalıştırma

1. `YMH219-ChatApp.sln` dosyasını Visual Studio 2022'de açın.
2. Solution'a sağ tık → **Configure Startup Projects** → **Multiple startup projects** → `ChatServer` ve `ChatClient` için **Start** seçin.
3. **F5**. Sunucu penceresinde **Sunucuyu Başlat**'a basın (varsayılan port **5050**).
4. İstemcide sunucu adresi (`127.0.0.1`), port ve kullanıcı adını girip **Bağlan**'a basın.
5. İkinci bir istemci için: `ChatClient\bin\Debug\net8.0-windows\ChatClient.exe` dosyasını bir kez daha çalıştırın.

**Yerel ağda (farklı bilgisayarlar):** İstemcide sunucu bilgisayarın IP adresini (`ipconfig` → IPv4) yazın. Windows Güvenlik Duvarı ilk çalıştırmada izin isteyebilir; "Özel ağ" için izin verin.

## Protokol

Her mesaj TCP akışında **tek satır** olarak gönderilir:

```
TUR|gönderen|alıcı|zaman(ticks)|metin
```

Metindeki `|`, `\` ve satır sonları kaçış karakterleriyle (`\p`, `\\`, `\n`) kodlanır; böylece kullanıcı ne yazarsa yazsın protokol bozulmaz.

| Tür | Yön | Anlamı |
|---|---|---|
| `Join` | İstemci → Sunucu | Kullanıcı adıyla katılma isteği |
| `JoinAck` / `Error` | Sunucu → İstemci | Kabul / ret (ör. kullanıcı adı kullanımda) |
| `Text` | iki yönlü | Genel mesaj (gönderen adını **sunucu** belirler) |
| `Private` | iki yönlü | Özel mesaj |
| `UserList` | Sunucu → İstemci | Çevrimiçi kullanıcılar |
| `System` | Sunucu → İstemci | Katıldı/ayrıldı bilgisi, duyuru |
| `Leave` | İstemci → Sunucu | Ayrılma |

## Temel Özellikler

- Aynı bilgisayarda veya yerel ağda çalışan **sunucu + birden fazla istemci**.
- İstemci **kullanıcı adı** ile bağlanır (boş/çok uzun/aynı ad reddedilir).
- Gönderilen ve alınan mesajlar **[SS:dd:ss] zaman bilgisiyle** gösterilir; kendi mesajlarınız mavi, özel mesajlar mor, sistem mesajları yeşil.
- **Bağlan**, **Mesaj Gönder** (Enter), **Bağlantıyı Kes** işlevleri.
- Bağlantı hataları (sunucu kapalı, adres çözülemedi, zaman aşımı, bağlantı koptu) **uygulama kapanmadan** mesaj kutusu ve durum çubuğuyla bildirilir.
- Sunucuda bağlı istemcilerin **kullanıcı adı, IP:port, bağlanma zamanı ve mesaj sayısı** listelenir.
- Sunucu: istemciyi atma, tüm istemcilere duyuru, sunucu günlüğü.

### İsteğe bağlı geliştirmeler (yapıldı)

- ✅ **Birden fazla istemci aynı odada** sohbet edebilir.
- ✅ **Özel mesaj:** sağdaki listeden kullanıcı seçip yazın.
- ✅ **Mesaj geçmişinin dosyaya kaydedilmesi** (hem sunucu hem istemci, `.txt`).

## Ekran Görüntüleri

| Sunucu | İstemci |
|---|---|
| ![Sunucu](screenshots/ekran1.png) | ![İstemci](screenshots/ekran2.png) |

## Bilinen Sınırlamalar / Geliştirme Notları

- Mesajlar şifrelenmeden gönderilir (TLS yok); yalnızca yerel ağ/ders amaçlıdır.
- Tek oda vardır; mesaj geçmişi sunucuda kalıcı veritabanında tutulmaz.
- Sunucu `ChatServerCore.StartAsync`'i UI iş parçacığından çağırdığı için `await` sonrası kod yine UI iş parçacığında çalışır; bu sayede istemci listesi kilitsiz ve güvenli şekilde güncellenir.

## Öğrenci

- **Ad Soyad:** _Adınız Soyadınız_
- **Öğrenci No:** _Numaranız_
- **Ders:** Nesne Tabanlı Programlama (YMH 219)

<p align="center">
  <strong>HTML → PDF</strong><br>
  <sub>WPF • Fluent Design • .NET 8</sub>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8-512BD4?style=flat-square&logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/Windows-WPF-0078D4?style=flat-square&logo=windows" alt="WPF">
  <img src="https://img.shields.io/badge/License-MIT-green?style=flat-square" alt="MIT">
</p>

---

<br>

<table>
<tr>
<td width="50%" valign="top">

### 🇹🇷 Türkçe

**HTML → PDF Dönüştürücü**, HTML dosyalarını PDF'e çeviren masaüstü uygulamasıdır. WPF ve Fluent Design (WPF-UI) ile Windows 11 tarzında arayüz sunar; dönüştürme işlemi PuppeteerSharp (headless Chromium) ile yapılır.

#### Özellikler

| Özellik | Açıklama |
|--------|----------|
| **Toplu dönüştürme** | Tek dosya veya klasör seçerek tüm `.html` / `.htm` dosyalarını PDF'e çevirir. |
| **Sürükle-bırak** | Dosya veya klasörü pencereye sürükleyip bırakarak kaynak seçebilirsiniz. |
| **PDF ayarları** | Sayfa boyutu (A4, Letter, A3, A5, Legal), kenar boşluğu (0,5–2 cm), yatay sayfa. |
| **Son kullanılan** | Son seçilen HTML ve çıktı klasörü ile PDF tercihleri saklanır. |
| **Sonuç listesi** | Her dosya için başarı veya hata mesajı tabloda gösterilir. |

#### Gereksinimler

- **.NET 8**
- **Windows** (WPF destekleyen sürüm)

#### Kurulum ve çalıştırma

```bash
git clone https://github.com/KULLANICI_ADIN/convert-html2pdf.git
cd convert-html2pdf
dotnet restore
dotnet run --project ConvertHtml2Pdf
```

> İlk çalıştırmada PuppeteerSharp Chromium indirir (~150 MB); sonraki dönüştürmeler hızlıdır.

#### Proje yapısı

```
convert-html2pdf/
├── ConvertHtml2Pdf.sln
└── ConvertHtml2Pdf/
    ├── App.xaml, MainWindow.xaml
    ├── Models/          # PdfConversionOptions, ConversionResult
    └── Services/        # HtmlToPdfService, AppSettingsService
```

</td>
<td width="50%" valign="top">

### 🇬🇧 English

**HTML → PDF Converter** is a desktop app that converts HTML files to PDF. It uses WPF with Fluent Design (WPF-UI) for a Windows 11–style UI and PuppeteerSharp (headless Chromium) for rendering.

#### Features

| Feature | Description |
|--------|-------------|
| **Batch conversion** | Select a single file or folder to convert all `.html` / `.htm` files to PDF. |
| **Drag & drop** | Drop a file or folder onto the window to set the source. |
| **PDF settings** | Page size (A4, Letter, A3, A5, Legal), margin (0.5–2 cm), landscape. |
| **Remember paths** | Last used HTML path, output folder, and PDF preferences are saved. |
| **Result list** | Per-file success or error message is shown in a table. |

#### Requirements

- **.NET 8**
- **Windows** (WPF-supported version)

#### Setup and run

```bash
git clone https://github.com/USERNAME/convert-html2pdf.git
cd convert-html2pdf
dotnet restore
dotnet run --project ConvertHtml2Pdf
```

> On first run, PuppeteerSharp downloads Chromium (~150 MB); subsequent conversions are fast.

#### Project structure

```
convert-html2pdf/
├── ConvertHtml2Pdf.sln
└── ConvertHtml2Pdf/
    ├── App.xaml, MainWindow.xaml
    ├── Models/          # PdfConversionOptions, ConversionResult
    └── Services/        # HtmlToPdfService, AppSettingsService
```

</td>
</tr>
</table>

---

<p align="center">
  <sub><strong>Lisans / License</strong> — MIT</sub>
</p>

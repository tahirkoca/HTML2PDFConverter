using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;
using Wpf.Ui.Controls;
using ConvertHtml2Pdf.Models;
using ConvertHtml2Pdf.Services;

namespace ConvertHtml2Pdf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    private readonly HtmlToPdfService _pdfService = new();
    private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
    private AppSettingsService _settings = null!;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_THICKFRAME = 0x00040000;
    private const int WS_MAXIMIZEBOX = 0x00010000;
    private const int WS_MINIMIZEBOX = 0x00020000;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        SourceInitialized += MainWindow_SourceInitialized;
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return;
        var style = GetWindowLong(hwnd, GWL_STYLE);
        style &= ~WS_THICKFRAME;
        style &= ~WS_MAXIMIZEBOX;
        SetWindowLong(hwnd, GWL_STYLE, style);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _settings = AppSettingsService.Load();
        if (!string.IsNullOrWhiteSpace(_settings.LastHtmlPath))
            HtmlPathTextBox.Text = _settings.LastHtmlPath;
        if (!string.IsNullOrWhiteSpace(_settings.LastOutputPath))
            OutputDirTextBox.Text = _settings.LastOutputPath;

        // PDF ayarları
        var paper = _settings.PdfOptions.PaperFormat;
        for (int i = 0; i < PaperFormatComboBox.Items.Count; i++)
        {
            if (PaperFormatComboBox.Items[i] is System.Windows.Controls.ComboBoxItem item &&
                item.Content?.ToString()?.Equals(paper, StringComparison.OrdinalIgnoreCase) == true)
            {
                PaperFormatComboBox.SelectedIndex = i;
                break;
            }
        }
        var margin = _settings.PdfOptions.MarginCm;
        for (int i = 0; i < MarginComboBox.Items.Count; i++)
        {
            if (MarginComboBox.Items[i] is System.Windows.Controls.ComboBoxItem item &&
                item.Tag is string tag && double.TryParse(tag, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) && Math.Abs(v - margin) < 0.01)
            {
                MarginComboBox.SelectedIndex = i;
                break;
            }
        }
        LandscapeCheckBox.IsChecked = _settings.PdfOptions.Landscape;
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveSettings();
    }

    private void SaveSettings()
    {
        _settings ??= AppSettingsService.Load();
        _settings.LastHtmlPath = HtmlPathTextBox.Text?.Trim();
        _settings.LastOutputPath = OutputDirTextBox.Text?.Trim();
        _settings.PdfOptions = GetPdfOptions();
        _settings.Save();
    }

    private PdfConversionOptions GetPdfOptions()
    {
        var paper = "A4";
        if (PaperFormatComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem pi)
            paper = pi.Content?.ToString() ?? "A4";
        var marginCm = 1.0;
        if (MarginComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem mi && mi.Tag is string tag)
            double.TryParse(tag, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out marginCm);
        return new PdfConversionOptions
        {
            PaperFormat = paper,
            MarginCm = marginCm,
            Landscape = LandscapeCheckBox.IsChecked == true
        };
    }

    private void Window_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            e.Effects = System.Windows.DragDropEffects.Copy;
        else
            e.Effects = System.Windows.DragDropEffects.None;
        e.Handled = true;
    }

    private void Window_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            return;
        var paths = (string[]?)e.Data.GetData(System.Windows.DataFormats.FileDrop);
        if (paths == null || paths.Length == 0)
            return;

        var path = paths[0];
        if (Directory.Exists(path))
        {
            HtmlPathTextBox.Text = path;
            if (string.IsNullOrWhiteSpace(OutputDirTextBox.Text))
                OutputDirTextBox.Text = path;
        }
        else if (File.Exists(path))
        {
            var ext = Path.GetExtension(path);
            if (string.Equals(ext, ".html", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ext, ".htm", StringComparison.OrdinalIgnoreCase))
            {
                HtmlPathTextBox.Text = path;
                if (string.IsNullOrWhiteSpace(OutputDirTextBox.Text))
                    OutputDirTextBox.Text = Path.GetDirectoryName(path) ?? "";
            }
        }
        e.Handled = true;
    }

    private void BrowseFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "HTML dosyaları (*.html;*.htm)|*.html;*.htm|Tüm dosyalar (*.*)|*.*",
            Title = "HTML dosyası seçin",
            Multiselect = false
        };
        if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.FileName))
        {
            HtmlPathTextBox.Text = dlg.FileName;
            if (string.IsNullOrWhiteSpace(OutputDirTextBox.Text))
                OutputDirTextBox.Text = Path.GetDirectoryName(dlg.FileName) ?? "";
            SaveSettings();
        }
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dlg = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "HTML dosyalarının bulunduğu klasörü seçin",
            UseDescriptionForTitle = true
        };
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
        {
            HtmlPathTextBox.Text = dlg.SelectedPath;
            if (string.IsNullOrWhiteSpace(OutputDirTextBox.Text))
                OutputDirTextBox.Text = dlg.SelectedPath;
            SaveSettings();
        }
    }

    private void BrowseOutputFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dlg = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "PDF dosyalarının kaydedileceği klasörü seçin",
            UseDescriptionForTitle = true
        };
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
        {
            OutputDirTextBox.Text = dlg.SelectedPath;
            SaveSettings();
        }
    }

    private async void Convert_Click(object sender, RoutedEventArgs e)
    {
        var inputPath = HtmlPathTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            ShowMessage("Lütfen bir HTML dosyası veya klasör seçin.", InfoBarSeverity.Warning);
            return;
        }

        var outputDir = OutputDirTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(outputDir))
            outputDir = File.Exists(inputPath) ? Path.GetDirectoryName(inputPath) ?? inputPath : inputPath;

        if (!Directory.Exists(outputDir))
        {
            try
            {
                Directory.CreateDirectory(outputDir);
            }
            catch (Exception ex)
            {
                ShowMessage($"Çıktı klasörü oluşturulamadı: {ex.Message}", InfoBarSeverity.Error);
                return;
            }
        }

        var htmlPaths = HtmlToPdfService.ResolveHtmlPaths(inputPath);
        if (htmlPaths.Count == 0)
        {
            ShowMessage("Seçilen konumda HTML dosyası bulunamadı.", InfoBarSeverity.Warning);
            return;
        }

        SetConvertingState(true);
        ResultListView.ItemsSource = null;
        ProgressBar.Visibility = Visibility.Visible;
        ProgressBar.IsIndeterminate = htmlPaths.Count == 1;
        if (htmlPaths.Count > 1)
        {
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = 0;
            ProgressBar.Maximum = htmlPaths.Count;
        }
        StatusTextBlock.Visibility = Visibility.Visible;
        StatusTextBlock.Text = htmlPaths.Count == 1
            ? "Dönüştürülüyor..."
            : $"0 / {htmlPaths.Count} dönüştürülüyor...";

        var progress = new Progress<(int current, int total)>(p =>
        {
            _dispatcher.Invoke(() =>
            {
                if (htmlPaths.Count > 1)
                {
                    ProgressBar.Value = p.current;
                    ProgressBar.Maximum = p.total;
                }
                StatusTextBlock.Text = $"{p.current} / {p.total} işlendi.";
            });
        });

        var pdfOptions = GetPdfOptions();

        try
        {
            var result = await _pdfService.ConvertAsync(htmlPaths, outputDir, pdfOptions, progress, default);
            ResultListView.ItemsSource = result.Items;

            if (result.ErrorCount == 0)
                ShowMessage($"{result.SuccessCount} dosya başarıyla PDF'e dönüştürüldü.", InfoBarSeverity.Success);
            else if (result.SuccessCount == 0)
                ShowMessage($"Tüm dosyalar başarısız. ({result.ErrorCount} hata)", InfoBarSeverity.Error);
            else
                ShowMessage($"{result.SuccessCount} başarılı, {result.ErrorCount} hata.", InfoBarSeverity.Warning);

            SaveSettings();
        }
        catch (OperationCanceledException)
        {
            ShowMessage("Dönüştürme iptal edildi.", InfoBarSeverity.Informational);
        }
        catch (Exception ex)
        {
            ShowMessage($"Hata: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            SetConvertingState(false);
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            StatusTextBlock.Visibility = Visibility.Collapsed;
            StatusTextBlock.Text = "";
        }
    }

    private void SetConvertingState(bool isConverting)
    {
        ConvertButton.IsEnabled = !isConverting;
        HtmlPathTextBox.IsEnabled = !isConverting;
        OutputDirTextBox.IsEnabled = !isConverting;
        PaperFormatComboBox.IsEnabled = !isConverting;
        MarginComboBox.IsEnabled = !isConverting;
        LandscapeCheckBox.IsEnabled = !isConverting;
    }

    private void ShowMessage(string message, InfoBarSeverity severity)
    {
        MessageInfoBar.Title = severity switch
        {
            InfoBarSeverity.Success => "Başarılı",
            InfoBarSeverity.Warning => "Uyarı",
            InfoBarSeverity.Error => "Hata",
            _ => "Bilgi"
        };
        MessageInfoBar.Message = message;
        MessageInfoBar.Severity = severity;
        MessageInfoBar.IsOpen = true;
    }
}

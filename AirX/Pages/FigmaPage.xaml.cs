// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;

using System.Diagnostics;
using System.Net.Http;
using Windows.UI.Xaml;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System;
using Windows.Networking.BackgroundTransfer;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AirX.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FigmaPage : Page
    {
        private NewTextViewModel ViewModel = new();

        // private const string FileUrl = "https://www.pandafoods.ca/cdn/shop/products/6921804701525_1024x_cae75e83-139f-453a-aabf-472ba2215f7d_540x.jpg?v=1657391551"; // URL of the file to download
        private const string FileUrl = "https://www.mikutart.com/a.png";
        private const int BufferSize = 8192; // Size of the buffer in bytes

        private HttpClient httpClient;
        private Stopwatch stopwatch;


        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private static int _clicks = 0;

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            _clicks += 1;
        }

        private double _percent;

        public double Percent
        {
            get { return _percent; }
            set { _percent = value; OnPropertyChanged("Percent"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public FigmaPage()
        {
            this.InitializeComponent();
            httpClient = new HttpClient();
            stopwatch = new Stopwatch();

        }

        private async void OnDownloadClick(object sender, RoutedEventArgs e)
        {
            // var saveFile = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync("Laoganma.jpg", CreationCollisionOption.ReplaceExisting);
            var file = File.Create("C:\\Users\\gc135\\OneDrive\\桌面\\test.png");

            using (var response = await httpClient.GetAsync(FileUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = file.AsOutputStream().AsStreamForWrite()) //  await saveFile.OpenStreamForWriteAsync()
                    {
                        stopwatch.Start();
                        byte[] buffer = new byte[BufferSize];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            // await fileStream.WriteAsync(buffer, 0, bytesRead);

                            // Calculate download speed
                            double speed = fileStream.Position / stopwatch.Elapsed.TotalSeconds;
                            // SpeedTextBlock.Text = $"{speed:F2} bytes/sec";

                            // Calculate download progress
                            totalBytesRead += bytesRead;
                            double progress = (double)totalBytesRead / totalBytes;
                            // ProgressBar.Value = progress * 100;
                        }
                    }
                }
            }

            stopwatch.Stop();
            // SpeedTextBlock.Text = "Download complete!";
            // ProgressBar.Value = 100;
        }


        private StorageFile _selectedFile;
        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "Drop file here";
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = true;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }

            e.Handled = true;
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();

                // Assuming only one file is dropped, you can modify this logic for multiple files
                if (items.Count > 0 && items[0] is StorageFile file)
                {
                    // Store the file for later use
                    _selectedFile = file;

                    // Display the file information
                    // FileNameTextBlock.Text = "File Name: " + file.Name;
                    // FilePathTextBlock.Text = "File Path: " + file.Path;
                    BasicProperties properties = await file.GetBasicPropertiesAsync();
                    // FileSizeTextBlock.Text = "File Size: " + properties.Size + " bytes";
                }
            }
            e.Handled = true;
        }

       
        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFile != null)
            {
                // Open the selected file using the default program
                await Launcher.LaunchFileAsync(_selectedFile);
            }
        }

        private const string ApiEndpoint = "https://api.example.com/share"; // Replace with the actual API endpoint of the file-sharing service

        public async Task ShareFile(string filePath, string apiToken)
        {
            using (var httpClient = new HttpClient())
            {
                using (var form = new MultipartFormDataContent())
                {
                   
                    byte[] fileBytes = File.ReadAllBytes(filePath);

                    
                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.Add("Content-Type", "application/octet-stream");
                    fileContent.Headers.Add("Content-Disposition", $"form-data; name=\"file\"; filename=\"{Path.GetFileName(filePath)}\"");

                    
                    form.Add(fileContent);
                   
                    form.Add(new StringContent(apiToken), "api_key");
               
                    var response = await httpClient.PostAsync(ApiEndpoint, form);
                 
                    if (response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("File shared successfully!");
                    }
                    else
                    {
                        Debug.WriteLine($"File sharing failed. Error: {response.StatusCode}");
                    }
                }
            }
        }

    }

}

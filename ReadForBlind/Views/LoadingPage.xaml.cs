﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using Microsoft.Hawaii;
using Microsoft.Hawaii.Ocr.Client;
using System.Text;

namespace ReadForBlind.Views
{
    public partial class LoadingPage : PhoneApplicationPage
    {
        private BitmapImage bmp_raw;
        private WriteableBitmap bmp;
        private Reader reader;
        private Listener listener;
        private Utils utils;

        public LoadingPage()
        {
            InitializeComponent();
            bmp_raw = new BitmapImage();

            //CONNECT: remove the below comment to make it work with the camera class & comment the line below it
            //bmp_raw = (BitmapImage)PhoneApplicationService.Current.State["image"];
            bmp_raw = new BitmapImage(new Uri("/images/m20.jpg", UriKind.Relative));
            bmp_raw.CreateOptions = BitmapCreateOptions.None;       // makes the image creating instantaneous
            bmp_raw.ImageOpened += bmp_raw_ImageOpened;
            
            reader = new Reader();
            listener = new Listener();
        }

        private void bmp_raw_ImageOpened(object sender, RoutedEventArgs e)
        {
            // set the image that we got from camera to the bg of loadingpage
            bg.ImageSource = bmp_raw;
            utils = new Utils(bmp_raw.PixelWidth, bmp_raw.PixelHeight);
        }

        private void StartOcr()
        {
            reader.readText("Image has been captured.");
            reader.readText("Please let me analyse it.");

            utils.deskew(ref bmp);

            if (bmp.PixelHeight > 640 || bmp.PixelWidth > 640)
                Utils.resizeImage(ref bmp);

            byte[] photoBuffer = Utils.imageToByte(bmp);
            OcrService.RecognizeImageAsync(Utils.HawaiiApplicationId, photoBuffer, (output) =>
            {
                Dispatcher.BeginInvoke(() => OnOcrComplete(output));
            });
        }

        private async void OnOcrComplete(OcrServiceResult result)
        {
            if (result.Status == Status.Success)
            {
                int wordCount = 0;
                List<String> text = new List<string>();
                foreach (OcrText item in result.OcrResult.OcrTexts)
                {
                    wordCount += item.Words.Count;
                    foreach (var word in item.Words)
                    {
                        text.Add(word.Text);
                    }
                    //sb.AppendLine(item.Text);
                }
                //MessageBox.Show(sb.ToString());
                PhoneApplicationService.Current.State["text"] = text;
                NavigationService.Navigate(new Uri("/Views/OutputPage.xaml", UriKind.Relative));
            }
            else
            {
                statusText.Text = "[OCR conversion failed]\n" + result.Exception.Message;
                reader.readText("OCR conversion failed because : " + result.Exception.Message);
                reader.readText("Do you want to retry?");
                if (await listener.ConversionFailedConfirmation() == "yes")
                {
                    StartOcr();
                }
                else
                {
                    reader.readText("Getting back to new photo screen");
                    NavigationService.GoBack();
                }
            }
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            bmp = new WriteableBitmap(bmp_raw);
            StartOcr();     // Initiates the OCR process
        }

        // this button is just to check if the UI thread is not blocked while OCR is running in the bg
        private void ClickME(object sender, RoutedEventArgs e)
        {
            StartOcr();
        }

    }
}
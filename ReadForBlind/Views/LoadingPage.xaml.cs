using System;
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
        private Reader read;

        public LoadingPage()
        {
            InitializeComponent();
            bmp_raw = new BitmapImage();

            //CONNECT: remove the below comment to make it work with the camera class & comment the line below it
            //bmp_raw = (BitmapImage)PhoneApplicationService.Current.State["image"];
            bmp_raw = new BitmapImage(new Uri("/images/m20.jpg", UriKind.Relative));
            bmp_raw.CreateOptions = BitmapCreateOptions.None;       // makes the image creating instantaneous

            // set the image that we got from camera to the bg of loadingpage
            bg.ImageSource = bmp_raw;
            read = new Reader();
            
        }

        private void StartOcr() 
        {
            read.readText("Image has been captured.");
            read.readText("Please let me analyse it.");

            Utils.deskew(ref bmp);

            if (bmp.PixelHeight > 640 || bmp.PixelWidth > 640)
                Utils.resizeImage(ref bmp);

            byte[] photoBuffer = Utils.imageToByte(bmp);
            OcrService.RecognizeImageAsync(Utils.HawaiiApplicationId, photoBuffer, (output) =>
            {
                Dispatcher.BeginInvoke(() => OnOcrComplete(output));
            });
        }

        private void OnOcrComplete(OcrServiceResult result)
        {
            if (result.Status == Status.Success)
            {
                int wordCount = 0;
                List<String> text = new List<string>();
                foreach (OcrText item in result.OcrResult.OcrTexts)
                {
                    wordCount += item.Words.Count;
                    text.Add(item.Text);
                    //sb.AppendLine(item.Text);
                }
                //MessageBox.Show(sb.ToString());
                PhoneApplicationService.Current.State["text"] = text;
                NavigationService.Navigate(new Uri("/Views/OutputPage.xaml", UriKind.Relative));
            }
            else
            {
                statusText.Text = "[OCR conversion failed]\n" + result.Exception.Message;
                read.readText("OCR conversion failed because : " + result.Exception.Message);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Devices;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media.Imaging;
using System.Threading;

namespace ReadForBlind.Views
{
    public partial class Camera : PhoneApplicationPage
    {

        private PhotoCamera camera;
        private Thread imageProcessing;
        private Reader reader;
        private Listener listener;
        private static bool pumpARGBFrames;
        private static ManualResetEvent pauseFramesEvent = new ManualResetEvent(true);
        private WriteableBitmap wb, wbl;   // wb for original image    // wbl for lower resolution image

        public Camera()
        {
            InitializeComponent();
            reader = new Reader();
            listener = new Listener();
        }

        private void Process() {
            int w = (int)camera.PreviewResolution.Width;    // width
            int h = (int)camera.PreviewResolution.Height;   // height
            int w2 = w / 2;     // half the image width
            int h2 = h / 2;     // half the image height
            int[] ARGBPx = new int[w * h];      // original pixels
            int[] ARGBPl = new int[320 * 240];  // lower resolution pixels

            try
            {
                PhotoCamera phCam = (PhotoCamera)camera;
                while (pumpARGBFrames)
                {
                    pauseFramesEvent.WaitOne();
                    phCam.GetPreviewBufferArgb32(ARGBPx);
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        // REsize for performance gain
                        ARGBPx.CopyTo(wb.Pixels, 0);
                        wbl = wb.Resize(w2, h2, WriteableBitmapExtensions.Interpolation.Bilinear);
                    });
                    // Get skew angle
                    Utils.Deskew d = new Utils.Deskew(wbl);
                    double skewAngle = -1 * d.GetSkewAngle();
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        // Deskew
                        wbl = wbl.RotateFree(skewAngle);
                    });
                    wbl.Pixels.CopyTo(ARGBPl, 0);   // copy resized image pixels to array for further process in other thread
                    //ARGBPl = UtilsArray.GrayScale(ARGBPl);
                    ARGBPl = UtilsArray.Binarize(ARGBPl, 51);   // try & error with threashold value
                    ARGBPl = UtilsArray.Bitwise_not(ARGBPl);    // BUGGY - Makes the image disappear
                    ARGBPl = UtilsArray.Erode(ARGBPl, w2, h2);  // Erode the image
                    pauseFramesEvent.Reset();
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        // Copy back to lower resolution bitmapimage
                        ARGBPl.CopyTo(wbl.Pixels, 0);
                        wbl.Invalidate();
                        // TODO: identify if the image is complete, 
                        // IF yes, capture => stop this thread => crop the text area => send to hawaii => hope for best results
                        // else, continue
                        pauseFramesEvent.Set();
                    });
                }
            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke(delegate()
                {
                    // Display error message.
                    txtmsg.Text = e.Message;
                });
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true))
            {
                camera = new PhotoCamera(CameraType.Primary);
                this.setAutoFlash();
                camera.Initialized += new EventHandler<CameraOperationCompletedEventArgs>(cameraInitialized);
                camera.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(captureCompleted);
                camera.CaptureImageAvailable += new EventHandler<ContentReadyEventArgs>(captureImageAvailable);
                viewfinderBrush.SetSource(camera);
            }
            else
            {
                reader.readText("Sorry, but I can't find a working camera on this device");
            }
        }

        private void cameraInitialized(object sender, CameraOperationCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(delegate() {
                imageProcessing = new Thread(Process);
                pumpARGBFrames = true;
                wb = new WriteableBitmap((int)camera.PreviewResolution.Width, (int)camera.PreviewResolution.Height);
                wbl = new WriteableBitmap(320, 240);
                img.Source = wb;
                imageProcessing.Start();
                txtmsg.Text = "width = " + camera.PreviewResolution.Width.ToString() + " height = " + camera.PreviewResolution.Height.ToString();
            });
        }

        private void captureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(delegate()
            {
                txtmsg.Text = "Image captured";
            });
        }

        private void captureImageAvailable(object sender, ContentReadyEventArgs e)
        {
            Dispatcher.BeginInvoke(delegate()
            {
                txtmsg.Text = "Image Available";
                BitmapImage bmpImage = new BitmapImage();
                bmpImage.CreateOptions = BitmapCreateOptions.None;
                bmpImage.SetSource(e.ImageStream);
                PhoneApplicationService.Current.State["image"] = bmpImage;
                NavigationService.Navigate(new Uri("/Views/LoadingPage.xaml", UriKind.Relative));
            });
        }

        private void cameraCanvasTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (camera != null)
            {
                try
                {
                    camera.CaptureImage();
                }

                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(delegate()
                    {
                        txtmsg.Text = ex.Message;
                    });
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (camera != null)
            {
                pumpARGBFrames = false;
                camera.Dispose();
                camera.Initialized -= cameraInitialized;
                camera.CaptureCompleted -= captureCompleted;
                camera.CaptureImageAvailable -= captureImageAvailable;
            }
        }

        private void startFlash()
        {
            if (camera.IsFlashModeSupported(FlashMode.On))
            {
                try
                {
                    camera.FlashMode = FlashMode.On;
                }
                catch (Exception ex) { }
            }
        }

        private void stopFlash()
        {
            if (camera.IsFlashModeSupported(FlashMode.Off))
            {
                try
                {
                    camera.FlashMode = FlashMode.Off;
                }
                catch (Exception ex) { }
            }
        }

        private void setAutoFlash()
        {
            if (camera.IsFlashModeSupported(FlashMode.Auto))
            {
                try
                {
                    camera.FlashMode = FlashMode.Auto;
                }
                catch (Exception ex) { }
            }
        }

        private async void OpenVoiceCommand(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string result = await listener.Listen();
            if (result != null)
            {
                if (result.Contains("play") || result.Contains("start") || result.Contains("reed"))
                {
                    reader.readText("An image of the text has to be captured before reading it.");
                }
                else if (result.Contains("pause") || result.Contains("stop"))
                {
                    reader.readText("If you want to exit the app, say exit");
                }
                else if (result.Contains("new") || result.Contains("photo"))
                {
                    reader.readText("You are currently on new photo page");
                }
            }
        }
    }
}
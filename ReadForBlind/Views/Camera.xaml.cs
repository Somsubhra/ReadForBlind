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
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Devices.Sensors;
using System.IO;

namespace ReadForBlind.Views
{
    public partial class Camera : PhoneApplicationPage
    {

        private PhotoCamera camera;
        private Thread imageProcessing;
        private Reader reader;
        private Listener listener;
        private static bool pumpARGBFrames, isStable = false;
        private static ManualResetEvent pauseFramesEvent = new ManualResetEvent(true);
        private WriteableBitmap wb;   // wb for original image
        private Utils utils;
        private static Accelerometer acc;
        private double oldx, oldy, oldz;
        private static BitmapImage bmpimg;

        public Camera()
        {
            InitializeComponent();
            reader = new Reader();
            listener = new Listener();
            acc = new Accelerometer();
            try
            {
                acc.Start();
                acc.CurrentValueChanged += acc_CurrentValueChanged;
                oldy = acc.CurrentValue.Acceleration.Y;
                oldx = acc.CurrentValue.Acceleration.X;
                oldz = acc.CurrentValue.Acceleration.Z;
            }
            catch (AccelerometerFailedException) { }
        }

        private async void acc_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            if (!pumpARGBFrames)
            {
                acc.Stop();
                return;
            }
            if (Math.Abs((double)(oldx - acc.CurrentValue.Acceleration.X)) > 0.20 || Math.Abs((double)(oldy - acc.CurrentValue.Acceleration.Y)) > 0.20)
            {
                await reader.readText(" stabalize ");
                isStable = false;
            }
            else
            {
                isStable = true;
            }
        }

        private async void Process()
        {
            int w = (int)camera.PreviewResolution.Width;    // width
            int h = (int)camera.PreviewResolution.Height;   // height
            int w2 = w / 2;     // half the image width
            int h2 = h / 2;     // half the image height
            int[] ARGBPx = new int[w * h];      // original pixels
            int skipframe = 0;
            try
            {
                
                PhotoCamera phCam = (PhotoCamera)camera;
                while (pumpARGBFrames)
                {
                    pauseFramesEvent.WaitOne();
                    phCam.GetPreviewBufferArgb32(ARGBPx);
                    ARGBPx = utils.Binarize(ARGBPx, 140);   // try & error with threashold value
                    //ARGBPx = utils.Bitwise_not(ARGBPx);   // STILL BUGGY - Makes the Image disappear
                    ARGBPx = utils.Erode(ARGBPx);
                    Utils.Boundaries b = utils.CheckBoundaries(ARGBPx);
                    skipframe++;
                    if(skipframe == 4){
                        ImageHandler(b);      // This may create a lag // BEAWARE
                        skipframe = 0;
                    }
                    pauseFramesEvent.Reset();
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        ARGBPx.CopyTo(wb.Pixels, 0);
                        wb.Invalidate();
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
                    //txtmsg.Text = e.Message;
                    //reader.readText(e.Message);
                });
            }
        }

        private async Task ImageHandler(Utils.Boundaries b)
        //private void ImageHandler(Utils.Boundaries b)
        {
            if (b.Left || b.Right || b.Top || b.Bottom)
            {
                String s = "";
                int i = 0;
                if (b.Right)
                {
                    s += " right ";
                    i++;
                    //await reader.readText(" right ");
                }
                if (b.Left)
                {
                    s += " left ";
                    i++;
                    //await reader.readText(" left ");
                }
                if (b.Top)
                {
                    s += " top ";
                    i++;
                    //await reader.readText(" top ");
                }
                if (b.Bottom)
                {
                    s += " bottom ";
                    i++;
                    //await reader.readText(" bottom ");
                }
                if (i > 2)
                    s = " up ";
                Dispatcher.BeginInvoke(delegate() {
                    txtmsg.Text = s;
                });
                reader.readText(s);
            }
            else
            {
                Dispatcher.BeginInvoke(delegate()
                {
                    txtmsg.Text = "Please don't move the phone, let me click";
                });
                await reader.readText("Please don't move the phone, let me click");
                //if (isStable)
                //{
                    camera.Focus();
                    pumpARGBFrames = false;
                    reader.Dispose();
                //}
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true))
            {
                camera = new PhotoCamera(CameraType.Primary);
                camera.Initialized += cameraInitialized;
                camera.CaptureCompleted += captureCompleted;
                camera.CaptureImageAvailable += captureImageAvailable;
                camera.AutoFocusCompleted += cam_AutoFocusCompleted;
                viewfinderBrush.SetSource(camera);
                previewTransform.Rotation = camera.Orientation;
                reader.readText("Lift the phone");
            }
            else
            {
                reader.readText("Sorry, but I can't find a working camera on this device");
            }
        }

        private void cam_AutoFocusCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            camera.CaptureImage();
        }

        private void cameraInitialized(object sender, CameraOperationCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(delegate()
            {
                DispatcherTimer dt = new DispatcherTimer();
                dt.Interval = new TimeSpan(0, 0, 0, 0, 600);
                dt.Tick += dt_Tick;
                
                pumpARGBFrames = true;
                int w = (int)camera.PreviewResolution.Width;
                int h = (int)camera.PreviewResolution.Height;
                wb = new WriteableBitmap(w, h);
                utils = new Utils(w, h);
                img.Source = wb;
                
                txtmsg.Text = "width = " + w.ToString() + " height = " + h.ToString();
                setAutoFlash();
                dt.Start();
            });
        }

        private void dt_Tick(object sender, EventArgs e)
        {
            imageProcessing = new Thread(Process);
            imageProcessing.Start();
        }

        private void captureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(delegate()
            {
                txtmsg.Text = "Image captured";
                PhoneApplicationService.Current.State["image"] = bmpimg;
                NavigationService.Navigate(new Uri("/Views/LoadingPage.xaml", UriKind.Relative));
            });
            
        }

        private void captureImageAvailable(object sender, ContentReadyEventArgs e)
        {
            Dispatcher.BeginInvoke(delegate()
            {
                txtmsg.Text = "Image Available";
                bmpimg = new BitmapImage();
                bmpimg.CreateOptions = BitmapCreateOptions.None;
                bmpimg.SetSource(e.ImageStream);
                WriteableBitmap wb = new WriteableBitmap(bmpimg);
                Utils.resizeImage(ref wb);
                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveJpeg(ms, (int)wb.PixelWidth, (int)wb.PixelHeight, 0, 100);
                    bmpimg.SetSource(ms);
                }
                //PhoneApplicationService.Current.State["image"] = bmpimg;
                //NavigationService.Navigate(new Uri("/Views/LoadingPage.xaml", UriKind.Relative));
            });
        }

        private void cameraCanvasTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (camera != null)
            {
                try
                {
                    camera.Focus();
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
                acc.Stop();
                imageProcessing.Abort();
                reader = null;
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
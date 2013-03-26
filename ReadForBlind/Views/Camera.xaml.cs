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
        private MediaLibrary mediaLibrary;
        private Thread imageProcessing;
        private bool process;
        private Reader read;

        public Camera()
        {
            InitializeComponent();
            mediaLibrary = new MediaLibrary();
            read = new Reader();
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
                read.readText("Sorry, but I can't find a working camera on this device");
            }
        }

        private void cameraInitialized(object sender, CameraOperationCompletedEventArgs e) { 
           
        }

        private void captureCompleted(object sender, CameraOperationCompletedEventArgs e) {
            Dispatcher.BeginInvoke(delegate() {
                txtmsg.Text = "Image captured";
            });
        }

        private void captureImageAvailable(object sender, ContentReadyEventArgs e) {
            Dispatcher.BeginInvoke(delegate() {
                txtmsg.Text = "Image Available";
                BitmapImage bmpImage = new BitmapImage();
                bmpImage.CreateOptions = BitmapCreateOptions.None;
                bmpImage.SetSource(e.ImageStream);
                PhoneApplicationService.Current.State["image"] = bmpImage;
                NavigationService.Navigate(new Uri("/Views/LoadingPage.xaml", UriKind.Relative));
            });
        }
            
        private void cameraCanvasTapped(object sender, System.Windows.Input.GestureEventArgs e) {
            if (camera != null) {
                try
                {
                    camera.CaptureImage();
                }

                catch (Exception ex) {
                    Dispatcher.BeginInvoke(delegate() {
                        txtmsg.Text = ex.Message;
                    });
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (camera != null) {
                camera.Dispose();
                camera.Initialized -= cameraInitialized;
                camera.CaptureCompleted -= captureCompleted;
                camera.CaptureImageAvailable -= captureImageAvailable;
            }
        }

        private void startFlash() {
            if (camera.IsFlashModeSupported(FlashMode.On))
            {
                try
                {
                    camera.FlashMode = FlashMode.On;
                }
                catch (Exception ex) { }
            }
        }

        private void stopFlash() {
            if (camera.IsFlashModeSupported(FlashMode.Off))
            {
                try
                {
                    camera.FlashMode = FlashMode.Off;
                }
                catch (Exception ex) { }
            }
        }

        private void setAutoFlash() {
            if (camera.IsFlashModeSupported(FlashMode.Auto)) {
                try
                {
                    camera.FlashMode = FlashMode.Auto;
                }
                catch(Exception ex) { }
            }
        }
    }
}
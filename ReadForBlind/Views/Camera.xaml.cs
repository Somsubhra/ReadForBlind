﻿using System;
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
        private Thread imgageProcessing;
        private bool process;

        public Camera()
        {
            InitializeComponent();
            mediaLibrary = new MediaLibrary();
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
    }
}
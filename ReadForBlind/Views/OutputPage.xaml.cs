using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace ReadForBlind.Views
{
    public partial class OutputPage : PhoneApplicationPage
    {
        private String readText;
        public OutputPage()
        {
            InitializeComponent();

            readText = PhoneApplicationService.Current.State["text"].ToString();
            txt.Text = readText;
        }

        // this method should listen for the user input: play, pause, restart, etc..
        private void ScreenDoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            throw new NotImplementedException();
        }

        // Play pause the running speech
        // if it's playing pause it
        // if it's paused, play it.
        private void ScreenTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            NavigationService.RemoveBackEntry();
            NavigationService.GoBack();
        }
    }
}
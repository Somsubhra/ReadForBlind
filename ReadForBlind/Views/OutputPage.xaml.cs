using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading;
using System.Threading.Tasks;


namespace ReadForBlind.Views
{
    public partial class OutputPage : PhoneApplicationPage
    {
        private String readText;
        int count = 0;
        List<String> text;
        CancellationTokenSource cts = null;
        Reader read;

        public OutputPage()
        {
            InitializeComponent();

            text = (List<String>)PhoneApplicationService.Current.State["text"];
            readText = String.Join(" ", text);
            txt.Text = readText;
            read = new Reader();
            //startConv();
        }

        private async void startConv() 
        {
            await read.readText("May I reed the text for you? Tap the screen for yes");
        }

        // this method should listen for the user input: play, pause, restart, etc..
        private void ScreenDoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            throw new NotImplementedException();
        }

        // Play pause the running speech
        // if it's playing pause it
        // if it's paused, play it.
        private async void ScreenTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (cts == null)
            {
                cts = new CancellationTokenSource();
                try
                {
                    ReadText(cts.Token);
                }
                catch (OperationCanceledException)
                {
                }
            }
            else
            {
                cts.Cancel();
                cts = null;
            }
        }


        private async Task ReadText(CancellationToken token)
        {
            for (; count < text.Count; count++)
            {
                token.ThrowIfCancellationRequested();
                await read.readText(text[count]);
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            NavigationService.RemoveBackEntry();
            NavigationService.GoBack();
        }
    }
}
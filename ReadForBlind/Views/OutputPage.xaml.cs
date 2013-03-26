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
        Reader reader;
        Listener listener;

        public OutputPage()
        {
            InitializeComponent();

            text = (List<String>)PhoneApplicationService.Current.State["text"];
            readText = String.Join(" ", text);
            txt.Text = readText;
            reader = new Reader();
            listener = new Listener();
            startConv();
        }

        private async void startConv() 
        {
            await reader.readText("May I reed the text for you? Tap the screen for yes");
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
            if (cts == null)
                PlayText();
            else
                PauseText();
        }

        private void PlayText() {
            cts = new CancellationTokenSource();
            try{
                ReadText(cts.Token);
            }
            catch (OperationCanceledException){}
        }

        private void PauseText() {
            cts.Cancel();
            cts = null;
        }
        private async Task ReadText(CancellationToken token)
        {
            for (; count < text.Count; count++)
            {
                token.ThrowIfCancellationRequested();
                await reader.readText(text[count]);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationService.RemoveBackEntry();
        }

        //protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        //{
        //    e.Cancel = true;
        //    NavigationService.RemoveBackEntry();
        //    NavigationService.GoBack();
        //}

        private async void OpenVoiceCommand(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //string result = await listener.Listen();
            //if (result != null)
            //{
            //    if (result.Contains("play") || result.Contains("start") || result.Contains("reed")) {
            //        PlayText();
            //    }
            //    else if (result.Contains("pause") || result.Contains("stop")) {
            //        PauseText();
            //    }
            //    else if (result.Contains("new") || result.Contains("photo")) {
            //        NavigationService.GoBack();
            //    }
            //}
            
        }
    }
}
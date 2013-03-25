using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Phone.Speech.Synthesis;
using Windows.Phone.Speech;
using System.Threading.Tasks;
using System.Threading;

namespace ReadForBlind.Views
{
    public partial class OutputPage : PhoneApplicationPage
    {
        private String readText;
        private SpeechSynthesizer synthesizer;
        volatile bool play = false;
        volatile bool playing = false;
        private int count = 0;
        private List<String> text;
        CancellationTokenSource cts = null;
        Reader read;

        public OutputPage()
        {
            InitializeComponent();

            text = (List<String>)PhoneApplicationService.Current.State["text"];
            readText = String.Join(" ", text);
            txt.Text = readText;
            read = new Reader();
            startConv();
        }

        private void startConv() 
        {
            read.readText("May I reed the text for you? Tap the screen for yes");
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
               
        }

        private async Task ReadText(CancellationToken token) 
        {
            
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            NavigationService.RemoveBackEntry();
            NavigationService.GoBack();
        }
    }
}
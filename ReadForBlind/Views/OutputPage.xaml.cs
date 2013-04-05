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
using Windows.Phone.Speech.Synthesis;


namespace ReadForBlind.Views
{
    /// <summary>
    /// Class for the Output page for the app
    /// </summary>
    public partial class OutputPage : PhoneApplicationPage
    {
        private String readText;
        int count = 0, maxTextLength;
        List<String> text;
        Reader reader;
        Listener listener;
        SpeechSynthesizer synth;
        bool playing = false;

        /// <summary>
        /// The constructor of the Output page
        /// </summary>
        public OutputPage()
        {
            InitializeComponent();
           
            text = (List<String>)PhoneApplicationService.Current.State["text"];
            readText = String.Join(" ", text);
            txt.Text = readText;
            reader = new Reader();
            listener = new Listener();
            maxTextLength = text.Count;
            startConv();
        }

        /// <summary>
        /// Read the first speech of the output screen
        /// </summary>
        private async void startConv()
        {
            reader.readText("May I reed the text for you? Tap the screen for yes");
        }

        /// <summary>
        /// Event handler when the screen is double tapped (used for play, pause and restart, etc.)
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments passed to the event handler when the screen is double tapped</param>
        private void ScreenDoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Event handler when screen is tapped (used for play/pause)
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments passed to the event handler when the screen is tapped</param>
        private void ScreenTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!playing)
                PlayText();
            else
                PauseText();
        }

        /// <summary>
        /// Plays the text
        /// </summary>
        private void PlayText()
        {
            reader.CancelAll();
            synth = new SpeechSynthesizer();
            synth.BookmarkReached += synth_BookmarkReached;
            try
            {
                synth.SpeakSsmlAsync(makeSSML());
            }
            catch (System.FormatException e)
            {
                reader.readText("this text cannot be read");
                return;
            }
            
            playing = true;
        }


        /// <summary>
        /// Makes an SSML of the text to be spoken
        /// </summary>
        /// <returns>The SSML of the text</returns>
        private string makeSSML()
        {
            if (count == maxTextLength)
            {
                count = 0;
            }
            String s = "<speak version=\"1.0\" ";
            s += "xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\">";
            for (int i = count; i < maxTextLength; i++)
            {
                s += text[i];
                s += "<mark name=\"START\"/>";
            }
            s += "<mark name=\"END\"/>";
            s += "</speak>";
            return s;
        }

        /// <summary>
        /// Event handler when a bookmark is reached
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments passed to the event handler when bookmark is reached</param>
        private async void synth_BookmarkReached(SpeechSynthesizer sender, SpeechBookmarkReachedEventArgs e)
        {
            count++;
            if (e.Bookmark == "END")
            {
                listener.PlaySpeechOff();
                playing = false;
                if (synth != null)
                    synth.Dispose();
            }
        }

        /// <summary>
        /// Pauses the text
        /// </summary>
        private void PauseText()
        {
            if (synth != null) synth.Dispose();
            playing = false;
        }

        /// <summary>
        /// Reads the text
        /// </summary>
        /// <param name="token">The cancelleation token</param>
        /// <returns>The task of reading the text</returns>
        private async Task ReadText(CancellationToken token)
        {
            for (; count < text.Count; count++)
            {
                token.ThrowIfCancellationRequested();
                await reader.readText(text[count]);
            }
        }

        /// <summary>
        /// Event handler when the app navigates away from the Output Page
        /// </summary>
        /// <param name="e">The arguments passed to the event handler when the output page is navigated away from</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            try
            {
                synth.Dispose();
            }
            catch (Exception)
            {}
                
        }


        /// <summary>
        /// Event handler when the app navigates to the output page
        /// </summary>
        /// <param name="e">The arguments passed to the event handler when the app navigated to the output page</param>
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

        /// <summary>
        /// Event handler to open the voice commands for the page
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments passed to the event handler when the voice command is opened</param>
        private async void OpenVoiceCommand(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string result = await listener.Listen();
            if (result != null)
            {
                if (result.Contains("play") || result.Contains("start") || result.Contains("reed"))
                {
                    PlayText();
                }
                else if (result.Contains("pause") || result.Contains("stop"))
                {
                    PauseText();
                }
                else if (result.Contains("new") || result.Contains("photo"))
                { 
                    NavigationService.GoBack();
                    //NavigationService.Navigate(new Uri("/Camera.xaml", UriKind.Relative));
                }
                else if (result.Contains("repeat") || result.Contains("restart"))
                {
                    count = 0;
                    PlayText();
                }
            }

        }
    }
}
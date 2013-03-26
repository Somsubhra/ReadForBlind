using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Phone.Speech.Recognition;

namespace ReadForBlind
{
    class Listener
    {
        private SpeechRecognizer listener;
        private Reader reader;

        public Listener() {
            listener = new SpeechRecognizer();
            reader = new Reader();
            loadGrammar();
        }

        private void loadGrammar() {
            string[] actions = { "play", "pause", "reed", "stop", "close", "exit", "quit", "start" };

            listener.AudioProblemOccurred += Recognizer_AudioProblemOccurred;
            listener.Grammars.AddGrammarFromList("actions", actions);
        }

        private async void Recognizer_AudioProblemOccurred(SpeechRecognizer sender, SpeechAudioProblemOccurredEventArgs args)
        {
            await reader.readText("Please speak clearly.");
        }

        public async Task<String> Listen() {
            Stream stream = TitleContainer.OpenStream("notify.wav");
            if (stream != null)
            {
                var effect = SoundEffect.FromStream(stream);
                FrameworkDispatcher.Update();
                effect.Play();
            }
            SpeechRecognitionResult result = await listener.RecognizeAsync();
            if (result.TextConfidence == SpeechRecognitionConfidence.High && result.Text.Length > 0)
                return IsBuiltIn(result.Text);
            else
                await reader.readText("Sorry but I didn't get you");
            return null;
        }

        private String IsBuiltIn(String txt) {
            txt = txt.ToLower();
            if (txt.Contains("quit") || txt.Contains("exit") || txt.Contains("close")) {
                Application.Current.Terminate();
            }
            return txt;
        }
    }
}

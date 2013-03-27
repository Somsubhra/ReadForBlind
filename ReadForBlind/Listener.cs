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

        public Listener()
        {
            listener = new SpeechRecognizer();
            reader = new Reader();
            loadGrammar();
        }

        private void loadGrammar()
        {
            string[] actions = { "play", "pause", "reed", "stop", "close", "exit", "quit", "start", "repeat", "restart" };

            listener.AudioProblemOccurred += Recognizer_AudioProblemOccurred;
            listener.Grammars.AddGrammarFromList("actions", actions);
        }

        private async void Recognizer_AudioProblemOccurred(SpeechRecognizer sender, SpeechAudioProblemOccurredEventArgs args)
        {
            await reader.readText("Please speak clearly.");
        }

        public async Task<String> Listen()
        {
            playSound();
            SpeechRecognitionResult result = await listener.RecognizeAsync();
            if (result.TextConfidence == SpeechRecognitionConfidence.High && result.Text.Length > 0)
                return IsBuiltIn(result.Text);
            else
                await reader.readText("Sorry but I didn't get you");
            return null;
        }

        public void playSound()
        {
            Stream stream = TitleContainer.OpenStream("Assets/notify.wav");
            if (stream != null)
            {
                var effect = SoundEffect.FromStream(stream);
                FrameworkDispatcher.Update();
                effect.Play();
            }
        }

        public void PlaySpeechOff()
        {
            Stream stream = TitleContainer.OpenStream("Assets/SpeechOff.wav");
            if (stream != null)
            {
                var effect = SoundEffect.FromStream(stream);
                FrameworkDispatcher.Update();
                effect.Play();
            }
        }

        private String IsBuiltIn(String txt)
        {
            txt = txt.ToLower();
            if (txt.Contains("quit") || txt.Contains("exit") || txt.Contains("close"))
            {
                Application.Current.Terminate();
            }
            return txt;
        }

        public async Task<String> ConversionFailedConfirmation()
        {
            playSound();
            SpeechRecognizer sp = new SpeechRecognizer();
            string[] confirm = { "yes", "no" };
            sp.Grammars.AddGrammarFromList("confirm", confirm);
            SpeechRecognitionResult result = await listener.RecognizeAsync();
            if (result.TextConfidence >= SpeechRecognitionConfidence.Medium && result.Text.Length > 0)
                return result.Text.ToLower();
            else
                await reader.readText("Sorry but I didn't get you");
            return null;
        }
    }
}

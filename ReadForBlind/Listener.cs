using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Speech.Recognition;

namespace ReadForBlind
{
    class Listener
    {
        private SpeechRecognizer listener;
        private SpeechRecognizerInformation englishRecognizer;

        public Listener() {
            listener = new SpeechRecognizer();
            this.configure();
        }

        private void configure() {
            englishRecognizer = InstalledSpeechRecognizers.All.FirstOrDefault(d => d.Language.ToUpper() == "EN-US");
            listener.SetRecognizer(englishRecognizer);
        }

        private void loadGrammar() {
            listener.Grammars.AddGrammarFromUri("Grammar-EN", new Uri("ms-appx:///grammar.xml", UriKind.Absolute));
        }
    }
}

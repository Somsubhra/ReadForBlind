using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Speech.Synthesis;

namespace ReadForBlind
{
    class Reader
    {
        private SpeechSynthesizer reader;

        public Reader() {
            reader = new SpeechSynthesizer();
        }

        public async void readText(string text) {
            await reader.SpeakTextAsync(text);
        }
    }
}

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

        public Listener() {
            listener = new SpeechRecognizer();
        }
    }
}

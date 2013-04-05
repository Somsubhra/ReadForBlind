using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Speech.Synthesis;

namespace ReadForBlind
{
    /// <summary>
    /// The wrapper class for speech generation
    /// </summary>
    class Reader
    {
        private SpeechSynthesizer reader;

        /// <summary>
        /// The constructor for the reader
        /// </summary>
        public Reader()
        {
            reader = new SpeechSynthesizer();
        }


        /// <summary>
        /// Read the specified text
        /// </summary>
        /// <param name="text">The text to be read</param>
        /// <returns>The task of reading the text</returns>
        public async Task readText(string text)
        {
            await reader.SpeakTextAsync(text);
        }

        /// <summary>
        /// Reads the welcome text
        /// </summary>
        public void readWelcomeText()
        {
            string welcomeText = "Welcome. You may now hold your phone over the text for me to reader it aloud.";
            readText(welcomeText);
        }


        /// <summary>
        /// Disposes the reader object
        /// </summary>
        public void Dispose() {
            reader.Dispose();
        }

        /// <summary>
        /// Cancels all the tasks of the reader object
        /// </summary>
        public void CancelAll() {
            reader.CancelAll();
        }
    }
}

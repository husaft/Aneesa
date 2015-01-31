using System;
using System.Configuration;
using System.Globalization;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;

namespace Aneesa.UI
{
    public partial class MainForm : Form
    {
        private SpeechRecognitionEngine recognizer;
        private SpeechSynthesizer synthesizer;
        private CultureInfo culture;
        private StringBuilder currentlySpoken;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var cultureTag = ConfigurationManager.AppSettings["culture"];
            culture = CultureInfo.GetCultureInfoByIetfLanguageTag(cultureTag);
            synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();
            synthesizer.SpeakStarted += Synthesizer_SpeakStarted;
            synthesizer.SpeakProgress += Synthesizer_SpeakProgress;
            synthesizer.SpeakCompleted += Synthesizer_SpeakCompleted;
            recognizer = new SpeechRecognitionEngine(culture);
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.LoadGrammar(new DictationGrammar());
            Speak(SpeakResource.Welcome);
        }

        private void Synthesizer_SpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            currentlySpoken = new StringBuilder();
            statusCircle.InnerColor = Brushes.LightCyan;
            statusCircle.OuterColor = Brushes.LightGray;
            statusCircle.Invalidate();
        }

        private void Synthesizer_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            var promptTxt = e.Prompt.GetText();
            var txt = promptTxt.Substring(e.CharacterPosition, e.CharacterCount + 1);
            currentlySpoken.Append(txt);
            statusText.Text = currentlySpoken.ToString();
            var last = txt.Last();
            if (last == '!' || last == '.' || last == ';' || last == ':')
                currentlySpoken.Clear();
        }

        private void Synthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            currentlySpoken.Clear();
            statusCircle.InnerColor = Brushes.White;
            statusCircle.OuterColor = Brushes.LightBlue;
            statusCircle.Invalidate();
        }

        private void Speak(string text)
        {
            var prompt = new PromptBuilder(culture);
            prompt.StartSentence(culture);
            prompt.AppendTextWithHint(text, SayAs.Text);
            prompt.EndSentence();
            synthesizer.SpeakAsync(prompt);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            culture = null;
            synthesizer.Dispose();
            synthesizer = null;
            recognizer.Dispose();
            recognizer = null;
        }
    }
}
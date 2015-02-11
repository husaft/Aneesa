using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Windows.Forms;

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
			recognizer.LoadGrammarCompleted += recognizer_LoadGrammarCompleted;
			recognizer.SpeechRecognized += recognizer_SpeechRecognized;
			recognizer.SpeechRecognitionRejected += recognizer_SpeechRecognitionRejected;
			// Get all grammar files and load them
			var currentDir = Environment.CurrentDirectory;
			var grammarFiles = Directory.GetFiles(currentDir, "*Grammar.xml", SearchOption.AllDirectories);
			Array.ForEach(grammarFiles, f => recognizer.LoadGrammarAsync(new Grammar(f)));
			// Greet the user
			Speak(SpeakResource.Welcome);
		}

		private void recognizer_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
		{
			Speak(SpeakResource.Sorry);
		}

		private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
		{
			OnSpeechRecognized(e.Result);
		}
		
		private void OnSpeechRecognized(RecognitionResult result)
		{
			var ruleName = result.Grammar.RuleName;
			switch (ruleName) {
				case "TellClock":
					TellClock();
					break;
				case "TellName":
					TellName();
					break;
				default:
					Speak(SpeakResource.Sorry);
					break;
			}
		}
		
		private void TellName()
		{
			Speak(SpeakResource.AboutMe);
		}
		
		private void TellClock()
		{
			Speak("It is "+DateTime.Now.ToString("HH:mm")+"!", SayAs.Time24);
		}

		private void recognizer_LoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
		{
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
			// Start recognize
			if (recognizer == null)
				return;
			recognizer.RecognizeAsync(RecognizeMode.Single);
		}

		private void Speak(string text, SayAs mode = SayAs.Text)
		{
			var prompt = new PromptBuilder(culture);
			prompt.StartSentence(culture);
			prompt.AppendTextWithHint(text, mode);
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
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Speech.Synthesis;
using System.Text;
using System.Windows.Forms;

using Aneesa.Util;
using Aneesa.Win;

namespace Aneesa.UI
{
	public partial class MainForm : Form
	{
		private SpeechRecognitionEngine recognizer;
		private SpeechSynthesizer synthesizer;
		private CultureInfo culture;
		private StringBuilder currentlySpoken;
		private StartMenuGrammar smg;
		private WinSearchGrammar wsg;
		
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
			// Get all grammar files and read them
			var currentDir = Environment.CurrentDirectory;
			var grammarFiles = Directory.GetFiles(currentDir, "*.cg.xml", SearchOption.AllDirectories);
			var srgsDocs = grammarFiles.Select(f => new SrgsDocument(f)).ToArray();
			// Extract all rules
			var allRules = srgsDocs.SelectMany(d => d.Rules);
			var privRules = allRules.Where(r => r.Scope == SrgsRuleScope.Private).ToArray();
			// Build grammars and load them
			var grammars = srgsDocs.SelectMany(d => d.Rules.Where(r => r.Scope == SrgsRuleScope.Public)
			                                   .Select(pr => new Grammar(d, pr.Id))).ToArray();
			Array.ForEach(grammars, g => recognizer.LoadGrammarAsync(g));
			// Generate on-the-fly grammars
			smg = new StartMenuGrammar();		
			wsg = new WinSearchGrammar();
			var onTheFly = smg.GenerateGrammar().Concat(wsg.GenerateGrammar()).ToArray();
			Array.ForEach(onTheFly, d => recognizer.LoadGrammarAsync(new Grammar(d)));
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
				case "TellLogon":
					TellLogon();
					break;
				case "TellRunning":
					TellRunning();
					break;
				case "TellWinVer":
					TellWinVer();
					break;
				case "TellCPUs":
					TellCPUs();
					break;
				case "TellSpace":
					TellSpace();
					break;
				case "TellUser":
					TellUser();
					break;
				default:
					if (smg.RuleNames.Contains(ruleName))
					{
						smg.OnRecognize(ruleName, result.Text, s => Speak(s));
						break;
					}
					if (wsg.RuleNames.Contains(ruleName))
					{
						wsg.OnRecognize(ruleName, result.Text, s => Speak(s));
						break;
					}
					Speak(SpeakResource.Sorry);
					break;
			}
		}
		
		private void TellUser()
		{
			var machine = Environment.MachineName;
			var identity = WindowsIdentity.GetCurrent().Name.Replace(machine+"\\", string.Empty);
			Speak(string.Format("You're logged in as {0} on {1}.", identity, machine));
		}
		
		#region Disk space
		private void TellSpace()
		{
			var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed).ToArray();
			var freeSpace = drives.Sum(d => d.AvailableFreeSpace);
			var diskSpace = drives.Sum(d => d.TotalSize);
			Speak(string.Format("You've got {0} and {1} of it are free.",
			                    GetHumanSpace(diskSpace, true), GetHumanSpace(freeSpace, true)));
		}
		
		private static string GetHumanSpace(long bytes, bool longFormat)
		{
			string[] sizes;
			if (longFormat)
				sizes = new [] { "bytes", "kilobytes", "megabytes", "gigabytes", "terabytes" };
			else
				sizes = new [] { "B", "KB", "MB", "GB", "TB" };
			long tmp;
			int order = 0;
			while ((tmp = bytes / 1024L) > 0L)
			{
				bytes = tmp;
				order++;
			}
			return string.Format("{0:0.##} {1}", bytes, sizes[order]);
		}
		#endregion
		
		private void TellCPUs()
		{
			var processors = Environment.ProcessorCount;
			Speak(string.Format("I'm using {0} CPUs.", processors));
		}
		
		private void TellWinVer()
		{
			var winver = Environment.OSVersion.VersionString;
			Speak(string.Format("You've got {0}.", winver), SayAs.NumberOrdinal);
		}
		
		#region Logon & running
		private void TellLogon()
		{
			var date = GetLogonTime().ToString("f", culture);
			Speak("Your computer started at "+date+".");
		}
		
		private void TellRunning()
		{
			var time = CalcPcRunningTime();
			Speak(string.Format("Your computer is running since {0} hours and {1} minutes.", time.Hours, time.Minutes));
		}
		
		private static TimeSpan CalcPcRunningTime()
		{
			return DateTime.Now - GetLogonTime();
		}
		
		private static DateTime GetLogonTime()
		{
			return Process.GetProcesses().Select(p => SafeHelpers.GetStartTimeSafe(p)).Min().GetValueOrDefault();
		}
		#endregion
		
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
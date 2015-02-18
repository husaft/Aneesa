
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition.SrgsGrammar;
using System.Windows.Forms;

using Aneesa.API;

namespace Aneesa.UI
{
	public class TimerGrammar : IRuntimeGrammar, IDisposable
	{
		private readonly int interval;
		
		private Timer timer;
		private int seconds;
		private Action<string> speaker;
		
		public TimerGrammar()
		{
			interval = 1000;
			seconds = 0;
			timer = new Timer();
			timer.Tick += timer_Tick;
			timer.Interval = interval;
			RuleNames = new [] { "SetTimer" };
		}
		
		private void timer_Tick(object sender, EventArgs e)
		{
			seconds--;
			if (seconds > 0)
				return;
			timer.Stop();
			timer.Enabled = false;
			OnAlarm();
		}
		
		public IEnumerable<string> RuleNames { get; private set; }
		
		public IEnumerable<SrgsDocument> GenerateGrammar()
		{
			return Enumerable.Empty<SrgsDocument>();
		}
		
		private void OnAlarm()
		{
			speaker(string.Format("Your alarm is done."));
		}
		
		public void OnRecognize(string ruleName, string text, Action<string> speaker)
		{
			if (ruleName == "SetTimer")
			{
				var duration = TimeSpan.Zero;
				if (text.Contains("two"))
				{
					duration = TimeSpan.FromMinutes(2);
				}
				if (text.Contains("five"))
				{
					duration = TimeSpan.FromMinutes(5);
				}
				if (text.Contains("ten"))
				{
					duration = TimeSpan.FromMinutes(10);
				}
				this.speaker = speaker;
				seconds = (int)duration.TotalSeconds;
				timer.Enabled = true;
				timer.Start();
				speaker(string.Format("Sure, I will remind you in {0} minutes.",
				                      (int)duration.TotalMinutes));
				return;
			}
		}
		
		public void Dispose()
		{
			if (timer == null)
				return;
			timer.Stop();
			timer.Dispose();
			timer = null;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Recognition.SrgsGrammar;

using Aneesa.API;
using Aneesa.Util;
using IWshRuntimeLibrary;

namespace Aneesa.Win
{
	public class StartMenuGrammar : IRuntimeGrammar
	{
		private readonly WshShell shell;
		private readonly string openPrefix;
		private readonly string closePrefix;
		
		private IDictionary<string, string> lnkDict;
		
		public StartMenuGrammar()
		{
			shell = new WshShell();
			openPrefix = "Open ";
			closePrefix = "Close ";
			RuleNames = new [] { "OpenApp", "CloseApp" };
		}
		
		public IEnumerable<SrgsDocument> GenerateGrammar()
		{
			// Get all interesting special folders
			var myStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
			var cmStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
			var myDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			var cmDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
			// Search all link files
			var allFolders = new [] { myStartMenu, cmStartMenu, myDesktop, cmDesktop };
			var lnkFiles = allFolders.SelectMany(f => Directory.GetFiles(f, "*.lnk", SearchOption.AllDirectories));
			// Extract information from link files
			var lnks = ExtractStartMenuLinks(shell, lnkFiles).Distinct();
			lnkDict = lnks.ToDictionary(k => k.Name, v => v.Path);
			// Create rule for opening
			var keys = lnkDict.Keys.OrderBy(k => k).ToArray();
			var oRoot = new SrgsRule(RuleNames.First(), new SrgsItem(openPrefix), new SrgsOneOf(keys));
			oRoot.Scope = SrgsRuleScope.Public;
			// Create rule for closing
			var cRoot = new SrgsRule(RuleNames.Last(), new SrgsItem(closePrefix), new SrgsOneOf(keys));
			cRoot.Scope = SrgsRuleScope.Public;
			// Create document for opening
			var oDoc = new SrgsDocument(oRoot);
			oDoc.Mode = SrgsGrammarMode.Voice;
			yield return oDoc;
			// Create document for closing
			var cDoc = new SrgsDocument(cRoot);
			cDoc.Mode = SrgsGrammarMode.Voice;
			yield return cDoc;
		}
		
		#region Helpers
		private IEnumerable<StartMenuLink> ExtractStartMenuLinks(WshShell shell, IEnumerable<string> files)
		{
			foreach (var file in files)
			{
				var link = (IWshShortcut)shell.CreateShortcut(file);
				var desc = GetDescriptionSafe(link);
				var path = link.TargetPath;
				var dir = link.WorkingDirectory;
				var args = link.Arguments;
				var name = Path.GetFileNameWithoutExtension(file);
				if (string.IsNullOrWhiteSpace(path))
					continue;
				yield return new StartMenuLink(name, path);
			}
		}
		
		private string GetDescriptionSafe(IWshShortcut link)
		{
			try
			{
				return link.Description;
			} catch (COMException) {
				return null;
			}
		}
		#endregion
		
		public IEnumerable<string> RuleNames { get; private set; }
		
		public void OnRecognize(string ruleName, string text, Action<string> speaker)
		{
			if (ruleName == RuleNames.First())
			{
				var app = text.Replace(openPrefix, string.Empty).Trim();
				string path;
				if (!lnkDict.TryGetValue(app, out path))
					return;
				Process.Start(path);
				speaker(string.Format("Started {0}.", app));
				return;
			}
			if (ruleName == RuleNames.Last())
			{
				var app = text.Replace(closePrefix, string.Empty).Trim();
				string path;
				if (!lnkDict.TryGetValue(app, out path))
					return;
				var proc = Process.GetProcesses().OrderByDescending(p => SafeHelpers.GetStartTimeSafe(p)).FirstOrDefault(
					p => {
						var mod = SafeHelpers.GetModulesSafe(p).FirstOrDefault();
						return mod == null ? false
							: mod.FileName == path ? true
							: Path.GetFileName(mod.FileName) == Path.GetFileName(path);
					});
				if (proc == null)
				{
					speaker(string.Format("Couldn't find {0}!", app));
					return;
				}
				SafeHelpers.TrySafe(() => proc.CloseMainWindow());
				SafeHelpers.TrySafe(() => proc.Close());
				SafeHelpers.TrySafe(() => proc.Kill());
				speaker(string.Format("Closed {0}.", app));
				return;
			}
		}
	}
}
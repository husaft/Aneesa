
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Recognition.SrgsGrammar;

using Aneesa.API;
using IWshRuntimeLibrary;

namespace Aneesa.Win
{
	public class StartMenuGrammar : IRuntimeGrammar
	{
		private readonly WshShell shell;
		private readonly string openPrefix;
		
		private IDictionary<string, string> lnkDict;
		
		public StartMenuGrammar()
		{
			shell = new WshShell();
			openPrefix = "Open ";
			RuleName = "OpenApp";
		}
		
		public SrgsDocument GenerateGrammar()
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
			// Create rule
			var keys = lnkDict.Keys.OrderBy(k => k).ToArray();
			var root = new SrgsRule(RuleName, new SrgsItem(openPrefix), new SrgsOneOf(keys));
			root.Scope = SrgsRuleScope.Public;
			// Create document
			var doc = new SrgsDocument(root);
			doc.Mode = SrgsGrammarMode.Voice;
			return doc;
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
		
		public string RuleName { get; private set; }
		
		public void OnRecognize(string text, Action<string> speaker)
		{
			var app = text.Replace(openPrefix, string.Empty).Trim();
			string path;
			if (!lnkDict.TryGetValue(app, out path))
				return;
			Process.Start(path);
			speaker(string.Format("Started {0} for you.", app));
		}
	}
}
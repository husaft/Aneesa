
using System;
using System.Collections.Generic;
using System.Speech.Recognition.SrgsGrammar;

namespace Aneesa.API
{
	public interface IRuntimeGrammar
	{
		IEnumerable<SrgsDocument> GenerateGrammar();
		
		IEnumerable<string> RuleNames { get; }
		
		void OnRecognize(string ruleName, string text, Action<string> speaker);
	}
}
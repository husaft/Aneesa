
using System;
using System.Collections.Generic;
using System.Speech.Recognition.SrgsGrammar;

namespace Aneesa.API
{
	public interface IRuntimeGrammar
	{
		SrgsDocument GenerateGrammar();
		
		string RuleName { get; }
		
		void OnRecognize(string text, Action<string> speaker);
	}
}
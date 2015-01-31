using System;
using System.Speech.Synthesis;
using System.Globalization;
using System.Configuration;

namespace Aneesa.Incubator
{
    class Program
    {
        static void Main(string[] args)
        {
            var synth = new SpeechSynthesizer();

            var cultureTag = ConfigurationManager.AppSettings["culture"];
            var culture = CultureInfo.GetCultureInfoByIetfLanguageTag(cultureTag);

            var prompt = new PromptBuilder(culture);
            prompt.StartSentence(culture);
            prompt.AppendTextWithHint(SpeakResources.Welcome, SayAs.Text);
            prompt.EndSentence();

            synth.SpeakAsync(prompt);
            
            Console.WriteLine("Hello!");
            Console.ReadLine();

            synth.Dispose();
        }
    }
}
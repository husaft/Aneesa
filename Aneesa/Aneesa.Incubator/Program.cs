using System;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Globalization;
using System.Configuration;

namespace Aneesa.Incubator
{
    class Program
    {
        static void Main(string[] args)
        {
            var synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();

            var cultureTag = ConfigurationManager.AppSettings["culture"];
            var culture = CultureInfo.GetCultureInfoByIetfLanguageTag(cultureTag);

            var prompt = new PromptBuilder(culture);
            prompt.StartSentence(culture);
            prompt.AppendTextWithHint(SpeakResources.Welcome, SayAs.Text);
            prompt.EndSentence();

            synth.SpeakAsync(prompt);

            var recog = new SpeechRecognitionEngine(culture);
            recog.LoadGrammar(new DictationGrammar());
            recog.SetInputToDefaultAudioDevice();
            recog.SpeechRecognized += Recog_SpeechRecognized;

            recog.RecognizeAsync(RecognizeMode.Multiple);

            Console.WriteLine("Hello!");
            Console.ReadLine();

            synth.Dispose();
            recog.Dispose();
        }

        private static void Recog_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            var recog = sender as SpeechRecognitionEngine;
            var args = e.Result;
            OnSpeechRecognized(recog, args);
        }

        private static void OnSpeechRecognized(SpeechRecognitionEngine engine, RecognitionResult result)
        {
            Console.WriteLine(">> " + result.Text + " / " + result.Confidence);
        }
    }
}
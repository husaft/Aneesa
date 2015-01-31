using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;
using System.Reflection;

namespace Aneesa.UI
{
    internal static class Extensions
    {
        public static string GetText(this Prompt prompt)
        {
            var field = prompt.GetType().GetField("_text", BindingFlags.Instance | BindingFlags.NonPublic);
            return (string)field.GetValue(prompt);
        }
    }
}
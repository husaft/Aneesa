
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Aneesa.Util
{
	public class SafeHelpers
	{
		public static IEnumerable<ProcessModule> GetModulesSafe(Process proc)
		{
			try
			{
				return proc.Modules.OfType<ProcessModule>();
			} catch (Win32Exception) {
				return Enumerable.Empty<ProcessModule>();
			}
		}
		
		public static DateTime? GetStartTimeSafe(Process proc)
		{
			try
			{
				return proc.StartTime;
			} catch (Win32Exception) {
				return null;
			}
		}
		
		public static Exception TrySafe(Action action)
		{
			try 
			{
				action();
				return null;
			} catch (Exception e) {
				return e;
			}
		}
	}
}
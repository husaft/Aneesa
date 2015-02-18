using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aneesa.Util
{
	public static class Extensions
	{
		
		public static IEnumerable<T> GetCustomAttrs<T>(this MemberInfo info)
		{
			return info.GetCustomAttributes(typeof(T), true).OfType<T>();
		}
		
		public static T GetCustomAttr<T>(this MemberInfo info)
		{
			return GetCustomAttrs<T>(info).FirstOrDefault();
		}
		
	}
}
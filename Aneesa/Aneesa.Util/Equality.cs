
using System;
using System.Collections.Generic;

namespace Aneesa.Util
{
	public static class Equality
	{
		public static LinqEqualityComparer<T,K> Get<T,K>(Func<T,K> keySelector)
		{
			return new LinqEqualityComparer<T,K>(keySelector);
		}
	}
}
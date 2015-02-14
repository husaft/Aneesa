
using System;
using System.Collections.Generic;

namespace Aneesa.Util
{
	public class LinqEqualityComparer<T,K> : IEqualityComparer<T>
	{
		private readonly Func<T,K> keySelector;
		
		public LinqEqualityComparer(Func<T,K> keySelector)
		{
			this.keySelector = keySelector;
		}
		
		public bool Equals(T item1, T item2)
		{
			return keySelector(item1).Equals(keySelector(item2));
		}
		
		public int GetHashCode(T obj)
		{
			return keySelector(obj).GetHashCode();
		}
	}
}

using System;

namespace Aneesa.Win
{
	public class StartMenuLink
	{
		public StartMenuLink(string name, string path)
		{
			Name = name;
			Path = path;
		}
		
		public string Name { get;private set; }
		public string Path { get;private set; }
		
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			StartMenuLink other = obj as StartMenuLink;
			if (other == null)
				return false;
			return this.Name == other.Name && this.Path == other.Path;
		}
		
		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				if (Name != null)
					hashCode += 1000000007 * Name.GetHashCode();
				if (Path != null)
					hashCode += 1000000009 * Path.GetHashCode();
			}
			return hashCode;
		}
		
		public static bool operator ==(StartMenuLink lhs, StartMenuLink rhs)
		{
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
				return false;
			return lhs.Equals(rhs);
		}
		
		public static bool operator !=(StartMenuLink lhs, StartMenuLink rhs)
		{
			return !(lhs == rhs);
		}
		#endregion
	}
}
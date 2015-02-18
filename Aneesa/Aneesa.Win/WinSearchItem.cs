using System;
using System.Xml.Serialization;

namespace Aneesa.Win
{
	public class WinSearchItem
	{
		
		[XmlElement("System.ApplicationName")]
		public string ApplicationName { get; set; }
		
		[XmlElement("System.ItemName")]
		public string ItemName { get; set; }
		
		[XmlElement("System.Author")]
		public string[] Authors { get; set; }
		
		[XmlElement("System.DateAccessed")]
		public DateTime DateAccessed { get; set; }
		
		[XmlElement("System.DateCreated")]
		public DateTime DateCreated { get; set; }
		
		[XmlElement("System.DateImported")]
		public DateTime DateImported { get; set; }
		
		[XmlElement("System.DateModified")]
		public DateTime DateModified { get; set; }
		
		[XmlElement("System.FileExtension")]
		public string FileExtension { get; set; }
		
		[XmlElement("System.ItemType")]
		public string ItemType { get; set; }
		
		[XmlElement("System.FileOwner")]
		public string FileOwner { get; set; }
		
		[XmlElement("System.FileName")]
		public string FileName { get; set; }
		
		[XmlElement("System.ItemUrl")]
		public string ItemUrl { get; set; }
		
		[XmlElement("System.Kind")]
		public string[] Kind { get; set; }
		
		[XmlElement("System.Size")]
		public decimal Size { get; set; }
		
	}
}
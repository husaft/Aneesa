
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Speech.Recognition.SrgsGrammar;
using System.Text;
using System.Xml.Serialization;

using Aneesa.API;
using Aneesa.Util;

namespace Aneesa.Win
{
	public class WinSearchGrammar : IRuntimeGrammar, IDisposable
	{
		private OleDbConnection connection;
		
		public WinSearchGrammar()
		{
			var connectionString = "Provider=Search.CollatorDSO;Extended Properties=\"Application=Windows\"";
			connection = new OleDbConnection(connectionString);
			RuleNames = new [] { "ShowFiles" };
		}
		
		public IEnumerable<string> RuleNames { get; private set; }
		
		public IEnumerable<SrgsDocument> GenerateGrammar()
		{
			return Enumerable.Empty<SrgsDocument>();
		}
		
		#region Search index reading logic
		private static IEnumerable<T> RetrieveFromSearchIndex<T>(OleDbConnection conn,
		                                                         string author = null, string freetext = null,
		                                                         DateTime? before = null, DateTime? after = null)
		{
			var type = typeof(T);
			// Fetch properties
			var props = type.GetProperties().Select(
				p => Tuple.Create(p, p.GetCustomAttr<XmlElementAttribute>())).ToArray();
			// Define parameters
			var attrs = string.Join(", ", props.Select(p => p.Item2.ElementName));
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			var conds = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(author))
				conds.Append("and CONTAINS(System.Author, '\"" + author +"\"') ");
			if (!string.IsNullOrWhiteSpace(freetext))
				conds.Append("and FREETEXT('" + freetext + "') ");
			const string dateFmt = "yyyy-MM-dd";
			if (after != null)
				conds.Append("and System.DateModified >= '" + after.Value.ToString(dateFmt) + "' ");
			if (before != null)
				conds.Append("and System.DateModified <= '" + before.Value.ToString(dateFmt) + "' ");
			// Build necessary SQL
			var query = string.Format("SELECT {0} FROM SystemIndex WHERE scope = 'file:{1}' {2}",
			                          attrs, folder, conds);
			// Open if not already done
			if (conn.State != ConnectionState.Open)
				conn.Open();
			// Build command
			using (var cmd = new OleDbCommand(query, conn))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var obj = Activator.CreateInstance<T>();
						for (int i = 0; i < reader.FieldCount; i++)
						{
							var name = reader.GetName(i);
							var value = reader.GetValue(i);
							if (value is DBNull)
								value = null;
							var prop = props.FirstOrDefault(
								p => p.Item2.ElementName.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Item1;
							var targetType = prop.PropertyType;
							prop.SetValue(obj, value);
						}
						yield return obj;
					}
				}
			}
		}
		#endregion
		
		public void OnRecognize(string ruleName, string text, Action<string> speaker)
		{
			if (ruleName == "ShowFiles")
			{
				// That's today
				DateTime start = DateTime.Today;
				DateTime end = DateTime.Now;
				// Handle other time strings
				if (text.Contains("last month"))
				{
					start = new DateTime(end.Year, end.Month - 1, 1);
					end = new DateTime(end.Year, end.Month, 1).AddSeconds(-1);
				}
				if (text.Contains("last fortnight"))
				{
					start = end.AddDays(-14);
					end = start.AddDays(14).AddSeconds(-1);
				}
				if (text.Contains("last week"))
				{
					start = end.AddDays(-7);
					end = start.AddDays(7).AddSeconds(-1);
				}
				if (text.Contains("yesterday"))
				{
					start = start.AddDays(-1);
					end = start.AddDays(1).AddSeconds(-1);
				}
				// Search for items by date
				var items = RetrieveFromSearchIndex<WinSearchItem>(connection,
				                                                   after: start, before: end)
					.ToArray();
				if (items.Length == 0)
				{
					speaker("I have found nothing.");
					return;
				}
				if (items.Length > 5)
				{
					speaker(string.Format("I have found {0} items.", items.Length));
					return;
				}
				var myText = string.Join(", ", items.Select(i => i.ItemName));
				speaker("I've found: " + myText);
				return;
			}
		}
		
		public void Dispose()
		{
			if (connection == null)
				return;
			connection.Close();
			connection.Dispose();
			connection = null;
		}
	}
}
using LibraryDataBase.Services;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryDataBase.Models.DataBase
{
    internal class DataBase
	{
		public static MySqlConnection _connection = new MySqlConnection();
		public const string ConnectionString = "server=localhost;port=3306;username=webapi;password=!1N7XmccClyGXMOb;database=библиотечный фонд";
		public ConnectionState Status() => _connection.State;
		public bool Connect()
		{
			try
			{
				Log.WriteLine($"connecting...");
				_connection = new MySqlConnection(ConnectionString);
				_connection.Open();
				Log.WriteLine($"connect");
				return true;
			}
			catch { return false; }
		}
		
		public string[] GetTables()
		{
			List<string> tables = new List<string>();
			DataTable dt = _connection.GetSchema("Tables");
			foreach (DataRow row in dt.Rows)
			{
				string tablename = (string)row[2];
				tables.Add(tablename);
			}
			return tables.ToArray();
		}
		public DataTable SendQuery(string query)
		{
			Log.WriteLine($"[SQL QUERY] {query}", LogLevel.Warning);
			MySqlCommand command = new MySqlCommand(query, _connection);
			MySqlDataAdapter dtb = new MySqlDataAdapter();
			dtb.SelectCommand = command;
			DataTable dtable = new DataTable();
			dtb.Fill(dtable);
			return dtable;
		}

	}
}

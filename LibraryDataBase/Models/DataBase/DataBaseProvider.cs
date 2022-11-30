using LibraryDataBase.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryDataBase.Models.DataBase
{
	internal static class DataBaseProvider
	{
		static DataBase DB = new DataBase();
		public static void Connect()
		{
			if (DB.Status() == System.Data.ConnectionState.Open)
				return;

			if (DB.Connect())
			{
				MessageBoxHelper.ErrorShow($"Не удалось подключиться к базе данных!");
			}
		}

		public static DataTable SendQuery(string query)
		{
			Connect();
			return DB.SendQuery(query);
		}
	}
}

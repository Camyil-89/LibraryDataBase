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

			if (!DB.Connect())
			{
				MessageBoxHelper.ErrorShow($"Не удалось подключиться к базе данных!");
			}
		}
		public static string[] GetTables()
		{
			DB.Connect();
			return DB.GetTables();
		}
		public static string SqlGenerateInventoryNumber(int len)
		{
			Random random = new Random();
			string inventory_number = "";
			while (true)
			{
				inventory_number = "";
				for (int i = 0; i < len; i++) inventory_number += random.Next(0, 9);
				var answer = SendQuery($"SELECT * FROM `хранилище книг` WHERE `хранилище книг`.`инвентарный номер` = {inventory_number};");
				if (answer.Rows.Count == 0) break;
			}
			return inventory_number;
		}
		public static DataTable SendQuery(string query)
		{
			Connect();
			return DB.SendQuery(query);
		}
	}
}

using LibraryDataBase.Base.Command;
using LibraryDataBase.Models.DataBase;
using LibraryDataBase.Services;
using LibraryDataBase.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace LibraryDataBase.ViewModels
{

	internal class MainVM : Base.ViewModel.BaseViewModel
	{
		public MainVM()
		{
			#region Commands
			#endregion

			LoadTables();
		}


		#region FindText: Description
		/// <summary>Description</summary>
		private string _FindText;
		/// <summary>Description</summary>
		public string FindText { get => _FindText; set => Set(ref _FindText, value); }
		#endregion

		#region SelectedIndexRow: Description
		/// <summary>Description</summary>
		private int _SelectedIndexRow = -1;
		/// <summary>Description</summary>
		public int SelectedIndexRow { get => _SelectedIndexRow; set => Set(ref _SelectedIndexRow, value); }
		#endregion

		#region Table: Description
		/// <summary>Description</summary>
		private DataTable _Table;
		/// <summary>Description</summary>
		public DataTable Table { get => _Table; set => Set(ref _Table, value); }
		#endregion

		#region SelectedTable: Description
		/// <summary>Description</summary>
		private string _SelectedTable;
		/// <summary>Description</summary>
		public string SelectedTable
		{
			get => _SelectedTable; set
			{
				Set(ref _SelectedTable, value);
				UpdateTable();
			}
		}
		#endregion


		#region SNP: Description
		/// <summary>Description</summary>
		private string _SNP;
		/// <summary>Description</summary>
		public string SNP { get => _SNP; set => Set(ref _SNP, value); }
		#endregion


		#region TablesList: Description
		/// <summary>Description</summary>
		private ObservableCollection<string> _TablesList = new ObservableCollection<string>();
		/// <summary>Description</summary>
		public ObservableCollection<string> TablesList { get => _TablesList; set => Set(ref _TablesList, value); }
		#endregion

		#region Parametrs
		#endregion

		#region Commands


		#region FindCommand: Description
		private ICommand _FindCommand;
		public ICommand FindCommand => _FindCommand ??= new LambdaCommand(OnFindCommandExecuted, CanFindCommandExecute);
		private bool CanFindCommandExecute(object e) => true;
		private void OnFindCommandExecuted(object e)
		{
			try
			{
				if (string.IsNullOrEmpty(FindText))
				{
					UpdateTable();
					return;
				}
				DataTable find = DataBaseProvider.SendQuery($"SELECT * FROM `{SelectedTable}`");
				List<DataRow> removes = new List<DataRow>();
				foreach (DataRow i in find.Rows)
				{
					if (!string.Join("", i.ItemArray).Contains(FindText))
						removes.Add(i);
				}
				foreach (DataRow i in removes)
					find.Rows.Remove(i);
				Table = find;
				Table.TableName = SelectedTable;
			}
			catch (Exception ex) { MessageBoxHelper.ErrorShow(ex.Message); }
		}
		#endregion

		#region UpdateCommand: Description
		private ICommand _UpdateCommand;
		public ICommand UpdateCommand => _UpdateCommand ??= new LambdaCommand(OnUpdateCommandExecuted, CanUpdateCommandExecute);
		private bool CanUpdateCommandExecute(object e) => true;
		private void OnUpdateCommandExecuted(object e)
		{

		}
		#endregion

		#region AddCommand: Description
		private ICommand _AddCommand;
		public ICommand AddCommand => _AddCommand ??= new LambdaCommand(OnAddCommandExecuted, CanAddCommandExecute);
		private bool CanAddCommandExecute(object e) => true;
		private void OnAddCommandExecuted(object e)
		{
			try
			{
				AddWindow window = new AddWindow();
				AddVM vm = new AddVM();
				vm.Window = window;
				vm.Table = Table;
				vm.Generate();

				if (SelectedTable == "категория читателей")
					vm.MinСompleted = 1;

				vm.VisibilytiTopBar = SelectedTable == "хранилище книг" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

				window.DataContext = vm;
				window.ShowDialog();
				if (vm.AddToDataBase)
					UpdateTable();
			}
			catch (Exception ex) { MessageBoxHelper.ErrorShow(ex.Message); }
		}
		#endregion

		#region ChangeCommand: Description
		private ICommand _ChangeCommand;
		public ICommand ChangeCommand => _ChangeCommand ??= new LambdaCommand(OnChangeCommandExecuted, CanChangeCommandExecute);
		private bool CanChangeCommandExecute(object e) => true;
		private void OnChangeCommandExecuted(object e)
		{
			if (SelectedIndexRow == -1)
				return;
			try
			{
				AddWindow window = new AddWindow();
				AddVM vm = new AddVM();
				vm.Window = window;
				vm.Table = Table;

				if (SelectedTable == "категория читателей")
					vm.MinСompleted = 1;

				vm.Title = "Изменение";
				vm.DataRow = Table.Rows[SelectedIndexRow];
				vm.VisibilytiTopBar = SelectedTable == "хранилище книг" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

				vm.Generate();
				window.DataContext = vm;
				window.ShowDialog();
				if (vm.AddToDataBase)
					UpdateTable();
			}
			catch (Exception ex) { MessageBoxHelper.ErrorShow(ex.Message); }
		}
		#endregion

		#region DeleteCommand: Description
		private ICommand _DeleteCommand;
		public ICommand DeleteCommand => _DeleteCommand ??= new LambdaCommand(OnDeleteCommandExecuted, CanDeleteCommandExecute);
		private bool CanDeleteCommandExecute(object e) => true;
		private void OnDeleteCommandExecuted(object e)
		{
			try
			{
				DataBaseProvider.SendQuery($"DELETE FROM `{SelectedTable}` WHERE `{Table.Columns[0].ColumnName}` = '{Table.Rows[SelectedIndexRow].ItemArray[0]}'");
				UpdateTable();
			}
			catch (Exception ex) { MessageBoxHelper.ErrorShow(ex.Message); }
		}
		#endregion
		#endregion

		#region Functions

		private void UpdateTable()
		{
			try
			{
				FindText = "";
				Table = DataBaseProvider.SendQuery($"SELECT * FROM `{SelectedTable}`");
				Table.TableName = SelectedTable;
			}
			catch (Exception ex) { MessageBoxHelper.ErrorShow(ex.Message); }
		}

		private void LoadTables()
		{
			try
			{
				foreach (var i in DataBaseProvider.GetTables())
					if (i != "users")
						TablesList.Add(i);
				SelectedTable = TablesList.First();
			}
			catch (Exception ex) { MessageBoxHelper.ErrorShow(ex.Message); }
		}
		#endregion
	}
}

using LibraryDataBase.Base.Command;
using LibraryDataBase.Models.DataBase;
using LibraryDataBase.Services;
using LibraryDataBase.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace LibraryDataBase.ViewModels
{
	internal class LoginVM: Base.ViewModel.BaseViewModel
	{


		#region Login: Description
		/// <summary>Description</summary>
		private string _Login = "admin";
		/// <summary>Description</summary>
		public string Login { get => _Login; set => Set(ref _Login, value); }
		#endregion


		#region Password: Description
		/// <summary>Description</summary>
		private string _Password = "admin";
		/// <summary>Description</summary>
		public string Password { get => _Password; set => Set(ref _Password, value); }
		#endregion

		#region LoginCommand: Description
		private ICommand _LoginCommand;
		public ICommand LoginCommand => _LoginCommand ??= new LambdaCommand(OnLoginCommandExecuted, CanLoginCommandExecute);
		private bool CanLoginCommandExecute(object e) => !string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(Password);
		private void OnLoginCommandExecuted(object e)
		{
			try
			{
				var users = DataBaseProvider.SendQuery($"SELECT * FROM `users` WHERE `login` = '{Login}' AND `password` = '{Password}';");
				if (users.Rows.Count == 0)
				{
					MessageBoxHelper.WarningShow("Такого аккаунта нету!");
					return;
				}
				App.Current.MainWindow.Hide();
				MainWindow mainWindow = new MainWindow();
				MainVM vm = new MainVM();

				vm.SNP = $"{users.Rows[0].ItemArray[3]} {users.Rows[0].ItemArray[4]} {users.Rows[0].ItemArray[5]}";

				mainWindow.DataContext = vm;

				mainWindow.Show();
				mainWindow.Closed += MainWindow_Closed;
			}
			catch (Exception ex)
			{
				MessageBoxHelper.ErrorShow(ex.Message);
				return;
			}
		}

		private void MainWindow_Closed(object? sender, EventArgs e)
		{
			App.Current.Shutdown();
		}
		#endregion


		#region RegistrationCommand: Description
		private ICommand _RegistrationCommand;
		public ICommand RegistrationCommand => _RegistrationCommand ??= new LambdaCommand(OnRegistrationCommandExecuted, CanRegistrationCommandExecute);
		private bool CanRegistrationCommandExecute(object e) => true;
		private void OnRegistrationCommandExecuted(object e)
		{
			RegistrationWindow window = new RegistrationWindow();
			RegistrationVM vm = new RegistrationVM();

			vm.Window = window;

			window.DataContext = vm;
			window.ShowDialog();

			if (vm.CreateAccount)
			{
				App.Current.MainWindow.Hide();
				MainWindow mainWindow = new MainWindow();
				MainVM vm1 = new MainVM();

				vm1.SNP = $"{vm.Surname} {vm.Name} {vm.Patronymic}";

				mainWindow.DataContext = vm1;

				mainWindow.Show();
				mainWindow.Closed += MainWindow_Closed;
			}
		}
		#endregion
	}
}

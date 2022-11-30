using LibraryDataBase.Base.Command;
using LibraryDataBase.Models.DataBase;
using LibraryDataBase.Services;
using LibraryDataBase.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibraryDataBase.ViewModels
{
	internal class RegistrationVM : Base.ViewModel.BaseViewModel
	{

		#region Window: Description
		/// <summary>Description</summary>
		private RegistrationWindow _Window;
		/// <summary>Description</summary>
		public RegistrationWindow Window { get => _Window; set => Set(ref _Window, value); }
		#endregion




		#region Login: Description
		/// <summary>Description</summary>
		private string _Login;
		/// <summary>Description</summary>
		public string Login { get => _Login; set => Set(ref _Login, value); }
		#endregion


		#region Password: Description
		/// <summary>Description</summary>
		private string _Password;
		/// <summary>Description</summary>
		public string Password { get => _Password; set => Set(ref _Password, value); }
		#endregion


		#region Name: Description
		/// <summary>Description</summary>
		private string _Name;
		/// <summary>Description</summary>
		public string Name { get => _Name; set => Set(ref _Name, value); }
		#endregion


		#region Surname: Description
		/// <summary>Description</summary>
		private string _Surname;
		/// <summary>Description</summary>
		public string Surname { get => _Surname; set => Set(ref _Surname, value); }
		#endregion


		#region Patronymic: Description
		/// <summary>Description</summary>
		private string _Patronymic;
		/// <summary>Description</summary>
		public string Patronymic { get => _Patronymic; set => Set(ref _Patronymic, value); }
		#endregion


		public bool CreateAccount = false;



		#region ExitCommand: Description
		private ICommand _ExitCommand;
		public ICommand ExitCommand => _ExitCommand ??= new LambdaCommand(OnExitCommandExecuted, CanExitCommandExecute);
		private bool CanExitCommandExecute(object e) => true;
		private void OnExitCommandExecuted(object e)
		{
			Window.Close();
		}
		#endregion

		#region CreateAccountCommand: Description
		private ICommand _CreateAccountCommand;
		public ICommand CreateAccountCommand => _CreateAccountCommand ??= new LambdaCommand(OnCreateAccountCommandExecuted, CanCreateAccountCommandExecute);
		private bool CanCreateAccountCommandExecute(object e) => !string.IsNullOrEmpty(Name) &&
			!string.IsNullOrEmpty(Surname) &&
			!string.IsNullOrEmpty(Patronymic) &&
			!string.IsNullOrEmpty(Login) &&
			!string.IsNullOrEmpty(Password);
		private void OnCreateAccountCommandExecuted(object e)
		{
			try
			{
				var users = DataBaseProvider.SendQuery($"SELECT * FROM `users` WHERE `login` = '{Login}';");
				if (users.Rows.Count > 0)
				{
					MessageBoxHelper.WarningShow("Такой логин уже занят!");
					return;
				}
				DataBaseProvider.SendQuery($"INSERT INTO `users` (`id`, `login`, `password`, `surname`, `name`, `patronymic`) VALUES (NULL, '{Login}', '{Password}', '{Surname}', '{Name}', '{Patronymic}');");
			}
			catch (Exception ex)
			{
				MessageBoxHelper.ErrorShow(ex.Message);
				return;
			}
			CreateAccount = true;
			Window.Close();
		}
		#endregion

	}
}

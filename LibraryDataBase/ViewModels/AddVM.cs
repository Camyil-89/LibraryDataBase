using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using LibraryDataBase.Base.Command;
using LibraryDataBase.Windows;
using LibraryDataBase.Models.DataBase;
using LibraryDataBase.Services;
using Org.BouncyCastle.Bcpg;
using System.Reflection;

namespace LibraryDataBase.ViewModels
{

	internal class AddVM : Base.ViewModel.BaseViewModel
	{
		public AddVM()
		{
			#region Commands
			#endregion
		}

		#region Parametrs

		private string field_sql = "";

		public DataRow DataRow;

		#region Title: Description
		/// <summary>Description</summary>
		private string _Title = "Добавление";
		/// <summary>Description</summary>
		public string Title { get => _Title; set => Set(ref _Title, value); }
		#endregion

		#region VisibilytiTopBar: Description
		/// <summary>Description</summary>
		private Visibility _VisibilytiTopBar = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilytiTopBar { get => _VisibilytiTopBar; set => Set(ref _VisibilytiTopBar, value); }
		#endregion

		public bool AddToDataBase = false;
		public int MinСompleted = -1;

		#region Window: Description
		/// <summary>Description</summary>
		private AddWindow _Window;
		/// <summary>Description</summary>
		public AddWindow Window { get => _Window; set => Set(ref _Window, value); }
		#endregion

		#region Table: Description
		/// <summary>Description</summary>
		private DataTable _Table;
		/// <summary>Description</summary>
		public DataTable Table { get => _Table; set => Set(ref _Table, value); }
		#endregion


		#region Blocks: Description
		/// <summary>Description</summary>
		private ObservableCollection<StackPanel> _Blocks = new ObservableCollection<StackPanel>();
		/// <summary>Description</summary>
		public ObservableCollection<StackPanel> Blocks { get => _Blocks; set => Set(ref _Blocks, value); }
		#endregion

		#endregion

		#region Commands


		#region ExitCommand: Description
		private ICommand _ExitCommand;
		public ICommand ExitCommand => _ExitCommand ??= new LambdaCommand(OnExitCommandExecuted, CanExitCommandExecute);
		private bool CanExitCommandExecute(object e) => true;
		private void OnExitCommandExecuted(object e)
		{
			Window.Close();
		}
		#endregion


		#region GenerateNumberCommand: Description
		private ICommand _GenerateNumberCommand;
		public ICommand GenerateNumberCommand => _GenerateNumberCommand ??= new LambdaCommand(OnGenerateNumberCommandExecuted, CanGenerateNumberCommandExecute);
		private bool CanGenerateNumberCommandExecute(object e) => true;
		private void OnGenerateNumberCommandExecuted(object e)
		{
			try
			{
				foreach (var panel in Blocks)
				{
					foreach (UIElement child in panel.Children)
					{
						if (child.GetType() == typeof(TextBox) && (child as TextBox).Tag.ToString() == "инвентарный номер")
						{
							(child as TextBox).Text = DataBaseProvider.SqlGenerateInventoryNumber(8);
						}
					}
				}
			}
			catch (Exception ex) { MessageBoxHelper.ErrorShow(ex.Message); }
		}
		#endregion

		private void UpdateSql()
		{
			//UPDATE `хранилище книг` SET `ID_книга` = '3', `дата получения` = '2022-07-12' WHERE `хранилище книг`.`ID_хранящейся книга` = 16;
			string sql = $"UPDATE `{Table.TableName}` SET ";
			int count = 0;
			int column = 0;
			foreach (var panel in Blocks)
			{
				foreach (UIElement child in panel.Children)
				{
					if (child.GetType() == typeof(TextBox))
					{
						column++;
						if ((child as TextBox).Text == "")
						{
							sql += $"`{Table.Columns[column].ColumnName}` = '',";
							continue;
						}
						count++;
						sql += $"`{Table.Columns[column].ColumnName}` = '{(child as TextBox).Text}',";
					}
					else if (child.GetType() == typeof(ComboBox))
					{
						column++;
						if ((child as ComboBox).SelectedItem == null)
						{
							sql += $"`{Table.Columns[column].ColumnName}` = 'NULL',";
							continue;
						}
						count++;
						sql += $"`{Table.Columns[column].ColumnName}` = '{(child as ComboBox).SelectedItem.ToString().Split('|')[0].Trim()}',";
					}
					else if (child.GetType() == typeof(CheckBox))
					{
						column++;
						count++;
						int x = 0;
						if ((child as CheckBox).IsChecked == null)
							sql += $"`{Table.Columns[column].ColumnName}` = '0',";
						else
						{
							x = (bool)(child as CheckBox).IsChecked ? 1 : 0;
							sql += $"`{Table.Columns[column].ColumnName}` = '{x}',";
						}
					}
					else if (child.GetType() == typeof(DatePicker))
					{
						column++;
						count++;
						if ((child as DatePicker).SelectedDate == null)
						{
							if (MessageBoxHelper.QuestionShow($"Вы уверены что хотите оставить не заполненой дату в поле \"{(child as DatePicker).Tag}\"") != MessageBoxResult.Yes)
								return;
							sql += $"`{Table.Columns[column].ColumnName}` = 'NULL',";
							continue;
						}
						sql += $"`{Table.Columns[column].ColumnName}` = '{(child as DatePicker).SelectedDate.Value.Year}-{(child as DatePicker).SelectedDate.Value.Month}-{(child as DatePicker).SelectedDate.Value.Day}',";
					}
				}
			}
			sql = sql.Remove(sql.Length - 1);
			sql += $" WHERE `{Table.TableName}`.`{Table.Columns[0].ColumnName}` = '{DataRow.ItemArray[0]}'";
			if (MinСompleted == -1 && count != Table.Columns.Count - 1)
			{
				MessageBox.Show("У вас не все поля заполнены!", "Уведомление");
				return;
			}
			else if (count < MinСompleted)
			{
				MessageBox.Show("У вас не все поля заполнены!", "Уведомление");
				return;
			}
			try
			{
				DataBaseProvider.SendQuery(sql);
				AddToDataBase = true;
				Window.Close();
			}
			catch (Exception ex) { MessageBoxHelper.ErrorShow(ex.Message); }
		}
		#region AddCommand: Description
		private ICommand _AddCommand;
		public ICommand AddCommand => _AddCommand ??= new LambdaCommand(OnAddCommandExecuted, CanAddCommandExecute);
		private bool CanAddCommandExecute(object e) => true;
		private void OnAddCommandExecuted(object e)
		{
			if (DataRow != null)
			{
				UpdateSql();
				return;
			}
			//INSERT INTO `книги` (`ID_книга`, `Название`, `Рейтинг`, `ID_автор`, `ID_тип`) VALUES (NULL, 'test', '4', '2', '1');
			string sql = $"INSERT INTO `{Table.TableName}` {field_sql} VALUES (NULL ";
			int count = 0;
			foreach (var panel in Blocks)
			{
				foreach (UIElement child in panel.Children)
				{
					if (child.GetType() == typeof(TextBox))
					{
						if ((child as TextBox).Text == "")
						{
							sql += $",''";
							continue;
						}
						count++;
						sql += $",'{(child as TextBox).Text}'";
					}
					else if (child.GetType() == typeof(ComboBox))
					{
						if ((child as ComboBox).SelectedItem == null)
						{
							sql += $",NULL";
							continue;
						}
						count++;
						sql += $",'{(child as ComboBox).SelectedItem.ToString().Split('|')[0].Trim()}'";
					}
					else if (child.GetType() == typeof(CheckBox))
					{
						count++;
						int x = 0;
						if ((child as CheckBox).IsChecked == null)
							sql += $",'0'";
						else
						{
							x = (bool)(child as CheckBox).IsChecked ? 1 : 0;
							sql += $",'{x}'";
						}
					}
					else if (child.GetType() == typeof(DatePicker))
					{
						count++;
						if ((child as DatePicker).SelectedDate == null)
						{
							if (MessageBoxHelper.QuestionShow($"Вы уверены что хотите оставить не заполненой дату в поле \"{(child as DatePicker).Tag}\"") != MessageBoxResult.Yes)
								return;
							sql += $",NULL";
							continue;
						}
						sql += $",'{(child as DatePicker).SelectedDate.Value.Year}-{(child as DatePicker).SelectedDate.Value.Month}-{(child as DatePicker).SelectedDate.Value.Day}'";
					}
				}
			}
			sql += ");";

			if (MinСompleted == -1 && count != Table.Columns.Count - 1)
			{
				MessageBox.Show("У вас не все поля заполнены!", "Уведомление");
				return;
			}
			else if (count < MinСompleted)
			{
				MessageBox.Show("У вас не все поля заполнены!", "Уведомление");
				return;
			}
			try
			{
				DataBaseProvider.SendQuery(sql);
				AddToDataBase = true;
				Window.Close();
			}
			catch (Exception ex) { MessageBoxHelper.ErrorShow(ex.Message); }
		}
		#endregion
		#endregion

		#region Functions
		public void Generate()
		{
			field_sql = "(";
			bool first = true;
			foreach (DataColumn i in Table.Columns)
			{
				if (first)
				{
					first = false;
					field_sql += $"`{i.ColumnName}`";
					continue;
				}
				else
					field_sql += $",`{i.ColumnName}`";


				if (i.ColumnName == "ID_тип" && i.Table.TableName != "тип книги")
					CreateComboBox(i.ColumnName, $"SELECT * FROM `тип книги`");

				else if (i.ColumnName == "ID_книга" && i.Table.TableName != "книги")
					CreateComboBox(i.ColumnName, $"SELECT `книги`.`ID_книга`, `книги`.`Название`, CONCAT( `авторы`.`Фамилия`, ' ', `авторы`.`Имя`, ' ', `авторы`.`Отчество` ) AS ФИО FROM `книги` INNER JOIN `авторы` ON `авторы`.`ID_автор` = `книги`.`ID_автор`;");

				else if (i.ColumnName == "ID_полка" && i.Table.TableName != "полка")
					CreateComboBox(i.ColumnName, $"SELECT `полка`.`ID_полка`, `стелаж`.`ID_стелаж`, `читательский зал`.`ID_читательский зал`, `библиотеки`.`Название` FROM `полка` INNER JOIN `стелаж` ON `стелаж`.`ID_стелаж` = `полка`.`ID_стелаж` INNER JOIN `читательский зал` ON `читательский зал`.`ID_читательский зал` = `стелаж`.`ID_читательский зал` INNER JOIN `библиотеки` ON `библиотеки`.`ID_библиотеки` = `читательский зал`.`ID_библиотеки`;");

				else if (i.ColumnName == "ID_автор" && i.Table.TableName != "авторы")
					CreateComboBox(i.ColumnName, $"SELECT `ID_автор`, CONCAT(`Фамилия`, ' ', `Имя`, ' ', `Отчество`) AS ФИО FROM `авторы`");

				else if (i.ColumnName == "ID_зарегистрированного читателя" && i.Table.TableName != "зарегистрированные читатели")
					CreateComboBox(i.ColumnName, $"SELECT `зарегистрированные читатели`.`ID_зарегистрированного читателя`, `читатели`.`ID_читатель`, CONCAT(`читатели`.`Фамилия`, ' ', `читатели`.`Имя`, ' ', `читатели`.`Отчество`) AS ФИО, CONCAT('УЗ- ', `категория читателей`.`Учебное заведение`, ' Г- ', `категория читателей`.`Группа`, ' Ш- ', `категория читателей`.`Школа`, ' К- ', `категория читателей`.`Класс` , ' НР - ', `категория читателей`.`Научная работа`, ' П -', `категория читателей`.`пенсионер`) AS КАТЕГОРИЯ FROM `зарегистрированные читатели` INNER JOIN `читатели` ON `читатели`.`ID_читатель` = `зарегистрированные читатели`.`ID_читатель` INNER JOIN `категория читателей` ON `категория читателей`.`ID_категории` = `читатели`.`ID_категории`;");

				else if (i.ColumnName == "ID_хранящейся книга" && i.Table.TableName != "хранилище книг")
					CreateComboBox(i.ColumnName, $"SELECT `хранилище книг`.`ID_хранящейся книга`, `хранилище книг`.`инвентарный номер`, `книги`.`Название`, CONCAT( `авторы`.`Фамилия`, ' ', `авторы`.`Имя`, ' ', `авторы`.`Отчество` ) AS ФИО FROM `хранилище книг` INNER JOIN `книги` ON `книги`.`ID_книга` = `хранилище книг`.`ID_книга` INNER JOIN `авторы` ON `авторы`.`ID_автор` = `книги`.`ID_автор`;");

				else if (i.ColumnName == "ID_категории" && i.Table.TableName != "категория читателей")
					CreateComboBox(i.ColumnName, $"SELECT `категория читателей`.`ID_категории`, CONCAT( 'УЗ- ', `категория читателей`.`Учебное заведение`, ' Г- ', `категория читателей`.`Группа`, ' Ш- ', `категория читателей`.`Школа`, ' К- ', `категория читателей`.`Класс`, ' НР - ', `категория читателей`.`Научная работа`, ' П -', `категория читателей`.`пенсионер` ) AS КАТЕГОРИЯ FROM `категория читателей`;");

				else if (i.ColumnName == "ID_библиотеки" && i.Table.TableName != "библиотеки")
					CreateComboBox(i.ColumnName, $"SELECT `ID_библиотеки`, `Название`, `Адрес` FROM `библиотеки`;");

				else if (i.ColumnName == "ID_автора" && i.Table.TableName != "авторы")
					CreateComboBox(i.ColumnName, $"SELECT `ID_автор`, CONCAT( `авторы`.`Фамилия`, ' ', `авторы`.`Имя`, ' ', `авторы`.`Отчество` ) AS ФИО FROM `авторы`;");

				else if (i.ColumnName == "ID_жанра" && i.Table.TableName != "жанры")
					CreateComboBox(i.ColumnName, $"SELECT * FROM `жанры`;");

				else if (i.ColumnName == "ID_читатель" && i.Table.TableName != "читатели")
					CreateComboBox(i.ColumnName, $"SELECT `ID_читатель`, CONCAT( `Фамилия`, ' ', `Имя`, ' ', `Отчество` ) AS ФИО FROM `читатели`;");

				else if (i.ColumnName == "ID_абонемент" && i.Table.TableName != "абонементы")
					CreateComboBox(i.ColumnName, $"SELECT `ID_абонемент`, CONCAT( 'Читательский зал - ', `читательский зал`, ' Вне библиотеки - ', `вне библиотеки` ) AS абонемент FROM `абонементы`;");

				else if (i.ColumnName == "ID_работника" && i.Table.TableName != "работники")
					CreateComboBox(i.ColumnName, $"SELECT `работники`.`ID_работника`, `работники`.`ФИО`, `должности`.`название`, `библиотеки`.`Название`, `библиотеки`.`Адрес` FROM `работники` INNER JOIN `читательский зал` ON `читательский зал`.`ID_читательский зал` = `работники`.`ID_читательский зал` INNER JOIN `библиотеки` ON `библиотеки`.`ID_библиотеки` = `читательский зал`.`ID_библиотеки` INNER JOIN `должности` ON `должности`.`ID_должность` = `работники`.`ID_должность`;");

				else if (i.ColumnName == "ID_стелаж" && i.Table.TableName != "стелаж")
					CreateComboBox(i.ColumnName, $"SELECT `стелаж`.`ID_стелаж`, `читательский зал`.`ID_читательский зал`, `библиотеки`.`Название`, `библиотеки`.`Адрес` FROM `стелаж` INNER JOIN `читательский зал` ON `читательский зал`.`ID_читательский зал` = `стелаж`.`ID_читательский зал` INNER JOIN `библиотеки` ON `библиотеки`.`ID_библиотеки` = `читательский зал`.`ID_библиотеки`;");

				else if (i.ColumnName == "ID_обязаность" && i.Table.TableName != "обязанности")
					CreateComboBox(i.ColumnName, $"SELECT * FROM `обязанности`;");

				else if (i.ColumnName == "ID_читательский зал" && i.Table.TableName != "читательский зал")
					CreateComboBox(i.ColumnName, $"SELECT `читательский зал`.`ID_читательский зал`, `библиотеки`.`Название`, `библиотеки`.`Адрес` FROM `читательский зал` INNER JOIN `библиотеки` ON `библиотеки`.`ID_библиотеки` = `читательский зал`.`ID_библиотеки`;");

				else if (i.ColumnName == "ID_должность" && i.Table.TableName != "должности")
					CreateComboBox(i.ColumnName, $"SELECT * FROM `должности`;");

				else if (i.DataType == typeof(System.Boolean))
					CreateCheckBox(i.ColumnName);

				else if (i.DataType == typeof(DateTime))
					CreateDatePicker(i.ColumnName);

				else
					CreateTextBox(i.ColumnName);
			}
			field_sql += ")";
			if (DataRow != null)
			{
				int column = 0;
				foreach (var panel in Blocks)
				{
					foreach (UIElement child in panel.Children)
					{
						if (child.GetType() == typeof(TextBox))
						{
							column++;
							(child as TextBox).Text = DataRow.ItemArray[column].ToString();
						}
						else if (child.GetType() == typeof(ComboBox))
						{
							column++;
							foreach (var i in (child as ComboBox).Items)
							{
								if (i.ToString().Split('|')[0].Trim() == DataRow.ItemArray[column].ToString())
								{
									(child as ComboBox).SelectedItem = i;
									break;
								}
							}
						}
						else if (child.GetType() == typeof(CheckBox))
						{
							column++;
							(child as CheckBox).IsChecked = Boolean.Parse(DataRow.ItemArray[column].ToString());
						}
						else if (child.GetType() == typeof(DatePicker))
						{
							column++;
							if (DataRow.ItemArray[column].ToString() != "")
								(child as DatePicker).SelectedDate = DateTime.Parse(DataRow.ItemArray[column].ToString());
						}
					}
				}
			}
		}
		private void CreateComboBox(string name, string sql)
		{
			var panel = new StackPanel();
			panel.Orientation = Orientation.Horizontal;
			panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			panel.Margin = new System.Windows.Thickness(5);

			var comboBox = new ComboBox();
			comboBox.Width = 150;
			comboBox.FontSize = 14;
			comboBox.Tag = name;

			var table = DataBaseProvider.SendQuery(sql);
			if (table == null)
			{
				Window.Close();
				return;
			}

			foreach (DataRow i in table.Rows)
				comboBox.Items.Add(string.Join(" | ", i.ItemArray));

			TextBlock textBlock = new TextBlock();
			textBlock.Text = name;
			textBlock.FontSize = 14;
			textBlock.Margin = new System.Windows.Thickness(0, 0, 10, 0);


			panel.Children.Add(textBlock);
			panel.Children.Add(comboBox);

			Blocks.Add(panel);
		}
		private void CreateCheckBox(string name)
		{
			var panel = new StackPanel();
			panel.Orientation = Orientation.Horizontal;
			panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			panel.Margin = new System.Windows.Thickness(5);

			CheckBox checkBox = new CheckBox();
			checkBox.Content = name;
			checkBox.FlowDirection = FlowDirection.RightToLeft;
			checkBox.FontSize = 14;
			checkBox.Margin = new System.Windows.Thickness(0, 0, 10, 0);


			panel.Children.Add(checkBox);

			Blocks.Add(panel);
		}
		private void CreateDatePicker(string name)
		{
			var panel = new StackPanel();
			panel.Orientation = Orientation.Horizontal;
			panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			panel.Margin = new System.Windows.Thickness(5);

			var datePicker = new DatePicker();
			datePicker.Width = 150;
			datePicker.FontSize = 14;
			datePicker.Tag = name;
			datePicker.DisplayDate = DateTime.Now;

			TextBlock textBlock = new TextBlock();
			textBlock.Text = name;
			textBlock.FontSize = 14;
			textBlock.Margin = new System.Windows.Thickness(0, 0, 10, 0);


			panel.Children.Add(textBlock);
			panel.Children.Add(datePicker);

			Blocks.Add(panel);
		}
		private void CreateTextBox(string name)
		{
			var panel = new StackPanel();
			panel.Orientation = Orientation.Horizontal;
			panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			panel.Margin = new System.Windows.Thickness(5);

			var text_box = new TextBox();
			text_box.Width = 150;
			text_box.FontSize = 14;
			text_box.Tag = name;

			TextBlock textBlock = new TextBlock();
			textBlock.Text = name;
			textBlock.FontSize = 14;
			textBlock.Margin = new System.Windows.Thickness(0, 0, 10, 0);


			panel.Children.Add(textBlock);
			panel.Children.Add(text_box);

			Blocks.Add(panel);
		}
		#endregion
	}
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Data;
using System.Diagnostics;
using System.Data.OracleClient;
using Microsoft.Office.Interop.Excel;
using System.Configuration;

namespace Sverka
{
    public partial class MainWindow : System.Windows.Window
    {
        string СonnectionString;
        OracleDataAdapter adapter;
        System.Data.DataTable SALE_STOP_CHECK_Table;
        public MainWindow()
        {
            InitializeComponent();

            СonnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            UpdateTable();
            StatusScheduler();
        }
        public void StartingDataReconciliation()
        {
            OracleConnection connection = null;
            try
            {
                using (connection = new OracleConnection(СonnectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "dwh_arch.U$SALE_STOP_CHECK.SALE_STOP_PROC_TEST";

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }
        public void UpdateTable()
        {
            SALE_STOP_CHECK_Table = new System.Data.DataTable();
            OracleConnection connection = null;
            try
            {
                connection = new OracleConnection(СonnectionString);
                connection.Open();

                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandText = @"select * from DWH_ARCH.T$SALE_STOP_CHECK order by date_l desc";
                cmd.CommandType = CommandType.Text;

                object dr = cmd.ExecuteScalar();

                adapter = new OracleDataAdapter(cmd);
                adapter.Fill(SALE_STOP_CHECK_Table);
                SALE_STOP_CHECK_Grid.ItemsSource = SALE_STOP_CHECK_Table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        public void SearchInTable()
        {
            string search = TextBoxSearch.Text;
            SALE_STOP_CHECK_Table = new System.Data.DataTable();
            OracleConnection connection = null;
            try
            {
                connection = new OracleConnection(СonnectionString);
                connection.Open();

                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"select * from DWH_ARCH.T$SALE_STOP_CHECK where DRAW_KEY in ({search})";
                cmd.CommandType = CommandType.Text;

                object dr = cmd.ExecuteScalar();

                adapter = new OracleDataAdapter(cmd);
                adapter.Fill(SALE_STOP_CHECK_Table);
                SALE_STOP_CHECK_Grid.ItemsSource = SALE_STOP_CHECK_Table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        public void StartingScheduler()
        {
            OracleConnection connection = null;
            try
            {
                using (connection = new OracleConnection(СonnectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;//StoredProcedure
                        command.CommandText = "DBMS_SCHEDULER.ENABLE(name => 'DWH_ARCH.J$SALE_STOP_CHECK')";

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        public void StopScheduler()
        {
            OracleConnection connection = null;
            try
            {
                using (connection = new OracleConnection(СonnectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;//StoredProcedure
                        command.CommandText = "DBMS_SCHEDULER.DISABLE (name => 'DWH_ARCH.J$SALE_STOP_CHECK')";

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }


        public void StatusScheduler()
        {

            OracleConnection connection = null;
            Boolean status_scheduler;
            try
            {
                connection = new OracleConnection(СonnectionString);
                connection.Open();

                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandText = @"select enabled from user_SCHEDULER_jobs where job_name = 'J$SALE_STOP_CHECK'";
                cmd.CommandType = CommandType.Text;

                object dr = cmd.ExecuteScalar();
                status_scheduler = Convert.ToBoolean(dr.ToString());
                textBlockLOGS.Text = status_scheduler.ToString();

                if (status_scheduler == true)
                {
                    ImageLight.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/lightbulb.png"));
                    ImageLight.ToolTip = "На данынй момент JOB ВКЛЮЧЕН";
                    CheckBox1.IsChecked = true;
                }
                else
                {
                    ImageLight.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/lightbulb (1).png"));
                    ImageLight.ToolTip = "На данынй момент JOB ВЫКЛЮЧЕН";
                    CheckBox1.IsChecked = false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        private void ButtonZAPUSKSVERKI_Click(object sender, RoutedEventArgs e)
        {
            //Запуск сверки
            StartingDataReconciliation();
            //Обновить таблицу
            UpdateTable();
            //Проверяем статус расписания
            StatusScheduler();
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
            Worksheet sheet1 = (Worksheet)workbook.Sheets[1];
            TextBlock _b;
            for (int j = 0; j < SALE_STOP_CHECK_Grid.Columns.Count; j++)
            {

                Range myRange = (Range)sheet1.Cells[1, j + 1];
                sheet1.Cells[1, j + 1].Font.Bold = true;
                sheet1.Columns[j + 1].ColumnWidth = 15;
                myRange.Value2 = SALE_STOP_CHECK_Grid.Columns[j].Header;

            }

            for (int i = 0; i < SALE_STOP_CHECK_Grid.Columns.Count; i++)
            {
                for (int j = 0; j < SALE_STOP_CHECK_Grid.Items.Count; j++)
                {
                    Microsoft.Office.Interop.Excel.Range myRange = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[j + 2, i + 1]; //+2 +1 
                    _b = SALE_STOP_CHECK_Grid.Columns[i].GetCellContent(SALE_STOP_CHECK_Grid.Items[j]) as TextBlock;
                    myRange.Value2 = _b.Text.ToString();
                }
            }
            excel.Visible = true;
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchInTable();
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateTable();
            //Проверяем статус расписания
            StatusScheduler();
        }

        private void TextBoxSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchInTable();
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                var directory = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\"));
                var file = System.IO.Path.Combine(directory, "readme.txt");
                Process.Start(file);
            }
        }
        private void ButtonEXIT_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult Result = System.Windows.MessageBox.Show("Вы действительно хотите выйти?", "Выход", MessageBoxButton.YesNo);
            if (Result == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void CheckBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            //Снятие с расписания
            StopScheduler();
            //Проверяем статус расписания
            StatusScheduler();
        }

        private void CheckBox1_Checked(object sender, RoutedEventArgs e)
        {
            //Постановка на расписание
            StartingScheduler();
            //Проверяем статус расписания
            StatusScheduler();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult Result = System.Windows.MessageBox.Show("Вы действительно хотите выйти?", "Выход", MessageBoxButton.YesNo);
        }
    }
}

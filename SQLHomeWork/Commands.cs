using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SQLHomeWork
{
    class Commands
    {
        public static void Help()
        {
            Console.WriteLine("Команды:");
            Console.WriteLine("/help - Справка");
            Console.WriteLine("/addBankBranch - Добавить запись в предопределенную таблицу отделений банка");
            Console.WriteLine("/addPerson - Добавить запись в предопределенную таблицу пользователей");
            Console.WriteLine("/addTransaction - Добавить запись в предопределенную таблицу транзакций");
            Console.WriteLine("/exec - Выполнить код без возврата");
            Console.WriteLine("/sql - Выполнить код возврат - таблица");
            Console.WriteLine("/scalar - Выполнить код возврат - значение");
            Console.WriteLine("/curDB - Текущая БД");
            Console.WriteLine("/changeDB - Сменить БД");
            Console.WriteLine("/createDB - Создать БД");
            Console.WriteLine("/dropDB - Удалить БД");
            Console.WriteLine("/clear - Очистить окно консоли");
            Console.WriteLine("/exit - Выход из программы");
            Console.WriteLine();
        }

        private static string GetSQLCmd()
        {
            Console.Write("Введите запрос: ");
            string CMD = Console.ReadLine();
            if (Regex.Replace(CMD, @"\s+", " ").ToLower().Contains("drop database"))
            {
                Console.WriteLine("Я запрещаю ДРОПАТЬ БАЗЫ! Используй /dropDB");
                return null;
            }
            return CMD;
        }
        public static void Exec()
        {
            string CMD = GetSQLCmd();
            if (string.IsNullOrEmpty(CMD)) return;
            SQLHelper.ExecuteNonQuery(CMD);
        }

        public static void SQL()
        {
            string CMD = GetSQLCmd();
            if (string.IsNullOrEmpty(CMD)) return;
            List<string[]> rows = new List<string[]>();
            foreach (var row in SQLHelper.ExecuteSQL(CMD))
            {
                string[] rowItems = new string[row.Length]; 
                for (int i = 0; i < row.Length; i++)
                {
                    rowItems[i] = row[i].ToString();
                }
                rows.Add(rowItems);
            }
            if(rows.Count == 0)
            {
                Console.WriteLine("Нет возвращенных данных.");
            }
            DrawTableTool.Draw(rows);
        }

        public static void Scalar()
        {
            string CMD = GetSQLCmd();
            if (string.IsNullOrEmpty(CMD)) return;
            Console.WriteLine(SQLHelper.ExecuteScalar(CMD));
        }

        public static void AddBankBranch()
        {
            Console.Write("Введите название отделения: ");
            if(SQLHelper.ExecuteNonQuery(@"INSERT INTO ""BankBranches"" (""BranchName"") VALUES (@name)", new List<IDbDataParameter>() { new NpgsqlParameter(parameterName: "name", value: Console.ReadLine()) })
                != null)
                Console.WriteLine("Новое отделение создано.");
            else Console.WriteLine("Не удалось создать новое отделение.");
        }

        public static void AddPerson()
        {
            Console.Write("Введите имя: ");
            string name = Console.ReadLine();
            Console.Write("Введите фамилию: ");
            string surname = Console.ReadLine();

            double balance = 0;
            Console.Write("Введите баланс: ");
            while(!double.TryParse(Console.ReadLine(), out balance))
            {
                Console.WriteLine("Формат ввода не верен.");
                Console.Write("Введите баланс: ");
            }
            int bankId = 0;
            Console.Write("Введите номер банка: ");
            while (!int.TryParse(Console.ReadLine(), out bankId))
            {
                Console.WriteLine("Формат ввода не верен.");
                Console.Write("Введите номер банка: ");
            }

            if(SQLHelper.ExecuteNonQuery(@"INSERT INTO ""Persons"" (""Name"", ""Surname"", ""Balance"", ""BankBranchId"") VALUES (@name, @surname, @balance, @bankId)", 
                new List<IDbDataParameter>() 
                { 
                    new NpgsqlParameter(parameterName: "name", value: name),
                    new NpgsqlParameter(parameterName: "surname", value: surname),
                    new NpgsqlParameter(parameterName: "balance", value: balance),
                    new NpgsqlParameter(parameterName: "bankId", value: bankId)
                }) != null)
                Console.WriteLine("Создана новая запись пользователя.");
            else
                Console.WriteLine("Не удалось создать новую запись пользователя.");
        }
        public static void AddTransaction()
        {
            int FromId = 0;
            Console.Write("Введите номер пользователя от кого перечисление: ");
            while (!int.TryParse(Console.ReadLine(), out FromId))
            {
                Console.WriteLine("Формат ввода не верен.");
                Console.Write("Введите номер пользователя: ");
            }


            int ToId = 0;
            Console.Write("Введите номер пользователя кому перечисление: ");
            while (!int.TryParse(Console.ReadLine(), out ToId))
            {
                Console.WriteLine("Формат ввода не верен.");
                Console.Write("Введите номер пользователя: ");
            }

            double value = 0;
            Console.Write("Введите cумму: ");
            while (!double.TryParse(Console.ReadLine(), out value))
            {
                Console.WriteLine("Формат ввода не верен.");
                Console.Write("Введите cумму: ");
            }

            if (SQLHelper.ExecuteNonQuery(@"INSERT INTO ""Transactions"" (""FromPid"", ""ToPid"", ""Value"", ""TimeStamp"") VALUES (@fpid, @tpid, @value, now())",
                new List<IDbDataParameter>()
                {
                    new NpgsqlParameter(parameterName: "fpid", value: FromId),
                    new NpgsqlParameter(parameterName: "tpid", value: ToId),
                    new NpgsqlParameter(parameterName: "value", value: value)
                }) != null)
                Console.WriteLine("Создана новая транзакция.");
            else
                Console.WriteLine("Не удалось создать новую транзакцию.");
        }

        public static void ChangeDB()
        {
            Console.Write("Введите имя базы данных: ");
            SQLHelper.BaseName = Console.ReadLine();
            CurDB();
        }
        public static void CurDB()
        {
            Console.WriteLine("Текущая база данных - " + SQLHelper.BaseName);
        }

        public static void CreateDB()
        {
            Console.Write("Введите название новой БД: ");
            string DBName = Console.ReadLine();
            if (SQLHelper.CreateDB(DBName) == -1) Console.WriteLine($"База данных {DBName} создана.");
            else Console.WriteLine($"Ошибка на создании базы данных {DBName}.");
        }

        public static void DropDB()
        {
            Console.Write("Введите название удаляемой БД: ");
            string DBName = Console.ReadLine();
            if (DBName.ToLower() == "postgres") { Console.WriteLine($"Нельзя удалить {DBName}"); return; }
            if (SQLHelper.DropDB(DBName) != null) Console.WriteLine($"База данных {DBName} удалена.");
            else Console.WriteLine($"Ошибка на удалении базы данных {DBName}.");
        }

    }
}

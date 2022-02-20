using System;
using System.Collections.Generic;
using System.Data;

namespace SQLHomeWork
{
    class Program
    {
        static void Main(string[] args)
        {
            ///Создание/Удаление БД происходит через подключение к БД "postgres" <see cref="SQLHelper.AdminConnectionString"/>
            ///Остальные команды проходят через БД <see cref="SQLHelper.BaseName"/>
            ///Настройки подключения задаются там же.
            ///SQL Скрипты там <see cref="CreateBankBranchesTable"/>
            ///И там <see cref="FillBankBranchesTable"/>
            SQLHelper.DropDB("sber_MBezruchenko");
            TemplateBaseInit.InitializeTemplateBase("sber_MBezruchenko");

            //Пример скаляра
            Console.WriteLine($"Строк в BankBranch {SQLHelper.ExecuteScalar("SELECT COUNT(*) FROM \"BankBranches\";")}");
            Console.WriteLine(SQLHelper.ExecuteScalar("SELECT version();"));

            //Пример с параметрами
            SQLHelper.ExecuteNonQuery("INSERT INTO \"BankBranches" +
                "\" (\"BranchName\") VALUES (@name)",
                new List<IDbDataParameter>() {
                    new Npgsql.NpgsqlParameter(parameterName:"name",value:"Новая запись")
                });
            Console.WriteLine("Добавляем запись в BankBranches");
            List<BankBranch> bankBranches = new();
            foreach (var item in SQLHelper.ExecuteSQL("SELECT \"Id\", \"BranchName\" FROM  \"BankBranches\""))
            {
                bankBranches.Add(new BankBranch((long)item[0], (string)item[1]));
            }
            TemplateBaseInit.DrawTable(bankBranches);


            Console.WriteLine("Для перехода к функционалу жмякаем ENTER");
            Console.ReadLine();
            string ucmd;
            Console.Clear();
            Commands.Help();
            while (true) 
            {
                ucmd = Console.ReadLine().ToLower();
                switch(ucmd)
                {
                    case "/help": Commands.Help(); break;
                    case "/addbankbranch": Commands.AddBankBranch(); break;
                    case "/addperson": Commands.AddPerson(); break;
                    case "/addtransaction": Commands.AddTransaction(); break;
                    case "/exec": Commands.Exec(); break;
                    case "/sql": Commands.SQL(); break;
                    case "/scalar": Commands.Scalar(); break;
                    case "/changedb": Commands.ChangeDB(); break;
                    case "/curdb": Commands.CurDB(); break;
                    case "/createdb": Commands.CreateDB(); break;
                    case "/dropdb": Commands.DropDB(); break;
                    case "/clear": Console.Clear(); break;
                    case "/exit": return;
                    default: Console.WriteLine("Команда не найдена."); break;
                }
            }
        }


    }
}

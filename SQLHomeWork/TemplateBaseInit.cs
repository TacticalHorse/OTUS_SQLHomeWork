using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLHomeWork
{
    static class TemplateBaseInit
    {

        public static void InitializeTemplateBase(string BaseName)
        {
            List<BankBranch> bankBranches = new();
            List<Person> persons = new();
            List<Transaction> transactions = new();

            SQLHelper.CreateDB(BaseName);
            SQLHelper.BaseName = BaseName;

            //Создаем BankBranch
            if (CreateBankBranchesTable() != null)
            {
                Console.WriteLine("Таблица BankBranches создана." + Environment.NewLine);
            }
            SQLHelper.ExecuteNonQuery("DELETE FROM \"BankBranches\";");
            if (FillBankBranchesTable() != null)
            {
                foreach (var item in SQLHelper.ExecuteSQL("SELECT \"Id\", \"BranchName\" FROM  \"BankBranches\""))
                {
                    bankBranches.Add(new BankBranch((long)item[0], (string)item[1]));
                }
                DrawTable(bankBranches);
            }
            Console.WriteLine("");

            //Создаем Persons
            if (CreatePersonsTable() != null)
            {
                Console.WriteLine("Таблица Persons создана." + Environment.NewLine);

            }
            SQLHelper.ExecuteNonQuery("DELETE FROM \"Persons\";");
            if (FillPersonsTable(bankBranches) != null)
            {
                foreach (var item in SQLHelper.ExecuteSQL("SELECT \"Id\", \"BankBranchId\", \"Name\", \"Surname\", \"Balance\" FROM  \"Persons\""))
                {
                    persons.Add(new Person((long)item[0], bankBranches.Find(x => x.Id == (long)item[1]), (string)item[2], (string)item[3], (double)item[4]));
                }
                DrawTable(persons);
            }
            Console.WriteLine("");

            //Создаем Transactions
            if (CreateTransactionsTable() != null)
            {
                Console.WriteLine("Таблица Transactions создана." + Environment.NewLine);

            }
            SQLHelper.ExecuteNonQuery("DELETE FROM \"Transactions\";");
            if (FillTransactionsTable(persons) != null)
            {
                foreach (var item in SQLHelper.ExecuteSQL("SELECT \"Id\", \"FromPid\", \"ToPid\", \"Value\",\"TimeStamp\" FROM  \"Transactions\""))
                {
                    transactions.Add(new Transaction((long)item[0], persons.Find(x => x.Id == (long)item[1]), persons.Find(x => x.Id == (long)item[2]), (double)item[3], (DateTime)item[4]));
                }
                DrawTable(transactions);
            }

            Console.WriteLine(Environment.NewLine + "После проведения транзакций");
            List<Person> UpdatedPersons = new();
            //После проведения транзакций
            foreach (var item in SQLHelper.ExecuteSQL("SELECT \"Id\", \"BankBranchId\", \"Name\", \"Surname\", \"Balance\" FROM  \"Persons\"  ORDER BY \"Id\""))
            {
                UpdatedPersons.Add(new Person((long)item[0], bankBranches.Find(x => x.Id == (long)item[1]), (string)item[2], (string)item[3], (double)item[4]));
            }
            DrawTable(UpdatedPersons);
        }

        #region DrawTable
        public static void DrawTable(List<BankBranch> bankBranches)
        {
            Console.WriteLine("_".PadRight(37, '_') + "_");
            Console.WriteLine("|{0,5}|{1,-30}|", "Id", "Название");
            Console.WriteLine("|".PadRight(37, '-') + "|");
            foreach (var item in bankBranches)
            {
                Console.WriteLine("|{0,5}|{1,-30}|", item.Id, item.BranchName);
            }
            Console.WriteLine("|".PadRight(37, '_') + "|");
        }
        public static void DrawTable(List<Person> persons)
        {
            Console.WriteLine("_".PadRight(90, '_') + "_");
            Console.WriteLine("|{0,5}|{1,-30}|{2,-20}|{3,-20}|{4,10}|", "Id", "Название отделения", "Имя", "Фамилия", "Баланс");
            Console.WriteLine("|".PadRight(90, '-') + "|");
            foreach (var item in persons)
            {
                Console.WriteLine("|{0,5}|{1,-30}|{2,-20}|{3,-20}|{4,10:N2}|", item.Id, item.BankBranch.BranchName, item.Name, item.Surname, item.Balance);
            }
            Console.WriteLine("|".PadRight(90, '_') + "|");
        }
        public static void DrawTable(List<Transaction> transactions)
        {
            Console.WriteLine("_".PadRight(100, '_') + "_");
            Console.WriteLine("|{0,5}|{1,-30}|{2,-30}|{3,10}|{4,20}|", "Id", "От кого", "Кому", "Сумма", "Дата");
            Console.WriteLine("|".PadRight(100, '-') + "|");
            foreach (var item in transactions)
            {
                Console.WriteLine("|{0,5}|{1,-30}|{2,-30}|{3,10}|{4,20}|", item.Id, $"{item.From.Surname} {item.From.Name}", $"{item.To.Surname} {item.To.Name}", item.Value, item.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            Console.WriteLine("|".PadRight(100, '_') + "|");
        }
        #endregion

        #region Create table
        private static int? CreateBankBranchesTable()
        {
            return SQLHelper.ExecuteNonQuery(
                @"CREATE TABLE ""BankBranches""
                (
                    ""Id"" bigserial NOT NULL,
                    ""BranchName"" text NOT NULL,
                    PRIMARY KEY(""Id"")
                ); ");
        }
        private static int? CreatePersonsTable()
        {
            return SQLHelper.ExecuteNonQuery(
                @"CREATE TABLE ""Persons""(
                    ""Id"" bigserial NOT NULL,
                    ""BankBranchId"" bigint NOT NULL,
                    ""Name"" text,
                    ""Surname"" text,
                    ""Balance"" double precision,
                    PRIMARY KEY(""Id""),
                    CONSTRAINT ""FK_Person_BBId_BankBranch_Id"" FOREIGN KEY(""BankBranchId"")
                        REFERENCES ""BankBranches"" (""Id"") ON DELETE CASCADE
                );");
        }
        private static int? CreateTransactionsTable()
        {
            return SQLHelper.ExecuteNonQuery(
                @"CREATE TABLE ""Transactions""
                (
                    ""Id"" bigserial NOT NULL,
                    ""FromPid"" bigint,
                    ""ToPid"" bigint,
                    ""Value"" double precision,
                    ""TimeStamp"" timestamp without time zone,
                    PRIMARY KEY (""Id"", ""FromPid"", ""ToPid"", ""Value""),
                    CONSTRAINT ""FK_Transaction_FPID_People_ID"" FOREIGN KEY (""FromPid"")
                        REFERENCES ""Persons"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_Transaction_TPID_People_ID"" FOREIGN KEY (""ToPid"")
                        REFERENCES ""Persons"" (""Id"") ON DELETE CASCADE
                );
                CREATE FUNCTION ""TR_Transactions_Ins_Handler""() RETURNS trigger AS $BODY$
                    BEGIN
                        IF (TG_OP = 'INSERT') THEN
	                        UPDATE ""Persons"" SET ""Balance"" = (""Balance"" + NEW.""Value"") WHERE ""Id"" = NEW.""ToPid"";
	                        UPDATE ""Persons"" SET ""Balance"" = (""Balance"" - NEW.""Value"") WHERE ""Id"" = NEW.""FromPid"";
                        END IF;
                        RETURN NEW; 
                    END
                    $BODY$
                    LANGUAGE 'plpgsql';

                    CREATE TRIGGER ""TR_Transactions_I"" AFTER INSERT ON ""Transactions""
                        FOR EACH ROW EXECUTE PROCEDURE ""TR_Transactions_Ins_Handler""();");
        }

        #endregion

        #region FillTable
        private static int? FillBankBranchesTable()
        {
            return SQLHelper.ExecuteNonQuery(
                @"INSERT INTO ""BankBranches"" (""BranchName"")
                    SELECT * FROM(VALUES
                    ('Московское отделение')
                    , ('Санкт-Петербургское отделение')
                    , ('Уральское отделение')
                    , ('Дальневосточное отделение')
                    , ('Сочинское отделение')
                )AS tmp(col1); ");
        }
        private static int? FillPersonsTable(List<BankBranch> bankBranches)
        {
            return SQLHelper.ExecuteNonQuery(
                @"INSERT INTO ""Persons"" (""BankBranchId"", ""Name"", ""Surname"", ""Balance"")
                SELECT * FROM(VALUES " +
                $" ({bankBranches[0].Id}, 'Ольга', 'Исмаилова', 2000) " +
                $",({bankBranches[1].Id}, 'Роман', 'Акиньшин', 2000) " +
                $",({bankBranches[2].Id}, 'Наталья', 'Лашпанова', 2000) " +
                $",({bankBranches[3].Id}, 'Константин', 'Зиньковский', 2000) " +
                $",({bankBranches[4].Id}, 'Татьяна', 'Рыбина', 2000) " +
            ")AS tmp(col1, col2, col3); ");
        }
        private static int? FillTransactionsTable(List<Person> persons)
        {
            return SQLHelper.ExecuteNonQuery(
                @"INSERT INTO ""Transactions"" (""FromPid"", ""ToPid"", ""Value"", ""TimeStamp"")
                    SELECT * FROM(VALUES " +
                    $" ({persons[0].Id}, {persons[2].Id}, 150.11, now()) " +
                    $",({persons[1].Id}, {persons[3].Id}, 500, now()) " +
                    $",({persons[3].Id}, {persons[4].Id}, 250.57, now()) " +
                    $",({persons[4].Id}, {persons[0].Id}, 777.77, now()) " +
                    $",({persons[2].Id}, {persons[1].Id}, 20, now()) " +
                ")AS tmp(col1, col2, col3, col4); ");
        }
        #endregion
    }
}

using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace SQLHomeWork
{
    internal static class SQLHelper
    {
        private static string Host = "localhost";
        private static string Port = "5433";
        private static string User = "postgres";
        private static string Pass = "postgres";
        public static string BaseName = "otus";
        private static string AdminConnectionString => $"Host={Host};Port={Port};Username={User};Password={Pass};Database=postgres;"; 
        private static string UserConnectionString => $"Host={Host};Port={Port};Username={User};Password={Pass};Database={BaseName};";

        private static IDbConnection GetDBConnection(string ConnStr)
        {
            return new NpgsqlConnection(connectionString: ConnStr);
        }

        private static IDbCommand GetCommand(string Command, IDbConnection Connection)
        {
            return new NpgsqlCommand(Command, (NpgsqlConnection)Connection);
        }

        private static ConnectionCommandPair InitCommand(string commandString, List<IDbDataParameter> sqlParameters = null, string ConnectionString = null)
        {
            ConnectionCommandPair Output = new ConnectionCommandPair();
            try
            {
                if (String.IsNullOrEmpty(ConnectionString)) ConnectionString = UserConnectionString;
                Output.Con = GetDBConnection(ConnectionString);
                Output.Con.Open();
                Output.Com = GetCommand(commandString, Output.Con);
                Output.Com.CommandType = CommandType.Text;
                Output.Com.Parameters.Clear();
                if (sqlParameters != null)
                {
                    foreach (var parameter in sqlParameters)
                    {
                        Output.Com.Parameters.Add(parameter);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.WriteLog(Logger.LogStatuses.Error, $"Ошибка подключении {Environment.NewLine}{ex.Message}");
            }
            return Output;
        }

        public static int? ExecuteNonQuery(string commandString, List<IDbDataParameter> sqlParameters = null)
        {
            try
            {
                using ConnectionCommandPair connectionCommandPair = InitCommand(commandString, sqlParameters);
                return connectionCommandPair.Com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogStatuses.Error, $"Ошибка на запросе {commandString} {Environment.NewLine}{ex.Message}");
            }
            return null;
        }

        public static string ExecuteScalar(string commandString, List<IDbDataParameter> sqlParameters = null)
        {
            string output = "";
            try
            {
                using ConnectionCommandPair connectionCommandPair = InitCommand(commandString, sqlParameters);
                output = connectionCommandPair.Com.ExecuteScalar()?.ToString();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogStatuses.Error, $"Ошибка на запросе {commandString} {Environment.NewLine}{ex.Message}");
            }
            return output;
        }

        public static IEnumerable<object[]> ExecuteSQL(string commandString, List<IDbDataParameter> sqlParameters = null)
        {
            IDataReader reader = null;
            ConnectionCommandPair connectionCommandPair = null;
            try
            {
                connectionCommandPair = InitCommand(commandString, sqlParameters);
                reader = connectionCommandPair.Com.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogStatuses.Error, $"Ошибка на запросе {commandString} {Environment.NewLine}{ex.Message}");
            }
            if (reader != null)
            {
                while (reader.Read())
                {
                    object[] output = new object[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        output[i] = reader.GetValue(i);
                    }
                    yield return output;
                }
                if (!reader.IsClosed) reader.Dispose(); 
            }
            connectionCommandPair?.Dispose();
        }

        public static int? CreateDB (string DBName)
        {
            try
            {
                string CreateDBCMD = $"CREATE DATABASE \"{DBName}\" " +
                    "WITH " +
                    "OWNER = postgres " +
                    "ENCODING = 'UTF8' " +
                    "CONNECTION LIMIT = -1;";
                using ConnectionCommandPair connectionCommandPair = InitCommand(CreateDBCMD, ConnectionString: AdminConnectionString);
                if (connectionCommandPair.Com == null) return 1;
                return connectionCommandPair.Com?.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogStatuses.Error, $"Ошибка на создании БД {DBName} {Environment.NewLine}{ex.Message}");
            }
            return null;
        }
        public static int? DropDB (string DBName)
        {
            try
            {
                string CreateDBCMD = $"DROP DATABASE \"{DBName}\";";
                using ConnectionCommandPair connectionCommandPair = InitCommand(CreateDBCMD, ConnectionString: AdminConnectionString);
                if (connectionCommandPair.Com == null) return 1;
                return connectionCommandPair.Com?.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogStatuses.Error, $"Ошибка на удалении БД {DBName} {Environment.NewLine}{ex.Message}");
            }
            return null;
        }


        public static IEnumerable<T> Select<T>(this IDataReader reader, Func<IDataReader,T> projection)
        {
            while (reader.Read())
            {
                yield return projection(reader);
            }
        }

        class ConnectionCommandPair:IDisposable
        {
            public IDbConnection Con = null;
            public IDbCommand Com = null;
            public void Dispose()
            {
                Con?.Dispose();
                Com?.Dispose();
            }
        }
    }
}

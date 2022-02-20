using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace SQLHomeWork
{
    internal static class SQLHelper
    {
        private static string host = "localhost";
        private static string port = "5433";
        private static string user = "postgres";
        private static string pass = "postgres";
        public static string BaseName = "otus";
        private static string AdminConnectionString => $"Host={host};Port={port};Username={user};Password={pass};Database=postgres;"; 
        private static string UserConnectionString => $"Host={host};Port={port};Username={user};Password={pass};Database={BaseName};";

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
            ConnectionCommandPair output = new();
            try
            {
                if (String.IsNullOrEmpty(ConnectionString)) ConnectionString = UserConnectionString;
                output.Con = GetDBConnection(ConnectionString);
                output.Con.Open();
                output.Com = GetCommand(commandString, output.Con);
                output.Com.CommandType = CommandType.Text;
                output.Com.Parameters.Clear();
                if (sqlParameters != null)
                {
                    foreach (var parameter in sqlParameters)
                    {
                        output.Com.Parameters.Add(parameter);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.WriteLog(Logger.LogStatuses.Error, $"Ошибка подключении {Environment.NewLine}{ex.Message}");
            }
            return output;
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
                string cmd = $"CREATE DATABASE \"{DBName}\" " +
                    "WITH " +
                    "OWNER = postgres " +
                    "ENCODING = 'UTF8' " +
                    "CONNECTION LIMIT = -1;";
                using ConnectionCommandPair connectionCommandPair = InitCommand(cmd, ConnectionString: AdminConnectionString);
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
                string cmd = $"DROP DATABASE \"{DBName}\";";
                using ConnectionCommandPair connectionCommandPair = InitCommand(cmd, ConnectionString: AdminConnectionString);
                if (connectionCommandPair.Com == null) return 1;
                return connectionCommandPair.Com?.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogStatuses.Error, $"Ошибка на удалении БД {DBName} {Environment.NewLine}{ex.Message}");
            }
            return null;
        }

        class ConnectionCommandPair:IDisposable
        {
            public IDbConnection Con = null;
            public IDbCommand Com = null;
            public void Dispose()
            {
                Con?.Close();
                Con?.Dispose();
                Com?.Dispose();
            }
        }
    }
}

using System;

namespace SQLHomeWork
{
    public class Logger
    {
        public static void WriteLog(LogStatuses status, string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss} {Enum.GetName(typeof(LogStatuses), status)}] {message}");
        }

        public enum LogStatuses
        {
            Message,
            Warning,
            Error
        }
    }
}

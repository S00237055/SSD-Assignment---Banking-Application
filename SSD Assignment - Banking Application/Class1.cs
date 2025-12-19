using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SSD_Assignment___Banking_Application
{
    public static class Logger
    {
        private  const string SourceName = "SSD Banking Application";
        private const string LogName = "Application";

        static Logger()
        {
            try
            {
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, LogName);
                    Console.WriteLine("CreatedEventSource");
                    Console.WriteLine("Exiting, execute the application a second time to use the source.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logger Error: Could not create event source. {ex.Message}");
                Console.WriteLine("Try running Visual Studio as Admin.");
            }
        }

        public static void LogTransaction(string tellerName, string accountNo, string accountHolder, string transactionType, string amount = "N/A", string reason = "N/A")
        {
            StringBuilder sb = new StringBuilder();

            // WHO
            sb.AppendLine($"Teller: {tellerName}");
            sb.AppendLine($"Account No: {accountNo}");
            sb.AppendLine($"Account Holder: {accountHolder}");

            // WHAT
            sb.AppendLine($"Transaction Type: {transactionType}");
            if (amount != "N/A")
                sb.AppendLine($"Amount: {amount}");


        }
    }
}

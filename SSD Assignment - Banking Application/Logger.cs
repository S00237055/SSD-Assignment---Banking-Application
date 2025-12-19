using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.IO;

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

            //WHERE
           

            string ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
            sb.AppendLine($"Where (IP): {ipAddress}");

            //WHEN
            sb.AppendLine($"When: {DateTime.Now}");

            //WHY
            if (amount != "N/A" && double.TryParse(amount, out double val) && val > 10000)
            {
                sb.AppendLine($"Why (Reason): {reason}");   
            }

            //HOW
            var assembly = Assembly.GetExecutingAssembly();
            sb.AppendLine($"How (App Name): {assembly.GetName().Name}");
            sb.AppendLine($"How (Version): {assembly.GetName().Version}");

            try
            {
                if (File.Exists(assembly.Location))
                {
                    using (var stream = File.OpenRead(assembly.Location))
                    using (var sha = SHA256.Create())
                    {
                        byte[] hash = sha.ComputeHash(stream);
                        sb.AppendLine($"How (Hash): {BitConverter.ToString(hash).Replace("-", "")}");
                    }
                }

            }
            catch (Exception ex) { 
                sb.AppendLine($"How (Hash): Could not calcualte ({ex.Message})");
            }

            try { 
                EventLog.WriteEntry(SourceName, sb.ToString(), EventLogEntryType.Information);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logger Error: Failed to write to event log. {ex.Message}");
            }
        }

        public static void LogLogin(string tellerName, bool success)
        {
            string message = success ? $"Successful Login: {tellerName}" : $"Failed Login Attempt: {tellerName}";
            EventLogEntryType type = success ? EventLogEntryType.Information : EventLogEntryType.Warning;

            try
            {
                EventLog.WriteEntry(SourceName, message, type);

            }
            catch { }
        }
    }
}

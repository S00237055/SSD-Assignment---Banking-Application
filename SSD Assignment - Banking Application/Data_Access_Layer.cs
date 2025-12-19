using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;
using SSD_Assignment___Banking_Application;

namespace Banking_Application
{
    public class Data_Access_Layer
    {

        private List<Bank_Account> accounts;
        public static String databaseName = "Banking Database.db";
        private static Data_Access_Layer instance = new Data_Access_Layer();
        //private string databasePassword;
        private EncryptionService enc;
        private Data_Access_Layer()//Singleton Design Pattern (For Concurrency Control) - Use getInstance() Method Instead.
        {
            enc = EncryptionService.getInstance();
            accounts = new List<Bank_Account>();

            //Console.WriteLine("Enter Database Password: ");
            //this.databasePassword = Console.ReadLine();
        }

        public static Data_Access_Layer getInstance()
        {
            return instance;
        }

        private SqliteConnection getDatabaseConnection()
        {
            
            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Data_Access_Layer.databaseName,
                Mode = SqliteOpenMode.ReadWriteCreate,
                //Password = this.databasePassword
            }.ToString();

            return new SqliteConnection(databaseConnectionString);

        }

        private void initialiseDatabase()
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Bank_Accounts(    
                        accountNo TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        address_line_1 TEXT,
                        address_line_2 TEXT,
                        address_line_3 TEXT,
                        town TEXT NOT NULL,
                        balance TEXT NOT NULL,
                        accountType INTEGER NOT NULL,
                        overdraftAmount TEXT,
                        interestRate TEXT
                    ) WITHOUT ROWID
                ";

                command.ExecuteNonQuery();
                
            }
        }

        public void loadBankAccounts()
        {
            if (!File.Exists(Data_Access_Layer.databaseName))
                initialiseDatabase();
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Bank_Accounts";
                    SqliteDataReader dr = command.ExecuteReader();
                    
                    while(dr.Read())
                    {

                        int accountType = dr.GetInt16(7);

                        if(accountType == Account_Type.Current_Account)
                        {
                            Current_Account ca = new Current_Account();
                            ca.accountNo = dr.GetString(0);

                            //Decrypt all fields
                            ca.name = enc.DecryptString(dr.GetString(1));
                            ca.address_line_1 = enc.DecryptString(dr.GetString(2));
                            ca.address_line_2 = enc.DecryptString(dr.GetString(3));
                            ca.address_line_3 = enc.DecryptString(dr.GetString(4));
                            ca.town = enc.DecryptString(dr.GetString(5));
                            ca.balance = Convert.ToDouble(enc.DecryptString(dr.GetString(6)));
                            if (!dr.IsDBNull(8))
                                ca.overdraftAmount = Convert.ToDouble(enc.DecryptString(dr.GetString(8)));
                            
                            accounts.Add(ca);
                        }
                        else
                        {
                            Savings_Account sa = new Savings_Account();
                            sa.accountNo = dr.GetString(0);
                            sa.name = enc.DecryptString(dr.GetString(1));
                            sa.address_line_1 = enc.DecryptString(dr.GetString(2));
                            sa.address_line_2 = enc.DecryptString(dr.GetString(3));
                            sa.address_line_3 = enc.DecryptString(dr.GetString(4));
                            sa.town = enc.DecryptString(dr.GetString(5));
                            sa.balance = Convert.ToDouble(enc.DecryptString(dr.GetString(6)));
                            if (!dr.IsDBNull(9))
                                sa.interestRate = Convert.ToDouble(enc.DecryptString(dr.GetString(9)));
                            
                            accounts.Add(sa);
                        }


                    }

                }

            }
        }

        public String addBankAccount(Bank_Account ba) 
        {

            if (ba.GetType() == typeof(Current_Account))
                ba = (Current_Account)ba;
            else
                ba = (Savings_Account)ba;

            accounts.Add(ba);

            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO Bank_Accounts VALUES(" +
                    "'" + ba.accountNo + "', " +
                    "'" + enc.EncryptString(ba.name) + "', " +
                    "'" + enc.EncryptString(ba.address_line_1) + "', " +
                    "'" + enc.EncryptString(ba.address_line_2) + "', " +
                    "'" + enc.EncryptString(ba.address_line_3) + "', " +
                    "'" + enc.EncryptString(ba.town) + "', " +
                    "'" + enc.EncryptString(ba.balance.ToString()) + "', " +
                    (ba.GetType() == typeof(Current_Account) ? 1 : 2) + ", ";

                if (ba.GetType() == typeof(Current_Account))
                {
                    Current_Account ca = (Current_Account)ba;
                    command.CommandText += "'" + enc.EncryptString(ca.overdraftAmount.ToString()) + "', NULL)";
                }

                else
                {
                    Savings_Account sa = (Savings_Account)ba;
                    command.CommandText += "NULL,'" + enc.EncryptString(sa.interestRate.ToString()) + "')";
                }

                command.ExecuteNonQuery();

            }

            return ba.accountNo;

        }

        public Bank_Account findBankAccountByAccNo(String accNo) 
        { 
        
            foreach(Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    return ba;
                }

            }

            return null; 
        }

        public bool closeBankAccount(String accNo) 
        {

            Bank_Account toRemove = null;
            
            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    toRemove = ba;
                    break;
                }

            }

            if (toRemove == null)
                return false;
            else
            {
                accounts.Remove(toRemove);

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = '" + toRemove.accountNo + "'";
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

        public bool lodge(String accNo, double amountToLodge)
        {

            Bank_Account toLodgeTo = null;
            

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    ba.lodge(amountToLodge);
                    toLodgeTo = ba;
                    break;
                }

            }

            if (toLodgeTo == null)
                return false;
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = '" + enc.EncryptString(toLodgeTo.balance.ToString()) + "' WHERE accountNo = '" + toLodgeTo.accountNo + "'";
                    command.ExecuteNonQuery();

                }

                
                return true;
            }

        }

        public bool withdraw(String accNo, double amountToWithdraw)
        {

            Bank_Account toWithdrawFrom = null;
            bool result = false;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    result = ba.withdraw(amountToWithdraw);
                    toWithdrawFrom = ba;
                    break;
                }

            }

            if (toWithdrawFrom == null || result == false)
                return false;
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = '" + enc.EncryptString(toWithdrawFrom.balance.ToString()) + "' WHERE accountNo = '" + toWithdrawFrom.accountNo + "'";
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

    }
}

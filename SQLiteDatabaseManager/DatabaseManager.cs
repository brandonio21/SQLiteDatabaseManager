using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SQLiteDatabaseManager
{
    public class DatabaseManager
    {
        private static readonly string DATABASE_FILE = "database.sqlite";
        private Dictionary<string, string> tables;

        private SQLiteConnection connection;

        public DatabaseManager()
        {

        }

        public DatabaseManager(bool makeDatabase)
        {
            if (makeDatabase)
            {
                // Let's see if the database file exists first
                if (!(DatabaseExists()))
                {
                    if (!(CreateDatabase()))
                    {
                        System.Windows.Forms.MessageBox.Show("Cannot create database file!");
                        return;
                    }
                }
                // The database is now in existence!
            } 
        }

        public bool OpenConnection()
        {
            // If the connection already exists, we will just let it be
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
                return true;

            try
            {
                connection = new SQLiteConnection("Data Source=" + DATABASE_FILE);
                connection.Open();
                return true;
            }
            catch (Exception e)
            {
                if (System.Windows.Forms.MessageBox.Show("Connection to Database Failed.\n" + e.Message,
                    "Connection Failed", System.Windows.Forms.MessageBoxButtons.RetryCancel,
                    System.Windows.Forms.MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                {
                    return OpenConnection();
                }
                else
                    return false;
            }
        }


        public bool DatabaseExists()
        {
            try
            {
                return (System.IO.File.Exists(DATABASE_FILE));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        public bool CreateDatabase()
        {
            try
            {
                // Create the database file
                SQLiteConnection.CreateFile(DATABASE_FILE);
                return true;
            }
            catch (Exception e)
            {
                if (System.Windows.Forms.MessageBox.Show("Something went wrong while" +
                    " trying to create the database file!\n" + e.StackTrace,
                    "Error Creating Database File!", System.Windows.Forms.MessageBoxButtons.RetryCancel,
                    System.Windows.Forms.MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                {
                    return CreateDatabase();
                }
                else
                    return false;
            }
        }

        public void AddTable(string TableName, string TableString)
        {
            if (this.tables == null)
                this.tables = new Dictionary<string, string>();

            this.tables.Add(TableName, TableString);
        }

        public bool CreateAllTables()
        {
            if (OpenConnection())
            {
                // There is an open connection to the database
                foreach (string creationString in this.tables.Values)
                {
                    try
                    {
                        SQLiteCommand command = new SQLiteCommand("CREATE TABLE " + creationString, connection);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error creating a table due to unknown errors. Skipping table creation:\n{0}", e.Message.ToString());
                        continue;
                    }
                }
                return true;
            }
            else
            {
                // For whatever reason, a connection failed to open
                System.Windows.Forms.MessageBox.Show("Connection Failed to Open!");
                return false;
            }
        }

        public bool TablesAreVerified()
        {
            if (OpenConnection())
            {
                // The connection is opened and can be used!
                try
                {
                    SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type=\"table\"", connection);
                    SQLiteDataReader results = command.ExecuteReader();
                    List<string> tableNames = new List<string>();
                    foreach (string key in this.tables.Keys)
                        tableNames.Add(key);

                    while (results.Read())
                    {
                        string tableName = results.GetValue(0).ToString();
                        if (tableNames.Contains(tableName))
                            tableNames.Remove(tableName);
                    }
                    results.Close();
                    if (tableNames.Count > 0)
                        return false;
                    else
                        return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Some error occurred while verifying the integrity of the tables.\n{0}", e.Message.ToString());
                    return false;
                }
            }
            else
            {
                // There is no connection, we must notify!
                System.Windows.Forms.MessageBox.Show("Connection Failed to Open!");
                return false;
            }
        }

        public bool CreateDatabaseTables(string TableString)
        {
            if (OpenConnection())
            {
                // The connection is opened and can be used!
                try
                {
                    SQLiteCommand command = new SQLiteCommand("CREATE TABLE " + TableString, connection);
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e)
                {
                    if (System.Windows.Forms.MessageBox.Show("Error creating the tables of the database: \n" +
                        e.Message, "Error Creating Tables", System.Windows.Forms.MessageBoxButtons.RetryCancel,
                        System.Windows.Forms.MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    {
                        return (CreateDatabaseTables(TableString));
                    }
                    else
                        return false;
                }

            }
            else
            {
                // There is no connection, we must notify!
                System.Windows.Forms.MessageBox.Show("Connection Failed to Open!");
                return false;
            }
        }

        public int InsertIntoTable(string TableName, string Fields, string Values)
        {
            if (OpenConnection())
            {
                // The connection is open and can be used for processing commands
                try
                {
                    string sqlQuery = "INSERT INTO " + TableName + " (" + Fields + ") VALUES (" + Values + ")";
                    SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
                    command.ExecuteNonQuery();
                    SQLiteCommand getInsertId = new SQLiteCommand(@"select last_insert_rowid()", connection);
                    long id = (long)getInsertId.ExecuteScalar();
                    return (int)id;
                }
                catch (Exception e)
                {
                    if (System.Windows.Forms.MessageBox.Show("Error inserting record into table:\n" + e.Message,
                        "Error Inserting into Table", System.Windows.Forms.MessageBoxButtons.RetryCancel,
                        System.Windows.Forms.MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    {
                        return (InsertIntoTable(TableName, Fields, Values));
                    }
                    else
                        return -1;
                }
            }
            else
            {
                // There is no connection! NOTIFY DA USER!
                System.Windows.Forms.MessageBox.Show("Connection failed to Open!");
                return -1;
            }
        }

        public SQLiteDataReader SelectFromTable(String Fields, String TableName, String Options)
        {
            if (OpenConnection())
            {
                // The connection is open and can be used for processing commands
                try
                {
                    string sqlQuery = "SELECT " + Fields + " FROM " + TableName + " " + Options;
                    SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
                    return command.ExecuteReader();
                }
                catch (Exception e)
                {
                    if (System.Windows.Forms.MessageBox.Show("Error selecting from the table:\n" + e.Message, 
                        "Error Selecting from Table", System.Windows.Forms.MessageBoxButtons.RetryCancel,
                        System.Windows.Forms.MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    {
                        return (SelectFromTable(Fields, TableName, Options));
                    }
                    else
                        return null;
                }
            }
            else
            {
                // There was an error opening the connection! Notify the user!
                System.Windows.Forms.MessageBox.Show("Connection Failed to Open!");
                return null;
            }
        }

        public bool UpdateTable(string TableName, string SetString, string Options)
        {
            if (OpenConnection())
            {
                // The connection is open and can be used for processing commands now
                try
                {
                    String sqlQuery = "UPDATE " + TableName + " " + SetString + " " + Options;
                    SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e)
                {
                    if (System.Windows.Forms.MessageBox.Show("Error Updating the Tables:\n" + e.Message,
                        "Error Updating Table!", System.Windows.Forms.MessageBoxButtons.RetryCancel,
                        System.Windows.Forms.MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    {
                        return (UpdateTable(TableName, SetString, Options));
                    }
                    else
                        return false;
                }
            }
            else
            {
                // There was some sort of error opening the connection!
                System.Windows.Forms.MessageBox.Show("Connection Failed to Open!");
                return false;
            }
        }

        public bool DeleteFromTable(string TableName, string Options)
        {
            if (OpenConnection())
            {
                // The connection is open and can be used for processing commands
                try
                {
                    string sqlQuery = "DELETE FROM " + TableName + " WHERE " + Options;
                    SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e)
                {
                    if (System.Windows.Forms.MessageBox.Show("Error Deleting an Entry from Table:\n" + e.Message,
                        "Error Deleting Entry", System.Windows.Forms.MessageBoxButtons.RetryCancel,
                        System.Windows.Forms.MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    {
                        return (DeleteFromTable(TableName, Options));
                    }
                    else
                        return false;
                }
            }
            else
            {
                // There was an error opening the connection! Notify the user!
                System.Windows.Forms.MessageBox.Show("Connection failed to open!");
                return false;
            }
        }

        public void CloseConnection()
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }
    }
}

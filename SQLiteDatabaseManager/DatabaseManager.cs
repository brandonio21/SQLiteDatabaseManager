using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SQLiteDatabaseManager
{
    class DatabaseManager
    {
        private static readonly string DATABASE_FILE = "database.sqlite";

        private SQLiteConnection connection;

        public DatabaseManager()
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

        public long InsertIntoTable(string TableName, string Fields, string Values)
        {
            if (OpenConnection())
            {
                // The connection is open and can be used for processing commands
                try
                {
                    string sqlQuery = "INSERT INTO " + TableName + " (" + Fields + ") values (" + Values + ")";
                    SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
                    long id = (long)command.ExecuteScalar();
                    return id;
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
    }
}

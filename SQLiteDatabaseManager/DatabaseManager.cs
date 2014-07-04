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

        public bool CreateDatabaseTables(string CreationString)
        {
            if (OpenConnection())
            {
                // The connection is opened and can be used!
                try
                {
                    SQLiteCommand command = new SQLiteCommand(CreationString, connection);
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e)
                {
                    if (System.Windows.Forms.MessageBox.Show("Error creating the tables of the database: \n" +
                        e.Message, "Error Creating Tables", System.Windows.Forms.MessageBoxButtons.RetryCancel,
                        System.Windows.Forms.MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    {
                        return (CreateDatabaseTables(CreationString));
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
    }
}

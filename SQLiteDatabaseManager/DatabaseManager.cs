using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SQLiteDatabaseManager
{
    /* DatabaseManager class
    * Class meant to manage all aspects of a SQLite Database. Keep in mind
    * that this is designed for use with a SINGLE database. Any further
    * complicated operations must be explicitly defined using the standard
    * SQLite API.
    *
    * Created by brandonio21
    * http://brandonio21.com
    */
    public class DatabaseManager
    {
        // Define the single database file
        private static readonly string DATABASE_FILE = "database.sqlite"; 
        private Dictionary<string, string> tables;

        // Only a single connection will exist to this database
        private SQLiteConnection connection;

        // Default constructor
        public DatabaseManager() {}

        /* Constructor with option to make database
        * Creates a new instance of a DatabaseManager and if specified
        * also creates the database file.
        * @param makeDatabase - A boolean indicating whether or not to make the DB file
        * @return A new instance of the DatabaseManager object
        */
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

        /* CreateBackup Method
        * Creates a backup of the current database file using the current date and
        * time in seconds format (UNIX Style).
        * @param applicationPath - The directory that the program exists in
        */
        public void CreateBackup(string applicationPath)
        {
            // Convert current date/Time to UNIX date/time and create a backup file with that name
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            CreateBackup(applicationPath, ((long)t.TotalSeconds).ToString() + ".sqlite");
        }

        /* CreateBackup Method with FileName 
        * Creates a backup of the current database file using the specified backup file
        * name. Useful for creating SNAPSHOTS before certain events for restore-points.
        * Note that all backups are automatically stored in applicationPath\backups.
        * @param applicationPath - The directory the program exists in
        * @param FileName - The name of the backup file
        */
        public void CreateBackup(string applicationPath, string FileName)
        {
            // If there is no backup directory, make it.
            if (!(System.IO.Directory.Exists(applicationPath + "\\backups")))
                System.IO.Directory.CreateDirectory(applicationPath + "\\backups");

            // If there is a file to backup, back it up (and overwrite contents)
            if (System.IO.File.Exists(applicationPath + "\\" + DATABASE_FILE))
                System.IO.File.Copy(applicationPath + "\\" + DATABASE_FILE, applicationPath + "\\backups\\" + FileName, true);
        }

        /* Restore Backup Method with Filename
        * Restores a selected backup (from the backup directory) by copying it into
        * the current database file and overwriting it. The file to restore is specified
        * by the user.
        * @param applicationPath - The directory the program exists in
        * @param FileName - The name of the backup file to restore
        */
        public void RestoreBackup(string applicationPath, string FileName)
        {
            CreateBackup(applicationPath); // Backup the current config before overwriting it with a restore
            System.IO.File.Copy(applicationPath + "\\backups\\" + FileName, applicationPath + "\\" + DATABASE_FILE, true);
        }

        /* Purge Backups Method
        * Deletes all backup files that exceed the day limit provided as a parameter.
        * @param applicationPath - The directory the program exists in
        * @param DayLimit - The oldest a backup file can be before it is deleted.
        */
        public void PurgeBackups(string applicationPath, int DayLimit)
        {
            // Loop through all backup files
            foreach (String filename in System.IO.Directory.GetFiles(applicationPath + "\\backups\\", "*.sqlite", System.IO.SearchOption.TopDirectoryOnly))
            {
                System.IO.FileInfo info = new System.IO.FileInfo(filename);
                if ((int)DateTime.UtcNow.Subtract(info.CreationTimeUtc).TotalDays > DayLimit)
                    info.Delete();
            }
        }

        /* Get Most Recent Backup Method
        * Scans through the backup directory looking at all potential backup files and finding the most
        * recent one using the creationdate attribute. 
        * @param applicationPath - The directory that the program exists in
        * @return The file name of the most recently created backup file. null If none exist
        */
        public string GetMostRecentBackup(string applicationPath)
        {
            System.IO.FileInfo mostRecentFile = null;
            DateTime mostRecentTime = DateTime.MinValue;
            foreach (String fileName in System.IO.Directory.GetFiles(applicationPath + "\\backups\\", "*.sqlite", System.IO.SearchOption.TopDirectoryOnly))
            {
                System.IO.FileInfo f = new System.IO.FileInfo(fileName);
                if (f.CreationTimeUtc.Subtract(mostRecentTime).TotalSeconds > 0)
                {
                    mostRecentFile = f;
                    mostRecentTime = f.CreationTimeUtc;
                }
            }
            if (mostRecentFile == null)
                return null;
            else
                return mostRecentFile.Name;

        }

        /* OpenConnection method
        * Opens a connection to the database (if not already open) and
        * returns a boolean indicative of the success. Also handles cases where 
        * timing may have been an issue by prompting the user with an option to
        * retry the operation.
        * @return A boolean indicating whether or not the connection has been opened
        */
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
                // Provide the user with a chance to retry on error.
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


        /* Database Exists method
        * This method checks to see if the database file exists. It does not
        * actually verify the integrity of the database, but simply checks if the
        * database file exists.
        * @return A boolean indicating whether or not the database file exists
        */
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

        /* CreateDatabase Method
        * This method creates the database file in the filesystem.
        * @return A boolean indicating the success of the creation of the database file.
        */
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
                // On error, give the user a chance to retry
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

        /* AddTable Method
        * Adds a table name (and its SQLite creation string) to the 
        * dictionary of tables, allowing the table to be verified or 
        * created.
        * @param TableName - the name of the table
        * @param TableString - The SQLite Table creation string 
        */
        public void AddTable(string TableName, string TableString)
        {
            if (this.tables == null)
                this.tables = new Dictionary<string, string>();

            this.tables.Add(TableName, TableString);
        }

        /* Create All Tables Method
        * Should be called directly after adding all tables using the `AddTable` method.
        * Creates any missing tables from the table dictionary and adds them to 
        * the database file. Console output is written whenever there
        * is a table that does not exist.
        * @return A boolean indicating whether or not tables where created.
        */
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
                        // Most likely this table already exists in the database. Spit out a message and move on
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

        /* Tables Are Verified Method
        * Gets a list of tables that currently exist within the
        * database file and compares them with the table design that 
        * exists in the table dictionary. This method checks to 
        * make sure that the two match (in table names only).
        * @return A boolean indicating whether or not the table names 
        *         in the database match those defined in the table dictionary
        */
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

        /* Create Database Tables Method
        * DEPRECATED.
        * Creates a database table with the specified name.
        * Deprecated in favor of the new AddTable->CreateAllTables logic.
        * @param TableString - The SQLite table creation string
        * @return A boolean indicating the success of the table creation
        */
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

        /* Insert Into Table method
        * Inserts specific values into the SQLite table.
        * @param TableName - The table to insert the values into
        * @param Fields    - The fields to insert the values in (delineated by comma)
        * @param Values    - The values to insert into their fields (in respective order, delineated by comma)
        * @return The ID of the newly inserted row.
        */
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

        /* Select From Table Method
        * Selects the specified fields from a table using the provided options
        * @param Fields    - The fields to select from the table
        * @param TableName - The name of the table to select from
        * @param Options   - The options to decide where to select (ie WHERE ID=0)
        * @return A reader object that can be used to get all information about the results of the select query
        */
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

        /* Update Table Method
        * Updates the specified fields of the specified table using the specified options.
        * @param TableName - The name of the table to update
        * @param SetString - The SET clause of a SQLite update query
        * @param Options   - The options about the update query (ie WHERE ID=0)
        * @return A boolean value indicating the success of the update.
        */
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

        /* Delete From Table Method
        * Deletes a specific entry from a specific table where specified
        * @param TableName - The table to delete from
        * @param Options   - The options of the deletion (ie WHERE ID=0)
        * @return A boolean indicating the success of the deletion
        */
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

        /* CloseConnection Method
        * Closes the DatabaseManager objects connection to the database. This should
        * be called whenever consecutive queries are finished to avoid memeory leaks.
        */
        public void CloseConnection()
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }
    }
}

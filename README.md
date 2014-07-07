SQLiteDatabaseManager
=====================

## Background ##
As I create more software for clients, I am continuing to think that 
it is a good idea to create more open-source libraries to use in my projects. 
This means that I can fix bugs within the libraries as they come up, and others 
can as well. This is another one of these libraries.

## Overview ##
This class serves as a simple wrapper for interaction with a .NET project 
and a SQLite database by providing easy methods with self-contained error catching 
that allows the user to easily implement database creation, editing, and reading.

## Technical Overview ##
Although this is a library and should be able to be universally implemented, 
I decided to make this library use MessageBoxes to interact with the end-user. 
Although this may not be the best idea, I will leave any changes to this system 
until they are needed by anyone.

This library uses the System.Data.SQLite library to communicate with SQLite databases.

## TO-DO LIST ##
0) Make a to-do list.
## [Version 0.4.0] - 01/15/2024

- Possibility to customize table name using replaces;
- If entity lacks a primary key, the first property can be considered as the identifier;
- Considered the Column attribute of a property as the column name of the table;
- Allow defining a minimum size for columns.


## [Version 0.3.0] - 12/03/2023

- Fixed and optimized inclusion of Foreign Key entities;
- Tables can be created simply by providing the entity;
- Set default types for the columns of the table to be created in the database;
- CustomCommand has been deprecated;
- Execute custom queries using the ExecuteSelect and ExecuteNonQuery methods;
- Deleting entities from the database is now possible;
- It's possible to set the maximum size of a column when creating a table;
- Use the namePropUnique parameter to ensure unique entity insertions;
- The Search method allows searching for a specific entity in the database;
- CreateTableIfNotExist allows creating tables without errors if they already exist.


## [Version 0.2.0] - 11/26/2023

- Possible manipulation of FK entities (considering include, virtual, etc);
- `CustomCommand` to perform custom commands;
- PK property can be automatically inferred from the entity;
- Possible to obtain only the string of commands for independent execution from the library;
- When performing updates, returns the number of modifications instead of true or false.


## [Version 0.1.0] - 09/12/2023

- Obtains the table name from class attribute;
- Considers `NotMapped` attribute;
- Compatible only with Oracle database;
- Support for NetFramework;
- Possible to perform insert, update, and select operations on entities;
- First version of the library published (alpha version).
[![NuGet](https://img.shields.io/nuget/v/EnttityHelper.svg)](https://www.nuget.org/packages/EnttityHelper/)

<img src="https://raw.githubusercontent.com/DiegoPiovezana/DiegoPiovezana/main/Images/br.png" width=2.0% height=2.0%> Veja a documentação em português [clicando aqui](EnttityHelper/Globalization/Readme_pt-br.md).<br/>

# EnttityHelper [Alpha]
Allows easy manipulation of entities in different databases.<br/>

<img src="EnttityHelper\Images\EnttityHelper_publish.png" width=100% height=100%><br/>
<br/>
[![Changelog](https://img.shields.io/badge/Changelog-View%20Here-blue.svg)](CHANGELOG.md) [![License](https://img.shields.io/badge/License-GPL-yellow.svg)](LICENSE.txt)

## AVAILABLE FEATURES:<br/>
✔ Open-Source;<br/>
✔ Uses ADO.NET;<br/>
✔ Compatible with different databases such as Oracle (SqlServer and SqLite coming soon);<br/>
✔ Performs the main operations: update, insert, select and delete entities;<br/>
✔ Allows you to create a table in the database according to properties of a C# object;<br/>
✔ Able to consider attributes of an object's properties to create a table;<br/>
✔ Independent manipulations: Entities can be manipulated without needing to be part of a Context;<br/>
✔ Secure manipulations: if the amount of change is not as expected, the transaction will not be effective;<br/>
✔ Perform entity, DataTable, IDataReader, or DataRow[] insertions efficiently;<br/>
✔ Possible to define the table names and column types in a fully customized or automatic way;<br/>
✔ Capable of creating tables from a DataTable;<br/>
✔ Select from one database and insert the result of that select into another database (`InserLinkSelect`);<br/>
✔ [Coming soon] Possible to establish Many-to-Many relationships.<br/>

<br/>

### CONTACT:
https://bit.ly/FeedbackHappyHelper

<br/><br/>

## INSTALLATION:
```
 dotnet add package EnttityHelper
```

<br/>

## EXAMPLE OF CRUD USAGE:
```c#
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExampleEntity
{
    [Table("TB_USER")]
    internal class User
    {
        [Key()] public int Id { get; internal set; }
        [Required][MaxLength(300)] public string Name { get; internal set; }
        [Required][MaxLength(100)] public string? GitHub { get; internal set; }
        public DateTime DtCreation { get; internal set; }
        [ForeignKey(nameof(Career))] public long IdCareer { get; internal set; }
        public virtual Career? Career { get; internal set; }        

        public User() { } // Mandatory empty constructor  
    }
}
```

```c#
using EH;

namespace App
{
    static class Program
    {
        static void Main()
        {
            // Creates a connection with the database using the connection string
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");

            if (eh.DbContext.ValidateConnection())
            {
                // Creates table - User object     
                eh.CreateTableIfNotExist<User>();

                // Creates a new entity
                User userD = new() { Id = 0, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };

                // Inserts into the database
                eh.Insert(userD);

                // Modifies the entity
                userD.Name = "Diêgo Piovezana";

                // Updates in the database
                eh.Update(userD);

                // Searches in the database
                User? userDSearched1 = eh.Search(userD);
                User? userDSearched2 = eh.Search(new User { Name = "John" }, true, nameof(User.Name));                              

                // Deletes user D from the database
                eh.Delete(userD);

                // Gets all users registered in the last week
                List<User>? usersWeek = eh.Get<User>()?.Where(u => u.DtCreation > DateTime.Now.AddDays(-7)).ToList();
            }
            else
            {
                Console.WriteLine("Unable to establish a connection to the database!");              
            }
        }
    }
}

```
<br/>

## EXAMPLE OF DATATABLE INSERTION:
```c#
using EH;

namespace App
{
    static class Program
    {
        static void Main()
        {
            // Creates a connection with the database using the connection string
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");

            if (eh.DbContext.ValidateConnection())
            {
                // Empty columns will automatically have the Object type. In the database, the Object type will be NVARCHAR2(100)
                eh.TypesDefault.Add("Object", "NVARCHAR2(100)");

                // Performs the reading of the first tab of the DataTable
                var dt = SheetHelper.GetDataTable(@"C:\Users\diego\Desktop\Tests\Converter\ColunasExcel.xlsx", "1");

                // If the table exists, it will be deleted
                if (eh.CheckIfExist("TableX")) eh.ExecuteNonQuery($"DROP TABLE TableX");

                // Possible to insert DataTable considering various scenarios
                eh.Insert(dt,null,true,"TableX"); 
                //eh.Insert(dt, null, true); // The table name will automatically be the name of the spreadsheet tab (removing special characters)
                //eh.Insert(dt, null, false); // The table will not be created and only the insertion of the DataTable will occur 
            }
            else
            {
                Console.WriteLine("Unable to establish a connection to the database!");              
            }
        }
    }
}
```
<br/>

## EXAMPLE OF LINK SELECT INSERTION:
```c#
using EH;

namespace App
{
    static class Program
    {
        static void Main()
        {
            // Creates a connection with the database using the connection string
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");

            if (eh.DbContext.ValidateConnection())
            {
                // Select in the database table from database 1
                string query = "SELECT * FROM SHEET8";

                // Creates a new connection with database 2
                EnttityHelper eh2 = new($"Data Source=152.27.13.90:49262/xe2;User Id=system2;Password=oracle2");

                // Inserts the result of the select into the table of database 2
                eh.InsertLinkSelect(query, eh2, "TEST_LINKSELECT");
            }
            else
            {
                Console.WriteLine("Unable to establish a connection to the database!");              
            }
        }
    }
}
```
<br/>
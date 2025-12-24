[![Português](https://img.shields.io/static/v1?label=Veja%20em&message=Português&color=FEDD00&labelColor=009739)](https://github.com/DiegoPiovezana/EnttityHelper/blob/main/EnttityHelper/Globalization/Readme_pt-br.md)


# EnttityHelper [Beta]
Allows easy manipulation of entities in different databases.

![EnttityHelper](https://raw.githubusercontent.com/DiegoPiovezana/EnttityHelper/888a0c2c276cbaf18a44e6123bf1e650f872d08f/EnttityHelper/Images/EnttityHelper_publish.png)

[![GitHub](https://img.shields.io/badge/GitHub-View%20Here-blue?logo=github)](https://github.com/DiegoPiovezana/EnttityHelper)

## AVAILABLE FEATURES:
* ✔ **Open-Source**
* ✔ **Uses ADO.NET**
* ✔ **Compatible with different databases** such as Oracle and SqlServer
* ✔ **CRUD Operations:** Update, insert, select and delete entities
* ✔ **Schema Generation:** Create tables based on C# objects or DataTables
* ✔ **Attributes Support:** Considers DataAnnotations (Key, Required, MaxLength, Table, etc)
* ✔ **Independent manipulations:** Entities don't need a Context
* ✔ **Secure manipulations:** Transaction-based changes
* ✔ **Bulk Operations:** Efficient insertion of DataTables or IDataReader
* ✔ **InserLinkSelect:** Select from one database and insert directly into another


### CONTACT:
https://bit.ly/FeedbackHappyHelper


## INSTALLATION:
```bash
 dotnet add package EnttityHelper
```

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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");

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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");

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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");

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

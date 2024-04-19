[![NuGet](https://img.shields.io/nuget/v/EnttityHelper.svg)](https://www.nuget.org/packages/EnttityHelper/)

<img src="https://raw.githubusercontent.com/DiegoPiovezana/DiegoPiovezana/main/Images/br.png" width=2.0% height=2.0%> Veja a documentação em português [clicando aqui](Globalization/Readme_pt-br.md).<br/>

# EnttityHelper [Beta]
Allows easy manipulation of entities in different databases.<br/>


<img src="Images\EnttityHelper_publish.png" width=100% height=100%><br/>

## AVAILABLE FEATURES:<br/>
✔ Open-Source;<br/>
✔ Uses ADO.NET;<br/>
✔ Compatible with different databases such as Oracle (SqlServer and SqLite coming soon);<br/>
✔ Performs the main operations: update, insert, select and delete entities;<br/>
✔ Allows you to create a table in the database according to properties of a C# object;<br/>
✔ Able to consider attributes of an object's properties to create a table;<br/>
✔ Entities can be manipulated without needing to be part of a Context;<br/>
✔ Independent manipulations: if the database is out of sync with the C# code, the manipulations may still work.<br/>

<br/>

### CONTACT:
https://bit.ly/FeedbackHappyHelper

<br/><br/>

## INSTALLATION:
```
 dotnet add package EnttityHelper --version 0.5.0
```

<br/>

## EXAMPLE OF USE:
```c#
using EH;

namespace App
{
    static class Program
    {
        static void Main()
        {
            // Create a connection with the database using the connection string
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");

            if (eh.DbContext.ValidateConnection())
            {
                // Create table - Object User     
                eh.CreateTableIfNotExist<User>();

                // Create new entity
                User userD = new() { Id = 0, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now };

                // Insert in database
                eh.Insert(userD);

                // Modify entity
                userD.Name = "Diêgo Piovezana";

                // Update in database
                eh.Update(userD);

                // Search in database
                User? userDSearched = eh.Search(userD);                               

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
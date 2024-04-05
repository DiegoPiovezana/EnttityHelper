[![NuGet](https://img.shields.io/nuget/v/EnttityHelper.svg)](https://www.nuget.org/packages/EnttityHelper/)

<img src="https://github.com/DiegoPiovezana/DiegoPiovezana/blob/main/Images/us.png?raw=true" width=2.0% height=2.0%> See the documentation in English by [clicking here](../../Readme.md).<br/>

# EnttityHelper [Beta]
Possibilita a fácil manipulação de entidades em diferentes bancos de dados.<br/>


<img src="..\Images\EnttityHelper_publish.png" width=100% height=100%><br/>

## RECURSOS DISPONÍVEIS:<br/>
✔ Open-Source;<br/>
✔ Usa ADO.NET;<br/>
✔ Compatível com diferentes bancos de dados, como Oracle (SqlServer e SqLite em breve);<br/>
✔ Realiza as principais operações: atualizar, inserir, selecionar e excluir entidades;<br/>
✔ Permite criar uma tabela no banco de dados de acordo com as propriedades de um objeto C#;<br/>
✔ Capaz de considerar atributos das propriedades de um objeto para criar uma tabela;<br/>
✔ As entidades podem ser manipuladas sem precisar fazer parte de um Contexto;<br/>
✔ Manipulações independentes: se o banco de dados estiver dessincronizado com o código C#, as manipulações ainda podem funcionar.<br/>

<br/>

### CONTATO:
https://bit.ly/FeedbackHappyHelper

<br/><br/>

## INSTALAÇÃO:
```
 dotnet add package EnttityHelper --version 0.4.0
```	

<br/>

## EXEMPLO DE USO:
```c#
using EH;

namespace App
{
    static class Program
    {
        static void Main()
        {
            // Cria uma conexão com o banco de dados usando a string de conexão
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");

            if (eh.DbContext.ValidateConnection())
            {
                // Cria tabela - Objeto User     
                eh.CreateTableIfNotExist<User>();

                // Cria uma nova entidade
                User userD = new() { Id = 0, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now };

                // Insere no banco de dados
                eh.Insert(userD);

                // Modifica a entidade
                userD.Name = "Diêgo Piovezana";

                // Atualiza no banco de dados
                eh.Update(userD);

                // Procura no banco de dados
                User? userDSearched = eh.Search(userD);                               

                // Deleta o usuário D do banco de dados
                eh.Delete(userD);

                // Obtém todos os usuários registrados na última semana
                List<User>? usersWeek = eh.Get<User>()?.Where(u => u.DtCreation > DateTime.Now.AddDays(-7)).ToList();
            }
            else
            {
                Console.WriteLine("Não foi possível estabelecer uma conexão com o banco de dados!");              
            }
        }
    }
}
```
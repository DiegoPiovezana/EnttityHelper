[![NuGet](https://img.shields.io/nuget/v/EnttityHelper.svg)](https://www.nuget.org/packages/EnttityHelper/)

<img src="https://raw.githubusercontent.com/DiegoPiovezana/DiegoPiovezana/main/Images/us.png" width=2.0% height=2.0%> See the documentation in English by [clicking here](../../Readme.md).<br/>

# EnttityHelper [Alpha]
Possibilita a fácil manipulação de entidades em diferentes bancos de dados.<br/>

<img src="..\Images\EnttityHelper_publish.png" width=100% height=100%><br/>
<br/>
[![Changelog](https://img.shields.io/badge/Changelog-View%20Here-blue.svg)](../../CHANGELOG.md) [![License](https://img.shields.io/badge/License-GPL-yellow.svg)](../../LICENSE.txt)

## RECURSOS DISPONÍVEIS:<br/>
✔ Open-Source;<br/>
✔ Usa ADO.NET;<br/>
✔ Compatível com diferentes bancos de dados, como Oracle (SqlServer e SqLite em breve);<br/>
✔ Realiza as principais operações: atualizar, inserir, selecionar e excluir entidades;<br/>
✔ Permite criar uma tabela no banco de dados de acordo com as propriedades de um objeto C#;<br/>
✔ Capaz de considerar atributos das propriedades de um objeto para criar uma tabela;<br/>
✔ Manipulações independentes: as entidades podem ser manipuladas sem precisar fazer parte de um Contexto;<br/>
✔ Manipulações seguras: se a quantidade de alteração não é a esperada, a transação não será efetivada;<br/>
✔ Realize inserções de entidades, DataTable, IDataReader ou DataRow[] de maneira eficiente;<br/>
✔ Possível definir o nome das tabelas e o tipo das colunas de maneira totalmente personalizada ou automática;<br/>
✔ Capaz de criar tabelas a partir de uma DataTable;<br/>
✔ Faça select em um banco e insira o resultado desse select em outro banco (`InserLinkSelect`);<br/>
✔ [Em breve] Possível estabelecer relações Muito-para-Muitos.<br/>

<br/>

### CONTATO:
https://bit.ly/FeedbackHappyHelper

<br/><br/>

## INSTALAÇÃO:
```
 dotnet add package EnttityHelper --version 0.5.0-alpha-1
```	
<br/>


## EXEMPLO DE USO EM CRUD:
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

        public User() { } // Construtor vazio obrigatório  
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
            // Cria uma conexão com o banco de dados usando a string de conexão
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");

            if (eh.DbContext.ValidateConnection())
            {
                // Cria tabela - Objeto User     
                eh.CreateTableIfNotExist<User>();

                // Cria uma nova entidade
                User userD = new() { Id = 0, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };

                // Insere no banco de dados
                eh.Insert(userD);

                // Modifica a entidade
                userD.Name = "Diêgo Piovezana";

                // Atualiza no banco de dados
                eh.Update(userD);

                // Procura no banco de dados
                User? userDSearched1 = eh.Search(userD);
                User? userDSearched2 = eh.Search(new User { Name = "John" }, true, nameof(User.Name));                              

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
<br/>

## EXEMPLO DE INSERT DE DATATABLE:
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
                // Colunas vazias terão automaticamente o tipo Object. No banco de dados, o tipo Object será NVARCHAR2(100)
                eh.TypesDefault.Add("Object", "NVARCHAR2(100)");

                // Realiza a leitura da primeira aba do DataTable
                var dt = SheetHelper.GetDataTable(@"C:\Users\diego\Desktop\Tests\Converter\ColunasExcel.xlsx", "1");

                // Se a tabela existir, ela será excluída
                if (eh.CheckIfExist("TableX")) eh.ExecuteNonQuery($"DROP TABLE TableX");

                // Possível inserir o DataTable considerando diversos cenários
                eh.Insert(dt,null,true,"TableX"); 
                //eh.Insert(dt, null, true); // O nome da tabela será automaticamente o nome da aba da planilha (retirando caracteres especiais)
                //eh.Insert(dt, null, false); // A tabela não será criada e apenas ocorrerá a inserção do DataTable 
            }
            else
            {
                Console.WriteLine("Não foi possível estabelecer uma conexão com o banco de dados!");              
            }
        }
    }
}
```
<br/>

## EXEMPLO DE INSERT LINK SELECT:
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
                // Select na tabela do banco de dados do banco de dados 1
                string query = "SELECT * FROM SHEET8";

                // Cria uma nova conexão com o banco de dados 2
                EnttityHelper eh2 = new($"Data Source=152.27.13.90:49262/xe2;User Id=system2;Password=oracle2");

                // Insere o resultado do select na tabela do banco de dados 2
                eh.InsertLinkSelect(query, eh2, "TEST_LINKSELECT");
            }
            else
            {
                Console.WriteLine("Não foi possível estabelecer uma conexão com o banco de dados!");              
            }
        }
    }
}
```
<br/>
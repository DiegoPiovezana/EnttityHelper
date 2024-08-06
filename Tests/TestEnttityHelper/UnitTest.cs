using EH;
using NUnit.Framework.Internal;
using Oracle.ManagedDataAccess.Client;
using SH;
using TestEH_UnitTest.Entities;
using TestEH_UnitTest.Entitities;
using TestEnttityHelper.OthersEntity;

namespace TestEnttityHelper
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test, Order(1)]
        public void TestPass()
        {
            Assert.Pass();
        }

        [Test, Order(2)]
        public void TestConnection()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            bool test = eh.DbContext.ValidateConnection();
            Assert.That(test, Is.EqualTo(true));
        }

        [Test, Order(3)]
        public void TestCreateTableIfNotExist()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                bool result = eh.CreateTableIfNotExist<EntityTest>();
                Assert.That(result, Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(4)]
        public void TestCreateTable()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                // Oracle.ManagedDataAccess.Client.OracleException : ORA-00955: name is already used by an existing object
                Assert.Throws<OracleException>(() => { eh.CreateTable<EntityTest>(); });
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(5)]
        public void TestInsertEntity()
        {
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                //bool result = eh.Insert(new { Id = 1, Name = "Test" }, null);

                //EntityTest entityTest = new() { Id = 90, Name = "Testando entidade 90 via C#", StartDate = DateTime.Now };
                User entityTest = new("Diego Piovezana") { Id = 1, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };
                
                //bool result = eh.Insert(entityTest);
                bool result = eh.Insert(entityTest, nameof(entityTest.Id), true) == 1;

                if (result) { eh.Delete(entityTest); }

                Assert.That(result, Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(6)]
        public void TestUpdateEntity()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 90, Name = "Testing entity 90 updating start time via C#", StartDate = DateTime.Now };
                bool result = eh.Update(entityTest, nameof(entityTest.Id)) == 1;

                Assert.That(result, Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(7)]
        public void TestSearchEntity()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 90 };
                var result = eh.Search(entityTest, true, nameof(entityTest.Id));

                Assert.That(result?.Name.Equals("Testing entity 90 updating start time via C#"), Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(8)]
        public void TestGetEntity()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                var result = eh.Get<EntityTest>();
                if (result is null) { Assert.Fail(); }
                //else { Assert.That(result[0].StartDate.Date.Equals(DateTime.Now.AddMinutes(-10)), Is.EqualTo(true)); }
                //else { Assert.That(result[0].StartDate, Is.LessThanOrEqualTo(DateTime.Now.AddMinutes(-10))); }
                else { Assert.That(result[0].StartDate, Is.GreaterThanOrEqualTo(DateTime.Now.AddMinutes(-15))); }
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(9)]
        public void TestNonQuery()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                bool result = eh.ExecuteNonQuery("DELETE FROM TB_ENTITY_TEST WHERE ID = 90") == 1;
                Assert.That(result, Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(10)]
        public void TestFullEntity()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 300, Name = "Testando 1 entidade 100 via C#", StartDate = DateTime.Now };
                eh.Insert(entityTest, nameof(entityTest.Id));

                EntityTest entityTest2 = new() { Id = 300, Name = "Testando 2 entidade 300 atualizando hora via C#", StartDate = DateTime.Now };
                eh.Update(entityTest2, nameof(entityTest.Id));

                var entities = eh.Get<EntityTest>(true, $"{nameof(EntityTest.Id)} = 300");

                if (entities is not null && entities[0].Name.Equals("Testando 2 entidade 300 atualizando hora via C#"))
                {
                    int result = eh.ExecuteNonQuery("DELETE FROM TB_ENTITY_TEST WHERE ID = 300");
                    Assert.That(result == 1, Is.EqualTo(true));
                }
            }
            else
            {
                Assert.Fail();
            }
        }


        [Test, Order(11)]
        public void TestOneToOne()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 300, Name = "Testando 1 entidade 100 via C#", StartDate = DateTime.Now };
                eh.Insert(entityTest, nameof(entityTest.Id));

                EntityTest entityTest2 = new() { Id = 300, Name = "Testando 2 entidade 300 atualizando hora via C#", StartDate = DateTime.Now };
                eh.Update(entityTest2, nameof(entityTest.Id));

                var entities = eh.Get<EntityTest>(true, $"{nameof(EntityTest.Id)} = 300");

                if (entities is not null && entities[0].Name.Equals("Testando 2 entidade 300 atualizando hora via C#"))
                {
                    int result = eh.ExecuteNonQuery("DELETE FROM TB_ENTITY_TEST WHERE ID = 300");
                    Assert.That(result == 1, Is.EqualTo(true));
                }
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(12)]
        public void TestManyToOne()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                //eh.CreateTableIfNotExist<Career>();
                //eh.CreateTableIfNotExist<User>();

                //Career carrer = new(1,"Developer");
                //eh.Insert(carrer);

                //User user = new("Diego Piovezana") { Id = 0, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };
                //eh.Insert(user);

                var carrers = eh.Get<Career>();
                var users = eh.Get<User>();

                Assert.Pass();

            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(13)]
        public void TestManyToMany()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                /////////////////////////////////////////////////// 
                // CREATE TABLE

                if (eh.CheckIfExist("TB_GROUP_USERSTOGROUPS")) eh.ExecuteNonQuery("DROP TABLE TB_GROUP_USERSTOGROUPS");
                if (eh.CheckIfExist(eh.GetTableName<Group>())) eh.ExecuteNonQuery($"DROP TABLE {eh.GetTableName<Group>()}");
                if (eh.CheckIfExist(eh.GetTableName<User>())) eh.ExecuteNonQuery($"DROP TABLE {eh.GetTableName<User>()}");
                if (eh.CheckIfExist(eh.GetTableName<Career>())) eh.ExecuteNonQuery($"DROP TABLE {eh.GetTableName<Career>()}");

                //eh.CreateTableIfNotExist<Group>(true);
                //eh.CreateTableIfNotExist<User>(false);

                eh.CreateTableIfNotExist<User>(true);
                eh.CreateTableIfNotExist<Group>(false);

                // TODO: Entity FK necessary only for the Get (include)
                eh.CreateTableIfNotExist<Career>();

                /////////////////////////////////////////////////// 
                // INSERT

                Career carrer = new() { IdCareer = 1, Name = "Pleno", CareerLevel = 2, Active = true };
                eh.Insert(carrer);
                
                Career carrer2 = new() { IdCareer = 3, Name = "Trainee", CareerLevel = 0, Active = true };
                eh.Insert(carrer2);                                
                
                Group group1 = new() { Id = 1, Name = "Developers", Description = "Developer Group" };
                eh.Insert(group1);

                Group group2 = new() { Id = 2, Name = "Testers", Description = "Tester Group" };
                eh.Insert(group2);

                //eh.ExecuteNonQuery("DELETE FROM TB_USER");
                User user = new() { Id = 1, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };
                List<Group> groupsUser = new() { group1, group2 };
                foreach (var group in groupsUser) { user.Groups.Add(group); }
                eh.Insert(user);


                User user2 = new() { Id = 2, Name = "John Victor", GitHub = "@JohnVictor", DtCreation = DateTime.Now, IdCareer = 3 };
                eh.Insert(user2);

                Group group3 = new() { Id = 3, Name = "Operation", Description = "Operation Group" };
                group3.Users.Add(user2);
                eh.Insert(group3);


                /////////////////////////////////////////////////// 
                // GET
                var carrers = eh.Get<Career>();
                var users = eh.Get<User>();
                var groups = eh.Get<Group>();


                /////////////////////////////////////////////////// 
                // UPDATE
                User userUpdate = new() { Id = 1, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };
                userUpdate.Groups.Add(group2);
                eh.Update(userUpdate);

                var usersUpdated = eh.Get<User>();

                Assert.Pass();
            }
            else
            {
                Assert.Fail("Connection failed!");
            }
        }

        [Test, Order(14)]
        public void TestInsertDataTable()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                // Empty columns will automatically have the Object type. In the database, the Object type will be NVARCHAR2(100)
                eh.TypesDefault.Add("Object", "NVARCHAR2(100)");

                // Reads the first tab of the DataTable
                var dt = SheetHelper.GetDataTable(@"C:\Users\diego\Desktop\Tests\Converter\ColunasExcel.xlsx", "1");

                // If the table exists, it will be deleted
                if (eh.CheckIfExist("TableX")) eh.ExecuteNonQuery($"DROP TABLE TableX");

                // Possible to insert the DataTable considering different scenarios
                eh.Insert(dt, null, true, "TableX");
                //eh.Insert(dt, null, true); // The table name will automatically be the name of the spreadsheet tab (removing special characters)
                //eh.Insert(dt, null, false); // The table will not be created and only the DataTable will be inserted              

                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(15)]
        public void TestInsertLinkSelect()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                // Select from database table from database 1
                string query = "SELECT * FROM SHEET8";

                // Create a new connection to database 2
                EnttityHelper eh2 = new($"Data Source=152.27.13.90:49262/xe2;User Id=system2;Password=oracle2");

                // Insert the result of the select into the database table 2
                eh.InsertLinkSelect(query, eh2, "TEST_LINKSELECT");

                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(16)]
        public void TestFullEntityREADME()
        {
            // Create a connection with the database using the connection string
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");

            if (eh.DbContext.ValidateConnection())
            {
                // Create table - Object User     
                eh.CreateTableIfNotExist<User>();

                // Create new entity
                User userD = new("Diego Piovezana") { Id = 1, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };

                // Insert in database
                eh.Insert(userD);

                // Modify entity
                userD.Name = "Diego Piovezana";

                // Update in database
                eh.Update(userD);

                // Search in database
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
                Assert.Fail();
            }
        }

        [Test, Order(17)]
        public void TestManyInsertions()
        {
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                // Create many entities
                User user1 = new("Diego Piovezana") { Id = 1, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };
                User user2 = new("User Test One") { Id = 2, GitHub = "@UserTestOne", DtCreation = DateTime.Now, IdCareer = 2 };
                User user3 = new("User Test Two") { Id = 3, GitHub = "@UserTestTwo", DtCreation = DateTime.Now, IdCareer = 3 };

                List<User>? users = new() { user1, user2, user3 };

                // Inserts the entities
                int result = eh.Insert(users); 
                
                Assert.That(result == 3, Is.EqualTo(true));                
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(17)]
        public void TestManyUpdates()
        {
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                // Create many entities
                User user1 = new("Diego Piovezana") { Id = 1, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 3 };
                User user2 = new("User Test One") { Id = 2, GitHub = "@UserTestOne", DtCreation = DateTime.Now, IdCareer = 1 };
                User user3 = new("User Test Two") { Id = 3, GitHub = "@UserTestTwo", DtCreation = DateTime.Now, IdCareer = 1 };

                List<User>? users = new() { user1, user2, user3 };

                // Updates the entities
                int result = eh.Update(users);

                Assert.That(result == 3, Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
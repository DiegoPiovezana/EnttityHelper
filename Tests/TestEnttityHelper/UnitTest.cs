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
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                //bool result = eh.Insert(new { Id = 1, Name = "Test" }, null);

                EntityTest entityTest = new() { Id = 90, Name = "Testando entidade 90 via C#", StartDate = DateTime.Now };
                //bool result = eh.Insert(entityTest);
                bool result = eh.Insert(entityTest, nameof(entityTest.Id)) == 1;

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
                eh.CreateTableIfNotExist<Group>();

                //eh.ExecuteNonQuery("DROP TABLE TB_USER");
                eh.CreateTableIfNotExist<User>();

                Group group1 = new() { Id = 0, Name = "Developers", Description = "Developer Group" };
                eh.Insert(group1);

                Group group2 = new() { Id = 1, Name = "Testers", Description = "Tester Group" };
                eh.Insert(group2);

                User user = new("Diego Piovezana") { Id = 0, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1, IdGroups = new List<int>() { 0, 1 } };
                eh.Insert(user);

                var carrers = eh.Get<Career>();
                var users = eh.Get<User>();

                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(14)]
        public void TestInsertDataTable()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                eh.TypesDefault.Add("Object", "NVARCHAR2(100)");

                var dt = SheetHelper.GetDataTable(@"C:\Users\diego\Desktop\Tests\Converter\ColunasExcel.xlsx", "Sheet8");
                eh.Insert(dt);

                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(154)]
        public void TestInsertLinkSelect()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                string query = "SELECT * FROM SHEET8";
                EnttityHelper eh2 = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");

                eh.InsertLinkSelect(query, eh2,"TEST_LINKSELECT");

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
                User userD = new("Diego Piovezana") { Id = 0, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now };

                // Insert in database
                eh.Insert(userD);

                // Modify entity
                userD.Name = "Diego Piovezana";

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
                Assert.Fail();
            }
        }



    }
}
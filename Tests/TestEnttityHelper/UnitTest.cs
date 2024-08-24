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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
            bool test = eh.DbContext.ValidateConnection();
            Assert.That(test, Is.EqualTo(true));
        }

        [Test, Order(3)]
        public void TestCreateTableIfNotExist()
        {
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                bool result = eh.CreateTableIfNotExist<EntityTest>(false);
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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                // Oracle.ManagedDataAccess.Client.OracleException : ORA-00955: name is already used by an existing object
                Assert.Throws<OracleException>(() => { eh.CreateTable<EntityTest>(false); });
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

                EntityTest entityTest = new() { Id = 90, Name = "Testando entidade 90 via C#", StartDate = DateTime.Now };
                //User entityTest = new("Diego Piovezana") { Id = 1, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };

                //bool result = eh.Insert(entityTest);
                int result = eh.Insert(entityTest, nameof(entityTest.Id), true);

                //if (result) { eh.Delete(entityTest); }

                Assert.That(result == 1, Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(6)]
        public void TestUpdateEntity()
        {
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
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
        public void TestGetEntity_SuccessfulRetrieval()
        {
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");

            if (eh.DbContext.ValidateConnection())
            {
                var result = eh.Get<EntityTest>();

                Assert.That(result, Is.Not.Null, "Result should not be null.");
                Assert.That(result, Has.Count.GreaterThanOrEqualTo(1), "At least one entity should be returned.");

                var firstEntity = result.First();
                Assert.That(firstEntity.StartDate, Is.LessThanOrEqualTo(DateTime.Now.AddMinutes(-10)), "StartDate should be 10 minutes earlier than now.");
                Assert.That(firstEntity.StartDate, Is.GreaterThanOrEqualTo(DateTime.Now.AddMinutes(-15)), "StartDate should be at most 15 minutes earlier than now.");
            }
            else
            {
                Assert.Fail("Failed to validate database connection.");
            }
        }

        [Test, Order(9)]
        public void TestGetEntity_FailureWhenNoConnection()
        {
            EnttityHelper eh = new($"Data Source=invalid;User Id=invalid;Password=invalid");

            Assert.Throws<InvalidOperationException>(() => eh.Get<EntityTest>(), "Should throw exception when connection is invalid.");
        }


        [Test, Order(9)]
        public void TestNonQuery()
        {
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
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
                eh.CreateTableIfNotExist<Career>(false);

                /////////////////////////////////////////////////// 
                // INSERT

                Career carrer1 = new() { IdCareer = 1, Name = "Pleno", CareerLevel = 2, Active = true };
                //eh.Insert(carrer);

                Career carrer2 = new() { IdCareer = 3, Name = "Trainee", CareerLevel = 0, Active = true };
                //eh.Insert(carrer2);

                var resultCarrers = eh.Insert(new List<Career>() { carrer1, carrer2 });
                Assert.That(resultCarrers == 2, Is.EqualTo(true));

                Group group1 = new() { Id = 1, Name = "Developers", Description = "Developer Group" };
                //eh.Insert(group1);

                Group group2 = new() { Id = 2, Name = "Testers", Description = "Tester Group" };
                //eh.Insert(group2);

                var resultGroups = eh.Insert(new List<Group>() { group1, group2 });
                Assert.That(resultGroups == 2, Is.EqualTo(true));

                eh.ExecuteNonQuery("DELETE FROM TB_USER");

                // Insert user with group
                User user1 = new() { Id = 1, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };
                List<Group> groupsUserAdd = new() { group1, group2 };
                foreach (var group in groupsUserAdd) { user1.Groups.Add(group); }
                Assert.That(eh.Insert(user1) == 1, Is.EqualTo(true));

                // Insert group with user
                User user2 = new() { Id = 2, Name = "John Victor", GitHub = "@JohnVictor", DtCreation = DateTime.Now, IdCareer = 3 };
                Assert.That(eh.Insert(user2) == 1, Is.EqualTo(true));

                Group group3 = new() { Id = 3, Name = "Operation", Description = "Operation Group" };
                group3.Users.Add(user2);
                Assert.That(eh.Insert(group3) == 1, Is.EqualTo(true));


                /////////////////////////////////////////////////// 
                // GET
                var carrers = eh.Get<Career>();
                var groups = eh.Get<Group>();
                var users = eh.Get<User>();
                Assert.Multiple(() =>
                {
                    Assert.That(carrers.Count == 2, Is.EqualTo(true));
                    Assert.That(groups.Count == 3, Is.EqualTo(true));
                    Assert.That(users.Count == 2, Is.EqualTo(true));
                });

                /////////////////////////////////////////////////// 
                // UPDATE
                //User userUpdate = new() { Id = 1, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };
                user1.Groups = new List<Group>() { group1 };
                eh.Update(user1);

                var usersUpdated = eh.Get<User>();
                Assert.That(usersUpdated.Count == 2, Is.EqualTo(true));

                var groupsUser1 = usersUpdated.Where(u => u.Id == 1).FirstOrDefault().Groups;
                Assert.Multiple(() =>
                {
                    Assert.That(groupsUser1.Count == 1, Is.EqualTo(true));
                    Assert.That(groupsUser1.FirstOrDefault().Name.Equals("Testers"), Is.EqualTo(true));
                });
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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");

            if (eh.DbContext.ValidateConnection())
            {
                // Create table - Object User     
                eh.CreateTableIfNotExist<User>(false);

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
        public void TestManyInsertionsSimple()
        {
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                eh.CreateTableIfNotExist<Group>(true); // Necessary to then create the MxN relationship table
                eh.CreateTableIfNotExist<User>(false);

                // Test for one entity
                User entityTest = new("Diego Piovezana One") { Id = 0, GitHub = "@DiegoPiovezanaOne", DtCreation = DateTime.Now, IdCareer = 1 };
                bool result1 = eh.Insert(entityTest, nameof(entityTest.GitHub), true) == 1;
                if (result1) { eh.Delete(entityTest); }
                Assert.That(result1, Is.EqualTo(true));

                // Create many entities
                User user1 = new("Diego Piovezana One Repeat") { Id = 0, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };
                User user2 = new("User Test One") { Id = 0, GitHub = "@UserTestOne", DtCreation = DateTime.Now, IdCareer = 2 };
                User user3 = new("User Test Two") { Id = 0, GitHub = "@UserTestTwo", DtCreation = DateTime.Now, IdCareer = 3 };

                List<User>? users = new() { user1, user2, user3 };

                // Inserts the entities
                int result2 = eh.Insert(users);

                Assert.That(result2 == 3, Is.EqualTo(true));

            }
        }

        [Test, Order(17)]
        public void TestManyInsertionsMxN()
        {
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                // INSERT THE MANY ENTITIES (MXN)
                Group group4 = new() { Id = 0, Name = "Masters", Description = "Masters Group" };
                Group group5 = new() { Id = 0, Name = "Managers", Description = "Managers Group" };
                List<Group> groups = new() { group4, group5 };
                int result3 = eh.Insert(groups, nameof(Group.Name), true);
                Assert.That(result3 == 2, Is.EqualTo(true));

                // It is necessary to first insert the groups, and then link them to the user
                // Otherwise, the local ID of groups 4 and 5 will be incorrectly used, instead of the one defined by the database.

                User userM = new("Maria da Silva") { Id = 0, GitHub = "@MariaSilva", DtCreation = DateTime.Now, IdCareer = 1 };
                userM.Groups.Add(group4);
                userM.Groups.Add(group5);
                int result4 = eh.Insert(userM, nameof(userM.GitHub), true);
                Assert.That(result4 == 3, Is.EqualTo(true));

                eh.ExecuteNonQuery($"DELETE FROM TB_GROUP_USERSTOGROUPS WHERE ID_TB_USER = {userM.Id}");
                eh.Delete(group4);
                eh.Delete(group5);
                eh.Delete(userM);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(17)]
        public void TestManyInsertionsNxM()
        {
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                // INSERT THE MANY ENTITIES (NXM)
                User userX = new("Xavier Souza") { Id = -404, GitHub = "@XavierSouza", DtCreation = DateTime.Now, IdCareer = 2 };
                User userY = new("Yasmin Corsa") { Id = -405, GitHub = "@YasminCorsa", DtCreation = DateTime.Now };
                List<User> users = new() { userX, userY };
                int result5 = eh.Insert(users, nameof(User.Name), false);
                Assert.That(result5 == 2, Is.EqualTo(true));

                Group group6 = new() { Id = 0, Name = "Group Six", Description = "Group Six Test" };
                group6.Users.Add(userX);
                group6.Users.Add(userY);
                int result6 = eh.Insert(group6, nameof(group6.Name), true);
                Assert.That(result6 == 3, Is.EqualTo(true));

                eh.ExecuteNonQuery($"DELETE FROM TB_GROUP_USERSTOGROUPS WHERE ID_TB_GROUP_USERS  = {group6.Id}");
                eh.Delete(userX);
                eh.Delete(userY);
                eh.Delete(group6);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(18)]
        public void TestManyUpdates()
        {
            EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                // Create many entities
                User user1 = new("Diego Piovezana") { Id = 1, GitHub = "@DiegoPiovezana18", DtCreation = DateTime.Now, IdCareer = 3 };
                User user2 = new("User Test One") { Id = 2, GitHub = "@UserTestOne18", DtCreation = DateTime.Now, IdCareer = 1 };
                User user3 = new("User Test Two") { Id = 3, GitHub = "@UserTestTwo18", DtCreation = DateTime.Now, IdCareer = 1 };

                List<User>? users = new() { user1, user2, user3 };
                int result1 = eh.Insert(users);
                Assert.That(result1 == 3, Is.EqualTo(true));

                // Update entities
                user1.IdCareer = 1;
                user2.Name = "User Test One Updt";
                user3.GitHub = "@UpdtUserTestTwo18";

                // Updates the entities
                int result2 = eh.Update(users);
                Assert.That(result2 == 3, Is.EqualTo(true));               
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
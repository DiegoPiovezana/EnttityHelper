using EH;
using Oracle.ManagedDataAccess.Client;
using SH;
using System.Diagnostics;
using TestEH_UnitTest.Entities;
using TestEH_UnitTest.Entitities;
using TestEnttityHelper.OthersEntity;

namespace TestEH_UnitTest
{
    public class EntityHelperTests
    {
        private readonly EnttityHelper _enttityHelper;

        public EntityHelperTests()
        {
            _enttityHelper = new EnttityHelper("Data Source=localhost:1521/xe;User Id=system;Password=oracle");
        }

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
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
            bool test = eh.DbContext.ValidateConnection();
            Assert.That(test, Is.EqualTo(true));
        }

        [Test, Order(3)]
        public void TestCreateTableIfNotExist()
        {
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");

            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 8, Name = "Entity Test", StartDate = DateTime.Now };
                if (eh.CountEntity(entityTest) > 0) eh.Delete(entityTest);
                int result1 = eh.Insert(entityTest, nameof(entityTest.Id), true);
                Assert.That(result1, Is.EqualTo(1));

                var result = eh.Get<EntityTest>();

                Assert.That(result, Is.Not.Null, "Result should not be null.");
                Assert.That(result, Has.Count.GreaterThanOrEqualTo(1), "At least one entity should be returned.");

                var lastEntity = result.Last();
                Assert.That(lastEntity.StartDate, Is.LessThanOrEqualTo(DateTime.Now.AddSeconds(10)), "StartDate should be 10 seconds earlier than now.");
            }
            else
            {
                Assert.Fail("Failed to validate database connection.");
            }
        }

        [Test, Order(9)]
        public void TestGetEntity_FailureWhenNoConnection()
        {
            // Arrange
            EnttityHelper eh = new($"Data Source=invalid;User Id=invalid;Password=invalid");

            // Act & Assert
            var exception = Assert.Throws<SystemException>(() => eh.Get<EntityTest>());

            // Verifica se a mensagem da exceção está correta
            Assert.That(exception.Message, Is.EqualTo("Invalid database type!"));
        }



        [Test, Order(9)]
        public void TestNonQuery()
        {
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
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
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                /////////////////////////////////////////////////// 
                // CREATE TABLE

                if (eh.CheckIfExist(eh.GetTableNameManyToMany(typeof(User), nameof(User.Groups)))) eh.ExecuteNonQuery($"DROP TABLE {eh.GetTableNameManyToMany(typeof(User), nameof(User.Groups))}");
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

                int resultCarrers = eh.Insert(new List<Career>() { carrer1, carrer2 });
                Assert.That(resultCarrers == 2, Is.EqualTo(true));

                Group group1 = new() { Id = 1, Name = "Developers", Description = "Developer Group" };
                //eh.Insert(group1);

                Group group2 = new() { Id = 2, Name = "Testers", Description = "Tester Group" };
                //eh.Insert(group2);

                int resultGroups = eh.Insert(new List<Group>() { group1, group2 });
                Assert.That(resultGroups == 2, Is.EqualTo(true));

                eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()}"); // DELETE FROM TB_USER

                // Insert user with group
                User user1 = new() { Id = 1, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };
                List<Group> groupsUserAdd = new() { group1, group2 };
                foreach (var group in groupsUserAdd) { user1.Groups.Add(group); }
                Assert.That(eh.Insert(user1) == 3, Is.EqualTo(true));

                // Insert group with user
                User user2 = new() { Id = 2, Name = "John Victor", GitHub = "@JohnVictor", DtCreation = DateTime.Now, IdCareer = 3 };
                Assert.That(eh.Insert(user2) == 1, Is.EqualTo(true));

                // Insert new group with user
                Group group3 = new() { Id = 3, Name = "Operation", Description = "Operation Group" };
                group3.Users.Add(user2);
                Assert.That(eh.Insert(group3) == 2, Is.EqualTo(true));


                /////////////////////////////////////////////////// 
                // GET
                List<Career>? carrers = eh.Get<Career>();
                List<Group>? groups = eh.Get<Group>();
                List<User>? users = eh.Get<User>();
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

                List<User>? usersUpdated = eh.Get<User>();
                Assert.That(usersUpdated.Count == 2, Is.EqualTo(true));

                var groupsUser1 = usersUpdated.Where(u => u.Id == 1).FirstOrDefault().Groups;
                Assert.Multiple(() =>
                {
                    Assert.That(groupsUser1.Count == 1, Is.EqualTo(true));
                    Assert.That(groupsUser1.FirstOrDefault().Name.Equals("Developers"), Is.EqualTo(true));
                });

                eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableNameManyToMany(typeof(User), nameof(User.Groups))}"); // DELETE FROM TB_GROUP_USERStoGROUPS
                eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()}");
                eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Group>()}");

                // Insert new group with new user
                User user3 = new() { Id = 3, Name = "Maria Joaquina", GitHub = "@MariaJoaquina", DtCreation = DateTime.Now, IdCareer = 1 };
                Assert.That(eh.Insert(user3) == 1, Is.EqualTo(true)); // TODO: Insert together
                Group group4 = new() { Id = 4, Name = "Analyst", Description = "Analyst Group" };
                group4.Users.Add(user3);
                Assert.That(eh.Insert(group4), Is.EqualTo(2)); // user + group + aux_tb = 3

                Assert.Pass();
            }
            else
            {
                Assert.Fail("Connection failed!");
            }
        }

        [Test, Order(101)]
        public void TestInsertDataTable()
        {
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
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

        [Test, Order(102)]
        public void TestInsertLinkSelect()
        {
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
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

        [Test, Order(103)]
        public void TestFullEntityREADME()
        {
            // Create a connection with the database using the connection string
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");

            if (eh.DbContext.ValidateConnection())
            {
                // Create table - Object User     
                eh.CreateTableIfNotExist<User>(false);

                // Create new entity
                User userD = new("Diego Piovezana") { Id = 16, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };

                // Insert in database
                eh.Insert(userD);

                // Modify entity
                userD.Name = "Diêgo Piovezana";

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

        [Test, Order(104)]
        public void TestManyInsertionsSimple()
        {
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                eh.CreateTableIfNotExist<Group>(true); // Necessary to then create the MxN relationship table
                eh.CreateTableIfNotExist<User>(false);

                eh.CreateTableIfNotExist<Career>(true);
                Career carrer1 = new() { IdCareer = 10, Name = "Manag", CareerLevel = 5, Active = true };

                // Test for one entity
                User entityTest = new("Diego Piovezana One") { Id = 21, GitHub = "@DiegoPiovezanaOne", DtCreation = DateTime.Now, IdCareer = 10 };
                bool result1 = eh.Insert(entityTest, nameof(entityTest.GitHub), true) == 1;
                if (result1) { eh.Delete(entityTest); }
                Assert.That(result1, Is.EqualTo(true));

                // Create many entities
                User user1 = new("Diego Piovezana One Repeat") { Id = 21, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 10 };
                User user2 = new("User Test One") { Id = 22, GitHub = "@UserTestOne", DtCreation = DateTime.Now, IdCareer = 10 };
                User user3 = new("User Test Two") { Id = 23, GitHub = "@UserTestTwo", DtCreation = DateTime.Now, IdCareer = 10 };

                List<User>? users = new() { user1, user2, user3 };

                // Inserts the entities
                int result2 = eh.Insert(users);

                Assert.That(result2 == 3, Is.EqualTo(true));

                // Test for one entity
                User entityError = new("John Tester") { Id = 21, GitHub = "@DiegoPiovezanaOne", DtCreation = DateTime.Now, IdCareer = 100 };
                var ex = Assert.Throws<FileNotFoundException>(() => eh.Insert(entityTest, nameof(entityTest.GitHub), true));
                Assert.That(ex.Message, Does.Contain("does not exist!"));
            }
        }

        [Test, Order(105)]
        public void TestInsertEntityWithoutEntity_AndManyDelete()
        {
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                eh.CreateTableIfNotExist<Group>(true); // Necessary to then create the MxN relationship table
                eh.CreateTableIfNotExist<User>(false);

                eh.CreateTableIfNotExist<Career>(true);
                Career carrer1 = new() { IdCareer = 1, Name = "Junior", CareerLevel = 1, Active = true };
                Career carrer2 = new() { IdCareer = 2, Name = "Pleno", CareerLevel = 2, Active = true };
                Career carrer3 = new() { IdCareer = 3, Name = "Senior", CareerLevel = 3, Active = true };
                if (eh.CheckIfExist(eh.GetTableName<Career>(), 1, $"{nameof(Career.IdCareer)} = {carrer1.IdCareer}")) eh.Delete(carrer1, nameof(Career.IdCareer));
                if (eh.CheckIfExist(eh.GetTableName<Career>(), 1, $"{nameof(Career.IdCareer)} = {carrer2.IdCareer}")) eh.Delete(carrer2);
                if (eh.CheckIfExist(eh.GetTableName<Career>(), 1, $"{nameof(Career.IdCareer)} = {carrer3.IdCareer}")) eh.Delete(carrer3);
                List<Career>? carrers = new() { carrer1, carrer2, carrer3 };
                int result10 = eh.Insert(carrers);
                Assert.AreEqual(3, result10);

                // Many Delete
                carrers = new() { carrer1, carrer1, carrer1 };  // Repeated
                int result11 = eh.Delete(carrers);
                Assert.AreEqual(1, result11);
                int result12 = eh.Insert(carrers);
                Assert.AreEqual(1, result12); // Cannot insert repeated mass (namePropUnique was not used) -- Can insert duplicates separately (if the database allows)
                carrers = new() { carrer1, carrer2, carrer3 };
                int result14 = eh.Delete(carrers);
                Assert.AreEqual(3, result14);
                int result1 = eh.Insert(carrers);
                Assert.AreEqual(3, result1);



                // Test for one entity - Without Career
                User entityTest1 = new("Diego Piovezana One") { Id = 1051, GitHub = "@DiegoPiovezanaOne", DtCreation = DateTime.Now };
                int result2 = eh.Insert(entityTest1, nameof(entityTest1.GitHub), true);
                Assert.AreEqual(1, result2);

                // Test for one entity - With Career
                User entityTest2 = new("Diego Piovezana Two") { Id = 1052, GitHub = "@DiegoPiovezanaTwo", DtCreation = DateTime.Now, IdCareer = 3 };
                int result3 = eh.Insert(entityTest2, nameof(entityTest2.GitHub), true);
                Assert.AreEqual(1, result3);

                if (result2 > 0) { eh.Delete(entityTest1); }
                if (result3 > 0) { eh.Delete(entityTest2); }

                eh.Delete(carrers);
            }
        }

        [Test, Order(106)]
        public void TestManyInsertionsMxN()
        {
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                // INSERT THE MANY ENTITIES (MXN)
                Group group4 = new() { Id = 1061, Name = "Masters2", Description = "Masters Group" };
                Group group5 = new() { Id = 1062, Name = "Managers", Description = "Managers Group" };
                List<Group> groups = new() { group4, group5 };
                int result3 = eh.Insert(groups, nameof(Group.Name), true);
                Assert.That(result3 == 2, Is.EqualTo(true));

                // It is necessary to first insert the groups, and then link them to the user
                // Otherwise, the local ID of groups 4 and 5 will be incorrectly used, instead of the one defined by the database.

                User userM = new("Maria da Silva") { Id = 1061, GitHub = "@MariaSilva", DtCreation = DateTime.Now, IdCareer = 1 };
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

        [Test, Order(107)]
        public void TestManyInsertionsNxM()
        {
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                Career carrer2 = new() { IdCareer = 2, Name = "Pleno", CareerLevel = 2, Active = true };

                if (!eh.CheckIfExist(
                    eh.GetTableName<Career>(),
                    1,
                    $"{nameof(Career.IdCareer)} = {carrer2.IdCareer}")
                    )
                {

                    int result10 = eh.Insert(carrer2);
                    Assert.AreEqual(1, result10);
                }

                User userNameRepeat = new("Xavier Souza") { Id = 1070, GitHub = "@XavierSouza", DtCreation = DateTime.Now, IdCareer = 2 };
                var ex = Assert.Throws<Exception>(() => eh.Insert(userNameRepeat, nameof(userNameRepeat.GitHub), true));
                Assert.That(ex.Message, Does.Contain("EH-101"));

                // INSERT THE MANY ENTITIES (NXM)
                User userX = new("Jayme Souza") { Id = 1071, GitHub = "@JaymeSouza", DtCreation = DateTime.Now, IdCareer = 2 };
                User userY = new("Bruna Corsa") { Id = 1072, GitHub = "@BrunaCorsa", DtCreation = DateTime.Now };
                List<User> users = new() { userX, userY };
                int result5 = eh.Insert(users, nameof(User.Name), false);
                Assert.AreEqual(result5, 2);

                Group group6 = new() { Id = 1071, Name = "Group Six", Description = "Group Six Test" };
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

        [Test, Order(201)]
        public void TestManyUpdates()
        {
            EnttityHelper eh = new($"Data Source=localhost:1521/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                Career carrer1 = new(1, "Developer");
                Career carrer2 = new(2, "Management");
                Career carrer3 = new(3, "Analyst");
                //eh.Insert(new List<Career> { carrer1, carrer2, carrer3 });


                //int deletes = eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()} WHERE ID IN (1, 2, 3)");

                // Create many entities
                User user1 = new("Diego Piovezana") { Id = 1, GitHub = "@DiegoPiovezana18", DtCreation = DateTime.Now, IdCareer = 3 };
                User user2 = new("User Test Two") { Id = 2, GitHub = "@UserTestTwo18", DtCreation = DateTime.Now, IdCareer = 1 };
                User user3 = new("User Test Three") { Id = 3, GitHub = "@UserTestThree18", DtCreation = DateTime.Now, IdCareer = 1 };

                List<User>? users = new() { user1, user2, user3 };
                int result1 = eh.Insert(users);
                Assert.That(result1 == 3, Is.EqualTo(true));

                // Update entities
                user1.IdCareer = 1;
                user2.Name = "User Test Two Updt";
                user3.GitHub = "@UpdtUserTestThree18";

                int result2 = eh.Update(users);
                Assert.That(result2 == 3, Is.EqualTo(true));

                // Update one entity
                User user4 = new("User Test Four") { Id = 4, GitHub = "@UserTestFour18", DtCreation = DateTime.Now, IdCareer = 2 };
                int result3 = eh.Insert(user4);
                Assert.That(result3 == 1, Is.EqualTo(true));
                user4.GitHub = "@UpdtUserTestFour18";
                int result4 = eh.Update(user4);
                Assert.That(result4 == 1, Is.EqualTo(true));
            }
            else
            {
                Assert.Fail("Connection failed!");
            }
        }

        [Test]
        [TestCase(null, "Name cannot be null or empty. (Parameter 'name')")]
        [TestCase("", "Name cannot be null or empty. (Parameter 'name')")]
        [TestCase("1InvalidName", "Name must start with a letter or an underscore.")]
        [TestCase("Invalid Name@2023", "Invalid character '@' detected in name.")]
        [TestCase("SELECT", "Name cannot be a reserved keyword. (Parameter 'result')")]
        public void NormalizeColumnOrTableName_Should_ThrowArgumentException_ForInvalidInputs(string name, string expectedMessage)
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _enttityHelper.NormalizeColumnOrTableName(name, false));
            Assert.AreEqual(expectedMessage, ex.Message);
        }

        [Test]
        [TestCase("Invalid Name@2023", "Invalid_Name_2023")]
        [TestCase("SELECT", "c_SELECT")]
        [TestCase("123invalidName", "c_123invalidName")]
        [TestCase("invalid@name", "invalid_name")]
        [TestCase("name$", "name_")]
        [TestCase("name with space", "name_with_space")]
        public void NormalizeColumnOrTableName_Should_AdjustName_When_InvalidCharactersOrReservedKeyword(string name, string expected)
        {
            // Act
            string result = _enttityHelper.NormalizeColumnOrTableName(name, true);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase("ThisIsAVeryLongNameThatExceedsTheLimitOfThirtyCharacters", "ThisIsAVeryLongNameThatExceeds")]
        [TestCase("Valid_Name123", "Valid_Name123")]
        [TestCase("validName_123", "validName_123")]
        [TestCase("validName", "validName")]
        public void NormalizeColumnOrTableName_Should_ReturnCorrectlyAdjustedName_When_ValidInputs(string name, string expected)
        {
            // Act
            string result = _enttityHelper.NormalizeColumnOrTableName(name, true);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void LoadCSV_ShouldInsertData_WhenValidCSVFile()
        {
            Assert.That(_enttityHelper.DbContext.ValidateConnection());

            // Arrange
            //string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\ColunasExcel.csv"; 
            //var insertCount = 101_253;

            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\CabecalhoIrregular.csv";
            var insertCount = 100;

            string tableName = "TestTable";
            int batchSize = 100000;
            int timeout = 300000;

            // Mock the Insert method to return a successful insert count
            //var mock = new Mock<EnttityHelper>();
            //mock.Setup(m => m.Insert(It.IsAny<CSVDataReader>(), tableName, true, It.IsAny<int>()))
            //    .Returns(insertCount);

            if (_enttityHelper.CheckIfExist(tableName)) _enttityHelper.ExecuteNonQuery($"DROP TABLE {tableName}");

            // Act
            int result = _enttityHelper.LoadCSV(csvFilePath, true, tableName, batchSize, timeout);

            // Assert
            Assert.AreEqual(insertCount - 1, result); // -1 because the first row is the header
        }

        [Test]
        public void LoadCSV_ShouldThrowException_WhenCSVFileIsInvalid()
        {
            // Arrange
            string csvFilePath = "invalid_path.csv";
            string tableName = "TestTable";

            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() => _enttityHelper.LoadCSV(csvFilePath, true, tableName));
            Assert.That(ex.Message, Does.Contain("File not found"));
        }

        [Test]
        public void LoadCSV_BIGCSVFile()
        {
            Assert.Pass(); // Skip



            Assert.That(_enttityHelper.DbContext.ValidateConnection());

            DateTime startTime = DateTime.Now;
            Debug.WriteLine($"Start: {startTime}");

            // Arrange   
            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\BigCsvGerado_5000000x20.csv";
            var insertCount = 5_000_000;

            bool createTable = true;
            string tableName = "TestTable_BigCsv";
            int batchSize = 100_000;
            int timeOutSeconds = 50; // Timeout in seconds to insert 1 batch (max)

            if (_enttityHelper.CheckIfExist(tableName)) _enttityHelper.ExecuteNonQuery($"DROP TABLE {tableName}");

            // Act
            int result = _enttityHelper.LoadCSV(csvFilePath, createTable, tableName, batchSize, timeOutSeconds);

            DateTime endTime = DateTime.Now;
            Debug.WriteLine($"End: {endTime}");

            Debug.WriteLine($"Elapsed: {endTime - startTime}");

            // Assert
            Assert.AreEqual(insertCount - 1, result); // -1 because the first row is the header
        }

        [Test]
        public void LoadTXT_ShouldInsertData()
        {
            Assert.That(_enttityHelper.DbContext.ValidateConnection());

            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\CabecalhoIrregular.txt";
            var insertCount = 100;

            string tableName = "TestTable_Txt";
            int batchSize = 100000;
            int timeout = 30;
            char delimiter = ';';

            if (_enttityHelper.CheckIfExist(tableName)) _enttityHelper.ExecuteNonQuery($"DROP TABLE {tableName}");

            // Act
            int result = _enttityHelper.LoadCSV(csvFilePath, true, tableName, batchSize, timeout, delimiter);

            // Assert
            Assert.AreEqual(insertCount - 1, result); // -1 because the first row is the header
        }

        [Test]
        public void LoadCSV_WithoutHeader1()
        {
            Assert.That(_enttityHelper.DbContext.ValidateConnection());

            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\CabecalhoVazio.csv";
            var insertCount = 100; // J100

            string tableName = "TestTable";
            int batchSize = 100;
            int timeout = 30;
            char delimiter = ';';
            bool hasHeader = true; // The header exists, but is empty

            if (_enttityHelper.CheckIfExist(tableName)) _enttityHelper.ExecuteNonQuery($"DROP TABLE {tableName}");

            // Act
            int result = _enttityHelper.LoadCSV(csvFilePath, true, tableName, batchSize, timeout, delimiter, hasHeader);

            // Assert
            Assert.AreEqual(insertCount - 1, result); // -1 because the first row is the blank header
        }

        [Test]
        public void LoadCSV_WithoutHeader2()
        {
            Assert.That(_enttityHelper.DbContext.ValidateConnection());

            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\CabecalhoInexistente.csv";
            var insertCount = 101_254; // AB101254

            string tableName = "TestTable";
            int batchSize = 100_000;
            int timeout = 60;
            char delimiter = ';';
            bool hasHeader = false; // The CSV file doesn't

            if (_enttityHelper.CheckIfExist(tableName)) _enttityHelper.ExecuteNonQuery($"DROP TABLE {tableName}");

            // Act
            int result = _enttityHelper.LoadCSV(csvFilePath, true, tableName, batchSize, timeout, delimiter, hasHeader);

            // Assert
            Assert.AreEqual(insertCount - 0, result); // -0 because the first row isn't the header
        }



    }
}
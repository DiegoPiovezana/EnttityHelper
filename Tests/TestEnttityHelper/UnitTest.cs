using EH;
using Oracle.ManagedDataAccess.Client;
using SH;
using System.Data;
using System.Diagnostics;
using System.Text;
using TestEH_UnitTest.Entities;
using TestEnttityHelper.OthersEntity;

namespace TestEH_UnitTest
{
    public class EntityHelperTests
    {
        //private readonly EnttityHelper _enttityHelper;

        private const string stringConnection11g = "Data Source=localhost:1521/xe;User Id=system;Password=oracle";
        private const string stringConnection19c = "Data Source=localhost:49262/orclcdb;User Id=system;Password=oracle";

        private readonly string stringConnectionBd2 = stringConnection19c;
        private readonly string stringConnectionBd1 = stringConnection11g;

        public EntityHelperTests()
        {
            //_enttityHelper = new EnttityHelper("Data Source=localhost:1521/xe;User Id=system;Password=oracle");
        }

        [SetUp]
        public void Setup()
        {

        }

        private void ResetTables(string idTest)
        {
            EnttityHelper eh = new(stringConnectionBd1);
            eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableNameManyToMany(typeof(User), nameof(User.Groups))} WHERE TO_CHAR(ID_TB_GROUP_USERS) LIKE '{idTest}__' OR TO_CHAR(ID_TB_USERS) LIKE '{idTest}__'");  // DELETE FROM TB_GROUP_USERStoGROUPS
            eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()} WHERE TO_CHAR({nameof(User.Id)}) LIKE '{idTest}__'");
            eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Group>()} WHERE TO_CHAR({nameof(Group.Id)}) LIKE '{idTest}__'");
            eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Career>()} WHERE TO_CHAR({nameof(Career.IdCareer)}) LIKE '{idTest}__'");
            if (eh.CheckIfExist(eh.GetTableName<Ticket>())) eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Ticket>()} WHERE TO_CHAR({nameof(Ticket.IdLog)}) LIKE '{idTest}__'");
        }

        [Test, Order(1)]
        public void TestPass()
        {
            Assert.Pass();
        }

        [Test, Order(2)]
        public void TestConnection()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            bool test = eh.DbContext.ValidateConnection();
            Assert.That(test, Is.EqualTo(true));
        }

        [Test, Order(3)]
        public void TestCreateTableIfNotExist()
        {
            EnttityHelper eh = new(stringConnectionBd1);
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
        public void TestCreateTableAlreadExist()
        {
            EnttityHelper eh = new(stringConnectionBd1);
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
        
        [Test, Order(4)]
        public void TestDropTable()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
               var result = eh.ExecuteNonQuery($"DROP TABLE {eh.GetTableName<EntityTest>()}");
               Assert.That(result, Is.EqualTo(-1)); // If the table does not exist, it returns 0
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(5)]
        public void TestInsertEntity()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
                //bool result = eh.Insert(new { Id = 1, Name = "Test" }, null);

                EntityTest entityTest = new() { Id = 90, Name = "Testando entidade 90 via C#", StartDate = DateTime.Now };
                //User entityTest = new("Diego Piovezana") { Id = 1, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };

                //bool result = eh.Insert(entityTest);
                long result = eh.Insert(entityTest, true, nameof(entityTest.Id), true);

                //if (result) { eh.Delete(entityTest); }

                Assert.AreEqual(result, 1);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(6)]
        public void TestUpdateEntity()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 90, Name = "Testing entity 90 updating start time via C#", StartDate = DateTime.Now };
                var result = eh.Update(entityTest, nameof(entityTest.Id));

                Assert.That(result, Is.EqualTo(1));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(7)]
        public void TestSearchEntity()
        {
            EnttityHelper eh = new(stringConnectionBd1);
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
            EnttityHelper eh = new(stringConnectionBd1);

            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 8, Name = "Entity Test", StartDate = DateTime.Now };
                if (eh.CountEntity(entityTest) > 0) eh.Delete(entityTest);
                long result1 = eh.Insert(entityTest, true, nameof(entityTest.Id), true);
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
            var exception = Assert.Throws<Exception>(() => new EnttityHelper($"Data Source=invalid;User Id=invalid;Password=invalid"));
            Assert.That(exception.Message, Is.EqualTo("Invalid connection string!"));
        }

        [Test, Order(10)]
        public void TestNonQuery()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
                var result = eh.ExecuteNonQuery("DELETE FROM TB_ENTITY_TEST WHERE ID = 90");
                Assert.That(result, Is.EqualTo(1));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(10)]
        public void TestFullEntity()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 300, Name = "Testando 1 entidade 100 via C#", StartDate = DateTime.Now };
                eh.Insert(entityTest, true, nameof(entityTest.Id));

                EntityTest entityTest2 = new() { Id = 300, Name = "Testando 2 entidade 300 atualizando hora via C#", StartDate = DateTime.Now };
                eh.Update(entityTest2, nameof(entityTest.Id));

                var entities = eh.Get<EntityTest>(true, $"{nameof(EntityTest.Id)} = 300");

                if (entities is not null && entities[0].Name.Equals("Testando 2 entidade 300 atualizando hora via C#"))
                {
                    long result = eh.ExecuteNonQuery("DELETE FROM TB_ENTITY_TEST WHERE ID = 300");
                    Assert.That(result, Is.EqualTo(1));
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
            EnttityHelper eh = new(stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 300, Name = "Testando 1 entidade 100 via C#", StartDate = DateTime.Now };
                eh.Insert(entityTest, true, nameof(entityTest.Id));

                EntityTest entityTest2 = new() { Id = 300, Name = "Testando 2 entidade 300 atualizando hora via C#", StartDate = DateTime.Now };
                eh.Update(entityTest2, nameof(entityTest.Id));

                var entities = eh.Get<EntityTest>(true, $"{nameof(EntityTest.Id)} = 300");

                if (entities is not null && entities[0].Name.Equals("Testando 2 entidade 300 atualizando hora via C#"))
                {
                    long result = eh.ExecuteNonQuery("DELETE FROM TB_ENTITY_TEST WHERE ID = 300");
                    Assert.That(result, Is.EqualTo(1));
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
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection(), Is.True);

            // Ensure tables exist or create if they do not
            eh.CreateTableIfNotExist<Career>(createOnlyPrimaryTable: false);
            eh.CreateTableIfNotExist<User>(createOnlyPrimaryTable: true);

            var deletesOld = eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()} WHERE TO_CHAR(ID) LIKE '12%'");
            Assert.IsTrue(deletesOld >= 0 && deletesOld <= 2);

            deletesOld = eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Career>()} WHERE TO_CHAR(IDCAREER) LIKE '12%'");
            Assert.IsTrue(deletesOld >= 0 && deletesOld <= 2);

            // Insert a career entity
            Career career = new Career(121, "Developer");
            long careerInsertResult = eh.Insert(career, true);
            Assert.AreEqual(1, careerInsertResult, "Career insertion failed.");

            // Insert a user entity linked to the created career
            User user = new User("Diego Piovezana")
            {
                Id = 121,
                GitHub = "@DiegoPiovezana",
                DtCreation = DateTime.Now,
                IdCareer = 121
            };
            
            long userInsertResult = eh.Insert(user, true);
            Assert.AreEqual(1, userInsertResult, "User insertion failed.");

            // Retrieve and validate career entities
            var careers = eh.Get<Career>();
            Assert.IsNotNull(careers, "Failed to retrieve careers.");
            Assert.IsTrue(careers.Any(), "No careers found.");

            // Retrieve and validate user entities
            var users = eh.Get<User>();
            Assert.IsNotNull(users, "Failed to retrieve users.");
            Assert.IsTrue(users.Any(), "No users found.");
        }

        [Test, Order(13)]
        public void TestManyToMany()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection(), Is.EqualTo(true));

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

            Career carrer1 = new() { IdCareer = 1301, Name = "Pleno", CareerLevel = 2, Active = true };
            //eh.Insert(carrer);

            Career carrer2 = new() { IdCareer = 1302, Name = "Trainee", CareerLevel = 0, Active = true };
            //eh.Insert(carrer2);

            long resultCarrers = eh.Insert(new List<Career>() { carrer1, carrer2 });
            Assert.That(resultCarrers, Is.EqualTo(2));

            Group group1 = new() { Id = 1301, Name = "Developers", Description = "Developer Group" };
            //eh.Insert(group1);

            Group group2 = new() { Id = 1302, Name = "Testers", Description = "Tester Group" };
            //eh.Insert(group2);

            long resultGroups = eh.Insert(new List<Group>() { group1, group2 });
            Assert.That(resultGroups == 2, Is.EqualTo(true));

            eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()} WHERE TO_CHAR({nameof(User.Id)}) LIKE '13__'"); // DELETE FROM TB_USER

            // Insert user with group
            User user1 = new() { Id = 1301, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1301 };
            List<Group> groupsUserAdd = new() { group1, group2 };
            foreach (var group in groupsUserAdd) { user1.Groups.Add(group); }
            Assert.That(eh.Insert(user1), Is.EqualTo(3));

            // Insert group with user
            User user2 = new() { Id = 1302, Name = "John Victor", GitHub = "@JohnVictor", DtCreation = DateTime.Now, IdCareer = 1302 };
            Assert.That(eh.Insert(user2), Is.EqualTo(1));

            // Insert new group with user
            Group group3 = new() { Id = 1303, Name = "Operation", Description = "Operation Group" };
            group3.Users.Add(user2);
            Assert.That(eh.Insert(group3), Is.EqualTo(2));


            /////////////////////////////////////////////////// 
            // GET
            List<Career>? carrers = eh.Get<Career>();
            List<Group>? groups = eh.Get<Group>();
            List<User>? users = eh.Get<User>();
            Assert.Multiple(() =>
            {
                Assert.That(carrers.Count, Is.EqualTo(2));
                Assert.That(groups.Count, Is.EqualTo(3));
                Assert.That(users.Count, Is.EqualTo(2));
            });

            /////////////////////////////////////////////////// 
            // UPDATE
            //User userUpdate = new() { Id = 1, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 1 };
            user1.Groups = new List<Group>() { group1 }; // Remove group2
            var result = eh.Update(user1);
            Assert.That(result, Is.EqualTo(2)); // Update user1 (there will always be an update in the entity) + Remove group2 from aux table

            List<User>? usersUpdated = eh.Get<User>();
            Assert.That(usersUpdated.Count == 2, Is.EqualTo(true));

            var groupsUser1 = usersUpdated.Where(u => u.Id == 1301).FirstOrDefault().Groups;
            Assert.Multiple(() =>
            {
                Assert.That(groupsUser1.Count, Is.EqualTo(1));
                Assert.That(groupsUser1.FirstOrDefault().Name.Equals("Developers"), Is.EqualTo(true));
            });

            // Insert new group with new user
            User user3 = new() { Id = 1303, Name = "Maria Joaquina", GitHub = "@MariaJoaquina", DtCreation = DateTime.Now, IdCareer = 1301 };
            Assert.That(eh.Insert(user3), Is.EqualTo(1)); // TODO: Insert together
            Group group4 = new() { Id = 1304, Name = "Analyst", Description = "Analyst Group" };
            group4.Users.Add(user3);
            Assert.That(eh.Insert(group4), Is.EqualTo(2)); // user + group + aux_tb = 3

            ResetTables("13");
        }

        [Test, Order(101)]
        public void TestInsertDataTable()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
                // Empty columns will automatically have the Object type. In the database, the Object type will be NVARCHAR2(100)
                eh.TypesDefault.Add("Object", "NVARCHAR2(100)");

                // Reads the first tab of the DataTable
                var dt = SheetHelper.GetDataTable(@"C:\Users\diego\Desktop\Tests\Converter\ColunasExcel.xlsx", "1");

                // If the table exists, it will be deleted
                if (eh.CheckIfExist("TableX")) eh.ExecuteNonQuery($"DROP TABLE TableX");

                // Possible to insert the DataTable considering different scenarios
                var result = eh.Insert(dt, true, null, true, "TableX");
                //eh.Insert(dt, null, true); // The table name will automatically be the name of the spreadsheet tab (removing special characters)
                //eh.Insert(dt, null, false); // The table will not be created and only the DataTable will be inserted              

                Assert.That(result, Is.EqualTo(dt.Rows.Count));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(102)]
        public void InsertLinkSelect_ByCSV()
        {

            // Create a connection with the database 1
            EnttityHelper eh1 = new(stringConnectionBd1);
            Assert.IsTrue(eh1.DbContext.ValidateConnection());

            // Create a new connection to database 2
            EnttityHelper eh2 = new(stringConnectionBd2);
            if (!eh2.DbContext.ValidateConnection())
            {
                Assert.Ignore("Database 2 connection validation failed. Test skipped.");
            }

            DateTime startTime = DateTime.Now;
            Debug.WriteLine($"Start Load CSV: {startTime}");

            // CSV file
            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\ExcelCsvGerado_1000000x10.csv";

            bool createTable = true;
            string tableName = "TestTable50K_Csv"; //TestTable1M_Csv
            int batchSize = 10_000; // 200_000
            int timeOutSeconds = 50; // Timeout in seconds to insert 1 batch (max)
            char delimiter = ';';
            bool hasHeader = true; // The CSV file has a header

            string rowsToLoad = "1:50000";
            var insertCount = 50_000; // 1_000_000
            if (hasHeader) insertCount--;

            string tableNameDestiny = "TEST_LINKSELECT_CSV";

            try
            {
                long result1 = 0;
                if (!eh1.CheckIfExist(tableName))
                {
                    // Act
                    result1 = eh1.LoadCSV(csvFilePath, createTable, tableName, batchSize, timeOutSeconds, delimiter, hasHeader, rowsToLoad);
                    Assert.That(result1, Is.EqualTo(insertCount));
                }

                DateTime endTime = DateTime.Now;
                Debug.WriteLine($"End Load CSV: {endTime}");
                Debug.WriteLine($"Elapsed Load CSV: {endTime - startTime}");

                startTime = DateTime.Now;
                Debug.WriteLine($"Start Link Select: {startTime}");

                // Select from database table from database 1
                string query = $"SELECT * FROM {tableName}";

                // Insert the result of the select into the database table of database 2
                var result2 = eh1.InsertLinkSelect(query, eh2, tableNameDestiny);
                Assert.That(result2, Is.EqualTo(result1));

                endTime = DateTime.Now;
                Debug.WriteLine($"End Link Select: {endTime}");
                Debug.WriteLine($"Elapsed Link Select: {endTime - startTime}");

                var countDb1 = eh1.ExecuteScalar($"SELECT COUNT(*) FROM {tableName}");
                var countDb2 = eh2.ExecuteScalar($"SELECT COUNT(*) FROM {tableNameDestiny}");
                Assert.That(countDb1, Is.EqualTo(insertCount));
            }
            finally
            {
                eh1.ExecuteNonQuery($"DROP TABLE {tableName}");
                eh2.ExecuteNonQuery($"DROP TABLE {tableNameDestiny}");
            }
        }

        [Test, Order(103)]
        public void TestFullEntityREADME()
        {
            // Create a connection with the database using the connection string
            EnttityHelper eh = new(stringConnectionBd1);

            if (eh.DbContext.ValidateConnection())
            {
                // Create table - Object User     
                eh.CreateTableIfNotExist<Career>(true);
                eh.CreateTableIfNotExist<Group>(true); // Necessary to then create the MxN relationship table (Group X User)
                eh.CreateTableIfNotExist<User>(false);

                // Create new entity
                Career career = new(10301, "Developer");
                User userD = new("Diego Piovezana") { Id = 10321, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 10301 };

                // Insert in database
                eh.Insert(career);
                eh.Insert(userD);

                // Modify entity
                userD.Name = "Di�go Piovezana";

                // Update in database
                eh.Update(userD);

                // Search in database
                User? userDSearched1 = eh.Search(userD);
                User? userDSearched2 = eh.Search(new User { GitHub = "@DiegoPiovezana" }, true, nameof(User.GitHub));
                Assert.That(userDSearched1.Name, Is.EqualTo(userDSearched2.Name));

                // Deletes user D from the database
                eh.Delete(userD);

                // Gets all users registered in the last week
                List<User>? usersWeek = eh.Get<User>()?.Where(u => u.DtCreation > DateTime.Now.AddDays(-7)).ToList();

                eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableNameManyToMany(typeof(User), nameof(User.Groups))} WHERE TO_CHAR(ID_TB_GROUP_USERS) LIKE '103%' OR TO_CHAR(ID_TB_USERS) LIKE '103%'");  // DELETE FROM TB_GROUP_USERStoGROUPS
                eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()} WHERE TO_CHAR({nameof(User.Id)}) LIKE '103%'");
                eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Group>()} WHERE TO_CHAR({nameof(Group.Id)}) LIKE '103%'");
                eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Career>()} WHERE TO_CHAR({nameof(Career.IdCareer)}) LIKE '103%'");
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
            EnttityHelper eh = new(stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
                eh.CreateTableIfNotExist<Group>(true); // Necessary to then create the MxN relationship table
                eh.CreateTableIfNotExist<User>(false);

                eh.CreateTableIfNotExist<Career>(true);
                Career carrer1 = new() { IdCareer = 10, Name = "Manag", CareerLevel = 5, Active = true };
                long countCarrer1 = eh.CountEntity(carrer1);
                Assert.IsTrue(countCarrer1 == 0 || countCarrer1 == 1);
                if (countCarrer1 == 0)
                {
                    var result = eh.Insert(carrer1);
                    Assert.AreEqual(result, 1);
                }

                var deletesOld = eh.ExecuteNonQuery("DELETE FROM TB_USERS WHERE TO_CHAR(ID) LIKE '104%'");
                Assert.IsTrue(deletesOld >= 0 && deletesOld <= 4);

                // Test for one entity
                User entityTest = new("Diego Piovezana One") { Id = 1041, GitHub = "@DiegoPiovezanaOne", DtCreation = DateTime.Now, IdCareer = 10 };
                bool result1 = eh.Insert(entityTest, true, nameof(entityTest.GitHub), true) == 1;
                if (result1) { Assert.AreEqual(eh.Delete(entityTest), 1); }
                Assert.That(result1, Is.EqualTo(true));

                // Create many entities
                User user1 = new("Diego Piovezana One Repeat") { Id = 1042, GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 10 };
                User user2 = new("User Test One") { Id = 1043, GitHub = "@UserTestOne", DtCreation = DateTime.Now, IdCareer = 10 };
                User user3 = new("User Test Two") { Id = 1044, GitHub = "@UserTestTwo", DtCreation = DateTime.Now, IdCareer = 10 };
                List<User>? users = new() { user1, user2, user3 };

                // Inserts the entities                
                long result2 = eh.Insert(users);
                Assert.AreEqual(result2, 3);

                // Test for one entity
                User entityError = new("John Tester") { Id = 1045, GitHub = "@DiegoPiovezanaOne", DtCreation = DateTime.Now, IdCareer = 100 };
                var ex = Assert.Throws<InvalidOperationException>(() => eh.Insert(entityError, true, nameof(entityTest.GitHub), true));
                Assert.That(ex.Message, Does.Contain("Career with IdCareer '100' or table 'TB_CAREERS' does not exist!"));

                Assert.AreEqual(eh.Delete(carrer1), 1);
                Assert.AreEqual(eh.Delete(users), 3);
            }
        }

        [Test, Order(105)]
        public void TestInsertEntityWithoutEntity_AndManyDelete()
        {
            EnttityHelper eh = new(stringConnectionBd1);
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
                long result10 = eh.Insert(carrers);
                Assert.AreEqual(3, result10);

                // Many Delete
                carrers = new() { carrer1, carrer1, carrer1 };  // Repeated
                long result11 = eh.Delete(carrers);
                Assert.AreEqual(1, result11);
                long result12 = eh.Insert(carrers);
                Assert.AreEqual(1, result12); // Cannot insert repeated mass (namePropUnique was not used) -- Can insert duplicates separately (if the database allows)
                carrers = new() { carrer1, carrer2, carrer3 };
                long result14 = eh.Delete(carrers);
                Assert.AreEqual(3, result14);
                long result1 = eh.Insert(carrers);
                Assert.AreEqual(3, result1);



                // Test for one entity - Without Career
                User entityTest1 = new("Diego Piovezana One") { Id = 1051, GitHub = "@DiegoPiovezanaOne", DtCreation = DateTime.Now };
                long result2 = eh.Insert(entityTest1, true, nameof(entityTest1.GitHub), true);
                Assert.AreEqual(1, result2);

                // Test for one entity - With Career
                User entityTest2 = new("Diego Piovezana Two") { Id = 1052, GitHub = "@DiegoPiovezanaTwo", DtCreation = DateTime.Now, IdCareer = 3 };
                long result3 = eh.Insert(entityTest2, true, nameof(entityTest2.GitHub), true);
                Assert.AreEqual(1, result3);

                if (result2 > 0) { eh.Delete(entityTest1); }
                if (result3 > 0) { eh.Delete(entityTest2); }

                eh.Delete(carrers);
            }
        }

        [Test, Order(106)]
        public void TestManyInsertionsMxN()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
                // INSERT THE MANY ENTITIES (MXN)
                Group group4 = new() { Id = 1061, Name = "Masters106-2", Description = "Masters Group" };
                Group group5 = new() { Id = 1062, Name = "Managers106", Description = "Managers Group" };
                List<Group> groups = new() { group4, group5 };
                long result3 = eh.Insert(groups, true, nameof(Group.Name), true);
                Assert.That(result3 == 2, Is.EqualTo(true));

                eh.CreateTableIfNotExist<Career>(true);
                Career carrer1 = new() { IdCareer = 1061, Name = "Manag", CareerLevel = 5, Active = true };
                long countCarrer1 = eh.CountEntity(carrer1);
                Assert.IsTrue(countCarrer1 == 0 || countCarrer1 == 1);
                if (countCarrer1 == 0)
                {
                    var result = eh.Insert(carrer1);
                    Assert.AreEqual(result, 1);
                }

                // It is necessary to first insert the groups, and then link them to the user
                // Otherwise, the local ID of groups 4 and 5 will be incorrectly used, instead of the one defined by the database.

                User userM = new("Maria da Silva") { Id = 1061, GitHub = "@MariaSilva", DtCreation = DateTime.Now, IdCareer = 1061 };
                userM.Groups.Add(group4);
                userM.Groups.Add(group5);
                long result4 = eh.Insert(userM, true, nameof(userM.GitHub), true);
                Assert.That(result4 == 3, Is.EqualTo(true));

                eh.ExecuteNonQuery($"DELETE FROM TB_GROUP_USERSTOGROUPS WHERE ID_TB_USERS = {userM.Id}");
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
            EnttityHelper eh = new(stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
                Career carrer2 = new() { IdCareer = 1072, Name = "Pleno", CareerLevel = 2, Active = true };

                if (!eh.CheckIfExist(
                    eh.GetTableName<Career>(),
                    1,
                    $"{nameof(Career.IdCareer)} = {carrer2.IdCareer}")
                    )
                {

                    long result10 = eh.Insert(carrer2);
                    Assert.AreEqual(1, result10);
                }

                // INSERT THE MANY ENTITIES (NXM)
                User userX = new("Jayme Souza") { Id = 1071, GitHub = "@JSouza", DtCreation = DateTime.Now, IdCareer = 1072 };
                User userY = new("Bruna Corsa") { Id = 1072, GitHub = "@BrunaCorsa", DtCreation = DateTime.Now };
                List<User> users = new() { userX, userY };
                long result5 = eh.Insert(users, true, nameof(User.Name), false);
                Assert.AreEqual(result5, 2);

                User userNameRepeat = new("Jhonny Souza") { Id = 1070, GitHub = "@JSouza", DtCreation = DateTime.Now, IdCareer = 1072 };
                var ex = Assert.Throws<Exception>(() => eh.Insert(userNameRepeat, true, nameof(userNameRepeat.GitHub), true));
                Assert.That(ex.Message, Does.Contain("EH-101"));

                Group group6 = new() { Id = 1071, Name = "Group Six", Description = "Group Six Test" };
                group6.Users.Add(userX);
                group6.Users.Add(userY);
                long result6 = eh.Insert(group6, true, nameof(group6.Name), true);
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

        [Test, Order(108)]
        public void TestInsert_EntityPropNull()
        {
            //EnttityHelper eh = new(stringConnection11g);
            EnttityHelper eh = new(connectionString: stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
                eh.CreateTableIfNotExist<Group>(createOnlyPrimaryTable: true);
                // ATTENTION: The User table depends on the Group to establish the MxN relationship and create the auxiliary table (even if users without group)

                User userX = new("Jayme Souza") { Id = 1081, GitHub = "@JSouza108", DtCreation = DateTime.Now };
                User userY = new("Bruna Corsa") { Id = 1082, GitHub = "@BrunaCorsa108", DtCreation = DateTime.Now };
                List<User> users = new() { userX, userY };
                long result1 = eh.Insert(entity: users, namePropUnique: nameof(User.GitHub), createTable: true);
                Assert.That(actual: result1, expression: Is.EqualTo(expected: 2));

                eh.CreateTableIfNotExist<Ticket>(createOnlyPrimaryTable: false);

                try
                {
                    eh.ExecuteNonQuery(
                    $"CREATE SEQUENCE SEQUENCE_TICKET " +
                    $"START WITH 1 " +
                    $"INCREMENT BY 1 "
                    );
                }
                catch (Exception) { } // Ignore if the sequence already exists                    

                var resultTrigger = eh.ExecuteNonQuery(
                    $"CREATE OR REPLACE TRIGGER TRIGGER_TICKET " +
                    $"BEFORE INSERT ON TB_TICKET " +
                    $"FOR EACH ROW " +
                    $"BEGIN " +
                    $":NEW.IdLog := SEQUENCE_TICKET.NEXTVAL; " +
                    $"END;"
                    );

                // Ticket with user
                Ticket ticketUserX = new(userX, "Obs", "Num", "Previous", "After");
                Assert.That(eh.Insert(entity: ticketUserX, namePropUnique: null, createTable: false), Is.EqualTo(expected: 1));

                // Ticket without user
                Ticket ticketEmpty = new();
                ticketEmpty.DateCreate = DateTime.Now;
                ticketEmpty.IdUser = null;
                ticketEmpty.User = userY; // Will be ignored because IdUser is null
                Assert.That(eh.Insert(ticketEmpty, true, null, true), Is.EqualTo(1));

                eh.ExecuteNonQuery($"DROP TABLE {eh.GetTableName<Ticket>()}");
                eh.Delete(userX);
                eh.Delete(userY);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test, Order(109)]
        public void TestInsert_EntityPropMissing()
        {
            EnttityHelper eh = new(stringConnectionBd1);

            if (eh.DbContext.ValidateConnection())
            {
                eh.CreateTableIfNotExist<Group>(true);

                User userX = new("Jayme Souza") { Id = 1091, GitHub = "@JSouza109", DtCreation = DateTime.Now };
                User userY = new("Bruna Corsa") { Id = 1092, GitHub = "@BrunaCorsa109", DtCreation = DateTime.Now };
                List<User> users = new() { userX, userY };

                long result1 = eh.Insert(entity: users, namePropUnique: nameof(User.GitHub), createTable: true);
                Assert.That(result1, Is.EqualTo(2));

                eh.CreateTableIfNotExist<Ticket>(createOnlyPrimaryTable: false);

                try
                {
                    eh.ExecuteNonQuery(
                        $"CREATE SEQUENCE SEQUENCE_TICKET " +
                        $"START WITH 1 " +
                        $"INCREMENT BY 1 "
                    );
                }
                catch (Exception) { } // Ignore if the sequence already exists                    

                var resultTrigger = eh.ExecuteNonQuery(
                    $"CREATE OR REPLACE TRIGGER TRIGGER_TICKET " +
                    $"BEFORE INSERT ON TB_TICKET " +
                    $"FOR EACH ROW " +
                    $"BEGIN " +
                    $":NEW.IdLog := SEQUENCE_TICKET.NEXTVAL; " +
                    $"END;");

                // Ticket with user
                Ticket ticketUserX = new(userX, "Obs", "Num", "Previous", "After");
                Assert.That(eh.Insert(ticketUserX, namePropUnique: null, createTable: false), Is.EqualTo(1));

                // Ticket without user (IdUser is null, user will be ignored)
                Ticket ticketEmpty = new();
                ticketEmpty.DateCreate = DateTime.Now;
                ticketEmpty.IdUser = null;  // Check if nullable handling works here
                ticketEmpty.User = userY; // Will be ignored because IdUser is null
                Assert.That(eh.Insert(ticketEmpty, true, null, true), Is.EqualTo(1));

                // Test: Insert with empty string for required fields
                Ticket ticketWithEmptyStrings = new(userX, "", "", "", "");
                Assert.That(eh.Insert(ticketWithEmptyStrings, namePropUnique: null, createTable: false), Is.EqualTo(1));
                //Assert.That(() => eh.Insert(ticketWithEmptyStrings, namePropUnique: null, createTable: false), Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Required"));

                // Test: Insert with null in non-nullable string fields (should throw exception)
                Ticket ticketWithNullStrings = new(userX, null, null, null, null);
                Assert.That(eh.Insert(ticketWithNullStrings, namePropUnique: null, createTable: false), Is.EqualTo(1));
                //Assert.That(() => eh.Insert(ticketWithNullStrings, namePropUnique: null, createTable: false), Throws.TypeOf<ArgumentNullException>().With.Message.Contains("cannot be null"));

                // Test: Insert with invalid IdUser type (nullable long)
                Ticket ticketWithInvalidIdUser = new(userX, "Valid Obs", "Valid Num", "Valid Previous", "Valid After");
                ticketWithInvalidIdUser.IdUser = 999999; // Assuming a user with this Id does not exist               
                Assert.That(() => eh.Insert(ticketWithInvalidIdUser, namePropUnique: null, createTable: false), Throws.TypeOf<InvalidOperationException>().With.Message.Contains("not exist"));

                // Test: Insert with DateTime as null (should handle null DateTime gracefully if not required)
                Ticket ticketWithNullDate = new(userX, "Obs", "Num", "Previous", "After");
                ticketWithNullDate.DateCreate = default(DateTime);  // Check if empty DateTime works or throws
                Assert.That(eh.Insert(ticketWithNullDate, namePropUnique: null, createTable: false), Is.EqualTo(1));

                // Test: Insert with special characters in fields
                Ticket ticketWithSpecialChars = new(userX, "Obs @#$%", "Num123", "Previous Special", "After Special");
                Assert.That(eh.Insert(ticketWithSpecialChars, namePropUnique: null, createTable: false), Is.EqualTo(1));

                // Test: Checking insertion with IdUser as null for a new Ticket without relationship
                Ticket ticketWithNullUser = new(null, "Observation", "Num", "Previous", "After");
                ticketWithNullUser.IdUser = null;
                Assert.That(eh.Insert(ticketWithNullUser, namePropUnique: null, createTable: false), Is.EqualTo(1));

                eh.ExecuteNonQuery($"DROP TABLE {eh.GetTableName<Ticket>()}");
                eh.Delete(userX);
                eh.Delete(userY);
            }
            else
            {
                Assert.Fail();
            }
        }


        [Test, Order(455)]
        public void TestManyUpdates()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            if (eh.DbContext.ValidateConnection())
            {
                Career carrer1 = new(45501, "Developer");
                Career carrer2 = new(45502, "Management");
                Career carrer3 = new(45503, "Analyst");
                long resultCarrer = eh.Insert(new List<Career> { carrer1, carrer2, carrer3 });
                Assert.That(resultCarrer, Is.EqualTo(3));

                //int deletes = eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()} WHERE ID IN (1, 2, 3)");

                // Create many entities
                User user1 = new("Diego Piovezana") { Id = 45501, GitHub = "@DiegoPiovezana18", DtCreation = DateTime.Now, IdCareer = 45503 };
                User user2 = new("User Test Two") { Id = 45502, GitHub = "@UserTestTwo18", DtCreation = DateTime.Now, IdCareer = 45501 };
                User user3 = new("User Test Three") { Id = 45503, GitHub = "@UserTestThree18", DtCreation = DateTime.Now, IdCareer = 45501 };

                List<User>? users = new() { user1, user2, user3 };
                long result1 = eh.Insert(users);
                Assert.That(result1 == 3, Is.EqualTo(true));

                // Update entities
                user1.IdCareer = 45501;
                user2.Name = "User Test Two Updt";
                user3.GitHub = "@UpdtUserTestThree18";

                long result2 = eh.Update(users);
                Assert.That(result2 == 3, Is.EqualTo(true));

                // Update one entity
                User user4 = new("User Test Four") { Id = 45504, GitHub = "@UserTestFour18", DtCreation = DateTime.Now, IdCareer = 45502 };
                long result3 = eh.Insert(user4);
                Assert.That(result3 == 1, Is.EqualTo(true));
                user4.GitHub = "@UpdtUserTestFour18";
                long result4 = eh.Update(user4);
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
            EnttityHelper eh = new(stringConnectionBd1);
            var ex = Assert.Throws<ArgumentException>(() => eh.NormalizeColumnOrTableName(name, false));
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
            EnttityHelper eh = new(stringConnectionBd1);
            string result = eh.NormalizeColumnOrTableName(name, true);

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
            EnttityHelper eh = new(stringConnectionBd1);
            string result = eh.NormalizeColumnOrTableName(name, true);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void LoadCSV_ShouldInsertData_WhenValidCSVFile()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection());

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

            if (eh.CheckIfExist(tableName)) eh.ExecuteNonQuery($"DROP TABLE {tableName}");

            // Act
            long result = eh.LoadCSV(csvFilePath, true, tableName, batchSize, timeout);

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
            EnttityHelper eh = new(stringConnectionBd1);
            var ex = Assert.Throws<FileNotFoundException>(() => eh.LoadCSV(csvFilePath, true, tableName));
            Assert.That(ex.Message, Does.Contain("File not found"));
        }

        [Test]
        public void LoadCSV_BIGCSVFile()
        {
            DateTime dtTarget = new DateTime(2024, 11, 28, 00, 30, 0);
            int minuteExpirationTest = 40;

            if (DateTime.Now - dtTarget > TimeSpan.FromMinutes(minuteExpirationTest))
            {
                Assert.Ignore("Large csv file upload test was ignored!");
            }

            EnttityHelper eh = new(stringConnectionBd1);

            Assert.That(eh.DbContext.ValidateConnection());

            DateTime startTime = DateTime.Now;
            Debug.WriteLine($"Start: {startTime}");

            // Arrange   
            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\BigCsvGerado_5000000x20.csv";
            var insertCount = 5_000_000;

            bool createTable = true;
            string tableName = "TestTable_BigCsv";
            int batchSize = 100_000;
            int timeOutSeconds = 50; // Timeout in seconds to insert 1 batch (max)

            if (eh.CheckIfExist(tableName)) eh.ExecuteNonQuery($"DROP TABLE {tableName}");

            // Act
            long result = eh.LoadCSV(csvFilePath, createTable, tableName, batchSize, timeOutSeconds);

            DateTime endTime = DateTime.Now;
            Debug.WriteLine($"End: {endTime}");

            Debug.WriteLine($"Elapsed: {endTime - startTime}");

            // Assert
            Assert.AreEqual(insertCount - 1, result); // -1 because the first row is the header
        }

        [Test]
        public void LoadTXT_ShouldInsertData()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection());

            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\CabecalhoIrregular.txt";
            var insertCount = 100;

            string tableName = "TestTable_Txt";
            int batchSize = 100000;
            int timeout = 30;
            char delimiter = ';';
            bool hasHeader = true; // The header exists
            if (hasHeader) insertCount--; // Remove the header

            if (eh.CheckIfExist(tableName)) eh.ExecuteNonQuery($"DROP TABLE {tableName}");

            long result = eh.LoadCSV(csvFilePath, true, tableName, batchSize, timeout, delimiter, hasHeader);
            Assert.That(result, Is.EqualTo(insertCount));
        }

        [Test]
        public void LoadCSV_WithoutHeader1()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection());

            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\CabecalhoVazio.csv";
            var insertCount = 100; // J100

            string tableName = "TestTableCsvHeader1";
            int batchSize = 100;
            int timeout = 30;
            char delimiter = ';';
            bool hasHeader = true; // The header exists, but is empty

            if (eh.CheckIfExist(tableName)) eh.ExecuteNonQuery($"DROP TABLE {tableName}");

            // Act
            long result = eh.LoadCSV(csvFilePath, true, tableName, batchSize, timeout, delimiter, hasHeader);

            // Assert
            Assert.AreEqual(insertCount - 1, result); // -1 because the first row is the blank header
        }

        [Test]
        public void LoadCSV_WithoutHeader2()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection());

            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\CabecalhoInexistente.csv";
            var insertCount = 101_254; // AB101254

            string tableName = "TestTableCsvHeader2";
            int batchSize = 100_000;
            int timeout = 60;
            char delimiter = ';';
            bool hasHeader = false; // The CSV file doesn't
            if (hasHeader) insertCount--; // Remove the header

            if (eh.CheckIfExist(tableName)) eh.ExecuteNonQuery($"DROP TABLE {tableName}");

            long result = eh.LoadCSV(csvFilePath, true, tableName, batchSize, timeout, delimiter, hasHeader);
            Assert.AreEqual(insertCount, result);
        }

        [Test]
        public void LoadCSV_RangeRows()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection());

            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\CabecalhoIrregular.csv"; // J100

            string tableName = "TestTableCsv_RangeRows";
            int batchSize = 100;
            int timeout = 15;
            char delimiter = ';';
            bool hasHeader = false;

            //string rangeRows = "1:20,30 ,50,99,-1"; // "1:23, -34:56, 70, 75, -1"
            //var insertCount = 24;

            string rangeRows = "2:-2"; // second to penultimat row
            var insertCount = 98;

            if (hasHeader) insertCount--; // Remove the header

            if (eh.CheckIfExist(tableName)) eh.ExecuteNonQuery($"DROP TABLE {tableName}");

            long result = eh.LoadCSV(csvFilePath, true, tableName, batchSize, timeout, delimiter, hasHeader, rangeRows);
            Assert.AreEqual(insertCount, result);
        }

        [Test]
        public void LoadCSV_UTF8()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection());

            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\ExcelUTF8.csv"; // J4
            Encoding encodingRead = Encoding.UTF8;

            string tableName = "TestTableCsv_UTF8";
            int batchSize = 10;
            int timeout = 1;
            char delimiter = ';';
            bool hasHeader = true;

            //string rangeRows = "1:20,30 ,50,99,-1"; // "1:23, -34:56, 70, 75, -1"
            //var insertCount = 24;

            string rangeRows = ":"; // all rows
            var insertCount = 4;

            if (hasHeader) insertCount--; // Remove the header

            if (eh.CheckIfExist(tableName)) eh.ExecuteNonQuery($"DROP TABLE {tableName}");

            long result = eh.LoadCSV(csvFilePath, true, tableName, batchSize, timeout, delimiter, hasHeader, rangeRows, encodingRead);
            Assert.AreEqual(insertCount, result);
        }

        [Test, Order(201)]
        public void TestIncludeAll()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection(), Is.EqualTo(true));

            /////////////////////////////////////////////////// 
            // CREATE TABLE

            eh.CreateTableIfNotExist<Group>(createOnlyPrimaryTable: true); // The M:N auxiliary table will not be created here
            eh.CreateTableIfNotExist<User>(createOnlyPrimaryTable: false); // It will be created here
            eh.CreateTableIfNotExist<Career>(false);


            /////////////////////////////////////////////////// 
            // DELETE

            //eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableNameManyToMany(typeof(User), nameof(User.Groups))} WHERE TO_CHAR(ID_TB_GROUP_USERS) LIKE '201%' OR TO_CHAR(ID_TB_USERS) LIKE '201%'");
            //eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()} WHERE TO_CHAR({nameof(User.Id)}) LIKE '201%'");
            //eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Group>()} WHERE TO_CHAR({nameof(Group.Id)}) LIKE '201%'");
            //eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Career>()} WHERE TO_CHAR({nameof(Career.IdCareer)}) LIKE '201%'");
            ResetTables("201");


            /////////////////////////////////////////////////// 
            // INSERT

            // Careers
            Career carrer1 = new() { IdCareer = 20101, Name = "Pleno", CareerLevel = 2, Active = true };
            Career carrer2 = new() { IdCareer = 20102, Name = "Trainee", CareerLevel = 0, Active = true };
            long resultCarrers = eh.Insert(new List<Career>() { carrer1, carrer2 });
            Assert.That(resultCarrers, Is.EqualTo(2));

            // Groups
            Group group1 = new() { Id = 20101, Name = "Developers", Description = "Developer Group" };
            Group group2 = new() { Id = 20102, Name = "Testers", Description = "Tester Group" };
            long resultGroups = eh.Insert(new List<Group>() { group1, group2 });
            Assert.That(resultGroups, Is.EqualTo(2));

            // Insert user with group
            User user1 = new() { Id = 20101, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 20101 };
            List<Group> groupsUserAdd = new() { group1, group2 };
            foreach (var group in groupsUserAdd) { user1.Groups.Add(group); }
            Assert.That(eh.Insert(user1), Is.EqualTo(3));

            // Insert group with user
            User user2 = new() { Id = 20102, Name = "John Victor", GitHub = "@JohnVictor", DtCreation = DateTime.Now, IdCareer = 20102, IdSupervisor = 20101 };
            Assert.That(eh.Insert(user2), Is.EqualTo(1));

            // Insert new group with user
            Group group3 = new() { Id = 20103, Name = "Operation", Description = "Operation Group" };
            group3.Users.Add(user2);
            Assert.That(eh.Insert(group3), Is.EqualTo(2));

            // Insert new user with new group
            User user3 = new() { Id = 20103, Name = "Maria Joaquina", GitHub = "@MariaJoaquina", DtCreation = DateTime.Now, IdCareer = 20102, IdSupervisor = 20102 };
            Assert.That(eh.Insert(user3), Is.EqualTo(1)); // TODO: Insert together
            Group group4 = new() { Id = 20104, Name = "Analyst", Description = "Analyst Group" };
            group4.Users.Add(user3);
            Assert.That(eh.Insert(group4), Is.EqualTo(2)); // group + aux_tb = 2


            /////////////////////////////////////////////////// 
            // GET

            List<Career>? carrers = eh.Get<Career>();
            List<Group>? groups = eh.Get<Group>();
            List<User>? users = eh.Get<User>();
            //Assert.Multiple(() =>
            //{
            //    Assert.That(carrers.Count, Is.EqualTo(2));
            //    Assert.That(groups.Count, Is.EqualTo(4));
            //    Assert.That(users.Count, Is.EqualTo(3));
            //});

            // Get Supervisor and check include LEVEL 1
            var user1Get = users?.Where(u => u.Id == 20102).FirstOrDefault(); // User1 is supervisor of user2
            User? supUser1Get = user1Get?.Supervisor;
            Assert.That(supUser1Get?.IdCareer, Is.EqualTo(20101));

            var user2Get = users?.Where(u => u.Id == 20103).FirstOrDefault(); // User2 is supervisor of user3
            User? supUser2Get = user2Get?.Supervisor; // Supervisor (level 1)
            Assert.That(supUser2Get?.Supervisor, Is.Null); // Supervisor of supervisor (level 2)

            Group? groupUser1Get = supUser1Get?.Groups.FirstOrDefault();
            Assert.That(groupUser1Get, Is.Null); // Level 2 - Null OK


            /////////////////////////////////////////////////// 
            // INCLUDE

            User? supSupUser2Get = supUser2Get?.Supervisor; // User3 -> User2 (Supervisor) [-> User1]
            Assert.That(supSupUser2Get, Is.Null); // Null OK

            ICollection<User> usersSup = new List<User>() { supUser1Get, supUser2Get };
            eh.IncludeAll(usersSup); // +Level

            supSupUser2Get = supUser2Get?.Supervisor; // User3 -> User2 (Supervisor - Level 1) -> User1 (Supervisor of Supervisor - Level 2) OK
            Assert.That(supSupUser2Get.Name, Is.EqualTo("Diego Piovezana"));

            groupUser1Get = supUser1Get?.Groups.OrderBy(g => g.Id).FirstOrDefault();
            eh.IncludeAll(groupUser1Get);
            Assert.That(groupUser1Get.Name, Is.EqualTo("Developers"));


            /////////////////////////////////////////////////// 
            // UPDATE

            user1.Groups = new List<Group>() { group1 }; // Remove group2
            user2.IdSupervisor = user3.Id; // Change supervisor (user1 -> use3) ???
            user2.Supervisor = user3; // Change supervisor (user1 -> use3) ???
            eh.Update(new List<User>() { user1, user2 });

            List<User>? usersUpdated = eh.Get<User>(true, $"TO_CHAR({nameof(User.Id)}) LIKE '201%'");
            Assert.That(usersUpdated.Count, Is.EqualTo(3));

            var groupsUser1 = usersUpdated.Where(u => u.Id == 20101).FirstOrDefault().Groups;
            Assert.Multiple(() =>
            {
                Assert.That(groupsUser1.Count, Is.EqualTo(1));
                Assert.That(groupsUser1.FirstOrDefault().Name, Is.EqualTo("Developers"));
            });

            var supUser2 = usersUpdated.Where(u => u.Id == 20102).FirstOrDefault().Supervisor;
            Assert.Multiple(() =>
            {
                Assert.That(supUser2, !Is.Null);
                Assert.That(supUser2.Name, Is.EqualTo("Maria Joaquina"));
            });


            /////////////////////////////////////////////////// 
            // DELETE

            //eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableNameManyToMany(typeof(User), nameof(User.Groups))} WHERE TO_CHAR(ID_TB_GROUP_USERS) LIKE '201%' OR TO_CHAR(ID_TB_USERS) LIKE '201%'");
            //eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()} WHERE TO_CHAR({nameof(User.Id)}) LIKE '201%'");
            //eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Group>()} WHERE TO_CHAR({nameof(Group.Id)}) LIKE '201%'");
            //eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Career>()} WHERE TO_CHAR({nameof(Career.IdCareer)}) LIKE '201%'");
            ResetTables("201");
        }

        [Test, Order(202)]
        public void Insert_GetQuery()
        {
            // Arrange
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection());

            User user = new()
            {
                Id = 20201,
                Name = "John Victor",
                GitHub = "@JohnVictor",
                DtCreation = DateTime.Now,
                IdCareer = 20202,
                IdSupervisor = 20201
            };

            // Act
            var results = eh.GetQuery.Insert(user);
            var result = results.FirstOrDefault();

            string query = result.ToQuery(eh.DbContext);
            
            // Assert
            StringAssert.StartsWith("INSERT INTO TB_USERS (Id, Name, GitHub, DtCreation, IdCareer, IdSupervisor) VALUES (:Id", result.Sql);
            StringAssert.Contains("'20202'", query);
            StringAssert.Contains("'20201'", query);
            StringAssert.EndsWith("RETURNING Id INTO :Result", query);

            Debug.WriteLine(result);
        }

        [Test, Order(203)]
        public void LoadCSV_And_GetPaginated()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection());

            string csvFilePath = "C:\\Users\\diego\\Desktop\\Tests\\Converter\\ExcelCsvGerado_1000000x10.csv";

            string tableName = "TestTableCsv_RangeRowsBig";
            int batchSize = 200_000;
            int timeout = 15;
            char delimiter = ';';
            bool hasHeader = true;

            //string rangeRows = "2:-2"; // second to penultimat row
            //var insertCount = 999_998;

            string rangeRows = "800000:-2"; // 800k th to penultimat row => 200k rows
            var insertCount = 200_000;

            if (hasHeader) insertCount--; // Remove the header => 199_999 rows

            if (!eh.CheckIfExist(tableName))
            {
                long result = eh.LoadCSV(csvFilePath, true, tableName, batchSize, timeout, delimiter, hasHeader, rangeRows);
                Assert.AreEqual(insertCount, result);
            }
            else
            {
                eh.ExecuteNonQuery($"DROP TABLE {tableName}");
                long result = eh.LoadCSV(csvFilePath, true, tableName, batchSize, timeout, delimiter, hasHeader, rangeRows);
                Assert.AreEqual(insertCount, result);
            }

            var paginated1 = eh.ExecuteSelectDt($"SELECT * FROM {tableName}", pageSize: 20); // pageIndex: 0
            Assert.AreEqual(20, paginated1.Rows.Count);
            var paginated2 = eh.ExecuteSelectDt($"SELECT * FROM {tableName}", pageSize: 28_500, pageIndex: 7);
            Assert.AreEqual(499, paginated2.Rows.Count);
            var paginated3 = eh.ExecuteSelectDt($"SELECT * FROM {tableName}", pageSize: 1_000_000, pageIndex: 0);
            Assert.AreEqual(insertCount, paginated3.Rows.Count);


            var paginated1a = eh.ExecuteSelectDt($"SELECT * FROM {tableName}", pageSize: 50_000, pageIndex: 0);
            Assert.AreEqual(50_000, paginated1a.Rows.Count);
            var paginated1b = eh.ExecuteSelectDt($"SELECT * FROM {tableName}", pageSize: 50_000, pageIndex: 3);
            Assert.AreEqual(49_999, paginated1b.Rows.Count);
            var paginated1c = eh.ExecuteSelectDt($"SELECT * FROM {tableName}", pageSize: 50_000, pageIndex: 4);
            Assert.AreEqual(0, paginated1c.Rows.Count);

            long totalRecords = eh.GetTotalRecordCountAsync($"SELECT * FROM {tableName}").Result;
            Assert.AreEqual(insertCount, totalRecords);

            eh.ExecuteNonQuery($"DROP TABLE {tableName}");
        }

        [Test, Order(204)]
        public void GetUserPaginated()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection());

            try
            {
                //eh.CreateTableIfNotExist<Group>(true); // Entity FK necessary for table MxN User and Group
                //eh.CreateTableIfNotExist<User>(false);

                eh.CreateTableIfNotExist<User>(true);
                eh.CreateTableIfNotExist<Career>(false);

                Career carrer1 = new() { IdCareer = 20401, Name = "Pleno", CareerLevel = 2, Active = true };
                Assert.AreEqual(eh.Insert(carrer1), 1);

                const int countUsers = 20;
                var users = new List<User>
                {
                    new() { Id = 20401, Name = "Diego Piovezana", GitHub = "@DiegoPiovezana", DtCreation = DateTime.Now, IdCareer = 20401 },
                    new() { Id = 20402, Name = "John Victor", GitHub = "@JohnVictor", DtCreation = DateTime.Now, IdCareer = 20401, IdSupervisor = 20401 },
                    new() { Id = 20403, Name = "Alice Souza", GitHub = "@AliceSouza", DtCreation = DateTime.Now, IdCareer = 20401 },
                    new() { Id = 20404, Name = "Maria Oliveira", GitHub = "@MariaOliveira", DtCreation = DateTime.Now, IdCareer = 20401, IdSupervisor = 20401 },
                    new() { Id = 20405, Name = "Carlos Silva", GitHub = "@CarlosSilva", DtCreation = DateTime.Now, IdCareer = 20401 },
                    new() { Id = 20406, Name = "Bruna Ferreira", GitHub = "@BrunaFerreira", DtCreation = DateTime.Now, IdCareer = 20401, IdSupervisor = 20404 },
                    new() { Id = 20407, Name = "Pedro Santos", GitHub = "@PedroSantos", DtCreation = DateTime.Now, IdCareer = 20401 },
                    new() { Id = 20408, Name = "Fernanda Lima", GitHub = "@FernandaLima", DtCreation = DateTime.Now, IdCareer = 20401, IdSupervisor = 20405 },
                    new() { Id = 20409, Name = "Lucas Pereira", GitHub = "@LucasPereira", DtCreation = DateTime.Now, IdCareer = 20401 },
                    new() { Id = 20410, Name = "J�lia Alves", GitHub = "@JuliaAlves", DtCreation = DateTime.Now, IdCareer = 20401, IdSupervisor = 20407 },
                    new() { Id = 20411, Name = "Gustavo Rocha", GitHub = "@GustavoRocha", DtCreation = DateTime.Now, IdCareer = 20401 },
                    new() { Id = 20412, Name = "Let�cia Costa", GitHub = "@LeticiaCosta", DtCreation = DateTime.Now, IdCareer = 20401, IdSupervisor = 20406 },
                    new() { Id = 20413, Name = "Thiago Martins", GitHub = "@ThiagoMartins", DtCreation = DateTime.Now, IdCareer = 20401 },
                    new() { Id = 20414, Name = "Nat�lia Souza", GitHub = "@NataliaSouza", DtCreation = DateTime.Now, IdCareer = 20401, IdSupervisor = 20410 },
                    new() { Id = 20415, Name = "Vin�cius Oliveira", GitHub = "@ViniciusOliveira", DtCreation = DateTime.Now, IdCareer = 20401 },
                    new() { Id = 20416, Name = "Amanda Ribeiro", GitHub = "@AmandaRibeiro", DtCreation = DateTime.Now, IdCareer = 20401, IdSupervisor = 20411 },
                    new() { Id = 20417, Name = "Renato Almeida", GitHub = "@RenatoAlmeida", DtCreation = DateTime.Now, IdCareer = 20401 },
                    new() { Id = 20418, Name = "Patr�cia Silva", GitHub = "@PatriciaSilva", DtCreation = DateTime.Now, IdCareer = 20401, IdSupervisor = 20413 },
                    new() { Id = 20419, Name = "F�bio Gomes", GitHub = "@FabioGomes", DtCreation = DateTime.Now, IdCareer = 20401 },
                    new() { Id = 20420, Name = "Gabriela Santos", GitHub = "@GabrielaSantos", DtCreation = DateTime.Now, IdCareer = 20401, IdSupervisor = 20404 }
                };

                Assert.AreEqual(eh.Insert(users), countUsers);

                string nameTable = eh.GetTableName<User>();

                var notPaginated1 = eh.ExecuteSelectDt($"SELECT * FROM {nameTable} WHERE TO_CHAR({nameof(User.Id)}) LIKE '204%'", pageSize: null);
                Assert.AreEqual(countUsers, notPaginated1.Rows.Count);

                var notPaginated2 = eh.ExecuteSelect<User>($"SELECT * FROM {nameTable} WHERE TO_CHAR({nameof(User.Id)}) LIKE '204%'", pageSize: null);
                Assert.AreEqual(countUsers, notPaginated2.Count);

                var notPaginated3 = eh.Get<User>(includeAll: false, filter: $"TO_CHAR({nameof(User.Id)}) LIKE '204%'", tableName: nameTable, pageSize: null);
                Assert.AreEqual(countUsers, notPaginated3.Count);


                var paginated1 = eh.ExecuteSelectDt($"SELECT * FROM {nameTable} WHERE TO_CHAR({nameof(User.Id)}) LIKE '204%'", pageSize: 10, pageIndex: 0);
                Assert.AreEqual(10, paginated1.Rows.Count);

                var paginated2 = eh.ExecuteSelect<User>($"SELECT * FROM {nameTable} WHERE TO_CHAR({nameof(User.Id)}) LIKE '204%'", pageSize: 15, pageIndex: 1);
                Assert.AreEqual(5, paginated2.Count);

                var paginated3 = eh.Get<User>(includeAll: false, filter: $"TO_CHAR({nameof(User.Id)}) LIKE '204%'", tableName: null, pageSize: 15, pageIndex: 1);
                Assert.AreEqual(5, paginated3.Count);
            }
            finally
            {
                eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()} WHERE TO_CHAR({nameof(User.Id)}) LIKE '204%'");
                eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Career>()} WHERE TO_CHAR({nameof(Career.IdCareer)}) LIKE '204%'");
            }
        }

        [Test, Order(205)]
        public async Task ComplexQueryWithPaginationAndRecordCount()
        {
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection());

            try
            {

                // Cria��o das tabelas
                eh.CreateTableIfNotExist<Group>(createOnlyPrimaryTable: true); // Doesnt create the TB_GROUP_USERStoGROUPS table
                eh.CreateTableIfNotExist<User>(createOnlyPrimaryTable: false); // Cretes the TB_GROUP_USERStoGROUPS table
                eh.CreateTableIfNotExist<Career>(createOnlyPrimaryTable: false);


                // Inser��o de carreira
                Career career1 = new() { IdCareer = 20501, Name = "Junior", CareerLevel = 1, Active = true };
                Career career2 = new() { IdCareer = 20502, Name = "Pleno", CareerLevel = 2, Active = true };
                Assert.AreEqual(eh.Insert(new List<Career> { career1, career2 }), 2);


                // Inser��o de grupos
                var groups = Enumerable.Range(20501, 5).Select(i => new Group
                {
                    Id = i,
                    Name = $"Group {i}",
                    Description = $"Description for Group {i}"
                }).ToList();
                Assert.AreEqual(eh.Insert(groups), groups.Count);


                // Inser��o de usu�rios
                var users = Enumerable.Range(20501, 20).Select(i =>
                {
                    var user = new User
                    {
                        Id = i,
                        Name = $"User {i}",
                        GitHub = $"@User{i}",
                        DtCreation = DateTime.Now,
                        IdCareer = i % 2 == 0 ? 20501 : 20502
                    };

                    // Adiciona os grupos ao usu�rio
                    user.Groups.Add(groups[i % groups.Count]);
                    if (i % 3 == 0) // Adiciona outro grupo como exemplo
                    {
                        user.Groups.Add(groups[(i + 1) % groups.Count]);
                    }

                    return user;

                }).ToList();

                // Insere usu�rios e valida a quantidade de registros inseridos em ambas as tabelas
                int expectedUserCount = users.Count;
                int expectedAuxiliaryCount = users.Sum(u => u.Groups.Count); // Soma os relacionamentos User-Group
                int totalExpectedInserts = expectedUserCount + expectedAuxiliaryCount;

                // Realiza a inser��o
                long actualInsertCount = eh.Insert(users);

                // Valida a quantidade total de inser��es
                Assert.AreEqual(totalExpectedInserts, actualInsertCount);


                // Query complexa com JOIN, UNION, SUM, WITH, etc.
                string complexQuery = @"
                WITH UserGroupSummary AS (
                    SELECT 
                        u.Id AS UserId,
                        u.Name AS UserName,
                        g.Id AS GroupId,
                        g.Name AS GroupName,
                        c.Name AS CareerName,
                        COUNT(ug.ID_TB_USERS) AS UserCountInGroup
                    FROM {0} u
                    JOIN {1} ug ON u.Id = ug.ID_TB_USERS
                    JOIN {2} g ON ug.ID_TB_GROUP_USERS = g.Id
                    JOIN {3} c ON u.IdCareer = c.IdCareer                    
                    GROUP BY u.Id, u.Name, g.Id, g.Name, c.Name
                )

                SELECT 
                    UserId,
                    UserName,
                    GroupId,
                    GroupName,
                    CareerName,
                    UserCountInGroup
                FROM UserGroupSummary
                WHERE TO_CHAR(UserId) LIKE '205%'

                UNION ALL

                SELECT 
                    NULL AS UserId, 
                    NULL AS UserName, 
                    g.Id AS GroupId, 
                    g.Name AS GroupName, 
                    NULL AS CareerName, 
                    COUNT(ug.ID_TB_USERS) AS UserCountInGroup
                FROM {1} ug
                JOIN {2} g ON ug.ID_TB_GROUP_USERS = g.Id
                WHERE TO_CHAR(g.Id) LIKE '205%'
                GROUP BY g.Id, g.Name

                ORDER BY GroupId, UserId NULLS LAST";


                // Substituindo os nomes das tabelas na query
                complexQuery = string.Format(complexQuery,
                    eh.GetTableName<User>(),
                    eh.GetTableNameManyToMany(typeof(User), nameof(User.Groups)),
                    eh.GetTableName<Group>(),
                    eh.GetTableName<Career>());


                // Teste de pagina��o
                var paginatedResult = eh.ExecuteSelectDt(complexQuery, pageSize: 10, pageIndex: 0);
                Assert.AreEqual(10, paginatedResult.Rows.Count);

                var secondPageResult = eh.ExecuteSelectDt(complexQuery, pageSize: 10, pageIndex: 3); // Total: 32 (4 pages)
                Assert.AreEqual(2, secondPageResult.Rows.Count); // Segunda p�gina deve ter registros (dependendo da quantidade de dados)


                // Teste de contagem total
                long totalRecords = await eh.GetTotalRecordCountAsync(complexQuery);
                Assert.AreEqual(32, totalRecords); // Deve retornar a quantidade total de registros da query


                // Valida��o cruzada
                long allRecords = eh.ExecuteSelectDt(complexQuery, pageSize: null).Rows.Count;
                Assert.AreEqual(totalRecords, allRecords);

            }
            finally
            {
                // Limpeza das tabelas
                ResetTables("205");
            }
        }


        [Test, Order(206)]
        public async Task GetTotalRecordCountAsync_ShouldHandleDifferentQueryTypes()
        {
            // Arrange
            EnttityHelper eh = new(stringConnectionBd1);
            Assert.That(eh.DbContext.ValidateConnection());

            eh.CreateTableIfNotExist<Group>(createOnlyPrimaryTable: true); // Doesnt create the TB_GROUP_USERStoGROUPS table
            eh.CreateTableIfNotExist<User>(createOnlyPrimaryTable: false); // Cretes the TB_GROUP_USERStoGROUPS table
            eh.CreateTableIfNotExist<Career>(createOnlyPrimaryTable: false);

            ResetTables("206");

            // Inser��o de carreira
            Career career1 = new() { IdCareer = 20601, Name = "Junior", CareerLevel = 1, Active = true };
            Career career2 = new() { IdCareer = 20602, Name = "Pleno", CareerLevel = 2, Active = true };
            Assert.AreEqual(eh.Insert(new List<Career> { career1, career2 }), 2);


            // Inser��o de grupos
            var groups = Enumerable.Range(20601, 5).Select(i => new Group
            {
                Id = i,
                Name = $"Group {i}",
                Description = $"Description for Group {i}"
            }).ToList();
            Assert.AreEqual(eh.Insert(groups), groups.Count);


            // Inser��o de usu�rios
            var users = Enumerable.Range(20601, 20).Select(i =>
            {
                var user = new User
                {
                    Id = i,
                    Name = $"User {i}",
                    GitHub = $"@User{i}",
                    DtCreation = DateTime.Now,
                    IdCareer = i % 2 == 0 ? 20601 : 20602
                };

                // Adiciona os grupos ao usu�rio
                user.Groups.Add(groups[i % groups.Count]);
                if (i % 3 == 0) // Adiciona outro grupo como exemplo
                {
                    user.Groups.Add(groups[(i + 1) % groups.Count]);
                }

                return user;

            }).ToList();

            // Insere usu�rios e valida a quantidade de registros inseridos em ambas as tabelas
            int expectedUserCount = users.Count;
            int expectedAuxiliaryCount = users.Sum(u => u.Groups.Count); // Soma os relacionamentos User-Group
            int totalExpectedInserts = expectedUserCount + expectedAuxiliaryCount;

            // Realiza a inser��o
            long actualInsertCount = eh.Insert(users);

            // Valida a quantidade total de inser��es
            Assert.AreEqual(totalExpectedInserts, actualInsertCount);



            // Query com WITH e JOIN - 27 registros
            string complexQueryWithWithAndJoin = @"
                WITH UserGroupSummary AS (
                    SELECT 
                        u.Id AS UserId,
                        u.Name AS UserName,
                        g.Id AS GroupId,
                        g.Name AS GroupName
                    FROM TB_USERS u
                    JOIN TB_GROUP_USERStoGROUPS ug ON u.Id = ug.ID_TB_USERS
                    JOIN TB_GROUP_USERS g ON ug.ID_TB_GROUP_USERS = g.Id
                )
                SELECT * FROM UserGroupSummary WHERE TO_CHAR(UserId) LIKE '206%' ORDER BY GroupName, UserName";

            // Query com UNION - 8 registros
            string queryWithUnion = @"
                SELECT Id, Name 
                FROM TB_USERS
                WHERE Id < 20605 
                AND TO_CHAR(Id) LIKE '206%'
                UNION ALL
                SELECT Id, Name 
                FROM TB_GROUP_USERS
                WHERE Id < 20605
                AND TO_CHAR(Id) LIKE '206%'
                ORDER BY Name";

            // Query simples sem cl�usulas adicionais - 20 registros
            string simpleQuery = "SELECT Id, Name FROM TB_USERS WHERE TO_CHAR(Id) LIKE '206%'";

            // Query com subquery - 6 registros
            string queryWithSubquery = @"
                SELECT u.Id, u.Name
                FROM TB_USERS u
                WHERE u.Id IN (SELECT ug.ID_TB_USERS FROM TB_GROUP_USERStoGROUPS ug WHERE ug.ID_TB_GROUP_USERS = 20601)";

            // Teste para cada query
            // Act & Assert
            long totalRecords;

            // Teste com complexQueryWithWithAndJoin
            totalRecords = await eh.GetTotalRecordCountAsync(complexQueryWithWithAndJoin);
            Assert.AreEqual(27, totalRecords, "A contagem de registros da query complexa est� incorreta.");

            // Teste com queryWithUnion
            totalRecords = await eh.GetTotalRecordCountAsync(queryWithUnion);
            Assert.AreEqual(8, totalRecords, "A contagem de registros da query com UNION est� incorreta.");

            // Teste com simpleQuery
            totalRecords = await eh.GetTotalRecordCountAsync(simpleQuery);
            Assert.AreEqual(20, totalRecords, "A contagem de registros da query simples est� incorreta.");

            // Teste com queryWithSubquery
            totalRecords = await eh.GetTotalRecordCountAsync(queryWithSubquery);
            Assert.AreEqual(6, totalRecords, "A contagem de registros da query com subquery est� incorreta.");


            eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableNameManyToMany(typeof(User), nameof(User.Groups))}");
            eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<User>()} WHERE TO_CHAR({nameof(User.Id)}) LIKE '206%'");
            eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Group>()} WHERE TO_CHAR({nameof(Group.Id)}) LIKE '206%'");
            eh.ExecuteNonQuery($"DELETE FROM {eh.GetTableName<Career>()} WHERE TO_CHAR({nameof(Career.IdCareer)}) LIKE '206%'");

        }




    }
}
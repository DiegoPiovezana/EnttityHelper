using DiegoPiov.UserManagement;
using EH;
using NUnit.Framework.Internal;
using TestEnttityHelper.OthersEntity;

namespace TestEnttityHelper
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [Test]
        public void TestConnection()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            bool test = eh.DbContext.ValidateConnection();
            Assert.That(test, Is.EqualTo(true));
        }

        [Test]
        public void TestInsertEntity()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                //bool result = eh.Insert(new { Id = 1, Name = "Test" }, null);

                EntityTest entityTest = new() { Id = 80, Name = "Testando entidade 80 via C#", StartDate = DateTime.Now };
                //bool result = eh.Insert(entityTest);
                bool result = eh.Insert(entityTest, nameof(entityTest.Id)) == 1;

                Assert.That(result, Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestUpdateEntity()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 1, Name = "Testando entidade 1 atualizando hora via C#", StartDate = DateTime.Now };
                bool result = eh.Update(entityTest, nameof(entityTest.Id)) > 0;

                Assert.That(result, Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestSearchEntity()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 1 };
                var result = eh.Search(entityTest, true, nameof(entityTest.Id));

                Assert.That(result?.Name.Equals("Teste 1 via banco"), Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestGetEntity()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                var result = eh.Get<EntityTest>();

                Assert.That(result[5].StartDate.Date.Equals(DateTime.Today), Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestFullEntity()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                EntityTest entityTest = new() { Id = 300, Name = "Testando 1 entidade 100 via C#", StartDate = DateTime.Now };
                eh.Insert(entityTest, nameof(entityTest.Id));

                EntityTest entityTest2 = new() { Id = 300, Name = "Testando 2 entidade 300 atualizando hora via C#", StartDate = DateTime.Now };
                eh.Update(entityTest2, nameof(entityTest.Id));

                var result = eh.Get<EntityTest>(true, $"{nameof(EntityTest.Id)} = 300");

                Assert.That(result[0].Name.Equals("Testando 2 entidade 300 atualizando hora via C#"), Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestFileDirSR()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {               
                eh.TypesDefault.Add("Version", "NVARCHAR2(10)");
                eh.ReplacesTableName = new Dictionary<string, string> { { "&ID", "1" } };

                eh.CreateTableIfNotExist<FileDirSR>();                
                eh.Insert(new FileDirSR { Name = "Teste1"});
                //eh.Insert(new FileDirSR { Name = "Teste2", Hidden = true, SizeMinKb = 50, MandatoryExist = true });
                //eh.Insert(new FileDirSR { Name = "Teste3", Hidden = false, SizeMinKb = 10, MandatoryExist = true, DateTimeModifiedMin = DateTime.ParseExact("14/01/2023 09:00", "dd/MM/yyyy HH:mm", null) });
                //var ent = new FileDirSR { Name = "Teste4", Hidden = true, SizeMinKb = 20, VersionMin = "1.2.0" };
                //eh.Insert(ent);
                var inserts = eh.Get<FileDirSR>() ?? throw new Exception("Inserts is null");
                var entityTest = eh.Search(new FileDirSR { Name = "Teste4" });
                
                Console.WriteLine(entityTest);
                Assert.That(inserts.Count == 1, Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }


    }
}
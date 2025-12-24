using EH;
using EH.Connection;
using NUnit.Framework.Internal;
using DGP.UserManagement;

namespace TestEnttityHelper
{
    public class Tests_UM
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestConnection()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            bool test = eh.DbContext.ValidateConnection();
            Assert.That(test, Is.EqualTo(true));
        }

        [Test]
        public void TestCadastrarUsuario()
        {
            Database db = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");

            Usuario user = new()
            {
                IdUsuario = 2,
                EidUsuario = "usuario.teste",
                Email = "usuario.teste@gmail.com",
                Login = "805555",
                Nome = "Usuário Teste",
                Ativo = true,
                IdOrigem = 0,
                Origem = new Origem() { IdOrigem = 0, Nome = "Origem Teste" },  
                DtCriacao = DateTime.Now,
                DtUltimoLogin = null,
                DtAtivacao = DateTime.Now,
                DtDesativacao = null,
                DtAlteracao = null,
                DtRevisao = null,
                UsuarioInterno = "Y",
                IdSupervisor = 0,
                AdminTest = "1"
            };

            var result = UsuarioDTO.CadastrarUsuario(user, db);
            Assert.That(result, Is.EqualTo(true));
        }


        [Test]
        public void TestAtualizarUsuario()
        {
            Database db = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");

            Usuario user = new()
            {
                IdUsuario = 1,
                EidUsuario = "usuario.teste",
                Email = "usuario.teste@gmail.com",
                Login = "805555",
                Nome = "Usuário Teste Atualizado",
                Ativo = true,
                DtCriacao = DateTime.Now,
                DtUltimoLogin = null,
                DtAtivacao = DateTime.Now,
                DtDesativacao = null,
                DtAlteracao = null,
                DtRevisao = null,
                UsuarioInterno = "Y",
                IdSupervisor = null,
                AdminTest = "1"
            };

            var result = UsuarioDTO.AtualizarUsuario(user, db);
            Assert.That(result, Is.EqualTo(true));
        }


        [Test]
        public void TestRevisarUsuarios()
        {
            Database db = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");

            //Usuario user = new()
            //{
            //    IdUsuario = 2,
            //    EidUsuario = "usuario2.teste",
            //    Email = "usuario2.teste@gmail.com",
            //    Login = "13123",
            //    Nome = "Usuário Supervisionado Teste 2",
            //    Ativo = true,
            //    IdOrigem = 0,
            //    DtCriacao = DateTime.Now,
            //    DtUltimoLogin = null,
            //    DtAtivacao = DateTime.Now,
            //    DtDesativacao = null,
            //    DtAlteracao = null,
            //    DtRevisao = null,
            //    UsuarioInterno = "Y",
            //    IdSupervisor = 1,
            //    AdminTest = "1"
            //};

            //var result1 = UsuarioDTO.CadastrarUsuario(user, db);

            Usuario supervisor = new()
            {
                IdUsuario = 1,
                EidUsuario = "usuario.teste",
                Email = "usuario.teste@gmail.com",
                Login = "805555",
                Nome = "Usuário Teste",
                Ativo = true,
                DtCriacao = DateTime.Now,
                DtUltimoLogin = null,
                DtAtivacao = DateTime.Now,
                DtDesativacao = null,
                DtAlteracao = null,
                DtRevisao = null,
                UsuarioInterno = "Y",
                IdSupervisor = null,
                AdminTest = "1"
            };

            var result2 = UsuarioDTO.RevisarUsuarios(supervisor, db) > 0;
            Assert.That(result2, Is.EqualTo(true));
        }

        [Test]
        public void TestSearchUsuario()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                Usuario user = new()
                {
                    IdUsuario = 1,
                    EidUsuario = "usuario.teste",
                    Email = "usuario.teste@gmail.com",
                    Login = "805555",
                    Nome = "Usuário Teste"
                };

                var result = eh.Search(user);

                Assert.That(result?.Supervisor?.IdUsuario.Equals(0), Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestGetUsuario()
        {
            EnttityHelper eh = new($"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle");
            if (eh.DbContext.ValidateConnection())
            {
                var result = eh.Get<Usuario>();

                Assert.That(result[5].Supervisor?.IdUsuario.Equals(0), Is.EqualTo(true));
            }
            else
            {
                Assert.Fail();
            }
        }



    }
}
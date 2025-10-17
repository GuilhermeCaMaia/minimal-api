
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;

namespace Test.Domain.Servicos
{
    public class VeiculoServicoTest
    {
        private DbContexto CriarContextoDeTeste()
        {
            // Obtém o diretório base onde o app está rodando
            var basePath = AppContext.BaseDirectory;

            // "Sobe" até a pasta raiz do projeto (3 níveis acima do bin)
            var projectPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", ".."));

            // Monta o builder de configuração
            var builder = new ConfigurationBuilder()
                .SetBasePath(projectPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            return new DbContexto(configuration);
        }

        public void TestarSalvarAdministrador()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var vei = new Veiculo();
            vei.Id = 1;
            vei.Nome = "Teste";
            vei.Marca = "marca de teste";
            vei.Ano = 2025;

            var veiculoServico = new VeiculoServico(context);

            // Act
            veiculoServico.Incluir(vei);

            // Assert
            Assert.AreEqual(1, veiculoServico.Todos(1).Count());
        }

        public void TestandoBuscarPorId()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var vei = new Veiculo();
            vei.Id = 1;
            vei.Nome = "Teste";
            vei.Marca = "marca de teste";
            vei.Ano = 2025;

            var veiculoServico = new VeiculoServico(context);

            // Act
            veiculoServico.Incluir(vei);
            var veidobanco = veiculoServico.BuscarPorId(vei.Id);

            // Assert
            Assert.AreEqual(1, veidobanco.Id);
        }
    }
}
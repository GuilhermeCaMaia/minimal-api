using MinimalApi.Dominio.Entidades;

namespace Test.Domain.Entidades;

[TestClass]
public class VeiculoTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        // Arrange
        var veiculo = new Veiculo();

        // Act
        veiculo.Id = 1;
        veiculo.Nome = "Teste";
        veiculo.Marca = "TesteMarca";
        veiculo.Ano = 2025;

        // Assert
        Assert.AreEqual(1, veiculo.Id);
        Assert.AreEqual("Teste", veiculo.Nome);
        Assert.AreEqual("TesteMarca", veiculo.Marca);
        Assert.AreEqual(2025, veiculo.Ano);

    }
}
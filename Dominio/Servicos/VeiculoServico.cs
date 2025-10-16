using System.Linq;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.DTO;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos;

public class VeiculoServico : IVeiculoServico
{
    private readonly DbContexto _contexto;
    public VeiculoServico(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public void Apagar(Veiculo veiculo)
    {
        _contexto.veiculos.Remove(veiculo);
        _contexto.SaveChanges();
    }

    public void Atualizar(Veiculo veiculo)
    {
        _contexto.veiculos.Update(veiculo);
        _contexto.SaveChanges();
    }

    public Veiculo? BuscarPorId(int id)
    {
        return _contexto.veiculos.Where(v => v.Id == id).FirstOrDefault();
    }

    public void Incluir(Veiculo veiculo)
    {
        _contexto.veiculos.Add(veiculo);
        _contexto.SaveChanges();
    }

    public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
    {
        var query = _contexto.veiculos.AsQueryable();
        if (!string.IsNullOrEmpty(nome))
            query = query.Where(v => EF.Functions.Like(v.Nome, $"%{nome}%"));

        int itensPorPagina = 10;
        if (pagina != null)
            query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

        return query.ToList();
    }
}

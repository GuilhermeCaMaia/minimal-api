
using MinimalApi.Dominio.Enuns;

namespace MinimalApi.DTO;
public class AdministradorDTO
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
    public Perfil? Perfil { get; set; }
}
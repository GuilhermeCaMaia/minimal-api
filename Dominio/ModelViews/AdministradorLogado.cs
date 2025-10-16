namespace MinimalApi.Dominio.ModelViews;

public record AdministradorLogado
{
    public string Nome { get; init; }
    public string Email { get; init; }
    public string Perfil { get; init; }
    public string Token { get; init; }
}
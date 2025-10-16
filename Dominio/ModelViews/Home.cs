namespace MinimalApi.Dominio.ModelViews
{
    public struct Home
    {
        public string Documentacao { get => "/swagger"; }

        public string Mensagem { get => "Bem-vindo à API de veiculos minimal-api!"; }
    }
}
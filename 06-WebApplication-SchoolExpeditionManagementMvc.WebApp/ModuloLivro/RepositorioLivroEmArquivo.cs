using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Compartilhado;

namespace _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloLivro;

public class RepositorioLivroEmArquivo : RepositorioBaseEmArquivo<Livro>, IRepositorioLivro
{
    public RepositorioLivroEmArquivo(ContextoJson contexto) : base(contexto) { }

    protected override List<Livro> CarregarRegistros()
    {
        return contexto.Livros;
    }
}
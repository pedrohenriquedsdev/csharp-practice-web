using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Compartilhado;

namespace _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloEstante;

public class RepositorioEstanteEmArquivo : RepositorioBaseEmArquivo<Estante>, IRepositorioEstante
{
    public RepositorioEstanteEmArquivo(ContextoJson contexto) : base(contexto) { }

    protected override List<Estante> CarregarRegistros()
    {
        return contexto.Estantes;
    }
}
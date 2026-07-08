using System;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado.Arquivos;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloFacilitador;

public class RepositorioFacilitadorEmArquivo : RepositorioBaseEmArquivo<Facilitador>
{
    public RepositorioFacilitadorEmArquivo(ContextoJson contexto) : base(contexto)
    {
    }

    protected override List<Facilitador> CarregarRegistros()
    {
        return contexto.Facilitadores;
    }
}

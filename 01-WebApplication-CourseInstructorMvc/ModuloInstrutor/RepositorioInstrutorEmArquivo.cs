using System;
using _01_WebApplication_CourseInstructorMvc.Compartilhado.Arquivos;

namespace _01_WebApplication_CourseInstructorMvc.ModuloInstrutor;

public class RepositorioInstrutorEmArquivo : RepositorioBaseEmArquivo<Instrutor>
{
    public RepositorioInstrutorEmArquivo(ContextoJson contexto) : base(contexto)
    {
    }

    protected override List<Instrutor> CarregarRegistros()
    {
        return contexto.Instrutores;
    }
}





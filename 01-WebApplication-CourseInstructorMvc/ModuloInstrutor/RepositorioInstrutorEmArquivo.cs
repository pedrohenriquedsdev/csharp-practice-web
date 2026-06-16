using System;
using _01_WebApplication_CourseInstructorMvc.Compartilhado.Arquivos;

namespace _01_WebApplication_CourseInstructorMvc.ModuloInstrutor;

public class RepositorioInstrutorEmArquivo : RepositorioBaseEmArquivo<Curso>
{
    public RepositorioInstrutorEmArquivo(ContextoJson contexto) : base(contexto)
    {
    }

    protected override List<Curso> CarregarRegistros()
    {
        return contexto.Instrutores;
    }
}





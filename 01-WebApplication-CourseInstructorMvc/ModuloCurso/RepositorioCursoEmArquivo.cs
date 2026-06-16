using System;
using _01_WebApplication_CourseInstructorMvc.Compartilhado.Arquivos;

namespace _01_WebApplication_CourseInstructorMvc.ModuloCurso;

public class RepositorioCursoEmArquivo : RepositorioBaseEmArquivo<Curso>
{
    public RepositorioCursoEmArquivo(ContextoJson contexto) : base(contexto)
    {
    }

    protected override List<Curso> CarregarRegistros()
    {
        return contexto.Cursos;
    }
}

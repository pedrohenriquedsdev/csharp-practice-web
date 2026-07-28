using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Compartilhado;
using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloAluno;

namespace _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloAluno;

public class RepositorioAlunoEmArquivo : RepositorioBaseEmArquivo<Aluno>, IRepositorioAluno
{
    public RepositorioAlunoEmArquivo(ContextoJson contexto) : base(contexto) { }

    protected override List<Aluno> CarregarRegistros()
    {
        return contexto.Alunos;
    }
}
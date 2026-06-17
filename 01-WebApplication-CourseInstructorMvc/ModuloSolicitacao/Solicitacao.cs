using _01_WebApplication_CourseInstructorMvc.Compartilhado;
using _01_WebApplication_CourseInstructorMvc.ModuloCurso;

namespace _01_WebApplication_CourseInstructorMvc.ModuloSolicitacao;

public class Solicitacao : EntidadeBase<Solicitacao>
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; } = null;
    public Curso Curso { get; set; } = null!;
    public DateTime DataAbertura { get; set; } = DateTime.Now;
    public bool EstaConcluido { get; set; }
    public int TempoDecorrido
    {
        get
        {
            TimeSpan diferencaTempo = DateTime.Now.Subtract(DataAbertura);

            return diferencaTempo.Days;
        }
    }

    public Solicitacao() { }

    public Solicitacao(string titulo, string? descricao, Curso curso) : this()
    {
        Titulo = titulo;
        Descricao = descricao;
        Curso = curso;
    }

    public void Concluir()
    {
        EstaConcluido = true;
    }

    public override List<string> Validar()
    {
        List<string> erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Titulo) || Titulo.Length < 2 || Titulo.Length > 50)
            erros.Add("O campo \"Título\" deve conter entre 2 e 50 caracteres.");

        if (Curso == null)
            erros.Add("O campo \"Curso\" deve ser preenchido.");

        return erros;
    }

    public override void AtualizarDados(Solicitacao entidadeAtualizada)
    {
        Titulo = entidadeAtualizada.Titulo;
        Descricao = entidadeAtualizada.Descricao;
        Curso = entidadeAtualizada.Curso;
    }
}

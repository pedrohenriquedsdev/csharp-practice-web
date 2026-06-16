using _01_WebApplication_CourseInstructorMvc.Compartilhado;
using _01_WebApplication_CourseInstructorMvc.ModuloInstrutor;

namespace _01_WebApplication_CourseInstructorMvc.ModuloCurso;

public class Curso : EntidadeBase<Curso>
{
    public string Nome { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public DateTime DataInicio { get; set; }

    public Instrutor Instrutor { get; set; } = null!;

    public Curso() { }

    public Curso(string nome, decimal valor, DateTime dataInicio, Instrutor instrutor)
    {
        Nome = nome;
        Valor = valor;
        DataInicio = dataInicio;
        Instrutor = instrutor;
    }

    public override void AtualizarDados(Curso entidadeAtualizada)
    {
        Nome = entidadeAtualizada.Nome;
        Valor = entidadeAtualizada.Valor;
        DataInicio = entidadeAtualizada.DataInicio;
    }

    public override List<string> Validar()
    {
        List<string> erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Nome))
            erros.Add("O campo \"Nome\" é obrigatório.");
        else if (Nome.Length < 2 || Nome.Length > 50)
            erros.Add("O campo \"Nome\" deve conter entre 2 e 50 caracteres.");

        if (Valor <= 0)
            erros.Add("O campo \"Valor\" deve conter um valor positivo.");

        if (DataInicio == DateTime.MinValue)
            erros.Add("O campo \"Data de Início\" é obrigatório.");

        if (Instrutor == null)
            erros.Add("O campo \"Instrutor\" deve ser preenchido.");

        return erros;
    }
}
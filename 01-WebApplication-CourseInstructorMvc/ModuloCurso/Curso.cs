using _01_WebApplication_CourseInstructorMvc.Compartilhado;
using _01_WebApplication_CourseInstructorMvc.ModuloInstrutor;

namespace _01_WebApplication_CourseInstructorMvc.ModuloCurso;

public class Curso : EntidadeBase<Curso>
{
    public required string Nome { get; set; }

    public decimal Valor { get; set; }

    public DateTime DataInicio { get; set; }

    public Instrutor? Instrutor;

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

        return erros;
    }
}
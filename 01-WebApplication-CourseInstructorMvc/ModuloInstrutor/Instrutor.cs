using _01_WebApplication_CourseInstructorMvc.Compartilhado;

namespace _01_WebApplication_CourseInstructorMvc.ModuloInstrutor;

public class Instrutor : EntidadeBase<Instrutor>
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;

    public Instrutor() { }

    public Instrutor(string nome, string email, string telefone)
    {
        Nome = nome;
        Email = email;
        Telefone = telefone;
    }

    public override List<string> Validar()
    {
        List<string> erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Nome) || Nome.Length < 2 || Nome.Length > 50)
            erros.Add("O campo \"Nome\" deve conter entre 2 e 50 caracteres.");

        if (Telefone == null)
            erros.Add("O campo \"Telefone\" deve ser preenchido.");

        if (Email == null)
            erros.Add("O campo \"Email\" deve ser preenchido.");

        return erros;
    }

    public override void AtualizarDados(Instrutor entidadeAtualizada)
    {
        Nome = entidadeAtualizada.Nome;
        Email = entidadeAtualizada.Email;
        Telefone = entidadeAtualizada.Telefone;
    }
}
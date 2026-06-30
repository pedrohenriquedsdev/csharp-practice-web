using _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloFacilitador;

public class Facilitador : EntidadeBase<Facilitador>
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Especialidade { get; set; } = string.Empty;
    public Facilitador() { }

    public Facilitador(string nome, string email, string especialidade) : this()
    {
        Nome = nome;
        Email = email;
        Especialidade = especialidade;
    }

    public override void AtualizarDados(Facilitador entidadeAtualizada)
    {
        Nome = entidadeAtualizada.Nome;
        Email = entidadeAtualizada.Email;
        Especialidade = entidadeAtualizada.Especialidade;
    }

    public override List<string> Validar()
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Nome) || Nome.Length < 2 || Nome.Length > 50)
            erros.Add("O campo \"Nome\" deve conter entre 2 e 50 caracteres.");

        if (string.IsNullOrWhiteSpace(Email))
            erros.Add("O campo \"Email\" deve ser preenchido.");

        if (string.IsNullOrWhiteSpace(Especialidade))
            erros.Add("O campo \"Especialidade\" deve ser preenchido.");

        return erros;
    }
}

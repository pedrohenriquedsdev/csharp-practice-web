using _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloParticipante;

public class Participante : EntidadeBase<Participante>
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;

    public Participante() { }

    public Participante(string nome, string email, string telefone) : this()
    {
        Nome = nome;
        Email = email;
        Telefone = telefone;
    }

    public override void AtualizarDados(Participante entidadeAtualizada)
    {
        Nome = entidadeAtualizada.Nome;
        Email = entidadeAtualizada.Email;
        Telefone = entidadeAtualizada.Telefone;
    }

    public override List<string> Validar()
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Nome) || Nome.Length < 2 || Nome.Length > 50)
            erros.Add("O campo \"Nome\" deve conter entre 2 e 50 caracteres.");

        if (string.IsNullOrWhiteSpace(Email) || Email.Length < 5 || Email.Length > 100)
            erros.Add("O campo \"Email\" deve conter entre 5 e 100 caracteres.");

        if (string.IsNullOrWhiteSpace(Telefone) || Telefone.Length < 10 || Telefone.Length > 15)
            erros.Add("O campo \"Telefone\" deve conter entre 10 e 15 caracteres.");

        return erros;
    }
}

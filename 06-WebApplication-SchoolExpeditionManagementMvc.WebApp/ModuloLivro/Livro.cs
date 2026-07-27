using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Compartilhado;
using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloEstantes;

namespace _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloLivro;

public class Livro : EntidadeBase<Livro>
{
    public string Titulo { get; set; } = string.Empty;

    public int NumeroEdicao { get; set; } = 1;

    public DateTime DataPublicacao { get; set; }

    public Estante Estante { get; set; } = null!;

    public Livro()
    {
    }

    // Construtor para facilitar a criação de um Livro já preenchido.
    public Livro(string titulo, int numeroEdicao, DateTime dataPublicacao, Estante estante) : this()
    {
        Titulo = titulo;
        NumeroEdicao = numeroEdicao;
        DataPublicacao = dataPublicacao;
        Estante = estante;
    }

    // Regras que dependem de outras entidades
    // (como título e edição duplicados) são validadas no repositório.
    public override List<string> Validar()
    {
        List<string> erros = new();

        if (string.IsNullOrWhiteSpace(Titulo))
            erros.Add("O campo \"Título\" é obrigatório.");

        else if (Titulo.Length < 2 || Titulo.Length > 100)
            erros.Add("O campo \"Título\" deve possuir entre 2 e 100 caracteres.");

        if (NumeroEdicao <= 0)
            erros.Add("O campo \"Número da edição\" deve ser maior que zero.");

        if (DataPublicacao == default)
            erros.Add("O campo \"Data de publicação\" é obrigatório.");

        else if (DataPublicacao > DateTime.Today)
            erros.Add("A data de publicação não pode ser futura.");

        if (Estante == null)
            erros.Add("O campo \"Estante\" é obrigatório.");

        return erros;
    }

    // Atualiza os dados editáveis da entidade.
    // O Id permanece o mesmo.
    public override void AtualizarDados(Livro entidadeAtualizada)
    {
        Titulo = entidadeAtualizada.Titulo;
        NumeroEdicao = entidadeAtualizada.NumeroEdicao;
        DataPublicacao = entidadeAtualizada.DataPublicacao;
        Estante = entidadeAtualizada.Estante;
    }
}
using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Compartilhado;

namespace _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloAluno;

public class Aluno : EntidadeBase<Aluno>
{
    public string Nome { get; set; } = string.Empty;

    public string NomeResponsavel { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    // Construtor vazio. Necessário para alguns cenários do ASP.NET MVC.
    public Aluno()
    {
    }

    // Construtor para facilitar a criação de um Aluno já preenchida.
    public Aluno(string nome, string nomeResponsavel, string telefone) : this()
    {
        Nome = nome;
        NomeResponsavel = nomeResponsavel;
        Telefone = telefone;
    }

    // Responsável por validar apenas as regras da própria entidade.
    public override List<string> Validar()
    {
        List<string> erros = new();

        // Validação do nome
        if (string.IsNullOrWhiteSpace(Nome))
            erros.Add("O campo \"Nome\" é obrigatório.");
        else if (Nome.Length < 3 || Nome.Length > 100)
            erros.Add("O campo \"Nome\" deve possuir entre 3 e 100 caracteres.");

        // Validação do responsável
        if (string.IsNullOrWhiteSpace(NomeResponsavel))
            erros.Add("O campo \"Nome do Responsável\" é obrigatório.");
        else if (NomeResponsavel.Length < 3 || NomeResponsavel.Length > 100)
            erros.Add("O campo \"Nome do Responsável\" deve possuir entre 3 e 100 caracteres.");

        // Validação do telefone
        if (string.IsNullOrWhiteSpace(Telefone))
            erros.Add("O campo \"Telefone\" é obrigatório.");
        else
        {
            // Considera apenas os dígitos do telefone
            string telefoneSomenteDigitos = new(Telefone.Where(char.IsDigit).ToArray());

            if (telefoneSomenteDigitos.Length < 10 || telefoneSomenteDigitos.Length > 11)
                erros.Add("O campo \"Telefone\" deve possuir entre 10 e 11 dígitos.");
        }

        return erros;
    }

    // Atualiza os dados editáveis da entidade.
    // O Id permanece o mesmo.
    public override void AtualizarDados(Aluno entidadeAtualizada)
    {
        Nome = entidadeAtualizada.Nome;
        NomeResponsavel = entidadeAtualizada.NomeResponsavel;
        Telefone = entidadeAtualizada.Telefone;
    }
}
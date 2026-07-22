using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Compartilhado;

namespace _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloEstantes;

public class Estante : EntidadeBase<Estante>
{
    // Código de negócio da estante. Deve ser único no sistema.
    public string Codigo { get; set; } = string.Empty;

    // Cor escolhida pelo usuário.
    public string Cor { get; set; } = string.Empty;

    // Prazo máximo de empréstimo para livros desta estante.
    // O documento define 7 dias como valor padrão.
    public int DiasEmprestimo { get; set; } = 7;

    // Construtor vazio. Necessário para alguns cenários do ASP.NET MVC.
    public Estante()
    {
    }

    // Construtor para facilitar a criação de uma estante já preenchida.
    public Estante(string codigo, string cor, int diasEmprestimo = 7) : this()
    {
        Codigo = codigo;
        Cor = cor;
        DiasEmprestimo = diasEmprestimo;
    }

    // Responsável por validar apenas as regras da própria entidade.
    // Regras que dependem de outras entidades (como código duplicado)
    public override List<string> Validar()
    {
        List<string> erros = new();

        if (string.IsNullOrWhiteSpace(Codigo))
            erros.Add("O campo \"Código\" é obrigatório.");

        else if (Codigo.Length > 50)
            erros.Add("O campo \"Código\" deve possuir no máximo 50 caracteres.");

        if (string.IsNullOrWhiteSpace(Cor))
            erros.Add("O campo \"Cor\" é obrigatório.");

        if (DiasEmprestimo <= 0)
            erros.Add("Os dias de empréstimo devem ser maiores que zero.");

        return erros;
    }

    // Atualiza os dados editáveis da entidade.
    // O Id permanece o mesmo.
    public override void AtualizarDados(Estante entidadeAtualizada)
    {
        Codigo = entidadeAtualizada.Codigo;
        Cor = entidadeAtualizada.Cor;
        DiasEmprestimo = entidadeAtualizada.DiasEmprestimo;
    }
}
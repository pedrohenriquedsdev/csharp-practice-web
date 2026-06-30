using _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloFacilitador;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloOficina;

public class Oficina : EntidadeBase<Oficina>
{
    public string Titulo { get; set; } = string.Empty;
    public int CargaHoraria { get; set; }
    public int Vagas { get; set; }
    public decimal ValorContribuicao { get; set; }
    public Facilitador Facilitador { get; set; } = null!; // estabelece o relacionamento entre as duas entidades.

    public Oficina() { }

    public Oficina(
        string titulo,
        int cargaHoraria,
        int vagas,
        decimal valorContribuicao,
        Facilitador facilitador) : this()
    {
        Titulo = titulo;
        CargaHoraria = cargaHoraria;
        Vagas = vagas;
        ValorContribuicao = valorContribuicao;
        Facilitador = facilitador;
    }

    public override void AtualizarDados(Oficina entidadeAtualizada)
    {
        Titulo = entidadeAtualizada.Titulo;
        CargaHoraria = entidadeAtualizada.CargaHoraria;
        Vagas = entidadeAtualizada.Vagas;
        ValorContribuicao = entidadeAtualizada.ValorContribuicao;
        Facilitador = entidadeAtualizada.Facilitador;
    }

    public override List<string> Validar()
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Titulo) || Titulo.Length < 2 || Titulo.Length > 50)
            erros.Add("O campo \"Título\" deve conter entre 2 e 50 caracteres.");

        if (CargaHoraria <= 0 || CargaHoraria > 500)
            erros.Add("A carga horária deve estar entre 1 e 500 horas.");

        if (Vagas < 10 || Vagas > 100)
            erros.Add("A oficina deve conter entre 10 e 100 vagas.");

        if (ValorContribuicao < 0)
            erros.Add("O valor de contribuição não pode ser negativo.");

        if (Facilitador == null)
            erros.Add("O campo \"Facilitador\" deve ser preenchido.");

        return erros;
    }
}

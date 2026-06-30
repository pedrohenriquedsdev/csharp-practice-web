using _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloOficina;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloParticipante;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloInscricao;

public class Inscricao : EntidadeBase<Inscricao>
{
    public Oficina Oficina { get; set; } = null!;
    public Participante Participante { get; set; } = null!;
    public DateTime DataInscricao { get; set; } = DateTime.Now;
    public bool EstaCancelada { get; set; }

    public override void AtualizarDados(Inscricao entidadeAtualizada)
    {
        Oficina = entidadeAtualizada.Oficina;
        Participante = entidadeAtualizada.Participante;
        DataInscricao = entidadeAtualizada.DataInscricao;
        EstaCancelada = entidadeAtualizada.EstaCancelada;
    }

    public override List<string> Validar()
    {
        var erros = new List<string>();

        if (Oficina == null)
            erros.Add("O campo \"Oficina\" deve ser preenchido.");

        if (Participante == null)
            erros.Add("O campo \"Participante\" deve ser preenchido.");

        if (DataInscricao > DateTime.Now)
            erros.Add("A data de inscrição não pode ser futura.");

        return erros;
    }
}

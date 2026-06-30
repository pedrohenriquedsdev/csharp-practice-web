using System.Text.Json;
using System.Text.Json.Serialization;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloInscricao;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloOficina;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloParticipante;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado.Arquivos;

public sealed class ContextoJson
{
    public List<Participante> Participantes { get; set; } = new List<Participante>();
    public List<Oficina> Oficinas { get; set; } = new List<Oficina>();
    public List<Inscricao> Inscricoes { get; set; } = new List<Inscricao>();

    private readonly string caminhoArquivo;

    public ContextoJson()
    {
        string caminhoAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        string caminhoDiretorio = Path.Combine(caminhoAppData, "GestaoDeEquipamentosWeb");

        Directory.CreateDirectory(caminhoDiretorio);

        caminhoArquivo = Path.Combine(caminhoDiretorio, "dados.json");
    }

    public void Salvar()
    {
        JsonSerializerOptions opcoesJson = new JsonSerializerOptions();
        opcoesJson.WriteIndented = true;
        opcoesJson.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opcoesJson.ReferenceHandler = ReferenceHandler.Preserve;

        string jsonString = JsonSerializer.Serialize(this, opcoesJson);

        File.WriteAllText(caminhoArquivo, jsonString);
    }

    public void Carregar()
    {
        if (!File.Exists(caminhoArquivo))
            return;

        string jsonString = File.ReadAllText(caminhoArquivo);

        JsonSerializerOptions opcoesJson = new JsonSerializerOptions();
        opcoesJson.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opcoesJson.ReferenceHandler = ReferenceHandler.Preserve;

        ContextoJson? contextoSalvo = JsonSerializer.Deserialize<ContextoJson>(jsonString, opcoesJson);

        if (contextoSalvo == null)
            return;

        Participantes = contextoSalvo.Participantes;
        Oficinas = contextoSalvo.Oficinas;
        Inscricoes = contextoSalvo.Inscricoes;
    }
}
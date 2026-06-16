using System.Text.Json;
using System.Text.Json.Serialization;
using _01_WebApplication_CourseInstructorMvc.ModuloCurso;
using _01_WebApplication_CourseInstructorMvc.ModuloInstrutor;

namespace _01_WebApplication_CourseInstructorMvc.Compartilhado.Arquivos;

public sealed class ContextoJson
{
    public List<ModuloInstrutor.Instrutor> Instrutores { get; set; } = new List<ModuloInstrutor.Instrutor>();
    public List<ModuloCurso.Curso> Cursos { get; set; } = new List<ModuloCurso.Curso>();

    private readonly string caminhoArquivo;

    public ContextoJson() // setando algumas estruturas básicas de persistência. PS -> devemos criar primeiramente a entidade.
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

        // Novo objeto contextoSalvo. 
        // Copiar os dados desse objeto salvo para o contexto atual:
        ContextoJson? contextoSalvo = JsonSerializer.Deserialize<ContextoJson>(jsonString, opcoesJson);

        if (contextoSalvo == null)
            return;

        Instrutores = contextoSalvo.Instrutores;
        Cursos = contextoSalvo.Cursos;
    }
}
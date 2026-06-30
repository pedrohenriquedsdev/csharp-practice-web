using System.Security.Cryptography;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado;

public abstract class EntidadeBase<T> // essa class pode trabalhar com qualquer tipo sem precisar definir qual no momento da criação.
{
    public string Id {get; set; } = Convert
            .ToHexStringLower(RandomNumberGenerator.GetBytes(20))
            .Substring(0, 7);

    public abstract List<string> Validar();
    public abstract void AtualizarDados(T entidadeAtualizada);
}

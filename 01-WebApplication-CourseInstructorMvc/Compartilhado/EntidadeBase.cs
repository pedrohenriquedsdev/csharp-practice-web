using System.Security.Cryptography;

namespace GestaoDeEquipamentosWeb.ConsoleApp.Compartilhado;

public abstract class EntidadeBase<T>
{
    public string Id { get; set; } = Convert
            .ToHexStringLower(RandomNumberGenerator.GetBytes(20))
            .Substring(0, 7);

    public abstract List<string> Validar();
    public abstract void AtualizarDados(T entidadeAtualizada);
}
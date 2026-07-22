using System.Security.Cryptography;

namespace _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Compartilhado

{
    public abstract class EntidadeBase<T>
    {
        public string Id { get; set; } = Convert
                .ToHexStringLower(RandomNumberGenerator.GetBytes(20))
                .Substring(0, 7);

        public abstract List<string> Validar();
        public abstract void AtualizarDados(T entidadeAtualizada);
    }
}
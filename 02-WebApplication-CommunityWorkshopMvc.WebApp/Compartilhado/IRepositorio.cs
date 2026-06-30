using System;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado;

public interface IRepositorio<T> where T : EntidadeBase<T>
{
    List<T> SelecionarTodos();
    T? SelecionarPorId(Guid id);

    void Cadastrar(T entidade);
    bool Editar(Guid id, T entidadeAtualizada);
    bool Excluir(Guid id);

    List<T> Filtrar(Predicate<T> filtro);
}

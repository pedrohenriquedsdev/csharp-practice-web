using System;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado;

public interface IRepositorio<T> where T : EntidadeBase<T>
{
    List<T> SelecionarTodos();
    T? SelecionarPorId(string id);

    void Cadastrar(T entidade);
    bool Editar(string id, T entidadeAtualizada);
    bool Excluir(string id);

    List<T> Filtrar(Predicate<T> filtro);
}

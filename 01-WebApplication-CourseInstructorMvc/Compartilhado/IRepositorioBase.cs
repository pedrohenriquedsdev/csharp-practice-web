namespace _01_WebApplication_CourseInstructorMvc.Compartilhado;


public interface IRepositorio<T> where T : EntidadeBase<T>
{
    void Cadastrar(T entidade);
    bool Editar(string idSelecionado, T entidadeAtualizada);
    bool Excluir(T registro);
    T? SelecionarPorId(string idSelecionado);
    List<T> SelecionarTodos();
}
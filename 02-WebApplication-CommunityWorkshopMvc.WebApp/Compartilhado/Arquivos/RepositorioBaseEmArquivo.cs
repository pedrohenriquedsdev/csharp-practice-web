using _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado.Arquivos;

public abstract class RepositorioBaseEmArquivo<T> :
    IRepositorio<T>
    where T : EntidadeBase<T>
{
    protected ContextoJson contexto;
    protected List<T> registros;

    public RepositorioBaseEmArquivo(ContextoJson contexto)
    {
        this.contexto = contexto;
        registros = CarregarRegistros();
    }

    protected abstract List<T> CarregarRegistros();

    public void Cadastrar(T entidade)
    {
        registros.Add(entidade);

        contexto.Salvar();
    }

    public bool Editar(string id, T entidadeAtualizada)
    {
        T? registroSelecionado = SelecionarPorId(id);

        if (registroSelecionado == null)
            return false;

        registroSelecionado.AtualizarDados(entidadeAtualizada);

        contexto.Salvar();

        return true;
    }

    public bool Excluir(string id)
    {
        T? registroSelecionado = SelecionarPorId(id);

        if (registroSelecionado == null)
            return false;

        registros.Remove(registroSelecionado);

        contexto.Salvar();

        return true;
    }

    public T? SelecionarPorId(string id)
    {
        foreach (T registro in registros)
        {
            if (registro.Id == id)
                return registro;
        }

        return null;
    }

    public List<T> SelecionarTodos()
    {
        return registros;
    }

    public List<T> Filtrar(Predicate<T> filtro)
    {
        List<T> registrosFiltrados = new();

        foreach (T registro in registros)
        {
            if (filtro(registro))
                registrosFiltrados.Add(registro);
        }

        return registrosFiltrados;
    }
}
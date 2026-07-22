namespace _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Compartilhado;

public abstract class RepositorioBaseEmArquivo<T> where T : EntidadeBase<T>
{
    protected ContextoJson contexto;
    protected List<T> registros;

    public RepositorioBaseEmArquivo(ContextoJson contexto)
    {
        this.contexto = contexto;
        this.registros = CarregarRegistros();
    }

    protected abstract List<T> CarregarRegistros();

    public void Cadastrar(T entidade)
    {
        registros.Add(entidade);

        contexto.Salvar();
    }

    public bool Editar(string idSelecionado, T entidadeAtualizada)
    {
        T? registroSelecionado = SelecionarPorId(idSelecionado);

        if (registroSelecionado == null)
            return false;

        registroSelecionado.AtualizarDados(entidadeAtualizada);

        contexto.Salvar();

        return true;
    }

    public bool Excluir(T registro)
    {
        bool conseguiuExcluir = registros.Remove(registro);

        if (conseguiuExcluir)
            contexto.Salvar();

        return conseguiuExcluir;
    }

    public bool Excluir(string idSelecionado)
    {
        T? registroSelecionado = SelecionarPorId(idSelecionado);

        if (registroSelecionado == null)
            return false;

        return Excluir(registroSelecionado);
    }

    public T? SelecionarPorId(string idSelecionado)
    {
        foreach (T registro in registros)
        {
            if (registro.Id == idSelecionado)
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
        List<T> registrosFiltrados = new List<T>();

        foreach (T e in registros)
        {
            if (filtro(e))
                registrosFiltrados.Add(e);
        }

        return registrosFiltrados;
    }
}
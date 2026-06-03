using System;
using PraticasDeliberadas.WebApplication.Controllers;
using PraticasDeliberadas.WebApplication.Models.Entities;

namespace PraticasDeliberadas.WebApplication.Models.Repositories;

public class RepositorioLivro
{
    private readonly List<Livro> livros = new();
    private int contadorId = 1;

    public void Cadastrar(Livro livro)
    {
        livro.Id = contadorId++;
        livros.Add(livro);
    }

    public List<Livro> SelecionarTodos()
    {
        return livros;
    }

    public Livro? SelecionarPorId(int id)
    {
        return livros.FirstOrDefault(livro => livro.Id == id);
    }

    public void Editar(int id, Livro livroEditado)
    {
        Livro? livro = SelecionarPorId(id);

        if (livro is null)
            return;

        livro.Titulo = livroEditado.Titulo;
        livro.Autor = livroEditado.Autor;
        livro.Genero = livroEditado.Genero;
        livro.EstaDisponivel = livroEditado.EstaDisponivel;
    }

    public void Excluir(int id)
    {
        Livro? livro = SelecionarPorId(id);

        if (livro is null)
            return;

        livros.Remove(livro);
    }
}

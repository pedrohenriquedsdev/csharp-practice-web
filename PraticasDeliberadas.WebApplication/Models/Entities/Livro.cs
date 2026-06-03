using System;

namespace PraticasDeliberadas.WebApplication.Models.Entities;

public class Livro
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Autor { get; set; }
    public string Genero { get; set; }
    public bool EstaDisponivel { get; set; }

    public Livro()
    {

    }

    public Livro(string titulo, string autor, string genero)
    {
        Titulo = titulo;
        Autor = autor;
        Genero = genero;
        EstaDisponivel = true;
    }
}

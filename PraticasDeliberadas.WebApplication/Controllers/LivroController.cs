using Microsoft.AspNetCore.Mvc;
using PraticasDeliberadas.WebApplication.Models.Entities;
using PraticasDeliberadas.WebApplication.Models.Repositories;

namespace PraticasDeliberadas.WebApplication.Controllers
{
    public class LivroController : Controller
    {
        private readonly RepositorioLivro repositorioLivro; // atributo para que Controller use métodos de rep

        public LivroController()
        {
            repositorioLivro = new RepositorioLivro(); // instânciando dentro da prop class
        }

        public IActionResult Index()
        {
            List<Livro> livros = repositorioLivro.SelecionarTodos();

            return View(livros);
        }

        public IActionResult Detalhes(int id)
        {
            Livro? livro = repositorioLivro.SelecionarPorId(id);

            if (livro is null)
                return NotFound();

            return View(livro);
        }

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(Livro livro)
        {
            repositorioLivro.Cadastrar(livro);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            Livro? livro = repositorioLivro.SelecionarPorId(id);

            if (livro is null)
                return NotFound();

            return View(livro);
        }

        [HttpPost]
        public IActionResult Editar(int id, Livro livroEditado)
        {
            repositorioLivro.Editar(id, livroEditado);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Excluir(int id)
        {
            Livro? livro = repositorioLivro.SelecionarPorId(id);

            if (livro is null)
                return NotFound();

            return View(livro);
        }

        [HttpPost]
        public IActionResult ConfirmarExclusao(int id)
        {
            repositorioLivro.Excluir(id);

            return RedirectToAction(nameof(Index));
        }
    }
}

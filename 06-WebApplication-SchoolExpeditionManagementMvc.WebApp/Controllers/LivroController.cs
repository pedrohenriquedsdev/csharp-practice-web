using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Compartilhado;
using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloEstante;
using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloLivro;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Controllers;

public class LivroController : Controller
{
    private readonly IRepositorioLivro repositorioLivro;
    private readonly IRepositorioEstante repositorioEstante;

    public LivroController()
    {
        ContextoJson contexto = new ContextoJson();
        contexto.Carregar();

        repositorioLivro = new RepositorioLivroEmArquivo(contexto);
        repositorioEstante = new RepositorioEstanteEmArquivo(contexto);
    }

    [HttpGet]
    public ActionResult Listar()
    {
        List<Livro> livros = repositorioLivro.SelecionarTodos();

        List<ListarLivroViewModel> listarVms = new List<ListarLivroViewModel>();

        foreach (Livro l in livros)
        {
            ListarLivroViewModel viewModel = new ListarLivroViewModel(
                l.Id,
                l.Titulo,
                l.NumeroEdicao,
                l.DataPublicacao,
                l.Estante.Codigo,
                l.Status
            );

            listarVms.Add(viewModel);
        }

        return View(listarVms);
    }

    [HttpGet]
    public ActionResult Cadastrar()
    {
        ViewBag.Estantes = CarregarEstantes();

        CadastrarLivroViewModel cadastrarVm = new CadastrarLivroViewModel(
            string.Empty,
            1,
            DateTime.Today,
            string.Empty
        );

        return View(cadastrarVm);
    }

    [HttpPost]
    public ActionResult Cadastrar(CadastrarLivroViewModel cadastrarVm)
    {
        Estante? estante =
            repositorioEstante.SelecionarPorId(cadastrarVm.EstanteId);

        if (!string.IsNullOrWhiteSpace(cadastrarVm.EstanteId) && estante == null)
        {
            ModelState.AddModelError(
                nameof(cadastrarVm.EstanteId),
                "Selecione uma estante válida."
            );
        }

        Livro? livroExistente = repositorioLivro
            .Filtrar(l =>
                l.Titulo == cadastrarVm.Titulo &&
                l.NumeroEdicao == cadastrarVm.NumeroEdicao
            )
            .FirstOrDefault();

        if (livroExistente != null)
        {
            ModelState.AddModelError(
                nameof(cadastrarVm.Titulo),
                "Já existe um livro com este título e número de edição."
            );
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Estantes = CarregarEstantes();

            return View(cadastrarVm);
        }

        Livro novoLivro = new Livro(
            cadastrarVm.Titulo,
            cadastrarVm.NumeroEdicao,
            cadastrarVm.DataPublicacao,
            estante!
        );

        repositorioLivro.Cadastrar(novoLivro);

        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult Editar(string id)
    {
        Livro? livro = repositorioLivro.SelecionarPorId(id);

        if (livro == null)
            return RedirectToAction(nameof(Listar));

        ViewBag.Estantes = CarregarEstantes();

        EditarLivroViewModel editarVm = new EditarLivroViewModel(
            livro.Id,
            livro.Titulo,
            livro.NumeroEdicao,
            livro.DataPublicacao,
            livro.Estante.Id
        );

        return View(editarVm);
    }

    [HttpPost]
    public ActionResult Editar(EditarLivroViewModel editarVm)
    {
        Estante? estante =
            repositorioEstante.SelecionarPorId(editarVm.EstanteId);

        if (!string.IsNullOrWhiteSpace(editarVm.EstanteId) && estante == null)
        {
            ModelState.AddModelError(
                nameof(editarVm.EstanteId),
                "Selecione uma estante válida."
            );
        }

        Livro? livroExistente = repositorioLivro
            .Filtrar(l =>
                l.Titulo == editarVm.Titulo &&
                l.NumeroEdicao == editarVm.NumeroEdicao &&
                l.Id != editarVm.Id
            )
            .FirstOrDefault();

        if (livroExistente != null)
        {
            ModelState.AddModelError(
                nameof(editarVm.Titulo),
                "Já existe um livro com este título e número de edição."
            );
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Estantes = CarregarEstantes();

            return View(editarVm);
        }

        Livro livroAtualizado = new Livro(
            editarVm.Titulo,
            editarVm.NumeroEdicao,
            editarVm.DataPublicacao,
            estante!
        );

        repositorioLivro.Editar(editarVm.Id, livroAtualizado);

        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult Excluir(string id)
    {
        Livro? livro = repositorioLivro.SelecionarPorId(id);

        if (livro == null)
            return RedirectToAction(nameof(Listar));

        ExcluirLivroViewModel excluirVm = new ExcluirLivroViewModel(
            livro.Id,
            livro.Titulo,
            livro.NumeroEdicao,
            livro.DataPublicacao,
            livro.Estante.Codigo,
            livro.Status
        );

        return View(excluirVm);
    }

    [HttpPost]
    [ActionName("Excluir")]
    public ActionResult ExcluirConfirmado(ExcluirLivroViewModel excluirVm)
    {
        Livro? livro = repositorioLivro.SelecionarPorId(excluirVm.Id);

        if (livro != null)
            repositorioLivro.Excluir(livro);

        return RedirectToAction(nameof(Listar));
    }

    private List<SelectListItem> CarregarEstantes()
    {
        List<Estante> estantes = repositorioEstante.SelecionarTodos();

        List<SelectListItem> selecionarEstantes = new List<SelectListItem>();

        foreach (Estante e in estantes)
        {
            SelectListItem selecionarEstanteVm = new SelectListItem(
                e.Codigo,
                e.Id
            );

            selecionarEstantes.Add(selecionarEstanteVm);
        }

        return selecionarEstantes;
    }
}
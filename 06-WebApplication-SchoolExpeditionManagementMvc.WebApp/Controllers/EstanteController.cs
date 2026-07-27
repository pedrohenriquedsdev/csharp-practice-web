using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Compartilhado;
using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloEstante;
using Microsoft.AspNetCore.Mvc;

namespace _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Controllers;

public class EstanteController : Controller
{
    private readonly IRepositorioEstante repositorioEstante;
    public EstanteController()
    {
        ContextoJson contexto = new ContextoJson();
        contexto.Carregar();

        repositorioEstante = new RepositorioEstanteEmArquivo(contexto);
    }

    [HttpGet]
    public ActionResult Listar()
    {
        List<Estante> estantes = repositorioEstante.SelecionarTodos();

        List<ListarEstanteViewModel> listarVms = new List<ListarEstanteViewModel>();

        foreach (Estante e in estantes)
        {
            // mapear objeto por objeto para viewModels
            ListarEstanteViewModel viewModel = new ListarEstanteViewModel(
                e.Id,
                e.Codigo,
                e.Cor,
                e.DiasEmprestimo
            );

            listarVms.Add(viewModel);
        }

        return View(listarVms);
    }

    [HttpGet]
    public ActionResult Cadastrar()
    {
        CadastrarEstanteViewModel cadastrarVm = new CadastrarEstanteViewModel(
            string.Empty,
            string.Empty,
            7
        );

        return View(cadastrarVm);
    }

    [HttpPost]
    public ActionResult Cadastrar(CadastrarEstanteViewModel cadastrarVm)
    {
        Estante? estanteExistente = repositorioEstante
            .Filtrar(e => e.Codigo == cadastrarVm.Codigo)
            .FirstOrDefault();

        if (estanteExistente != null)
        {
            ModelState.AddModelError(
                nameof(cadastrarVm.Codigo),
                "Já existe uma estante com este código."
            );
        }

        if (!ModelState.IsValid)
            return View(cadastrarVm);

        Estante novaEstante = new Estante(
            cadastrarVm.Codigo,
            cadastrarVm.Cor,
            cadastrarVm.DiasEmprestimo
        );

        repositorioEstante.Cadastrar(novaEstante);

        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult Editar(string id)
    {
        Estante? estante = repositorioEstante.SelecionarPorId(id);

        if (estante == null)
            return RedirectToAction(nameof(Listar));

        EditarEstanteViewModel editarVm = new EditarEstanteViewModel(
            id,
            estante.Codigo,
            estante.Cor,
            estante.DiasEmprestimo
        );

        return View(editarVm);
    }

    [HttpPost]
    public ActionResult Editar(EditarEstanteViewModel editarVm)
    {
        if (!ModelState.IsValid)
            return View(editarVm);

        Estante estanteAtualizada = new Estante(
            editarVm.Codigo,
            editarVm.Cor,
            editarVm.DiasEmprestimo
        );

        repositorioEstante.Editar(editarVm.Id, estanteAtualizada);

        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult Excluir(string id)
    {
        Estante? estante = repositorioEstante.SelecionarPorId(id);

        if (estante == null)
            return RedirectToAction(nameof(Listar));

        ExcluirEstanteViewModel excluirVm = new ExcluirEstanteViewModel(
            id,
            estante.Codigo,
            estante.Cor,
            estante.DiasEmprestimo
        );

        return View(excluirVm);
    }

    [HttpPost]
    [ActionName("Excluir")]
    public ActionResult ExcluirConfirmado(ExcluirEstanteViewModel excluirVm)
    {
        Estante? estante = repositorioEstante.SelecionarPorId(excluirVm.Id);

        if (estante == null)
            return RedirectToAction(nameof(Listar));

        repositorioEstante.Excluir(estante);

        return RedirectToAction(nameof(Listar));
    }
}
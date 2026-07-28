using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Compartilhado;
using _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.ModuloAluno;
using Microsoft.AspNetCore.Mvc;

namespace _06_WebApplication_SchoolExpeditionManagementMvc.WebApp.Controllers;

public class AlunoController : Controller
{
    private readonly IRepositorioAluno repositorioAluno;

    public AlunoController()
    {
        ContextoJson contexto = new ContextoJson();
        contexto.Carregar();

        repositorioAluno = new RepositorioAlunoEmArquivo(contexto);
    }

    [HttpGet]
    public ActionResult Listar()
    {
        List<Aluno> alunos = repositorioAluno.SelecionarTodos();

        List<ListarAlunoViewModel> listarVms = new List<ListarAlunoViewModel>();

        foreach (Aluno a in alunos)
        {
            ListarAlunoViewModel viewModel = new ListarAlunoViewModel(
                a.Id,
                a.Nome,
                a.NomeResponsavel,
                a.Telefone
            );

            listarVms.Add(viewModel);
        }

        return View(listarVms);
    }

    [HttpGet]
    public ActionResult Cadastrar()
    {
        CadastrarAlunoViewModel cadastrarVm = new CadastrarAlunoViewModel(
            string.Empty,
            string.Empty,
            string.Empty
        );

        return View(cadastrarVm);
    }

    [HttpPost]
    public ActionResult Cadastrar(CadastrarAlunoViewModel cadastrarVm)
    {
        Aluno? alunoExistente = repositorioAluno
            .Filtrar(a =>
                a.Nome == cadastrarVm.Nome &&
                a.Telefone == cadastrarVm.Telefone)
            .FirstOrDefault();

        if (alunoExistente != null)
        {
            ModelState.AddModelError(
                nameof(cadastrarVm.Telefone),
                "Já existe um aluno com este nome e telefone."
            );
        }

        if (!ModelState.IsValid)
            return View(cadastrarVm);

        Aluno novoAluno = new Aluno(
            cadastrarVm.Nome,
            cadastrarVm.NomeResponsavel,
            cadastrarVm.Telefone
        );

        repositorioAluno.Cadastrar(novoAluno);

        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult Editar(string id)
    {
        Aluno? aluno = repositorioAluno.SelecionarPorId(id);

        if (aluno == null)
            return RedirectToAction(nameof(Listar));

        EditarAlunoViewModel editarVm = new EditarAlunoViewModel(
            id,
            aluno.Nome,
            aluno.NomeResponsavel,
            aluno.Telefone
        );

        return View(editarVm);
    }

    [HttpPost]
    public ActionResult Editar(EditarAlunoViewModel editarVm)
    {
        if (!ModelState.IsValid)
            return View(editarVm);

        Aluno alunoAtualizado = new Aluno(
            editarVm.Nome,
            editarVm.NomeResponsavel,
            editarVm.Telefone
        );

        repositorioAluno.Editar(editarVm.Id, alunoAtualizado);

        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult Excluir(string id)
    {
        Aluno? aluno = repositorioAluno.SelecionarPorId(id);

        if (aluno == null)
            return RedirectToAction(nameof(Listar));

        ExcluirAlunoViewModel excluirVm = new ExcluirAlunoViewModel(
            id,
            aluno.Nome,
            aluno.NomeResponsavel,
            aluno.Telefone
        );

        return View(excluirVm);
    }

    [HttpPost]
    [ActionName("Excluir")]
    public ActionResult ExcluirConfirmado(ExcluirAlunoViewModel excluirVm)
    {
        Aluno? aluno = repositorioAluno.SelecionarPorId(excluirVm.Id);

        if (aluno == null)
            return RedirectToAction(nameof(Listar));

        repositorioAluno.Excluir(aluno);

        return RedirectToAction(nameof(Listar));
    }
}
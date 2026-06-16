using _01_WebApplication_CourseInstructorMvc.Compartilhado;
using _01_WebApplication_CourseInstructorMvc.Compartilhado.Arquivos;
using _01_WebApplication_CourseInstructorMvc.Models;
using _01_WebApplication_CourseInstructorMvc.ModuloCurso;
using _01_WebApplication_CourseInstructorMvc.ModuloInstrutor;
using Microsoft.AspNetCore.Mvc;

namespace _01_WebApplication_CourseInstructorMvc.Controllers
{
    public class CursoController : Controller
    {
        private readonly IRepositorio<Curso> repositorioCurso;
        private readonly IRepositorio<Instrutor> repositorioInstrutor;

        public CursoController()
        {
            ContextoJson contexto = new ContextoJson();
            contexto.Carregar();

            repositorioCurso = new RepositorioCursoEmArquivo(contexto);
            repositorioInstrutor = new RepositorioInstrutorEmArquivo(contexto);
        }
        public ActionResult Listar()
        {
            List<Curso> cursos = repositorioCurso.SelecionarTodos();

            List<ListarCursosViewModel> listarVms = new List<ListarCursosViewModel>();

            foreach (Curso c in cursos)
            {
                ListarCursosViewModel viewModel = new ListarCursosViewModel(
                    c.Id,
                    c.Nome,
                    c.Valor,
                    c.DataInicio,
                   c.Instrutor?.Nome ?? "Sem instrutor" //isso vem como string, por isso definimos como str no VM
                );

                listarVms.Add(viewModel);
            }

            return View(listarVms);
        }

        [HttpGet]
        public ActionResult Cadastrar()
        {
            ViewBag.Instrutores = CarregarInstrutores();

            return View();
        }

        [HttpPost]
        public ActionResult Cadastrar(CadastrarCursoViewModel cadastrarVm)
        {
            Instrutor? instrutor = repositorioInstrutor.SelecionarPorId(cadastrarVm.InstrutorId);

            if (instrutor == null)
                return RedirectToAction(nameof(Listar));

            Curso novoCurso = new Curso(
                cadastrarVm.Nome,
                cadastrarVm.Valor,
                cadastrarVm.DataInicio,
                instrutor
            );

            repositorioCurso.Cadastrar(novoCurso);

            return RedirectToAction(nameof(Listar));
        }

        [HttpGet]
        public ActionResult Editar(string id)
        {
            Curso? curso = repositorioCurso.SelecionarPorId(id);

            if (curso == null)
                return RedirectToAction(nameof(Listar));

            EditarCursoViewModel editarVm = new EditarCursoViewModel(
                id,
                curso.Nome,
                curso.Valor,
                curso.DataInicio,
                curso.Instrutor.Id
            );

            ViewBag.Instrutores = CarregarInstrutores();

            return View(editarVm);
        }

        [HttpPost]
        public ActionResult Editar(EditarCursoViewModel editarVm)
        {
            Instrutor? instrutor = repositorioInstrutor.SelecionarPorId(editarVm.InstrutorId);

            if (instrutor == null)
                return RedirectToAction(nameof(Listar));

            Curso cursoAtualizado = new Curso(
                editarVm.Nome,
                editarVm.Valor,
                editarVm.DataInicio,
                instrutor
            );

            repositorioCurso.Editar(editarVm.Id, cursoAtualizado);

            return RedirectToAction(nameof(Listar));
        }

        [HttpGet]
        public ActionResult Excluir(string id)
        {
            Curso? curso = repositorioCurso.SelecionarPorId(id);

            if (curso == null)
                return RedirectToAction(nameof(Listar));

            ExcluirCursoViewModel excluirVm = new ExcluirCursoViewModel(
                id,
                curso.Nome,
                curso.Valor,
                curso.DataInicio,
                curso.Instrutor.Nome
            );

            return View(excluirVm);
        }

        [HttpPost]
        [ActionName("Excluir")]
        public ActionResult ExcluirConfirmado(ExcluirCursoViewModel excluirVm)
        {
            Curso? curso = repositorioCurso.SelecionarPorId(excluirVm.Id);

            if (curso != null)
                repositorioCurso.Excluir(curso);

            return RedirectToAction(nameof(Listar));
        }

        // Busca os instrutores cadastrados e converte cada entidade em uma ViewModel para uso na View.
        private List<ListarInstrutoresViewModel> CarregarInstrutores()
        {
            List<Instrutor> instrutores = repositorioInstrutor.SelecionarTodos();

            List<ListarInstrutoresViewModel> listarVms = new List<ListarInstrutoresViewModel>();

            foreach (Instrutor i in instrutores)
            {
                // mapear objeto por objeto para viewModels
                ListarInstrutoresViewModel viewModel = new ListarInstrutoresViewModel(
                    i.Id,
                    i.Nome,
                    i.Email,
                    i.Telefone
                );

                listarVms.Add(viewModel);
            }

            return listarVms;
        }
    }
}

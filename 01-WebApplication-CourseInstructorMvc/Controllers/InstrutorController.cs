using _01_WebApplication_CourseInstructorMvc.Compartilhado;
using _01_WebApplication_CourseInstructorMvc.Compartilhado.Arquivos;
using _01_WebApplication_CourseInstructorMvc.Models;
using _01_WebApplication_CourseInstructorMvc.ModuloInstrutor;
using Microsoft.AspNetCore.Mvc;

namespace _01_WebApplication_CourseInstructorMvc.Controllers
{
    public class InstrutorController : Controller
    {
        // 
        private readonly IRepositorio<Instrutor> repositorioInstrutor;

        public InstrutorController()
        {
            ContextoJson contexto = new ContextoJson();

            contexto.Carregar();

            repositorioInstrutor = new RepositorioInstrutorEmArquivo(contexto);
        }

        [HttpGet]
        public ActionResult Listar()
        {
            List<Instrutor> instrutores = repositorioInstrutor.SelecionarTodos();

            List<ListarInstrutoresViewModel> listarVms = new List<ListarInstrutoresViewModel>();

            foreach (Instrutor i in instrutores)
            {
                ListarInstrutoresViewModel viewModel = new ListarInstrutoresViewModel(
                    i.Id,
                    i.Nome,
                    i.Email,
                    i.Telefone
                );

                listarVms.Add(viewModel);
            }

            return View(listarVms);
        }

        // CADASTRAR
        [HttpGet]
        public ActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Cadastrar(CadastrarInstrutorViewModel cadastrarVm)
        {
            Instrutor novoInstrutor = new Instrutor(
            cadastrarVm.Nome,
            cadastrarVm.Email,
            cadastrarVm.Telefone
        );

            repositorioInstrutor.Cadastrar(novoInstrutor);

            return RedirectToAction(nameof(Listar));
        }

        // EDITAR
        [HttpGet]
        public ActionResult Editar(string id)
        {
            Instrutor? instrutor = repositorioInstrutor.SelecionarPorId(id);

            if (instrutor == null)
                return RedirectToAction(nameof(Listar));

            EditarInstrutorViewModel editarVm = new EditarInstrutorViewModel(
                id,
                instrutor.Nome,
                instrutor.Email,
                instrutor.Telefone
            );

            return View(editarVm);
        }

        [HttpPost]
        public ActionResult Editar(EditarInstrutorViewModel editarVm)
        {
            Instrutor instrutorAtualizado = new Instrutor(
                editarVm.Nome,
                editarVm.Email,
                editarVm.Telefone
            );

            repositorioInstrutor.Editar(editarVm.Id, instrutorAtualizado);

            return RedirectToAction(nameof(Listar));
        }

        // EXCLUSÃO
        [HttpGet]
        public ActionResult Excluir(string id)
        {
            Instrutor? instrutor = repositorioInstrutor.SelecionarPorId(id);

            if (instrutor == null)
                return RedirectToAction(nameof(Listar));

            ExcluirInstrutorViewModel excluirVm = new ExcluirInstrutorViewModel(
                id,
                instrutor.Nome,
                instrutor.Email,
                instrutor.Telefone
            );

            return View(excluirVm);
        }

        [HttpPost]
        [ActionName("Excluir")]
        public ActionResult ExcluirConfirmado(ExcluirInstrutorViewModel excluirVm)
        {
            Instrutor? instrutor = repositorioInstrutor.SelecionarPorId(excluirVm.Id);

            if (instrutor == null)
                return RedirectToAction(nameof(Listar));

            repositorioInstrutor.Excluir(instrutor);

            return RedirectToAction(nameof(Listar));
        }
    }
}

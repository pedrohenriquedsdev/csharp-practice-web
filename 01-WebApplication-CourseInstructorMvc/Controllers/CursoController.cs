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
                    c.Instrutor.Nome //isso vem como string, por isso definimos como str no VM
                );

                listarVms.Add(viewModel);
            }

            return View(listarVms);
        }

    }
}

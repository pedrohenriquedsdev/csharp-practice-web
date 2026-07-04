# Guia prático — projeto paralelo ASP.NET Core MVC

Este material foi escrito a partir da análise do repositório **Gestão de Equipamentos Web**. A ideia não é decorar uma sequência de arquivos: é conseguir prever o caminho que os dados percorrem e saber por que cada peça existe.

## 0. Diagnóstico do projeto analisado

### Qual versão é a mais completa?

A base recomendada é a branch **`v5`**, no commit `f584077` (20/05/2026). Ela é a versão funcionalmente mais completa e a que está selecionada neste diretório. A conclusão não foi tomada pelo número da versão: foi feita pela comparação do histórico e do conteúdo das branches.

| Marco | Ganho principal |
|---|---|
| `v1` | Home e CRUD de fabricantes |
| `v2` | Layout e estilo |
| `v3` | ViewModels e CRUD de equipamentos |
| `v4` | CRUD de chamados, DataAnnotations, ModelState e Tag Helpers |
| `v5` | status de chamado, filtro, delegate `Predicate<T>` e lambdas |
| `refatoração-organizacao` | branch posterior (22/05), com reorganização experimental, mas sem as adições da v5 |

---

### Papel de cada área da v5

| Área | Responsabilidade | Exemplos |
|---|---|---|
| `Controllers` | Orquestra HTTP, decide view/redirect, mapeia dados | `EquipamentoController` |
| `Models` | Contratos próprios de cada tela e validação de entrada | `CadastrarEquipamentoViewModel` |
| `Modulo...` | Regras e dados do negócio por assunto | `Fabricante`, `Chamado` |
| `Compartilhado` | Código reutilizável | `EntidadeBase<T>`, `IRepositorio<T>` |
| `Compartilhado/Arquivos` | Infraestrutura de persistência | `ContextoJson`, repositório base |
| `Views` | HTML Razor que apresenta o ViewModel | `Views/Chamado/Listar.cshtml` |
| `wwwroot` | Arquivos estáticos servidos diretamente | `css/styles.css` |
| `Program.cs` | Composição do aplicativo e pipeline HTTP | MVC, middlewares e rota |

O projeto atual separa razoavelmente interface, domínio e persistência, mas ainda mistura a composição de objetos nos controllers. No paralelo, evolua para uma organização por módulo e DI — sem transformar o estudo em uma catedral de pastas antes da hora.

---

## 1. Proposta do projeto paralelo: Gestão de Oficinas Criativas Comunitárias

### Por que este tema

Um centro comunitário oferece oficinas de fotografia, cerâmica, teatro e jardinagem. O sistema organiza quem facilita cada oficina, quem participa e o estado de cada inscrição. É um contexto diferente do projeto original, mas preserva relações reais, formulários dependentes e um estado que muda — o terreno ideal para reaprender o MVC sem apenas trocar nomes.

### Entidades e relações

```text
Facilitador 1 ───── * Oficina 1 ───── * Inscricao * ───── 1 Participante
                       │                  │
                       │                  └─ status: ativa/cancelada, data de inscrição
                       └─ título, carga horária, vagas, valor de contribuição
```

| Entidade | Campos essenciais | Papel no treino |
|---|---|---|
| `Facilitador` | Id, Nome, Email, Especialidade | cadastro pai; CRUD simples |
| `Oficina` | Id, Titulo, CargaHoraria, Vagas, ValorContribuicao, Facilitador | filho de facilitador; dropdown |
| `Participante` | Id, Nome, Email, Telefone | outro CRUD simples e pai de inscrição |
| `Inscricao` | Id, Oficina, Participante, DataInscricao, EstaCancelada | duas relações, cálculo de dias, status e filtro |

Para espelhar exatamente a progressão da v5, trate uma oficina como uma turma. Depois, como desafio, crie `EdicaoOficina` para separar o tema da oficina de suas turmas em datas diferentes.

### Funcionalidades e telas

1. Início com links para todos os módulos.
2. Facilitadores: listar, cadastrar, editar, confirmar exclusão.
3. Participantes: o mesmo CRUD, implementado depois sem copiar sem entender.
4. Oficinas: CRUD; ao cadastrar/editar, selecionar facilitador.
5. Inscrições: CRUD; selecionar oficina e participante, registrar uma observação opcional e cancelar/reativar a inscrição.
6. Listagem de inscrições com filtros Todas, Ativas e Canceladas.
7. Regra: não criar inscrição ativa repetida para o mesmo participante na mesma oficina; não ultrapassar o número de vagas. Use `Any` e `Count` como evolução LINQ.
8. Regra: não excluir facilitador se houver oficinas; não excluir oficina se houver inscrições. Comece exibindo erro amigável no `ModelState`/mensagem e só depois decida se quer cascata.

---

## 2. Roadmap de implementação

Faça uma etapa por vez, com uma pequena demonstração manual no fim. Só avance quando conseguir explicar o fluxo em voz alta.

## ~~Etapa 0 — Preparação e rota~~

**Objetivo:** ~~criar solução MVC, `Program.cs`, layout, página inicial e links de navegação.~~ **Pratica:** ~~pipeline, `AddControllersWithViews`, `UseStaticFiles`, `UseRouting`, rota padrão, Razor e layout.~~ **Resultado esperado:** ~~`/` abre Home e os links geram URLs pelo Tag Helper.~~

## ~~Etapa 1 — Domínio e contratos~~

**Objetivo:** ~~criar `EntidadeBase<T>`, `Facilitador`, `Oficina`, `Participante`, `Inscricao` e seus métodos `AtualizarDados`.~~ **Pratica:** ~~POO, construtores, propriedades, composição e relações.~~ **Resultado esperado:** ~~o código representa o negócio sem depender de controller/view.~~

### Etapa 2 — Persistência JSON e repositórios

**Objetivo:** criar `ContextoJson`, `IRepositorio<T>`, `RepositorioBaseEmArquivo<T>` e repositórios concretos. **Pratica:** interfaces, genéricos, herança, serialização e persistência. **Resultado esperado:** cadastrar, editar e excluir em uma lista salva no JSON.

### Etapa 3 — CRUD vertical de Facilitador

**Objetivo:** entregar uma fatia completa simples antes de multiplicar módulos. **Pratica:** controller, actions GET/POST, ViewModels, views Razor, mapeamento, DataAnnotations e PRG. **Resultado esperado:** Facilitador funciona de ponta a ponta, incluindo erros de formulário.

### Etapa 4 — CRUD de Participante sem muleta

**Objetivo:** repetir o padrão conscientemente. **Pratica:** mesmas peças com menos consulta ao código anterior. **Resultado esperado:** você consegue dizer por que existem duas actions `Cadastrar`.

### Etapa 5 — Oficina e primeiro cadastro dependente

**Objetivo:** cadastrar oficina selecionando facilitador. **Pratica:** `SelectListItem`, `asp-items`, `FacilitadorId`, `ModelState.AddModelError`, repopulação do select em erro. **Resultado esperado:** ao submeter sem facilitador ou com ID inválido, a tela retorna com opções e mensagem; com ID válido, a oficina referencia o facilitador.

### Etapa 6 — Inscrição com duas dependências

**Objetivo:** selecionar oficina e participante e controlar cancelamento. **Pratica:** duas consultas por ID, checkbox, propriedade calculada e mapping. **Resultado esperado:** listagem mostra oficina, participante, data, dias desde a inscrição e status.

### Etapa 7 — Filtro, delegate, método anônimo, lambda e LINQ

**Objetivo:** filtrar inscrições ativas/canceladas em uma única listagem. **Pratica:** `Predicate<T>` ou `Func<T,bool>`, método anônimo, lambda, query string, `asp-route-status`, `Where`, `OrderByDescending`. **Resultado esperado:** URLs diferentes mantêm filtro e estado visual do link ativo.

### Etapa 8 — DI e organização em camadas/módulos

**Objetivo:** retirar `new Repositorio...` dos controllers. **Pratica:** container, `AddScoped`, injeção por construtor, interfaces e composição. **Resultado esperado:** controller recebe `IRepositorio<Oficina>` e `IRepositorio<Facilitador>`; trocar JSON por fake não muda a action.

### Etapa 9 — Integridade, validações de domínio e refatoração

**Objetivo:** tornar o comportamento seguro e eliminar repetição justificadamente. **Pratica:** validação de negócio, `Any`, ViewModel de formulário, métodos de mapeamento e tratamento de exclusão dependente. **Resultado esperado:** nenhuma relação fica órfã e validações têm uma casa clara.

---

## 8. Checklist final — só marque quando conseguir explicar

- [ ] Sei explicar URL → rota → controller → action → view.
- [ ] Sei diferenciar GET que mostra formulário de POST que altera estado.
- [ ] Sei explicar por que uso PRG após POST válido.
- [ ] Consigo criar uma entidade sem colocar HTML, `ViewBag` ou `ModelState` nela.
- [ ] Sei justificar cada ViewModel e mapear entidade ↔ ViewModel sem “mágica”.
- [ ] Sei dizer o que model binding faz e o que ele não valida sozinho.
- [ ] Sei dizer como DataAnnotations chegam ao `ModelState` e por que retorno a mesma view quando inválido.
- [ ] Sei criar Tag Helpers para campo, erro, link, rota e select.
- [ ] Sei repopular dropdown ao retornar formulário inválido.
- [ ] Sei modelar e validar `Facilitador → Oficina → Inscricao → Participante`.
- [ ] Sei explicar interface, classe genérica e herança no repositório.
- [ ] Sei localizar o arquivo JSON, explicar serialização e o limite dessa persistência.
- [ ] Sei explicar `Predicate<T>`, escrever uma lambda e reescrevê-la como método anônimo.
- [ ] Sei usar `Where`, `Select`, `Any`, ordenação e paginação básica com LINQ.
- [ ] Sei explicar por que DI melhora o controller e registrar uma interface com ciclo de vida `Scoped`.
- [ ] Sei impedir exclusão que viola relações e explicar a política escolhida.
- [ ] Consigo adicionar um CRUD novo repetindo o fluxo inteiro, não apenas copiando arquivos.
- [ ] Consigo apontar onde uma regra pertence: view, ViewModel, controller, domínio, repositório ou infraestrutura.

Quando todos os itens fizerem sentido, você não terá apenas “feito um sistema de oficinas”. Terá construído o mapa mental de uma aplicação MVC pequena, porém inteira — exatamente o tipo de base que torna os próximos projetos menos misteriosos.

# Guia prático — projeto paralelo ASP.NET Core MVC

Este material foi escrito a partir da análise do projeto **ControleDeMedicamentosWeb.WebApp**. O objetivo não é copiar o sistema original, mas reconstruir o mesmo raciocínio em outro contexto, entendendo como MVC, serviços, validações, repositórios, JSON e views trabalham juntos.

O projeto paralelo proposto será uma **Gestão de Insumos de Ateliê Criativo Comunitário**. A aplicação controlará materiais usados em oficinas de arte, costura, marcenaria leve, pintura e atividades manuais: tintas, pincéis, tecidos, papéis, cola, ferramentas consumíveis e kits criativos.

Importante: este guia segue o recorte pedido — **somente persistência com JSON**. Tudo que envolve banco de dados, SQL Server, Dapper, connection string, projeto `.sqlproj` ou infraestrutura SQL deve ser ignorado para este estudo.

---

## 0. Diagnóstico do projeto analisado

### Qual versão é a mais completa?

O repositório possui versões até `v9/main`, mas a linha mais recente inclui integrações com banco de dados e logging. Como o objetivo deste guia é estudar **somente com JSON**, a base prática recomendada é a branch **`origin/v5`**, commit `5efcae1`.

A `v5` é a versão completa antes da entrada da infraestrutura SQL. Ela contém os módulos principais, camada de aplicação, AutoMapper, FluentResults, DI, organização modular, views Razor, Bootstrap e repositórios em arquivo.

| Marco | Ganho principal |
|---|---|
| `v0` | projeto inicial e README |
| `v1` | módulo de fornecedores |
| `v2` | módulo de pacientes |
| `v3` | módulo de medicamentos |
| `v4` | módulo de funcionários |
| `v5` | módulo de estoque com entrada, saída e histórico usando JSON |
| `v6+` | evolução para SQL/infra externa, fora do escopo deste guia |

Então, a leitura correta é:

```text
Versão mais nova do repositório: v9/main
Versão mais completa para estudo com JSON: v5
```

### O que a v5 entrega

- Organização modular em `Modulos/ModuloFornecedor`, `ModuloPaciente`, `ModuloFuncionario`, `ModuloMedicamento` e `ModuloEstoque`.
- Camadas por módulo: `Dominio`, `Infra`, `Aplicacao` e `Apresentacao`.
- CRUD completo de fornecedores, pacientes, funcionários e medicamentos.
- Registro de entrada de estoque.
- Registro de saída de estoque.
- Histórico de estoque por medicamento.
- Quantidade em estoque calculada pelo histórico de requisições.
- Relacionamentos entre fornecedor, medicamento, funcionário, paciente e requisições.
- Repositórios em arquivo usando `ContextoJson`.
- Repositório genérico com `IRepositorio<T>` e `RepositorioBaseEmArquivo<T>`.
- Persistência JSON com preservação de referência e suporte a polimorfismo.
- `Guid` como identificador das entidades.
- Injeção de dependência com `AddScoped`.
- Camada de aplicação com serviços.
- DTOs entre Controller e Serviço.
- `FluentResults` para sucesso/falha.
- `ModelStateExtensions` e `TempDataExtensions`.
- AutoMapper para ViewModel ↔ DTO.
- Views Razor com Bootstrap e Tag Helpers.

### O conceito mais importante deste projeto

No projeto, a quantidade em estoque não é simplesmente um número editado no formulário do medicamento. Ela é calculada a partir das movimentações:

```text
Estoque atual =
  soma das entradas
  -
  soma das saídas
```

Esse é um salto importante em relação a CRUD puro. Agora você não apenas cadastra dados: você registra eventos de negócio e calcula o estado atual a partir deles.

---

## 1. Visão geral da arquitetura

### Organização modular

O projeto organiza cada assunto em um módulo:

```text
Modulos
  ├── ModuloFornecedor
  ├── ModuloPaciente
  ├── ModuloFuncionario
  ├── ModuloMedicamento
  └── ModuloEstoque
```

Cada módulo pode ter:

```text
Dominio       → entidades e interfaces do negócio
Infra         → repositório em arquivo
Aplicacao     → serviços e DTOs
Apresentacao  → controller, views, viewmodels e profiles
```

Essa organização é útil porque cada módulo carrega uma fatia inteira da aplicação. Se você vai mexer em estoque, por exemplo, quase tudo que importa está dentro de `ModuloEstoque`.

### Fluxo de uma requisição de entrada

Exemplo: registrar entrada de um insumo no estoque.

```text
Navegador
  │ POST /Estoque/RegistrarEntrada
  ▼
Rota MVC
  │ encontra EstoqueController.RegistrarEntrada(...)
  ▼
Controller
  │ recebe RegistrarEntradaViewModel
  │ valida ModelState
  │ mapeia ViewModel → DTO
  ▼
ServicoEstoque
  │ busca o insumo pelo Guid
  │ busca o colaborador responsável pelo Guid
  │ valida quantidade
  │ cria RequisicaoEntrada
  ▼
Repositório
  │ salva a requisição
  ▼
ContextoJson
  │ grava dados.json
  ▼
Controller
  │ redireciona para o histórico
  ▼
View
  │ exibe estoque atualizado e movimentações
```

### Papel de cada camada

| Camada | Responsabilidade | Exemplo |
|---|---|---|
| Apresentação | receber HTTP, mostrar telas e validar entrada inicial | Controllers, ViewModels, Views |
| Aplicação | executar casos de uso e regras de negócio | `ServicoEstoque`, `ServicoMedicamento` |
| Domínio | representar conceitos reais do sistema | `Medicamento`, `RequisicaoEntrada`, `RequisicaoSaida` |
| Infraestrutura JSON | salvar e carregar dados | `ContextoJson`, repositórios em arquivo |
| Compartilhado | código reutilizável | `EntidadeBase`, `IRepositorio`, extensões |

O Controller não deve fazer tudo. Ele é o maestro da requisição. A regra de negócio fica no serviço e no domínio.

---

## 2. Mapa de aprendizado

### 1. Organização Modular no MVC

**O que é:** dividir o projeto por módulos de negócio, e não apenas por tipo técnico de arquivo.

**Para que serve:** deixar cada funcionalidade concentrada em uma região clara do projeto.

**Onde aparece:** `ModuloFornecedor`, `ModuloPaciente`, `ModuloFuncionario`, `ModuloMedicamento`, `ModuloEstoque`.

**O que dominar:** criar um módulo novo com domínio, aplicação, infraestrutura e apresentação.

### 2. View Location personalizada

**O que é:** configuração que muda os caminhos onde o MVC procura views.

**Para que serve:** permitir views dentro dos módulos.

**Onde aparece:** `AddPresentationConfig`.

```text
/Modulos/Modulo{1}/Apresentacao/Views/{0}.cshtml
/Compartilhado/Apresentacao/Views/{0}.cshtml
```

**O que dominar:** saber que `EstoqueController.Historico()` procura `Modulos/ModuloEstoque/Apresentacao/Views/Historico.cshtml`.

### 3. Controllers e Actions

**O que é:** Controller recebe a requisição; action executa o caso HTTP específico.

**Para que serve:** conectar navegador e aplicação.

**Onde aparece:** `FornecedorController`, `PacienteController`, `FuncionarioController`, `MedicamentoController`, `EstoqueController`.

**O que dominar:** GET monta tela; POST processa alteração.

### 4. ViewModels

**O que é:** modelos específicos para cada tela.

**Para que serve:** separar tela de entidade de domínio.

**Onde aparece:** `MedicamentoViewModels`, `EstoqueViewModels`, `FornecedorViewModels`, etc.

**Exemplo:** `RegistrarEntradaViewModel` carrega dados do insumo, o colaborador selecionado, a quantidade e a lista de colaboradores para o dropdown.

**O que dominar:** ViewModel de cadastro não precisa ser igual à entidade.

### 5. Model Binding, DataAnnotations e ModelState

**O que é:** o binder converte dados HTTP em ViewModel; DataAnnotations validam campos; ModelState guarda erros.

**Para que serve:** validar entrada antes de executar regra de negócio.

**Onde aparece:** `[Required]`, `[StringLength]`, `[Range]`, `if (!ModelState.IsValid)`.

**O que dominar:** mesmo com DataAnnotations, o serviço ainda precisa validar se IDs existem e se a operação faz sentido.

### 6. Bootstrap

**O que é:** biblioteca CSS/JS usada para layout e componentes visuais.

**Para que serve:** criar telas consistentes com navbar, cards, forms, botões e alerts.

**Onde aparece:** `_Layout.cshtml`, listagens em cards, alerts, badges, forms e histórico de estoque.

**O que dominar:** usar `container`, `card`, `alert`, `btn`, `badge`, `form-control`, `form-select`, `list-group`.

### 7. Tag Helpers

**O que é:** atributos Razor que conectam HTML ao MVC.

**Para que serve:** gerar campos, links, validações e rotas sem montar strings manualmente.

**Onde aparece:** `asp-for`, `asp-validation-for`, `asp-action`, `asp-controller`, `asp-route-id`, `asp-route-medicamentoId`.

**O que dominar:** `asp-route-medicamentoId="@m.Id"` faz o ID chegar como parâmetro da action.

### 8. Injeção de Dependência

**O que é:** o container cria e entrega objetos necessários para controllers e serviços.

**Para que serve:** reduzir acoplamento e centralizar composição.

**Onde aparece:** `Program.cs`, `AddInfraRepositories`, `AddApplicationServices`, `AddPresentationConfig`.

**No recorte JSON:** registre os repositórios `EmArquivo`, não os repositórios SQL.

```text
IRepositorioFornecedor  → RepositorioFornecedorEmArquivo
IRepositorioFuncionario → RepositorioFuncionarioEmArquivo
IRepositorioMedicamento → RepositorioMedicamentoEmArquivo
IRepositorioPaciente    → RepositorioPacienteEmArquivo
IRepositorioRequisicao  → RepositorioRequisicaoEmArquivo
```

### 9. Camada de Aplicação

**O que é:** camada de serviços que executa casos de uso.

**Para que serve:** evitar Controller gordo.

**Onde aparece:** `ServicoFornecedor`, `ServicoPaciente`, `ServicoFuncionario`, `ServicoMedicamento`, `ServicoEstoque`.

**O que dominar:** serviço recebe DTO, valida regra, usa repositório e retorna `Result`.

### 10. FluentResults

**O que é:** biblioteca para representar sucesso ou falha.

**Para que serve:** retornar erro de negócio sem lançar exceção.

**Onde aparece:** todos os serviços.

```text
Result.Ok()
Result.Fail("Insumo não encontrado.")
Result.Ok(detalhesDto)
```

**O que dominar:** `Result` para operações sem valor e `Result<T>` para operações que retornam dados.

### 11. TempData

**O que é:** armazenamento temporário que sobrevive a um redirect.

**Para que serve:** mostrar mensagem após redirecionar.

**Onde aparece:** quando um registro não é encontrado e a action volta para a listagem.

```text
TempData.AddErrorMessage(resultado)
RedirectToAction("Listar")
```

### 12. Métodos de Extensão

**O que é:** métodos estáticos que parecem fazer parte de outro tipo.

**Para que serve:** reduzir repetição e melhorar legibilidade.

**Onde aparece:**

- `services.AddInfraRepositories()`
- `services.AddApplicationServices()`
- `services.AddPresentationConfig()`
- `ModelState.AddModelError(resultado)`
- `TempData.AddErrorMessage(resultado)`

### 13. AutoMapper

**O que é:** biblioteca para mapear objetos.

**Para que serve:** converter ViewModel ↔ DTO e DTO ↔ ViewModel sem repetir construtores manualmente.

**Onde aparece:** Profiles de cada módulo.

**O que dominar:** AutoMapper não executa regra de negócio. Ele só transporta dados entre formatos.

### 14. Repositórios em arquivo

**O que é:** classes que salvam e leem listas de entidades de um arquivo JSON.

**Para que serve:** persistir dados sem banco.

**Onde aparece:** `RepositorioBaseEmArquivo<T>`, `ContextoJson` e repositórios específicos `EmArquivo`.

**O que dominar:** o repositório manipula listas; `ContextoJson` salva o estado completo.

### 15. Polimorfismo no JSON

**O que é:** salvar objetos derivados dentro de uma lista do tipo base.

**Para que serve:** guardar entradas e saídas na mesma lista de requisições.

**Onde aparece:** `RequisicaoBase` com:

```text
RequisicaoEntrada
RequisicaoSaida
```

**O que dominar:** uma lista de `RequisicaoBase` pode conter objetos de tipos diferentes. O JSON precisa saber qual tipo concreto cada item representa.

### 16. Cálculo de estoque por histórico

**O que é:** o estoque atual é calculado pelas movimentações.

**Para que serve:** manter rastreabilidade. Você não sabe apenas “quanto tem”; sabe por que tem aquela quantidade.

**Onde aparece:** propriedade `QuantidadeEmEstoque` do medicamento.

**O que dominar:** entrada soma, saída subtrai, e o histórico explica o saldo.

### 17. LINQ e normalização de documentos

**O que é:** LINQ consulta coleções; normalização remove formatação de documentos.

**Para que serve:** validar duplicidade mesmo quando o usuário digita com máscara diferente.

**Onde aparece:** `Any`, `Select`, `Where`, `Sum`, `ToList` e métodos como `NormalizarDigitos`.

**O que dominar:** comparar CPF/CNPJ/cartão sem pontuação.

---

## 3. Projeto paralelo: Gestão de Insumos de Ateliê Criativo Comunitário

### Por que este tema?

Um ateliê comunitário oferece oficinas de pintura, costura, reciclagem criativa e artesanato. Para isso, mantém um estoque de insumos: tintas, tecidos, cola, linhas, pincéis, papéis, madeiras leves, fitas e materiais de consumo.

O sistema deve controlar:

- quem fornece os insumos;
- quem registra entradas;
- para qual oficina/projeto os insumos saem;
- qual o saldo atual de cada insumo;
- o histórico de entradas e saídas.

### Mapeamento com o projeto original

```text
Controle de Medicamentos       Projeto paralelo
Fornecedor                     FornecedorInsumo
Funcionario                    Colaborador
Paciente                       OficinaAtendida
Medicamento                    Insumo
RequisicaoEntrada              EntradaEstoque
RequisicaoSaida                SaidaEstoque
MedicamentoPrescrito           InsumoSolicitado
Estoque                        Estoque
```

### Entidades

| Entidade | Campos principais | Papel |
|---|---|---|
| `FornecedorInsumo` | Id, Nome, Telefone, Cnpj | quem fornece materiais |
| `Colaborador` | Id, Nome, Telefone, Cpf | quem registra entradas |
| `OficinaAtendida` | Id, Nome, Responsavel, Codigo | quem recebe saídas de materiais |
| `Insumo` | Id, Nome, Descricao, Fornecedor, Requisicoes | material controlado |
| `EntradaEstoque` | Id, DataCriacao, Colaborador, Insumo, Quantidade | movimentação que aumenta estoque |
| `SaidaEstoque` | Id, DataCriacao, OficinaAtendida, InsumosSolicitados | movimentação que reduz estoque |
| `InsumoSolicitado` | Insumo, Quantidade | item dentro de uma saída |

### Relacionamentos

```text
FornecedorInsumo 1 ─── * Insumo

Colaborador 1 ─── * EntradaEstoque

OficinaAtendida 1 ─── * SaidaEstoque

Insumo 1 ─── * EntradaEstoque
Insumo 1 ─── * InsumoSolicitado * ─── 1 SaidaEstoque
```

### Regras de negócio

#### FornecedorInsumo

- Nome obrigatório entre 3 e 100 caracteres.
- Telefone obrigatório.
- CNPJ obrigatório com 14 dígitos.
- Não pode haver fornecedor com CNPJ duplicado.

#### Colaborador

- Nome obrigatório entre 3 e 100 caracteres.
- Telefone obrigatório.
- CPF obrigatório com 11 dígitos.
- Não pode haver colaborador com CPF duplicado.

#### OficinaAtendida

- Nome obrigatório entre 3 e 100 caracteres.
- Responsável obrigatório.
- Código interno obrigatório.
- Não pode haver oficina com mesmo código.

#### Insumo

- Nome obrigatório entre 3 e 100 caracteres.
- Descrição obrigatória entre 5 e 255 caracteres.
- Fornecedor obrigatório.
- Não pode haver insumo com mesmo nome para o mesmo fornecedor.
- Estoque atual é calculado pelo histórico.
- Insumo com menos de 20 unidades deve ser destacado como baixo estoque.

#### EntradaEstoque

- Insumo obrigatório.
- Colaborador obrigatório.
- Quantidade maior que zero.
- Ao registrar entrada, o histórico do insumo ganha uma movimentação.

#### SaidaEstoque

- Insumo obrigatório.
- Oficina atendida obrigatória.
- Quantidade maior que zero.
- Não pode sair quantidade maior que o estoque atual.
- Ao registrar saída, o histórico do insumo ganha uma movimentação.

---

## 4. Roadmap de implementação

### Etapa 1 — Projeto e estrutura modular

**Objetivo:** criar projeto MVC, layout, Bootstrap e estrutura de módulos.

**Conceitos praticados:** MVC, Razor, rota padrão, Bootstrap, ViewLocation personalizada.

**Resultado esperado:** aplicação abre na Home e navega entre módulos.

```text
ModuloFornecedorInsumo
ModuloColaborador
ModuloOficinaAtendida
ModuloInsumo
ModuloEstoque
```

### Etapa 2 — Domínio

**Objetivo:** criar entidades, classe base e movimentações de estoque.

**Conceitos praticados:** entidades, `Guid`, herança, polimorfismo, relacionamentos e propriedades calculadas.

**Resultado esperado:** `Insumo.QuantidadeEmEstoque` calcula o saldo por entradas e saídas.

### Etapa 3 — JSON e repositórios em arquivo

**Objetivo:** criar `ContextoJson`, `IRepositorio<T>` e repositórios `EmArquivo`.

**Conceitos praticados:** persistência JSON, listas, repositório genérico, `ReferenceHandler.Preserve` e polimorfismo.

**Resultado esperado:** dados são salvos em `dados.json`, sem banco.

### Etapa 4 — DI e métodos de extensão

**Objetivo:** registrar repositórios, serviços e apresentação.

**Conceitos praticados:** DI, `AddScoped`, métodos de extensão, `IServiceCollection`.

**Resultado esperado:** `Program.cs` fica simples:

```text
builder.Services.AddInfraRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddPresentationConfig();
```

### Etapa 5 — CRUD de FornecedorInsumo

**Objetivo:** criar o primeiro CRUD completo.

**Conceitos praticados:** Controller, ViewModel, DTO, Serviço, FluentResults, AutoMapper, TempData.

**Resultado esperado:** fornecedores são cadastrados, editados, listados e excluídos com validação de CNPJ duplicado.

### Etapa 6 — CRUD de Colaborador e OficinaAtendida

**Objetivo:** criar os cadastros auxiliares necessários ao estoque.

**Conceitos praticados:** validação de documentos, normalização de dígitos, duplicidade e PRG.

**Resultado esperado:** colaboradores e oficinas estão prontos para uso nas movimentações.

### Etapa 7 — CRUD de Insumo

**Objetivo:** cadastrar insumos vinculados a fornecedores.

**Conceitos praticados:** dropdown, relacionamento, repopulação de select, validação de fornecedor existente.

**Resultado esperado:** insumo só é criado com fornecedor válido.

### Etapa 8 — Entrada de estoque

**Objetivo:** registrar entrada de insumo.

**Conceitos praticados:** ação específica fora do CRUD básico, histórico, `Guid` na rota, hidden inputs, DTO e serviço.

**Resultado esperado:** entrada aumenta o saldo calculado do insumo.

### Etapa 9 — Saída de estoque

**Objetivo:** registrar saída para uma oficina.

**Conceitos praticados:** validação de estoque disponível, relacionamento com oficina, regra de quantidade.

**Resultado esperado:** saída reduz o saldo e bloqueia retirada acima do disponível.

### Etapa 10 — Histórico

**Objetivo:** exibir entradas e saídas de um insumo.

**Conceitos praticados:** LINQ, `Where`, `Any`, `Sum`, projeção para ViewModels, cards e list-groups.

**Resultado esperado:** tela mostra saldo atual, entradas e saídas.

---

## 5. Fluxos de conhecimento

### Fluxo A — Cadastrar um Insumo

```text
GET Insumo/Cadastrar
  → serviço busca fornecedores
  → controller monta ViewModel
  → view mostra select

POST Insumo/Cadastrar
  → Model Binding cria CadastrarInsumoViewModel
  → DataAnnotations validam campos simples
  → AutoMapper cria CadastrarInsumoDto
  → Serviço busca FornecedorId
  → Serviço valida duplicidade
  → Serviço cria Insumo
  → Repositório salva
  → RedirectToAction(Listar)
```

Se o fornecedor não existir, o serviço retorna falha no campo `FornecedorId`. O Controller adiciona o erro ao `ModelState` e devolve a view com o dropdown recarregado.

### Fluxo B — Registrar entrada de estoque

```text
GET Estoque/RegistrarEntrada?insumoId=...
  → serviço busca insumo
  → controller busca colaboradores
  → view mostra dados do insumo e select de colaborador

POST RegistrarEntrada
  → ViewModel contém InsumoId, ColaboradorId e Quantidade
  → serviço busca insumo
  → serviço busca colaborador
  → serviço valida quantidade
  → cria EntradaEstoque
  → entrada se registra no histórico do insumo
  → repositório salva
  → redirect para Histórico
```

Esse fluxo mostra que estoque não é campo editável. Estoque é consequência de uma movimentação.

### Fluxo C — Registrar saída de estoque

```text
POST RegistrarSaida
  → busca insumo
  → busca oficina atendida
  → valida quantidade maior que zero
  → compara quantidade solicitada com estoque atual
  → cria InsumoSolicitado
  → cria SaidaEstoque
  → saída se registra no histórico do insumo
  → salva
```

Se a quantidade solicitada for maior que o saldo, a operação falha:

```text
Result.Fail("A quantidade solicitada excede o estoque disponível.")
```

O erro volta para o formulário com `ModelState`.

### Fluxo D — Histórico do insumo

```text
GET Estoque/Historico?insumoId=...
  → serviço busca detalhes do insumo
  → seleciona entradas daquele insumo
  → seleciona saídas daquele insumo
  → mapeia DTOs para ViewModels
  → view exibe saldo, entradas e saídas
```

Essa tela fecha o ciclo mental:

```text
Entrada soma
Saída subtrai
Histórico explica
Saldo atual resume
```

### Fluxo E — Erro com TempData

Quando o usuário tenta abrir o histórico de um insumo inexistente:

```text
EstoqueController.Historico(insumoId)
  → serviço não encontra insumo
  → TempData.AddErrorMessage(resultado)
  → RedirectToAction("Listar", "Insumo")
  → Listar mostra alert Bootstrap
```

Use `TempData` quando a mensagem precisa sobreviver a um redirecionamento.

### Fluxo F — Erro com ModelState

Quando o usuário seleciona uma oficina inválida no POST de saída:

```text
POST RegistrarSaida
  → serviço retorna Result.Fail com campo OficinaAtendidaId
  → Controller chama ModelState.AddModelError(resultado)
  → retorna a mesma view
  → asp-validation-for mostra a mensagem
```

Use `ModelState` quando a mensagem pertence ao formulário atual.

---

## 6. Estrutura sugerida de arquivos

```text
GestaoInsumosAtelie.WebApp
  ├── Compartilhado
  │   ├── Aplicacao
  │   │   └── InjecaoDependencia.cs
  │   ├── Apresentacao
  │   │   ├── Extensions
  │   │   │   ├── ModelStateExtensions.cs
  │   │   │   └── TempDataExtensions.cs
  │   │   ├── Views
  │   │   │   ├── _Layout.cshtml
  │   │   │   └── Index.cshtml
  │   │   └── InjecaoDependencia.cs
  │   ├── Dominio
  │   │   ├── EntidadeBase.cs
  │   │   └── IRepositorio.cs
  │   └── Infra
  │       ├── Arquivos
  │       │   ├── ContextoJson.cs
  │       │   └── RepositorioBaseEmArquivo.cs
  │       └── InjecaoDependencia.cs
  ├── Modulos
  │   ├── ModuloFornecedorInsumo
  │   ├── ModuloColaborador
  │   ├── ModuloOficinaAtendida
  │   ├── ModuloInsumo
  │   └── ModuloEstoque
  └── Program.cs
```

Não crie pasta, classe ou configuração de SQL neste projeto paralelo. A infraestrutura deve ficar 100% em `Compartilhado/Infra/Arquivos`.

---

## 7. Boas práticas para aplicar

- Use ViewModel para telas e DTO para serviços.
- Não receba entidade de domínio diretamente em POST.
- Não altere estoque manualmente no cadastro do insumo.
- Registre entrada e saída como eventos de negócio.
- Calcule estoque a partir do histórico.
- Valide documentos normalizando dígitos.
- Valide relacionamento no serviço, não apenas no formulário.
- Recarregue dropdowns quando retornar view inválida.
- Use `ModelState` para erro de formulário.
- Use `TempData` para erro após redirect.
- Use `Result` e `Result<T>` em serviços.
- Use AutoMapper apenas para transporte de dados.
- Use repositórios `EmArquivo` no projeto paralelo.
- Mantenha `ContextoJson` como única persistência.
- Use LINQ para consultar histórico, mas sem esconder regras importantes.

---

## 8. Desafios extras

1. **Baixo estoque:** destacar insumos com menos de 20 unidades.
2. **Filtro por baixo estoque:** mostrar apenas insumos críticos.
3. **Filtro por fornecedor:** listar insumos de um fornecedor específico.
4. **Busca por nome:** pesquisar insumos com `Contains`.
5. **Entrada em lote:** registrar vários insumos recebidos de uma vez.
6. **Saída com múltiplos insumos:** evoluir saída para vários itens, como a lista de `InsumoSolicitado`.
7. **Cancelamento de movimentação:** estudar como desfazer entrada/saída sem apagar histórico.
8. **Relatório mensal:** somar entradas e saídas por período.
9. **Validação personalizada:** criar atributo para CPF/CNPJ.
10. **Dashboard:** total de insumos, baixo estoque, entradas do mês e saídas do mês.
11. **Histórico ordenado:** ordenar movimentações por data decrescente.
12. **Reposição sugerida:** calcular quanto comprar para voltar a um estoque mínimo.

---

## 9. Checklist final

Marque apenas quando conseguir explicar sozinho.

- [ ] Sei explicar por que a base JSON recomendada é a `v5`.
- [ ] Sei explicar o que foi ignorado por envolver banco/SQL.
- [ ] Sei criar uma aplicação MVC com organização modular.
- [ ] Sei configurar views dentro dos módulos.
- [ ] Sei explicar Controller → Serviço → Repositório → JSON.
- [ ] Sei diferenciar ViewModel, DTO e Entidade.
- [ ] Sei explicar Model Binding.
- [ ] Sei explicar DataAnnotations e ModelState.
- [ ] Sei retornar a mesma view com erros de validação.
- [ ] Sei usar TempData após redirect.
- [ ] Sei registrar dependências com `AddScoped`.
- [ ] Sei criar métodos de extensão para DI.
- [ ] Sei usar FluentResults em serviços.
- [ ] Sei converter falhas de serviço para ModelState.
- [ ] Sei converter falhas de serviço para TempData.
- [ ] Sei configurar AutoMapper com Profiles.
- [ ] Sei criar repositórios em arquivo.
- [ ] Sei explicar `ContextoJson`.
- [ ] Sei explicar `ReferenceHandler.Preserve`.
- [ ] Sei explicar polimorfismo JSON com requisições de entrada e saída.
- [ ] Sei calcular estoque por histórico.
- [ ] Sei bloquear saída acima do estoque disponível.
- [ ] Sei usar `Guid` em rotas, forms e entidades.
- [ ] Sei usar LINQ com `Any`, `Select`, `Where`, `Sum` e `ToList`.
- [ ] Sei normalizar CPF/CNPJ antes de comparar.
- [ ] Sei montar dropdowns de relacionamentos.
- [ ] Sei recarregar dropdowns após erro.
- [ ] Sei explicar por que estoque não deve ser editado diretamente.
- [ ] Sei seguir o fluxo completo: form → binder → ViewModel → DTO → serviço → entidade → repositório → JSON → redirect → view.

Ao concluir esse projeto paralelo, você terá praticado uma aplicação MVC mais rica que CRUD simples: uma aplicação com histórico, saldo calculado, entradas, saídas, regras de integridade e persistência em JSON. Esse é exatamente o tipo de projeto que ajuda a entender como as peças do ASP.NET Core MVC trabalham juntas.

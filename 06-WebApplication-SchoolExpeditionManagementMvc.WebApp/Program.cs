// ASP.NET Core - Aplicação Web
// Builder de um servidor web
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Criação da instância do servidor web
WebApplication app = builder.Build();

// Middlewares - Funções que executam em cada chamada que o nosso servidor vai receber
app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();

// Inicia o loop da aplicação
app.Run();
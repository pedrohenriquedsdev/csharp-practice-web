WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(); // ativa o mvc

WebApplication app = builder.Build();

app.UseStaticFiles(); //libera css, imagens e etc

app.UseRouting(); //ativa o sistema de rotas da aplicação ASP.NET Core.

app.MapDefaultControllerRoute(); //permite rotas 

app.Run();
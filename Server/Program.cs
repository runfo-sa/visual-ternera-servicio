using Core;
using Server.Logic;
using Server.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Tabla en Memoria
        builder.Services.AddSqlServer<ClientStatusDb>("Server=rafatest;Database=VisualTernera;Trusted_Connection=true;Encrypt=True;TrustServerCertificate=True");

        // Swager Docs
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "<Title>",
                Description = "<Description>",
                Version = "v1"
            });
        });

        // Watcher singleton
        builder.Services.AddSingleton<Watcher>();

        var app = builder.Build();
        IServiceProvider services = app.Services;

        Watcher watcher = services.GetService<Watcher>()
            ?? throw new Exception("La carpeta de etiquetas del servidor no esta siendo observada.");

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // API Endpoints:
        app.MapPost("/validarcliente", async (ClientStatusDb db, Client client, HttpContext context) =>
        {
            Status status = Analysis.CheckClient(client, watcher.ServerEtiquetas);

            ClientStatus? clientStatus = db.Find(client.Name);
            if (clientStatus is null)
            {
                await db.EstadoCliente.AddAsync(new ClientStatus(client.Name, status));
            }
            else
            {
                clientStatus.Estado = status;
                clientStatus.UltimaConexion = DateTime.Now;
            }
            await db.SaveChangesAsync();

            Console.WriteLine("DEBUG: " + db.Find(client.Name));
            return TypedResults.Ok();
        })
        .WithName("PostValidarCliente")
        .WithOpenApi();

        app.MapPost("/multiplesinstalaciones", async (ClientStatusDb db, Client client, HttpContext context) =>
        {
            ClientStatus? clientStatus = db.Find(client.Name);
            if (clientStatus is null)
            {
                await db.EstadoCliente.AddAsync(new ClientStatus(client.Name, Status.MultipleInstalaciones));
            }
            else
            {
                clientStatus.Estado = Status.MultipleInstalaciones;
                clientStatus.UltimaConexion = DateTime.Now;
            }
            await db.SaveChangesAsync();

            Console.WriteLine("DEBUG: " + db.Find(client.Name));

            var commonpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var path = Path.Combine(commonpath, "VisualTerneraServer\\" + client.Name);

            Logger.Log(path, "Se encontraron mas de una instalación de PiQuatro, revisar el log del cliente para mas información.");

            return TypedResults.Ok();
        })
        .WithName("PostMultiplesInstalaciones")
        .WithOpenApi();

        app.Run();
    }
}

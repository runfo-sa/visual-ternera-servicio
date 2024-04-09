using Core;
using Microsoft.EntityFrameworkCore;
using Server.Logic;
using Server.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Tabla en Memoria
        builder.Services.AddDbContext<ClientStatusDb>(opt => opt.UseInMemoryDatabase("StatusClient"));

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
        app.MapPost("/validateClient", async (ClientStatusDb db, Client client, HttpContext context) =>
        {
            Console.WriteLine("DEBUG: " + client.ToString());
            Console.WriteLine("DEBUG: " + client.Etiquetas.Length);
            Console.WriteLine("DEBUG: " + context.Connection.RemoteIpAddress?.ToString());

            Status status = Analysis.CheckClient(client, watcher.ServerEtiquetas);

            ClientStatus? clientStatus = await db.Clients.FindAsync(client.Id);
            if (clientStatus is null)
            {
                await db.Clients.AddAsync(new ClientStatus(client.Id, status));
            }
            else
            {
                clientStatus = new ClientStatus(client.Id, status);
            }
            await db.SaveChangesAsync();

            Console.WriteLine("DEBUG: " + db.Clients.Find(client.Id));
            return TypedResults.Ok();
        })
        .WithName("PostValidateClient")
        .WithOpenApi();

        app.Run();
    }
}

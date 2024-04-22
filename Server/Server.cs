using Core;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Server.Logic;
using Server.Models;
using System.Net;
using System.Threading.RateLimiting;

namespace Server
{
    public static class Server
    {
        private static readonly string LogFolder = "Visual Ternera Server\\";
        private static readonly string ClientFolder = "client-repo";
        private static readonly string ClientFile = "Client.exe";
        private static readonly string InstallerFile = "installer.ps1";

        public static void Run(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSqlServer<ClientStatusDb>("Server=rafatest;Database=VisualTernera;Trusted_Connection=true;Encrypt=True;TrustServerCertificate=True");

            // Swager Docs
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Visual Ternera - Controlador de Etiquetas",
                    Description = "Web API encargada de reportar el estado de etiquetas en cada puesto",
                    Version = "v1"
                });
            });

            // Watchers singleton
            builder.Services.AddSingleton<WatcherPiQuatro>();
            builder.Services.AddSingleton(new WatcherClient(ClientFolder, ClientFile));

            // Limita la cantidad de pedidos a 100 cada 5 minutos
            builder.Services.AddRateLimiter(_ => _
                .AddFixedWindowLimiter(policyName: "fixed", options =>
                {
                    options.PermitLimit = 100;
                    options.Window = TimeSpan.FromMinutes(10);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 50;
                })
            );

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseRateLimiter();

            //app.UseHttpsRedirection();

            var piQuatroWatch = app.Services.GetService<WatcherPiQuatro>();
            var clientWatch = app.Services.GetService<WatcherClient>();

            app.Use(async (context, next) =>
            {
                // Middleware que verifica si es una conexión valida y la termina en caso de no serlo.
                // Solamente para estos endpoints.
                if (context.Request.Path.StartsWithSegments("/validarcliente") ||
                    context.Request.Path.StartsWithSegments("/multiplesinstalaciones"))
                {
                    var query = context.Request.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    StringValues key;
                    StringValues hash;

                    if (!query.TryGetValue("request-key", out key)
                        || key.IsNullOrEmpty()
                        || !query.TryGetValue("request-hash", out hash)
                        || hash.IsNullOrEmpty()
                        // Comprobamos que el hash y key recibidos sean coherentes
                        || Encryption.EncryptKey(key!) != hash
                        // Comprobamos que podemos obtener el mismo hash con nuestra clave publica y privada
                        || Encryption.EncryptKey() != hash)
                    {
                        context.Response.StatusCode = (Int32)HttpStatusCode.Unauthorized;
                        await context.Response.WriteAsync("Unauthorized");
                        return;
                    }
                }

                await next();
            });

            app.UseRouting();

            // API Endpoints:
            app.MapPost("/validarcliente", async (ClientStatusDb db, Request client, HttpContext context) =>
            {
                Status status = Analysis.CheckClient(client, piQuatroWatch!.ServerEtiquetas);

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

                return TypedResults.Ok(Enum.GetName(typeof(Status), status));
            })
            .WithName("PostValidarCliente")
            .WithOpenApi();

            app.MapPost("/multiplesinstalaciones", async (ClientStatusDb db, Request client, HttpContext context) =>
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

                var commonpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var path = Path.Combine(commonpath, LogFolder + client.Name);

                Logger.Log(path, "Se encontraron mas de una instalación de PiQuatro, revisar el log del cliente para mas información.");

                return TypedResults.Ok(Enum.GetName(typeof(Status), Status.MultipleInstalaciones));
            })
            .WithName("PostMultiplesInstalaciones")
            .WithOpenApi();

            app.MapGet("/obtenercliente", async (string key, HttpContext context) =>
            {
                if (key != Encryption.DOWNLOAD_KEY)
                {
                    await context.Response.WriteAsync("Unauthorized");
                    return Results.Unauthorized();
                }

                var bytes = await File.ReadAllBytesAsync(Path.Combine(ClientFolder, ClientFile));
                return Results.File(bytes, "application/vnd.microsoft.portable-executable", ClientFile);
            })
            .WithName("GetObtenerCliente")
            .WithOpenApi();

            app.MapGet("/instalador", async (string key, HttpContext context) =>
            {
                if (key != Encryption.DOWNLOAD_KEY)
                {
                    await context.Response.WriteAsync("Unauthorized");
                    return Results.Unauthorized();
                }

                var bytes = await File.ReadAllBytesAsync(Path.Combine(ClientFolder, InstallerFile));
                return Results.File(bytes, "text/plain", InstallerFile);
            })
            .WithName("GetInstalador")
            .WithOpenApi();

            app.MapGet("/clienteversion", () =>
            {
                return TypedResults.Ok(clientWatch!.ClientHash);
            })
            .WithName("GetVersionCliente")
            .WithOpenApi();

            app.Run();
        }
    }
}
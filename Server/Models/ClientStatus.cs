using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    /// <summary>
    /// Representación del estado de un cliente en una tabla.
    /// </summary>
    [PrimaryKey(nameof(Id))]
    [Index(nameof(Cliente), IsUnique = true)]
    public class ClientStatus
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }

        public string Cliente { get; set; }
        public Status Estado { get; set; }
        public DateTime UltimaConexion { get; set; }

        public ClientStatus(string Cliente, Status Estado, DateTime UltimaConexion)
        {
            this.Cliente = Cliente;
            this.Estado = Estado;
            this.UltimaConexion = UltimaConexion;
        }

        public ClientStatus(string Cliente, Status Estado)
        {
            this.Cliente = Cliente;
            this.Estado = Estado;
            UltimaConexion = DateTime.Now;
        }

        public override string ToString()
        {
            return Cliente + "::" + Estado + "::" + UltimaConexion;
        }
    }

    /// <summary>
    /// Instancia de conexión con una base de datos para la tabla de <see cref="ClientStatus"/>
    /// </summary>
    public class ClientStatusDb(DbContextOptions options) : DbContext(options)
    {
        public DbSet<ClientStatus> EstadoCliente { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientStatus>().ToTable(b => b.IsMemoryOptimized());
            modelBuilder.HasDefaultSchema("service");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }

        public ClientStatus? Find(string Name)
        {
            List<ClientStatus> clientStatus = [.. EstadoCliente.Where(e => e.Cliente == Name)];
            return clientStatus.IsNullOrEmpty() ? null : clientStatus.First();
        }
    }
}
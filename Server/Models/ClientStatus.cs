using Microsoft.EntityFrameworkCore;

namespace Server.Models
{
    /// <summary>
    /// Estado del <see cref="Core.Client"/>
    /// </summary>
    public enum Status
    {
        Okay,
        Sobrantes,
        Desactualizada,
        DesactualizadaSobrantes,
    }

    /// <summary>
    /// Representación del estado de un cliente en una tabla.
    /// </summary>
    [PrimaryKey(nameof(Name))]
    public class ClientStatus(string Name, Status Status)
    {
        public string Name { get; private set; } = Name;
        public Status Status { get; private set; } = Status;
        public DateTime LastUpdate { get; private set; } = DateTime.Now;

        public override string ToString()
        {
            return Name + "::" + Status + "::" + LastUpdate;
        }
    }

    /// <summary>
    /// Instancia de conexión con una base de datos para la tabla de <see cref="ClientStatus"/>
    /// </summary>
    public class ClientStatusDb(DbContextOptions options) : DbContext(options)
    {
        public DbSet<ClientStatus> Clients { get; set; } = null!;
    }
}

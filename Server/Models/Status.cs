namespace Server.Models
{
    /// <summary>
    /// Estado del cliente
    /// </summary>
    public enum Status : byte
    {
        Okay = 0,
        Sobrantes = 2,
        Desactualizada = 1,
        DesactualizadaSobrantes = 3,
        MultipleInstalaciones = 4,
    }
}
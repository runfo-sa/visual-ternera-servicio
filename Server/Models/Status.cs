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
        MultipleInstalaciones = 4,
        DesactualizadaSobrantes = 3,
    }
}
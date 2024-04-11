namespace Server.Models
{
    /// <summary>
    /// Estado del <see cref="Core.Client"/>
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

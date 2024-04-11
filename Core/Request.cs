namespace Core
{
    /// <summary>
    /// Representa el mensaje que es enviado desde un clienta hacia el servidor.
    /// </summary>
    /// <param name="Name">Nombre identificador del cliente, es su IPv4</param>
    /// <param name="Etiquetas">Listado de etiquetas presentes en la maquina del cliente</param>
    public record struct Request(string Name, Etiqueta[]? Etiquetas);
}

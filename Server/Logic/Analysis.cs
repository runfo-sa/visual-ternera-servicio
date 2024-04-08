using Core;
using Server.Models;

namespace Server.Logic
{
    public static class Analysis
    {
        /// <summary>
        /// Función que compara la lista de <see cref="Etiqueta"/> del cliente con las del servidor.
        /// <br/>
        /// Analiza en busca de archivos faltantes, sobrantes o distintos.
        /// </summary>
        /// <returns><see cref="Status"/> - Estado del cliente</returns>
        public static Status CheckClient(List<Etiqueta> clientEtiquetas, List<Etiqueta> serverEtiquetas)
        {
            /*TODO: Logica de comparación*/
            return Status.Bad;
        }
    }
}

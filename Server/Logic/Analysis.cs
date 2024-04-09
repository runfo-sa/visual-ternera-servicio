using Core;
using Microsoft.IdentityModel.Tokens;
using Server.Models;
using System.Collections.Frozen;

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
        public static Status CheckClient(Client client, FrozenSet<Etiqueta> serverEtiquetas)
        {
            FrozenSet<Etiqueta> clientEtiquetas = client.Etiquetas.ToFrozenSet();

            // Obtenemos el conjunto distinto del servidor
            IEnumerable<Etiqueta> desactualizadas = serverEtiquetas.Except(clientEtiquetas);
            IEnumerable<Etiqueta>? sobrantes = null;

            // Chequeamos si tiene archivos sobrantes
            if (clientEtiquetas.Count > serverEtiquetas.Count)
            {
                sobrantes = clientEtiquetas.Except(serverEtiquetas, new EtiquetaCompareName());
            }

            Status status = (sobrantes is not null) ?
                ((desactualizadas.IsNullOrEmpty()) ? Status.Sobrantes : Status.DesactualizadaSobrantes)
                : ((desactualizadas.IsNullOrEmpty()) ? Status.Okay : Status.Desactualizada);

            if (status != Status.Okay)
            {
                DateTime date = DateTime.Now;

                var commonpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var path = Path.Combine(commonpath, "VisualTerneraServer\\" + client.Id);
                var file = Path.Combine(path, date.ToString("yyyy_MM_dd") + ".log");

                Directory.CreateDirectory(path);

                string list = (sobrantes is not null && status == Status.Sobrantes) ?
                    "Sobrantes:\n" + string.Join('\n', sobrantes)
                    : (sobrantes is not null) ?
                    "Desactualizadas:\n" + string.Join('\n', desactualizadas) +
                    "\nSobrantes:\n" + string.Join('\n', sobrantes)
                    : "Desactualizadas:\n" + string.Join('\n', desactualizadas);

                string separator = new('-', 128);
                File.AppendAllText(file,
                    string.Format("{0}\nError - {1}\n{2}\n{3}",
                    separator, date.ToString("hh:mm:ss"), separator, list)
                );
            }

            return status;
        }
    }
}

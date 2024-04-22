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
        public static Status CheckClient(Request client, FrozenSet<Etiqueta> serverEtiquetas)
        {
            FrozenSet<Etiqueta> clientEtiquetas = client.Etiquetas!.ToFrozenSet();

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
                var commonpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var path = Path.Combine(commonpath, "Visual Ternera Server\\" + client.Name);

                string list = (sobrantes is not null && status == Status.Sobrantes) ?
                    $"Sobrantes:{Environment.NewLine}" + string.Join(Environment.NewLine, sobrantes) : (sobrantes is not null) ?

                    $"Desactualizadas:{Environment.NewLine}" + string.Join(Environment.NewLine, desactualizadas) +
                    $"{Environment.NewLine}Sobrantes:{Environment.NewLine}" + string.Join(Environment.NewLine, sobrantes)

                    : $"Desactualizadas:{Environment.NewLine}" + string.Join(Environment.NewLine, desactualizadas);

                Logger.Log(path, list);
            }

            return status;
        }
    }
}
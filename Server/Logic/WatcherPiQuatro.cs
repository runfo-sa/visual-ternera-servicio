using Core;
using System.Collections.Frozen;

namespace Server.Logic
{
    /// <summary>
    /// Observa la carpeta de Etiquetas en el servidor de Twins,
    /// para poder actualizar la lista en cuanto ocurra algun cambio.
    /// </summary>
    public class WatcherPiQuatro() : Watcher(SERVER_PATH, "*.e01")
    {
        public const string SERVER_PATH = "\\\\twinssrv\\Twins\\PiQuatro\\Etiquetas";

        public FrozenSet<Etiqueta> ServerEtiquetas = Scanner.GetEtiquetas(SERVER_PATH).ToFrozenSet();

        public override void UpdateList(Object sender, FileSystemEventArgs e)
        {
            ServerEtiquetas = Scanner.GetEtiquetas(SERVER_PATH).ToFrozenSet();
        }
    }
}
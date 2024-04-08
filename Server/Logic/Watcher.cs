using Core;

namespace Server.Logic
{
    /// <summary>
    /// Observa la carpeta de Etiquetas en el servidor de Twins,
    /// para poder actualizar la lista en cuanto ocurra algun cambio.
    /// 
    /// <br/>
    /// <br/>
    /// 
    /// <b>TO-FIX: Debido a como funciona la clase <see cref="FileSystemWatcher"/>,
    /// esto puede generar multiples actualizaciones por un solo cambio, no muy eficiente.</b>
    /// </summary>
    public class Watcher
    {
        private const string SERVER_PATH = "\\\\twinssrv\\Twins\\PiQuatro\\Etiquetas";

        private readonly FileSystemWatcher watcher = new()
        {
            Path = SERVER_PATH,
            Filter = "*.e01",
            EnableRaisingEvents = true,
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
        };

        public List<Etiqueta> ServerEtiquetas = Scanner.GetEtiquetas(SERVER_PATH);

        public Watcher()
        {
            watcher.Changed += UpdateList;
            watcher.Created += UpdateList;
            watcher.Deleted += UpdateList;
            watcher.Renamed += UpdateList;
            watcher.Error += (object sender, ErrorEventArgs e) => Console.Error.WriteLine(e.GetException());
        }

        private void UpdateList(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Updating server list...");
            ServerEtiquetas = Scanner.GetEtiquetas(SERVER_PATH);
        }
    }
}

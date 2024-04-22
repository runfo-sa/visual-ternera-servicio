namespace Server.Logic
{
    /// <summary>
    /// Clase abstracta que se encarga de revisar una carpeta para ejecutar la funcion implementada.
    /// <br/>
    /// <b>TO-FIX: Debido a como funciona la clase <see cref="FileSystemWatcher"/>,
    /// esto puede generar multiples actualizaciones por un solo cambio, no muy eficiente.</b>
    /// </summary>
    public abstract class Watcher
    {
        private readonly FileSystemWatcher watcher;

        public Watcher(string path, string filter)
        {
            watcher = new()
            {
                Path = path,
                Filter = filter,
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
            };

            watcher.Changed += UpdateList;
            watcher.Created += UpdateList;
            watcher.Deleted += UpdateList;
            watcher.Renamed += UpdateList;
            watcher.Error += (object sender, ErrorEventArgs e) => Console.Error.WriteLine(e.GetException());
        }

        public abstract void UpdateList(object sender, FileSystemEventArgs e);
    }
}
using Core;

namespace Client.Service
{
    public static class Reporter
    {
        private static readonly string FOLDER_PATH = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Visual Ternera Service\\Logs\\"
        );

        /// <summary>
        /// Escribe un <paramref name="msg"/> atravez del <see cref="Logger"/>, en la dirección AppData\VisualTerneraService.
        /// <br/>
        /// Abre el explorador de archivos con el log seleccionado.
        /// </summary>
        public static void ReportError(string msg, bool openFolder = true)
        {
            var file = Logger.Log(FOLDER_PATH, msg);
            if (openFolder)
            {
                System.Diagnostics.Process.Start("explorer.exe", string.Format("/select, \"{0}\"", file));
            }
        }
    }
}
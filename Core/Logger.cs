namespace Core
{
    public static class Logger
    {
        /// <summary>
        /// Registra el <paramref name="msg"/> en un archivo de formato <b>"yyyy_mm_dd.log"</b> dentro de <paramref name="path"/>
        /// </summary>
        /// <param name="path">Dirección en donde se creara el archivo .log</param>
        /// <param name="msg">Mensaje a guardar en el .log</param>
        /// <returns>Dirección al archivo .log</returns>
        public static string Log(string path, string msg)
        {
            DateTime date = DateTime.Now;

            var file = Path.Combine(path, date.ToString("yyyy_MM_dd") + ".log");
            Directory.CreateDirectory(path);

            string separator = new('-', 128);
            File.AppendAllText(file,
                string.Format(
                    "{0}\nError - {1}\n{2}\n{3}\n\n",
                    separator,
                    date.ToString("HH:mm:ss"),
                    separator,
                    msg
                )
            );

            return file;
        }
    }
}
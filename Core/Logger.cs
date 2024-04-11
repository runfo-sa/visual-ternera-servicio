namespace Core
{
    public static class Logger
    {
        public static string Log(string path, string msg)
        {
            DateTime date = DateTime.Now;
            var file = Path.Combine(path, date.ToString("yyyy_MM_dd") + ".log");

            Directory.CreateDirectory(path);

            string separator = new('-', 128);
            File.AppendAllText(file,
                string.Format("\n{0}\nError - {1}\n{2}\n{3}\n\n",
                separator, date.ToString("HH:mm:ss"), separator, msg)
            );

            return file;
        }
    }
}

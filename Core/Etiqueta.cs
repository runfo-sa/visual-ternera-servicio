namespace Core
{
    /// <summary>
    /// Registro de un archivo de etiquetas.
    /// </summary>
    /// <param name="Name">Especificación: En minusculas</param>
    /// <param name="Hash">Especificación: SHA256</param>
    /// <param name="Date">Especificación: dd/mm/yyyy hh:mm:ss</param>
    public readonly record struct Etiqueta(string Hash, string Date, string Name);

    public class EtiquetaCompareName : IEqualityComparer<Etiqueta>
    {
        public bool Equals(Etiqueta x, Etiqueta y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(Etiqueta et)
        {
            return et.Name.GetHashCode();
        }
    }
}

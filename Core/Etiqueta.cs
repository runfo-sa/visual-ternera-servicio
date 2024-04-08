namespace Core
{
    public record struct Etiqueta(string Name, string Hash, string Date)
    {
        public override readonly string ToString()
        {
            return Name + " - " + Hash + " - " + Date;
        }
    }
}

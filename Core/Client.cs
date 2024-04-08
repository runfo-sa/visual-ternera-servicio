using System.Text;

namespace Core
{
    public class Client(string Id, List<Etiqueta> Etiquetas)
    {
        public string Id { get; set; } = Id;

        public List<Etiqueta> Etiquetas { get; set; } = Etiquetas;

        public override String ToString()
        {
            StringBuilder sb = new("Cliente:\nIP:" + Id + "\nEtiquetas:\n");
            foreach (var et in Etiquetas)
            {
                sb.AppendLine(et.ToString());
            }
            return sb.ToString();
        }
    }
}

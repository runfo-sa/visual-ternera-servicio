using System.Text;

namespace Core
{
    public class Client(string Name, Etiqueta[] Etiquetas)
    {
        public string Name { get; set; } = Name;

        public Etiqueta[] Etiquetas { get; set; } = Etiquetas;

        public override String ToString()
        {
            StringBuilder sb = new("Cliente:\nIP:" + Name + "\nEtiquetas:\n");
            foreach (var et in Etiquetas)
            {
                sb.AppendLine(et.ToString());
            }
            return sb.ToString();
        }
    }
}

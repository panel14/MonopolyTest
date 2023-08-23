using System.Text;

namespace MonopolyTest.Domain.Models
{
    public class Box
    {
        public Guid Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Length { get; set; }
        public int Weigth { get; set; }
        public DateTime ProductionDate { get; set; }
        public Guid? PalleteId { get; set; }

        public Box() { }

        public static Box Parse(string boxString)
        {
            boxString = boxString.Remove(0, 1);
            boxString = boxString.Remove(boxString.Length - 1);

            string[] values = boxString.Split(",");
            Box box = new()
            {
                Id = Guid.Parse(values[0]),
                Width = int.Parse(values[1]),
                Height = int.Parse(values[2]),
                Length = int.Parse(values[3]),
                Weigth = int.Parse(values[4]),
                ProductionDate = DateTime.Parse(values[5]),
            };
            if (values.Length > 5)
            {
                box.PalleteId = Guid.Parse(values[6]);
            }
            return box;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append('(');
            stringBuilder.Append(Id + ", ");
            stringBuilder.Append(Width + ", ");
            stringBuilder.Append(Height + ", ");
            stringBuilder.Append(Length + ", ");
            stringBuilder.Append(Weigth + ", ");
            stringBuilder.Append(ProductionDate.ToString() + ", ");
            if (PalleteId !=  null)
            {
                stringBuilder.Append(PalleteId + ")");
            }
            else
            {
                stringBuilder.Append(')');
            }
            return stringBuilder.ToString();
        }
    }
}

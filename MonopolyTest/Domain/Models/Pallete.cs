using System.Text;

namespace MonopolyTest.Domain.Models
{
    public class Pallete
    {
        public Guid Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Length { get; set; }
        public int Weigth { get; set; }

        public DateTime ProductionDate { get; set; }

        public Pallete() { }

        public static Pallete Parse(string palleteString)
        {
            palleteString = palleteString.Remove(0);
            palleteString = palleteString.Remove(palleteString.Length - 1);

            string[] values = palleteString.Split(',');
            Pallete pallete = new()
            {
                Id = Guid.Parse(values[0]),
                Width = int.Parse(values[1]),
                Height = int.Parse(values[2]),
                Length = int.Parse(values[3]),
                Weigth = int.Parse(values[4])
            };
            return pallete;
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
            stringBuilder.AppendFormat("yyyy-MM-dd", ProductionDate.ToString() + ")");
            return stringBuilder.ToString();
        }
    }
}

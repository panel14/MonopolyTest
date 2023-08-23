using MonopolyTest.Domain.Models;

namespace MonopolyTest.DataUtils
{
    public static class DataGenerator
    {
        private static readonly int PalleteLimit = 150;
        private static readonly int BoxLimit = 100;

        private static DateTime GenerateDate()
        {
            DateTime start = new(2011, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(new Random().Next(range));
        }

        private static Box GenerateBox(Guid? palleteId)
        {
            Random random = new();

            return new Box()
            {
                Id = Guid.NewGuid(),
                Width = random.Next(1, BoxLimit),
                Height = random.Next(1, BoxLimit),
                Length = random.Next(1, BoxLimit),
                Weigth = random.Next(1, BoxLimit),
                ProductionDate = GenerateDate(),
                PalleteId = palleteId
            };
        }

        private static Pallete GeneratePallete()
        {
            Random random = new();

            return new Pallete() 
            {
                Id = Guid.NewGuid(),
                Width = random.Next(1, PalleteLimit),
                Height = random.Next(1, PalleteLimit),
                Length = random.Next(1, PalleteLimit),
                Weigth = 30,
                ProductionDate = DateTime.MinValue,
            };
        }

        public static StorageCollection GenerateStorage(int palletesNums = 3, int boxNums = 10)
        {
            StorageCollection storage = new();
            List<Guid> palleteIds = new();
            for (int i = 0; i < palletesNums; i++)
            {
                Pallete pallete = GeneratePallete();
                palleteIds.Add(pallete.Id);
                storage.AddPallete(pallete);
            }

            for (int i = 0; i < boxNums; i++)
            {
                Guid curPallete = palleteIds[i % palletesNums];
                storage.AddBoxToPallette(GenerateBox(curPallete), curPallete);
            }

            return storage;
        }
    }
}

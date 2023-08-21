namespace MonopolyTest.Domain.Models
{
    public class StorageCollection
    {
        public List<Pallete> Palletes { get; private set; } 
        public List<Box> Boxes { get; private set; }

        public StorageCollection(List<Pallete> palletes, List<Box> boxes) 
        {
            Palletes = palletes;
            Boxes = boxes;
        }

        public StorageCollection() 
        {
            Palletes = new();
            Boxes = new();
        }

        private DateTime CountPalleteProductionDate(Guid palleteId)
        {
            return Boxes.Where(x => x.PalleteId == palleteId).Select(x => GetBoxBBDate(x.Id)).Min();
        }

        public DateTime GetBoxBBDate(Guid boxId)
        {
            Box? box = Boxes.FirstOrDefault(x => x.Id == boxId);
            if (box != null)
            {
                return box.ProductionDate + new TimeSpan(100, 0, 0, 0);
            }
            return DateTime.Now;
        }

        public DateTime GetPalleteBBDate(Guid palleteId)
        {
            Pallete? pallete = Palletes.FirstOrDefault(x => x.Id == palleteId);
            if (pallete != null)
            {
                return pallete.ProductionDate + new TimeSpan(100, 0, 0, 0);
            }
            return DateTime.Now;
        }

        public int GetBoxVolume(Guid boxId)
        {
            Box? box = Boxes.FirstOrDefault(x => x.Id == boxId);
            if (box != null)
            {
                return box.Width * box.Height * box.Length;
            }
            return 0;
        }

        public int GetPalleteVolume(Guid palleteId)
        {
            return Boxes.Where(x => x.PalleteId == palleteId).Select(x => x.Width * x.Height * x.Length).Sum();
        }

        public void AddPallete(Pallete pallete)
        {
            Palletes.Add(pallete);
        }

        private void UpdatePallete(Pallete pallete)
        {
            Pallete? toDelete = Palletes.FirstOrDefault(x => x.Id == pallete.Id);
            if (toDelete != null)
            {
                Palletes.Remove(toDelete);
                Palletes.Add(pallete);
            }
        }

        public int CountPalleteWeigth(Guid palleteId)
        {
            return Boxes.Where(x => x.PalleteId == palleteId).Select(x => x.Weigth).Sum();
        }

        public void AddBoxToPallette(Box box, Guid palleteId)
        {
            Pallete? pallete = Palletes.FirstOrDefault(x => x.Id == palleteId);
            if (pallete != null)
            {
                if (box.Width > pallete.Width ||  box.Height > pallete.Height)
                {
                    return;
                }
                Boxes.Add(box);

                pallete.Weigth += box.Weigth;
                int correctWeigth = CountPalleteWeigth(palleteId);
                if (pallete.Weigth != correctWeigth)
                {
                    pallete.Weigth = correctWeigth;
                }

                pallete.ProductionDate = CountPalleteProductionDate(palleteId);
                UpdatePallete(pallete);
            }
        }
    }
}

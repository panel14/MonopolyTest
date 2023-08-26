using MonopolyTest.Domain.Models;

namespace MonopolyTest.DataUtils
{
    public static class DataPresenter
    {
        public static void ShowBox(StorageCollection storage, Guid boxId)
        {
            Box? box = storage.Boxes.FirstOrDefault(x => x.Id == boxId);
            if (box != null)
            {
                Console.WriteLine("-----------------------");
                Console.WriteLine("{0, -25} {1, 5}", "ID", box.Id);
                Console.WriteLine("{0, -25} {1, 5}", "ШИРИНА", box.Width);
                Console.WriteLine("{0, -25} {1, 5}", "ГЛУБИНА", box.Height);
                Console.WriteLine("{0, -25} {1, 5}", "ВЫСОТА", box.Length);
                Console.WriteLine("{0, -25} {1, 5}", "Вес", box.Weigth);
                Console.WriteLine("{0, -25} {1, 5}", "ОБЪЕМ", storage.GetBoxVolume(box.Id));
                Console.WriteLine("{0, -25} {1, 5}", "ДАТА ПРОИЗВОДСТВА", box.ProductionDate.ToString("yyyy-MM-dd"));
                Console.WriteLine("{0, -25} {1, 5}", "СРОК ГОДНОСТИ", storage.GetBoxBBDate(box.Id).ToString("yyyy-MM-dd"));
                if (box.PalleteId != null)
                {
                    Console.WriteLine("{0, -25} {1, 5}", "СТОИТ НА ПАЛЛЕТЕ", box.PalleteId);
                }
                else
                {
                    Console.WriteLine("Не стоит ни накакой паллете.");
                }
                Console.WriteLine();
            }
        }

        public static void ShowPallete(StorageCollection storage, Guid palleteId)
        {
            Pallete? pallete = storage.Palletes.FirstOrDefault(x => x.Id == palleteId);
            if (pallete != null)
            {
                Console.WriteLine("-----------------------");
                Console.WriteLine("{0, -25} {1, 5}", "ID", pallete.Id);
                Console.WriteLine("{0, -25} {1, 5}", "ШИРИНА", pallete.Width);
                Console.WriteLine("{0, -25} {1, 5}", "ГЛУБИНА", pallete.Height);
                Console.WriteLine("{0, -25} {1, 5}", "ВЫСОТА", pallete.Length);
                Console.WriteLine("{0, -25} {1, 5}", "Вес", pallete.Weigth);
                Console.WriteLine("{0, -25} {1, 5}", "ОБЪЕМ", storage.GetPalleteVolume(pallete.Id));
                Console.WriteLine("{0, -25} {1, 5}", "ДАТА ПРОИЗВОДСТВА", pallete.ProductionDate.ToString("yyyy-MM-dd"));
                Console.WriteLine("{0, -25} {1, 5}", "СРОК ГОДНОСТИ", storage.GetPalleteBBDate(pallete.Id).ToString("yyyy-MM-dd"));

                Console.WriteLine();
            }
        }

        public static void ShowStorage(StorageCollection storage)
        {
            Console.WriteLine("СКЛАД:");

            int i = 0;
            int j = 0;
            foreach (Pallete pallete in storage.Palletes)
            {
                Console.WriteLine("ПАЛЛЕТА {0}:", i);
                ShowPallete(storage, pallete.Id);
                i++;

                List<Box> boxes = storage.Boxes.Where(x => x.PalleteId == pallete.Id).ToList();
                Console.WriteLine("Коробок на паллете {0}:", boxes.Count);

                int weight = 0;
                int volume = 0;
                DateTime min = DateTime.MinValue;
                foreach (Box box in boxes)
                {
                    Console.WriteLine("КОРОБКА {0}:", j);
                    ShowBox(storage, box.Id);
                    weight += box.Weigth;
                    volume += storage.GetBoxVolume(box.Id);
                    if (box.ProductionDate > min)
                    {
                        min = box.ProductionDate;
                    }

                    j++;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Общий вес {0} коробок: {1}. Вес паллеты: {2}", j, weight, pallete.Weigth);
                Console.WriteLine("Объем {0} коробок: {1}. Объём паллеты: {2}", j, volume, storage.GetPalleteVolume(pallete.Id));
                Console.WriteLine("Минимальная дата производства из коробок: {0}. Дата производства палеты: {1}", min.ToString("yyy-MM-dd"), pallete.ProductionDate.ToString("yyyy-MM-dd"));
                Console.ForegroundColor = ConsoleColor.White;

                j = 0;
                Console.WriteLine();
            }

            var missedBoxes = storage.Boxes.Where(x => x.PalleteId == null);
            Console.WriteLine("Коробки без паллет:");

            i = 0;
            foreach(Box box in missedBoxes)
            {
                Console.WriteLine("ПОТЕРЯННАЯ КОРОБКА {0}", i);
                ShowBox(storage, box.Id);
            }
        }
    }
}

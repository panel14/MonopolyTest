
using MonopolyTest.Domain.Models;

namespace MonopolyTest.DataUtils
{
    public static class DataSorter
    {
        public static List<Guid> SortSetByBBDate(StorageCollection storage, int numsOfPallete = 3)
        {
            var sorted = storage.Boxes.OrderByDescending(x => storage.GetBoxBBDate(x.Id)).ToList();
            List<Guid> palletesIds = new();

            int i = 0;
            while (palletesIds.Count < numsOfPallete && i < sorted.Count)
            {
                Guid current = sorted[i].PalleteId ?? Guid.Empty;
                if (current != Guid.Empty && !palletesIds.Contains(current))
                {
                    palletesIds.Add(current);
                }
                i++;
            }

            var palletes = storage.Palletes.Where(x => palletesIds.Contains(x.Id))
                .OrderBy(x => storage.GetPalleteVolume(x.Id))
                .Select(x => x.Id).ToList();

            return palletes;
        }

        public static IEnumerable<IOrderedEnumerable<Pallete>> SortSetByBBDateGroup(StorageCollection storage)
        {
            return storage.Palletes.GroupBy(x => x.ProductionDate.Month).Select(group => group.OrderBy(x => x.Weigth));
        }
    }
}

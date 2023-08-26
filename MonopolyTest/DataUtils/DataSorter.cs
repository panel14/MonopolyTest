
using MonopolyTest.Domain.Models;

namespace MonopolyTest.DataUtils
{
    public static class DataSorter
    {
        public static IEnumerable<IOrderedEnumerable<Pallete>> SortSetByBBDateGroup(StorageCollection storage)
        {
            return storage.Palletes.GroupBy(x => storage.GetPalleteBBDate(x.Id).Month).Select(group => group.OrderBy(x => x.Weigth));
        }

        public static List<Guid> SortSetByBBDate(StorageCollection storage, int numsOfPallete = 3)
        {
            var sorted = storage.Boxes.OrderBy(x => storage.GetBoxBBDate(x.Id));
            
            List<Guid> guids = new();
            foreach (var box in sorted)
            {
                if (guids.Count == numsOfPallete)
                {
                    break;
                }
                if (box.PalleteId != null && !guids.Contains(box.Id))
                {
                    guids.Add((Guid)box.PalleteId);
                }
            }

            return storage.Palletes.Where(x => guids.Contains(x.Id))
                .OrderBy(x => storage.GetPalleteVolume(x.Id)).Select(x => x.Id)
                .ToList();
        }
    }
}

using MonopolyTest.Domain.Models;
using MonopolyTest.Utils;
using System.Data.SqlClient;
using System.Text;

namespace MonopolyTest.DataReader
{
    public static class DataReader
    {
        private static readonly string connetionFilename = "connection.txt";
        //TODO: Add file format checks
        public static StorageCollection ReadDataFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            List<Pallete> palletes = new();
            List<Box> boxes = new();

            List<string> strings = File.ReadAllLines(filePath).ToList();
            strings.RemoveAt(0);

            int boxIndex = strings.IndexOf("Boxes:");

            for (int i = 0; i < boxIndex; i++)
            {
                palletes.Add(Pallete.Parse(strings[i]));
            }
            for (int i = boxIndex + 1; i < strings.Count; i++)
            {
                boxes.Add(Box.Parse(strings[i]));
            }

            return new StorageCollection(palletes, boxes);
        }

        public static void SaveDataToFile(StorageCollection storage, string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            StringBuilder sb = new("Pallets:\n");
            foreach (Pallete pallete in storage.Palletes)
            {
                sb.AppendLine(pallete.ToString());
            }
            sb.AppendLine("Boxes:");

            foreach (Box box in storage.Boxes)
            {
                sb.AppendLine(box.ToString());
            }
            File.AppendAllText(filePath, sb.ToString());
        }

        public static StorageCollection ReadDataFromDB(string connectionString, string palleteTable = "Pallete", string boxTable = "Box")
        {
            List<Box> boxes = new();
            List<Pallete> palletes = new();

            using SqlConnection connection = new(connectionString);
            connection.OpenAsync();

            using SqlCommand getPallets = new()
            {
                Connection = connection,
                CommandText = $"select * from {palleteTable}",
            };
            using SqlDataReader palleteReader = getPallets.ExecuteReader();

            while (palleteReader.Read())
            {
                Pallete pallete = new()
                {
                    Id = palleteReader.GetGuid(0),
                    Width = palleteReader.GetInt32(1),
                    Height = palleteReader.GetInt32(2),
                    Length = palleteReader.GetInt32(3),
                    Weigth = palleteReader.GetInt32(4),
                    ProductionDate = palleteReader.GetDateTime(5),
                };
                palletes.Add(pallete);
            }

            using SqlCommand getBoxes = new()
            {
                Connection = connection,
                CommandText = $"select * from {boxTable}"
            };
            using SqlDataReader boxesReader = getBoxes.ExecuteReader();

            while (boxesReader.Read())
            {
                Box box = new()
                {
                    Id = boxesReader.GetGuid(0),
                    Width = boxesReader.GetInt32(1),
                    Height = boxesReader.GetInt32(2),
                    Length = boxesReader.GetInt32(3),
                    Weigth = boxesReader.GetInt32(4),
                    PalleteId = boxesReader.SafeGetGuid(5),
                    ProductionDate = boxesReader.GetDateTime(6),
                };
                boxes.Add(box);
            }

            return new StorageCollection(palletes, boxes);
        }

        public static void SaveDataToDB(StorageCollection collection, string connectionString, string palleteTable = "Pallete", string boxTable = "Box")
        {
            using SqlConnection connection = new(connectionString);
            connection.OpenAsync();

            StringBuilder insertBoxes = new("begin transaction;");
            foreach (Box box in collection.Boxes)
            {
                insertBoxes.Append($"insert into {boxTable} values");
                insertBoxes.AppendLine(box.ToString() + ';');
            }
            insertBoxes.AppendLine("commit;");

            using SqlCommand saveBoxes = new()
            {
                Connection = connection,
                CommandText = insertBoxes.ToString(),
            };
            saveBoxes.ExecuteNonQuery();

            StringBuilder insertPalletes = new("begin transaction;");
            foreach (Pallete pallete in collection.Palletes)
            {
                insertPalletes.Append($"insert into {palleteTable} values");
                insertPalletes.AppendLine(pallete.ToString() + ';');
            }
            insertPalletes.AppendLine("commit;");

            using SqlCommand savePalletes = new()
            {
                Connection = connection,
                CommandText = insertPalletes.ToString()
            };
            savePalletes.ExecuteNonQuery();
        }

        public static void GetIntValueFromUser(out int value, string message, string error, Predicate<int> pred)
        {
            Console.WriteLine(message);
            while (!int.TryParse(Console.ReadLine(), out value) || !pred(value))
            {
                Console.WriteLine(error);
            }
        }

        private static void GetDateValueFromUser(out DateTime value, string message, string error)
        {
            Console.WriteLine(message);
            while (!DateTime.TryParse(Console.ReadLine(), out value))
            {
                Console.WriteLine(error);
            }
        }

        public static Pallete ReadPalleteFromUser()
        {
            int intFields = 3;

            int[] values = new int[intFields];
            List<string> messages = new()
            {
                "Введите ширину паллеты:",
                "Введите высоту паллеты:",
                "Введите длину паллеты:",
            };
            List<string> errors = new()
            {
                "Неверный формат ввода. Ширина должна быть целым числом больше 0",
                "Неверный формат ввода. Высота должна быть целым числом больше 0",
                "Неверный формат ввода. Длина должна быть целым числом больше 0",
            };

            for (int i = 0; i < intFields; i++)
            {
                GetIntValueFromUser(out values[i], messages[i], errors[i], x => x > 0);
            }

            return new Pallete() 
            {
                Id = Guid.NewGuid(),
                Width = values[0],
                Height = values[1],
                Length = values[2],
                Weigth = 30,
                ProductionDate = DateTime.MinValue,
            };
        }

        public static Box ReadBoxFromUser(List<Pallete> palletes)
        {
            int intFields = 4;
            int[] values = new int[intFields];
            List<string> messages = new()
            {
                "Введите ширину коробки:",
                "Введите высоту коробки:",
                "Введите длину коробки:",
                "Введите вес коробки:"
            };
            List<string> errors = new()
            {
                "Неверный формат ввода. Ширина должна быть целым числом больше 0",
                "Неверный формат ввода. Высота должна быть целым числом больше 0",
                "Неверный формат ввода. Длина должна быть целым числом больше 0",
                "Неверный формат ввода. Вес должен быть целым числом больше 0"
            };

            for (int i = 0; i < intFields; i++)
            {
                GetIntValueFromUser(out values[i], messages[i], errors[i], x => x > 0);
            }
            GetDateValueFromUser(out DateTime prod, "Введите дату производства коробки:", "Неверный формат ввода. Формат: дд.мм.гггг");

            Dictionary<int, Guid?> palletsIds = new();
            int j;
            for (j = 0; j < palletes.Count; j++)
            {
                palletsIds.Add(j + 1, palletes[j].Id);
            }
            j++;
            Console.WriteLine("Введите паллету (Id) на которую будет вложена коробка. Список доступных коробок:");
            foreach(KeyValuePair<int, Guid?> pair in palletsIds)
            {
                Console.WriteLine(pair.Key + " : " + pair.Value);
            }
            string message = $"Введите номер соответствующего Id:";
            GetIntValueFromUser(out int index, message, "Неверный формат ввода.", x => x > 0 && x < j);

            return new Box() 
            {
                Id = Guid.NewGuid(),
                Width = values[0],
                Height = values[1],
                Length = values[2],
                Weigth = values[3],
                ProductionDate = prod,
                PalleteId = palletsIds[index],
            };
        }

        public static void SaveConnectionsStrings(string connectionString)
        {
            File.AppendAllText(connetionFilename, connectionString + "\n");
        }

        public static Dictionary<int, string> GetConnectionsStrings()
        {
            string[] strings = File.ReadAllLines(connetionFilename);
            Dictionary<int, string> pairs = new();

            foreach (string s in strings)
            {
                string[] pair = s.Split(':');
                pairs.Add(int.Parse(pair[0]), pair[1].Trim());
            }
            return pairs;
        }
    }
}

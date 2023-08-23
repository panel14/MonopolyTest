using MonopolyTest.Domain.Models;
using MonopolyTest.Utils;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MonopolyTest.DataReader
{
    public enum SaveType
    {
        FILEPATH,
        CONNECTION_STRING,
    }

    public static class DataReader
    {
        private static readonly string configFilename = Path.Combine(Environment.CurrentDirectory, "config.txt");
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

        public static void SaveDataToDB(StorageCollection collection, string connectionString, string palleteTable = "Pallete", string boxTable = "Box", bool needTruncate = false)
        {
            using SqlConnection connection = new(connectionString);
            connection.OpenAsync();

            if (needTruncate)
            {
                using SqlCommand truncate = connection.CreateCommand();
                using SqlTransaction transaction = connection.BeginTransaction();

                truncate.Connection = connection;
                truncate.Transaction = transaction;

                truncate.CommandText = $"truncate table {palleteTable}";
                truncate.ExecuteNonQuery();

                truncate.CommandText = $"truncate table {boxTable}";
                truncate.ExecuteNonQuery();

                transaction.Commit();
            }


            using SqlCommand saveBoxes = connection.CreateCommand();
            using SqlTransaction saveBoxesTrans = connection.BeginTransaction();

            saveBoxes.Connection = connection;
            saveBoxes.Transaction = saveBoxesTrans;

            foreach (Box box in collection.Boxes)
            {
                saveBoxes.CommandText = $"insert into {boxTable} values {box}";
                saveBoxes.ExecuteNonQuery();
            }
            saveBoxesTrans.Commit();

            using SqlCommand savePalletes = connection.CreateCommand();
            using SqlTransaction savePalletesTrans = connection.BeginTransaction();

            savePalletes.Connection = connection;
            savePalletes.Transaction = savePalletesTrans;

            foreach (Pallete pallete in collection.Palletes)
            {
                savePalletes.CommandText = $"insert into {palleteTable} values {pallete}";
                savePalletes.ExecuteNonQuery();
            }
            savePalletesTrans.Commit();
        }

        public static void GetIntValueFromUser(out int value, string message, string error, Predicate<int> pred)
        {
            Console.WriteLine(message);
            while (!int.TryParse(Console.ReadLine(), out value) || !pred(value))
            {
                Console.WriteLine(error);
            }
        }

        public static void GetStringValueFromUser(out string value, string message, string error, string pattern = @".+")
        {
            Regex regex = new(pattern);

            Console.WriteLine(message);
            while (true)
            {
                value = Console.ReadLine();
                if (value != null && regex.IsMatch(value))
                {
                    return;
                }
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
            string message = $"Введите номер соответствующего Id: (введите \"0\" чтобы оставить поле пустым)";
            GetIntValueFromUser(out int index, message, "Неверный формат ввода.", x => x >= 0 && x < j);

            return new Box() 
            {
                Id = Guid.NewGuid(),
                Width = values[0],
                Height = values[1],
                Length = values[2],
                Weigth = values[3],
                ProductionDate = prod,
                PalleteId = (index > 0) ? palletsIds[index] : null,
            };
        }

        private static void SaveConfigurationStrings(string str, SaveType type)
        {
            List<string> lines = File.ReadAllLines(configFilename).ToList();
            
            foreach (string line in Enum.GetNames(typeof(SaveType)))
            {
                if (!lines.Contains(line))
                {
                    lines.Add(line);
                }
            }

            lines.Insert(lines.IndexOf(type.ToString()) + 1, str);
            File.WriteAllLines(configFilename, lines);
        }

        private static Dictionary<int, string> GetConfigurationStrings(SaveType type)
        {
            Dictionary<int, string> pairs = new();

            List<string> strings = File.ReadAllLines(configFilename).ToList();
            int midIndex = strings.IndexOf(SaveType.CONNECTION_STRING.ToString());

            int startIndex = 0;
            int endIndex = 0;

            switch (type)
            {
                case SaveType.CONNECTION_STRING:
                    startIndex = midIndex + 1;
                    endIndex = strings.Count;
                    break;
                case SaveType.FILEPATH:
                    startIndex = 1;
                    endIndex = midIndex;
                    break;
                default:
                    break;
            }

            int index = 0;
            for (int i = startIndex; i < endIndex; i++)
            {
                pairs.Add(index, strings[i].Trim());
                index++;
            }
            return pairs;
        }

        private static string GetAndSaveUserString(string message, string error, SaveType saveType)
        {
            GetStringValueFromUser(out string newLine, message, error);

            string saveMessage = "Сохранить строку?[y/n]:";
            GetStringValueFromUser(out string answer, saveMessage, saveMessage, "^[yn]{1}$");

            if (answer.Equals("y"))
            {
                SaveConfigurationStrings(newLine, saveType);
            }

            return newLine;
        }

        public static string GetUserStringWithSave(string name, SaveType saveType)
        {
            Dictionary<int, string> strings = GetConfigurationStrings(saveType);
            string newLine;
            if (strings.Count > 0)
            {
                Console.WriteLine($"Имеются сохраненные {name}");
                foreach (KeyValuePair<int, string> pair in strings)
                {
                    Console.WriteLine(pair.Key + " : " + pair.Value);
                }
                Console.WriteLine(strings.Count + " : Ввести новую строку");
                Console.WriteLine("Выберите нужную опцию:");

                int index;
                while (!int.TryParse(Console.ReadLine(), out index) || index < 0 || index > strings.Count)
                {
                    Console.WriteLine("Неверный формат ввода. Повторите попытку:");
                }
                if (index == strings.Count)
                {
                    newLine = GetAndSaveUserString("Введите строку", "Строка не должна быть пустой", saveType);
                }
                else
                {
                    newLine = strings[index];
                }
            }
            else
            {
                newLine = GetAndSaveUserString("Введите строку", "Строка не должна быть пустой", saveType);
            }
            return newLine;
        }
    }
}

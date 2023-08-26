using MonopolyTest.Domain.Models;
using MonopolyTest.Utils;
using System.Data.SqlClient;
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
        private static readonly string ConfigFilename = Path.Combine(Environment.CurrentDirectory, "config.txt");
        private static readonly string ScriptFilename = Path.Combine(Environment.CurrentDirectory, "createScript.sql");

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

            File.WriteAllText(filePath, string.Empty);

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

        private static bool IsCorrectTableName(string? tableName)
        {
            Regex regex = new("(^[A-Z]{1}[a-z]+)?");
            return tableName == null || !regex.IsMatch(tableName);
        }

        public static StorageCollection ReadDataFromDB(string connectionString, string? palleteTable, string? boxTable)
        {
            palleteTable = IsCorrectTableName(palleteTable) ? palleteTable : "Pallete";
            boxTable = IsCorrectTableName(boxTable) ? boxTable : "Box";

            List<Box> boxes = new();
            List<Pallete> palletes = new();

            using SqlConnection connection = new(connectionString);
            connection.Open();

            using SqlCommand getPallets = new()
            {
                Connection = connection,
                CommandText = $"select * from {palleteTable}",
            };
            using (SqlDataReader palleteReader = getPallets.ExecuteReader())
            {
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
            }

            using SqlCommand getBoxes = new()
            {
                Connection = connection,
                CommandText = $"select * from {boxTable}"
            };
            using (SqlDataReader boxesReader = getBoxes.ExecuteReader())
            {
                while (boxesReader.Read())
                {
                    Box box = new()
                    {
                        Id = boxesReader.GetGuid(0),
                        Width = boxesReader.GetInt32(1),
                        Height = boxesReader.GetInt32(2),
                        Length = boxesReader.GetInt32(3),
                        Weigth = boxesReader.GetInt32(4),
                        ProductionDate = boxesReader.GetDateTime(5),
                        PalleteId = boxesReader.SafeGetGuid(6),
                    };
                    boxes.Add(box);
                }
            }

            return new StorageCollection(palletes, boxes);
        }

        public static void SaveDataToDB(StorageCollection collection, string connectionString,
            string? palleteTable, string? boxTable, bool needTruncate = false, bool needCreate = false)
        {
            palleteTable = IsCorrectTableName(palleteTable) && !needCreate ? palleteTable : "Pallete";
            boxTable = IsCorrectTableName(boxTable) && !needCreate ? boxTable : "Box";

            using SqlConnection connection = new(connectionString);
            connection.Open();

            if (needCreate)
            {
                using SqlCommand create = new()
                {
                    Connection = connection,
                    CommandText = File.ReadAllText(ScriptFilename)
                };
                create.ExecuteNonQuery();
            }

            if (!needCreate && needTruncate)
            {
                StringBuilder sb = new("begin transaction;");
                foreach (string s in new List<string> { palleteTable, boxTable })
                {
                    sb.Append($"truncate table {s};");
                }
                sb.Append("commit;");
                SqlCommand truncate = new()
                {
                    Connection = connection,
                    CommandText = sb.ToString(),
                };
                truncate.ExecuteNonQuery();
            }

            StringBuilder sbb = new ("begin transaction;");
            foreach (Box box in collection.Boxes)
            {
                sbb.Append($"insert into {boxTable} values {box};");
            }
            sbb.Append("commit;");

            SqlCommand saveBoxes = new()
            {
                Connection = connection,
                CommandText = sbb.ToString()
            };
            saveBoxes.ExecuteNonQuery();

            StringBuilder sbp = new("begin transaction;");
            foreach (Pallete pallete in collection.Palletes)
            {
                sbp.Append($"insert into {palleteTable} values {pallete};");
            }
            sbp.Append("commit;");

            SqlCommand savePalletes = new()
            {
                Connection = connection,
                CommandText = sbp.ToString()
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
            var sorted = palletes.Where(p => p.Width > values[0] && p.Height > values[1]).ToList();
            int j;
            for (j = 0; j < sorted.Count; j++)
            {
                palletsIds.Add(j + 1, sorted[j].Id);
            }
            j++;
            Console.WriteLine("Введите паллету (Id) на которую будет вложена коробка. Список доступных коробок:");
            if (palletsIds.Count == 0)
            {
                foreach (KeyValuePair<int, Guid?> pair in palletsIds)
                {
                    Console.WriteLine(pair.Key + " : " + pair.Value);
                }
            }
            else
            {
                Console.WriteLine("Нет подходящих паллет (ширина или высота коробки превышают аналогичные параметры паллет, либо на складе нет паллет.)");
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
                ProductionDate = prod.Date,
                PalleteId = (index > 0) ? palletsIds[index] : null,
            };
        }

        private static void SaveConfigurationStrings(string str, SaveType type)
        {
            List<string> lines = File.ReadAllLines(ConfigFilename).ToList();
            
            foreach (string line in Enum.GetNames(typeof(SaveType)))
            {
                if (!lines.Contains(line))
                {
                    lines.Add(line);
                }
            }

            lines.Insert(lines.IndexOf(type.ToString()) + 1, str);
            File.WriteAllLines(ConfigFilename, lines);
        }

        private static Dictionary<int, string> GetConfigurationStrings(SaveType type)
        {
            Dictionary<int, string> pairs = new();

            List<string> strings = File.ReadAllLines(ConfigFilename).ToList();
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

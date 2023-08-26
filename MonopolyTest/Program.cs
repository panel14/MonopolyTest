using MonopolyTest.DataReader;
using MonopolyTest.DataUtils;
using MonopolyTest.Domain.Models;

string infoDataCommands = "Ввести данные:\n" +
    "h  - вручную\n" +
    "g  - сгенерировать набор данных\n" +
    "f - прочитать набор из файла\n" +
    "db - подключится к существующей базе данных\n" +
    "sh - посмотреть всю коллекцию\n" +
    "info - вывести информацию по текущим командам\n" +
    "next - закончить набор данных";

Console.WriteLine("Добро пожаловать! Введите одну из команд:");
Console.WriteLine(infoDataCommands);

string wrongFormat = "Неизвестная команда. Повторите ввод";
string? command;

StorageCollection storageCollection = new();

while ((command = Console.ReadLine()) != "next")
{
    switch (command)
    {
        case "h":
            List<Box> boxes = new();
            List<Pallete> palletes = new();
            string curOptions = "Что хотите добавить?\np - добавить паллету\nb - добавить коробку\nend - закончить ввод";
            Console.WriteLine(curOptions);
            string? handCommand;

            while ((handCommand = Console.ReadLine()) != "end")
            {
                switch (handCommand)
                {
                    case "p":
                        Pallete pallete = DataReader.ReadPalleteFromUser();
                        palletes.Add(pallete);
                        Console.WriteLine("Паллета добавлена.");
                        break;
                    case "b":
                        Box box = DataReader.ReadBoxFromUser(palletes);
                        boxes.Add(box);
                        Console.WriteLine("Коробка добавлена.");
                        break;
                    default:
                        Console.WriteLine(wrongFormat);
                        break;
                }
                Console.WriteLine(curOptions);
            }

            storageCollection = new StorageCollection(palletes, boxes);

            break;

        case "g":
            DataReader.GetIntValueFromUser(out int pNums, "Введите количество палет:", "Неверный формат. Введите целое число больше 0", x => x > 0);
            DataReader.GetIntValueFromUser(out int bNums, "Введите количество коробок:", "Неверный формат. Введите целое число больше 0", x => x > 0);

            storageCollection = DataGenerator.GenerateStorage(pNums, bNums);
            Console.WriteLine("Коллеция сгенерирована.");
            break;
        case "f":
            Console.WriteLine("Введите полное имя файла (абсолютный путь):");
            string? filePath = DataReader.GetUserStringWithSave("имена файлов", SaveType.FILEPATH);
            try
            {
                storageCollection = DataReader.ReadDataFromFile(filePath);
                Console.WriteLine("Коллеция прочитана.");
            }
            catch(FileNotFoundException)
            {
                Console.WriteLine("Указанного файла не существует.");
            }

            break;

        case "db":
            Console.WriteLine("Введите строку подключения:");
            string newLine = DataReader.GetUserStringWithSave("строки подключения", SaveType.CONNECTION_STRING);

            Console.WriteLine("Введите название таблицы с паллетами. При пустом вводе будет использовано значение по умолчанию (Pallete):");
            string? palleteTable = Console.ReadLine();

            Console.WriteLine("Введите название таблицы с коробками. При пустом вводе будет использовано значение по умолчанию (Box):");
            string? boxTable = Console.ReadLine();

            try
            {
                storageCollection = DataReader.ReadDataFromDB(newLine, palleteTable, boxTable);
                Console.WriteLine("Коллеция прочитана.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: {0}.\n Попробуйте ещё раз.", ex.Message);
            }

            break;
        case "sh":
            DataPresenter.ShowStorage(storageCollection);
            break;
        case "info":
            Console.WriteLine(infoDataCommands);
            break;
        case "next":
            break;
        default: 
            Console.WriteLine(wrongFormat); 
            break;
    }
    Console.WriteLine("Введите команду:");
}

string infoOptions = "Выберите опцию:\n"
    + "sh - посмотреть всю коллекцию\n"
    + "sf - сохранить коллецию в файл\n"
    + "sdb - сохранить коллекцию в базу данных\n"
    + "info - вывести информацию по опциям\n"
    + "exit - выйти из приложения\n\n"
    + "Функции вывода на экран по заданию:\n"
    + "t1 - группировка палет по сроку годности\n"
    + "t2 - сортировка паллет по возрастанию объема\n"
    + "sht - показать задания полностью";

Console.WriteLine(infoOptions);

string? option;

while ((option = Console.ReadLine()) != "exit")
{
    switch(option)
    {
        case "sh":
            DataPresenter.ShowStorage(storageCollection);
            break;
        case "sf":
            Console.WriteLine("Введите полное имя файла (абсолютный путь):");
            string filePath = DataReader.GetUserStringWithSave("имена файлов", SaveType.FILEPATH);
            try
            {
                DataReader.SaveDataToFile(storageCollection, filePath);
                Console.WriteLine("Коллекция сохранена.");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Указанного файла не существует.");
            }

            break;
        case "sdb":
            Console.WriteLine("Введите строку подключения:");
            string newLine = DataReader.GetUserStringWithSave("строки подключения", SaveType.CONNECTION_STRING);

            string createMessage= "Создать таблицы перед добавлением данных?[y/n]\n" +
                   "При согласии, будет принята попытка создать таблицы паллет и коробок с именами Pallete и Box (CREATE TABLE...)";
            DataReader.GetStringValueFromUser(out string cAnswer, createMessage, "Создать таблицы перед добавлением данных?[y/n]", "^[yn]{1}$");
            string? palleteTable = null;
            string? boxTable = null;

            if (cAnswer.Equals("n"))
            {
                Console.WriteLine("Введите название таблицы с паллетами. При пустом вводе будет использовано значение по умолчанию (Pallete):");
                palleteTable = Console.ReadLine();

                Console.WriteLine("Введите название таблицы с коробками. При пустом вводе будет использовано значение по умолчанию (Box):");
                boxTable = Console.ReadLine();
            }

            string truncateMessage = "Очистить таблицей перед сохранением?[y/n]\nПри согласии, будет принята попытка очистить таблицы паллет и коробок (TRUNCATE TABLE...)";
            DataReader.GetStringValueFromUser(out string tAnswer, truncateMessage, "Очистить таблицей перед сохранением?[y/n]", "^[yn]{1}$");
            try
            {
                DataReader.SaveDataToDB(storageCollection, newLine, palleteTable, boxTable, needCreate: cAnswer.Equals("y"), needTruncate: tAnswer.Equals("y"));
                Console.WriteLine("Коллекция сохранена");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Ошибка: {0}.\n Попробуйте ещё раз.", ex.Message);
            }

            break;
        case "t2":
            List<Guid> palletes = DataSorter.SortSetByBBDate(storageCollection);
            foreach (Guid p in palletes)
            {
                Console.WriteLine("ID: {0}:\nСрок годности: {1}. Наибольший срок годности коробки на паллете: {2} Объем: {3}", 
                    p,
                    storageCollection.GetPalleteBBDate(p).ToString("yyyy-MM-dd"),
                    storageCollection.Boxes.Where(x => x.PalleteId == p).Select(x => storageCollection.GetBoxBBDate(x.Id)).Min(),
                    storageCollection.GetPalleteVolume(p));
                Console.WriteLine("----------");
            }
            Console.WriteLine("Все сроки годности коробок:");
            List<DateTime> BBDates = storageCollection.Boxes.Select(x => storageCollection.GetBoxBBDate(x.Id)).OrderBy(x => x).ToList();
            foreach (DateTime date in BBDates)
            {
                Console.WriteLine(date.Date.ToString("yyyy-MM-dd"));
            }
            break;
        case "t1":
            IEnumerable<IOrderedEnumerable<Pallete>> groups = DataSorter.SortSetByBBDateGroup(storageCollection);
            Console.WriteLine("ГРУППЫ:");
            int i = 0;
            foreach (IOrderedEnumerable<Pallete> group in groups)
            {
                Console.WriteLine("ГРУППА {0}", i);
                
                foreach(Pallete p in group)
                {
                    Console.WriteLine("ID: {0}. Срок годности: {1}. Вес: {2}", p.Id, storageCollection.GetPalleteBBDate(p.Id).ToString("yyyy-MM-dd"), p.Weigth);
                }
                i++;
            }
            break;
        case "sht":
            Console.WriteLine("t1 (Task 1): Сгруппировать все паллеты по сроку годности," +
                " отсортировать по возрастанию срока годности, в каждой группе отсортировать паллеты по весу.\n" +
                "t2 (Task 2): 3 паллеты, которые содержат коробки с наибольшим сроком годности, отсортированные по возрастанию объема.");
            break;
        case "info":
            Console.WriteLine(infoOptions);
            break;
        case "exit":
            Console.WriteLine("До свидания!");
            break;
        default:
            Console.WriteLine(wrongFormat);
            break;
    }
    Console.WriteLine("Введите команду:");
}

using MonopolyTest.DataReader;
using MonopolyTest.DataUtils;
using MonopolyTest.Domain.Models;
using System.Collections.Generic;
using System.Linq;

Console.WriteLine("Добро пожаловать! Введите одну из команд:");
Console.WriteLine("Ввести данные:\n" +
    "h  - вручную\n" +
    "g  - сгенерировать набор данных\n" +
    "f - прочитать набор из файла\n" +
    "db - подключится к существующей базе данных\n" +
    "next - закончить набор данных");

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

            Console.WriteLine("Что хотите добавить?\np - добавить паллету\nb - добавить коробку\nend - закончить ввод");
            string? handCommand = Console.ReadLine();

            while (handCommand != "end")
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
            string? filePath = Console.ReadLine();
            if (filePath != null && !filePath.Equals(string.Empty))
            {
                try
                {
                    storageCollection = DataReader.ReadDataFromFile(filePath);
                    Console.WriteLine("Коллеция прочитана.");
                }
                catch(FileNotFoundException)
                {
                    Console.WriteLine("Указанного файла не существует.");
                }
            }
            break;
        case "db":
            string newLine = GetUserConnectionString();            
            storageCollection = DataReader.ReadDataFromDB(newLine);
            Console.WriteLine("Коллеция прочитана.");
            break;
        case "next":
            break;
        default: 
            Console.WriteLine(wrongFormat); 
            break;
    }
    Console.WriteLine("Введите команду:");
}

Console.WriteLine("Выберите опцию:\n"
    + "sh - посмотреть всю коллекцию\n"
    + "sf - сохранить коллецию в файл\n"
    + "sdb - сохранить коллекцию в базу данных"
    + "exit - выйти из приложения");

Console.WriteLine();
Console.WriteLine("Функции вывода на экран по заданию:\n" +
    "t1 - группировка палет по сроку годности\n" +
    "t2 - сортировка паллет по возрастанию объема\n" +
    "sht - показать задания полностью");

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
            string? filePath = Console.ReadLine();
            if (filePath != null && !filePath.Equals(string.Empty))
            {
                try
                {
                    DataReader.SaveDataToFile(storageCollection, filePath);
                    Console.WriteLine("Коллекция сохранена.");
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Указанного файла не существует.");
                }
            }
            break;
        case "sdb":
            string newLine = GetUserConnectionString();
            DataReader.SaveDataToDB(storageCollection, newLine);
            Console.WriteLine("Коллекция сохранена");
            break;
        case "t1":
            List<Guid> palletes = DataSorter.SortSetByBBDate(storageCollection);
            foreach (Guid p in palletes)
            {
                Console.WriteLine("ID: {0}. Срок годности: {1}. Объем: {2}", p, storageCollection.GetPalleteBBDate(p), storageCollection.GetPalleteVolume(p));
                Console.WriteLine();
                Console.WriteLine("Все сроки годности:");
            }
            List<DateTime> BBDates = storageCollection.Palletes.Select(x => storageCollection.GetPalleteBBDate(x.Id)).OrderBy(x => x).ToList();
            foreach (DateTime date in BBDates)
            {
                Console.WriteLine(date.ToString("dd/MM/yyyy"));
            }
            break;
        case "t2":
            IEnumerable<IOrderedEnumerable<Pallete>> groups = DataSorter.SortSetByBBDateGroup(storageCollection);
            Console.WriteLine("ГРУППЫ:");
            int i = 0;
            foreach (IOrderedEnumerable<Pallete> group in groups)
            {
                Console.WriteLine("ГРУППА {0}", i);
                
                foreach(Pallete p in group)
                {
                    Console.WriteLine("ID: {0}. Срок годности: {1}. Вес: {2}", p.Id, storageCollection.GetPalleteBBDate(p.Id), p.Weigth);
                }
                i++;
            }
            break;
        case "sht":
            Console.WriteLine("t1 (Task 1): Сгруппировать все паллеты по сроку годности," +
                " отсортировать по возрастанию срока годности, в каждой группе отсортировать паллеты по весу.\n" +
                "t2 (Task 2): 3 паллеты, которые содержат коробки с наибольшим сроком годности, отсортированные по возрастанию объема.");
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


//Add checks
static string GetUserString()
{
    Console.WriteLine("Введите строку подключения:");
    string newLine = Console.ReadLine();
    Console.WriteLine("Сохранить новую строку?[y/n]");

    string? answer = Console.ReadLine();

    if (answer != null || answer.Equals("y"))
    {
        DataReader.SaveConnectionsStrings(newLine);
    }

    return newLine;
}

static string GetUserConnectionString()
{
    Dictionary<int, string> strings = DataReader.GetConnectionsStrings();
    string newLine = string.Empty;
    if (strings.Count > 0)
    {
        Console.WriteLine("Имеются сохраненные строки подключения:");
        foreach (KeyValuePair<int, string> pair in strings)
        {
            Console.WriteLine(pair.Key + " : " + pair.Value);
        }
        Console.WriteLine(strings.Count + " : Ввести новую строку подключения");
        Console.WriteLine("Выберите нужную опцию:");

        int index;
        while (!int.TryParse(Console.ReadLine(), out index) || index < 0 || index > strings.Count)
        {
            Console.WriteLine("Неверный формат ввода. Повторите попытку:");
        }
        if (index == strings.Count)
        {
            newLine = GetUserString();
        }
        else
        {
            newLine = strings[index];
        }
    }
    else
    {
        newLine = GetUserString();
    }
    return newLine;
}

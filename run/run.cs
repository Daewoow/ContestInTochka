using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;


class HotelCapacity
{
    public static bool CheckCapacity(int maxCapacity, List<Guest> guests)
    {
        if (maxCapacity < 0)
            throw new ArgumentException(nameof(maxCapacity));
        
        var events = new List<(DateTime date, int change)>();
        foreach (var g in guests)
        {
            events.Add((DateTime.ParseExact(g.CheckIn, "yyyy-MM-dd", CultureInfo.InvariantCulture), +1)); 
            events.Add((DateTime.ParseExact(g.CheckOut, "yyyy-MM-dd", CultureInfo.InvariantCulture), -1));
        }

        events.Sort((a, b) =>
        {
            var cmp = a.date.CompareTo(b.date);
            return cmp != 0 
                ? cmp 
                : a.change.CompareTo(b.change);
        });

        var current = 0;
        foreach (var ev in events)
        {
            current += ev.change;
            if (current > maxCapacity)
                return false;
        }

        return true;
    }


    public class Guest
    {
        public string Name { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
    }
    
    public static void Main()
    {
        int maxCapacity = int.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());


        List<Guest> guests = new List<Guest>();


        for (int i = 0; i < n; i++)
        {
            string line = Console.ReadLine();
            Guest guest = ParseGuest(line);
            guests.Add(guest);
        }


        bool result = CheckCapacity(maxCapacity, guests);


        Console.WriteLine(result ? "True" : "False");
    }


    // Простой парсер JSON-строки для объекта Guest
    static Guest ParseGuest(string json)
    {
        var guest = new Guest();


        // Извлекаем имя
        Match nameMatch = Regex.Match(json, "\"name\"\\s*:\\s*\"([^\"]+)\"");
        if (nameMatch.Success)
            guest.Name = nameMatch.Groups[1].Value;


        // Извлекаем дату заезда
        Match checkInMatch = Regex.Match(json, "\"check-in\"\\s*:\\s*\"([^\"]+)\"");
        if (checkInMatch.Success)
            guest.CheckIn = checkInMatch.Groups[1].Value;


        // Извлекаем дату выезда
        Match checkOutMatch = Regex.Match(json, "\"check-out\"\\s*:\\s*\"([^\"]+)\"");
        if (checkOutMatch.Success)
            guest.CheckOut = checkOutMatch.Groups[1].Value;


        return guest;
    }
}
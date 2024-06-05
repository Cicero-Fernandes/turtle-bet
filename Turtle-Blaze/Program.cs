//Grupo
//Cicero Eduardo Silva Fernandes
//Leticia Regina Oliveira da Silva
//Tierry Willis Yun de Souza

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Collections.Concurrent;

class Program
{
    static void Main(string[] args)
    {
        var menuMusic = new Action(TartarugasNinjas.MenuMusic);
        Parallel.Invoke(menuMusic);

        List<TartarugasNinjas> turtles = new List<TartarugasNinjas>();
        ConcurrentBag<Bet> bets = new ConcurrentBag<Bet>();
        int menuOption;
        do
        {
            Console.Clear();
            Console.WriteLine("--WELCOME TO THE TURTLE RACE!--");
            Console.WriteLine("\n1 - Register your Ninja Turtle \n2 - View Ninja Turtle data \n3 - Start the race \n4 - Credits \n5 - Exit");
            Console.Write("\nChoose one of the options above: ");
            int.TryParse(Console.ReadLine(), out menuOption);

            switch (menuOption)
            {
                case 1:
                    RegisterTurtle(turtles);
                    break;
                case 2:
                    Console.Clear();
                    Console.WriteLine("--DATA ON YOUR NINJA RUNNER TURTLE!--");
                    TartarugasNinjas.DisplayData(turtles);
                    Console.WriteLine("Press any key to return to the menu.");
                    Console.ReadKey();
                    break;
                case 3:
                    if (turtles.Count > 0)
                    {
                        bets.Clear();
                        Bet.toBet(bets, turtles);
                        Console.Clear();
                        Bet.betReport(bets, turtles);
                        Console.Clear();
                        Console.WriteLine("--THE START HAS BEEN GIVEN!!--\n");
                        TartarugasNinjas.StartRace(bets, turtles);
                    }
                    else
                    {
                        Console.WriteLine("No turtles registered. Register at least one turtle before starting the race.");
                        Console.ReadKey();
                    }
                    break;
                case 4:
                    Credits();
                    break;
                case 5:
                    Console.WriteLine("\nLeaving...");
                    Thread.Sleep(1000);
                    break;
                default:
                    Console.WriteLine("Invalid option!");
                    break;
            }
        } while (menuOption != 5);
    }

    static void RegisterTurtle(List<TartarugasNinjas> turtles)
    {
        Console.Clear();
        Console.WriteLine("--REGISTER YOUR TURTLE--");
        Console.WriteLine("\nEnter the name of the turtle: ");
        string name = Console.ReadLine();
        Console.WriteLine("Enter the weight in kg: ");
        double weight = double.Parse(Console.ReadLine());
        Console.WriteLine("Enter the length in meters: ");
        double length = double.Parse(Console.ReadLine());

        Console.WriteLine("Choose a color: ");
        Console.WriteLine("| 1 - Red | 2 - Blue | 3 - Green | 4 - Yellow | 5 - Pink | 6 - Black |\n| 7 - DarkYellow | 8 - Cyan | 9 - Gray | 10 - DarkBlue | 11 - DarkGreen | 12 - DarkRed |");
        int indexCor;
        while (!int.TryParse(Console.ReadLine(), out indexCor) || indexCor < 1 || indexCor > 12)
        {
            Console.Write("Invalid color! Enter a valid number: ");
        }

        ConsoleColor color;
        switch (indexCor)
        {
            case 1:
                color = ConsoleColor.Red;
                break;
            case 2:
                color = ConsoleColor.Blue;
                break;
            case 3:
                color = ConsoleColor.Green;
                break;
            case 4:
                color = ConsoleColor.Yellow;
                break;
            case 5:
                color = ConsoleColor.Magenta;
                break;
            case 6:
                color = ConsoleColor.Black;
                break;
            case 7:
                color = ConsoleColor.DarkYellow;
                break;
            case 8:
                color = ConsoleColor.Cyan;
                break;
            case 9:
                color = ConsoleColor.Gray;
                break;
            case 10:
                color = ConsoleColor.DarkBlue;
                break;
            case 11:
                color = ConsoleColor.DarkGreen;
                break;
            case 12:
                color = ConsoleColor.DarkRed;
                break;
            default:
                color = ConsoleColor.White;
                break;
        }

        turtles.Add(new TartarugasNinjas(name, weight, color, length));
        Console.WriteLine("Turtle registered successfully!\n");

        Console.WriteLine("Do you want to register another Turtle? \n1 - Yes\n2 - No");
        int.TryParse(Console.ReadLine(), out int op);
        if (op == 1)
        {
            RegisterTurtle(turtles);
        }
    }

    static void Credits()
    {
        Console.WriteLine("TURTLE RACE CREATORS: \n\nCícero Eduardo \nLetícia Regina \nTierry Willis");
        Console.WriteLine("Press any key to return to the menu.");
        Console.ReadKey();
    }
}
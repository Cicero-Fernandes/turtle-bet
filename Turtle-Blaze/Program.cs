using System;
using System.Collections.Generic;
using System.Data;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

public class TartarugasNinjas
{
    private int Position { get; set; }
    private int Speed { get; set; }
    private int RestTime { get; set; } //Tempo de descanso
    private RaceStrategy Strategy { get; set; }
    private string Name { get; set; }
    private double Weight { get; set; } //Peso
    private ConsoleColor Color { get; set; }
    private double Length { get; set; } //Comprimento
    private char Symbol { get; set; }
    private readonly Random random = new Random();
    private double IMC { get; set; }
    private int Stamina { get; set; }
    private int TimeSleep { get; set; }
    private static readonly object lockObject = new object();

    public TartarugasNinjas(string name, double weigth, ConsoleColor color, double length)
    {
        Name = name;
        Weight = weigth;
        Color = color;
        Length = length;
        Speed = 0;
        Position = 0;
        RestTime = 0;
        IMC = CalculationIMC();
        Strategy = RaceStrategy.Acceleration;
        Symbol = '>';
        AdjustStamina_and_RestTime();
    }

    public enum RaceStrategy
    {
        Acceleration,
        Slowdown,
        Rest //Descanso
    }

    private void AdjustStamina_and_RestTime()
    {
        double imc = CalculationIMC();

        if (imc < 18.5)
        {
            Stamina = 1;
            TimeSleep = 2;

        }
        else if (imc < 25)
        {
            Stamina = 4;
            TimeSleep = 2;
        }
        else if (imc < 30)
        {
            Stamina = 3;
            TimeSleep = 3;
        }
        else
        {
            Stamina = 2;
            TimeSleep = 4;
        }
    }

    public double CalculationIMC()
    {
        return Weight / (Length * Length);
    }

    public void ToMove(int trackSize, int line)
    {
        int staminaInicial = Stamina; // Armazenando a quantidade inicial de stamina

        while (Position < trackSize)
        {
            if (Stamina <= 0)
            {
                Strategy = RaceStrategy.Rest;
                Thread.Sleep(TimeSleep * 1000); // Simulando o tempo de descanso
                Stamina = staminaInicial; // Recuperando a quantidade inicial de stamina
                Symbol = 'z';
                RestTime++;
            }
            else
            {
                Strategy = (RaceStrategy)random.Next(0, 3); // Alternando entre Aceleração, Desaceleração e Velocidade Inicial
            }

            switch (Strategy)
            {
                case RaceStrategy.Acceleration:
                    Speed = Math.Max(1, Speed + random.Next(1, 3));
                    Stamina = Stamina - Speed;
                    Symbol = 'a';
                    break;
                case RaceStrategy.Slowdown:
                    Speed = Math.Max(1, Speed - random.Next(1, 3));
                    Symbol = 'd';
                    break;
                default:
                    Symbol = '>';
                    break;
            }
            Position += Speed;
            Position = Math.Min(Position, trackSize);
            Drawing(this, trackSize, line);
            Thread.Sleep(1000);
        }
    }

    public static void ResetRace(List<TartarugasNinjas> turtles)
    {
        Random random = new Random();
        Parallel.ForEach(turtles, turtle =>
        {
            turtle.Position = 0;
            turtle.RestTime = 0;
            turtle.Speed = random.Next(1, 6);
            turtle.Strategy = RaceStrategy.Acceleration;
        });
    }

    public static void Drawing(TartarugasNinjas turtle, int trackSize, int line)
    {

        lock (lockObject)
        {
            Console.SetCursorPosition(0, line);
            Console.ForegroundColor = turtle.Color;

            // Corrigindo o cálculo do comprimento da string
            int filledLength = Math.Max(0, Math.Min(trackSize - 1, turtle.Position)); //Comprimento preenchido
            int emptyLength = Math.Max(0, trackSize - filledLength - 2); // -2 para considerar os caracteres '|'  Comprimento vazio
            Console.Write("|" + new string('-', filledLength) + turtle.Symbol + new string(' ', emptyLength) + "| " + " (" + turtle.Name + " walked " + turtle.Position + "m" + ")" + " --Thread nº " + Thread.CurrentThread.ManagedThreadId + "--");
            Console.ResetColor();
        }
    }
    public static void DisplayData(List<TartarugasNinjas> turtles)
    {
        Console.Clear();
        Console.WriteLine("--DATA ON YOUR NINJA RUNNER TURTLE!--");
        foreach (TartarugasNinjas turtle in turtles)
        {
            Console.WriteLine($"\nName: {turtle.Name}\nWeigth: {turtle.Weight}\nColor: {turtle.Color}\nLength: {turtle.Length}\nIMC: {turtle.IMC:F2}\nStamina: {turtle.Stamina}\n");
        }
        Console.WriteLine("Press any key to return to the menu.");
        Console.ReadKey();
    }

    public static void StartRace(List<TartarugasNinjas> turtle)
    {
        TouchMusics("Corrida.wav");
        ResetRace(turtle);
        int trackSize = 100;
        TartarugasNinjas winner = null;

        List<Thread> threads = new List<Thread>();
        // Iniciando a corrida para cada tartaruga
        for (int i = 0; i < turtle.Count; i++)
        {
            // Calculando a linha onde a tartaruga será desenhada (começando da linha 2)
            int line = i + 2;

            // Criando uma cópia local da variável tartaruga para evitar o problema de closure
            TartarugasNinjas turtlee = turtle[i];
            Thread thread = new Thread(() => turtlee.ToMove(trackSize, line));
            threads.Add(thread);
            thread.Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        foreach (TartarugasNinjas turtlee in turtle)
        {
            if (turtlee.Position >= trackSize && (winner == null || turtlee.RestTime < winner.RestTime))
            {
                winner = turtlee;
            }
        }

        if (winner != null)
        {
            Console.WriteLine("\n\n\n\n\nThe winner of the race is:");
            Console.WriteLine($"\nName: {winner.Name}\nWeight: {winner.Weight}\nColor: {winner.Color}\nLength: {winner.Length}\n");
            TouchMusics("Vitoria.wav");
        }
        Console.WriteLine("Press any key to continue.");
        Console.ReadKey();
    }

    public static void TouchMusics(string fileName)
    {
        string dirAtual = Environment.CurrentDirectory;
        string dirMusic = dirAtual + @"\music\";
        try
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(dirMusic + fileName);
            player.Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred when trying to play the audio file: " + ex.Message);
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        TartarugasNinjas.TouchMusics("Menuu.wav");

        List<TartarugasNinjas> turtles = new List<TartarugasNinjas>();
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
                    TartarugasNinjas.DisplayData(turtles);
                    break;
                case 3:
                    if (turtles.Count > 0)
                    {
                        Console.Clear();
                        Console.WriteLine("--THE START HAS BEEN GIVEN!!--\n");
                        TartarugasNinjas.StartRace(turtles);
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

        if (turtles.Count < 5)
        {
            Console.WriteLine("Do you want to register another Turtle? \n1 - Yes\n2 - No");
            int.TryParse(Console.ReadLine(), out int op);
            if (op == 1)
            {
                RegisterTurtle(turtles);
            }
        }

        if (turtles.Count > 5)
        {
            Console.Clear();
            Console.WriteLine("\nExceeded the limit of competitors!\n");
            Console.WriteLine("Press any key to return to the menu.");
            Console.ReadKey();
        }
    }

    static void Credits()
    {
        Console.WriteLine("TURTLE RACE CREATORS: \n\nCícero Eduardo \nLetícia Regina \nTierry Willis");
        Console.WriteLine("Press any key to return to the menu.");
        Console.ReadKey();
    }
}
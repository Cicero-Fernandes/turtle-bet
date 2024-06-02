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

public class TartarugasNinjas
{
    public int Position { get; set; }
    public int Speed { get; set; }
    public int RestTime { get; set; } //Tempo de descanso
    public RaceStrategy Strategy { get; set; }
    public string Name { get; set; }
    public double Weight { get; set; } //Peso
    public ConsoleColor Color { get; set; }
    public double Length { get; set; } //Comprimento
    public char Symbol { get; set; }
    public readonly Random random = new Random();
    public double IMC { get; set; }
    public int TotalBets { get; set; }
    public int Stamina { get; set; }
    public int TimeSleep { get; set; }
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
        TotalBets = 0;
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
            turtle.TotalBets = 0;
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
        //Console.Clear();
        //Console.WriteLine("--DATA ON YOUR NINJA RUNNER TURTLE!--");
        foreach (TartarugasNinjas turtle in turtles)
        {
            Console.WriteLine($"\nNumber: {turtles.IndexOf(turtle) + 1} \nName: {turtle.Name}\nWeigth: {turtle.Weight}\nColor: {turtle.Color}\nLength: {turtle.Length}\nIMC: {turtle.IMC:F2}\nStamina: {turtle.Stamina}\n");
        }
        //Console.WriteLine("Press any key to return to the menu.");
        //Console.ReadKey();
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
            //player.Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred when trying to play the audio file: " + ex.Message);
        }
    }
}

public class Bet
{
    private string Better { get; set; }
    private int ChosenTurtleIndex { get; set; }
    private int BetAmount { get; set; }

    //Metodo apostar que será chamado na main
    public Bet(string better, int chosenTurtleIndex, int betAmount)
    {
        Better = better;
        ChosenTurtleIndex = chosenTurtleIndex;
        BetAmount = betAmount;
    }

    public static void toBet(ConcurrentBag<Bet> bets, List<TartarugasNinjas> turtles)
    {
        Console.Clear();
        Console.WriteLine("--PLACE YOUR BET--");
        Console.WriteLine("\nPlease provide a name: ");
        string better = Console.ReadLine();

        TartarugasNinjas.DisplayData(turtles);
        Console.WriteLine("Report the number of the shrunken turtle.");
        int chosenTurtleIndex = int.Parse(Console.ReadLine()) - 1;

        Console.WriteLine("\nFinally, enter the value of your bet (between 1 and 5).");
        int betAmount;
        while (!int.TryParse(Console.ReadLine(), out betAmount) || betAmount < 1 || betAmount > 5)
        {
            Console.Write("Invalid value! Enter a valid number: ");
        }
        bets.Add(new Bet(better, chosenTurtleIndex, betAmount));
        turtles[chosenTurtleIndex].TotalBets += betAmount;
        Console.WriteLine("\nBet registered successfully!\n");

        Console.WriteLine("\nPress any key to return to the menu.");
        Console.ReadKey();
    }

    public static void betReport(ConcurrentBag<Bet> bets, List<TartarugasNinjas> turtles)
    {
        Console.Clear();
        Console.WriteLine("--BET REPORT--\n");

        Console.WriteLine("Individual Bets:\n");
        foreach (Bet bet in bets)
        {
            Console.WriteLine($"Better: {bet.Better}, Turtle: {turtles[bet.ChosenTurtleIndex].Name}, Amount: {bet.BetAmount}");
        }

        Console.WriteLine("\nTotal Bets on Each Turtle:\n");
        foreach (var turtle in turtles)
        {
            Console.WriteLine($"{turtle.Name}: {turtle.TotalBets}");
        }

        Console.WriteLine("\nPress any key to start the race.");
        Console.ReadKey();
    }

}

class Program
{
    static void Main(string[] args)
    {
        TartarugasNinjas.TouchMusics("Menuu.wav");

        List<TartarugasNinjas> turtles = new List<TartarugasNinjas>();
        ConcurrentBag<Bet> bets = new ConcurrentBag<Bet>();
        int menuOption;
        do
        {
            Console.Clear();
            Console.WriteLine("--WELCOME TO THE TURTLE RACE!--");
            Console.WriteLine("\n1 - Register your Ninja Turtle \n2 - View Ninja Turtle data \n3 - Place your bet \n4 - Start the race \n5 - Credits \n6 - Exit");
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
                    if (turtles.Count != 0)
                    {
                        Bet.toBet(bets, turtles);
                    }
                    else
                    {
                        Console.WriteLine("\nBefore betting you need to create the turtles");
                        Thread.Sleep(1000);
                    }

                    break;
                case 4:
                    if (turtles.Count > 0)
                    {
                        Bet.betReport(bets, turtles);
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
                case 5:
                    Credits();
                    break;
                case 6:
                    Console.WriteLine("\nLeaving...");
                    Thread.Sleep(1000);
                    break;
                default:
                    Console.WriteLine("Invalid option!");
                    break;
            }
        } while (menuOption != 6);
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
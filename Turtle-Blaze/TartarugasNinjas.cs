//Grupo
//Cicero Eduardo Silva Fernandes
//Leticia Regina Oliveira da Silva
//Tierry Willis Yun de Souza

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
            Console.Write("|" + new string('-', filledLength) + turtle.Symbol + new string(' ', emptyLength) + "| " + " (" + turtle.Name + " " + turtle.Position + "m" + ")" + " <T nº " + Thread.CurrentThread.ManagedThreadId + ">");
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

    public static void StartRace(ConcurrentBag<Bet> bets, List<TartarugasNinjas> turtle)
    {
        var raceMusic = new Action(RaceMusic);
        Parallel.Invoke(raceMusic);
        ResetRace(turtle);
        int trackSize = 100;
        bool fineshedRace = false;
        List<TartarugasNinjas> ranking = new List<TartarugasNinjas>();
        List<Thread> threads = new List<Thread>();
        // Iniciando a corrida para cada tartaruga
        for (int i = 0; i < turtle.Count; i++)
        {
            int index = i; // Crie uma cópia local de 'i'
            Thread thread = new Thread(() => turtle[index].ToMove(trackSize, index + 2));
            threads.Add(thread);
            thread.Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        foreach (TartarugasNinjas turtlee in turtle)
        {
            if (turtlee.Position >= trackSize)
            {
                ranking.Add(turtlee);
                ranking.Sort((a, b) => a.RestTime.CompareTo(b.RestTime));
            }
            if (ranking.Count() == turtle.Count())
            {
                fineshedRace = true;
            }

        }

        if (fineshedRace == true)
        {
            Bet.bettingResult(bets, ranking);
        }
        Console.WriteLine("\nPress any key to continue.");
        Console.ReadKey();
    }

    public static void RaceMusic()
    {
        string dirAtual = Environment.CurrentDirectory;
        string dirMusic = dirAtual + @"\music\";
        try
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(dirMusic + "Corrida.wav");
            player.Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred when trying to play the audio file: " + ex.Message);
        }
    }

    public static void MenuMusic()
    {
        string dirAtual = Environment.CurrentDirectory;
        string dirMusic = dirAtual + @"\music\";
        try
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(dirMusic + "Menuu.wav");
            player.Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred when trying to play the audio file: " + ex.Message);
        }
    }

    public static void WinnerMusic()
    {
        string dirAtual = Environment.CurrentDirectory;
        string dirMusic = dirAtual + @"\music\";
        try
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(dirMusic + "Vitoria.wav");
            player.Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred when trying to play the audio file: " + ex.Message);
        }
    }
}

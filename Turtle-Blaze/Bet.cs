//Grupo
//Cicero Eduardo Silva Fernandes
//Leticia Regina Oliveira da Silva
//Tierry Willis Yun de Souza

using System.Collections.Concurrent;

public class Bet
{
    private string Better { get; set; }
    private TartarugasNinjas ChosenTurtle { get; set; }
    private double BetAmount { get; set; }

    //Metodo apostar que será chamado na main
    public Bet(string better, TartarugasNinjas chosenTurtle, double betAmount)
    {
        Better = better;
        ChosenTurtle = chosenTurtle;
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
        turtles[chosenTurtleIndex].TotalBets += betAmount;
        bets.Add(new Bet(better, turtles[chosenTurtleIndex], Convert.ToDouble(betAmount)));
        Console.WriteLine("\nBet registered successfully!\n");

        Console.WriteLine("Do you want to register a new Bet? \n1 - Yes\n2 - No");
        int.TryParse(Console.ReadLine(), out int op);
        if (op == 1)
        {
            toBet(bets, turtles);
        }
    }

    public static void betReport(ConcurrentBag<Bet> bets, List<TartarugasNinjas> turtles)
    {
        Console.Clear();
        Console.WriteLine("--BET REPORT--\n");

        Console.WriteLine("Individual Bets:\n");
        foreach (Bet bet in bets)
        {
            Console.WriteLine($"Better: {bet.Better}, Turtle: {bet.ChosenTurtle.Name}, Amount: {bet.BetAmount}");
        }

        Console.WriteLine("\nTotal Bets on Each Turtle:\n");
        foreach (var turtle in turtles)
        {
            Console.WriteLine($"{turtle.Name}: {turtle.TotalBets}");
        }

        Console.WriteLine("\nPress any key to start the race.");
        Console.ReadKey();
    }

    public static void bettingResult(ConcurrentBag<Bet> bets, List<TartarugasNinjas> ranking)
    {
        Console.WriteLine("\n\n\n\nThe winner of the race is:");
        Console.WriteLine($"\nName: {ranking[0].Name}\nWeight: {ranking[0].Weight}\nColor: {ranking[0].Color}\nLength: {ranking[0].Length}\n");
        var winnerMusic = new Action(TartarugasNinjas.WinnerMusic);
        Parallel.Invoke(winnerMusic);

        Console.WriteLine("Full Ranking:\n");
        foreach (TartarugasNinjas turtle in ranking)
        {
            Console.WriteLine($"{ranking.IndexOf(turtle) + 1}. Name: {turtle.Name}, Weight: {turtle.Weight}, Color: {turtle.Color}, Length: {turtle.Length}");
        }

        Console.WriteLine("\nBet Result:\n");

        Parallel.ForEach(bets, bet =>
        {
            if (bet.ChosenTurtle == ranking[0])
            {
                bet.BetAmount *= 2;
                Console.WriteLine($"{bet.Better}, bet on first place and won: {bet.BetAmount}");
            }
            else if (bet.ChosenTurtle == ranking[1])
            {
                bet.BetAmount *= 1.5;
                Console.WriteLine($"{bet.Better}, bet on second place and won: {bet.BetAmount}");
            }
            else if (bet.ChosenTurtle == ranking[2])
            {
                bet.BetAmount *= 0.5;
                Console.WriteLine($"{bet.Better}, bet on third place and won: {bet.BetAmount}");
            }
            else
            {
                Console.WriteLine($"{bet.Better}, made a bad bet and lost: {bet.BetAmount}");
            }
        }
        );
    }

}

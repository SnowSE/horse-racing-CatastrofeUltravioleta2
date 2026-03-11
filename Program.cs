namespace HorseRace;

class Program
{
    private static AutoResetEvent[] signalHorseEvents = new[]
   {
        new AutoResetEvent(false),
        new AutoResetEvent(false),
        new AutoResetEvent(false),
        new AutoResetEvent(false)
    };

    private static AutoResetEvent[] horseDoneEvents = new[]
    {
        new AutoResetEvent(false),
        new AutoResetEvent(false),
        new AutoResetEvent(false),
        new AutoResetEvent(false)
    };
    private static int[] steps = new int[4];

    private static int finish = 50;
    private static List<string> winners = new List<string>();
    private static string[] horseNames = { "HorseA", "HorseB", "HorseC", "HorseD" };
    private static bool isRaceFinished = false;

    private static double playerBalance = 1000.0;
    private static int betHorseIndex = -1;
    private static double betAmount = 0;

    static void Main(string[] args)
    {
        while (true)
        {
            steps = new int[4];
            winners = new List<string>();
            isRaceFinished = false;
            signalHorseEvents = new[]
            {
                new AutoResetEvent(false),
                new AutoResetEvent(false),
                new AutoResetEvent(false),
                new AutoResetEvent(false)
            };
            horseDoneEvents = new[]
            {
                new AutoResetEvent(false),
                new AutoResetEvent(false),
                new AutoResetEvent(false),
                new AutoResetEvent(false)
            };

            if (!ShowBettingMenu())
                break;


            Console.Clear();
            Console.WriteLine("Welcome to the Racing Program:");
            System.Console.WriteLine("HorseA:");
            System.Console.WriteLine("HorseB:");
            System.Console.WriteLine("HorseC:");
            System.Console.WriteLine("HorseD:");


            Thread[] horseThreads = new Thread[4];
            for (int i = 0; i < 4; i++)
            {
                int horseIndex = i;
                horseThreads[i] = new Thread(() => HorseRun(horseIndex));
                horseThreads[i].Start();
            }

            while (true)
            {
                foreach (var signalEvent in signalHorseEvents)
                    signalEvent.Set();

                foreach (var doneEvent in horseDoneEvents)
                    doneEvent.WaitOne();

                drawRace();

                lock (winners)
                {
                    if (winners.Count > 0)
                    {
                        isRaceFinished = true;
                        foreach (var signalEvent in signalHorseEvents)
                            signalEvent.Set();

                        break;
                    }
                }

            }

            System.Console.WriteLine();
            lock (winners)
            {
                if (winners.Count > 1)
                {
                    Console.WriteLine("It's a tie between: " + string.Join(", ", winners));
                }
                else
                {
                    Console.WriteLine("The winner is: " + winners[0]);
                }
            }

            BetResult();

            if (playerBalance <= 0)
            {
                Console.WriteLine("\nYou're out of money! Game over.");
                break;
            }

            Console.WriteLine("\nDo you want quit? (Q).");
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Q)
                break;

        }

        static bool ShowBettingMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Place your bets");
                Console.WriteLine($"Your balance: {playerBalance}");
                Console.WriteLine("Pick a horse to bet on:");
                for (int i = 0; i < 4; i++)
                    Console.WriteLine($"    [{i + 1}] {horseNames[i]}");

                Console.WriteLine($"    [5] Skip betting this race");
                Console.WriteLine($"    [Q] Quit game");
                Console.WriteLine();
                Console.Write("  Choice: ");

                string input = Console.ReadLine().ToUpper();

                if (input == "Q")
                    return false;

                if (input == "5")
                {
                    betHorseIndex = -1;
                    betAmount = 0;
                    return true;
                }

                if (!int.TryParse(input, out int choice) || choice < 1 || choice > 4)
                    continue;

                betHorseIndex = choice - 1;

                while (true)
                {
                    System.Console.WriteLine();
                    Console.WriteLine($"  Betting on: {horseNames[betHorseIndex]}");
                    Console.WriteLine($"  Win multiplier: 3x");
                    Console.WriteLine($"  Enter bet amount: ");

                    string amountInput = Console.ReadLine();

                    if (!double.TryParse(amountInput, out double amount) || amount <= 0)
                    {
                        Console.WriteLine("Please enter a valid number.");
                        continue;
                    }

                    if (amount > playerBalance)
                    {
                        Console.WriteLine($"You only have {playerBalance}.");
                        continue;
                    }

                    betAmount = amount;
                    return true;
                }
            }
        }

        static void BetResult()
        {
            if (betHorseIndex == -1)
            {
                Console.WriteLine("  (You didn't place a bet this race.)");
                return;
            }

            if (winners.Contains(horseNames[betHorseIndex]))
            {
                double winAmount = betAmount * 3;
                playerBalance += winAmount - betAmount;
                Console.WriteLine($"{horseNames[betHorseIndex]} won! You bet {betAmount} and won {winAmount}!");
                Console.WriteLine($"  New balance: {playerBalance}");
            }
            else
            {
                playerBalance -= betAmount;
                Console.WriteLine($"{horseNames[betHorseIndex]} didn't win. You lost {betAmount}.");
                Console.WriteLine($"  New balance: {playerBalance}");
            }
        }

        static void drawRace()
        {
            for (int i = 0; i < 4; i++)
            {
                Console.SetCursorPosition(0, i + 1);
                Console.Write(horseNames[i] + ": ");

                for (int j = 0; j < steps[i]; j++)
                    Console.Write("-");

                lock (winners)
                {
                    if (winners.Contains(horseNames[i]))
                        Console.Write("WINNER!");
                }
            }
        }

        static void HorseRun(int horseIndex)
        {

            while (true)
            {
                signalHorseEvents[horseIndex].WaitOne();

                if (isRaceFinished)
                    break;

                if (Random.Shared.Next(1000) == 0)
                {
                    lock (steps)
                    {
                        steps[horseIndex]++;
                    }

                    if (steps[horseIndex] >= finish)
                    {
                        lock (winners)
                        {
                            winners.Add(horseNames[horseIndex]);
                        }
                    }
                }

                horseDoneEvents[horseIndex].Set();
            }

        }

    }
}

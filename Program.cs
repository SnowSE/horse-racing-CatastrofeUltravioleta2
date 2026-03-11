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
    static void Main(string[] args)
    {
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
                    lock(steps)
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

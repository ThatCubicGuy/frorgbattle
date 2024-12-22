using FrogBattleV2.Classes.Characters;
using FrogBattleV2.Classes.GameLogic;

internal class Program
{
    private static int Main(string[] args)
    { // Pick a fighter
        Fighter? player1 = null, player2 = null;
        int fail;
        if (OperatingSystem.IsOSPlatform("windows")) Console.WindowWidth = 160;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        List<string> players1 = new() { "Rex", "Bayonetta", "Raiden", "Cubic", "Mami", "Alice"};
        List<string> players2 = new() { "Jex", "Jeanne", "Jetstream Sam", "Farabac", "Tomoe", "Lizzie" };
        do
        {
            Console.Write("Select player 1: ");
            string p1 = Console.ReadLine() ?? string.Empty;
            fail = 0;
            switch (p1)
            {
                case "a":
                    player1 = new Rexulti(players1[0]);
                    players1[0] = players2[0];
                    break;
                case "b":
                    player1 = new Bayonetta(players1[1]);
                    players1[1] = players2[1];
                    break;
                case "c":
                    player1 = new Raiden(players1[2]);
                    players1[2] = players2[2];
                    break;
                case "d":
                    player1 = new Cubic(players1[3]);
                    players1[3] = players2[3];
                    break;
                case "e":
                    player1 = new MamiTomoe(players1[4]);
                    players1[4] = players2[4];
                    break;
                case "f":
                    player1 = new Alice(players1[5]);
                    players1[5] = players2[5];
                    break;
                case "g":
                    player1 = new Kongle(players1[6]);
                    players1[6] = players2[6];
                    break;
                case "z":
                    player1 = new God("God");
                    break;
                default:
                    Console.WriteLine($"\"{p1}\" is not a valid player.");
                    fail = 1;
                    break;
            }
        } while (fail == 1);
        do
        {
            Console.Write("Select player 2: ");
            string p2 = Console.ReadLine() ?? string.Empty;
            fail = 0;
            switch (p2)
            {
                case "a":
                    player2 = new Rexulti(players1[0]);
                    break;
                case "b":
                    player2 = new Bayonetta(players1[1]);
                    break;
                case "c":
                    player2 = new Raiden(players1[2]);
                    break;
                case "d":
                    player2 = new Cubic(players1[3]);
                    break;
                case "e":
                    player2 = new MamiTomoe(players1[4]);
                    break;
                case "f":
                    player2 = new Alice(players1[5]);
                    break;
                case "g":
                    player2 = new Kongle(players1[6]);
                    break;
                case "z":
                    player2 = new God("Jesus");
                    break;
                default:
                    Console.WriteLine($"\"{p2}\" is not a valid player.");
                    fail = 1;
                    break;
            }
        } while (fail == 1);
        if (player1 == null || player2 == null) return 0;
        Console.Clear();
        int turn = 0;
        string output = "Round start!";
        try
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(player1.ToString());
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\tVS.");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(player2.ToString());
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(output);
                if (turn % 2 == 0) // Check whose turn it is
                {
                    output = player1.PlayTurnCONSOLE(player2, output);
                }
                else
                {
                    output = player2.PlayTurnCONSOLE(player1, output);
                }
                Console.Clear(); // Wipe the slate clean!
                turn++;
            }
        }
        catch (Exception ex)
        {
            if (ex.InnerException == Fighter.GameEndMechanic)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(output);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.Cyan;
                try
                {
                    _ = player1.Hp;
                    Console.WriteLine($"{player1.Name} is declared the winner!");
                }
                catch
                {
                    Console.WriteLine($"{player2.Name} is declared the winner!");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Game ended due to exception.");
            }
        }
        return turn;
    }
}
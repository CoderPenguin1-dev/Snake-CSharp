using System.Xml.Serialization;

class Program
{
    const char snakeSegment = '#';

    static void AppleChecker(ref int length, ref int[] foodPos, int x, int y)
    {
        Random rng = new();
        if (x == foodPos[0] && y == foodPos[1])
        {
            length++;
            foodPos[0] = rng.Next(1, 34); foodPos[1] = rng.Next(1, 11);
        }
    }

/*
      Input Map

          0
      
    3  -  |  -  1

          2
*/

    static void InputHandler(ref int gameRunning, ref int length, ref bool paused, ref int direction, List<int[]> posList)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    if (direction != 2 && !paused || length == 0 && !paused) direction = 0;
                    break;

                case ConsoleKey.DownArrow:
                    if (direction != 0 && !paused || length == 0 && !paused) direction = 2;
                    break;

                case ConsoleKey.LeftArrow:
                    if (direction != 1 && !paused || length == 0 && !paused) direction = 3;
                    break;

                case ConsoleKey.RightArrow:
                    if (direction != 3 && !paused || length == 0 && !paused) direction = 1;
                    break;

                case ConsoleKey.Escape:
                    // Clear pause message if paused.
                    if (paused)
                    {
                        Console.SetCursorPosition(6, 5);
                        Console.WriteLine("                       ");
                        Console.SetCursorPosition(7, 6);
                        Console.WriteLine("                    ");

                        // Rewrite le snek
                        foreach (int[] pos in posList)
                        {
                            Console.SetCursorPosition(pos[0], pos[1]);
                            Console.Write(snakeSegment);
                        }
                    }
                    // Enable/Disable pausing.
                    paused = !paused;
                    break;

                case ConsoleKey.M:
                    if (paused) gameRunning = -1;
                    break;
            }
        }
    }

    public static void Main(string[] args)
    {
        Console.Title = "Snake"; Console.CursorVisible = false; // Setup the window.
        int previousLength = 0;

        while (true)
        {
            #region New High Score Check
            if (previousLength > 0)
            {
                bool firstScore = false;
                Console.Clear(); Console.ForegroundColor = ConsoleColor.Gray;
                if (!File.Exists("scores.dat"))
                {
                    File.WriteAllText("scores.dat", "");
                    firstScore = true;
                }
                string[] scores = File.ReadAllLines("scores.dat");
                int score;
                // Check to see if the scores file was already there.
                if (firstScore) score = 0;
                else score = int.Parse(scores[0].Split(';')[1]);
                if (previousLength > score)
                {
                    Console.WriteLine("New High Score!");
                    Console.Write("Please write your name: ");
                    string name = Console.ReadLine();

                    // Put new high score on top.
                    File.WriteAllText("scores.dat", $"{name};{previousLength}");
                    foreach (string s in scores)
                        File.AppendAllText("scores.dat", "\n" + s);
                }
            }
            #endregion

            Console.Clear();
            Console.WriteLine("====== Snake ======");
            Console.WriteLine($"Previous Score: {previousLength:D3}");
            Console.WriteLine("===================\n");
            Console.WriteLine("[1] Play Game");
            Console.WriteLine("[2] High Scores");
            Console.WriteLine("[3] Quit Game");
            while (true)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.D1)
                {
                    previousLength = Game();
                    break;
                }
                if (key == ConsoleKey.D2)
                {
                    Console.Clear();
                    if (!File.Exists("scores.dat")) Console.WriteLine("No High Scores.");
                    else
                    {
                        string[] scores = File.ReadAllLines("scores.dat");
                        foreach (string scoreLine in scores)
                        {
                            string name = scoreLine.Split(";")[0];
                            int score = int.Parse(scoreLine.Split(";")[1]);
                            Console.WriteLine(name + " : " + score);
                        }
                    }
                    Console.WriteLine("Press any key to continue.");
                    Console.ReadKey();
                    break;
                }
                if (key == ConsoleKey.D3) Environment.Exit(0);
            }
        }
    }

    static void Draw(List<int[]> posList, int[] foodPos, int length)
    {
        bool appleDrawn = false;
        Console.SetCursorPosition(0, 0);

        // Border Top & Score
        string display = "";
        for (int i = 0; i < 35; i++)
        {
            if (i == 13)
            {
                display += $"Score:{length:D3}";
                i += 8;
                continue;
            }
            display += "█";
        }
        display += "\n";

        // Border Sides & Game
        for (int y = 0; y < 11; y++)
        {
            display += "█";
            for (int x = 0; x < 33; x++)
            {
                int[] currentPos = { x + 1, y + 1 };
                bool segmentDrawn = false;
                if (posList.Any(p => p.SequenceEqual(currentPos)))
                {
                    // Draw head of snake.
                    if (posList[^1][0] == currentPos[0] && posList[^1][1] == currentPos[1])
                    {
                        segmentDrawn = true;
                        display += "@";
                    }
                    // Draw apple over snake segment.
                    if (foodPos[0] == currentPos[0] && foodPos[1] == currentPos[1])
                    {
                        appleDrawn = true;
                        display += "*";
                    }
                    else if (!segmentDrawn) display += "#";
                }
                // Draw apple if it wasn't already drawn.
                else if (foodPos[0] == currentPos[0] && foodPos[1] == currentPos[1] && !appleDrawn)
                    display += "*";
                else display += " ";
            }

            display += "█";
            display += "\n";
        }

        // Border Bottom
        for (int i = 0; i < 35; i++)
        {
            display += "█";
        }
        Console.WriteLine(display);
    }

    static int Game()
    {
        Console.Clear();

        int x = 1, y = 1;
        int direction = 1;
        int length = 0;
        List<int[]> posList = new() { new int[2] { x, y } }; 
        int gameRunning = 1; // -1 = Quit From Paused Menu, 0 = Game Over, 1 = Game Running
        int gameSpeed = 150;
        bool paused = false;
        int[] foodPos = new int[2] { 5, 5 };

        while (gameRunning == 1)
        {
            Thread.Sleep(gameSpeed);
            InputHandler(ref gameRunning, ref length, ref paused, ref direction, posList);

            // Clear the buffer to prevent the movement from "locking up."
            while (Console.KeyAvailable) Console.ReadKey(true);

            if (!paused) // Game Loop
            {

                // Direction Handler
                switch (direction)
                {
                    case 0: // Up
                        y--;
                        break;
                    case 2: // Down
                        y++;
                        break;
                    case 1: // Left
                        x++;
                        break;
                    case 3: // Right
                        x--;
                        break;
                }

                AppleChecker(ref length, ref foodPos, x, y);

                // Collision Check w/ Walls
                if (x == 34 || x == 0)
                {
                    gameRunning = 0;
                    continue;
                }
                if (y == 12 || y == 0)
                {
                    gameRunning = 0;
                    continue;
                }

                if (posList.Count > length)
                    posList.RemoveAt(0);

                // Collision Check w/ Self
                foreach (int[] pos in posList)
                    if (pos[0] == x && pos[1] == y)
                    {
                        gameRunning = 0;
                        continue;
                    }

                posList.Add(new int[2] { x, y });
                Draw(posList, foodPos, length);
            }
            else // Pause Menu
            {
                Console.SetCursorPosition(6, 5);
                Console.WriteLine("Paused. Esc to unpause.");
                Console.SetCursorPosition(7, 6);
                Console.WriteLine("M to goto main menu.");
            }
        }

        // Game Over Message
        if (gameRunning == 0)
        {
            Console.SetCursorPosition(12, 5);
            Console.WriteLine("Game Over!");
            Thread.Sleep(1000);
            // Just clear the buffer real quick to prevent the "Press any key." message from being skipped early.
            while (Console.KeyAvailable) Console.ReadKey(true);
            Console.SetCursorPosition(10, 6);
            Console.WriteLine("Press any key.");
            Console.ReadKey(true);
        }
        return length;
    }
}
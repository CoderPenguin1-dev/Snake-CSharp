class Program
{
    static readonly Random rng = new();
    const char snakeSegment = '#';

    static void AppleChecker(ref int length, ref int[] foodPos, int x, int y)
    {
        if (x == foodPos[0] && y == foodPos[1])
        {
            length++;
            foodPos[0] = rng.Next(1, 34); foodPos[1] = rng.Next(1, 11);
        }
    }

    static void DrawApple(int[] foodPos)
    {
        Console.SetCursorPosition(foodPos[0], foodPos[1]);
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("*");
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
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
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
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("===== Snake =====\n");
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

    static int Game()
    {
        Console.Clear();

        int x = 1, y = 1;
        int direction = 1;
        int length = 0;
        List<int[]> posList = new() { new int[2] { x, y } }; 
        int gameRunning = 1;
        int gameSpeed = 150;
        bool paused = false;
        int[] foodPos = new int[2] { 5, 5 };

        #region Draw Border
        Console.ForegroundColor = ConsoleColor.DarkGray;
        for (int i = 0; i < 35; i++)
        {
            Console.SetCursorPosition(i, 0);
            Console.Write("█");
        }
        for (int i = 0; i < 35; i++)
        {
            Console.SetCursorPosition(i, 11);
            Console.Write("█");
        }
        for (int i = 1; i < 11; i++)
        {
            Console.SetCursorPosition(0, i);
            Console.Write("█");
        }
        for (int i = 1; i < 11; i++)
        {
            Console.SetCursorPosition(34, i);
            Console.Write("█");
        }
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        #endregion

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
                Console.SetCursorPosition(x, y); Console.Write(snakeSegment); // Draw Snake Segment
                DrawApple(foodPos);

                // Redraw Right Border To Fix Removal Bug (please C# let me do stuff without having to hack you up with a hacksaw every 5 seconds..)
                Console.ForegroundColor = ConsoleColor.DarkGray;
                for (int i = 1; i < 11; i++)
                {
                    Console.SetCursorPosition(34, i);
                    Console.Write("█");
                }
                // Draw in score and set color for snake.
                Console.SetCursorPosition(13, 0);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"Score:{length:D3}");

                // Collision Check w/ Walls
                if (x == 34 || x == 0) gameRunning = 0;
                if (y == 11 || y == 0) gameRunning = 0;

                if (posList.Count > length)
                {
                    Console.SetCursorPosition(posList[0][0], posList[0][1]);
                    Console.Write(" ");
                    posList.RemoveAt(0);
                }

                // Done to prevent the snake from literally deleting itself visually.
                foreach (int[] pos in posList)
                {
                    Console.SetCursorPosition(pos[0], pos[1]);
                    Console.Write("#");
                }

                // Collision Check w/ Self
                foreach (int[] pos in posList)
                    if (pos[0] == x && pos[1] == y) gameRunning = 0;

                posList.Add(new int[2] { x, y });
            }
            else // Pause Menu
            {
                Console.SetCursorPosition(6, 5);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Paused. Esc to unpause.");
                Console.SetCursorPosition(7, 6);
                Console.WriteLine("M to goto main menu.");
            }
        }

        #region Game Over Message
        if (gameRunning == 0)
        {
            Console.SetCursorPosition(12, 5);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Game Over!");
            Thread.Sleep(1000);
            // Just clear the buffer real quick to prevent the "Press any key." message from being skipped early.
            while (Console.KeyAvailable) Console.ReadKey(true);
            Console.SetCursorPosition(10, 6);
            Console.WriteLine("Press any key.");
        }
        #endregion
        Console.ReadKey(true);
        if (gameRunning == 0) return length;
        else return 0;
    }
}
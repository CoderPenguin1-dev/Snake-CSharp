using System.Runtime.InteropServices;

class Program
{
    const int SNAKE_COLOR = 82;
    const int BORDER_COLOR = 255;
    const int APPLE_COLOR = 196;

    public static void Main(string[] args)
    {
        #region Color Stuffs For Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool GetConsoleMode(IntPtr handle, out int mode);
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetStdHandle(int handle);

            var handle = GetStdHandle(-11);
            GetConsoleMode(handle, out int mode);
            SetConsoleMode(handle, mode | 0x4);
        }
        #endregion
        Console.Title = "Snake"; Console.CursorVisible = false; // Setup the window.
        int previousLength = 0;
        Console.Write("\x1b[48;5;0m\x1b[38;5;255");

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
            Console.WriteLine("===================");
            Console.WriteLine("[1] Play Game");
            Console.WriteLine("[2] High Scores");
            Console.WriteLine("[3] Quit Game");
            while (true)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.D1)
                {
                    previousLength = GameLoop();
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

    static void AppleSpawnerChecker(ref int length, ref int[] foodPos, ref int gameRunning, int x, int y, List<int[]> posList)
    {
        Random rng = new();
        if (x == foodPos[0] && y == foodPos[1])
        {
            length++;
            int orgX = foodPos[0], orgY = foodPos[1];
            int[] currentPos = foodPos;
            bool validPosFound = false;
            // 816 due to screensize (34x12) multiplied by two for redundancy.
            for (int i = 0; i < 816; i++)
            {
                currentPos[0] = rng.Next(1, 34); currentPos[1] = rng.Next(1, 11);

                // We don't want it landing in the same place *or* in the player. Ignore the new placement if either apply.s
                if (currentPos.SequenceEqual(new int[] { orgX, orgY })) // Same Place
                    continue;
                if (posList.Any(p => p.SequenceEqual(currentPos))) // Player
                    continue;

                validPosFound = true;
                break;
            }
            if (validPosFound) foodPos = currentPos;
            else gameRunning = 0;
        }
    }

    // 0 = North, 1 = East, 2 = South, 3 = West
    static void InputHandler(ref int gameRunning, ref bool paused, ref int direction)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.UpArrow or ConsoleKey.W:
                    if (direction != 2 && !paused) direction = 0;
                    break;

                case ConsoleKey.DownArrow or ConsoleKey.S:
                    if (direction != 0 && !paused) direction = 2;
                    break;

                case ConsoleKey.LeftArrow or ConsoleKey.A:
                    if (direction != 1 && !paused) direction = 3;
                    break;

                case ConsoleKey.RightArrow or ConsoleKey.D:
                    if (direction != 3 && !paused) direction = 1;
                    break;

                case ConsoleKey.Escape:
                    paused = !paused;
                    break;

                case ConsoleKey.M:
                    if (paused) gameRunning = -1;
                    break;
            }
        }
    }

    static void DrawGame(List<int[]> posList, int[] foodPos, int length)
    {
        Console.SetCursorPosition(0, 0);

        // Border Top & Score
        string display = $"\x1b[38;5;{BORDER_COLOR}m";
        for (int i = 0; i < 35; i++)
        {
            if (i == 13)
            {
                display += $"Score:{length - 1:D3}";
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
                if (posList.Any(p => p.SequenceEqual(currentPos)))
                {
                    display += $"\x1b[38;5;{SNAKE_COLOR}m";
                    // Draw head of snake.
                    if (posList[^1][0] == currentPos[0] && posList[^1][1] == currentPos[1])
                        display += "@";
                    // If not the head, draw the body.
                    else display += "#";
                }
                // Draw apple.
                else if (foodPos[0] == currentPos[0] && foodPos[1] == currentPos[1])
                {
                    display += $"\x1b[38;5;{APPLE_COLOR}m";
                    display += "*";
                }

                // Draw nothing if it isn't a snake segment or apple.
                else display += " ";
            }

            display += $"\x1b[38;5;{BORDER_COLOR}m";
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

    static int GameLoop()
    {
        Console.Clear();

        int headX = 1, headY = 1;
        int direction = 1;
        int length = 1;
        List<int[]> posList = new() { new int[2] { headX, headY } }; 
        int gameRunning = 1; // -1 = Quit From Paused Menu, 0 = Game Over, 1 = Game Running
        int gameSpeed = 140;
        bool paused = false;
        int[] foodPos = new int[2] { 5, 5 };
        int[] previousTailPosition = posList[0]; // Used to change the end of the tail when the snake dies.

        while (gameRunning == 1)
        {
            Thread.Sleep(gameSpeed);
            InputHandler(ref gameRunning, ref paused, ref direction);

            // Clear the buffer to prevent the movement from "locking up."
            while (Console.KeyAvailable) Console.ReadKey(true);

            if (!paused) // Game Loop
            {

                // Direction Handler
                switch (direction)
                {
                    case 0: // Up
                        headY--;
                        break;
                    case 2: // Down
                        headY++;
                        break;
                    case 1: // Left
                        headX++;
                        break;
                    case 3: // Right
                        headX--;
                        break;
                }

                AppleSpawnerChecker(ref length, ref foodPos, ref gameRunning, headX, headY, posList);

                if (posList.Count > length)
                {
                    previousTailPosition = posList[0];
                    posList.RemoveAt(0);
                }

                // Collision Check w/ Walls
                // Contine statements there to prevent the snake from clipping into the wall.
                if (headX == 34 || headX == 0)
                    gameRunning = 0;
                if (headY == 12 || headY == 0)
                    gameRunning = 0;

                // Collision Check w/ Self
                foreach (int[] pos in posList)
                    if (pos[0] == headX && pos[1] == headY)
                        gameRunning = 0;

                if (gameRunning != 1) continue;
                posList.Add(new int[2] { headX, headY });
                DrawGame(posList, foodPos, length);
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
            #region Dead Snake
            foreach (int[] pos in posList)
            {
                Console.SetCursorPosition(pos[0], pos[1]);
                Console.Write("+");
            }
            Console.SetCursorPosition(previousTailPosition[0], previousTailPosition[1]);
            Console.Write("+");
            Console.SetCursorPosition(posList[^1][0], posList[^1][1]);
            Console.Write("X");
            #endregion

            Thread.Sleep(1000);
            Console.SetCursorPosition(12, 5);
            Console.WriteLine("Game Over!");
            // Just clear the buffer real quick to prevent the "Press any key." message from being skipped early.
            while (Console.KeyAvailable) Console.ReadKey(true);
            Console.SetCursorPosition(10, 6);
            Console.WriteLine("Press any key.");
            Console.ReadKey(true);
        }
        return length - 1;
    }
}
using System.Runtime.InteropServices;

class Program
{
    const int SNAKE_COLOR = 82;
    const int BORDER_COLOR = 255;
    const int APPLE_COLOR = 196;
    const int MESSAGE_COLOR = 196;
    const int BACKGROUND_COLOR = 0;
    const int TEXT_COLOR = 255;

    // When these values are subtracted or added to, it is to compensate for whatever bug happened.
    const int MAX_HORIZONTAL_PLAYSPACE = 34;
    const int MAX_VERTICAL_PLAYSPACE = 11;

    public static void Main(string[] args)
    {
        #region Color Stuff For Windows
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
        Console.Title = "Snake"; Console.CursorVisible = false;
        int previousSnakeSegmentAmount = 0;

        while (true)
        {
            #region New High Score Check
            if (previousSnakeSegmentAmount > 0)
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
                if (previousSnakeSegmentAmount > score)
                {
                    Console.WriteLine("New High Score!");
                    Console.Write("Please write your name: ");
                    string name = Console.ReadLine();

                    // Put new high score on top.
                    File.WriteAllText("scores.dat", $"{name};{previousSnakeSegmentAmount}");
                    foreach (string s in scores)
                        File.AppendAllText("scores.dat", "\n" + s);
                }
            }
            #endregion

            Console.Clear();
            Console.Write($"\x1b[48;5;{BACKGROUND_COLOR}m\x1b[38;5;{TEXT_COLOR}m");
            Console.WriteLine("====== Snake ======");
            Console.WriteLine($"Previous Score: {previousSnakeSegmentAmount:D3}");
            Console.WriteLine("===================");
            Console.WriteLine("[1] Play Game");
            Console.WriteLine("[2] High Scores");
            Console.WriteLine("[3] Quit Game");
            while (true)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.D1)
                {
                    previousSnakeSegmentAmount = GameLoop();
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

    static void AppleSpawnerChecker(ref int segments, ref int[] foodPosition, ref int gameRunning, int headX, int headY, List<int[]> segmentPositions)
    {
        Random rng = new();
        if (headX == foodPosition[0] && headY == foodPosition[1])
        {
            segments++;
            int orgX = foodPosition[0], orgY = foodPosition[1];
            int[] currentPos = foodPosition;
            bool validPosFound = false;
            // 816 due to screensize (34x11) multiplied by two for redundancy.
            for (int i = 0; i < 748; i++)
            {
                currentPos[0] = rng.Next(1, MAX_HORIZONTAL_PLAYSPACE); currentPos[1] = rng.Next(1, MAX_VERTICAL_PLAYSPACE);

                // We don't want it landing in the same place *or* in the player. Ignore the new placement if either apply.
                if (currentPos[0] == orgX && currentPos[1] == orgY) // Same Place
                    continue;
                if (segmentPositions.Any(p => p.SequenceEqual(currentPos))) // Player
                    continue;

                validPosFound = true;
                break;
            }
            if (validPosFound) foodPosition = currentPos;
            else gameRunning = 0;
        }
    }

    // 0 = North, 1 = East, 2 = South, 3 = West
    static void InputHandler(ref int gameRunning, ref bool gamePaused, ref int direction)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.UpArrow or ConsoleKey.W:
                    if (direction != 2 && !gamePaused) direction = 0;
                    break;

                case ConsoleKey.DownArrow or ConsoleKey.S:
                    if (direction != 0 && !gamePaused) direction = 2;
                    break;

                case ConsoleKey.LeftArrow or ConsoleKey.A:
                    if (direction != 1 && !gamePaused) direction = 3;
                    break;

                case ConsoleKey.RightArrow or ConsoleKey.D:
                    if (direction != 3 && !gamePaused) direction = 1;
                    break;

                case ConsoleKey.Escape:
                    gamePaused = !gamePaused;
                    break;

                case ConsoleKey.M:
                    if (gamePaused) gameRunning = -1;
                    break;
            }
        }
    }

    static void DrawGame(List<int[]> segmentPositions, int[] foodPosition, int segments)
    {
        Console.SetCursorPosition(0, 0);

        // Border Top & Score
        string display = $"\x1b[38;5;{BORDER_COLOR}m";
        for (int i = 0; i < MAX_HORIZONTAL_PLAYSPACE + 1; i++)
        {
            if (i == 13)
            {
                display += $"Score:{segments - 1:D3}";
                i += 8;
                continue;
            }
            display += "█";
        }
        display += "\n";

        // Border Sides & Game
        for (int y = 0; y < MAX_VERTICAL_PLAYSPACE; y++)
        {
            display += "█";
            bool lastPositionSnake = false;
            for (int x = 0; x < MAX_HORIZONTAL_PLAYSPACE - 1; x++)
            {
                int[] currentPos = { x + 1, y + 1 };
                if (segmentPositions.Any(p => p.SequenceEqual(currentPos)))
                {
                    if (!lastPositionSnake)
                    {
                        lastPositionSnake = true;
                        display += $"\x1b[38;5;{SNAKE_COLOR}m";
                    }
                    // Draw head of snake.
                    if (segmentPositions[^1][0] == currentPos[0] && segmentPositions[^1][1] == currentPos[1])
                    display += "@";
                    // If not the head, draw the body.
                    else display += "#";
                }
                // Draw apple.
                else if (foodPosition[0] == currentPos[0] && foodPosition[1] == currentPos[1])
                {
                    lastPositionSnake = false;
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
        for (int i = 0; i < MAX_HORIZONTAL_PLAYSPACE + 1; i++)
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
        int snakeSegments = 1;
        List<int[]> segmentPositions = new() { new int[2] { headX, headY } }; 
        // Used to change the end of the tail when the snake dies.
        // If not for this, the end of the tail would never be changed as it's not in the position list by the time the death checks are done.
        int[] previousTailPosition = segmentPositions[0];

        int gameRunning = 1; // -1 = Quit From Paused Menu, 0 = Game Over, 1 = Game Running
        int gameSpeed = 120;
        bool gamePaused = false;
        int[] foodPosition = new int[2] { 5, 5 };

        while (gameRunning == 1)
        {
            Thread.Sleep(gameSpeed);
            InputHandler(ref gameRunning, ref gamePaused, ref direction);

            // Clear the buffer to prevent the movement from "locking up."
            while (Console.KeyAvailable) Console.ReadKey(true);

            if (!gamePaused) // Game Loop
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

                AppleSpawnerChecker(ref snakeSegments, ref foodPosition, ref gameRunning, headX, headY, segmentPositions);

                if (segmentPositions.Count > snakeSegments)
                {
                    previousTailPosition = segmentPositions[0];
                    segmentPositions.RemoveAt(0);
                }

                // Collision Check w/ Walls
                if (headX == MAX_HORIZONTAL_PLAYSPACE || headX == 0)
                    gameRunning = 0;
                if (headY == MAX_VERTICAL_PLAYSPACE + 1 || headY == 0)
                    gameRunning = 0;

                // Collision Check w/ Self
                foreach (int[] pos in segmentPositions)
                    if (pos[0] == headX && pos[1] == headY)
                        gameRunning = 0;

                if (gameRunning != 1) continue;
                segmentPositions.Add(new int[2] { headX, headY });
                DrawGame(segmentPositions, foodPosition, snakeSegments);
            }
            else // Pause Menu
            {
                Console.Write($"\x1b[38;5;{MESSAGE_COLOR}m");
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
            foreach (int[] pos in segmentPositions)
            {
                Console.SetCursorPosition(pos[0], pos[1]);
                Console.Write("+");
            }
            Console.SetCursorPosition(previousTailPosition[0], previousTailPosition[1]);
            Console.Write("+");
            Console.SetCursorPosition(segmentPositions[^1][0], segmentPositions[^1][1]);
            Console.Write("X");
            #endregion

            Thread.Sleep(1000);
            Console.SetCursorPosition(12, 5);
            // Just clear the buffer real quick to prevent the "Press any key." message from being skipped early.
            while (Console.KeyAvailable) Console.ReadKey(true);
            Console.Write($"\x1b[38;5;{MESSAGE_COLOR}m");
            Console.WriteLine("Game Over!");
            Console.SetCursorPosition(10, 6);
            Console.WriteLine("Press any key.");
            Console.ReadKey(true);
        }
        return snakeSegments - 1;
    }
}
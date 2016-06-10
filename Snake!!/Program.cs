using System;
using System.Collections.Generic;
using System.Drawing;
using System.Timers;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Snake__
{
    enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    class Movement
    {
        public Direction Movedirection;

        public Movement()
        {
            Movedirection = Direction.Up;
        }
    }

    class Player
    {
        public String Name;
    }

    class Score
    {
        public int Value;

        public int AddToScore(int foodcounter)
        {
            Value += 100 + foodcounter * 10;
            return Value;
        }

    }
    class GameBoard
    {
        public int Width;
        public int Height;
        public int Foodcounter;
        public bool Gameover;
        

        public GameBoard()
        {
            Width = 30;
            Height = 20;
            Gameover = false;
            Foodcounter = 0;
        }
        public static void WriteAt(string s, int x, int y)
        {
            try
            {
                Console.SetCursorPosition(x, y);
                Console.Write(s);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
            }
        }
        public static void WriteAt(char s, int x, int y)
        {
            try
            {
                Console.SetCursorPosition(x, y);
                Console.Write(s);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
            }
        }

        // Method to draw the borders of the game field
        public void Drawborders()
        {
            // Draw left border
            for (int i = 1; i <= Height; i++)
            {
                WriteAt("|", 0, i);
            }

            // Draw right border
            for (int i = 1; i <= Height; i++)
            {
                WriteAt("|", Width, i);
            }

            // Draw top border
            for (int i = 1; i <= Width; i++)
            {
                WriteAt("-", i, 0);
            }

            // Draw bottom border
            for (int i = 1; i <= Width; i++)
            {
                WriteAt("-", i, Height + 1);
            }

            // Draw the four corners
            WriteAt("*", 0, 0);
            WriteAt("*", Width, Height);
            WriteAt("*", Width, 0);
            WriteAt("*", 0, Height);

        }
    }
    class Body
    {
        public List<Point> Location;

        public Body()
        {
            //Initializes the snake with 3 sections
            Location = new List<Point>();
            for (int i = 0; i < 3; i++)
            {
                Location.Add(new Point(10, 10 + i));
            }
        }
    }

    class Food
    {
        public Point Location;

        public Food()
        {
            Location = GenerateFoodLocation();
        }
        public Point GenerateFoodLocation()
        {
            Random rnd = new Random();
            Point temp = new Point
            {
                X = rnd.Next(30),
                Y = rnd.Next(20)
            };
            return temp;
        }
    }

    class Program
    {
        private static GameBoard _snakegame;
        private static Body _snakebody;
        private static Movement _snakedirection;
        private static Food _snakefood;
        private static Score _snakescore;
        private static Player _snakeplayer;


        public static Timer MyTimer = new Timer();

        // Timer will not function without this
        [STAThread]

        static void Main(string[] args)
        {
            // Create connection to SQL db
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SnakeSQL"].ConnectionString);

            // Initialize all the game objects
            _snakegame = new GameBoard();
            _snakebody = new Body();
            _snakedirection = new Movement();
            _snakefood = new Food();
            _snakescore = new Score();
            _snakeplayer = new Player();

            // Query player for their name
            Console.WriteLine("Welcome to Snake!!");
            Console.Write("What is your name? ");
            _snakeplayer.Name = Console.ReadLine();

            // Create references to stored procedures to get statistics on the provided name
            SqlCommand checkIfNewPlayerCommand = new SqlCommand("exec spCountPlayerName @Player_Name = '" + _snakeplayer.Name + "'", connection);
            SqlCommand getHighScoreCommand = new SqlCommand("exec spGetHighScore @Player_Name = '" + _snakeplayer.Name + "'", connection);
            SqlCommand getAvgScoreCommand = new SqlCommand("exec spGetAvgScore @Player_Name = '" + _snakeplayer.Name + "'", connection);
            SqlCommand getGameCountCommand = new SqlCommand("exec spGetGameCount @Player_Name = '" + _snakeplayer.Name + "'", connection);
            // SqlCommand getTop10ScoresCommand = new SqlCommand("exec spGetTop10Scores", connection);

            // Initialize connection to SQL db
            connection.Open();
            
            // If this is a returning player, present them with their stats
            if ((int) checkIfNewPlayerCommand.ExecuteScalar() >= 1)
            {
                int highscore = (int) getHighScoreCommand.ExecuteScalar();
                int avgscore = (int) getAvgScoreCommand.ExecuteScalar();
                int gamecount = (int) getGameCountCommand.ExecuteScalar();

                // Present player with their statistics and then start the game
                Console.Clear();
                Console.WriteLine("Hello, " + _snakeplayer.Name + "! Your current high score is " + highscore + ".");
                Console.WriteLine("Your average score is " + avgscore + " over " + gamecount + " rounds played.");
                
                
            }

            // If they are a new player, welcome them and move onwards to starting the game
            else
            {
                Console.WriteLine("Hello, " + _snakeplayer.Name + ", it looks like you're a new player! No statistics to display. ");
            }

            Console.WriteLine();
            Console.WriteLine("The current high scores are: ");
            LoadTop10().ForEach(Console.WriteLine);
            Console.WriteLine();
            Console.WriteLine("Press any key and the game will start.");
            connection.Close();

            // Game begins once a key is pressed
            Console.ReadKey();
            Console.Clear();
            // Snakegame.Drawborders();

            // Sleep for half a second.  This helps prevent the player from instantly losing the game if 
            // they start the game with the left arrow key (since the snake initially moves right)
            Thread.Sleep(500);

            // Beginning game timer
            MyTimer.Elapsed += DisplayTimeEvent;
            MyTimer.Interval = 100;
            MyTimer.Start();
            DrawScreen();
            while (true)
            {
                // Player can press Q to quit and upload their score
                if ((Keyboard.GetKeyStates(Key.Q) & KeyStates.Down) > 0)
                {
                    MyTimer.Stop();
                    break;
                }

                // These detect arrow key presses to determine movement
                if ((Keyboard.GetKeyStates(Key.Left) & KeyStates.Down) > 0)
                {
                    _snakedirection.Movedirection = Direction.Left;
                }
                if ((Keyboard.GetKeyStates(Key.Right) & KeyStates.Down) > 0)
                {
                    _snakedirection.Movedirection = Direction.Right;
                }
                if ((Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                {
                    _snakedirection.Movedirection = Direction.Up;
                }
                if ((Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                {
                    _snakedirection.Movedirection = Direction.Down;
                }

                // If the Gameover variable is true then break the loop
                if (_snakegame.Gameover)
                {
                    Console.SetCursorPosition(0, _snakegame.Height + 2);
                    Console.WriteLine("Game has ended");
                    break;
                }              
            }

            // Upload score to [Snake].[dbo].[tblGameScores]
            Console.WriteLine("Uploading your score");
            SqlCommand uploadCommand = new SqlCommand("exec spInsertNewScore @PLAYER_NAME = '" + _snakeplayer.Name + "', @SCORE = " + _snakescore.Value, connection);

            connection.Open();
            uploadCommand.ExecuteNonQuery();
            connection.Close();

            Console.WriteLine("Score has been uploaded");
            Console.WriteLine("Game has ended");

            // Game ends after this
            Console.ReadLine();
        }

        private static void DisplayTimeEvent(object source, ElapsedEventArgs e)
        {
            // Get current head position
            Point next = _snakebody.Location[0];

            // Determine where the head should go
            if (_snakedirection.Movedirection == Direction.Left)
                next = new Point(next.X - 1, next.Y);
            if (_snakedirection.Movedirection == Direction.Right)
                next = new Point(next.X + 1, next.Y);
            if (_snakedirection.Movedirection == Direction.Up)
                next = new Point(next.X, next.Y - 1);
            if (_snakedirection.Movedirection == Direction.Down)
                next = new Point(next.X, next.Y + 1);


            // If head hits a wall, set Gameover to true to end the game
            if (next.X == -1 | next.Y == -1 | next.X == _snakegame.Width || next.Y == _snakegame.Height)
            {
                Console.SetCursorPosition(0, _snakegame.Height + 1);
                Console.WriteLine(_snakeplayer.Name + ", you've achieved a score of " + _snakescore.Value + "!");
                MyTimer.Stop();
                _snakegame.Gameover = true;
            }
            // If snake hits itself, set Gameover to true to end the game
            else if (_snakebody.Location.Contains(next))
            {
                Console.SetCursorPosition(0, _snakegame.Height + 1);
                Console.WriteLine(_snakeplayer.Name + ", you've achieved a score of " + _snakescore.Value + "!");
                MyTimer.Stop();
                _snakegame.Gameover = true;
            }
            else
            {
                // Insert the new head into body
                _snakebody.Location.Insert(0, next);
                if(!(_snakegame.Gameover))
                    WriteAt("@", _snakebody.Location[0].X, _snakebody.Location[0].Y);

                // Remove tail from body, but don't do it if the head is on the food [head eats the food and snake gets longer]
                if (_snakebody.Location[0] != _snakefood.Location)
                {
                    WriteAt(".", _snakebody.Location[_snakebody.Location.Count - 1].X, _snakebody.Location[_snakebody.Location.Count - 1].Y);
                    _snakebody.Location.RemoveAt(_snakebody.Location.Count - 1);
                }

                // If head is on the food then generate & draw new food location
                // Also increase score
                if (_snakebody.Location[0] == _snakefood.Location)
                {
                    _snakefood.Location = _snakefood.GenerateFoodLocation();
                    WriteAt("#", _snakefood.Location.X, _snakefood.Location.Y);
                    _snakescore.Value = _snakescore.AddToScore(_snakegame.Foodcounter);
                    _snakegame.Foodcounter += 1;
                }
            }
        }

        // Method that connects to the SnakeSQL DB and gets a list of the top ten players and their scores
        // Then it converts it into a list of Strings and returns that list
        private static List<String> LoadTop10()
        {

            var listOfScores = new List<String>();
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SnakeSQL"].ConnectionString);
            connection.Open();
            
            using (var getTop10ScoresCommand = new SqlCommand("exec spGetTop10Scores", connection))
            {
                using (var reader = getTop10ScoresCommand.ExecuteReader())
                {
                    while (reader.Read())

                    {
                        listOfScores.Add(string.Format("{0}{1}", reader["PLAYER_NAME"].ToString().PadRight(20, ' '), reader["SCORE"]));
                    }
                }
            }
            return listOfScores;
        }

        // Method to draw a string starting at a certain position in the console
        public static void WriteAt(string s, int x, int y)
        {
            try
            {
                Console.SetCursorPosition(x, y);
                Console.Write(s);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
            }
        }

        // Method to draw the borders of the game field
        // Currently unused
        public void Drawborders(int width, int height)
        {
            // Draw left border
            for (int i = 1; i < height; i++)
            {
                WriteAt("|", 0, i);
            }

            // Draw right border
            for (int i = 1; i < height; i++)
            {
                WriteAt("|", i, 0);
            }

            // Draw top border
            for (int i = 1; i < width; i++)
            {
                WriteAt("-", 0, i);
            }

            // Draw bottom border
            for (int i = 1; i < width; i++)
            {
                WriteAt("-", i, 0);
            }

            // Draw the four corners
            WriteAt("*", 0, 0);
            WriteAt("*", width, height);
            WriteAt("*", width, 0);
            WriteAt("*", 0, height);

        }

        // Method that draws the initial game screen
        public static void DrawScreen()
        {
            // Console.Clear();
            Console.SetCursorPosition(0, 0);

            char[,] screen = new char[_snakegame.Width, _snakegame.Height];

            // Fill with background
            for (int x = 0; x < _snakegame.Width; ++x)
                for (int y = 0; y < _snakegame.Height; ++y)
                    screen[x, y] = '.';
            // Update with food location
            screen[_snakefood.Location.X, _snakefood.Location.Y] = '#';

            // Update with snake location
            foreach (Point point in _snakebody.Location)
            {
                screen[point.X, point.Y] = '@';
            }

            // Render screen to console
            for (int y = 0; y < _snakegame.Height; ++y)
            {
                for (int x = 0; x < _snakegame.Width; ++x)
                {
                    Console.Write(screen[x, y]);
                }
                Console.WriteLine();
            }
        }

    }
}

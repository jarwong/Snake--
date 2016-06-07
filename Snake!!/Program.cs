using System;
using System.Collections.Generic;
using System.Drawing;
using System.Timers;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Configuration;
using System.Net;
using System.Net.NetworkInformation;

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
            Movedirection = Direction.Right;
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
            Width = 31;
            Height = 21;
            Gameover = false;
            Foodcounter = 0;
        }
    }
    class Body
    {
        public List<Point> Snakebody;

        public Body()
        {
            Snakebody = new List<Point>();
            for (int i = 0; i < 3; i++)
            {
                Snakebody.Add(new Point(10, 10 + i));
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
                X = rnd.Next(31),
                Y = rnd.Next(21)
            };
            return temp;
        }
    }

    class Program
    {
        public static GameBoard Snakegame;
        public static Body Snakebody;
        public static Movement Snakedirection;
        public static Food Snakefood;
        public static Score Snakescore;
        public static Player Snakeplayer;


        public static Timer MyTimer = new Timer();

        [STAThread]
        static void Main(string[] args)
        {
            // Initialize connection to SQL db
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SnakeSQL"].ConnectionString);

            // Initialize all the game objects
            Snakegame = new GameBoard();
            Snakebody = new Body();
            Snakedirection = new Movement();
            Snakefood = new Food();
            Snakescore = new Score();
            Snakeplayer = new Player();

            // Query player for their name
            Console.WriteLine("Welcome to Snake!!");
            Console.Write("What is your name? ");
            Snakeplayer.Name = Console.ReadLine();

            // Run stored procedures to get statistics on the provided name
            SqlCommand checkIfNewPlayerCommand = new SqlCommand("exec spCountPlayerName @Player_Name = '" + Snakeplayer.Name + "'", connection);
            SqlCommand getHighScoreCommand = new SqlCommand("exec spGetHighScore @Player_Name = '" + Snakeplayer.Name + "'", connection);
            SqlCommand getAvgScoreCommand = new SqlCommand("exec spGetAvgScore @Player_Name = '" + Snakeplayer.Name + "'", connection);
            SqlCommand getGameCountCommand = new SqlCommand("exec spGetGameCount @Player_Name = '" + Snakeplayer.Name + "'", connection);
            SqlCommand getTop10ScoresCommand = new SqlCommand("exec spGetTop10Scores", connection);
            connection.Open();

            List<String> top10Scores = new List<string>();
            
            
            // If this is a returning player, present them with their stats
            if ((int) checkIfNewPlayerCommand.ExecuteScalar() >= 1)
            {
                int highscore = (int) getHighScoreCommand.ExecuteScalar();
                int avgscore = (int) getAvgScoreCommand.ExecuteScalar();
                int gamecount = (int) getGameCountCommand.ExecuteScalar();

                // Present player with their statistics and then start the game
                Console.Clear();
                Console.WriteLine("Hello, " + Snakeplayer.Name + "! Your current high score is " + highscore + ".");
                Console.WriteLine("Your average score is " + avgscore + " over " + gamecount + " rounds played.");
                
                
            }
            //If they are a new player, welcome them and move onwards to starting the game
            else
            {
                Console.WriteLine("Hello, " + Snakeplayer.Name + ", it looks like you're a new player! No statistics to display. ");
            }

            Console.WriteLine();
            Console.WriteLine("The current high scores are: ");
            LoadTop10().ForEach(Console.WriteLine);
            Console.WriteLine();
            Console.WriteLine("Press any key and the game will start.");
            connection.Close();




            Console.ReadKey();

            // Beginning game timer
            MyTimer.Elapsed += DisplayTimeEvent;
            MyTimer.Interval = 100;
            MyTimer.Start();

            while (true)
            {
                // Player can press Q to quit
                if ((Keyboard.GetKeyStates(Key.Q) & KeyStates.Down) > 0)
                {
                    break;
                }

                // These detect arrow key presses to determine movement
                if ((Keyboard.GetKeyStates(Key.Left) & KeyStates.Down) > 0)
                {
                    Snakedirection.Movedirection = Direction.Left;
                }
                if ((Keyboard.GetKeyStates(Key.Right) & KeyStates.Down) > 0)
                {
                    Snakedirection.Movedirection = Direction.Right;
                }
                if ((Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0)
                {
                    Snakedirection.Movedirection = Direction.Up;
                }
                if ((Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0)
                {
                    Snakedirection.Movedirection = Direction.Down;
                }

                // If the Gameover variable is true then the game will end and break the loop
                if (Snakegame.Gameover)
                {
                    Console.WriteLine("Game has ended");
                    break;
                }              
            }

            // Upload score to [Snake].[dbo].[tblGameScores]
            Console.WriteLine("Uploading your score");
            SqlCommand uploadCommand = new SqlCommand("INSERT INTO [dbo].[tblGameScores] (PLAYER_NAME, SCORE) Values ('" + Snakeplayer.Name + "', " + Snakescore.Value + ");", connection);

            connection.Open();
            uploadCommand.ExecuteNonQuery();
            connection.Close();

            Console.WriteLine("Score has been uploaded");
            Console.WriteLine("Game has ended");

            Console.ReadLine();
        }

        public static void DisplayTimeEvent(object source, ElapsedEventArgs e)
        {
            Console.Clear();

            char[,] render = new char[Snakegame.Width, Snakegame.Height];

            // Fill with background
            for (int x = 0; x < Snakegame.Width; ++x)
                for (int y = 0; y < Snakegame.Height; ++y)
                    render[x, y] = '.';
            // Update with food location
            render[Snakefood.Location.X, Snakefood.Location.Y] = '#';

            // Update with snake location
            foreach (Point point in Snakebody.Snakebody)
            {
                render[point.X, point.Y] = '@';
            }



            // Render to console
            for (int y = 0; y < Snakegame.Height; ++y)
            {
                for (int x = 0; x < Snakegame.Width; ++x)
                {
                    Console.Write(render[x, y]);
                }
                Console.WriteLine();
            }
            

            // Remove tail from body, but don't do it if the head is on the food
            if (Snakebody.Snakebody[0] != Snakefood.Location)
            {
                Snakebody.Snakebody.RemoveAt(Snakebody.Snakebody.Count - 1);
            }
            // If head is on the food then generate new food location
            // Also increase score
            if (Snakebody.Snakebody[0] == Snakefood.Location)
            {
                Snakefood.Location = Snakefood.GenerateFoodLocation();
                Snakescore.Value = Snakescore.AddToScore(Snakegame.Foodcounter);
                Snakegame.Foodcounter += 1;
            }


            // Get head position
            Point next = Snakebody.Snakebody[0];

            // Determine where the head should be
            if (Snakedirection.Movedirection == Direction.Left)
                next = new Point(next.X - 1, next.Y);
            if (Snakedirection.Movedirection == Direction.Right)
                next = new Point(next.X + 1, next.Y);
            if (Snakedirection.Movedirection == Direction.Up)
                next = new Point(next.X, next.Y - 1);
            if (Snakedirection.Movedirection == Direction.Down)
                next = new Point(next.X, next.Y + 1);

            // If snake hits itself, end the game
            if (Snakebody.Snakebody.Contains(next))
            {
                Console.WriteLine(Snakeplayer.Name + ", you've achieved a score of " + Snakescore.Value + "!");
                MyTimer.Stop();
                Snakegame.Gameover = true;
            }


            // Insert the new head into body
            Snakebody.Snakebody.Insert(0, next);

            // If head hits a wall, end the game
            if (Snakebody.Snakebody[0].X == -1 | Snakebody.Snakebody[0].Y == -1 | Snakebody.Snakebody[0].X == Snakegame.Width || Snakebody.Snakebody[0].Y == Snakegame.Height)
            {
                
                Console.WriteLine(Snakeplayer.Name + ", you've achieved a score of " + Snakescore.Value + "!");
                MyTimer.Stop();
                Snakegame.Gameover = true;
            }

        }

        public static List<String> LoadTop10()
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
                        listOfScores.Add(string.Format("{0}\t{1}", reader["PLAYER_NAME"].ToString(), reader["SCORE"].ToString()));
                    }
                }
            }

            return listOfScores;


        } 


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Timers;
using System.Windows.Input;


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
    class GameBoard
    {
        public int width;
        public int height;

        public GameBoard()
        {
            width = 31;
            height = 21;
        }
    }
    class Body
    {
        public List<Point> snakebody;

        public Body()
        {
            snakebody = new List<Point>();
            for (int i = 0; i < 3; i++)
            {
                snakebody.Add(new Point(10, 10 + i));
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
            Point temp = new Point();
            temp.X = rnd.Next(31);
            temp.Y = rnd.Next(21);
            return temp;
        }
    }

    class Program
    {
        public static GameBoard Snakegame;
        public static Body Snakebody;
        public static Movement Snakedirection;
        public static Food Snakefood;

        [STAThread]
        static void Main(string[] args)
        {

            Timer myTimer = new Timer();
            myTimer.Elapsed += new ElapsedEventHandler(DisplayTimeEvent);
            myTimer.Interval = 200;
            myTimer.Start();

            Snakegame = new GameBoard();
            Snakebody = new Body();
            Snakedirection = new Movement();
            Snakefood = new Food();


            while (true)
            {
                if ((Keyboard.GetKeyStates(Key.Q) & KeyStates.Down) > 0)
                {
                    break;
                }
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
                //Console.WriteLine("test");
            }
        }

        public static void DisplayTimeEvent(object source, ElapsedEventArgs e)
        {
            Console.Clear();

            char[,] render = new char[Snakegame.width, Snakegame.height];

            // Fill with background
            for (int x = 0; x < Snakegame.width; ++x)
                for (int y = 0; y < Snakegame.height; ++y)
                    render[x, y] = '.';

            // Update with snake location
            foreach (Point point in Snakebody.snakebody)
            {
                render[point.X, point.Y] = '@';
            }

            // Update with food location
            render[Snakefood.Location.X, Snakefood.Location.Y] = '#';

            // Render to console
            for (int y = 0; y < Snakegame.height; ++y)
            {
                for (int x = 0; x < Snakegame.width; ++x)
                {
                    Console.Write(render[x, y]);
                }
                Console.WriteLine();
            }

           

            // Remove tail from body, but don't do it if the head is on the food
            if (Snakebody.snakebody[0] != Snakefood.Location)
            {
                Snakebody.snakebody.RemoveAt(Snakebody.snakebody.Count - 1);
            }
            if (Snakebody.snakebody[0] == Snakefood.Location)
            {
                Snakefood.Location = Snakefood.GenerateFoodLocation();
            }
            

            // Get head position
            Point next = Snakebody.snakebody[0];

            // Calculate where the head should be next based on the snake's direction
            if (Snakedirection.Movedirection == Direction.Left)
                next = new Point(next.X - 1, next.Y);
            if (Snakedirection.Movedirection == Direction.Right)
                next = new Point(next.X + 1, next.Y);
            if (Snakedirection.Movedirection == Direction.Up)
                next = new Point(next.X, next.Y - 1);
            if (Snakedirection.Movedirection == Direction.Down)
                next = new Point(next.X, next.Y + 1);

            // Insert new head into the snake's body
            Snakebody.snakebody.Insert(0, next);
        }

        // Array with map characters
    }
}

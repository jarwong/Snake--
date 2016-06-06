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
            width = 30;
            height = 20;
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

    class Program
    {
        public static GameBoard Snakegame;
        public static Body Snakebody;
        public static Movement Snakedirection;

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
            //while ((Keyboard.GetKeyStates(Key.Q) & KeyStates.Down) > 0)
            //{
            //    //Console.WriteLine("derp"); 
            //    ;
            //}
        }

        //[STAThread]
        public static void DisplayTimeEvent(object source, ElapsedEventArgs e)
        {
            Console.Clear();
            char[,] render = new char[Snakegame.width, Snakegame.height];

            // Fill with background
            for (int x = 0; x < Snakegame.width; ++x)
                for (int y = 0; y < Snakegame.height; ++y)
                    render[x, y] = '.';

            foreach (Point point in Snakebody.snakebody)
            {
                render[point.X, point.Y] = '#';
            }

            // Render to console
            for (int y = 0; y < Snakegame.height; ++y)
            {
                for (int x = 0; x < Snakegame.width; ++x)
                {
                    Console.Write(render[x, y]);
                }
                Console.WriteLine();
            }

            // Remove tail from body
            Snakebody.snakebody.RemoveAt(Snakebody.snakebody.Count - 1);

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

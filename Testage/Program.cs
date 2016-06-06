using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Testage
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Test");
            List<Point> snakebody = new List<Point>();

            //These will be the dimensions of the playing field
            int width = 20;
            int height = 15;


            //Create the initial snake with 3 sections
            for (int i = 0; i < 3; i++)
            {
                snakebody.Add(new Point(5, 2 + i));
            }

            Timer myTimer = new Timer();
            myTimer.Elapsed += new ElapsedEventHandler(UpdateSnake);
            myTimer.Interval = 700;
            myTimer.Start();
            while (true)
            {
                if ((Keyboard.GetKeyStates(Key.Q) & KeyStates.Down) > 0)
                {
                    break;
                }
            }

        }

        public static void UpdateSnake(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Merpage");
        }

    }

}

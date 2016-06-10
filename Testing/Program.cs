using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;

namespace Testing
{
    class Program
    {

        static void Main(string[] args)
        {
            //SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SnakeSQL"].ConnectionString);

            //Console.WriteLine("Inserting name!");
            //SqlCommand myCommand = new SqlCommand("INSERT INTO [dbo].[tblPlayers] (Id, Name) Values (1, 'TestUser');", connection);
            //myCommand.ExecuteNonQuery();
            //Console.ReadLine();

            GameBoard gametest = new GameBoard();
            gametest.Drawborders();
            Console.ReadLine();
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
                WriteAt("|", Width + 1, i);
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
            WriteAt("*", Width + 1, Height + 1);
            WriteAt("*", Width + 1, 0);
            WriteAt("*", 0, Height + 1);

        }
    }
}

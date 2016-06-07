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
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SnakeSQL"].ConnectionString);

            Console.WriteLine("Inserting name!");
            SqlCommand myCommand = new SqlCommand("INSERT INTO [dbo].[tblPlayers] (Id, Name) Values (1, 'TestUser');", connection);
            myCommand.ExecuteNonQuery();
            Console.ReadLine();

        }

    }
}

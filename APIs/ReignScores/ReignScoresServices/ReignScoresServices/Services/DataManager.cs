using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReignScores.Services
{
	public static class DataManager
	{
		public static SqlConnection CreateConnectionObject()
		{
			// Test URL
			return new SqlConnection
			(
				@"Data Source=tcp:localhost;Initial Catalog=ReignScores;User ID=sa;Password=TestPass"
			);
		}
	}
}

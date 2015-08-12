using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReignPushNotificationServices.Services
{
	public static class DataManager
	{
		public static SqlConnection CreateConnectionObject()
		{
			// Test URL
			return new SqlConnection
			(
				@"Data Source=tcp:localhost;Initial Catalog=ReignPushNotifications;User ID=sa;Password=TestPass"
			);
		}
	}
}

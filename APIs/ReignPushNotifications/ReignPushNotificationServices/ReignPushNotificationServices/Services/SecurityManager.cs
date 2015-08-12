using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ReignPushNotificationServices.Services
{
	public static class SecurityManager
	{
		public static string Hash(string value)
		{
			SHA256 sha = SHA256Managed.Create();
			var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
			string hexValue = "";
			foreach (byte x in hash)
			{
				hexValue += x.ToString("x");
			}

			return hexValue;
		}
	}
}

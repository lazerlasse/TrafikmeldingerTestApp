using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TrafikmeldingerTestApp.Services
{
	public class Pushover
	{
		private readonly Uri pushoverUri = new("https://api.pushover.net/1/messages.json");

		public string SendMessage(string message)
		{
			NameValueCollection parameters = new()
			{
				{ "token", "auzub1ddcxm3kzi4ewwnj8hw5qykiz" },
				{ "user", "gh6ys2zv9if96pqvfxeyhjicibayz8" },
				{ "message", message }
			};

			using WebClient client = new();

			try
			{
				client.UploadValuesAsync(pushoverUri, parameters);
			}
			catch (Exception e)
			{
				return "Fejl" + e.Message;
			}

			return "Melding Sendt";
		}
	}
}

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using QuickType;

namespace WeeBot.Modules
{
	// Create a module with no prefix
	public class InfoModule : ModuleBase<SocketCommandContext>
	{
		// ~say hello world -> hello world
		[Command("say")]
		[Summary("Echoes a message.")]
		public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
			=> ReplyAsync(echo);

		// ReplyAsync is a method on ModuleBase 
	}

	public class Commands : ModuleBase<SocketCommandContext>
	{

        // ~say hello world -> hello world
        [Command("ping")]
		public async Task Ping()
		{
			await ReplyAsync("pong");
		}

		[Command("randomemoji")]
		public async Task RandomEmoji()
        {
			List<string> myList = new List<string>()
			{
				"\uD83C\uDF61",
				"\uD83D\uDC1D",
				"\uD83D\uDCA2",
				"\u0023"
			};
			Random r = new Random();
			int index = r.Next(myList.Count);
			string randomString = myList[index];

			await ReplyAsync(randomString);
		}

		[Command("image")]
		public async Task Image()
		{

			string chars = "01234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghiklmnopqrstuvwxyz";
			var stringlength = 7; /* could be 6 or 7, but takes forever because there are lots of dead images */
				var text = "";
				for (var i = 0; i < stringlength; i++)
				{
					Random r = new Random();
					int rnum = r.Next(chars.Length);
					text += chars.Substring(rnum, 1);
				}

				var source = "https://i.imgur.com/" + text + ".jpg";

				var client = new HttpClient();
				HttpResponseMessage result = await client.GetAsync(source);

			string responseUri = result.RequestMessage.RequestUri.ToString();

			if (responseUri == "https://i.imgur.com/removed.png")
			{
				Image();
			}
			else
			{

				var builder = new EmbedBuilder()
						.WithImageUrl(source.ToString())
						.WithColor(new Color(33, 176, 252))
						.WithTitle("test")
						.WithUrl(source.ToString())
						.WithFooter(source);
				var embed = builder.Build();
				await Context.Channel.SendMessageAsync(null, false, embed);
			}
		}

		[Command("imgurAPI")]
		public async Task ImgurApi()
        {

			var client = new HttpClient();
			var request = new HttpRequestMessage(HttpMethod.Get, "https://api.imgur.com/3/gallery/random/random");

			var byteArray = new UTF8Encoding().GetBytes("5080de90f5f4854");
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", "5080de90f5f4854");

			Console.WriteLine(client.DefaultRequestHeaders);

			var response = await client.SendAsync(request);

			Console.WriteLine(await response.Content.ReadAsStringAsync());

			var responseJson = await response.Content.ReadAsAsync<JsonInfo>();

				Console.WriteLine("----------------------------------------------------------");
				Console.WriteLine(responseJson.Title);
				Console.WriteLine("----------------------------------------------------------");

		}

		[Command("meme")]
		[Alias("reddit")]
		public async Task Meme(string subreddit = null)
        {
			var client = new HttpClient();
			var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");
            if (!result.StartsWith("["))
            {
				await Context.Channel.SendMessageAsync("This subreddit doesn't exist");
				return;
            }
			JArray arr = JArray.Parse(result);
			JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

			var builder = new EmbedBuilder()
				.WithImageUrl(post["url"].ToString())
				.WithColor(new Color(33, 176, 252))
				.WithTitle(post["title"].ToString())
				.WithUrl("https://reddit.com" + post["permalink"].ToString())
				.WithFooter($"🗨 {post["num_comments"]} ⬆️ {post["ups"]}");
				var embed = builder.Build();
			await Context.Channel.SendMessageAsync(null, false, embed);


		}
	}

	// Create a module with the 'sample' prefix
	[Group("sample")]
	public class SampleModule : ModuleBase<SocketCommandContext>
	{
		// ~sample square 20 -> 400
		[Command("square")]
		[Summary("Squares a number.")]
		public async Task SquareAsync(
			[Summary("The number to square.")]
		int num)
		{
			// We can also access the channel from the Command Context.
			await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
		}

		// ~sample userinfo --> foxbot#0282
		// ~sample userinfo @Khionu --> Khionu#8708
		// ~sample userinfo Khionu#8708 --> Khionu#8708
		// ~sample userinfo Khionu --> Khionu#8708
		// ~sample userinfo 96642168176807936 --> Khionu#8708
		// ~sample whois 96642168176807936 --> Khionu#8708
		[Command("userinfo")]
		[Summary
		("Returns info about the current user, or the user parameter, if one passed.")]
		[Alias("user", "whois")]
		public async Task UserInfoAsync(
			[Summary("The (optional) user to get info from")]
		SocketUser user = null)
		{
			var userInfo = user ?? Context.Client.CurrentUser;
			await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
		}
	}
}

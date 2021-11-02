using Microsoft.Office.Interop.Excel;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HtmlAgilityPack;
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
using WeeBot.Models;

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
			var stringlength = 5;
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
				await Image();
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

		[Command("imgurSpam")]
		public async Task ImgurSpam(int amount = 0)
        {

			var client = new HttpClient();
			var request = new HttpRequestMessage(HttpMethod.Get, "https://api.imgur.com/3/gallery/random/random");

			var byteArray = new UTF8Encoding().GetBytes("5080de90f5f4854");
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", "5080de90f5f4854");

			var response = await client.SendAsync(request);

			var responseJson = await response.Content.ReadAsAsync<JsonInfo>();

			if(amount == 0)
            {
				amount = responseJson.Test.Count();
            }

			foreach (var obj in responseJson.Test.Take(amount))
			{
				var builder = new EmbedBuilder()
						.WithImageUrl(obj.Link)
						.WithColor(new Color(33, 176, 252))
						.WithTitle("test")
						.WithUrl(obj.Link);
				var embed = builder.Build();
				await Context.Channel.SendMessageAsync(null, false, embed);
			}

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

		[Command("companies")]
		public async Task Companies() {
			Application myApp;
			Workbook myWorkBook;
			Worksheet myWorkSheet;
			myApp = new Application();
			myWorkBook = myApp.Workbooks.Add();
			myWorkSheet = (Worksheet)myWorkBook.Worksheets.get_Item(1);

			var client = new HttpClient();
				var result = await client.GetStringAsync("https://www.ft.com/americas-fastest-growing-companies-2021");

				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(result);

				var tableList = htmlDocument.DocumentNode.Descendants("table")
					.Where(node => node.GetAttributeValue("class", "")
					.Equals("o-table o-table--row-stripes o-table--compact o-table--responsive-overflow")).ToList();

				var companyList = tableList[0].Descendants("tr").ToList();

			var rw = 1;

				foreach (HtmlNode row in companyList)
				{
				var cl = 1;

				foreach (HtmlNode cell in row.SelectNodes("th|td"))
                {
					myWorkSheet.Cells[rw, cl] = cell.InnerText;
					cl++;
				}

				if(rw == 1)
                {
					myWorkSheet.Cells[rw, cl + 1] = "Stock price";
                }

					var cellList = row.SelectNodes("th|td");
					HtmlNode cell1 = htmlDocument.CreateElement("div");
					HtmlNode cell2 = htmlDocument.CreateElement("div");
				try
				{
					cell1 = cellList[2];
					cell2 = cellList[11];
				}
				catch(Exception e)
                {

                }
					if (cell2.InnerText == "Yes")
					{
						try
						{
							client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, */*; q=0.8");
							client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "peerdist");
							client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US");
							client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0) Gecko/20100101 Firefox/93.0");
							var result2 = await client.GetStringAsync("https://markets.businessinsider.com/searchresults?_search=" + cell1.InnerText);

							var htmlDocument2 = new HtmlDocument();
							htmlDocument2.LoadHtml(result2);

							var div = htmlDocument2.DocumentNode.Descendants("div")
							.Where(node => node.GetAttributeValue("class", "")
							.Equals("price-section__values")).ToList();

							var price = div[0].SelectNodes("span");

							Console.WriteLine($"Price: {div[0].InnerText}");

							cl++;
							myWorkSheet.Cells[rw, cl] = div[0].InnerText;
						}
						catch (Exception e)
						{

						}

					}
				rw++;
				}
				myWorkSheet.SaveAs("C:/Users/Gilgamesh/Desktop/Company.xlsx");
				myWorkBook.Close();
				myApp.Quit();
		}

		//[Command("join", RunMode = RunMode.Async)]
		//public async Task JoinChannel(IVoiceChannel channel = null)
		//{
		//	channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
		//	if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

		//	try
		//	{
		//		var audioClient = await channel.ConnectAsync();
  //          }
		//	catch (Exception exception)
		//        {
		//            await ReplyAsync(exception.Message);
		//        }
		//	}

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
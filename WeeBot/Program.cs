using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using WeeBot.Modules;
using WeeBot.Services;

namespace WeeBot
{
	public class Program
	{

		private DiscordSocketClient _client;
		private CommandService _commands;
		private IServiceProvider _services;
		private LavaNode _lavaNode;
		private AudioService _audioService;
		private AudioModule _soundModule;
		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			_client = new DiscordSocketClient();

			_commands = new CommandService();

			_services = new ServiceCollection()
				.AddSingleton<AudioService>()
				.AddSingleton(_client)
				.AddSingleton(_commands)
				.AddSingleton<AudioModule>()
				.AddLavaNode(x =>
				{
					x.SelfDeaf = false;
					x.Authorization = "WeeBotLink2333";
				})
				.BuildServiceProvider();

			_client.Log += Log;
			_client.Ready += OnReadyAsync;
			_lavaNode = _services.GetRequiredService<LavaNode>();
			_audioService = _services.GetRequiredService<AudioService>();
			_soundModule = _services.GetRequiredService<AudioModule>();
			_lavaNode.OnTrackEnded += _soundModule.OnTrackEnded;

			//  You can assign your bot token to a string, and pass that in to connect.
			//  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
			var token = "ODU0NjgwMTkwMjk0MDMyMzk0.YMnc9A.XqzoUZG5Fm912j2Rj8uKeKfyhV4";

			// Some alternative options would be to keep your token in an Environment Variable or a standalone file.
			// var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
			// var token = File.ReadAllText("token.txt");
			// var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

			await RegisterCommandsAsync();

			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();

			// Block this task until the program is closed.
				await Task.Delay(-1);
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		private async Task OnReadyAsync()
		{
			if (!_lavaNode.IsConnected)
			{
				await _lavaNode.ConnectAsync();
			}

		}

		public async Task RegisterCommandsAsync()
		{
			_client.MessageReceived += HandleCommandAsync;
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
		}

		private async Task HandleCommandAsync(SocketMessage arg)
		{
			var message = arg as SocketUserMessage;
			var context = new SocketCommandContext(_client, message);
			if (message.Author.IsBot) return;

			int argPos = 0;
			if (message.HasStringPrefix("~", ref argPos))
			{
				var result = await _commands.ExecuteAsync(context, argPos, _services);
				Console.WriteLine(message);
				Console.WriteLine(result);
				if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
				if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);
			}
		}
	}
}

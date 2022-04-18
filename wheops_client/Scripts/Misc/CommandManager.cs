using System.Collections.Generic;

public static class CommandManager {
	public delegate void CommandHandler(string[] args);
	public static Dictionary<string, CommandHandler> CommandHandlers = new Dictionary<string, CommandHandler>();

	public static void Init() {
		CommandHandlers = new Dictionary<string, CommandHandler>() {
			{ "exit",			CMD_Exit },
			{ "clear",			CMD_Clear },
			{ "list", 			CMD_List },
			{ "debug", 			CMD_Debug },
			{ "connect", 			CMD_Connect },
			{ "say", 			CMD_Say },
		};

		Logger.Info("CommandManager initialized");
	}

	public static void CMD_Clear(string[] args) {
		Console.Instance.ClearOutput();
		Console.Instance.ClearInput();
	}

	public static void CMD_List(string[] args) {
		string output="";
		foreach(string cmd in CommandHandlers.Keys) {
			output += " - " + cmd + "\n";
		}

		Console.Instance.Print(output);
	}

	public static void CMD_Exit(string[] args) { 
		Console.Instance.PrintWarning("closing the game!");
		Global.Instance.GetTree().Quit();
	}

	public static void CMD_Debug(string[] args) {
		if(args.Length < 1) {
			Console.Instance.PrintError("debug [on,off*]");
		}

		switch(args[0]) {
			case "on":
				Logger.Info("Debug Label is on");
				DebugLabel.Instance.Show();
				break;

			case "off":
				Logger.Info("Debug Label is off");
				DebugLabel.Instance.Hide();
				break;

			default:
				Logger.Error("debug [on,off*]");
				break;
		}
	}

	public static void CMD_Connect(string[] args) {
		short port;
		string ip, nickname;
		
		if(args.Length < 2) {
			Logger.Error("connect [nickname*] [ip*] [port]");
			return;
		}

		nickname = args[0];
		ip = args[1];
		
		if(args.Length < 3) {
			port = 26950;
			Logger.Error("Port not specified, using 26950 by default");
		} else {
			if(!short.TryParse(args[2], out port)) {
				Logger.Error("connect [nickname*] [ip*] [port]");
				return;
			}
		}

		Logger.Info($"Trying to connect to {ip}:{port} as {nickname}...");
	}
	
	public static void CMD_Say(string[] args) {
	}
}

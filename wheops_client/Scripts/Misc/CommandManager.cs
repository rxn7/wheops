using Godot;
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
			{ "ch_gap", 			CMD_CrosshairGap },
			{ "ch_color", 			CMD_CrosshairColor },
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
		Logger.Info("Closing the game!");
		Global.Instance.GetTree().Quit();
	}

	public static void CMD_Debug(string[] args) {
		void PrintUsage() => Logger.Error("debug [on,off*]");

		if(args.Length < 1) {
			PrintUsage();
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
				PrintUsage();
				break;
		}
	}

	public static void CMD_Connect(string[] args) {
		void PrintUsage() => Logger.Error("connect [nickname*] [ip*] [port]");

		short port;
		string ip, nickname;
		
		if(args.Length < 2) {
			PrintUsage();
			return;
		}

		nickname = args[0];
		ip = args[1];
		
		if(args.Length < 3) {
			port = 26950;
			Logger.Error("Port not specified, using 26950 by default");
		} else {
			if(!short.TryParse(args[2], out port)) {
				PrintUsage();
				return;
			}
		}

		Logger.Info($"Trying to connect to {ip}:{port} as {nickname}...");
	}

	public static void CMD_CrosshairGap(string[] args) {
		void PrintUsage() => Logger.Error("ch_gap [value*]");

		if(args.Length < 1) {
			PrintUsage();
			return;
		}

		float gap;
		if(args[0] == "default" || args[0] == "def") {
			gap = Crosshair.DEFAULT_GAP;
		} else if(!float.TryParse(args[0], out gap)) {
			PrintUsage();
			return;
		}

		Global.Instance.CurrentMap?.Player.Hud.m_crosshair.SetGap(gap);
		Config.SetValue("crosshair", "gap", gap);
	}

	public static void CMD_CrosshairColor(string[] args) {
		void PrintUsage() => Logger.Error("ch_color [r*] [g*] [b*]");

		Color color;
		if(args.Length == 1 && (args[0] == "def" || args[0] == "default")) {
			color = Crosshair.DEFAULT_COLOR;
		} else {
			if(args.Length < 3) {
				PrintUsage();
				return;
			}

			byte r, g, b;
			if(!byte.TryParse(args[0], out r)) {
				PrintUsage();
				return;
			}
			if(!byte.TryParse(args[1], out g)) {
				PrintUsage();
				return;
			}
			if(!byte.TryParse(args[2], out b)) {
				PrintUsage();
				return;
			}

			color = Color.Color8(r,g,b);
		}

		Global.Instance.CurrentMap?.Player?.Hud.m_crosshair.SetColor(color);
		Config.SetValue("crosshair", "color", color);
	}
	
	public static void CMD_Say(string[] args) {
	}
}

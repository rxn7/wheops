using Godot;
using System.Collections.Generic;

public static class CommandManager {
	public static Dictionary<string, Command> Commands = new Dictionary<string, Command>();

	public static void Init() {
		AddCommand(new Command("exit", CMD_Exit, "Closes the game", null));
		AddCommand(new Command("map", CMD_Map, "Loads the specified map", new Command.Argument("map", false)));
		AddCommand(new Command("clear", CMD_Clear, "Clears the console output", null));
		AddCommand(new Command("list", CMD_List, "Lists all the available commnds", null));
		AddCommand(new Command("debug", CMD_Debug, "Toggles the debug visibility", null) );
		AddCommand(new Command("ch_gap", CMD_CrosshairGap, "Sets the crosshair's gap", new Command.Argument("value", false, Crosshair.DEFAULT_GAP)));
		AddCommand(new Command("ch_color", CMD_CrosshairColor, "Sets the crosshair's color", new Command.Argument("red", false, 255), new Command.Argument("green", false, 255), new Command.Argument("blue", false, 255)));
		AddCommand(new Command("ch_size", CMD_CrosshairSize, "Sets the crosshair's length and width", new Command.Argument("length", false, Crosshair.DEFAULT_LENGTH), new Command.Argument("width", false, Crosshair.DEFAULT_WIDTH)));
		AddCommand(new Command("sens", CMD_MouseSensetivity, "Sets the mouse sensetivity", new Command.Argument("sens", false, LocalPlayer.DEFAULT_MOUSE_SENS)));
		AddCommand(new Command("host", CMD_Host, "Hosts a server", new Command.Argument("port", true, (short)26950), new Command.Argument("max clients", true, (int)10)));
		AddCommand(new Command("connect", CMD_Connect, "Connects to a server", new Command.Argument("ip", false), new Command.Argument("port", false, 26950)));
		AddCommand(new Command("nick", CMD_Nick, "Sets your nickname", new Command.Argument("nickname", false, "NoName")));
		AddCommand(new Command("disconnect", CMD_Disconnect, "Disconnects you from a server if joined to any"));
		AddCommand(new Command("say", CMD_Say, "Send a chat message to the server", new Command.Argument("messge", false, "")));

		Logger.Info("CommandManager initialized");
	}

	private static void AddCommand(Command cmd) {
		Commands.Add(cmd.Name, cmd);
	}

	public static void Exeucte(string name, string[] args) {
		if(!Commands.ContainsKey(name)) {
			Logger.Error($"Invalid command: {name}");
			return;
		}

		Command cmd = Commands[name];
		if(!cmd.Handler(cmd, args)) {
			Logger.Error($"Failed to execute command {name}");
			Logger.Info($"Usage: {cmd.GetUsage()}");
		}
	}

	public static bool CMD_Clear(Command cmd, string[] args) {
		Console.Instance.ClearOutput();
		Console.Instance.ClearInput();

		return true;
	}

	public static bool CMD_List(Command cmd, string[] args) {
		string output="";
		foreach(Command c in Commands.Values) {
			output += $" > {c.GetUsage()} - {c.Description}\n";
		}

		Console.Instance.Print(output);

		return true;
	}

	public static bool CMD_Exit(Command cmd, string[] args) { 
		Logger.Info("Closing the game!");
		Global.Instance.GetTree().Quit();

		return true;
	}

	public static bool CMD_Debug(Command cmd, string[] args) {
		if(DebugLabel.Instance.Visible) {
			DebugLabel.Hide();
		} else {
			DebugLabel.Show();
		}

		return true;
	}

	public static bool CMD_CrosshairGap(Command cmd, string[] args) {
		if(args.Length != 1) {
			return false;
		}

		float gap;
		if(args[0] == "default" || args[0] == "def") {
			gap = Crosshair.DEFAULT_GAP;
		} else if(!float.TryParse(args[0], out gap)) {
			return false;
		}

		Global.Player.Hud.Crosshair.SetGap(gap);
		Config.SetValue("crosshair", "gap", gap);

		return true;
	}

	public static bool CMD_CrosshairColor(Command cmd, string[] args) {
		Color color;
		if(args.Length == 1 && (args[0] == "def" || args[0] == "default")) {
			color = Crosshair.DEFAULT_COLOR;
		} else if(args.Length == 3){
			byte r, g, b;
			if(!byte.TryParse(args[0], out r)) return false;
			if(!byte.TryParse(args[1], out g)) return false;
			if(!byte.TryParse(args[2], out b)) return false;

			color = Color.Color8(r,g,b);
		} else {
			return false;
		}

		Global.Player.Hud.Crosshair.SetColor(color);
		Config.SetValue("crosshair", "color", color);

		return true;
	}

	public static bool CMD_CrosshairSize(Command cmd, string[] args) {
		float length, width;
		if(args.Length == 1 && (args[0] == "def" || args[0] == "default")) {
			width = Crosshair.DEFAULT_WIDTH;
			length = Crosshair.DEFAULT_LENGTH;
		} else if(args.Length == 2) {
			if(!float.TryParse(args[0], out width)) return false;
			if(!float.TryParse(args[1], out length)) return false;
		} else {
			return false;
		}

		Config.SetValue("crosshair", "width", width);
		Config.SetValue("crosshair", "length", length);

		Global.Player.Hud.Crosshair.SetLength(length, false);
		Global.Player.Hud.Crosshair.SetWidth(width, true);

		return true;
	}

	public static bool CMD_MouseSensetivity(Command cmd, string[] args) {
		if(args.Length != 1) {
			return false;
		}

		float sens;
		if(args[0] == "def" || args[0] == "default") {
			sens = LocalPlayer.DEFAULT_MOUSE_SENS;
		} else if(!float.TryParse(args[0], out sens)) {
			return false;
		}

		Config.SetValue("mouse", "sens", sens);
		Global.Player.MouseSensetivity = sens;

		return true;
	}

	public static bool CMD_Map(Command cmd, string[] args) {
		if(args.Length != 1) {
			return false;
		}

		if(!NetworkManager.IsHost) {
			Logger.Error("Only host can change map!");
			return false;
		}

		string map = args[0];
		Global.LoadMap(map);

		return true;
	}

	public static bool CMD_Host(Command cmd, string[] args) {
		if(args.Length > 2) {
			return false;
		}

		short port;
		if(args.Length < 1) port = (short)cmd.Arguments[0].DefaultValue;
		else if(!short.TryParse(args[0], out port)) return false;

		int max_clients;
		if(args.Length < 2) max_clients = (int)cmd.Arguments[1].DefaultValue;
		else if(!int.TryParse(args[1], out max_clients)) return false;

		return NetworkManager.StartServer(port, max_clients);
	}

	public static bool CMD_Connect(Command cmd, string[] args) {
		if(args.Length < 2) {
			return false;
		}

		string ip = args[0];
		short port;
		
		if(args.Length < 2) {
			port = 26950;
			Logger.Info("Port not specified, using 26950 by default");
		} else {
			if(!short.TryParse(args[1], out port)) {
				return false;
			}
		}

		return NetworkManager.StartClient(ip, port);
	}

	public static bool CMD_Disconnect(Command cmd, string[] args) {
		NetworkManager.Disconnect();
		return true;
	}

	public static bool CMD_Nick(Command cmd, string[] args) {
		if(args.Length < 1) {
			return false;
		}

		string nickname = args[0];
		Config.SetValue("profile", "nickname", nickname);
		Global.Nickname = nickname;

		return true;
	}

	public static bool CMD_Say(Command cmd, string[] args) {
		if(args.Length < 1) {
			return false;
		}

		string message = string.Join(" ", args);
		if(NetworkManager.Network is Server server) {
			server.Sender.ChatMessage(-1, message);
		} else if(NetworkManager.Network is Client client) {
			client.Sender.ChatMessage(message);
		} else {
			Logger.Error("You need to be on a server to send a message");
			return false;
		}

		return true;
	}
}

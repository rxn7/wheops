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
		};

		Console.Instance.PrintSuccess("CommandManager initialized");
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
			Console.Instance.PrintError("debug [on,off]");
		}
	}
}

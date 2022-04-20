using Godot;

public static class Logger {
	public static void Error(object msg) {
		GD.PrintErr(msg);
		Console.Instance.PrintError(msg);
	}

	public static void Info(object msg) {
		GD.Print(msg);
		Console.Instance.PrintInfo(msg);
	}

	public static void Debug(object msg) {
		GD.Print("[DEBUG] ", msg);
		Console.Instance.PrintDebug(msg);
	}
}

using Godot;
using System;

public static class Config {
	private static ConfigFile File;
	public const string CONFIG_PATH = "user://config.ini";

	public static void Init() {
		LoadConfig();

		Logger.Info("Config initialized");
	}

	public static void LoadConfig() {
		File = new ConfigFile();
		Error status = File.Load(CONFIG_PATH);
		if(status != Error.Ok) {
			Logger.Error($"Error loading config file: {Enum.GetName(typeof(Error), status)}");
		}
	}

	public static void SetValue(string section, string key, object val) {
		File.SetValue(section, key, val);
		WriteToFile();
	}

	public static T GetValue<T>(string section, string key, T default_val) {
		object val = File.GetValue(section, key, default_val);

		if(!(val is T)) {
			val = default_val;
		}

		return (T)val;
	}

	public static void WriteToFile() {
		Error status = File.Save(CONFIG_PATH);
		if(status != Error.Ok) {
			Logger.Error($"Error saving config file: {Enum.GetName(typeof(Error), status)}");
		}
	}
}

using Godot;
using System;

public static class Config {
	private static ConfigFile s_File;
	private const string CONFIG_PATH = "user://config.ini";

	public static void Init() {
		LoadConfig();
		Logger.Info("Config initialized");
	}

	private static void LoadConfig() {
		s_File = new ConfigFile();
		Error status = s_File.Load(CONFIG_PATH);
		if (status != Error.Ok) {
			Logger.Error($"Error loading config file: {Enum.GetName(typeof(Error), status)}");
		}
	}

	public static void DeleteConfig() {
	}

	public static void SetValue(string section, string key, object val) {
		s_File.SetValue(section, key, val);
		WriteToFile();
	}

	public static T GetValue<T>(string section, string key, T default_val) {
		object val = s_File.GetValue(section, key, default_val);

		if (!(val is T)) {
			val = default_val;
		}

		return (T)val;
	}

	private static void WriteToFile() {
		Error status = s_File.Save(CONFIG_PATH);
		if (status != Error.Ok) {
			Logger.Error($"Error saving config file: {Enum.GetName(typeof(Error), status)}");
		}
	}
}

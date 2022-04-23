public delegate bool CommandHandler(Command cmd, string[] args);

public class Command {
	public enum EArgumentType {
		Number,
		Text,
	}

	public CommandHandler Handler { get; private set; }
	public Argument[] Arguments { get; private set; }
	public string Name { get; private set; }
	public string Description { get; private set; }

	public Command(string name, CommandHandler handler, string description, params Argument[] args) {
		Name = name;
		Handler = handler;
		Description = description;
		Arguments = args;
	}

	public string GetUsage() {
		string usage = $"{Name} ";

		if(Arguments != null) {
			foreach(Argument arg in Arguments) {
				usage += $"[{arg.Name}{(arg.Optional ? "" : "*")}] ";
			}
		}

		return usage;
	}

	public class Argument {
		public string Name { get; protected set; }
		public bool Optional { get; protected set; }
		public object DefaultValue { get; private set; }

		public Argument(string name, bool optional = false, object default_value = default) {
			Name = name;
			Optional = optional;
			DefaultValue = default_value;
		}
	}
}

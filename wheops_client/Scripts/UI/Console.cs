using System;
using System.Linq;
using Godot;

public class Console : Control {
	public static Console Instance {get; private set; }

	private LineEdit m_input;
	private RichTextLabel m_output;

	public bool IsActive() => Visible;

	public override void _Ready() {
		Instance = this;

		m_output = GetNode<RichTextLabel>("Output");
		m_input = GetNode<LineEdit>("InputHolder/Input");

		HideConsole();
	}

	public override void _Input(InputEvent ev) {
		if(Input.IsActionJustPressed("toggle_console")) {
			ToggleConsole();
		}

		if(!Visible) return;
	}

	public void ToggleConsole() {
		if(Visible)     HideConsole();
		else            OpenConsole();
	}

	public void OpenConsole() {
		m_input.GrabFocus();
		Visible = true;
		m_input.Clear();
	}

	public void HideConsole() {
		m_input.Clear();
		m_input.ReleaseFocus();
		Visible = false;
	}

	public void HandleCommand(string text) {
		string[] args = text.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
		if(args.Length < 1) return;

		Print("] " + text);

		string command = args[0].ToLower();
		args = args.Skip(1).ToArray();

		if(CommandManager.CommandHandlers.ContainsKey(command)) {
			CommandManager.CommandHandlers[command](args);
		} else {
			PrintError($"command '{command}' not found.");
		}

		m_output.AppendBbcode("\n");
	}

	// TODO: Delete first line while line count is bigger than X
	public void Print(object text) {
		m_output.AppendBbcode($"[color=white]{text}[/color]\n");
	}

	public void PrintError(object text) {
		m_output.AppendBbcode($"[color=red]ERROR: {text}[/color]\n");
	}

	public void PrintWarning(object text) {
		m_output.AppendBbcode($"[color=yellow]WARN: {text}[/color]\n");
	}

	public void PrintInfo(object text) {
		m_output.AppendBbcode($"[color=cyan]INFO: {text}[/color]\n");
	}

	public void PrintDebug(object text) {
		m_output.AppendBbcode($"[color=blue]DEBUG: {text}[/color]\n");
	}

	public void ClearOutput() {
		m_output.ScrollToLine(0);
		m_output.Clear();
	}

	public void ClearInput() {
		m_input.Clear();
	}

	private void OnInputEntered(string text) {
		m_input.Clear();
		HandleCommand(text);
	}

	private void OnInputChanged(string text) {
		if(text == "`") {
			m_input.Clear();
		}
	}
}

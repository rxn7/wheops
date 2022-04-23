using System;
using System.Linq;
using Godot;

public class Console : Control {
	public static Console Instance {get; private set; }

	private LineEdit m_Input;
	private RichTextLabel m_Output;

	public override void _Ready() {
		Instance = this;

		m_Output = GetNode<RichTextLabel>("Output");
		m_Input = GetNode<LineEdit>("InputHolder/Input");

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
		m_Input.GrabFocus();
		Visible = true;
		m_Input.Clear();
	}

	public void HideConsole() {
		m_Input.Clear();
		m_Input.ReleaseFocus();
		Visible = false;
	}

	public void HandleCommand(string text) {
		string[] args = text.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
		if(args.Length < 1) return;

		Print(">> " + text);

		string command = args[0].ToLower();
		args = args.Skip(1).ToArray();

		CommandManager.Exeucte(command, args);

		//m_output.AppendBbcode("\n");
	}

	// TODO: Delete first line while line count is bigger than X
	public void Print(object text) {
		m_Output.AppendBbcode($"[color=white]{text}[/color]\n");
	}

	public void PrintError(object text) {
		m_Output.AppendBbcode($"[color=red]ERROR: {text}[/color]\n");
	}

	public void PrintWarning(object text) {
		m_Output.AppendBbcode($"[color=yellow]WARN: {text}[/color]\n");
	}

	public void PrintInfo(object text) {
		m_Output.AppendBbcode($"[color=cyan]INFO: {text}[/color]\n");
	}

	public void PrintDebug(object text) {
		m_Output.AppendBbcode($"[color=blue]DEBUG: {text}[/color]\n");
	}

	public void ClearOutput() {
		m_Output.ScrollToLine(0);
		m_Output.Clear();
	}

	public void ClearInput() {
		m_Input.Clear();
	}

	private void OnInputEntered(string text) {
		m_Input.Clear();
		HandleCommand(text);
	}

	private void OnInputChanged(string text) {
		if(text == "`") {
			m_Input.Clear();
		}
	}
}

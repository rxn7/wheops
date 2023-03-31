using System;
using System.Linq;
using Godot;

public class Console : Control {
	public static Console Instance { get; private set; }
	public const int MAX_OUTPUT_LINES = 3;

	private LineEdit m_Input;
	private RichTextLabel m_Output;

	public static bool Active => Instance != null && Instance.Visible;

	public override void _Ready() {
		Instance = this;

		m_Output = GetNode<RichTextLabel>("Output");
		m_Input = GetNode<LineEdit>("InputHolder/Input");

		HideConsole();
	}

	public override void _Input(InputEvent ev) {
		if (Input.IsActionJustPressed("toggle_console")) {
			ToggleConsole();
		}

		if (!Visible) return;
	}

	public void ToggleConsole() {
		if (Visible) HideConsole();
		else OpenConsole();
	}

	public void OpenConsole() {
		m_Input.GrabFocus();
		Visible = true;
		m_Input.Clear();
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public void HideConsole() {
		m_Input.Clear();
		m_Input.ReleaseFocus();
		Visible = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public void HandleCommand(string text) {
		string[] args = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (args.Length < 1) return;

		Print(">> " + text);

		string command = args[0].ToLower();
		args = args.Skip(1).ToArray();

		CommandManager.Exeucte(command, args);
	}

	private void AppendOutput(string text) {
		/*
		while(m_Output.GetLineCount() > MAX_OUTPUT_LINES)
			m_Output.RemoveLine(0);
		*/

		m_Output.AppendBbcode(text);
	}

	public void Print(object text) {
		AppendOutput($"[color=white]{text}[/color]\n");
	}

	public void PrintError(object text) {
		AppendOutput($"[color=red]ERROR: {text}[/color]\n");
	}

	public void PrintWarning(object text) {
		AppendOutput($"[color=yellow]WARN: {text}[/color]\n");
	}

	public void PrintInfo(object text) {
		AppendOutput($"[color=cyan]INFO: {text}[/color]\n");
	}

	public void PrintDebug(object text) {
		AppendOutput($"[color=blue]DEBUG: {text}[/color]\n");
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
		if (text == "`") {
			m_Input.Clear();
		}
	}
}

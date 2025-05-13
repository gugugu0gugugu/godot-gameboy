using System;
using Godot;

public partial class Dialog : FileDialog {
    [Signal]
    public delegate void NewEmulatorEventHandler(string rompath);
    public void OnFileSelected(string path) {
        if (!System.IO.File.Exists(path)) {
            GD.Print("File does not exist!");
            return;
        }
        GD.Print(path);
        EmitSignal(SignalName.NewEmulator, path);
    }

    public void Pop() {
        Popup();
    }
}

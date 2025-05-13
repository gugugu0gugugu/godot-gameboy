using Godot;

public partial class File : MenuButton
{
    [Signal]
    public delegate void OnSelectCartridgeFileEventHandler(bool playing);
    PopupMenu _popupMenu;
    

    public override void _Ready()
    {
        base._Ready();
        _popupMenu = GetPopup();
        _popupMenu.IndexPressed += SubAction;
    }

    public void SubAction(long idx)
    {
        if (idx == 0) 
        {
            EmitSignal(SignalName.OnSelectCartridgeFile, true);
            return;
        }
        if (idx == 1)
        {
            EmitSignal(SignalName.OnSelectCartridgeFile, false);
            return;
        }
        
    }
    
}

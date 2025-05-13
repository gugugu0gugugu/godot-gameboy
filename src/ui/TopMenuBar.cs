using Godot;

public partial class TopMenuBar: Control {


    public string TopMenuBarName = "MenuBar";
    private Control _topMenuBar;
    
    public string FileSelectBtnName = "File";
    private Button _fileSelectBtn;

    public string DebugBtnName = "Debug";
    private Button _debugBtn;
    public string FileSelectDialogName = "FileDialog";
    private FileDialog _fileSelectDialog;
    public Button DebugBtn {get => _debugBtn;}
    public override void _Ready()
    {
        base._Ready();
        _topMenuBar = GetNode<Control>(TopMenuBarName);
        _fileSelectBtn = _topMenuBar.GetNode<Button>(FileSelectBtnName);
        _fileSelectDialog = _topMenuBar.GetNode<FileDialog>(FileSelectDialogName);
        _debugBtn = _topMenuBar.GetNode<Button>(DebugBtnName);
    }

}

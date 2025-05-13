using System.Linq.Expressions;
using Godot;
public partial class EmulatorInput : Node
{
    public const Key LEFT = Key.A;
    public const Key RIGHT = Key.D;
    public const Key UP = Key.W;
    public const Key DOWN = Key.S;

    public const Key A = Key.J;
    public const Key B = Key.K;
    public const Key SELECT = Key.Space;
    public const Key START = Key.Enter;
    private byte _state;
    public bool APressed 
    {
        get {return ((_state >> 7) & 0x1) == 0;}
        set 
        {
            if (!value)
                _state |= 0x80;
            else _state &= 0x7f;
        }
    }
    public bool BPressed 
    {
        get {return ((_state >> 6) & 0x1) == 0;}
        set 
        {
            if (!value)
                _state |= 0x40;
            else _state &= 0xBF;
        }
    }
    public bool SelectPressed 
    {
        get {return ((_state >> 5) & 0x1) == 0;}
        set 
        {
            if (!value)
                _state |= 0x20;
            else _state &= 0xDf;
        }
    }
    public bool StartPressed 
    {
        get {return ((_state >> 4) & 0x1) == 0;}
        set 
        {
            if (!value)
                _state |= 0x10;
            else _state &= 0xEf;
        }
    }
    public bool UpPressed 
    {
        get {return ((_state >> 3) & 0x1) == 0;}
        set 
        {
            if (!value)
                _state |= 0x08;
            else _state &= 0xF7;
        }
    }
    public bool DownPressed 
    {
        get {return ((_state >> 2) & 0x1) == 0;}
        set 
        {
            if (!value)
                _state |= 0x04;
            else _state &= 0xFB;
        }
    }
    public bool LeftPressed 
    {
        get {return ((_state >> 1) & 0x1) == 0;}
        set 
        {
            if (!value)
                _state |= 0x02;
            else _state &= 0xFD;
        }
    }
    public bool RightPressed 
    {
        get {return ((_state) & 0x1) == 0;}
        set 
        {
            if (!value)
                _state |= 0x01;
            else _state &= 0xFE;
        }
    }
    public EmulatorInput()
    {
        _state = 0xFF;
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed) 
            {
                switch (keyEvent.Keycode)
                {
                    case LEFT: LeftPressed = true; break;
                    case RIGHT: RightPressed = true; break;
                    case UP: UpPressed = true; break;
                    case DOWN: DownPressed = true; break;
                    case A: APressed = true; break;
                    case B: BPressed = true; break;
                    case SELECT: SelectPressed= true; break;
                    case START: StartPressed= true; break;
                }
            }
            if (!keyEvent.Pressed)
            {
                switch (keyEvent.Keycode)
                {
                    case LEFT: LeftPressed = false; break;
                    case RIGHT: RightPressed = false; break;
                    case UP: UpPressed = false; break;
                    case DOWN: DownPressed = false; break;
                    case A: APressed = false; break;
                    case B: BPressed = false; break;
                    case SELECT: SelectPressed= false; break;
                    case START: StartPressed = false; break;
                }
            }
        }
    }
}

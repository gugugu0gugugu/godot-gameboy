using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using Godot;
using Godot.Collections;

public partial class DebugWindow : Control
{
    bool _visibility;
    bool _logging;
    List<byte> _serial_data;

    HBoxContainer _register_af;
    HBoxContainer _register_bc;
    HBoxContainer _register_de;
    HBoxContainer _register_hl;
    Label _register_pc;
    Label _register_sp;
    Array<Label> _flags;
    VSplitContainer _cpu_stepping;
    Button _cpu_step_btn;
    Label _cpu_step_label;
    //VScrollBar _cpu_log_container;
    RichTextLabel _cpu_log_label;
    VScrollBar _seria_log_container;
    RichTextLabel _seria_log_label;
    TextureRect _tiles_container;
    Texture2D _tiles;
    UInt32 _step;

    public DebugWindow () 
    {
        _flags = new Array<Label>();
        _serial_data = new List<byte>();
    }
    public override void _Ready()
    {
        base._Ready();
        _register_af = GetNode<HBoxContainer>("VBoxContainer/CombineRegister/DetailAF");
        _register_bc = GetNode<HBoxContainer>("VBoxContainer/CombineRegister/DetailBC");
        _register_de = GetNode<HBoxContainer>("VBoxContainer/CombineRegister/DetailDE");
        _register_hl = GetNode<HBoxContainer>("VBoxContainer/CombineRegister/DetailHL");
        _register_pc = GetNode<Label>("VBoxContainer/CombineRegister/Special/PC_Value");
        _register_sp = GetNode<Label>("VBoxContainer/CombineRegister/Special/SP_Value");
        _flags.Add(GetNode<HBoxContainer>("VBoxContainer/CombineRegister/FlagsValue").GetChild<Label>(0));
        _flags.Add(GetNode<HBoxContainer>("VBoxContainer/CombineRegister/FlagsValue").GetChild<Label>(1));
        _flags.Add(GetNode<HBoxContainer>("VBoxContainer/CombineRegister/FlagsValue").GetChild<Label>(2));
        _flags.Add(GetNode<HBoxContainer>("VBoxContainer/CombineRegister/FlagsValue").GetChild<Label>(3));

        _cpu_stepping = GetNode<VSplitContainer>("VBoxContainer/CpuLogCtrl/CpuStepInfo");
        _cpu_step_btn = _cpu_stepping.GetNode<Button>("Step/CpuStep");
        _cpu_step_label = _cpu_stepping.GetNode<Label>("Info");
        //_step = (UInt32)_cpu_stepping.GetNode<TextEdit>("TextEdit").Text.ToInt();

        //_cpu_log_container = GetNode<VScrollBar>("CpuLog");
        _cpu_log_label= GetNode<RichTextLabel>("VBoxContainer/CpuLog");
        
        _seria_log_label = GetNode<RichTextLabel>("VBoxContainer/SerialLog");
        _tiles_container = GetNode<TextureRect>("VBoxContainer/Tiles");
    }


    public void Update(ref Emulator e)
    {
        _register_af.GetChild<Label>(1).Text = e.Cpu.AF.ToString("X4");
        _register_af.GetChild<Label>(3).Text = e.Cpu.A.ToString("X2");
        _register_af.GetChild<Label>(5).Text = e.Cpu.F.ToString("X2");

        _register_bc.GetChild<Label>(1).Text = e.Cpu.BC.ToString("X4");
        _register_bc.GetChild<Label>(3).Text = e.Cpu.B.ToString("X2");
        _register_bc.GetChild<Label>(5).Text = e.Cpu.C.ToString("X2");

        _register_de.GetChild<Label>(1).Text = e.Cpu.DE.ToString("X4");
        _register_de.GetChild<Label>(3).Text = e.Cpu.D.ToString("X2");
        _register_de.GetChild<Label>(5).Text = e.Cpu.E.ToString("X2");

        _register_hl.GetChild<Label>(1).Text = e.Cpu.HL.ToString("X4");
        _register_hl.GetChild<Label>(3).Text = e.Cpu.H.ToString("X2");
        _register_hl.GetChild<Label>(5).Text = e.Cpu.L.ToString("X2");

        _register_pc.Text = e.Cpu.PC.ToString("X4");
        _register_sp.Text = e.Cpu.SP.ToString("X4");

        _flags[0].Text = e.Cpu.FZ.ToString();
        _flags[1].Text = e.Cpu.FN.ToString();
        _flags[2].Text = e.Cpu.FH.ToString();
        _flags[3].Text = e.Cpu.FC.ToString();
        if (!e.Suspend)
        {
            _cpu_step_label.Text = "Can't step cpu when emulator isn't suspended";
        }
        else
        {
            _cpu_step_label.Text = Instructions.GetOptName(e.BusRead(e.Cpu.PC));
        }
        CpuLog(ref e);
    }
    public void CpuLog(ref Emulator e)
    {
        if (!_logging) return;
        StringBuilder buffer = new StringBuilder();
        buffer.AppendFormat("{0:X2} {1:X2} {2:X2} A:    {3:X2}  ", e.BusRead(e.Cpu.PC), e.BusRead((UInt16)(e.Cpu.PC + 1)), e.BusRead((UInt16)(e.Cpu.PC + 2)), e.Cpu.A);
        buffer.Append("F: " + (e.Cpu.FZ ? "Z" : "-"));
        buffer.Append(e.Cpu.FN ? "N" : "-");
        buffer.Append(e.Cpu.FH ? "H" : "-");
        buffer.Append(e.Cpu.FC ? "C" : "-");
        buffer.AppendFormat("   F: {0:X2}   BC: {1:X4}  DE: {2:X4}  HL: {3:X4}  SP: {4:X4}\n",
            e.Cpu.F,
            e.Cpu.BC,
            e.Cpu.DE,
            e.Cpu.HL,
            e.Cpu.SP
        );
        _cpu_log_label.Text += buffer.ToString();
    }
    public void SerialLog(ref Emulator e)
    {
            while (e.Serial.OutputBuffer.Count > 0) {
                byte data = e.Serial.OutputBuffer[0];
                e.Serial.OutputBuffer.RemoveAt(0);
                _serial_data.Add(data);
            }
            _seria_log_label.Text = _serial_data.ToArray().GetStringFromAscii();
    }
    public void TilesLog(ref Emulator e)
    {
        //if (_tiles == null) {
            Image image;
            uint width = 16 << 3;
            uint height = 24 << 3;
            uint image_data_size = width * height << 2;
            uint row_pitch_size = width << 2;
            byte[] image_data = new byte[image_data_size];
            for (uint y = 0; y < (height >> 3); y++) 
            {
                for (uint x = 0;  x < (width >> 3); x++)
                {
                    uint tile_idx = y * (width >> 3) + x;
                    uint cur_tile_data_begin = ((y * row_pitch_size) << 3) + (x << 5);
                    for (uint line = 0; line < 8; line++)
                    {
                        // every tile has 16 bytes, and now decode it to rgba
                        DecodeTileLine(ref e.VRam, (tile_idx << 4) + (line << 1), ref image_data, cur_tile_data_begin + line * row_pitch_size);
                    }
                }
            }

            image = Image.CreateFromData((int)width, (int)height, false, Image.Format.Rgba8, image_data);
                ImageTexture tiles = ImageTexture.CreateFromImage(image);
                _tiles = tiles;
                _tiles_container.Texture = _tiles;
        //}
        
    }
    private void DecodeTileLine(ref byte[] orgData, uint orgOffset, ref byte[] receive, uint receiveOffset)
    {
        for (int b = 7; b >= 0; b--)
        {
            byte l = (byte)((orgData[orgOffset] >> b) & 0x1);
            byte h = (byte)(((orgData[orgOffset + 1] >> b) & 0x1) << 1);
            byte color = (byte)(h | l);

            color = color switch
            {
                0 => 0xff,
                1 => 0xaa,
                2 => 0x55,
                3 => 0x00,
                _ => throw new InvalidDataException("Invalid color index " + color.ToString()),
            };
            receive[receiveOffset + ((7-b) << 2)] = color;
            receive[receiveOffset + ((7-b) << 2) + 1] = color;
            receive[receiveOffset + ((7-b) << 2) + 2] = color;
            receive[receiveOffset + ((7-b) << 2) + 3] = 0x80;
        }
    }
    public void StartLogCallback()
    {
        _logging = true;
    }
    public void StopLogCallback()
    {
        _logging = false;
    }
    public void ClearLogCallback()
    {
        _cpu_log_label.Text = "";
    }
}

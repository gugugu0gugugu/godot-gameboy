using System;
using System.IO;



public partial class Instructions {
    protected delegate void Nothing(Emulator e);
    public static void X00_Nop(Emulator e) {
        e.Tick(1);
    }
    public static void X01_LD_I162BC(Emulator e) {
        e.Cpu.BC = ReadU16(e);
        e.Tick(3);
    }

    public static void X02_LD_A2MBD(Emulator e) {
        e.BusWrite(e.Cpu.BC, e.Cpu.A);
        e.Tick(2);
    }
    public static void X03_INC_BC(Emulator e) {
        e.Cpu.BC++;
        e.Tick(2);
    }
    public static void X04_INC_B(Emulator e) {
        Inc8(e, ref e.Cpu.B);
        e.Tick(1);
    }
    public static void X05_DEC_B(Emulator e) {
        Dec8(e, ref e.Cpu.B);
        e.Tick(1);
    }

    public static void X06_LD_I82B(Emulator e) {
        e.Cpu.B = ReadU8(e);
        e.Tick(2);
    }
    public static void X07_RLCA(Emulator e) {
        RLC8(e, ref e.Cpu.A);
        e.Cpu.ResetFZ();
        e.Tick(1);
    }
    public static void X08_LD_SP2MI16(Emulator e) {
        UInt16 addr = ReadU16(e);
        e.Tick(2);
        e.BusWrite(addr, (byte)(e.Cpu.SP & 0x00FF));
        e.Tick(1);
        e.BusWrite((UInt16)(addr + 1u), (byte)(e.Cpu.SP >> 8));
        e.Tick(2);
    }
    public static void X09_ADD_HL_BC(Emulator e) {
        e.Cpu.HL = ADD16(e, e.Cpu.HL, e.Cpu.BC);
        e.Tick(2);
    }
    public static void X0A_LD_MBC2A(Emulator e) {
        e.Cpu.A = e.BusRead(e.Cpu.BC);
        e.Tick(2);
    }
    public static void X0B_DEC_BC(Emulator e) {
        e.Cpu.BC--;
        e.Tick(2);
    }
    public static void X0C_INC_C(Emulator e) {
        Inc8(e, ref e.Cpu.C);
        e.Tick(1);
    }
    public static void X0D_DEC_C(Emulator e) {
        Dec8(e, ref e.Cpu.C);
        e.Tick(1);
    }
    public static void X0E_LD_I82C(Emulator e) {
        e.Cpu.C = ReadU8(e);
        e.Tick(2);
    }
    public static void X0F_RRCA(Emulator e) {
        RRC8(e, ref e.Cpu.A);
        e.Cpu.ResetFZ();
        e.Tick(1);
    }
    public static void X10_STOP(Emulator e) {
        ReadU8(e);
        e.Pause();
        e.Tick(1);
    }

    public static void X11_LD_I162DE(Emulator e) {
        e.Cpu.DE = ReadU16(e);
        e.Tick(3);
    }
    public static void X12_LD_A2MDE(Emulator e) {
        e.BusWrite(e.Cpu.DE, e.Cpu.A);
        e.Tick(2);
    }
    public static void X13_INC_DE(Emulator e) {
        e.Cpu.DE++;
        e.Tick(2);
    }
    public static void X14_INC_D(Emulator e) {
        Inc8(e, ref e.Cpu.D);
        e.Tick(1);
    }
    public static void X15_DEC_D(Emulator e) {
        Dec8(e, ref e.Cpu.D);
        e.Tick(1);
    }
    public static void X16_LD_I82D(Emulator e) {
        e.Cpu.D = ReadU8(e);
        e.Tick(2);
    }
    public static void X17_RLA(Emulator e) {
        RL8(e, ref e.Cpu.A);
        e.Cpu.ResetFZ();
        e.Tick(1);
    }
    public static void X18_JR_I8(Emulator e) {
        sbyte offset = (sbyte)ReadU8(e);
        e.Cpu.PC += (UInt16)(Int16)offset;
        e.Tick(3);
    }
    public static void X19_ADD_HL_DE(Emulator e) {
        e.Cpu.HL = ADD16(e, e.Cpu.HL, e.Cpu.DE);
        e.Tick(2);
    }

    public static void X1A_LD_MDE2A(Emulator e) {
        e.Cpu.A = e.BusRead(e.Cpu.DE);
        e.Tick(2);
    }
    public static void X1B_DEC_DE(Emulator e) {
        e.Cpu.DE--;
        e.Tick(2);
    }
    public static void X1C_INC_E(Emulator e) {
        Inc8(e, ref e.Cpu.E);
        e.Tick(1);
    }
    public static void X1D_DEC_E(Emulator e) {
        Dec8(e, ref e.Cpu.E);
        e.Tick(1);
    }

    public static void X1E_LD_I82E(Emulator e) {
        e.Cpu.E = ReadU8(e);
        e.Tick(2);
    }
    public static void X1F_RRA(Emulator e) {
        RR8(e, ref e.Cpu.A);
        e.Cpu.ResetFZ();
        e.Tick(1);
    }
    public static void X20_JRNZ_I8(Emulator e) {
        sbyte offset = (sbyte)ReadU8(e);
        if (!e.Cpu.FZ) {
            e.Cpu.PC += (UInt16)(Int16)offset;
            e.Tick(3);
        } else {
            e.Tick(2);
        }
    }
    public static void X21_LD_I162HL(Emulator e) {
        e.Cpu.HL = ReadU16(e);
        e.Tick(3);
    }
    public static void X22_LDI_A2MHL(Emulator e) {
        e.BusWrite(e.Cpu.HL, e.Cpu.A);
        e.Cpu.HL++;
        e.Tick(2);
    }
    public static void X23_INC_HL(Emulator e) {
        e.Cpu.HL++;
        e.Tick(2);
    }
    public static void X24_INC_H(Emulator e) {
        Inc8(e, ref e.Cpu.H);
        e.Tick(1);
    }
    public static void X25_DEC_H(Emulator e) {
        Dec8(e, ref e.Cpu.H);
        e.Tick(1);
    }
    public static void X26_LD_I82H(Emulator e) {
        e.Cpu.H = ReadU8(e);
        e.Tick(2);
    }
    public static void X27_DAA(Emulator e) {
        if (e.Cpu.FN) {
            if (e.Cpu.FC)
            {
                if (e.Cpu.FH) e.Cpu.A += 0x9a;
                else e.Cpu.A += 0xa0;
            }
            else
            {
                if (e.Cpu.FH) e.Cpu.A += 0xfa;
            }
        }
        else
        {
            if (e.Cpu.FC || (e.Cpu.A > 0x99))
            {
                if (e.Cpu.FH || ((e.Cpu.A & 0x0f) > 0x09)) e.Cpu.A += 0x66;
                else e.Cpu.A += 0x60;
                e.Cpu.SetFC();
            }
            else
            {
                if (e.Cpu.FH || ((e.Cpu.A & 0x0f) > 0x09)) e.Cpu.A += 0x06;
            }
        }
        ZeroFlag(e, e.Cpu.A);
        e.Cpu.ResetFH();
        e.Tick(1);
    }

    public static void X28_JRZ_I8(Emulator e) {
        sbyte offset = (sbyte)ReadU8(e);
        if (e.Cpu.FZ) {
            e.Cpu.PC += (UInt16)(Int16)offset;
            e.Tick(3);
        } else {
            e.Tick(2);
        }
    }
    public static void X29_ADD_HL_HL(Emulator e) {
        e.Cpu.HL = ADD16(e, e.Cpu.HL, e.Cpu.HL);
        e.Tick(2);
    }

    public static void X2A_LDI_MHL2A(Emulator e) {
        e.Cpu.A = e.BusRead(e.Cpu.HL);
        e.Cpu.HL++;
        e.Tick(2);
    }
    public static void X2B_DEC_HL(Emulator e) {
        e.Cpu.HL--;
        e.Tick(2);
    }
    public static void X2C_INC_L(Emulator e) {
        Inc8(e, ref e.Cpu.L);
        e.Tick(1);
    }
    public static void X2D_DEC_L(Emulator e) {
        Dec8(e, ref e.Cpu.L);
        e.Tick(1);
    }
    public static void X2E_LD_I82L(Emulator e) {
        e.Cpu.L = ReadU8(e);
        e.Tick(2);
    }
    public static void X2F_CPL(Emulator e) {
        e.Cpu.A = (byte)(e.Cpu.A ^ 0xff);
        e.Cpu.SetFN();
        e.Cpu.SetFH();
        e.Tick(1);
    }

    public static void X30_JRNC_I8(Emulator e) {
        sbyte offset = (sbyte)ReadU8(e);
        if (!e.Cpu.FC) {
            e.Cpu.PC += (UInt16)(Int16)offset;
            e.Tick(3);
        } else {
            e.Tick(2);
        }
    }
    public static void X31_LD_I162SP(Emulator e) {
        e.Cpu.SP = ReadU16(e);
        e.Tick(3);
    }
    public static void X32_LDD_A2MHL(Emulator e) {
        e.BusWrite(e.Cpu.HL, e.Cpu.A);
        e.Cpu.HL--;
        e.Tick(2);
    }
    public static void X33_INC_SP(Emulator e) {
        e.Cpu.SP++;
        e.Tick(2);
    }
    public static void X34_INC_MHL(Emulator e) {
        byte data = e.BusRead(e.Cpu.HL);
        e.Tick(1);
        Inc8(e, ref data);
        e.BusWrite(e.Cpu.HL, data);
        e.Tick(2);
    }
    public static void X35_DEC_MHL(Emulator e) {
        byte data = e.BusRead(e.Cpu.HL);
        e.Tick(1);
        Dec8(e, ref data);
        e.BusWrite(e.Cpu.HL, data);
        e.Tick(2);
    }
    public static void X36_LD_I82MHL(Emulator e) {
        byte data = ReadU8(e);
        e.Tick(1);
        e.BusWrite(e.Cpu.HL, data);
        e.Tick(2);
    }
    public static void X37_SCF(Emulator e) {
        e.Cpu.ResetFN();
        e.Cpu.ResetFH();
        e.Cpu.SetFC();
        e.Tick(1);
    }

    public static void X38_JRC_I8(Emulator e) {
        sbyte offset = (sbyte)ReadU8(e);
        if (e.Cpu.FC) {
            e.Cpu.PC += (UInt16)(Int16)offset;
            e.Tick(3);
        } else {
            e.Tick(2);
        }
    }
    public static void X39_ADD_HL_SP(Emulator e) {
        e.Cpu.HL = ADD16(e, e.Cpu.HL, e.Cpu.SP);
        e.Tick(2);
    }

    public static void X3A_LDD_MHL2A(Emulator e) {
        e.Cpu.A = e.BusRead(e.Cpu.HL);
        e.Cpu.HL--;
        e.Tick(2);
    }
    public static void X3B_DEC_SP(Emulator e) {
        e.Cpu.SP--;
        e.Tick(2);
    }
    public static void X3C_INC_A(Emulator e) {
        Inc8(e, ref e.Cpu.A);
        e.Tick(1);
    }
    public static void X3D_DEC_A(Emulator e) {
        Dec8(e, ref e.Cpu.A);
        e.Tick(1);
    }
    public static void X3E_LD_I82A(Emulator e) {
        e.Cpu.A = ReadU8(e);
        e.Tick(2);
    }
    public static void X3F_CCF(Emulator e) {
        e.Cpu.ResetFN();
        e.Cpu.ResetFH();
        if (e.Cpu.FC)
        {
            e.Cpu.ResetFC();
        } else e.Cpu.SetFC();
        e.Tick(1);
    }


    public static void X40_LD_B2B(Emulator e) {
        e.Cpu.B = e.Cpu.B;
        e.Tick(1);
    }
    public static void X41_LD_C2B(Emulator e) {
        e.Cpu.B = e.Cpu.C;
        e.Tick(1);
    }
    public static void X42_LD_D2B(Emulator e) {
        e.Cpu.B = e.Cpu.D;
        e.Tick(1);
    }
    public static void X43_LD_E2B(Emulator e) {
        e.Cpu.B = e.Cpu.E;
        e.Tick(1);
    }    
    public static void X44_LD_H2B(Emulator e) {
        e.Cpu.B = e.Cpu.H;
        e.Tick(1);
    }     
    public static void X45_LD_L2B(Emulator e) {
        e.Cpu.B = e.Cpu.L;
        e.Tick(1);
    }    
    public static void X46_LD_MHL2B(Emulator e) {
        e.Cpu.B = e.BusRead(e.Cpu.HL);
        e.Tick(2);
    }
    public static void X47_LD_A2B(Emulator e) {
        e.Cpu.B = e.Cpu.A;
        e.Tick(1);
    }    
    public static void X48_LD_B2C(Emulator e) {
        e.Cpu.C = e.Cpu.B;
        e.Tick(1);
    }    
    public static void X49_LD_C2C(Emulator e) {
        e.Cpu.C = e.Cpu.C;
        e.Tick(1);
    }    
    public static void X4A_LD_D2C(Emulator e) {
        e.Cpu.C = e.Cpu.D;
        e.Tick(1);
    }    
    public static void X4B_LD_E2C(Emulator e) {
        e.Cpu.C = e.Cpu.E;
        e.Tick(1);
    }    
    public static void X4C_LD_H2C(Emulator e) {
        e.Cpu.C = e.Cpu.H;
        e.Tick(1);
    }    
    public static void X4D_LD_L2C(Emulator e) {
        e.Cpu.C = e.Cpu.L;
        e.Tick(1);
    }    
    public static void X4E_LD_MHL2C(Emulator e) {
        e.Cpu.C = e.BusRead(e.Cpu.HL);
        e.Tick(2);
    }
    public static void X4F_LD_A2C(Emulator e) {
        e.Cpu.C = e.Cpu.A;
        e.Tick(1);
    }    
    public static void X50_LD_B2D(Emulator e) {
        e.Cpu.D = e.Cpu.B;
        e.Tick(1);
    }
    public static void X51_LD_C2D(Emulator e) {
        e.Cpu.D = e.Cpu.C;
        e.Tick(1);
    }
    public static void X52_LD_D2D(Emulator e) {
        e.Cpu.D = e.Cpu.D;
        e.Tick(1);
    }
    public static void X53_LD_E2D(Emulator e) {
        e.Cpu.D = e.Cpu.E;
        e.Tick(1);
    }
    public static void X54_LD_H2D(Emulator e) {
        e.Cpu.D = e.Cpu.H;
        e.Tick(1);
    }
    public static void X55_LD_L2D(Emulator e) {
        e.Cpu.D = e.Cpu.L;
        e.Tick(1);
    }
    public static void X56_LD_MHL2D(Emulator e) {
        e.Cpu.D = e.BusRead(e.Cpu.HL);
        e.Tick(2);
    }
    public static void X57_LD_A2D(Emulator e) {
        e.Cpu.D = e.Cpu.A;
        e.Tick(1);
    }
    public static void X58_LD_B2E(Emulator e) {
        e.Cpu.E = e.Cpu.B;
        e.Tick(1);
    }
    public static void X59_LD_C2E(Emulator e) {
        e.Cpu.E = e.Cpu.C;
        e.Tick(1);
    }
    public static void X5A_LD_D2E(Emulator e) {
        e.Cpu.E = e.Cpu.D;
        e.Tick(1);
    }
    public static void X5B_LD_E2E(Emulator e) {
        e.Cpu.E = e.Cpu.E;
        e.Tick(1);
    }
    public static void X5C_LD_H2E(Emulator e) {
        e.Cpu.E = e.Cpu.H;
        e.Tick(1);
    }
    public static void X5D_LD_L2E(Emulator e) {
        e.Cpu.E = e.Cpu.L;
        e.Tick(1);
    }
    public static void X5E_LD_MHL2E(Emulator e) {
        e.Cpu.E = e.BusRead(e.Cpu.HL);
        e.Tick(2);
    }
    public static void X5F_LD_A2E(Emulator e) {
        e.Cpu.E = e.Cpu.A;
        e.Tick(1);
    }
    public static void X60_LD_B2H(Emulator e) {
        e.Cpu.H = e.Cpu.B;
        e.Tick(1);
    }
    public static void X61_LD_C2H(Emulator e) {
        e.Cpu.H = e.Cpu.C;
        e.Tick(1);
    }
    public static void X62_LD_D2H(Emulator e) {
        e.Cpu.H = e.Cpu.D;
        e.Tick(1);
    }
    public static void X63_LD_E2H(Emulator e) {
        e.Cpu.H = e.Cpu.E;
        e.Tick(1);
    }
    public static void X64_LD_H2H(Emulator e) {
        e.Cpu.H = e.Cpu.H;
        e.Tick(1);
    }
    public static void X65_LD_L2H(Emulator e) {
        e.Cpu.H = e.Cpu.L;
        e.Tick(1);
    }
    public static void X66_LD_MHL2H(Emulator e) {
        e.Cpu.H = e.BusRead(e.Cpu.HL);
        e.Tick(2);
    }
    public static void X67_LD_A2H(Emulator e) {
        e.Cpu.H = e.Cpu.A;
        e.Tick(1);
    }
    public static void X68_LD_B2L(Emulator e) {
        e.Cpu.L = e.Cpu.B;
        e.Tick(1);
    }
    public static void X69_LD_C2L(Emulator e) {
        e.Cpu.L = e.Cpu.C;
        e.Tick(1);
    }
    public static void X6A_LD_D2L(Emulator e) {
        e.Cpu.L = e.Cpu.D;
        e.Tick(1);
    }
    public static void X6B_LD_E2L(Emulator e) {
        e.Cpu.L = e.Cpu.E;
        e.Tick(1);
    }
    public static void X6C_LD_H2L(Emulator e) {
        e.Cpu.L = e.Cpu.H;
        e.Tick(1);
    }
    public static void X6D_LD_L2L(Emulator e) {
        e.Cpu.L = e.Cpu.L;
        e.Tick(1);
    }
    public static void X6E_LD_MHL2L(Emulator e) {
        e.Cpu.L = e.BusRead(e.Cpu.HL);
        e.Tick(2);
    }
    public static void X6F_LD_A2L(Emulator e) {
        e.Cpu.L = e.Cpu.A;
        e.Tick(1);
    }
    public static void X70_LD_B2MHL(Emulator e) {
        e.BusWrite(e.Cpu.HL, e.Cpu.B);
        e.Tick(2);
    }
    public static void X71_LD_C2MHL(Emulator e) {
        e.BusWrite(e.Cpu.HL, e.Cpu.C);
        e.Tick(2);
    }
    public static void X72_LD_D2MHL(Emulator e) {
        e.BusWrite(e.Cpu.HL, e.Cpu.D);
        e.Tick(2);
    }
    public static void X73_LD_E2MHL(Emulator e) {
        e.BusWrite(e.Cpu.HL, e.Cpu.E);
        e.Tick(2);
    }
    public static void X74_LD_H2MHL(Emulator e) {
        e.BusWrite(e.Cpu.HL, e.Cpu.H);
        e.Tick(2);
    }
    public static void X75_LD_L2MHL(Emulator e) {
        e.BusWrite(e.Cpu.HL, e.Cpu.L);
        e.Tick(2);
    }
    public static void X76_HALT(Emulator e) {
        e.Cpu.Halted = true;
        e.Tick(1);
    }
    public static void X77_LD_A2MHL(Emulator e) {
        e.BusWrite(e.Cpu.HL, e.Cpu.A);
        e.Tick(2);
    }
    public static void X78_LD_B2A(Emulator e) {
        e.Cpu.A = e.Cpu.B;
        e.Tick(1);
    }
    public static void X79_LD_C2A(Emulator e) {
        e.Cpu.A = e.Cpu.C;
        e.Tick(1);
    }
    public static void X7A_LD_D2A(Emulator e) {
        e.Cpu.A = e.Cpu.D;
        e.Tick(1);
    }
    public static void X7B_LD_E2A(Emulator e) {
        e.Cpu.A = e.Cpu.E;
        e.Tick(1);
    }
    public static void X7C_LD_H2A(Emulator e) {
        e.Cpu.A = e.Cpu.H;
        e.Tick(1);
    }
    public static void X7D_LD_L2A(Emulator e) {
        e.Cpu.A = e.Cpu.L;
        e.Tick(1);
    }
    public static void X7E_LD_MHL2A(Emulator e) {
        e.Cpu.A = e.BusRead(e.Cpu.HL);
        e.Tick(2);
    }
    public static void X7F_LD_A2A(Emulator e) {
        e.Cpu.A = e.Cpu.A;
        e.Tick(1);
    }
    public static void X80_ADD_A_B(Emulator e) {
        e.Cpu.A = ADD8(e, e.Cpu.A, e.Cpu.B);
        e.Tick(1);
    }
    public static void X81_ADD_A_C(Emulator e) {
        e.Cpu.A = ADD8(e, e.Cpu.A, e.Cpu.C);
        e.Tick(1);
    }
    public static void X82_ADD_A_D(Emulator e) {
        e.Cpu.A = ADD8(e, e.Cpu.A, e.Cpu.D);
        e.Tick(1);
    }
    public static void X83_ADD_A_E(Emulator e) {
        e.Cpu.A = ADD8(e, e.Cpu.A, e.Cpu.E);
        e.Tick(1);
    }
    public static void X84_ADD_A_H(Emulator e) {
        e.Cpu.A = ADD8(e, e.Cpu.A, e.Cpu.H);
        e.Tick(1);
    }
    public static void X85_ADD_A_L(Emulator e) {
        e.Cpu.A = ADD8(e, e.Cpu.A, e.Cpu.L);
        e.Tick(1);
    }
    public static void X86_ADD_A_MHL(Emulator e) {
        byte v = e.BusRead(e.Cpu.HL);
        e.Tick(1);
        e.Cpu.A = ADD8(e, e.Cpu.A, v);
        e.Tick(1);
    }
    public static void X87_ADD_A_A(Emulator e) {
        e.Cpu.A = ADD8(e, e.Cpu.A, e.Cpu.A);
        e.Tick(1);
    }
    public static void X88_ADC_A_B(Emulator e) {
        e.Cpu.A = ADC8(e, e.Cpu.A, e.Cpu.B);
        e.Tick(1);
    }
    public static void X89_ADC_A_C(Emulator e) {
        e.Cpu.A = ADC8(e, e.Cpu.A, e.Cpu.C);
        e.Tick(1);
    }
    public static void X8A_ADC_A_D(Emulator e) {
        e.Cpu.A = ADC8(e, e.Cpu.A, e.Cpu.D);
        e.Tick(1);
    }
    public static void X8B_ADC_A_E(Emulator e) {
        e.Cpu.A = ADC8(e, e.Cpu.A, e.Cpu.E);
        e.Tick(1);
    }
    public static void X8C_ADC_A_H(Emulator e) {
        e.Cpu.A = ADC8(e, e.Cpu.A, e.Cpu.H);
        e.Tick(1);
    }
    public static void X8D_ADC_A_L(Emulator e) {
        e.Cpu.A = ADC8(e, e.Cpu.A, e.Cpu.L);
        e.Tick(1);
    }
    public static void X8E_ADC_A_MHL(Emulator e) {
        byte v = e.BusRead(e.Cpu.HL);
        e.Tick(1);
        e.Cpu.A = ADC8(e, e.Cpu.A, v);
        e.Tick(1);
    }
    
    public static void X8F_ADC_A_A(Emulator e) {
        e.Cpu.A = ADC8(e, e.Cpu.A, e.Cpu.A);
        e.Tick(1);
    }
    public static void X90_SUB_B(Emulator e) {
        e.Cpu.A = Sub8(e, e.Cpu.A, e.Cpu.B);
        e.Tick(1);
    }
    public static void X91_SUB_C(Emulator e) {
        e.Cpu.A = Sub8(e, e.Cpu.A, e.Cpu.C);
        e.Tick(1);
    }
    public static void X92_SUB_D(Emulator e) {
        e.Cpu.A = Sub8(e, e.Cpu.A, e.Cpu.D);
        e.Tick(1);
    }
    public static void X93_SUB_E(Emulator e) {
        e.Cpu.A = Sub8(e, e.Cpu.A, e.Cpu.E);
        e.Tick(1);
    }
    public static void X94_SUB_H(Emulator e) {
        e.Cpu.A = Sub8(e, e.Cpu.A, e.Cpu.H);
        e.Tick(1);
    }
    public static void X95_SUB_L(Emulator e) {
        e.Cpu.A = Sub8(e, e.Cpu.A, e.Cpu.L);
        e.Tick(1);
    }
    public static void X96_SUB_MHL(Emulator e) {
        byte v = e.BusRead(e.Cpu.HL);
        e.Tick(1);
        e.Cpu.A = Sub8(e, e.Cpu.A, v);
        e.Tick(1);
    }
    public static void X97_SUB_A(Emulator e) {
        e.Cpu.A = Sub8(e, e.Cpu.A, e.Cpu.A);
        e.Tick(1);
    }
    public static void X98_SBC_A_B(Emulator e) {
        e.Cpu.A = Sbc8(e, e.Cpu.A, e.Cpu.B);
        e.Tick(1);
    }
    public static void X99_SBC_A_C(Emulator e) {
        e.Cpu.A = Sbc8(e, e.Cpu.A, e.Cpu.C);
        e.Tick(1);
    }
    public static void X9A_SBC_A_D(Emulator e) {
        e.Cpu.A = Sbc8(e, e.Cpu.A, e.Cpu.D);
        e.Tick(1);
    }
    public static void X9B_SBC_A_E(Emulator e) {
        e.Cpu.A = Sbc8(e, e.Cpu.A, e.Cpu.E);
        e.Tick(1);
    }
    public static void X9C_SBC_A_H(Emulator e) {
        e.Cpu.A = Sbc8(e, e.Cpu.A, e.Cpu.H);
        e.Tick(1);
    }
    public static void X9D_SBC_A_L(Emulator e) {
        e.Cpu.A = Sbc8(e, e.Cpu.A, e.Cpu.L);
        e.Tick(1);
    }
    public static void X9E_SUC_A_MHL(Emulator e) {
        byte v = e.BusRead(e.Cpu.HL);
        e.Tick(1);
        e.Cpu.A = Sbc8(e, e.Cpu.A, v);
        e.Tick(1);
    }
    public static void X9F_SBC_A_A(Emulator e) {
        e.Cpu.A = Sbc8(e, e.Cpu.A, e.Cpu.A);
        e.Tick(1);
    }
    public static void XA0_AND_B(Emulator e) {
        e.Cpu.A = And8(e, e.Cpu.A, e.Cpu.B);
        e.Tick(1);
    }
    public static void XA1_AND_C(Emulator e) {
        e.Cpu.A = And8(e, e.Cpu.A, e.Cpu.C);
        e.Tick(1);
    }
    public static void XA2_AND_D(Emulator e) {
        e.Cpu.A = And8(e, e.Cpu.A, e.Cpu.D);
        e.Tick(1);
    }
    public static void XA3_AND_E(Emulator e) {
        e.Cpu.A = And8(e, e.Cpu.A, e.Cpu.E);
        e.Tick(1);
    }

    public static void XA4_AND_H(Emulator e) {
        e.Cpu.A = And8(e, e.Cpu.A, e.Cpu.H);
        e.Tick(1);
    }
    public static void XA5_AND_L(Emulator e) {
        e.Cpu.A = And8(e, e.Cpu.A, e.Cpu.L);
        e.Tick(1);
    }
    public static void XA6_AND_MHL(Emulator e) {
        byte v = e.BusRead(e.Cpu.HL);
        e.Tick(1);
        e.Cpu.A = And8(e, e.Cpu.A, v);
        e.Tick(1);
    }

    public static void XA7_AND_A(Emulator e) {
        e.Cpu.A = And8(e, e.Cpu.A, e.Cpu.A);
        e.Tick(1);
    }
    public static void XA8_XOR_B(Emulator e) {
        e.Cpu.A = Xor8(e, e.Cpu.A, e.Cpu.B);
        e.Tick(1);
    }
    public static void XA9_XOR_C(Emulator e) {
        e.Cpu.A = Xor8(e, e.Cpu.A, e.Cpu.C);
        e.Tick(1);
    }
    public static void XAA_XOR_D(Emulator e) {
        e.Cpu.A = Xor8(e, e.Cpu.A, e.Cpu.D);
        e.Tick(1);
    }
    public static void XAB_XOR_E(Emulator e) {
        e.Cpu.A = Xor8(e, e.Cpu.A, e.Cpu.E);
        e.Tick(1);
    }
    public static void XAC_XOR_H(Emulator e) {
        e.Cpu.A = Xor8(e, e.Cpu.A, e.Cpu.H);
        e.Tick(1);
    }
    public static void XAD_XOR_L(Emulator e) {
        e.Cpu.A = Xor8(e, e.Cpu.A, e.Cpu.L);
        e.Tick(1);
    }
    public static void XAE_XOR_MHL(Emulator e) {
        byte v = e.BusRead(e.Cpu.HL);
        e.Tick(1);
        e.Cpu.A = Xor8(e, e.Cpu.A, v);
        e.Tick(1);
    }
    public static void XAF_XOR_A(Emulator e) {
        e.Cpu.A = Xor8(e, e.Cpu.A, e.Cpu.A);
        e.Tick(1);
    }
    public static void XB0_OR_B(Emulator e) {
        e.Cpu.A = Or8(e, e.Cpu.A, e.Cpu.B);
        e.Tick(1);
    }
    public static void XB1_OR_C(Emulator e) {
        e.Cpu.A = Or8(e, e.Cpu.A, e.Cpu.C);
        e.Tick(1);
    }
    public static void XB2_OR_D(Emulator e) {
        e.Cpu.A = Or8(e, e.Cpu.A, e.Cpu.D);
        e.Tick(1);
    }
    public static void XB3_OR_E(Emulator e) {
        e.Cpu.A = Or8(e, e.Cpu.A, e.Cpu.E);
        e.Tick(1);
    }
    public static void XB4_OR_H(Emulator e) {
        e.Cpu.A = Or8(e, e.Cpu.A, e.Cpu.H);
        e.Tick(1);
    }
    public static void XB5_OR_L(Emulator e) {
        e.Cpu.A = Or8(e, e.Cpu.A, e.Cpu.L);
        e.Tick(1);
    }
    public static void X86_OR_MHL(Emulator e) {
        byte v = e.BusRead(e.Cpu.HL);
        e.Tick(1);
        e.Cpu.A = Or8(e, e.Cpu.A, v);
        e.Tick(1);
    }

    public static void XB6_OR_I8(Emulator e) {
        byte v = e.BusRead(e.Cpu.HL);
        e.Tick(1);
        e.Cpu.A = Or8(e, e.Cpu.A, v);
        e.Tick(1);
    }
    public static void XB7_OR_A(Emulator e) {
        e.Cpu.A = Or8(e, e.Cpu.A, e.Cpu.A);
        e.Tick(1);
    }

    public static void XB8_CP_B(Emulator e) {
        CP_Byte(e, e.Cpu.A, e.Cpu.B);
        e.Tick(1);
    }
    public static void XB9_CP_C(Emulator e) {
        CP_Byte(e, e.Cpu.A, e.Cpu.C);
        e.Tick(1);
    }
    public static void XBA_CP_D(Emulator e) {
        CP_Byte(e, e.Cpu.A, e.Cpu.D);
        e.Tick(1);
    }
    public static void XBB_CP_E(Emulator e) {
        CP_Byte(e, e.Cpu.A, e.Cpu.E);
        e.Tick(1);
    }
    public static void XBC_CP_H(Emulator e) {
        CP_Byte(e, e.Cpu.A, e.Cpu.H);
        e.Tick(1);
    }
    public static void XBD_CP_L(Emulator e) {
        CP_Byte(e, e.Cpu.A, e.Cpu.L);
        e.Tick(1);
    }
    public static void XBE_CP_MHL(Emulator e) {
        byte v2 = e.BusRead(e.Cpu.HL);
        CP_Byte(e, e.Cpu.A, v2);
        e.Tick(2);
    }
    
    public static void XBF_CP_B(Emulator e) {
        CP_Byte(e, e.Cpu.A, e.Cpu.A);
        e.Tick(1);
    }

    public static void XC0_RET_NZ(Emulator e) {
        if (!e.Cpu.FZ) {
            e.Cpu.PC = Pop16(e);
            e.Tick(5);
        } else {
            e.Tick(2);
        }
    }

    public static void XC1_POP_BC(Emulator e) {
        e.Cpu.BC = Pop16(e);
        e.Tick(3);
    }
    public static void XC2_JPNZ_I16(Emulator e) {
        UInt16 addr = ReadU16(e);
        if (!e.Cpu.FZ) {
            e.Cpu.PC = addr;
            e.Tick(4);
        } 
        else
        {
            e.Tick(3);
        }
    }
    public static void XC3_JP_I16(Emulator e) {
        UInt16 addr = ReadU16(e);
        e.Cpu.PC = addr;
        e.Tick(4);
    }
    public static void XC4_CALLNZ_I16(Emulator e) {
        UInt16 addr = ReadU16(e);
        e.Tick(2);
        if (!e.Cpu.FZ) {
            Push16(e, e.Cpu.PC);
            e.Cpu.PC = addr;
            e.Tick(4);
        } else {
            e.Tick(1);
        }
    }

    public static void XC5_PUSH_BC(Emulator e) {
        Push16(e, e.Cpu.BC);
        e.Tick(4);
    }
    public static void XC6_ADD_A_I8(Emulator e) {
        byte v = ReadU8(e);
        e.Tick(1);
        e.Cpu.A = ADD8(e, e.Cpu.A, v);
        e.Tick(1);
    }
    public static void XC7_RST_00H(Emulator e) {
        Push16(e, e.Cpu.PC);
        e.Cpu.PC = 0;
        e.Tick(4);
    }

    public static void XC8_RETZ(Emulator e) {
        if (e.Cpu.FZ) {
            e.Cpu.PC = Pop16(e);
            e.Tick(5);
        } else {
            e.Tick(2);
        }
    }

    public static void XC9_RET(Emulator e) {
        e.Cpu.PC = Pop16(e);
        e.Tick(4);
    }

    public static void XCA_JPZ_I16(Emulator e) {
        UInt16 addr = ReadU16(e);
        if (e.Cpu.FZ) {
            e.Cpu.PC = addr;
            e.Tick(4);
        } else {
            e.Tick(3);
        }
    }
    public static void XCB_PREFIX_CB(Emulator e) {
        CB(e);
    }
    public static void XCC_CALLZ_I16(Emulator e) {
        UInt16 addr = ReadU16(e);
        e.Tick(2);
        if (e.Cpu.FZ) {
            Push16(e, e.Cpu.PC);
            e.Cpu.PC = addr;
            e.Tick(4);
        } else {
            e.Tick(1);
        }
    }
    public static void XCD_CALL_I16(Emulator e) {
        UInt16 addr = ReadU16(e);
        e.Tick(2);
        Push16(e, e.Cpu.PC);
        e.Cpu.PC = addr;
        e.Tick(4);
    }
    public static void XCE_ADC_A_I8(Emulator e) {
        byte v = ReadU8(e);
        e.Tick(1);
        e.Cpu.A = ADC8(e, e.Cpu.A, v);
        e.Tick(1);
    }
    public static void XCF_RST_08H(Emulator e) {
        Push16(e, e.Cpu.PC);
        e.Cpu.PC = 0x0008;
        e.Tick(4);
    }

    public static void XD0_RETNC(Emulator e) {
        if (!e.Cpu.FC) {
            e.Cpu.PC = Pop16(e);
            e.Tick(5);
        } else {
            e.Tick(2);
        }
    }
    public static void XD1_POP_DE(Emulator e) {
        e.Cpu.DE = Pop16(e);
        e.Tick(3);
    }
    
    public static void XD2_JPNC_I16(Emulator e) {
        UInt16 addr = ReadU16(e);
        if (!e.Cpu.FC) {
            e.Cpu.PC = addr;
            e.Tick(4);
        } else {
            e.Tick(3);
        }
    }
    public static void XD4_CALLNC_I16(Emulator e) {
        UInt16 addr = ReadU16(e);
        e.Tick(2);
        if (!e.Cpu.FC) {
            Push16(e, e.Cpu.PC);
            e.Cpu.PC = addr;
            e.Tick(4);
        } else {
            e.Tick(1);
        }
    }
    public static void XD5_PUSH_DE(Emulator e) {
        Push16(e, e.Cpu.DE);
        e.Tick(4);
    }
    public static void XD6_SUB_I8(Emulator e) {
        byte v = ReadU8(e);
        e.Tick(1);
        e.Cpu.A = Sub8(e, e.Cpu.A, v);
        e.Tick(1);
    }

    public static void XD7_RST_10H(Emulator e) {
        Push16(e, e.Cpu.PC);
        e.Cpu.PC = 0x0010;
        e.Tick(4);
    }

    public static void XD8_RET_C(Emulator e) {
        if (e.Cpu.FC) {
            e.Cpu.PC = Pop16(e);
            e.Tick(5);
        } else {
            e.Tick(2);
        }
    }
    public static void XD9_RETI(Emulator e) {
        e.Cpu.EnableInterruptMaster();
        XC9_RET(e);
    }

    public static void XDA_JPC_I16(Emulator e) {
        UInt16 addr = ReadU16(e);
        if (e.Cpu.FC) {
            e.Cpu.PC = addr;
            e.Tick(4);
        } else {
            e.Tick(3);
        }
    }
    
    public static void XDC_CALLC_I16(Emulator e) {
        UInt16 addr = ReadU16(e);
        e.Tick(2);
        if (e.Cpu.FC) {
            Push16(e, e.Cpu.PC);
            e.Cpu.PC = addr;
            e.Tick(4);
        } else {
            e.Tick(1);
        }
    }
    public static void XDE_SBC_A_I8(Emulator e) {
        byte v = ReadU8(e);
        e.Tick(1);
        e.Cpu.A = Sbc8(e, e.Cpu.A, v);
        e.Tick(1);
    }

    public static void XDF_RST_18H(Emulator e) {
        Push16(e, e.Cpu.PC);
        e.Cpu.PC = 0x0018;
        e.Tick(4);
    }


    public static void XE0_LDH_A2OMI8(Emulator e) {
        byte offset = ReadU8(e);
        e.Tick(1);
        e.BusWrite((UInt16)(0xff00 + (UInt16)offset), e.Cpu.A);
        e.Tick(2);
    }
    public static void XE1_POP_HL(Emulator e) {
        e.Cpu.HL = Pop16(e);
        e.Tick(3);
    }
    public static void XE2_LD_A2OMC(Emulator e) {
        e.BusWrite((UInt16)(e.Cpu.C + 0xff00), e.Cpu.A);
        e.Tick(2);
    }
    public static void XE5_PUSH_HL(Emulator e) {
        Push16(e, e.Cpu.HL);
        e.Tick(4);
    }
    public static void XE6_AND_I8(Emulator e) {
        byte v = ReadU8(e);
        e.Tick(1);
        e.Cpu.A = And8(e, e.Cpu.A, v);
        e.Tick(1);
    }
    public static void XE7_RST_20H(Emulator e) {
        Push16(e, e.Cpu.PC);
        e.Cpu.PC = 0x0020;
        e.Tick(4);
    }
    public static void XE8_ADD_SP_MI8(Emulator e) {
        e.Cpu.ResetFZ();
        e.Cpu.ResetFN();
        UInt16 l = e.Cpu.SP;
        sbyte r = (sbyte)ReadU8(e);
        e.Tick(1);

        UInt16 re = (UInt16)(l + r);
        UInt16 c = (UInt16)((UInt16)(l ^ r) ^ re);
        if ((c & 0x10) > 0) {
            e.Cpu.SetFH();
        } else {
            e.Cpu.ResetFH();
        }
        if ((c & 0x100) > 0) {
            e.Cpu.SetFC();
        } else {
            e.Cpu.ResetFC();
        }

        e.Cpu.SP = re;
        e.Tick(3);
    }

    public static void XE9_JP_HL(Emulator e) {
        e.Cpu.PC = e.Cpu.HL;
        e.Tick(1);
    }

    public static void XEA_LD_A2MI16(Emulator e) {
        UInt16 addr = ReadU16(e);
        e.Tick(2);
        e.BusWrite(addr, e.Cpu.A);
        e.Tick(2);
    }
    public static void XEE_XOR_I8(Emulator e) {
        byte v = ReadU8(e);
        e.Tick(1);
        e.Cpu.A = Xor8(e, e.Cpu.A, v);
        e.Tick(1);
    }

    public static void XEF_RST_28H(Emulator e) {
        Push16(e, e.Cpu.PC);
        e.Cpu.PC = 0x0028;
        e.Tick(4);
    }

    public static void XF0_LDH_OMI82A(Emulator e) {
        byte offset = ReadU8(e);
        e.Tick(1);
        e.Cpu.A = e.BusRead((UInt16)(0xff00 + (UInt16)offset));
        e.Tick(2);
    }
    public static void XF1_POP_AF(Emulator e) {
        e.Cpu.AF = Pop16(e);
        e.Tick(3);
    }
    public static void XF2_LD_OMC2A(Emulator e) {
        e.Cpu.A = e.BusRead((UInt16)(e.Cpu.C + 0xff00));
        e.Tick(2);
    }
    public static void XF3_DI(Emulator e) {
        e.Cpu.DisableInterruptMaster();
        e.Tick(1);
    }
    public static void XF5_PUSH_BC(Emulator e) {
        Push16(e, e.Cpu.AF);
        e.Tick(4);
    }
    public static void XF6_OR_I8(Emulator e) {
        byte data = ReadU8(e);
        e.Tick(1);
        e.Cpu.A = Or8(e, e.Cpu.A, data);
        e.Tick(1);
    }
    public static void XF7_RST_30H(Emulator e) {
        Push16(e, e.Cpu.PC);
        e.Cpu.PC = 0x0030;
        e.Tick(4);
    }

    public static void XF8_LD_SP_ADD_I8_2_HL(Emulator e) {
        e.Cpu.ResetFZ();
        e.Cpu.ResetFN();

        UInt16 l = e.Cpu.SP;
        Int16 r = (sbyte)ReadU8(e);
        e.Tick(1);

        UInt16 re = (UInt16)(l + r);
        UInt16 c = (UInt16)((UInt16)(l ^ r) ^ re);

        if ((c & 0x10) > 0) {
            e.Cpu.SetFH();
        } else e.Cpu.ResetFH();
        
        if ((c & 0x100) > 0) {
            e.Cpu.SetFC();
        } else  e.Cpu.ResetFC();

        e.Cpu.HL = re;
        e.Tick(2);
    }
    public static void XF9_LD_HL2SP(Emulator e) {
        e.Cpu.SP = e.Cpu.HL;
        e.Tick(2);
    }
    public static void XFA_LD_MI162A(Emulator e) {
        UInt16 addr = ReadU16(e);
        e.Tick(2);
        e.Cpu.A = e.BusRead(addr);
        e.Tick(2);
    }
    public static void XFB_EI(Emulator e) {
        e.Cpu.EnableInterruptMaster();
        e.Tick(1);
    }
    public static void XFE_CP_I8(Emulator e) {
        CP_Byte(e, e.Cpu.A, ReadU8(e));
        e.Tick(2);
    }
    public static void XFF_RST_38H(Emulator e) {
        Push16(e, e.Cpu.PC);
        e.Cpu.PC = 0x0038;
        e.Tick(4);
    }
    public static UInt16 ReadU16(Emulator e) {
        UInt16 r = (UInt16)(
            e.BusRead(e.Cpu.PC) |
            e.BusRead((ushort)(e.Cpu.PC + 1)) << 8);
        e.Cpu.PC += 2;
        return r;
    }
    public static byte ReadU8(Emulator e) {
        byte r = e.BusRead(e.Cpu.PC);
        ++e.Cpu.PC;
        return r;
    }
    public static void ZeroFlag(Emulator e, byte v) {
        if (v != 0) {
            e.Cpu.ResetFZ();
        } else {
            e.Cpu.SetFZ();
        }
    }

    public static void CP_Byte(Emulator e, byte v1, byte v2) {
        e.Cpu.SetFN();

        ZeroFlag(e, (v1 == v2) ? (byte)0 : (byte)1);

        if ((UInt16)(v1 & (UInt16)0x0f) < (UInt16)(v2 & (UInt16)0x0f)) {
            e.Cpu.SetFH();
        } else {
            e.Cpu.ResetFH();
        }

        if (v1 < v2) {
            e.Cpu.SetFC();
        } else {
            e.Cpu.ResetFC();
        }
    }

    public static void LD(Emulator e, CpuRegister dest, CpuRegister sour) {
        byte rdest = 0;
        switch (dest) {
            case CpuRegister.A:
                rdest = e.Cpu.A; break;
            case CpuRegister.B:
                rdest = e.Cpu.B; break;
        }
        switch (sour) {
            case CpuRegister.A:
                e.Cpu.A = rdest;
                break;
        }
    }
    public static void Push16(Emulator e, UInt16 v) {
        e.Cpu.SP -= 2;
        e.BusWrite((UInt16)(e.Cpu.SP + (UInt16)1), (byte)(v >> 8));
        e.BusWrite(e.Cpu.SP, (byte)(v & 0xff));
    }
    public static UInt16 Pop16(Emulator e) {
        byte l = e.BusRead(e.Cpu.SP);
        byte h = e.BusRead((UInt16)(e.Cpu.SP + 1));
        e.Cpu.SP += 2;
        return (UInt16)((h << 8) | l);
    }
    public static void Inc8(Emulator e, ref byte v) {
        ++v;
        ZeroFlag(e, v);
        e.Cpu.ResetFN();
        if ((v & 0x0F) == 0)
        {
            e.Cpu.SetFH();
        } 
        else 
        {
            e.Cpu.ResetFH();
        }
    }
    public static void Dec8(Emulator e, ref byte v) {
        --v;
        ZeroFlag(e, v);
        e.Cpu.SetFN();
        if ((v & 0x0f) == 0x0f) {
            e.Cpu.SetFH();
        } else {
            e.Cpu.ResetFH();
        }
    }
    public static byte ADD8(Emulator e, UInt16 v1, UInt16 v2) {
        byte r = (byte)(v1 + v2);
        ZeroFlag(e, r);
        e.Cpu.ResetFN();
        if ((v1 & 0x0f) + (v2 & 0x0f) > 0x0f) {
            e.Cpu.SetFH();
        } else {
            e.Cpu.ResetFH();
        }

        if (v1 + v2 > 0xff) {
            e.Cpu.SetFC();
        } else {
            e.Cpu.ResetFC();
        }
        return r;
    }
    public static UInt16 ADD16(Emulator e, UInt32 v1, UInt32 v2) {
        e.Cpu.ResetFN();
        UInt16 r = (UInt16)(v1 + v2);
        if ((v1 & 0xfff) + (v2 & 0xfff) > 0xfff) {
            e.Cpu.SetFH();
        } else {
            e.Cpu.ResetFH();
        }

        if (v1 + v2 > 0xffff) {
            e.Cpu.SetFC();
        } else {
            e.Cpu.ResetFC();
        }

        return r;
    }
    public static byte ADC8(Emulator e, UInt16 v1, UInt16 v2) {
        UInt16 c = e.Cpu.FC ? (UInt16)1 : (UInt16)0;
        byte r = (byte)(v1 + v2 + c);
        ZeroFlag(e, r);
        e.Cpu.ResetFN();

        if (((v1 & 0x0f) + (v2 & 0x0f) + c) > 0x0f) {
            e.Cpu.SetFH();
        } else {
            e.Cpu.ResetFH();
        }

        if ((v1 + v2 + c) > 0xff) {
            e.Cpu.SetFC();
        } else {
            e.Cpu.ResetFC();
        }
        return r;
    }

    public static byte Sub8(Emulator e, UInt16 v1, UInt16 v2) {
        e.Cpu.SetFN();
        byte r = (byte)((byte)v1 - (byte)v2);
        ZeroFlag(e, r);

        if ((v1 & 0x0f) < (v2 & 0x0f)) {
            e.Cpu.SetFH();
        } else {
            e.Cpu.ResetFH();
        }

        if (v1 < v2) {
            e.Cpu.SetFC();
        } else {
            e.Cpu.ResetFC();
        }
        return r;
    }
    public static byte Sbc8(Emulator e, UInt16 v1, UInt16 v2) {
        byte c = (byte)(e.Cpu.FC ? 1 : 0);
        byte r = (byte)(v1 - v2 - c);
        ZeroFlag(e, r);
        e.Cpu.SetFN();

        if ((v1 & 0x0f) < ((v2 & 0x0f) + c)) {
            e.Cpu.SetFH();
        } else {
            e.Cpu.ResetFH();
        }

        if (v1 < (v2 + c)) {
            e.Cpu.SetFC();
        } else {
            e.Cpu.ResetFC();
        }

        return r;
    }
    public static byte And8(Emulator e, byte v1, byte v2) {
        e.Cpu.ResetFN();
        e.Cpu.ResetFC();
        e.Cpu.SetFH();
        byte r = (byte)(v1 & v2);
        ZeroFlag(e, r);
        return r;
    }

    public static byte Xor8(Emulator e, byte v1, byte v2) {
        e.Cpu.ResetFN();
        e.Cpu.ResetFC();
        e.Cpu.ResetFH();
        byte r = (byte)(v1 ^ v2);
        ZeroFlag(e, r);
        return r;
    }
    public static byte Or8(Emulator e, byte v1, byte v2) {
        e.Cpu.ResetFN();
        e.Cpu.ResetFC();
        e.Cpu.ResetFH();
        byte r = (byte)(v1 | v2);
        ZeroFlag(e, r);
        return r;
    }
    public static void CB(Emulator e) {
        byte opt = ReadU8(e);
        e.Tick(1);

        byte register_bit = (byte)(opt & 0x07);
        byte opt_code = (byte)((opt & 0xf8) >> 3);
        byte v;
        switch (register_bit) {
            case 0:
                v = e.Cpu.B;break;
            case 1:
                v = e.Cpu.C;break;
            case 2:
                v = e.Cpu.D;break;
            case 3:
                v = e.Cpu.E;break;
            case 4:
                v = e.Cpu.H;break;
            case 5:
                v = e.Cpu.L;break;
            case 6:
                v = e.BusRead(e.Cpu.HL);
                e.Tick(1);
                break;
            case 7:
                v = e.Cpu.A;break;
            default:
                throw new InvalidDataException("Expect number from 0 to 7");
        }

        if (opt_code == 0) RLC8(e, ref v);
        else if (opt_code == 1) RRC8(e, ref v);
        else if (opt_code == 2) RL8(e, ref v);
        else if (opt_code == 3) RR8(e, ref v);
        else if (opt_code == 4) SLA8(e, ref v);
        else if (opt_code == 5) SRA8(e, ref v);
        else if (opt_code == 6) Swap8(e, ref v);
        else if (opt_code == 7) SRL8(e, ref v);
        else if (opt_code <= 0x0f) Bit8(e, v, (byte)(opt_code - 0x08));
        else if (opt_code <= 0x17) Res8(e, ref v, (byte)(opt_code - 0x10));
        else if (opt_code <= 0x1F) Set8(e, ref v, (byte)(opt_code - 0x18));
        else Console.WriteLine("\x1b[31m[Error]: Invalid CB OPT_CODE 0x{0:X2}!\x1b[0m", opt_code);

        if (opt_code <= 0x07 || opt_code >= 0x10)
        {
            switch (register_bit)
            {
                case 0: e.Cpu.B = v;break;
                case 1: e.Cpu.C = v;break;
                case 2: e.Cpu.D = v;break;
                case 3: e.Cpu.E = v;break;
                case 4: e.Cpu.H = v;break;
                case 5: e.Cpu.L = v;break;
                case 6: e.BusWrite(e.Cpu.HL, v);e.Tick(1);break;
                case 7: e.Cpu.A = v;break;
            }
        }
        e.Tick(1);
    }
    public static void RLC8(Emulator e, ref byte v) 
    {
        bool c = (v & 0x80) != 0;
        v <<= 1;
        if (c)
        {
            v |= 0x01;
            e.Cpu.SetFC();
        }
        else
        {
            e.Cpu.ResetFC();
        }
        ZeroFlag(e, v);
        e.Cpu.ResetFN();
        e.Cpu.ResetFH();
    }
    public static void RRC8(Emulator e, ref byte v) 
    {
        bool c = (v & 0x01) != 0;
        v >>= 1;
        if (c)
        {
            v |= 0x80;
            e.Cpu.SetFC();
        }
        else
        {
            e.Cpu.ResetFC();
        }
        ZeroFlag(e, v);
        e.Cpu.ResetFN();
        e.Cpu.ResetFH();
    }
    public static void RL8(Emulator e, ref byte v)
    {
        bool c = (v & 0x80) != 0;
        v <<= 1;
        v |= (byte)(e.Cpu.FC ? 1 : 0);
        if (c)
            e.Cpu.SetFC();
        else e.Cpu.ResetFC();
        ZeroFlag(e, v);
        e.Cpu.ResetFN();
        e.Cpu.ResetFH();
    }
    public static void RR8(Emulator e, ref byte v)
    {
        bool c = (v & 0x01) != 0;
        v >>= 1;
        v |= (byte)((e.Cpu.FC ? 1 : 0) << 7);
        if (c)
            e.Cpu.SetFC();
        else e.Cpu.ResetFC();
        ZeroFlag(e, v);
        e.Cpu.ResetFN();
        e.Cpu.ResetFH();
    }

    public static void SLA8(Emulator e, ref byte v)
    {
        bool c = (v & 0x80) != 0;
        v <<= 1;
        if (c)
            e.Cpu.SetFC();
        else e.Cpu.ResetFC();
        ZeroFlag(e, v);
        e.Cpu.ResetFN();
        e.Cpu.ResetFH();
    }

    public static void SRA8(Emulator e, ref byte v)
    {
        bool c = (v & 0x01) != 0;
        byte preserve = (byte)(v & 0x80);
        v >>= 1;
        v |= preserve;
        if (c)
            e.Cpu.SetFC();
        else e.Cpu.ResetFC();
        ZeroFlag(e, v);
        e.Cpu.ResetFN();
        e.Cpu.ResetFH();
    }

    public static void SRL8(Emulator e, ref byte v)
    {
        bool c = (v & 0x01) != 0;
        v >>= 1;
        v &= 0x7f;
        if (c)
            e.Cpu.SetFC();
        else e.Cpu.ResetFC();
        ZeroFlag(e, v);
        e.Cpu.ResetFN();
        e.Cpu.ResetFH();
    }
    public static void Swap8(Emulator e, ref byte v)
    {
        v = (byte)((byte)(v << 4) | (byte)(v >> 4));
        ZeroFlag(e, v);
        e.Cpu.ResetFC();
        e.Cpu.ResetFN();
        e.Cpu.ResetFH();
    }
    public static void Bit8(Emulator e, byte v, byte bit)
    {
        if (Bit(v, bit))
        {
            e.Cpu.ResetFZ();
        }
        else e.Cpu.SetFZ();
        e.Cpu.ResetFN();
        e.Cpu.SetFH();
    }
    
    public static bool Bit(byte v, byte bit)
    {
        return ((v >> bit) & 0x01) == 1;
    }
    public static void Res8(Emulator e, ref byte v, byte bit)
    {
        BitReset(ref v, bit);
    }
    public static void BitReset(ref byte v, byte bit)
    {
        v &= (byte)~(1 << bit);
    }
    public static void Set8(Emulator e, ref byte v, byte bit)
    {
        BitSet(ref v, bit);
    }
    public static void BitSet(ref byte v, byte bit)
    {
        v |= (byte)(1 << bit);
    }


    public static readonly string[] OptNames = {
        "x00 NOP",
        "x01 LD BC, d16",
        "x02 LD (BC), A",
        "x03 INC BC",
        "x04 INC B",
        "x05 DEC B",
        "x06 LD B, d8",
        "x07 RLCA",
        "x08 LD (a16), SP",
        "x09 ADD HL, BC",
        "x0A LD A, (BC)",
        "x0B DEC BC",
        "x0C INC C",
        "x0D DEC C",
        "x0E LD C, d8",
        "x0F RRCA",
        "x10 STOP 0",
        "x11 LD DE, d16",
        "x12 LD (DE), A",
        "x13 INC DE",
        "x14 INC D",
        "x15 DEC D",
        "x16 LD D, d8",
        "x17 RLA",
        "x18 JR r8",
        "x19 ADD HL, DE",
        "x1A LD A, (DE)",
        "x1B DEC DE",
        "x1C INC E",
        "x1D DEC E",
        "x1E LD E, d8",
        "x1F RRA",
        "x20 JR NZ, r8",
        "x21 LD HL, d16",
        "x22 LD (HL+), A",
        "x23 INC HL",
        "x24 INC H",
        "x25 DEC H",
        "x26 LD H, d8",
        "x27 DAA",
        "x28 JR Z, r8",
        "x29 ADD HL, HL",
        "x2A LD A, (HL+)",
        "x2B DEC HL",
        "x2C INC L",
        "x2D DEC L",
        "x2E LD L, d8",
        "x2F CPL",
        "x30 JR NC, r8",
        "x31 LD SP, d16",
        "x32 LD (HL-), A",
        "x33 INC SP",
        "x34 INC (HL)",
        "x35 DEC (HL)",
        "x36 LD (HL), d8",
        "x37 SCF",
        "x38 JR C, r8",
        "x39 ADD HL, SP",
        "x3A LD A, (HL-)",
        "x3B DEC SP",
        "x3C INC A",
        "x3D DEC A",
        "x3E LD A, d8",
        "x3F CCF",
        "x40 LD B, B",
        "x41 LD B, C",
        "x42 LD B, D",
        "x43 LD B, E",
        "x44 LD B, H",
        "x45 LD B, L",
        "x46 LD B, (HL)",
        "x47 LD B, A",
        "x48 LD C, B",
        "x49 LD C, C",
        "x4A LD C, D",
        "x4B LD C, E",
        "x4C LD C, H",
        "x4D LD C, L",
        "x4E LD C, (HL)",
        "x4F LD C, A",
        "x50 LD D, B",
        "x51 LD D, C",
        "x52 LD D, D",
        "x53 LD D, E",
        "x54 LD D, H",
        "x55 LD D, L",
        "x56 LD D, (HL)",
        "x57 LD D, A",
        "x58 LD E, B",
        "x59 LD E, C",
        "x5A LD E, D",
        "x5B LD E, E",
        "x5C LD E, H",
        "x5D LD E, L",
        "x5E LD E, (HL)",
        "x5F LD E, A",
        "x60 LD H, B",
        "x61 LD H, C",
        "x62 LD H, D",
        "x63 LD H, E",
        "x64 LD H, H",
        "x65 LD H, L",
        "x66 LD H, (HL)",
        "x67 LD H, A",
        "x68 LD L, B",
        "x69 LD L, C",
        "x6A LD L, D",
        "x6B LD L, E",
        "x6C LD L, H",
        "x6D LD L, L",
        "x6E LD L, (HL)",
        "x6F LD L, A",
        "x70 LD (HL), B",
        "x71 LD (HL), C",
        "x72 LD (HL), D",
        "x73 LD (HL), E",
        "x74 LD (HL), H",
        "x75 LD (HL), L",
        "x76 HALT",
        "x77 LD (HL), A",
        "x78 LD A, B",
        "x79 LD A, C",
        "x7A LD A, D",
        "x7B LD A, E",
        "x7C LD A, H",
        "x7D LD A, L",
        "x7E LD A, (HL)",
        "x7F LD A, A",
        "x80 ADD A, B",
        "x81 ADD A, C",
        "x82 ADD A, D",
        "x83 ADD A, E",
        "x84 ADD A, H",
        "x85 ADD A, L",
        "x86 ADD A, (HL)",
        "x87 ADD A, A",
        "x88 ADC A, B",
        "x89 ADC A, C",
        "x8A ADC A, D",
        "x8B ADC A, E",
        "x8C ADC A, H",
        "x8D ADC A, L",
        "x8E ADC A, (HL)",
        "x8F ADC A, A",
        "x90 SUB B",
        "x91 SUB C",
        "x92 SUB D",
        "x93 SUB E",
        "x94 SUB H",
        "x95 SUB L",
        "x96 SUB (HL)",
        "x97 SUB A",
        "x98 SBC A, B",
        "x99 SBC A, C",
        "x9A SBC A, D",
        "x9B SBC A, E",
        "x9C SBC A, H",
        "x9D SBC A, L",
        "x9E SBC A, (HL)",
        "x9F SBC A, A",
        "xA0 AND B",
        "xA1 AND C",
        "xA2 AND D",
        "xA3 AND E",
        "xA4 AND H",
        "xA5 AND L",
        "xA6 AND (HL)",
        "xA7 AND A",
        "xA8 XOR B",
        "xA9 XOR C",
        "xAA XOR D",
        "xAB XOR E",
        "xAC XOR H",
        "xAD XOR L",
        "xAE XOR (HL)",
        "xAF XOR A",
        "xB0 OR B",
        "xB1 OR C",
        "xB2 OR D",
        "xB3 OR E",
        "xB4 OR H",
        "xB5 OR L",
        "xB6 OR (HL)",
        "xB7 OR A",
        "xB8 CP B",
        "xB9 CP C",
        "xBA CP D",
        "xBB CP E",
        "xBC CP H",
        "xBD CP L",
        "xBE CP (HL)",
        "xBF CP A",
        "xC0 RET NZ",
        "xC1 POP BC",
        "xC2 JP NZ, a16",
        "xC3 JP a16",
        "xC4 CALL NZ, a16",
        "xC5 PUSH BC",
        "xC6 ADD A, d8",
        "xC7 RST 00H",
        "xC8 RET Z",
        "xC9 RET",
        "xCA JP Z, a16",
        "xCB PREFIX CB",
        "xCC CALL Z, a16",
        "xCD CALL a16",
        "xCE ADC A, d8",
        "xCF RST 08H",
        "xD0 RET NC",
        "xD1 POP DE",
        "xD2 JP NC, a16",
        "xD3 NULL",
        "xD4 CALL NC, a16",
        "xD5 PUSH DE",
        "xD6 SUB d8",
        "xD7 RST 10H",
        "xD8 RET C",
        "xD9 RETI",
        "xDA JP C, a16",
        "xDB NULL",
        "xDC CALL C, a16",
        "xDD NULL",
        "xDE SBC A, d8",
        "xDF RST 18H",
        "xE0 LDH (a8), A",
        "xE1 POP HL",
        "xE2 LD (C), A",
        "xE3 NULL",
        "xE4 NULL",
        "xE5 PUSH HL",
        "xE6 AND d8",
        "xE7 RST 20H",
        "xE8 ADD SP, r8",
        "xE9 JP (HL)",
        "xEA LD (a16), A",
        "xEB NULL",
        "xEC NULL",
        "xED NULL",
        "xEE XOR d8",
        "xEF RST 28H",
        "xF0 LDH A, (a8)",
        "xF1 POP AF",
        "xF2 LD A, (C)",
        "xF3 DI",
        "xF4 NULL",
        "xF5 PUSH AF",
        "xF6 OR d8",
        "xF7 RST 30H",
        "xF8 LD HL, SP+r8",
        "xF9 LD SP, HL",
        "xFA LD A, (a16)",
        "xFB EI",
        "xFC NULL",
        "xFD NULL",
        "xFE CP d8",
        "xFF RST 38H"
    };
    public static string GetOptName(int idx)
    {
        return OptNames[idx];
    }
}

public enum CpuRegister {
    A = 0,
    B = 1,
}

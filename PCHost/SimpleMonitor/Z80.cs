using System;

//
// This was originally designed for a different (reverse engineering) project, re-purposing
//to save me writing it from scratch - (its possible it was unfinished)
//It doesn't yet support Z80N instructions...
//

namespace SimpleMonitor
{
    public interface MachineInfo
    {
        bool FetchByte(UInt64 address, out byte _b);
        bool FetchWord(UInt64 address, out UInt16 _w);
    }

    public class D_Z80
    {

        [FlagsAttribute]
        public enum InstructionCoding : byte
        {
            None = 0,
            Jmp = 1,
            Call = 2,
            Ret = 4,
            Conditional = 8,
            Prefix = 16,
            Rst = 32,           // Fixme - not needed, need a way to specify implied operands instead
        };

        [FlagsAttribute]
        public enum InstructionOperand : byte
        {
            None = 0,
            Imm8 = 1,
            Imm16 = 2,
            Rel8 = 3,
            XDCBByte = 4,
            HWReg8 = 5,
            Imm16HiLo = 6,
        };

        public struct TableEntry
        {
            public string mnemonic;
            public InstructionCoding opType;
            public InstructionOperand[] operands;
        };

        readonly string[] reg8_nohl = { "b", "c", "d", "e", "h", "l", null, "a" };
        readonly string[] reg8 = { "b", "c", "d", "e", "h", "l", "(hl)", "a" };
        readonly string[] reg8_X = { "b", "c", "d", "e", "ixh", "ixl", null, "a" };
        readonly string[] reg8_Y = { "b", "c", "d", "e", "iyh", "iyl", null, "a" };
        readonly string[] reg16 = { "bc", "de", "hl", "sp" };
        readonly string[] reg16_X = { "bc", "de", "ix", "sp" };
        readonly string[] reg16_Y = { "bc", "de", "iy", "sp" };
        readonly string[] reg16_A = { "bc", "de", "hl", "af" };
        readonly string[] cc = { "nz", "z", "nc", "c" };
        readonly string[] ccc = { "nz", "z", "nc", "c", "po", "pe", "p", "m" };
        readonly string[] nnn = { "00", "08", "10", "18", "20", "28", "30", "38" };
        readonly string[] interruptModes = { "0", null, "1", "2" };
        readonly string[] bitIndex = { "0", "1", "2", "3", "4", "5", "6", "7" };
        readonly string[] alu = { "add a,", "adc a,", "sub a,", "sbc a,", "and ", "xor ", "or ", "cp " };
        readonly string[] rot = { "rlc ", "rrc ", "rl ", "rr ", "sla ", "sra ", "sll ", "srl " };

        public TableEntry[] baseTable;
        public TableEntry[] CBTable;
        public TableEntry[] DDTable;
        public TableEntry[] EDTable;
        public TableEntry[] FDTable;
        public TableEntry[] DDCBTable;
        public TableEntry[] FDCBTable;

        /*
                  Full Z80 Opcode List Including Undocumented Opcodes
                  ===================================================
                       File: DOCS.Comp.Z80.OpList - Update: 0.10
                        Author: J.G.Harston - Date: 09-09-1997

        nn nn             DD nn          CB nn       FD CB ff nn      ED nn
        --------------------------------------------------------------------------
        00 NOP            -              RLC  B      rlc (iy+0)->b    MOS_QUIT
        01 LD   BC,&0000  -              RLC  C      rlc (iy+0)->c    MOS_CLI 
        02 LD   (BC),A    -              RLC  D      rlc (iy+0)->d    MOS_BYTE
        03 INC  BC        -              RLC  E      rlc (iy+0)->e    MOS_WORD
        04 INC  B         -              RLC  H      rlc (iy+0)->h    MOS_WRCH
        05 DEC  B         -              RLC  L      rlc (iy+0)->l    MOS_RDCH
        06 LD   B,&00     -              RLC  (HL)   RLC  (IY+0)      MOS_FILE
        07 RLCA           -              RLC  A      rlc (iy+0)->a    MOS_ARGS
        08 EX   AF,AF'    -              RRC  B      rrc (iy+0)->b    MOS_BGET
        09 ADD  HL,BC     ADD  IX,BC     RRC  C      rrc (iy+0)->c    MOS_BPUT
        0A LD   A,(BC)    -              RRC  D      rrc (iy+0)->d    MOS_GBPB
        0B DEC  BC        -              RRC  E      rrc (iy+0)->e    MOS_FIND
        0C INC  C         -              RRC  H      rrc (iy+0)->h    MOS_FF0C
        0D DEC  C         -              RRC  L      rrc (iy+0)->l    MOS_FF0D
        0E LD   C,&00     -              RRC  (HL)   RRC  (IY+0)      MOS_FF0E
        0F RRCA           -              RRC  A      rrc (iy+0)->a    MOS_FF0F
        10 DJNZ &4546     -              RL   B      rl  (iy+0)->b    -
        11 LD   DE,&0000  -              RL   C      rl  (iy+0)->c    -
        12 LD   (DE),A    -              RL   D      rl  (iy+0)->d    -
        13 INC  DE        -              RL   E      rl  (iy+0)->e    -
        14 INC  D         -              RL   H      rl  (iy+0)->h    -
        15 DEC  D         -              RL   L      rl  (iy+0)->l    -
        16 LD   D,&00     -              RL   (HL)   RL   (IY+0)      -
        17 RLA            -              RL   A      rl  (iy+0)->a    -
        18 JR   &4546     -              RR   B      rr  (iy+0)->b    -
        19 ADD  HL,DE     ADD  IX,DE     RR   C      rr  (iy+0)->c    -
        1A LD   A,(DE)    -              RR   D      rr  (iy+0)->d    -
        1B DEC  DE        -              RR   E      rr  (iy+0)->e    -
        1C INC  E         -              RR   H      rr  (iy+0)->h    -
        1D DEC  E         -              RR   L      rr  (iy+0)->l    -
        1E LD   E,&00     -              RR   (HL)   RR   (IY+0)      -
        1F RRA            -              RR   A      rr  (iy+0)->a    -
        20 JR   NZ,&4546  -              SLA  B      sla (iy+0)->b    -
        21 LD   HL,&0000  LD   IX,&0000  SLA  C      sla (iy+0)->c    -
        22 LD  (&0000),HL LD  (&0000),IX SLA  D      sla (iy+0)->d    -
        23 INC  HL        INC  IX        SLA  E      sla (iy+0)->e    -
        24 INC  H         INC  IXH       SLA  H      sla (iy+0)->h    -
        25 DEC  H         DEC  IXH       SLA  L      sla (iy+0)->l    -
        26 LD   H,&00     LD   IXH,&00   SLA  (HL)   SLA  (IY+0)      -
        27 DAA            -              SLA  A      sla (iy+0)->a    -
        28 JR   Z,&4546   -              SRA  B      sra (iy+0)->b    -
        29 ADD  HL,HL     ADD  IX,IX     SRA  C      sra (iy+0)->c    -
        2A LD  HL,(&0000) LD  IX,(&0000) SRA  D      sra (iy+0)->d    -
        2B DEC  HL        DEC  IX        SRA  E      sra (iy+0)->e    -
        2C INC  L         INC  IXL       SRA  H      sra (iy+0)->h    -
        2D DEC  L         DEC  IXL       SRA  L      sra (iy+0)->l    -
        2E LD   L,&00     LD   IXL,&00   SRA  (HL)   SRA  (IY+0)      -
        2F CPL            -              SRA  A      sra (iy+0)->a    -
        30 JR   NC,&4546  -              SLS  B      sls (iy+0)->b    -
        31 LD   SP,&0000  -              SLS  C      sls (iy+0)->c    -
        32 LD   (&0000),A -              SLS  D      sls (iy+0)->d    -
        33 INC  SP        -              SLS  E      sls (iy+0)->e    -
        34 INC  (HL)      INC  (IX+0)    SLS  H      sls (iy+0)->h    -
        35 DEC  (HL)      DEC  (IX+0)    SLS  L      sls (iy+0)->l    -
        36 LD   (HL),&00  LD  (IX+0),&00 SLS  (HL)   SLS  (IY+0)      -
        37 SCF            -              SLS  A      sls (iy+0)->a    -
        38 JR   C,&4546   -              SRL  B      srl (iy+0)->b    -
        39 ADD  HL,SP     ADD  IX,SP     SRL  C      srl (iy+0)->c    -
        3A LD   A,(&0000) -              SRL  D      srl (iy+0)->d    -
        3B DEC  SP        -              SRL  E      srl (iy+0)->e    -
        3C INC  A         -              SRL  H      srl (iy+0)->h    -
        3D DEC  A         -              SRL  L      srl (iy+0)->l    -
        3E LD   A,&00     -              SRL  (HL)   SRL  (IY+0)      -
        3F CCF            -              SRL  A      srl (iy+0)->a    -
        40 LD   B,B       -              BIT  0,B    bit 0,(iy+0)->b  IN   B,(C)
        41 LD   B,C       -              BIT  0,C    bit 0,(iy+0)->c  OUT  (C),B
        42 LD   B,D       -              BIT  0,D    bit 0,(iy+0)->d  SBC  HL,BC
        43 LD   B,E       -              BIT  0,E    bit 0,(iy+0)->e  LD   (&0000),BC
        44 LD   B,H       LD   B,IXH     BIT  0,H    bit 0,(iy+0)->h  NEG
        45 LD   B,L       LD   B,IXL     BIT  0,L    bit 0,(iy+0)->l  RETN
        46 LD   B,(HL)    LD   B,(IX+0)  BIT  0,(HL) BIT  0,(IY+0)    IM   0
        47 LD   B,A       -              BIT  0,A    bit 0,(iy+0)->a  LD   I,A
        48 LD   C,B       -              BIT  1,B    bit 1,(iy+0)->b  IN   C,(C)
        49 LD   C,C       -              BIT  1,C    bit 1,(iy+0)->c  OUT  (C),C
        4A LD   C,D       -              BIT  1,D    bit 1,(iy+0)->d  ADC  HL,BC
        4B LD   C,E       -              BIT  1,E    bit 1,(iy+0)->e  LD   BC,(&0000)
        4C LD   C,H       LD   C,IXH     BIT  1,H    bit 1,(iy+0)->h  [neg]
        4D LD   C,L       LD   C,IXL     BIT  1,L    bit 1,(iy+0)->l  RETI
        4E LD   C,(HL)    LD   C,(IX+0)  BIT  1,(HL) BIT  1,(IY+0)    [im0]
        4F LD   C,A       -              BIT  1,A    bit 1,(iy+0)->a  LD   R,A
        50 LD   D,B       -              BIT  2,B    bit 2,(iy+0)->b  IN   D,(C)
        51 LD   D,C       -              BIT  2,C    bit 2,(iy+0)->c  OUT  (C),D
        52 LD   D,D       -              BIT  2,D    bit 2,(iy+0)->d  SBC  HL,DE
        53 LD   D,E       -              BIT  2,E    bit 2,(iy+0)->e  LD   (&0000),DE
        54 LD   D,H       LD   D,IXH     BIT  2,H    bit 2,(iy+0)->h  [neg]
        55 LD   D,L       LD   D,IXL     BIT  2,L    bit 2,(iy+0)->l  [retn]
        56 LD   D,(HL)    LD   D,(IX+0)  BIT  2,(HL) BIT  2,(IY+0)    IM   1
        57 LD   D,A       -              BIT  2,A    bit 2,(iy+0)->a  LD   A,I
        58 LD   E,B       -              BIT  3,B    bit 3,(iy+0)->b  IN   E,(C)
        59 LD   E,C       -              BIT  3,C    bit 3,(iy+0)->c  OUT  (C),E
        5A LD   E,D       -              BIT  3,D    bit 3,(iy+0)->d  ADC  HL,DE
        5B LD   E,E       -              BIT  3,E    bit 3,(iy+0)->e  LD   DE,(&0000)
        5C LD   E,H       LD   E,IXH     BIT  3,H    bit 3,(iy+0)->h  [neg]
        5D LD   E,L       LD   E,IXL     BIT  3,L    bit 3,(iy+0)->l  [reti]
        5E LD   E,(HL)    LD   E,(IX+0)  BIT  3,(HL) BIT  3,(IY+0)    IM   2
        5F LD   E,A       -              BIT  3,A    bit 3,(iy+0)->a  LD   A,R
        60 LD   H,B       LD   IXH,B     BIT  4,B    bit 4,(iy+0)->b  IN   H,(C)
        61 LD   H,C       LD   IXH,C     BIT  4,C    bit 4,(iy+0)->c  OUT  (C),H
        62 LD   H,D       LD   IXH,D     BIT  4,D    bit 4,(iy+0)->d  SBC  HL,HL
        63 LD   H,E       LD   IXH,E     BIT  4,E    bit 4,(iy+0)->e  LD   (&0000),HL
        64 LD   H,H       LD   IXH,IXH   BIT  4,H    bit 4,(iy+0)->h  [neg]
        65 LD   H,L       LD   IXH,IXL   BIT  4,L    bit 4,(iy+0)->l  [retn]
        66 LD   H,(HL)    LD   H,(IX+0)  BIT  4,(HL) BIT  4,(IY+0)    [im0]
        67 LD   H,A       LD   IXH,A     BIT  4,A    bit 4,(iy+0)->a  RRD
        68 LD   L,B       LD   IXL,B     BIT  5,B    bit 5,(iy+0)->b  IN   L,(C)
        69 LD   L,C       LD   IXL,C     BIT  5,C    bit 5,(iy+0)->c  OUT  (C),L
        6A LD   L,D       LD   IXL,D     BIT  5,D    bit 5,(iy+0)->d  ADC  HL,HL
        6B LD   L,E       LD   IXL,E     BIT  5,E    bit 5,(iy+0)->e  LD   HL,(&0000)
        6C LD   L,H       LD   IXL,IXH   BIT  5,H    bit 5,(iy+0)->h  [neg]
        6D LD   L,L       LD   IXL,IXL   BIT  5,L    bit 5,(iy+0)->l  [reti]
        6E LD   L,(HL)    LD   L,(IX+0)  BIT  5,(HL) BIT  5,(IY+0)    [im0]
        6F LD   L,A       LD   IXL,A     BIT  5,A    bit 5,(iy+0)->a  RLD
        70 LD   (HL),B    LD   (IX+0),B  BIT  6,B    bit 6,(iy+0)->b  IN   F,(C)
        71 LD   (HL),C    LD   (IX+0),C  BIT  6,C    bit 6,(iy+0)->c  OUT  (C),F
        72 LD   (HL),D    LD   (IX+0),D  BIT  6,D    bit 6,(iy+0)->d  SBC  HL,SP
        73 LD   (HL),E    LD   (IX+0),E  BIT  6,E    bit 6,(iy+0)->e  LD   (&0000),SP
        74 LD   (HL),H    LD   (IX+0),H  BIT  6,H    bit 6,(iy+0)->h  [neg]
        75 LD   (HL),L    LD   (IX+0),L  BIT  6,L    bit 6,(iy+0)->l  [retn]
        76 HALT           -              BIT  6,(HL) BIT  6,(IY+0)    [im1]
        77 LD   (HL),A    LD   (IX+0),A  BIT  6,A    bit 6,(iy+0)->a  [ld i,i?]
        78 LD   A,B       -              BIT  7,B    bit 7,(iy+0)->b  IN   A,(C)
        79 LD   A,C       -              BIT  7,C    bit 7,(iy+0)->c  OUT  (C),A
        7A LD   A,D       -              BIT  7,D    bit 7,(iy+0)->d  ADC  HL,SP
        7B LD   A,E       -              BIT  7,E    bit 7,(iy+0)->e  LD   SP,(&0000)
        7C LD   A,H       LD   A,IXH     BIT  7,H    bit 7,(iy+0)->h  [neg]
        7D LD   A,L       LD   A,IXL     BIT  7,L    bit 7,(iy+0)->l  [reti]
        7E LD   A,(HL)    LD   A,(IX+0)  BIT  7,(HL) BIT  7,(IY+0)    [im2]
        7F LD   A,A       -              BIT  7,A    bit 7,(iy+0)->a  [ld r,r?]
        80 ADD  A,B       -              RES  0,B    res 0,(iy+0)->b  -
        81 ADD  A,C       -              RES  0,C    res 0,(iy+0)->c  -
        82 ADD  A,D       -              RES  0,D    res 0,(iy+0)->d  -
        83 ADD  A,E       -              RES  0,E    res 0,(iy+0)->e  -
        84 ADD  A,H       ADD  A,IXH     RES  0,H    res 0,(iy+0)->h  -
        85 ADD  A,L       ADD  A,IXL     RES  0,L    res 0,(iy+0)->l  -
        86 ADD  A,(HL)    ADD  A,(IX+0)  RES  0,(HL) RES  0,(IY+0)    -
        87 ADD  A,A       -              RES  0,A    res 0,(iy+0)->a  -
        88 ADC  A,B       -              RES  1,B    res 1,(iy+0)->b  -
        89 ADC  A,C       -              RES  1,C    res 1,(iy+0)->c  -
        8A ADC  A,D       -              RES  1,D    res 1,(iy+0)->d  -
        8B ADC  A,E       -              RES  1,E    res 1,(iy+0)->e  -
        8C ADC  A,H       ADC  A,IXH     RES  1,H    res 1,(iy+0)->h  -
        8D ADC  A,L       ADC  A,IXL     RES  1,L    res 1,(iy+0)->l  -
        8E ADC  A,(HL)    ADC  A,(IX+0)  RES  1,(HL) RES  1,(IY+0)    -
        8F ADC  A,A       -              RES  1,A    res 1,(iy+0)->a  -
        90 SUB  A,B       -              RES  2,B    res 2,(iy+0)->b  -
        91 SUB  A,C       -              RES  2,C    res 2,(iy+0)->c  -
        92 SUB  A,D       -              RES  2,D    res 2,(iy+0)->d  -
        93 SUB  A,E       -              RES  2,E    res 2,(iy+0)->e  -
        94 SUB  A,H       SUB  A,IXH     RES  2,H    res 2,(iy+0)->h  -
        95 SUB  A,L       SUB  A,IXL     RES  2,L    res 2,(iy+0)->l  -
        96 SUB  A,(HL)    SUB  A,(IX+0)  RES  2,(HL) RES  2,(IY+0)    -
        97 SUB  A,A       -              RES  2,A    res 2,(iy+0)->a  -
        98 SBC  A,B       -              RES  3,B    res 3,(iy+0)->b  -
        99 SBC  A,C       -              RES  3,C    res 3,(iy+0)->c  -
        9A SBC  A,D       -              RES  3,D    res 3,(iy+0)->d  -
        9B SBC  A,E       -              RES  3,E    res 3,(iy+0)->e  -
        9C SBC  A,H       SBC  A,IXH     RES  3,H    res 3,(iy+0)->h  -
        9D SBC  A,L       SBC  A,IXL     RES  3,L    res 3,(iy+0)->l  -
        9E SBC  A,(HL)    SBC  A,(IX+0)  RES  3,(HL) RES  3,(IY+0)    -
        9F SBC  A,A       -              RES  3,A    res 3,(iy+0)->a  -
        A0 AND  B         -              RES  4,B    res 4,(iy+0)->b  LDI 
        A1 AND  C         -              RES  4,C    res 4,(iy+0)->c  CPI 
        A2 AND  D         -              RES  4,D    res 4,(iy+0)->d  INI 
        A3 AND  E         -              RES  4,E    res 4,(iy+0)->e  OTI 
        A4 AND  H         AND  IXH       RES  4,H    res 4,(iy+0)->h  -
        A5 AND  L         AND  IXL       RES  4,L    res 4,(iy+0)->l  -
        A6 AND  (HL)      AND  (IX+0)    RES  4,(HL) RES  4,(IY+0)    -
        A7 AND  A         -              RES  4,A    res 4,(iy+0)->a  -
        A8 XOR  B         -              RES  5,B    res 5,(iy+0)->b  LDD 
        A9 XOR  C         -              RES  5,C    res 5,(iy+0)->c  CPD 
        AA XOR  D         -              RES  5,D    res 5,(iy+0)->d  IND 
        AB XOR  E         -              RES  5,E    res 5,(iy+0)->e  OTD 
        AC XOR  H         XOR  IXH       RES  5,H    res 5,(iy+0)->h  -
        AD XOR  L         XOR  IXL       RES  5,L    res 5,(iy+0)->l  -
        AE XOR  (HL)      XOR  (IX+0)    RES  5,(HL) RES  5,(IY+0)    -
        AF XOR  A         -              RES  5,A    res 5,(iy+0)->a  -
        B0 OR   B         -              RES  6,B    res 6,(iy+0)->b  LDIR
        B1 OR   C         -              RES  6,C    res 6,(iy+0)->c  CPIR
        B2 OR   D         -              RES  6,D    res 6,(iy+0)->d  INIR
        B3 OR   E         -              RES  6,E    res 6,(iy+0)->e  OTIR
        B4 OR   H         OR   IXH       RES  6,H    res 6,(iy+0)->h  -
        B5 OR   L         OR   IXL       RES  6,L    res 6,(iy+0)->l  -
        B6 OR   (HL)      OR   (IX+0)    RES  6,(HL) RES  6,(IY+0)    -
        B7 OR   A         -              RES  6,A    res 6,(iy+0)->a  -
        B8 CP   B         -              RES  7,B    res 7,(iy+0)->b  LDDR
        B9 CP   C         -              RES  7,C    res 7,(iy+0)->c  CPDR
        BA CP   D         -              RES  7,D    res 7,(iy+0)->d  INDR
        BB CP   E         -              RES  7,E    res 7,(iy+0)->e  OTDR
        BC CP   H         CP   IXH       RES  7,H    res 7,(iy+0)->h  -
        BD CP   L         CP   IXL       RES  7,L    res 7,(iy+0)->l  -
        BE CP   (HL)      CP   (IX+0)    RES  7,(HL) RES  7,(IY+0)    -
        BF CP   A         -              RES  7,A    res 7,(iy+0)->a  -
        C0 RET  NZ        -              SET  0,B    set 0,(iy+0)->b  -
        C1 POP  BC        -              SET  0,C    set 0,(iy+0)->c  -
        C2 JP   NZ,&0000  -              SET  0,D    set 0,(iy+0)->d  -
        C3 JP   &0000     -              SET  0,E    set 0,(iy+0)->e  -
        C4 CALL NZ,&0000  -              SET  0,H    set 0,(iy+0)->h  -
        C5 PUSH BC        -              SET  0,L    set 0,(iy+0)->l  -
        C6 ADD  A,&00     -              SET  0,(HL) SET  0,(IY+0)    -
        C7 RST  &00       -              SET  0,A    set 0,(iy+0)->a  -
        C8 RET  Z         -              SET  1,B    set 1,(iy+0)->b  -
        C9 RET            -              SET  1,C    set 1,(iy+0)->c  -
        CA JP   Z,&0000   -              SET  1,D    set 1,(iy+0)->d  -
        CB **** CB ****   -              SET  1,E    set 1,(iy+0)->e  -
        CC CALL Z,&0000   -              SET  1,H    set 1,(iy+0)->h  -
        CD CALL &0000     -              SET  1,L    set 1,(iy+0)->l  -
        CE ADC  A,&00     -              SET  1,(HL) SET  1,(IY+0)    -
        CF RST  &08       -              SET  1,A    set 1,(iy+0)->a  -
        D0 RET  NC        -              SET  2,B    set 2,(iy+0)->b  -
        D1 POP  DE        -              SET  2,C    set 2,(iy+0)->c  -
        D2 JP   NC,&0000  -              SET  2,D    set 2,(iy+0)->d  -
        D3 OUT  (&00),A   -              SET  2,E    set 2,(iy+0)->e  -
        D4 CALL NC,&0000  -              SET  2,H    set 2,(iy+0)->h  -
        D5 PUSH DE        -              SET  2,L    set 2,(iy+0)->l  -
        D6 SUB  A,&00     -              SET  2,(HL) SET  2,(IY+0)    -
        D7 RST  &10       -              SET  2,A    set 2,(iy+0)->a  -
        D8 RET  C         -              SET  3,B    set 3,(iy+0)->b  -
        D9 EXX            -              SET  3,C    set 3,(iy+0)->c  -
        DA JP   C,&0000   -              SET  3,D    set 3,(iy+0)->d  -
        DB IN   A,(&00)   -              SET  3,E    set 3,(iy+0)->e  -
        DC CALL C,&0000   -              SET  3,H    set 3,(iy+0)->h  -
        DD **** DD ****   -              SET  3,L    set 3,(iy+0)->l  -
        DE SBC  A,&00     -              SET  3,(HL) SET  3,(IY+0)    -
        DF RST  &18       -              SET  3,A    set 3,(iy+0)->a  -
        E0 RET  PO        -              SET  4,B    set 4,(iy+0)->b  -
        E1 POP  HL        POP  IX        SET  4,C    set 4,(iy+0)->c  -
        E2 JP   PO,&0000  -              SET  4,D    set 4,(iy+0)->d  -
        E3 EX   (SP),HL   EX   (SP),IX   SET  4,E    set 4,(iy+0)->e  -
        E4 CALL PO,&0000  -              SET  4,H    set 4,(iy+0)->h  -
        E5 PUSH HL        PUSH IX        SET  4,L    set 4,(iy+0)->l  -
        E6 AND  &00       -              SET  4,(HL) SET  4,(IY+0)    -
        E7 RST  &20       -              SET  4,A    set 4,(iy+0)->a  -
        E8 RET  PE        -              SET  5,B    set 5,(iy+0)->b  -
        E9 JP   (HL)      JP   (IX)      SET  5,C    set 5,(iy+0)->c  -
        EA JP   PE,&0000  -              SET  5,D    set 5,(iy+0)->d  -
        EB EX   DE,HL     -              SET  5,E    set 5,(iy+0)->e  -
        EC CALL PE,&0000  -              SET  5,H    set 5,(iy+0)->h  -
        ED **** ED ****   -              SET  5,L    set 5,(iy+0)->l  -
        EE XOR  &00       -              SET  5,(HL) SET  5,(IY+0)    -
        EF RST  &28       -              SET  5,A    set 5,(iy+0)->a  -
        F0 RET  P         -              SET  6,B    set 6,(iy+0)->b  -
        F1 POP  AF        -              SET  6,C    set 6,(iy+0)->c  -
        F2 JP   P,&0000   -              SET  6,D    set 6,(iy+0)->d  -
        F3 DI             -              SET  6,E    set 6,(iy+0)->e  -
        F4 CALL P,&0000   -              SET  6,H    set 6,(iy+0)->h  -
        F5 PUSH AF        -              SET  6,L    set 6,(iy+0)->l  -
        F6 OR   &00       -              SET  6,(HL) SET  6,(IY+0)    -
        F7 RST  &30       -              SET  6,A    set 6,(iy+0)->a  -
        F8 RET  M         -              SET  7,B    set 7,(iy+0)->b  [z80]
        F9 LD   SP,HL     -              SET  7,C    set 7,(iy+0)->c  [z80]
        FA JP   M,&0000   -              SET  7,D    set 7,(iy+0)->d  [z80]
        FB EI             -              SET  7,E    set 7,(iy+0)->e  ED_LOAD
        FC CALL M,&0000   -              SET  7,H    set 7,(iy+0)->h  [z80]
        FD **** FD ****   -              SET  7,L    set 7,(iy+0)->l  [z80]
        FE CP   &00       -              SET  7,(HL) SET  7,(IY+0)    [z80]
        FF RST  &38       -              SET  7,A    set 7,(iy+0)->a  ED_DOS

        Notes on index registers
        ------------------------
        Where DD and IX are mentioned, FD and IY may be substituted and vis versa.

        Notes on Indexed Shift/Bit Operations
        -------------------------------------
        A shift or bit operation on an indexed byte in memory is done by prefixing
        a CB opcode refering to (HL) with DD or FD to specify (IX+n) or (IY+n). 
        If the CB opcode does not refer to (HL), slightly differing things happen.
        The majority of Z80 CPUs execute them as shown; the shift or bit operation
        is done on and indexed byte in memory, and then if the opcode does not
        specify (HL) originally, the resultant byte is copied into the specified
        register.  This is summarised with this example:
               CB 0x    RLC r          FD CB nn 0x     RLC (IY+nn)->r
               for x=0..5, 7 for r=B,C,D,E,H,L,A

        Some CPUs allow access to the high and low halves of the index register,
        if x is 4 or 5, the operation does RLC IYH or RLC IYH.
               CB 04   RLC H           FD CB nn 04     RLC IYH
               CB 05   RLC L           FD CB nn 05     RLC IYL

        Some CPUs treat all the subcodes as accessing the indexed byte and nothing
        else:
               CB 0x   RLC r           FD CB nn 0X     RLC (IY+nn)
                                       for all x=0..7

        Notes on ED opcodes
        -------------------
        J.G.Harston's !Z80Tube Z80 CoPro emulator includes the extra opcodes ED00
        to ED0F to interface with the host.  G.A.Lunter's Z80 Spectrum emulator
        includes the extra opcodes EDF8 to EDFF to interface to the host.
        */

        public D_Z80()
        {
            baseTable = new TableEntry[256];
            CBTable = new TableEntry[256];
            DDTable = new TableEntry[256];
            EDTable = new TableEntry[256];
            FDTable = new TableEntry[256];
            DDCBTable = new TableEntry[256];
            FDCBTable = new TableEntry[256];

            AddInstruction(ref baseTable, InstructionCoding.Prefix, new InstructionOperand[] { }, "11001011", "");
            AddInstruction(ref baseTable, InstructionCoding.Prefix, new InstructionOperand[] { }, "11011101", "");
            AddInstruction(ref baseTable, InstructionCoding.Prefix, new InstructionOperand[] { }, "11101101", "");
            AddInstruction(ref baseTable, InstructionCoding.Prefix, new InstructionOperand[] { }, "11111101", "");

            AddInstruction(ref baseTable, InstructionCoding.Jmp | InstructionCoding.Conditional, new InstructionOperand[] { InstructionOperand.Rel8 }, "00010000", "djnz @@0@@");
            AddInstruction(ref baseTable, InstructionCoding.Jmp, new InstructionOperand[] { InstructionOperand.Rel8 }, "00011000", "jr @@0@@");
            AddInstruction(ref baseTable, InstructionCoding.Jmp | InstructionCoding.Conditional, new InstructionOperand[] { InstructionOperand.Rel8 }, "001aa000", "jr {0},@@0@@", new string[][] { cc });
            AddInstruction(ref baseTable, InstructionCoding.Jmp, new InstructionOperand[] { InstructionOperand.Imm16 }, "11000011", "jp @@0@@");
            AddInstruction(ref baseTable, InstructionCoding.Jmp | InstructionCoding.Conditional, new InstructionOperand[] { InstructionOperand.Imm16 }, "11aaa010", "jp {0},@@0@@", new string[][] { ccc });
            AddInstruction(ref baseTable, InstructionCoding.Rst, new InstructionOperand[] { }, "11aaa111", "rst {0}", new string[][] { nnn });
            AddInstruction(ref baseTable, InstructionCoding.Call | InstructionCoding.Conditional, new InstructionOperand[] { InstructionOperand.Imm16 }, "11aaa100", "call {0},@@0@@", new string[][] { ccc });
            AddInstruction(ref baseTable, InstructionCoding.Call, new InstructionOperand[] { InstructionOperand.Imm16 }, "11001101", "call @@0@@");
            AddInstruction(ref baseTable, InstructionCoding.Ret | InstructionCoding.Conditional, new InstructionOperand[] { }, "11aaa000", "ret {0}", new string[][] { ccc });
            AddInstruction(ref baseTable, InstructionCoding.Ret, new InstructionOperand[] { }, "11001001", "ret");
            AddInstruction(ref baseTable, InstructionCoding.Ret, new InstructionOperand[] { }, "11101001", "jp (hl)");      // treated as ret, since we don't know the value of HL

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00aaa100", "inc {0}", new string[][] { reg8 });
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00aaa101", "dec {0}", new string[][] { reg8 });

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "11110011", "di");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "11111011", "ei");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "01110110", "halt");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00000111", "rlca");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00010111", "rla");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00001111", "rrca");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00011111", "rra");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00000000", "nop");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00100111", "daa");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00101111", "cpl");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00111111", "ccf");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00110111", "scf");

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "10101aaa", "xor {0}", new string[][] { reg8 });

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00001010", "ld a,(bc)");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00011010", "ld a,(de)");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00000010", "ld (bc),a");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00010010", "ld (de),a");

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "00aa0001", "ld {0},@@0@@", new string[][] { reg16 });
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "00111010", "ld a,(@@0@@)");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "00101010", "ld hl,(@@0@@)");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "00100010", "ld (@@0@@),hl");

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00aa1001", "add hl,{0}", new string[][] { reg16 });

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "11111001", "ld sp,hl");

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00aa0011", "inc {0}", new string[][] { reg16 });
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00aa1011", "dec {0}", new string[][] { reg16 });

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "01aaabbb", "ld {0},{1}", new string[][] { reg8_nohl, reg8 }); // ld (hl),(hl) is halt, so these 2 cover other combinations
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "01aaabbb", "ld {0},{1}", new string[][] { reg8, reg8_nohl });

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "00110010", "ld (@@0@@),a");

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "00aaa110", "ld {0},@@0@@", new string[][] { reg8 });

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "11010011", "out (@@0@@),a");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "11011011", "in a,(@@0@@)");

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "10aaabbb", "{0}{1}", new string[][] { alu, reg8 });
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "11aaa110", "{0}@@0@@", new string[][] { alu });


            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "11101011", "ex de,hl");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "11011001", "exx");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "00001000", "ex af,af'");
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "11100011", "ex (sp),hl");

            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "11aa0101", "push {0}", new string[][] { reg16_A });
            AddInstruction(ref baseTable, InstructionCoding.None, new InstructionOperand[] { }, "11aa0001", "pop {0}", new string[][] { reg16_A });

            // CB - complete
            AddInstruction(ref CBTable, InstructionCoding.None, new InstructionOperand[] { }, "00aaabbb", "{0}{1}", new string[][] { rot, reg8 });
            AddInstruction(ref CBTable, InstructionCoding.None, new InstructionOperand[] { }, "01aaabbb", "bit {0},{1}", new string[][] { bitIndex, reg8 });
            AddInstruction(ref CBTable, InstructionCoding.None, new InstructionOperand[] { }, "10aaabbb", "res {0},{1}", new string[][] { bitIndex, reg8 });
            AddInstruction(ref CBTable, InstructionCoding.None, new InstructionOperand[] { }, "11aaabbb", "set {0},{1}", new string[][] { bitIndex, reg8 });

            // DD

            AddInstruction(ref DDTable, InstructionCoding.Prefix, new InstructionOperand[] { }, "11001011", "");


            AddInstruction(ref DDTable, InstructionCoding.Ret, new InstructionOperand[] { }, "11101001", "jp (ix)");        // treated as ret, since we don't know the value of HL

            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { }, "11100101", "push ix");
            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { }, "11100001", "pop ix");


            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "00100001", "ld ix,@@0@@");
            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8, InstructionOperand.Imm8 }, "00110110", "ld (ix+@@0@@),@@1@@");

            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { }, "00aa1001", "add ix,{0}", new string[][] { reg16_X });

            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "01110aaa", "ld (ix+@@0@@),{0}", new string[][] { reg8_nohl });
            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "01aaa110", "ld {0},(ix+@@0@@)", new string[][] { reg8_nohl });

            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { }, "00100011", "inc ix");
            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { }, "00101011", "dec ix");
            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { }, "00aaa100", "inc {0}", new string[][] { reg8_X });
            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { }, "00aaa101", "dec {0}", new string[][] { reg8_X });
            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "00110100", "inc (ix+@@0@@)");
            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "00110101", "dec (ix+@@0@@)");

            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { }, "10aaabbb", "{0}{1}", new string[][] { alu, reg8_X });
            AddInstruction(ref DDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "10aaa110", "{0}(ix+@@0@@)", new string[][] { alu });

            // DDCB
            AddInstruction(ref DDCBTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.XDCBByte }, "01aaa110", "bit {0},(ix+@@0@@)", new string[][] { bitIndex });
            AddInstruction(ref DDCBTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.XDCBByte }, "10aaa110", "res {0},(ix+@@0@@)", new string[][] { bitIndex });
            AddInstruction(ref DDCBTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.XDCBByte }, "11aaa110", "set {0},(ix+@@0@@)", new string[][] { bitIndex });

            // ED

            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "01000100", "neg");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "01000111", "ld i,a");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "01001101", "reti");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "01000101", "retn");

            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "01aa1010", "adc hl,{0}", new string[][] { reg16 });
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "01aa0010", "sbc hl,{0}", new string[][] { reg16 });

            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "01aa0011", "ld (@@0@@),{0}", new string[][] { reg16 });
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "01aa1011", "ld {0},(@@0@@)", new string[][] { reg16 });

            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "01aaa001", "out (c),{0}", new string[][] { reg8_nohl });
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "01110001", "out (c),0");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "01aaa000", "in {0},(c)", new string[][] { reg8_nohl });
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "01110000", "in f,(c)");

            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10100000", "ldi");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10110000", "ldir");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10101000", "ldd");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10111000", "lddr");

            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10100010", "ini");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10110010", "inir");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10101010", "ind");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10111010", "indr");

            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10100011", "outi");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10110011", "otir");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10101011", "outd");
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10111011", "otdr");

            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "010aa110", "im {0}", new string[][] { interruptModes });

            // FD
            AddInstruction(ref FDTable, InstructionCoding.Prefix, new InstructionOperand[] { }, "11001011", "");

            AddInstruction(ref FDTable, InstructionCoding.Ret, new InstructionOperand[] { }, "11101001", "jp (iy)");        // treated as ret, since we don't know the value of HL

            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { }, "11100101", "push iy");
            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { }, "11100001", "pop iy");

            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "00100001", "ld iy,@@0@@");
            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8, InstructionOperand.Imm8 }, "00110110", "ld (iy+@@0@@),@@1@@");

            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { }, "00aa1001", "add iy,{0}", new string[][] { reg16_Y });

            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "01110aaa", "ld (iy+@@0@@),{0}", new string[][] { reg8_nohl });
            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "01aaa110", "ld {0},(iy+@@0@@)", new string[][] { reg8_nohl });


            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { }, "00100011", "inc iy");
            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { }, "00101011", "dec iy");
            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { }, "00aaa100", "inc {0}", new string[][] { reg8_Y });
            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { }, "00aaa101", "dec {0}", new string[][] { reg8_Y });
            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "00110100", "inc (iy+@@0@@)");
            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "00110101", "dec (iy+@@0@@)");

            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { }, "10aaabbb", "{0}{1}", new string[][] { alu, reg8_Y });
            AddInstruction(ref FDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "10aaa110", "{0}(iy+@@0@@)", new string[][] { alu });

            // FDCB
            AddInstruction(ref FDCBTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.XDCBByte }, "01aaa110", "bit {0},(iy+@@0@@)", new string[][] { bitIndex });
            AddInstruction(ref FDCBTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.XDCBByte }, "10aaa110", "res {0},(iy+@@0@@)", new string[][] { bitIndex });
            AddInstruction(ref FDCBTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.XDCBByte }, "11aaa110", "set {0},(iy+@@0@@)", new string[][] { bitIndex });


            // Next (z80N) extended ops

            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "00100011", "swapnib");                                                                 //23
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "00100100", "mirror");                                                                  //24
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm8 }, "00100111", "test @@0@@");                                       //27
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "00101000", "bsla de,b");                                                               //28
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "00101001", "bsra de,b");                                                               //29
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "00101010", "bsrl de,b");                                                               //2A
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "00101011", "bsrf de,b");                                                               //2B
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "00101100", "brlc de,b");                                                               //2C
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "00110000", "mul");                                                                     //30
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "00110001", "add hl,a");                                                                //31
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "00110010", "add de,a");                                                                //32
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "00110011", "add bc,a");                                                                //33
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "00110100", "add hl,@@0@@");                                    //34
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "00110101", "add de,@@0@@");                                    //35
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16 }, "00110110", "add bc,@@0@@");                                    //36
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.Imm16HiLo }, "10001010", "push @@0@@");                                  //8A
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10010000", "outinb");                                                                  //90
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.HWReg8, InstructionOperand.Imm8 }, "10010001", "nextreg @@0@@,@@1@@");    //91
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { InstructionOperand.HWReg8 }, "10010010", "nextreg @@0@@,a");                                //92
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10010011", "pixeldn");                                                                 //93
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10010100", "pixelad");                                                                 //94
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10010101", "setae");                                                                   //95
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10011000", "jp (c)");                                                                  //98
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10100100", "ldix");                                                                    //A4
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10100101", "ldws");                                                                    //A5
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10101100", "lddx");                                                                    //AC
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10110100", "ldirx");                                                                   //B4
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10110111", "ldpirx");                                                                  //B7
            AddInstruction(ref EDTable, InstructionCoding.None, new InstructionOperand[] { }, "10111100", "lddrx");                                                                   //BC
        }

        protected UInt64 GetLength(MachineInfo m, UInt64 address, ref TableEntry[] cTable)
        {
            UInt64 prefixTempLength;
            byte opcode;
            if (m.FetchByte(address, out opcode))
            {
                if ((cTable[opcode].opType & InstructionCoding.Prefix) == InstructionCoding.Prefix)
                {
                    switch (opcode)
                    {
                        case 0xCB:
                            if (cTable == DDTable)
                            {
                                prefixTempLength = GetLength(m, address + 2, ref DDCBTable);
                                if (prefixTempLength == 0)
                                    return 0;
                                return 2 + prefixTempLength;
                            }
                            if (cTable == FDTable)
                            {
                                prefixTempLength = GetLength(m, address + 2, ref FDCBTable);
                                if (prefixTempLength == 0)
                                    return 0;
                                return 2 + prefixTempLength;
                            }
                            if (cTable == baseTable)
                            {
                                prefixTempLength = GetLength(m, address + 1, ref CBTable);
                                if (prefixTempLength == 0)
                                    return 0;
                                return 1 + prefixTempLength;
                            }
                            return 0;
                        case 0xDD:
                            prefixTempLength = GetLength(m, address + 1, ref DDTable);
                            if (prefixTempLength == 0)
                                return 0;
                            return 1 + prefixTempLength;
                        case 0xED:
                            prefixTempLength = GetLength(m, address + 1, ref EDTable);
                            if (prefixTempLength == 0)
                                return 0;
                            return 1 + prefixTempLength;
                        case 0xFD:
                            prefixTempLength = GetLength(m, address + 1, ref FDTable);
                            if (prefixTempLength == 0)
                                return 0;
                            return 1 + prefixTempLength;
                    }
                    return 0;
                }
                if (cTable[opcode].mnemonic != null)
                {
                    string mnemonic = cTable[opcode].mnemonic;
                    UInt64 length = 1;
                    for (int ops = 0; ops < cTable[opcode].operands.Length; ops++)
                    {
                        switch (cTable[opcode].operands[ops])
                        {
                            case InstructionOperand.Imm8:
                                length++;
                                break;
                            case InstructionOperand.Imm16:
                                length += 2;
                                break;
                            case InstructionOperand.Rel8:
                                length++;
                                break;
                        }
                    }
                    return length;
                }
            }
            return 0;
        }

        public UInt64 GetLength(MachineInfo m, UInt64 address)
        {
            return GetLength(m, address, ref baseTable);
        }


        protected UInt64 Disassemble(MachineInfo m, UInt64 address, out string mnemonic, ref TableEntry[] cTable)
        {
            byte opcode;
            UInt64 prefixTempLength = 0;
            mnemonic = "";

            if (m.FetchByte(address, out opcode))
            {
                if ((cTable[opcode].opType & InstructionCoding.Prefix) == InstructionCoding.Prefix)
                {
                    switch (opcode)
                    {
                        case 0xCB:
                            if (cTable == DDTable)
                            {
                                prefixTempLength = Disassemble(m, address + 2, out mnemonic, ref DDCBTable);
                                if (prefixTempLength == 0)
                                    return 0;
                                return 2 + prefixTempLength;
                            }
                            if (cTable == FDTable)
                            {
                                prefixTempLength = Disassemble(m, address + 2, out mnemonic, ref FDCBTable);
                                if (prefixTempLength == 0)
                                    return 0;
                                return 2 + prefixTempLength;
                            }
                            if (cTable == baseTable)
                            {
                                prefixTempLength = Disassemble(m, address + 1, out mnemonic, ref CBTable);
                                if (prefixTempLength == 0)
                                    return 0;
                                return 1 + prefixTempLength;
                            }
                            return 0;
                        case 0xDD:
                            prefixTempLength = Disassemble(m, address + 1, out mnemonic, ref DDTable);
                            if (prefixTempLength == 0)
                                return 0;
                            return 1 + prefixTempLength;
                        case 0xED:
                            prefixTempLength = Disassemble(m, address + 1, out mnemonic, ref EDTable);
                            if (prefixTempLength == 0)
                                return 0;
                            return 1 + prefixTempLength;
                        case 0xFD:
                            prefixTempLength = Disassemble(m, address + 1, out mnemonic, ref FDTable);
                            if (prefixTempLength == 0)
                                return 0;
                            return 1 + prefixTempLength;
                    }
                    return 0;
                }
                if (cTable[opcode].mnemonic != null)
                {
                    byte b;
                    UInt16 w;

                    mnemonic = cTable[opcode].mnemonic;
                    UInt64 length = 1;

                    for (int ops = 0; ops < cTable[opcode].operands.Length; ops++)
                    {
                        switch (cTable[opcode].operands[ops])
                        {
                            case InstructionOperand.XDCBByte:
                                if (m.FetchByte(address - 1, out b))
                                {
                                    mnemonic = mnemonic.Replace("@@" + ops + "@@", b.ToString("X2"));
                                }
                                break;      // no length adjustment as already counted
                            case InstructionOperand.Imm8:
                                if (m.FetchByte(address + length, out b))
                                {
                                    mnemonic = mnemonic.Replace("@@" + ops + "@@", b.ToString("X2"));
                                }
                                length++;
                                break;
                            case InstructionOperand.HWReg8:
                                if (m.FetchByte(address + length, out b))
                                {
                                    mnemonic = mnemonic.Replace("@@" + ops + "@@", b.ToString("X2"));
                                }
                                length++;
                                break;
                            case InstructionOperand.Imm16:
                                if (m.FetchWord(address + length, out w))
                                {
                                    mnemonic = mnemonic.Replace("@@" + ops + "@@", w.ToString("X4"));
                                }
                                length += 2;
                                break;
                            case InstructionOperand.Imm16HiLo:
                                if (m.FetchWord(address + length, out w))
                                {
                                    UInt16 swapped = (UInt16)(w >> 8);
                                    swapped |= (UInt16)((w & 255) << 8);
                                    mnemonic = mnemonic.Replace("@@" + ops + "@@", swapped.ToString("X4"));
                                }
                                length += 2;
                                break;
                            case InstructionOperand.Rel8:
                                if (m.FetchByte(address + length, out b))
                                {
                                    // Sign extend b to 16
                                    w = b;
                                    if ((w & 0x80) == 0x80)
                                    {
                                        w |= 0xFF00;
                                    }
                                    unchecked
                                    {
                                        w += Convert.ToUInt16((address + 2) & 0xFFFF);
                                    }
                                    mnemonic = mnemonic.Replace("@@" + ops + "@@", w.ToString("X4"));
                                }
                                length++;
                                break;
                        }
                    }
                    return length;
                }
            }
            return 0;
        }

        public UInt64 Disassemble(MachineInfo m, UInt64 address, out string mnemonic)
        {
            return Disassemble(m, address, out mnemonic, ref baseTable);
        }

        // this is all horrid.. rethink!
        protected UInt64[] GetNextAddresses(MachineInfo m, UInt64 address, ref TableEntry[] cTable, UInt64 nextAddress)
        {
            byte opcode;

            if (m.FetchByte(address, out opcode))
            {
                if (cTable[opcode].opType == InstructionCoding.Prefix)
                {
                    switch (opcode)
                    {
                        case 0xCB:
                            if (cTable == DDTable)
                            {
                                return GetNextAddresses(m, address + 2, ref DDCBTable, nextAddress);
                            }
                            if (cTable == FDTable)
                            {
                                return GetNextAddresses(m, address + 2, ref FDCBTable, nextAddress);
                            }
                            if (cTable == baseTable)
                            {
                                return GetNextAddresses(m, address + 1, ref CBTable, nextAddress);
                            }
                            return null;
                        case 0xDD:
                            return GetNextAddresses(m, address + 1, ref DDTable, nextAddress);
                        case 0xED:
                            return GetNextAddresses(m, address + 1, ref EDTable, nextAddress);
                        case 0xFD:
                            return GetNextAddresses(m, address + 1, ref FDTable, nextAddress);
                    }
                    return null;
                }
                if (cTable[opcode].mnemonic != null)
                {
                    if ((cTable[opcode].opType & InstructionCoding.Conditional) == InstructionCoding.Conditional || (cTable[opcode].opType & InstructionCoding.Call) == InstructionCoding.Call)
                    {
                        byte b;
                        UInt16 w;

                        if ((cTable[opcode].opType & InstructionCoding.Ret) == InstructionCoding.Ret)
                        {
                            return new UInt64[1] { nextAddress };
                        }

                        switch (cTable[opcode].operands[0])
                        {
                            default:
                                return null;
                            case InstructionOperand.Rel8:
                                if (m.FetchByte(address + 1, out b))
                                {
                                    // Sign extend b to 16
                                    w = b;
                                    if ((w & 0x80) == 0x80)
                                    {
                                        w |= 0xFF00;
                                    }
                                    unchecked
                                    {
                                        w += Convert.ToUInt16((address + 2) & 0xFFFF);
                                    }
                                    return new UInt64[2] { w, nextAddress };
                                }
                                return null;
                            case InstructionOperand.Imm16:
                                if (m.FetchWord(address + 1, out w))
                                {
                                    return new UInt64[2] { w, nextAddress };
                                }
                                return null;
                        }
                    }
                    if ((cTable[opcode].opType & InstructionCoding.Ret) == InstructionCoding.Ret)
                    {
                        return null;
                    }
                    if ((cTable[opcode].opType & InstructionCoding.Rst) == InstructionCoding.Rst)
                    {
                        // TODO - spectrum has special use for RST 8 (error code byte follows) and RST 28 (calc stack follows)
                        //      -for now rst is treated as a single target destination (terminate at this instruction)
                        UInt64 rstAddress = opcode;
                        rstAddress &= 0x38;

                        //						if (rstAddress!=0x08 && rstAddress!=0x28)		// HACK - need a way for a machine to overide the analysis behaviour
                        {
                            return new UInt64[2] { rstAddress, nextAddress };
                        }
                        /*
                                                Console.WriteLine("RST : "+rstAddress.ToString("x8")+" "+nextAddress.ToString("x8"));

                                                if (rstAddress==0x08)
                                                {
                                                    return new UInt64[1]{rstAddress};		// no continuation after RST8
                                                }

                                                if (rstAddress==0x28)
                                                {
                                                    byte b;
                                                    UInt64 probeForEnd=nextAddress;
                                                    while (true)
                                                    {
                                                        if (!m.FetchByte(probeForEnd,out b))
                                                        {
                                                            Console.WriteLine("Failed to locate end of calculator stack!\n");
                                                            return new UInt64[1]{rstAddress};
                                                        }
                                                        probeForEnd++;
                                                        if (b==0x38)
                                                        {
                                                            break;
                                                        }
                                                    }

                                                    return new UInt64[2]{rstAddress,probeForEnd};
                                                }
                                                return new UInt64[1]{rstAddress};*/
                    }
                    if ((cTable[opcode].opType & InstructionCoding.Jmp) == InstructionCoding.Jmp)
                    {
                        byte b;
                        UInt16 w;
                        switch (cTable[opcode].operands[0])
                        {
                            default:
                                return null;
                            case InstructionOperand.Imm16:
                                if (m.FetchWord(address + 1, out w))
                                {
                                    return new UInt64[1] { w };
                                }
                                return null;
                            case InstructionOperand.Rel8:
                                if (m.FetchByte(address + 1, out b))
                                {
                                    // Sign extend b to 16
                                    w = b;
                                    if ((w & 0x80) == 0x80)
                                    {
                                        w |= 0xFF00;
                                    }
                                    unchecked
                                    {
                                        w += Convert.ToUInt16((address + 2) & 0xFFFF);
                                    }
                                    return new UInt64[1] { w };
                                }
                                return null;
                        }
                    }
                    return new UInt64[1] { nextAddress };
                }
            }
            return null;
        }

        public UInt64[] GetNextAddresses(MachineInfo m, UInt64 address)
        {
            UInt64 nextAddress = address + GetLength(m, address);
            if (nextAddress == address)
                return null;
            return GetNextAddresses(m, address, ref baseTable, nextAddress);
        }

        void AddInstruction(ref TableEntry[] table, InstructionCoding coding, InstructionOperand[] operands, string bits, string mnemonic, string[][] mappings = null)
        {
            byte opcode = 0;
            byte mask = 0;
            byte imask = 0;
            int mCnt = 0;
            for (int a = 0; a < 8; a++)
            {
                opcode <<= 1;
                mask <<= 1;
                switch (bits[a])
                {
                    case '1':
                        opcode |= 1;
                        break;
                    case 'a':
                    case 'b':
                        mask |= 1;
                        mCnt++;
                        break;
                }
            }
            imask = (byte)((~mask) & 255);
            mCnt = 1 << mCnt;

            byte incremental = 0;
            for (int b = 0; b < mCnt; b++)
            {
                byte lastIncremental = incremental;
                opcode &= imask;
                opcode |= (byte)(incremental & mask);

                // Need to create constants for mappings
                if (mappings == null || mappings.Length == 0)
                {
                    table[opcode].mnemonic = mnemonic;
                    table[opcode].operands = operands;
                    table[opcode].opType = coding;
                    break;  // we are done
                }
                else
                {
                    string[] str = new string[mappings.Length];
                    bool valid = true;
                    for (int c = 0; c < mappings.Length; c++)
                    {
                        int o = 0;
                        int val = 0;
                        for (int d = 0; d < 8; d++)
                        {
                            if (bits[7 - d] == 'a' + c)
                            {
                                val |= ((incremental >> d) & 1) << o;
                                o++;
                            }
                        }
                        str[c] = mappings[c][val];
                        valid &= (str[c] != null);
                    }

                    if (valid)
                    {
                        table[opcode].mnemonic = string.Format(mnemonic, str);
                        table[opcode].operands = operands;
                        table[opcode].opType = coding;
                    }
                }

                while (mask != 0 && (lastIncremental & mask) == (incremental & mask))
                {
                    incremental++;
                }
            }
        }
    }
}

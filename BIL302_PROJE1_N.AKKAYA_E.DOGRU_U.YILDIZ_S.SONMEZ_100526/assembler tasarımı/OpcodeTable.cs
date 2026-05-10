using System;
using System.Collections.Generic;

namespace PicoRV_Assembler
{
    public class InstructionInfo
    {
        public char Type { get; set; }
        public byte Opcode { get; set; }
        public byte Funct3 { get; set; }
        public byte Funct7 { get; set; }

        public InstructionInfo(char type, byte opcode, byte funct3, byte funct7 = 0)
        {
            Type = type;
            Opcode = opcode;
            Funct3 = funct3;
            Funct7 = funct7;
        }
    }

    public static class OpcodeTable
    {
        // Modern C# Dictionary Tanımlaması (CS1003 Hatasını Çözer)
        public static readonly Dictionary<string, InstructionInfo> Instructions = new Dictionary<string, InstructionInfo>
        {
            ["add"] = new InstructionInfo('R', 0x33, 0x0, 0x00),
            ["sub"] = new InstructionInfo('R', 0x33, 0x0, 0x20),
            ["and"] = new InstructionInfo('R', 0x33, 0x7, 0x00),
            ["or"] = new InstructionInfo('R', 0x33, 0x6, 0x00),

            ["addi"] = new InstructionInfo('I', 0x13, 0x0),
            ["lw"] = new InstructionInfo('I', 0x03, 0x2),

            ["sw"] = new InstructionInfo('S', 0x23, 0x2),

            ["beq"] = new InstructionInfo('B', 0x63, 0x0),
            ["bne"] = new InstructionInfo('B', 0x63, 0x1)
        };

        public static readonly Dictionary<string, int> Registers = new Dictionary<string, int>
        {
            ["x0"] = 0,
            ["zero"] = 0,
            ["x1"] = 1,
            ["ra"] = 1,
            ["x2"] = 2,
            ["sp"] = 2,
            ["x3"] = 3,
            ["gp"] = 3,
            ["x4"] = 4,
            ["tp"] = 4,
            ["x5"] = 5,
            ["t0"] = 5
        };
    }
}
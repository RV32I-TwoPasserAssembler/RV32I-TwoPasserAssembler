using System.Collections.Generic;

namespace PicoRV_Assembler
{
    public class InstructionInfo
    {
        public char Type { get; set; }
        public uint Opcode { get; set; }
        public uint Funct3 { get; set; }
        public uint Funct7 { get; set; }

        public InstructionInfo(char type, uint opcode, uint funct3 = 0, uint funct7 = 0)
        {
            Type = type;
            Opcode = opcode;
            Funct3 = funct3;
            Funct7 = funct7;
        }
    }

    public static class OpcodeTable
    {
        public static readonly Dictionary<string, int> Registers = new Dictionary<string, int>
        {
            {"x0", 0},   {"x1", 1},   {"x2", 2},   {"x3", 3},
            {"x4", 4},   {"x5", 5},   {"x6", 6},   {"x7", 7},
            {"x8", 8},   {"x9", 9},   {"x10", 10}, {"x11", 11},
            {"x12", 12}, {"x13", 13}, {"x14", 14}, {"x15", 15},
            {"x16", 16}, {"x17", 17}, {"x18", 18}, {"x19", 19},
            {"x20", 20}, {"x21", 21}, {"x22", 22}, {"x23", 23},
            {"x24", 24}, {"x25", 25}, {"x26", 26}, {"x27", 27},
            {"x28", 28}, {"x29", 29}, {"x30", 30}, {"x31", 31}
        };

        public static readonly Dictionary<string, InstructionInfo> Instructions = new Dictionary<string, InstructionInfo>
        {
            {"add",  new InstructionInfo('R', 0x33, 0x0, 0x00)},
            {"addi", new InstructionInfo('I', 0x13, 0x0)},
            {"beq",  new InstructionInfo('B', 0x63, 0x0)},
            {"bne",  new InstructionInfo('B', 0x63, 0x1)},
            {"sw",   new InstructionInfo('S', 0x23, 0x2)},
            {"jal",  new InstructionInfo('J', 0x6F)}, // YENİ EKLENEN JAL KOMUTU
            {"jalr", new InstructionInfo('I', 0x67, 0x0)},
            {"lw",   new InstructionInfo('I', 0x03, 0x2)}, // Load Word (Bellekten Register'a 32-bit okuma)
            {"lbu",  new InstructionInfo('I', 0x03, 0x4)}, // Load Byte Unsigned (UART'tan 8-bit okuma için ideal)
            {"sb",   new InstructionInfo('S', 0x23, 0x0)}, // Store Byte (Belleğe 8-bit yazma)
         
        };
    }
}
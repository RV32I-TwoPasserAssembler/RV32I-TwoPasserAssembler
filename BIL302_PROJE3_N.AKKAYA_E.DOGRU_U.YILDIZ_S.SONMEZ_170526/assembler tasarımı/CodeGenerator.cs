#nullable disable
using System;
using System.Collections.Generic;

namespace PicoRV_Assembler
{
    public class CodeGenerator
    {
        private int GetRegisterNumber(string regName)
        {
            if (OpcodeTable.Registers.TryGetValue(regName, out int regNum))
            {
                return regNum;
            }
            throw new Exception($"HATA: Geçersiz register ismi '{regName}'");
        }

        public uint GenerateInstruction(ParsedLine parsed, int currentAddress, ObjectFile objFile, SymbolTable symbolTable)
        {
            if (!OpcodeTable.Instructions.TryGetValue(parsed.Mnemonic, out InstructionInfo info))
            {
                throw new Exception($"HATA: Bilinmeyen komut '{parsed.Mnemonic}'. Lütfen OpcodeTable'da tanımlı olduğundan emin olun.");
            }

            switch (info.Type)
            {
                case 'R': return GenerateRType(info, parsed.Operands);
                case 'I': return GenerateIType(info, parsed.Operands);
                case 'S': return GenerateSType(info, parsed.Operands);
                case 'B': return GenerateBType(info, parsed.Operands, currentAddress, objFile, symbolTable);
                case 'J': return GenerateJType(info, parsed.Operands, currentAddress, objFile, symbolTable); // YENİ J-TYPE
                default: throw new Exception($"HATA: Desteklenmeyen komut tipi '{info.Type}'");
            }
        }

        private uint GenerateRType(InstructionInfo info, List<string> operands)
        {
            uint rd = (uint)GetRegisterNumber(operands[0]);
            uint rs1 = (uint)GetRegisterNumber(operands[1]);
            uint rs2 = (uint)GetRegisterNumber(operands[2]);
            uint f7 = info.Funct7;
            uint f3 = info.Funct3;
            uint op = info.Opcode;
            return (f7 << 25) | (rs2 << 20) | (rs1 << 15) | (f3 << 12) | (rd << 7) | op;
        }

        private uint GenerateIType(InstructionInfo info, List<string> operands)
        {
            uint rd = (uint)GetRegisterNumber(operands[0]);
            uint rs1 = (uint)GetRegisterNumber(operands[1]);
            int immValue = operands[2].StartsWith("0x") ? Convert.ToInt32(operands[2], 16) : Convert.ToInt32(operands[2]);
            uint imm12 = (uint)(immValue & 0xFFF);
            uint f3 = info.Funct3;
            uint op = info.Opcode;
            return (imm12 << 20) | (rs1 << 15) | (f3 << 12) | (rd << 7) | op;
        }

        private uint GenerateSType(InstructionInfo info, List<string> operands)
        {
            uint rs2 = (uint)GetRegisterNumber(operands[0]);
            uint rs1 = (uint)GetRegisterNumber(operands[1]);
            
            int immValue = operands[2].StartsWith("0x") ? Convert.ToInt32(operands[2], 16) : Convert.ToInt32(operands[2]);
            uint imm = (uint)(immValue & 0xFFF);
            uint imm4_0 = imm & 0x1F;
            uint imm11_5 = (imm >> 5) & 0x7F;
            uint f3 = info.Funct3;
            uint op = info.Opcode;
            
            return (imm11_5 << 25) | (rs2 << 20) | (rs1 << 15) | (f3 << 12) | (imm4_0 << 7) | op;
        }

        private uint GenerateBType(InstructionInfo info, List<string> operands, int currentAddress, ObjectFile objFile, SymbolTable symbolTable)
        {
            uint rs1 = (uint)GetRegisterNumber(operands[0]);
            uint rs2 = (uint)GetRegisterNumber(operands[1]);
            string label = operands[2];
            
            int targetAddress = 0;

            if (objFile.ExternalSymbols.Contains(label))
            {
                int refId = -1;
                foreach (var kvp in objFile.ReferenceTable)
                {
                    if (kvp.Value == label) { refId = kvp.Key; break; }
                }

                if (refId == -1)
                {
                    refId = objFile.ReferenceTable.Count + 1;
                    objFile.ReferenceTable[refId] = label;
                }

                objFile.RelocationTable.Add(new RelocationEntry 
                { 
                    Address = currentAddress, 
                    TargetRefId = refId,
                    InstructionType = 'B',
                    Operation = '+'
                });
                
                targetAddress = currentAddress; 
            }
            else
            {
                targetAddress = symbolTable.GetAddress(label);
            }

            int offset = targetAddress - currentAddress;
            uint imm = (uint)(offset & 0x1FFF);

            uint imm11 = (imm >> 11) & 0x1;
            uint imm4_1 = (imm >> 1) & 0xF;
            uint imm10_5 = (imm >> 5) & 0x3F;
            uint imm12 = (imm >> 12) & 0x1;
            uint f3 = info.Funct3;
            uint op = info.Opcode;
            
            return (imm12 << 31) | (imm10_5 << 25) | (rs2 << 20) | (rs1 << 15) | (f3 << 12) | (imm4_1 << 8) | (imm11 << 7) | op;
        }

        // YENİ EKLENEN J-TYPE ÜRETİCİ FONKSİYONU
        private uint GenerateJType(InstructionInfo info, List<string> operands, int currentAddress, ObjectFile objFile, SymbolTable symbolTable)
        {
            uint rd = (uint)GetRegisterNumber(operands[0]);
            string label = operands[1];
            int targetAddress = 0;

            if (objFile.ExternalSymbols.Contains(label))
            {
                int refId = -1;
                foreach (var kvp in objFile.ReferenceTable)
                {
                    if (kvp.Value == label) { refId = kvp.Key; break; }
                }

                if (refId == -1)
                {
                    refId = objFile.ReferenceTable.Count + 1;
                    objFile.ReferenceTable[refId] = label;
                }

                objFile.RelocationTable.Add(new RelocationEntry 
                { 
                    Address = currentAddress, 
                    TargetRefId = refId, 
                    InstructionType = 'J', 
                    Operation = '+' 
                });
                targetAddress = currentAddress; 
            }
            else
            {
                targetAddress = symbolTable.GetAddress(label);
            }

            int offset = targetAddress - currentAddress;
            uint imm = (uint)(offset & 0x1FFFFF); // 21-bit maske

            uint imm20 = (imm >> 20) & 0x1;
            uint imm10_1 = (imm >> 1) & 0x3FF;
            uint imm11 = (imm >> 11) & 0x1;
            uint imm19_12 = (imm >> 12) & 0xFF;
            uint op = info.Opcode;
            
            return (imm20 << 31) | (imm10_1 << 21) | (imm11 << 20) | (imm19_12 << 12) | (rd << 7) | op;
        }
    }
}
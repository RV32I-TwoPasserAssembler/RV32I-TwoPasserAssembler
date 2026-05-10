using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace PicoRV_Assembler
{
    public class Linker
    {
        public List<string> Link(List<ObjectFile> inputFiles, int textBase = 0x0000, int dataBase = 0x1000)
        {
            int currentTextAddress = textBase;
            int currentDataAddress = dataBase;

            Dictionary<string, int> globalSymbolTable = new Dictionary<string, int>();
            Dictionary<int, uint> memoryMap = new Dictionary<int, uint>();
            List<(RelocationEntry entry, int baseAddress, ObjectFile sourceFile)> allRelocations = new List<(RelocationEntry, int, ObjectFile)>();

            // =================================================================
            // 1. AŞAMA: BİRLEŞTİRME VE ADRES KAYDIRMA (MERGE & SHIFT)
            // =================================================================
            foreach (var file in inputFiles)
            {
                int maxTextAddress = 0;

                foreach (var hexLine in file.TextSegment)
                {
                    string[] parts = hexLine.Split(':');
                    int localAddr = Convert.ToInt32(parts[0].Trim().Replace("0x", ""), 16);
                    string machineCodeHex = parts[1].Trim().Substring(2, 8);
                    uint machineCode = Convert.ToUInt32(machineCodeHex, 16);

                    memoryMap.Add(currentTextAddress + localAddr, machineCode);
                    if (localAddr + 4 > maxTextAddress) maxTextAddress = localAddr + 4;
                }

                int maxDataAddress = 0;

                foreach (var hexLine in file.DataSegment)
                {
                    string[] parts = hexLine.Split(':');
                    int localAddr = Convert.ToInt32(parts[0].Trim().Replace("0x", ""), 16);
                    string machineCodeHex = parts[1].Trim().Substring(2, 8);
                    uint machineCode = Convert.ToUInt32(machineCodeHex, 16);

                    memoryMap.Add(currentDataAddress + localAddr, machineCode);
                    if (localAddr + 4 > maxDataAddress) maxDataAddress = localAddr + 4;
                }

                foreach (var globalSymName in file.GlobalSymbols)
                {
                    // UYARI GİDERİCİ: 'out SymbolData?' ile CS8600 uyarısı çözüldü
                    if (file.SymbolTable.TryGetValue(globalSymName, out SymbolData? symData))
                    {
                        if (globalSymbolTable.ContainsKey(globalSymName))
                        {
                            throw new Exception($"LİNK HATASI: '{globalSymName}' etiketi birden fazla dosyada GLOBAL olarak tanımlanmış!");
                        }

                        int absoluteAddress = symData.Segment == "Text"
                            ? symData.Address + currentTextAddress
                            : symData.Address + currentDataAddress;

                        globalSymbolTable.Add(globalSymName, absoluteAddress);
                    }
                    else
                    {
                        throw new Exception($"ASSEMBLY HATASI: '{globalSymName}' global olarak işaretlendi ancak kod içinde bulunamadı!");
                    }
                }

                foreach (var rel in file.RelocationTable)
                {
                    allRelocations.Add((rel, currentTextAddress, file));
                }

                currentTextAddress += maxTextAddress;
                currentDataAddress += maxDataAddress;
            }

            Console.WriteLine("\n--- ESTAB (Global Symbol / Harici Sembol Tablosu) ---");
            Console.WriteLine("Sembol Adı\t\t|\tAdres (Hex)");
            Console.WriteLine("-------------------------------------------------");
            foreach (var sym in globalSymbolTable)
            {
                Console.WriteLine($"{sym.Key,-23} |\t0x{sym.Value:X4}");
            }
            Console.WriteLine("-------------------------------------------------\n");

            // =================================================================
            // 2. AŞAMA: BAĞLAMA VE YAMA YAPMA (RELOCATION ARİTMETİĞİ)
            // =================================================================
            foreach (var relItem in allRelocations)
            {
                RelocationEntry rel = relItem.entry;
                int fileBaseAddress = relItem.baseAddress;
                ObjectFile sourceFile = relItem.sourceFile;

                // Data segmentindeki yamalar textBase'e değil, kendi base adreslerine göre çözümlenmelidir
                int instructionAddress = rel.InstructionType == 'D' ? sourceFile.DataSegment.Count > 0 ? currentDataAddress - (sourceFile.DataSegment.Count * 4) + rel.Address : rel.Address
                                                                    : fileBaseAddress + rel.Address;

                if (!sourceFile.ReferenceTable.ContainsKey(rel.TargetRefId))
                {
                    throw new Exception($"LİNK HATASI: Dosyada {rel.TargetRefId} numaralı referans kimliği bulunamadı!");
                }
                string targetLabel = sourceFile.ReferenceTable[rel.TargetRefId];

                if (!globalSymbolTable.ContainsKey(targetLabel))
                {
                    throw new Exception($"LİNK HATASI: '{targetLabel}' etiketi hiçbir nesne (.o) dosyasında tanımlanmamış!");
                }

                int targetAddress = globalSymbolTable[targetLabel];
                uint oldInstruction = memoryMap[instructionAddress];
                uint newInstruction = oldInstruction;

                if (rel.InstructionType == 'B')
                {
                    int currentOffset = DecodeBTypeOffset(oldInstruction);
                    int newOffset;

                    if (rel.Operation == '+') newOffset = currentOffset + (targetAddress - instructionAddress);
                    else if (rel.Operation == '-') newOffset = currentOffset - (targetAddress - instructionAddress);
                    else throw new Exception($"LİNK HATASI: Geçersiz modifikasyon işlemi '{rel.Operation}'");

                    newInstruction = EncodeBTypeOffset(oldInstruction, newOffset);
                }
                else if (rel.InstructionType == 'J')
                {
                    int currentOffset = DecodeJTypeOffset(oldInstruction);
                    int newOffset;

                    if (rel.Operation == '+') newOffset = currentOffset + (targetAddress - instructionAddress);
                    else if (rel.Operation == '-') newOffset = currentOffset - (targetAddress - instructionAddress);
                    else throw new Exception($"LİNK HATASI: Geçersiz modifikasyon işlemi '{rel.Operation}'");

                    newInstruction = EncodeJTypeOffset(oldInstruction, newOffset);
                }
                // YENİ EKLENEN 'D' TİPİ (DATA / ABSOLUTE RELOCATION)
                else if (rel.InstructionType == 'D')
                {
                    int currentVal = (int)oldInstruction;
                    int newVal;

                    if (rel.Operation == '+') newVal = currentVal + targetAddress;
                    else if (rel.Operation == '-') newVal = currentVal - targetAddress;
                    else throw new Exception($"LİNK HATASI: Geçersiz modifikasyon işlemi '{rel.Operation}'");

                    newInstruction = (uint)newVal;
                }

                memoryMap[instructionAddress] = newInstruction;
            }

            // =================================================================
            // YÜRÜTME (EXECADDR) VE MAP DOSYASI OLUŞTURMA
            // =================================================================
            int execAddr = textBase;

            if (globalSymbolTable.ContainsKey("start")) execAddr = globalSymbolTable["start"];
            else if (globalSymbolTable.ContainsKey("main")) execAddr = globalSymbolTable["main"];

            Console.WriteLine($"[BİLGİ] Yürütme Başlangıç Adresi (EXECADDR): 0x{execAddr:X4}");

            List<string> mapContent = new List<string>
            {
                "=================================================",
                "              PicoRV LİNK MAP DOSYASI            ",
                "=================================================",
                $"EXECADDR (Transfer Adresi)      : 0x{execAddr:X4}",
                $"TEXT Bölümü Başlangıcı (ROM)    : 0x{textBase:X4}",
                $"DATA Bölümü Başlangıcı (RAM)    : 0x{dataBase:X4}",
                "-------------------------------------------------",
                "ESTAB (Global Sembol Tablosu):",
                "-------------------------------------------------"
            };

            foreach (var sym in globalSymbolTable)
            {
                mapContent.Add($"{sym.Key,-25} : 0x{sym.Value:X4}");
            }

            mapContent.Add("=================================================");
            File.WriteAllLines("program.map", mapContent);
            Console.WriteLine("[BİLGİ] Bellek haritası (Memory Map) 'program.map' dosyasına kaydedildi.");

            // =================================================================
            // 3. AŞAMA: ÇIKTIYI ÜRET (HEX FORMATI - FPGA UYUMLU)
            // =================================================================
            List<string> finalOutput = new List<string>();
            int expectedWordAddress = textBase / 4;

            foreach (var kvp in memoryMap.OrderBy(k => k.Key))
            {
                int currentWordAddress = kvp.Key / 4;

                if (currentWordAddress != expectedWordAddress)
                {
                    finalOutput.Add($"@{currentWordAddress:X}");
                }

                finalOutput.Add(kvp.Value.ToString("X8"));
                expectedWordAddress = currentWordAddress + 1;
            }

            return finalOutput;
        }

        private int DecodeBTypeOffset(uint instruction)
        {
            uint imm12 = (instruction >> 31) & 0x1;
            uint imm10_5 = (instruction >> 25) & 0x3F;
            uint imm4_1 = (instruction >> 8) & 0xF;
            uint imm11 = (instruction >> 7) & 0x1;
            uint imm = (imm12 << 12) | (imm11 << 11) | (imm10_5 << 5) | (imm4_1 << 1);
            if ((imm & 0x1000) != 0) imm |= 0xFFFFE000;
            return (int)imm;
        }

        private uint EncodeBTypeOffset(uint instruction, int offset)
        {
            uint imm = (uint)(offset & 0x1FFF);
            uint imm11 = (imm >> 11) & 0x1;
            uint imm4_1 = (imm >> 1) & 0xF;
            uint imm10_5 = (imm >> 5) & 0x3F;
            uint imm12 = (imm >> 12) & 0x1;
            uint cleanInstruction = instruction & ~((1u << 31) | (0x3Fu << 25) | (0xFu << 8) | (1u << 7));
            uint patchBits = (imm12 << 31) | (imm10_5 << 25) | (imm4_1 << 8) | (imm11 << 7);
            return cleanInstruction | patchBits;
        }

        private int DecodeJTypeOffset(uint instruction)
        {
            uint imm20 = (instruction >> 31) & 0x1;
            uint imm10_1 = (instruction >> 21) & 0x3FF;
            uint imm11 = (instruction >> 20) & 0x1;
            uint imm19_12 = (instruction >> 12) & 0xFF;
            uint imm = (imm20 << 20) | (imm19_12 << 12) | (imm11 << 11) | (imm10_1 << 1);
            if ((imm & 0x100000) != 0) imm |= 0xFFE00000;
            return (int)imm;
        }

        private uint EncodeJTypeOffset(uint instruction, int offset)
        {
            uint imm = (uint)(offset & 0x1FFFFF);
            uint imm20 = (imm >> 20) & 0x1;
            uint imm10_1 = (imm >> 1) & 0x3FF;
            uint imm11 = (imm >> 11) & 0x1;
            uint imm19_12 = (imm >> 12) & 0xFF;
            uint cleanInstruction = instruction & 0xFFF;
            uint patchBits = (imm20 << 31) | (imm10_1 << 21) | (imm11 << 20) | (imm19_12 << 12);
            return cleanInstruction | patchBits;
        }
    }
}
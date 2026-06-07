using System;
using System.Collections.Generic;

namespace PicoRV_Assembler
{
    public enum SegmentType
    {
        Text,
        Data
    }

    public class AssemblerMotor
    {
        private Parser parser;
        private CodeGenerator codeGen;
        public SymbolTable SymTable { get; private set; }

        public AssemblerMotor()
        {
            parser = new Parser();
            codeGen = new CodeGenerator();
            SymTable = new SymbolTable();
        }

        public ObjectFile Assemble(string[] sourceLines, string fileName)
        {
            ObjectFile outputObject = new ObjectFile { FileName = fileName };
            List<ParsedLine> parsedLines = new List<ParsedLine>();

            // --- BİRİNCİ GEÇİŞ ---
            int currentTextAddress = 0;
            int currentDataAddress = 0;
            SegmentType currentSegment = SegmentType.Text;

            foreach (string line in sourceLines)
            {
                ParsedLine parsed = parser.ParseLine(line);
                parsedLines.Add(parsed);

                if (parsed.Type == "Empty") continue;

                if (parsed.Type == "Directive")
                {
                    if (parsed.Mnemonic == ".text") { currentSegment = SegmentType.Text; continue; }
                    else if (parsed.Mnemonic == ".data") { currentSegment = SegmentType.Data; continue; }
                    else if (parsed.Mnemonic == ".org" && parsed.Operands.Count > 0)
                    {
                        int orgVal = parsed.Operands[0].StartsWith("0x") ? Convert.ToInt32(parsed.Operands[0], 16) : Convert.ToInt32(parsed.Operands[0]);
                        if (currentSegment == SegmentType.Text) currentTextAddress = orgVal;
                        else currentDataAddress = orgVal;
                    }
                    else if (parsed.Mnemonic == ".extern" && parsed.Operands.Count > 0)
                    {
                        outputObject.ExternalSymbols.Add(parsed.Operands[0]);
                    }
                    else if (parsed.Mnemonic == ".global" && parsed.Operands.Count > 0)
                    {
                        outputObject.GlobalSymbols.Add(parsed.Operands[0]);
                    }
                    else if (parsed.Mnemonic == ".word" && parsed.Operands.Count > 0)
                    {
                        if (currentSegment == SegmentType.Text) currentTextAddress += 4;
                        else currentDataAddress += 4;
                    }
                }
                else if (parsed.Type == "Label")
                {
                    int addr = currentSegment == SegmentType.Text ? currentTextAddress : currentDataAddress;
                    SymTable.AddSymbol(parsed.LabelName, addr, currentSegment.ToString());
                }
                else if (parsed.Type == "Instruction")
                {
                    if (currentSegment == SegmentType.Text) currentTextAddress += 4;
                    else currentDataAddress += 4;
                }
            }

            // --- İKİNCİ GEÇİŞ ---
            currentTextAddress = 0;
            currentDataAddress = 0;
            currentSegment = SegmentType.Text;

            foreach (ParsedLine parsed in parsedLines)
            {
                if (parsed.Type == "Directive")
                {
                    if (parsed.Mnemonic == ".text") { currentSegment = SegmentType.Text; }
                    else if (parsed.Mnemonic == ".data") { currentSegment = SegmentType.Data; }
                    else if (parsed.Mnemonic == ".org" && parsed.Operands.Count > 0)
                    {
                        int orgVal = parsed.Operands[0].StartsWith("0x") ? Convert.ToInt32(parsed.Operands[0], 16) : Convert.ToInt32(parsed.Operands[0]);
                        if (currentSegment == SegmentType.Text) currentTextAddress = orgVal;
                        else currentDataAddress = orgVal;
                    }
                    else if (parsed.Mnemonic == ".word" && parsed.Operands.Count > 0)
                    {
                        string operand = parsed.Operands[0];
                        int val = 0;

                        // 1. Durum: Hexadecimal sayı kontrolü
                        if (operand.StartsWith("0x") && int.TryParse(operand.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out int hexVal))
                        {
                            val = hexVal;
                        }
                        // 2. Durum: Normal (Desimal) sayı kontrolü
                        else if (int.TryParse(operand, out int normalVal))
                        {
                            val = normalVal;
                        }
                        // 3. Durum: Sembol (Etiket) ise Linker'a D tipi yama talimatı bırak
                        else
                        {
                            val = 0;
                            int currentAddr = currentSegment == SegmentType.Text ? currentTextAddress : currentDataAddress;

                            int refId = -1;
                            foreach (var kvp in outputObject.ReferenceTable) { if (kvp.Value == operand) { refId = kvp.Key; break; } }
                            if (refId == -1) { refId = outputObject.ReferenceTable.Count + 1; outputObject.ReferenceTable[refId] = operand; }

                            outputObject.RelocationTable.Add(new RelocationEntry
                            {
                                Address = currentAddr,
                                TargetRefId = refId,
                                InstructionType = 'D', // Data Relocation
                                Operation = '+'
                            });
                        }

                        string hexCode = ((uint)val).ToString("X8");
                        int outAddr = currentSegment == SegmentType.Text ? currentTextAddress : currentDataAddress;
                        string outputLine = $"0x{outAddr:X4} : 0x{hexCode}    # {parsed.OriginalLine.Trim()}";

                        if (currentSegment == SegmentType.Data) outputObject.DataSegment.Add(outputLine);
                        else outputObject.TextSegment.Add(outputLine);

                        if (currentSegment == SegmentType.Text) currentTextAddress += 4;
                        else currentDataAddress += 4;
                    }
                }
                else if (parsed.Type == "Instruction")
                {
                    int currentAddr = currentSegment == SegmentType.Text ? currentTextAddress : currentDataAddress;
                    try
                    {
                        uint machineCode = codeGen.GenerateInstruction(parsed, currentAddr, outputObject, SymTable);
                        string hexCode = machineCode.ToString("X8");
                        string outputLine = $"0x{currentAddr:X4} : 0x{hexCode}    # {parsed.OriginalLine.Trim()}";

                        if (currentSegment == SegmentType.Text) outputObject.TextSegment.Add(outputLine);
                        else outputObject.DataSegment.Add(outputLine);

                        if (currentSegment == SegmentType.Text) currentTextAddress += 4;
                        else currentDataAddress += 4;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DERLEME HATASI (Adres 0x{currentAddr:X4}): {ex.Message} -> Satır: {parsed.OriginalLine}");
                    }
                }
            }

            return outputObject;
        }
    }
}
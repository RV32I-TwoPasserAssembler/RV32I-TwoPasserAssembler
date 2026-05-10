using System;
using System.Collections.Generic;

namespace PicoRV_Assembler
{
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

        // Dışarıdan okunan tüm satırları alıp makine kodu listesi döndüren ana metot
        public List<string> Assemble(string[] sourceLines)
        {
            List<string> machineCodeOutput = new List<string>();
            List<ParsedLine> parsedLines = new List<ParsedLine>();

            // =================================================================
            // BİRİNCİ GEÇİŞ (FIRST PASS): Etiketleri ve Adresleri Bul
            // Amaç: Sembol tablosunu oluşturmak ve bellek düzenini belirlemek
            // =================================================================
            int currentAddress = 0;

            foreach (string line in sourceLines)
            {
                ParsedLine parsed = parser.ParseLine(line);
                parsedLines.Add(parsed);

                if (parsed.Type == "Empty") continue;

                if (parsed.Type == "Directive")
                {
                    if (parsed.Mnemonic == ".org" && parsed.Operands.Count > 0)
                    {
                        // .org direktifi gelirse bellek adresini (Program Counter) ayarla
                        currentAddress = parsed.Operands[0].StartsWith("0x")
                            ? Convert.ToInt32(parsed.Operands[0], 16)
                            : Convert.ToInt32(parsed.Operands[0]);
                    }
                }
                else if (parsed.Type == "Label")
                {
                    // Etiketi (örn: "loop:") ve o anki adresi Sembol Tablosuna kaydet
                    SymTable.AddSymbol(parsed.LabelName, currentAddress);
                }
                else if (parsed.Type == "Instruction")
                {
                    // Her RISC-V komutu bellekte 4 byte (32-bit) yer kaplar
                    currentAddress += 4;
                }
            }

            // =================================================================
            // İKİNCİ GEÇİŞ (SECOND PASS): Makine Kodunu Üret
            // Amaç: Assembly komutlarını makine koduna dönüştürmek
            // =================================================================
            currentAddress = 0; // Adresi tekrar başa sarıyoruz

            foreach (ParsedLine parsed in parsedLines)
            {
                if (parsed.Type == "Directive" && parsed.Mnemonic == ".org")
                {
                    currentAddress = parsed.Operands[0].StartsWith("0x")
                            ? Convert.ToInt32(parsed.Operands[0], 16)
                            : Convert.ToInt32(parsed.Operands[0]);
                }
                else if (parsed.Type == "Instruction")
                {
                    try
                    {
                        // CodeGenerator'a komutu ve sembol tablosunu gönderiyoruz
                        uint machineCode = codeGen.GenerateInstruction(parsed, currentAddress, SymTable);

                        // Üretilen 32-bit sayıyı 8 haneli Hexadecimal (16'lık taban) string'e çevir
                        string hexCode = machineCode.ToString("X8");

                        // Çıktı listesine Adres, Makine Kodu ve Orijinal Kodun kendisini ekle
                        machineCodeOutput.Add($"0x{currentAddress:X4} : 0x{hexCode}    # {parsed.OriginalLine.Trim()}");

                        currentAddress += 4;
                    }
                    catch (Exception ex)
                    {
                        // PÇ7 Kapsamında Hata Yönetimi
                        Console.WriteLine($"DERLEME HATASI (Adres 0x{currentAddress:X4}): {ex.Message} -> Satır: {parsed.OriginalLine}");
                    }
                }
            }

            return machineCodeOutput;
        }
    }
}
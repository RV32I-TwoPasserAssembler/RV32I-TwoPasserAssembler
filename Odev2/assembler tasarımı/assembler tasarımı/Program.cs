using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PicoRV_Assembler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine(" PicoRV Linker & Assembler (Çoklu Dosya Desteği)");
            Console.WriteLine("=================================================\n");

            // KRİTİK HATA 1 DÜZELTİLDİ: module.s sisteme dahil edildi.
            string[] inputFiles = { "main.s", "gecikme.s"};
            string outputHexPath = "program.mi";

            if (args.Length > 0)
            {
                inputFiles = args;
            }

            List<ObjectFile> objectFiles = new List<ObjectFile>();
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

            try
            {
                // =================================================================
                // 1. AŞAMA: HER DOSYAYI AYRI AYRI ASSEMBLE ET (Derle)
                // =================================================================
                Console.WriteLine("[1/3] Assembly dosyaları derleniyor...");
                foreach (string file in inputFiles)
                {
                    if (!File.Exists(file))
                    {
                        Console.WriteLine($"[UYARI] '{file}' bulunamadı, atlanıyor...");
                        continue;
                    }

                    string[] sourceLines = File.ReadAllLines(file);
                    AssemblerMotor motor = new AssemblerMotor();

                    string objectFileName = file.Replace(".s", ".o");
                    ObjectFile objFile = motor.Assemble(sourceLines, objectFileName);

                    foreach (var symbol in motor.SymTable.symbols)
                    {
                        objFile.SymbolTable.Add(symbol.Key, symbol.Value);
                    }

                    objectFiles.Add(objFile);

                    string jsonOutput = JsonSerializer.Serialize(objFile, jsonOptions);
                    File.WriteAllText(objectFileName, jsonOutput);
                    Console.WriteLine($" -> '{file}' başarıyla derlendi ve '{objectFileName}' (JSON) oluşturuldu.");
                }

                // =================================================================
                // 2. AŞAMA: LİNKER İLE BİRLEŞTİR VE BAĞLA
                // =================================================================
                Console.WriteLine("\n[2/3] Linker (Bağlayıcı) çalıştırılıyor...");

                // YÖNERGE EKSİĞİ GİDERİLDİ: Basit Linker Script Mantığı (config dosyasından okunuyor)
                var (textBaseAddress, dataBaseAddress) = ParseLinkerScript("linker.ld");

                Linker linker = new Linker();
                List<string> finalMachineCode = linker.Link(objectFiles, textBaseAddress, dataBaseAddress);

                // =================================================================
                // 3. AŞAMA: ÇIKTIYI YAZDIR (HEX FORMATI)
                // =================================================================
                Console.WriteLine($"\n[3/3] Linkleme tamamlandı. Çıktı '{outputHexPath}' dosyasına yazılıyor...");
                File.WriteAllLines(outputHexPath, finalMachineCode);

                Console.WriteLine("\n--- Üretilen Nihai Makine Kodu ---");
                foreach (string code in finalMachineCode)
                {
                    Console.WriteLine(code);
                }
                Console.WriteLine("----------------------------------");

                Console.WriteLine("\nBAŞARILI: Linker görevini tamamladı! Artık FPGA'e yüklenebilir.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nKRİTİK HATA: {ex.Message}");
            }

            Console.WriteLine("\nÇıkmak için bir tuşa basın...");
            Console.ReadLine();
        }

        // BASİT LİNKER SCRIPT OKUYUCU FONKSİYON
        static (int textBase, int dataBase) ParseLinkerScript(string filePath)
        {
            int tBase = 0x0000;
            int dBase = 0x0800; // FPGA Bellek sınırlarına uygun varsayılan değer

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    // Yorum satırlarını atla
                    if (line.Trim().StartsWith("#")) continue;

                    string cleanLine = line.Trim().ToUpper();
                    if (cleanLine.StartsWith("TEXT="))
                    {
                        string val = cleanLine.Substring(5).Replace("0X", "");
                        tBase = Convert.ToInt32(val, 16);
                    }
                    else if (cleanLine.StartsWith("DATA="))
                    {
                        string val = cleanLine.Substring(5).Replace("0X", "");
                        dBase = Convert.ToInt32(val, 16);
                    }
                }
                Console.WriteLine($"[BİLGİ] '{filePath}' okundu -> TEXT: 0x{tBase:X4}, DATA: 0x{dBase:X4}");
            }
            else
            {
                Console.WriteLine($"[UYARI] '{filePath}' bulunamadı! Varsayılan bellek yerleşimi kullanılacak.");
            }
            return (tBase, dBase);
        }
    }
}
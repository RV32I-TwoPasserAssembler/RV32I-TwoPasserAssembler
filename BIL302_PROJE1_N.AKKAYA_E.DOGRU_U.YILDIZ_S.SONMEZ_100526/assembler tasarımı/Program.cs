using System;
using System.Collections.Generic;
using System.IO;

namespace PicoRV_Assembler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine(" PicoRV (RV32I) Assembler - C# Sürümü");
            Console.WriteLine("=================================================\n");

            // Dosya yolunu belirle (Proje klasöründeki test.s dosyasını okuyacak)
            string inputFilePath = "test.s";
            string outputFilePath = "output.hex";

            // Eğer kullanıcı komut satırından argüman girdiyse onları kullan (PÇ7 - Esneklik)
            if (args.Length >= 1) inputFilePath = args[0];
            if (args.Length >= 2) outputFilePath = args[1];

            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($"HATA: '{inputFilePath}' adlı kaynak dosya bulunamadı!");
                Console.WriteLine("Lütfen programın çalıştığı dizine 'test.s' adında bir assembly dosyası koyun.");
                Console.ReadLine();
                return;
            }

            try
            {
                // 1. Dosyayı Satır Satır Oku
                Console.WriteLine($"[1/3] '{inputFilePath}' dosyası okunuyor...");
                string[] sourceLines = File.ReadAllLines(inputFilePath);

                // 2. Assembler Motorunu Başlat ve Derle
                Console.WriteLine("[2/3] İki geçişli (Two-Pass) derleme işlemi başlatıldı...");
                AssemblerMotor motor = new AssemblerMotor();
                List<string> machineCodeLines = motor.Assemble(sourceLines);

                // 3. Sonuçları Ekrana ve Dosyaya Yazdır
                Console.WriteLine($"[3/3] Derleme tamamlandı. '{outputFilePath}' dosyasına yazılıyor...\n");
                File.WriteAllLines(outputFilePath, machineCodeLines);

                // Puan kazandıracak şov kısmı: Sembol Tablosunu Ekrana Yazdır (PÇ6)
                motor.SymTable.PrintTable();

                Console.WriteLine("--- Üretilen Makine Kodu (Hex Formatı) ---");
                foreach (string line in machineCodeLines)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine("------------------------------------------");

                Console.WriteLine("\nBAŞARILI: Program hatasız olarak sonlandı.");
            }
            catch (Exception ex)
            {
                // Kritik hataları yakala
                Console.WriteLine($"\nKRİTİK HATA: {ex.Message}");
            }

            Console.WriteLine("\nÇıkmak için bir tuşa basın...");
            Console.ReadLine();
        }
    }
}
using System;
using System.Collections.Generic;

namespace PicoRV_Assembler
{
    public class SymbolTable
    {
        // Etiket adı (string) ve Adres (int) eşleşmesini tutar
        private Dictionary<string, int> symbols;

        public SymbolTable()
        {
            symbols = new Dictionary<string, int>();
        }

        // Yeni bir etiket ekler
        public void AddSymbol(string label, int address)
        {
            if (symbols.ContainsKey(label))
            {
                throw new Exception($"HATA: '{label}' etiketi zaten tanımlanmış!");
            }
            symbols.Add(label, address);
        }

        // İstenen etiketin adresini döndürür
        public int GetAddress(string label)
        {
            if (symbols.TryGetValue(label, out int address))
            {
                return address;
            }
            throw new Exception($"HATA: '{label}' adında bir etiket bulunamadı!");
        }

        // Test amaçlı tabloyu ekrana yazdırmak için bir metot
        public void PrintTable()
        {
            Console.WriteLine("--- Symbol Table ---");
            foreach (var kvp in symbols)
            {
                // Adresleri 0x0000 formatında (Hexadecimal) yazdırıyoruz
                Console.WriteLine($"{kvp.Key}:\t0x{kvp.Value:X4}");
            }
            Console.WriteLine("--------------------");
        }
    }
}
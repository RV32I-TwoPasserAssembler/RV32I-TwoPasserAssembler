using System;
using System.Collections.Generic;

namespace PicoRV_Assembler
{
    public class SymbolTable
    {
        // ObjectFile içindeki sınıf ile aynı yapıyı kullanıyoruz
        public Dictionary<string, SymbolData> symbols { get; private set; }

        public SymbolTable()
        {
            symbols = new Dictionary<string, SymbolData>();
        }

        // GÜNCELLENDİ: Segment parametresi eklendi
        public void AddSymbol(string label, int address, string segment)
        {
            if (symbols.ContainsKey(label))
            {
                throw new Exception($"HATA: '{label}' etiketi zaten tanımlanmış!");
            }
            symbols.Add(label, new SymbolData { Address = address, Segment = segment });
        }

        public int GetAddress(string label)
        {
            if (symbols.TryGetValue(label, out SymbolData? entry))
            {
                return entry.Address;
            }
            throw new Exception($"HATA: '{label}' adında bir etiket bulunamadı!");
        }

        public void PrintTable()
        {
            Console.WriteLine("--- Symbol Table ---");
            foreach (var kvp in symbols)
            {
                Console.WriteLine($"{kvp.Key}:\t0x{kvp.Value.Address:X4} ({kvp.Value.Segment})");
            }
            Console.WriteLine("--------------------");
        }
    }
}
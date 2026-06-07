#nullable disable
using System.Collections.Generic;

namespace PicoRV_Assembler
{
    public class RelocationEntry
    {
        public int Address { get; set; }
        public char InstructionType { get; set; }
        public int TargetRefId { get; set; }
        public char Operation { get; set; } = '+';
    }

    // YENİ EKLENDİ: Sembolün adresini ve segmentini tutan veri yapısı
    public class SymbolData
    {
        public int Address { get; set; }
        public string Segment { get; set; } // "Text" veya "Data"
    }

    public class ObjectFile
    {
        public string FileName { get; set; }
        public List<string> TextSegment { get; set; }
        public List<string> DataSegment { get; set; }

        // GÜNCELLENDİ: Artık SymbolData tutuyor
        public Dictionary<string, SymbolData> SymbolTable { get; set; }

        public List<string> GlobalSymbols { get; set; }
        public Dictionary<int, string> ReferenceTable { get; set; }
        public List<RelocationEntry> RelocationTable { get; set; }
        public List<string> ExternalSymbols { get; set; }

        public ObjectFile()
        {
            FileName = string.Empty;
            TextSegment = new List<string>();
            DataSegment = new List<string>();
            SymbolTable = new Dictionary<string, SymbolData>();
            GlobalSymbols = new List<string>();
            ReferenceTable = new Dictionary<int, string>();
            RelocationTable = new List<RelocationEntry>();
            ExternalSymbols = new List<string>();
        }
    }
}
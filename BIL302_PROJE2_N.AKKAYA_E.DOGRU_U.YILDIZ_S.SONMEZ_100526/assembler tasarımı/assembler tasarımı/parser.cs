#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoRV_Assembler
{
    public class ParsedLine
    {
        public string OriginalLine { get; set; }
        public string Type { get; set; } 
        public string LabelName { get; set; }
        public string Mnemonic { get; set; } 
        public List<string> Operands { get; set; } 

        public ParsedLine()
        {
            Operands = new List<string>();
        }
    }

    public class Parser
    {
        public ParsedLine ParseLine(string line)
        {
            ParsedLine parsed = new ParsedLine { OriginalLine = line };

            int commentIndex = line.IndexOf('#');
            string cleanLine = commentIndex >= 0 ? line.Substring(0, commentIndex) : line;
            cleanLine = cleanLine.Trim();

            if (string.IsNullOrWhiteSpace(cleanLine))
            {
                parsed.Type = "Empty";
                return parsed;
            }

            if (cleanLine.EndsWith(":"))
            {
                parsed.Type = "Label";
                parsed.LabelName = cleanLine.TrimEnd(':').Trim();
                return parsed;
            }

            if (cleanLine.StartsWith("."))
            {
                parsed.Type = "Directive";
                var parts = cleanLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                parsed.Mnemonic = parts[0]; 
                
                if (parts.Length > 1)
                {
                    string argsPart = cleanLine.Substring(parts[0].Length).Trim();
                    parsed.Operands = argsPart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(op => op.Trim())
                                              .ToList();
                }
                return parsed;
            }

            parsed.Type = "Instruction";
            
            int firstSpace = cleanLine.IndexOfAny(new char[] { ' ', '\t' });
            
            if (firstSpace == -1) 
            {
                parsed.Mnemonic = cleanLine;
            }
            else
            {
                parsed.Mnemonic = cleanLine.Substring(0, firstSpace).Trim();
                string operandsPart = cleanLine.Substring(firstSpace).Trim();
                parsed.Operands = operandsPart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(op => op.Trim())
                                              .ToList();
            }

            return parsed;
        }
    }
}
using Pumkin.HelperFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pumkin.YAML
{
    /// <summary>
    /// Temporary yaml thing. Bad, don't use
    /// </summary>
    public static class PumkinsYAMLTools
    {
        public static string[] OpenFileGetBlocks(string filePath)
        {
            filePath = Helpers.LocalAssetsPathToAbsolutePath(filePath);

            if(!File.Exists(filePath))
                Debug.Log($"Invalid file at: {filePath}");

            return GetBlocksFromText(File.ReadAllText(filePath));
        }        

        public static string[] GetBlocksFromText(string text)
        {
            var lines = text.Split('\n');

            var blocks = new List<string>();
            string block = "";
            bool blockDone = false;
            
            for(int i = 0; i < lines.Length; i++)
            {
                if(lines[i].StartsWith("---") || i == lines.Length - 1)
                    blockDone = true;
                
                block += lines[i];
                if(!string.IsNullOrEmpty(lines[i]))
                    block += '\n';

                if(blockDone)
                {
                    blockDone = false;
                    blocks.Add(block);
                    block = "";
                }
            }
            
            return blocks.ToArray();
        }

        public static string[] BlockToLines(string block)
        {
            var split = block.Split('\n');
            if(split.Length > 0)
                return split.Select(s => s += "\n").ToArray();
            return null;
        }

        public static string LinesToBlock(string[] lines)
        {
            return string.Concat(lines);
        }

        public static void WriteBlocksToFile(string filePath, string[] blocks)
        {
            filePath = Helpers.LocalAssetsPathToAbsolutePath(filePath);
            string file = string.Concat(blocks);
            File.WriteAllText(filePath, file);
        }
    }
}

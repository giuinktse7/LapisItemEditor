using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Backend
{
    public class ItemNameLoader
    {
        // Reads item names from a file.
        // Row format: [[clientID]] [article] name
        //
        // Examples:
        //      [100] void
        //      [3457] a shovel
        //      [35523] an exotic amulet
        public static void LoadFromFile(GameData gameData, string file)
        {
            bool exists = File.Exists(file);
            if (!exists)
            {
                Trace.WriteLine($"LoadFromFile: Could not find file '{file}'");
                return;
            }

            const Int32 bufferSize = 1024;
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize);

            string? line;
            while ((line = streamReader.ReadLine()) != null)
            {
                int index = line.IndexOf("]");
                if (index == -1) continue;

                var rawClientId = line.Substring(1, index - 1);
                if (UInt32.TryParse(rawClientId, out uint clientId))
                {
                    var appearance = gameData.GetItemTypeByClientId(clientId);
                    if (appearance == null)
                    {
                        Trace.WriteLine($"No appearance with id {clientId}");
                        continue;
                    }

                    var name = line.Substring(index + 2);

                    if (name.StartsWith("a "))
                    {
                        appearance.Article = "a";
                        name = name.Substring(2);
                    }
                    else if (name.StartsWith("an "))
                    {
                        appearance.Article = "an";
                        name = name.Substring(3);
                    }

                    appearance.Data.Name = name;
                }
            }
        }
    }
}


using System;
using System.Text;
using System.IO;

namespace MM1DataDumper
{
   class Program
   {
      static string FILE_NAME = "MM.EXE";
      static string DUMP_FILE_NAME = "dump.txt";
      static string VERSION_NUMBER = "v0.1";

      static Item[] items = new Item[255];
      static int[] itemOffsets = new int[255];

      static void Main(string[] args)
      {
         if (File.Exists(FILE_NAME))
         {
            Console.Write($"Opening {FILE_NAME}... ");

            using (var stream = File.Open(FILE_NAME, FileMode.Open, FileAccess.Read))
            {
               Console.WriteLine("Success!\n");

               Console.WriteLine($"Might and Magic 1 Data Dumper ({VERSION_NUMBER}) by ryz\n");
               Console.WriteLine($"Press any key to dump data.\n");

               DumpItemData(stream);

               Console.WriteLine($"All done! data dumped to file {DUMP_FILE_NAME}.");
               Console.ReadLine();

            }
         }
         else
         {
            Console.WriteLine($"File {FILE_NAME} not found! Make sure it's in the same folder as this program.\nAborting.");
            Console.ReadLine();
         }
      }

      static void DumpItemData(FileStream _stream)
      {
         // Offset 0x19B2A - 0x1B311 (6120 byte)
         // 24 Byte per Item, 6120 / 24 = 255 items total
         // Thus each item is adressable by a single byte

         Console.WriteLine("Dumping all items...");

         File.WriteAllText(DUMP_FILE_NAME, string.Empty); // Clear file

         string header1 = $"# |Item Name     |Used by |Special|AMT|Magic   |Cost |Dmg|AC/Dmg|";
         string header2 = $"--+--------------+--------+-------+---+--------+-----+---+------+";

         File.AppendAllText(DUMP_FILE_NAME, header1 + Environment.NewLine);
         File.AppendAllText(DUMP_FILE_NAME, header2 + Environment.NewLine);


         for (int i = 0; i < items.Length; i++)
         {
            int itemChunkSize = 24;
            int itemAbsoluteOffset = 105258;

            itemOffsets[i] = i * itemChunkSize;

            items[i] = new Item();
            items[i].offset = itemAbsoluteOffset + itemOffsets[i];

            _stream.Position = items[i].offset;

            _stream.Read(items[i].nameChunk, 0, items[i].nameChunk.Length);
            _stream.Read(items[i].classChunk, 0, items[i].classChunk.Length);
            _stream.Read(items[i].specialChunk, 0, items[i].specialChunk.Length);
            _stream.Read(items[i].specialAmountChunk, 0, items[i].specialAmountChunk.Length);
            _stream.Read(items[i].isMagicChunk, 0, items[i].isMagicChunk.Length);
            _stream.Read(items[i].unknownChunk, 0, items[i].unknownChunk.Length);
            _stream.Read(items[i].chargesChunk, 0, items[i].chargesChunk.Length);
            _stream.Read(items[i].valueChunk, 0, items[i].valueChunk.Length);
            _stream.Read(items[i].damageChunk, 0, items[i].damageChunk.Length);
            _stream.Read(items[i].bonusChunk, 0, items[i].bonusChunk.Length);



            Console.WriteLine($"Item #{i} at Offset {items[i].offset} is: {Encoding.Default.GetString(items[i].nameChunk)}");

            int itemId = i + 1;
            string val = itemId.ToString("X2");
            string s = $"{val}|{Encoding.Default.GetString(items[i].nameChunk)}|{BitConverter.ToString(items[i].classChunk)}      |{BitConverter.ToString(items[i].specialChunk)}     |{BitConverter.ToString(items[i].specialAmountChunk)} |{BitConverter.ToString(items[i].isMagicChunk)}|{BitConverter.ToString(items[i].unknownChunk)}|{BitConverter.ToString(items[i].chargesChunk)}|{BitConverter.ToString(items[i].valueChunk)}|{BitConverter.ToString(items[i].damageChunk)} |{BitConverter.ToString(items[i].bonusChunk)}    |";

            File.AppendAllText(DUMP_FILE_NAME, s + Environment.NewLine);
         }
         Console.WriteLine("Done!");

      }

      static void DumpMonsterData()
      {
         // Offset 0x1B312 - 0x1CB71 (6240 byte)
         // 32 Byte per Monster, 6240 / 32 = 195 monsters total
      }
   }
}

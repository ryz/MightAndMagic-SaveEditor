using System;
using System.Text;
using System.IO;

namespace MM1DataDumper
{
   class Program
   {
      static string FILE_NAME = "MM.EXE";
      static string DUMP_FILE_NAME = "dump.txt";
      static string VERSION_NUMBER = "v0.2";

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
               //Console.ReadLine();

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

         string header =    $"ID|Item Name     |Category    |Used by  |EquipBonus|Amount|Magic     |Effect     |Charges|Cost  |Dmg|Bonus|";
         string separator = $"--+--------------+------------+---------+----------+------+----------+-----------+-------+------+---+-----+";

         File.AppendAllText(DUMP_FILE_NAME, header + Environment.NewLine);
         File.AppendAllText(DUMP_FILE_NAME, separator + Environment.NewLine);


         for (int i = 0; i < items.Length; i++)
         {
            int itemChunkSize = 24;
            int itemAbsoluteOffset = 105258;

            itemOffsets[i] = i * itemChunkSize;

            items[i] = new Item();
            items[i].offset = itemAbsoluteOffset + itemOffsets[i];
            items[i].id = i + 1;

            _stream.Position = items[i].offset;

            _stream.Read(items[i].nameChunk, 0, items[i].nameChunk.Length);
            _stream.Read(items[i].classChunk, 0, items[i].classChunk.Length);
            _stream.Read(items[i].specialChunk, 0, items[i].specialChunk.Length);
            _stream.Read(items[i].specialAmountChunk, 0, items[i].specialAmountChunk.Length);
            _stream.Read(items[i].magicStateChunk, 0, items[i].magicStateChunk.Length);
            _stream.Read(items[i].magicEffectChunk, 0, items[i].magicEffectChunk.Length);
            _stream.Read(items[i].chargesChunk, 0, items[i].chargesChunk.Length);
            _stream.Read(items[i].valueChunk, 0, items[i].valueChunk.Length);
            _stream.Read(items[i].damageChunk, 0, items[i].damageChunk.Length);
            _stream.Read(items[i].bonusChunk, 0, items[i].bonusChunk.Length);

            if (items[i].id < 61)
            {
               items[i].category = "1-H Weapon";
            }

            if (items[i].id < 86 && items[i].id >= 61)
            {
               items[i].category = "Range Weapon";
            }

            if (items[i].id < 121 && items[i].id >= 86)
            {
               items[i].category = "2-H Weapon";
            }

            if (items[i].id < 156 && items[i].id >= 121)
            {
               items[i].category = "Armor";
            }

            if (items[i].id < 171 && items[i].id >= 156)
            {
               items[i].category = "Shield";
            }

            if (items[i].id <= 255 && items[i].id >= 171)
            {
               items[i].category = "Misc";
            }

            if (BitConverter.IsLittleEndian)
            {
               Array.Reverse(items[i].valueChunk);
            }

            Console.WriteLine($"Item #{i} at Offset {items[i].offset} is: {Encoding.Default.GetString(items[i].nameChunk)}");

            string s = $"{items[i].id.ToString("X2")}|{Encoding.Default.GetString(items[i].nameChunk)}|{items[i].category.PadRight(12)}|{GetRestriction(items[i]).PadRight(9)}|{GetSpecialName(items[i]).PadRight(9)} |{items[i].specialAmount.ToString().PadRight(5)} |{GetMagicState(items[i]).PadRight(10)}|{GetMagicEffect(items[i]).PadRight(11)}|{items[i].charges.ToString().PadRight(6)} |{items[i].value.ToString().PadRight(6)}|{items[i].damage.ToString().PadRight(2)} |{items[i].bonus.ToString().PadRight(2)}   |";

            File.AppendAllText(DUMP_FILE_NAME, s + Environment.NewLine);
         }
         File.AppendAllText(DUMP_FILE_NAME, separator + Environment.NewLine);
         Console.WriteLine("Done!");

      }

      static string GetSpecialName(Item _item)
      {
         var s = BitConverter.ToString(_item.specialChunk);

         switch (s)
         {
            case "00": return "None";
            case "01": return "No Equip";
            case "10": return $"??? {s}"; // ? Only on item UNOBTANIUM
            case "13": return $"??? {s}"; // ? Only on item JADE AMULET
            case "15": return "Int+";
            case "17": return "Might+";
            case "19": return "Pers+";
            case "1D": return "Speed+";
            case "1F": return "Accuracy+";
            case "21": return "Luck+";
            case "25": return $"??? {s}"; // Only on items DIAMOND COLLAR and FIRE OPAL
            case "3C": return "AC+";
            case "58": return "MagicRes+";
            case "5A": return "FireRes+";
            case "5C": return "ColdRes+";
            case "5E": return "ElecRes+";
            case "60": return "AcidRes+";
            case "62": return "FearRes+";
            case "64": return "PoisnRes+";
            case "66": return "Holy+";
            case "6C": return $"Thief? {s}"; // ?
            case "FF": return "Cursed";
            default: return $"?({s})";
         }

      }

      static string GetMagicState(Item _item)
      {
         var s = BitConverter.ToString(_item.magicStateChunk);

         switch (s)
         {
            case "00": return "No";
            case "18": return "Might+";
            case "1E": return "Speed+";
            case "20": return "Acc+";
            case "22": return "Luck+";
            case "24": return "Level+";
            case "25": return "Age+";
            case "2B": return "Spell Pts+";
            case "30": return "Spell Lvl+";
            case "31": return "Gems+";
            case "3A": return "Gold+";
            case "3E": return "Food+";
            case "59": return "Magic+";
            case "63": return "Holy/Fear+";
            case "FF": return "Spell";
            default: return $"?({s})";
         }
      }

      static string GetMagicEffect(Item _item)
      {
         var s = BitConverter.ToString(_item.magicEffectChunk);

         if (GetMagicState(_item) == "Spell")
         {
            switch (s)
            {
               case "00": return "Whistle"; // Ruby Whistle only
               //case "01": return "C1/2"; // C1/2
               //case "02": return "C1/3"; // C1/3
               //case "03": return "C1/4"; // C1/4
               //case "04": return "C1/5"; // C1/5
               //case "05": return "C1/6"; // C1/6
               //case "06": return "C1/7"; // C1/7
               case "30": return "S1/2"; // S1/2
               case "31": return "S1/3"; // S1/3
               case "32": return "Flame Arrow"; // S1/4
               case "33": return "S1/5"; // S1/5
               case "34": return "S1/6"; // S1/6
               case "35": return "S1/7"; // S1/7
               case "36": return "S1/8"; // S1/8
               case "47": return "S4/1"; 
               case "48": return "S4/3"; 
               default: return $"?({s})";
            }
         }

         else if (GetMagicState(_item) == "Might+" || GetMagicState(_item) == "Speed+" || GetMagicState(_item) == "Acc+" || GetMagicState(_item) == "Luck+" || GetMagicState(_item) == "Level+" || GetMagicState(_item) == "Age+" || GetMagicState(_item) == "Spell Pts+" || GetMagicState(_item) == "Spell Lvl+" || GetMagicState(_item) == "Gems+" || GetMagicState(_item) == "Gold+" || GetMagicState(_item) == "Food+")
         {
            int i = _item.magicEffectChunk[0];

            return i.ToString();
         }

         switch (s)
         {
            case "00": return "None";
            default: return $"?({s})";
         }

      }

      static string GetRestriction(Item _item)
      {
         var s = BitConverter.ToString(_item.classChunk);

         switch (s)
         {
            case "00": return "RSCAPK/EG";
            case "01": return "-SCAPK/EG";
            case "02": return "R-CAPK/EG";
            case "03": return "--CAPK/EG";
            case "04": return "RS-APK/EG";
            case "05": return "-S-APK/EG";
            case "06": return "R--APK/EG";
            case "07": return "---APK/EG";
            case "08": return "RSC-PK/EG";
            case "09": return "-SC-PK/EG";
            case "0A": return "R-C-PK/EG";
            case "0B": return "--C-PK/EG";
            case "0C": return "RS--PK/EG";
            case "0D": return "-S--PK/EG";
            case "0E": return "R---PK/EG";
            case "0F": return "----PK/EG";

            case "15": return "-S-A-K/EG";
            case "1F": return "-----K/EG";

            case "21": return "-SCAP-/EG";

            case "30": return "----P-/EG";
            case "32": return "R-CA--/EG";
            case "34": return "RS-A--/EG";
            case "35": return "-S-A--/EG";
            case "36": return "R--A--/EG";
            case "37": return "---A--/EG";
            case "3B": return "--C---/EG";
            case "3D": return "-S----/EG";
            case "3E": return "R-----/EG";

            // Good Alignment only
            case "40": return "RSCAPK/-G";
            case "41": return "-SCAPK/-G";
            case "42": return "R-CAPK/-G";
            case "43": return "--CAPK/-G";
            case "44": return "RS-APK/-G";
            case "45": return "-S-APK/-G";
            case "46": return "R--APK/-G";
            case "47": return "---APK/-G";
            case "48": return "RSC-PK/-G";
            case "49": return "-SC-PK/-G";
            case "4A": return "R-C-PK/-G";
            case "4B": return "--C-PK/-G";
            case "4C": return "RS--PK/-G";
            case "4D": return "-S--PK/-G";
            case "4E": return "R---PK/-G";
            case "4F": return "----PK/-G";

            case "6F": return "----P-/-G";

            case "79": return "-SC---/-G";
            case "7B": return "--C---/-G";

            // Evil Alignment only
            case "80": return "RSCAPK/E-";
            case "81": return "-SCAPK/E-";
            case "82": return "R-CAPK/E-";
            case "83": return "--CAPK/E-";
            case "84": return "RS-APK/E-";
            case "85": return "-S-APK/E-";
            case "86": return "R--APK/E-";
            case "87": return "---APK/E-";
            case "88": return "RSC-PK/E-";
            case "89": return "-SC-PK/E-";
            case "8A": return "R-C-PK/E-";
            case "8B": return "--C-PK/E-";
            case "8C": return "RS--PK/E-";
            case "8D": return "-S--PK/E-";
            case "8E": return "R---PK/E-";
            case "8F": return "----PK/E-";

            case "AF": return "----P-/E-";

            case "BB": return "--C---/E-";

            // Neutral Alignment only
            case "C4": return "RS-APK/--";
            case "C6": return "R--APK/--";
            case "CF": return "----PK/--";

            case "D7": return "---A-K/--";

            case "FF": return "------/--";

            default: return $"??? ({s})";
         }
      }

      static void DumpMonsterData()
      {
         // Offset 0x1B312 - 0x1CB71 (6240 byte)
         // 32 Byte per Monster, 6240 / 32 = 195 monsters total
      }
   }
}

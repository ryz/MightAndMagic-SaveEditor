using System;
using System.Text;
using System.IO;

namespace MM1SaveEditor
{
   class Dumper
   {
      public static string EXE_FILE_NAME = "MM.EXE";
      static string ITEM_DUMP_FILE_NAME = "itemdump.txt";
      static string MONSTER_DUMP_FILE_NAME = "monsterdump.txt";
      static string VERSION_NUMBER = "v0.4";

      public static Item[] items = new Item[255];
      static int[] itemOffsets = new int[items.Length];

      static Monster[] monsters = new Monster[195];
      static int[] monsterOffsets = new int[monsters.Length];

      public static bool isInitialized = false;

      public void Init()
      {
         if (File.Exists(EXE_FILE_NAME))
         {
            Console.Write($"Opening {EXE_FILE_NAME}... ");

            using (var stream = File.Open(EXE_FILE_NAME, FileMode.Open, FileAccess.Read))
            {
               Console.WriteLine("Success!\n");
               Console.WriteLine($"Might and Magic 1 Data Dumper ({VERSION_NUMBER}) by ryz\n");

               InitializeItems();
               ParseAllItems(stream);

               InitializeMonsters();
               ParseAllMonsters(stream);

               isInitialized = true;
            }
         }
         else
         {
            Console.WriteLine($"File {EXE_FILE_NAME} not found! Make sure it's in the same folder as this program.\nAborting.");
         }
      }

      static void InitializeItems()
      {
         for (int i = 0; i < items.Length; i++)
         {
            int itemChunkSize = 24;
            int itemAbsoluteOffset = 105258;

            itemOffsets[i] = i * itemChunkSize;

            items[i] = new Item();
            items[i].offset = itemAbsoluteOffset + itemOffsets[i];
            items[i].id = i + 1;
         }
      }

      static void ParseAllItems(FileStream _stream)
      {
         // Offset 0x19B2A - 0x1B311 (6120 byte)
         // 24 Byte per Item, 6120 / 24 = 255 items total
         // Thus each item is adressable by a single byte

         for (int i = 0; i < items.Length; i++)
         {
            ParseItem(_stream, items[i]);

            Console.WriteLine($"Item #{i} at Offset {items[i].offset} is: {Encoding.Default.GetString(items[i].nameChunk)}");
         }
      }

      static void ParseItem(FileStream _stream, Item _item)
      {
         _stream.Position = _item.offset;

         _stream.Read(_item.nameChunk, 0, _item.nameChunk.Length);
         _stream.Read(_item.classChunk, 0, _item.classChunk.Length);
         _stream.Read(_item.specialChunk, 0, _item.specialChunk.Length);
         _stream.Read(_item.specialAmountChunk, 0, _item.specialAmountChunk.Length);
         _stream.Read(_item.magicStateChunk, 0, _item.magicStateChunk.Length);
         _stream.Read(_item.magicEffectChunk, 0, _item.magicEffectChunk.Length);
         _stream.Read(_item.chargesChunk, 0, _item.chargesChunk.Length);
         _stream.Read(_item.valueChunk, 0, _item.valueChunk.Length);
         _stream.Read(_item.damageChunk, 0, _item.damageChunk.Length);
         _stream.Read(_item.bonusChunk, 0, _item.bonusChunk.Length);

         if (_item.id < 61)
         {
            _item.category = "1-H Weapon";
         }

         if (_item.id < 86 && _item.id >= 61)
         {
            _item.category = "Range Weapon";
         }

         if (_item.id < 121 && _item.id >= 86)
         {
            _item.category = "2-H Weapon";
         }

         if (_item.id < 156 && _item.id >= 121)
         {
            _item.category = "Armor";
         }

         if (_item.id < 171 && _item.id >= 156)
         {
            _item.category = "Shield";
         }

         if (_item.id <= 255 && _item.id >= 171)
         {
            _item.category = "Misc";
         }

         if (BitConverter.IsLittleEndian)
         {
            Array.Reverse(_item.valueChunk);
         }
      }

      public void DumpAllItemsToFile()
      {

         string header =    $"ID|Item Name     |Category    |Used by  |EquipBonus|Amount|Magic     |Effect     |Charges|Cost  |Dmg|Bonus|";
         string separator = $"--+--------------+------------+---------+----------+------+----------+-----------+-------+------+---+-----+";

         Console.Clear();
         Console.WriteLine($"Dumping all items to file {ITEM_DUMP_FILE_NAME}...");

         File.WriteAllText(ITEM_DUMP_FILE_NAME, string.Empty); // Clear file
         File.AppendAllText(ITEM_DUMP_FILE_NAME, header + Environment.NewLine);
         File.AppendAllText(ITEM_DUMP_FILE_NAME, separator + Environment.NewLine);

         for (int i = 0; i < items.Length; i++)
         {
            DumpItem(items[i]);
         }

         File.AppendAllText(ITEM_DUMP_FILE_NAME, separator + Environment.NewLine);
         Console.WriteLine($"Done! All data dumped. Press any key to continue");

      }

      static void DumpItem(Item _item)
      {
         string s = $"{_item.id.ToString("X2")}|{Encoding.Default.GetString(_item.nameChunk)}|{_item.category.PadRight(12)}|{GetRestriction(_item).PadRight(9)}|{GetSpecialName(_item).PadRight(9)} |{_item.specialAmount.ToString().PadRight(5)} |{GetMagicState(_item).PadRight(10)}|{GetMagicEffect(_item).PadRight(11)}|{_item.charges.ToString().PadRight(6)} |{_item.value.ToString().PadRight(6)}|{_item.damage.ToString().PadRight(2)} |{_item.bonus.ToString().PadRight(2)}   |";
         File.AppendAllText(ITEM_DUMP_FILE_NAME, s + Environment.NewLine);
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

      static void InitializeMonsters()
      {
         for (int i = 0; i < monsters.Length; i++)
         {
            int monsterChunkSize = 32;
            int monsterAbsoluteOffset = 111378;

            monsterOffsets[i] = i * monsterChunkSize;

            monsters[i] = new Monster();
            monsters[i].offset = monsterAbsoluteOffset + monsterOffsets[i];
            monsters[i].id = i + 1;
         }
      }

      static void ParseAllMonsters(FileStream _stream)
      {
         // Offset 0x1B312 - 0x1CB71 (6240 byte)
         // 32 Byte per Monster, 6240 / 32 = 195 monsters total

         for (int i = 0; i < monsters.Length; i++)
         {
            ParseMonster(_stream, monsters[i]);

            Console.WriteLine($"Monster #{i} at Offset {monsters[i].offset} is: {Encoding.Default.GetString(monsters[i].nameChunk)}");
         }
      }

      static void ParseMonster(FileStream _stream, Monster _monster)
      {
         _stream.Position = _monster.offset;

         _stream.Read(_monster.nameChunk, 0, _monster.nameChunk.Length);
         _stream.Read(_monster.dataChunk, 0, _monster.dataChunk.Length);
         _stream.Read(_monster.healthChunk, 0, _monster.healthChunk.Length);
         _stream.Read(_monster.acChunk, 0, _monster.acChunk.Length);
         _stream.Read(_monster.damageChunk, 0, _monster.damageChunk.Length);
         _stream.Read(_monster.attacksChunk, 0, _monster.attacksChunk.Length);
         _stream.Read(_monster.speedChunk, 0, _monster.speedChunk.Length);
         _stream.Read(_monster.xpChunk, 0, _monster.xpChunk.Length);
         _stream.Read(_monster.dataChunk2, 0, _monster.dataChunk2.Length);

      }

      static void DumpAllMonstersToFile()
      {

         string header = "ID|Name           |Data |HP     |AC   |Damage|Attacks|Speed|XP   |Data2";
         string separator = "--+---------------+-----+-------+-----+------+-------+-----+-----+-----------------------";

         Console.WriteLine($"Dumping all monsters to file {MONSTER_DUMP_FILE_NAME}...");

         File.WriteAllText(MONSTER_DUMP_FILE_NAME, string.Empty); // Clear file
         File.AppendAllText(MONSTER_DUMP_FILE_NAME, header + Environment.NewLine);
         File.AppendAllText(MONSTER_DUMP_FILE_NAME, separator + Environment.NewLine);

         for (int i = 0; i < monsters.Length; i++)
         {
            DumpMonster(monsters[i]);
         }

         File.AppendAllText(MONSTER_DUMP_FILE_NAME, separator + Environment.NewLine);
         Console.WriteLine($"Done! data dumped to file {MONSTER_DUMP_FILE_NAME}.");
      }

      static void DumpMonster(Monster _monster)
      {
         string s = $"{_monster.id.ToString("X2")}|{Encoding.Default.GetString(_monster.nameChunk)}|{BitConverter.ToString(_monster.dataChunk, 0)}|{_monster.healthMin.ToString().PadLeft(3)}-{_monster.healthMax.ToString().PadRight(3)}|{_monster.ac.ToString().PadRight(5)}|{_monster.damage.ToString().PadRight(6)}|{_monster.attacks.ToString().PadRight(7)}|{_monster.speed.ToString().PadRight(5)}|{_monster.xp.ToString().PadRight(5)}|{BitConverter.ToString(_monster.dataChunk2, 0)}";
         File.AppendAllText(MONSTER_DUMP_FILE_NAME, s + Environment.NewLine);
      }


   }
}

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace MightAndMagicSaveEditor
{
   class Program
   {
      static string FILE_NAME = "ROSTER.DTA";
      static string VERSION_NUMBER = "v0.5";

      static Character[] characters = new Character[18];
      static int[] characterOffsets = new int[18];

      static void Main(string[] args)
      {
         if (File.Exists(FILE_NAME))
         {
            Console.Write($"Opening {FILE_NAME}... ");

            using (var stream = File.Open(FILE_NAME, FileMode.Open, FileAccess.ReadWrite))
            {
               Console.WriteLine("Success!\n");

               InitializeCharacters();
               MainMenu(stream);
            }
         }
         else
         {
            Console.WriteLine($"File {FILE_NAME} not found! Make sure it's in the same folder as this program.\nAborting.");
            Console.ReadLine();
         }
      }

      static void InitializeCharacters()
      {
         // create all the character constructors and set their offsets
         for (int i = 0; i < characters.Length; i++)
         {
            // Initialize stuff - There are 18 characters in the file, each is 127 bytes long
            int characterChunkSize = 127;

            characterOffsets[i] = i * characterChunkSize;

            characters[i] = new Character();
            characters[i].offset = characterOffsets[i];
         }
      }

      static void ReadCharacterSlots(FileStream _stream)
      {
         // There are 18 "character slot" bytes at Offset 2286=0x8EE in the ROSTER.DTA file.
         // We read this chunk to see if a character exists (value is not 0) and it's location/town (value is 1-5).
         // This prevents nasty errors for characters which have been wiped in-game, as the game literally sets every byte of that character to zero in this case.

         byte[] characterSlotsChunk = new byte[18];
         int characterSlotsOffset = 2286;

         _stream.Position = characterSlotsOffset;
         _stream.Read(characterSlotsChunk, 0, characterSlotsChunk.Length);

         // Check a character's slot value, determine if they exist and set their "location"
         for (int i = 0; i < characters.Length; i++)
         {
            characters[i].exists = (characterSlotsChunk[i] == 0) ? false : true;

            characters[i].locationNum = characterSlotsChunk[i];
         }

      }

      static void MainMenu(FileStream _stream)
      {
         ConsoleKeyInfo userInput;

         do
         {
            DisplayMainMenu();
            userInput = Console.ReadKey(false);

            switch (userInput.KeyChar.ToString())
            {
               case "1":
                  CharacterListMenu(_stream);
                  break;
               case "2":
                  QuickStartPackage(_stream, characters[0]);
                  break;
               default:
                  break;
            }
            Console.Clear();
         }
         while (userInput.Key != ConsoleKey.Escape);
      }

      static void DisplayMainMenu()
      {
         Console.WriteLine($"Might and Magic 1 Save Game Editor ({VERSION_NUMBER}) by ryz");
         Console.WriteLine();
         Console.WriteLine("1. List (and edit) characters");
         Console.WriteLine("2. Quick Start Package - Give each character XP, Gold and Gems");
         Console.WriteLine();
         Console.WriteLine("Press ESC to exit.");
      }

      static void DisplayCharacterListMenu(FileStream _stream)
      {
         Console.Clear();
         Console.WriteLine("Character List");
         Console.WriteLine();
         PrintCharacterHeader();

         int characterCounter = 0;

         for (int i = 0; i < characters.Length; i++)
         {
            if (!characters[i].exists)
            {
               Console.WriteLine($"-- ---EMPTY-SLOT-- --- ------- -------- -------- --- ----- -------- --------");
            }
            else
            {
               _stream.Position = characterOffsets[i];

               ParseCharacter(_stream, characters[i]);
               PrintCharacterShort(characters[i]);

               characterCounter++;
            }
         }

         Console.WriteLine();
         if(characterCounter <= 10)
         {
            Console.WriteLine($"Select a character (1-{characterCounter}). Press A to edit ALL characters.\nPress ESC to exit.");
         }
         else
         {
            Console.WriteLine($"Select a character (1-10, F1-F8). Press A to edit ALL characters.\nPress ESC to exit.");
         }


      }

      static void CharacterListMenu(FileStream _stream)
      {

         ConsoleKeyInfo input;
         ReadCharacterSlots(_stream);

         do
         {
            DisplayCharacterListMenu(_stream);
            input = Console.ReadKey(false);

            switch (input.Key)
            {
               case ConsoleKey.D1:
                  EditCharacter(_stream, characters[0]);
                  break;
               case ConsoleKey.D2:
                  EditCharacter(_stream, characters[1]);
                  break;
               case ConsoleKey.D3:
                  EditCharacter(_stream, characters[2]);
                  break;
               case ConsoleKey.D4:
                  EditCharacter(_stream, characters[3]);
                  break;
               case ConsoleKey.D5:
                  EditCharacter(_stream, characters[4]);
                  break;
               case ConsoleKey.D6:
                  EditCharacter(_stream, characters[5]);
                  break;
               case ConsoleKey.D7:
                  EditCharacter(_stream, characters[6]);
                  break;
               case ConsoleKey.D8:
                  EditCharacter(_stream, characters[7]);
                  break;
               case ConsoleKey.D9:
                  EditCharacter(_stream, characters[8]);
                  break;
               case ConsoleKey.D0:
                  EditCharacter(_stream, characters[9]);
                  break;
               case ConsoleKey.F1:
                  EditCharacter(_stream, characters[10]);
                  break;
               case ConsoleKey.F2:
                  EditCharacter(_stream, characters[11]);
                  break;
               case ConsoleKey.F3:
                  EditCharacter(_stream, characters[12]);
                  break;
               case ConsoleKey.F4:
                  EditCharacter(_stream, characters[13]);
                  break;
               case ConsoleKey.F5:
                  EditCharacter(_stream, characters[14]);
                  break;
               case ConsoleKey.F6:
                  EditCharacter(_stream, characters[15]);
                  break;
               case ConsoleKey.F7:
                  EditCharacter(_stream, characters[16]);
                  break;
               case ConsoleKey.F8:
                  EditCharacter(_stream, characters[17]);
                  break;
               case ConsoleKey.A:
                  EditAllCharacters(_stream);
                  break;
               default:
                  break;
            }

            Console.Clear();
         } while (input.Key != ConsoleKey.Escape);

      }

      // gives each character 5000 XP, 200 Gems and 5000 Gold
      static void QuickStartPackage(FileStream _stream, Character _char)
      {
         ReadCharacterSlots(_stream);

         // We parse, modify and write back parameters for each character
         for (int i = 0; i < characters.Length; i++)
         {
            Console.WriteLine($"\nReading Slot #{i + 1}...");

            if (!characters[i].exists)
            {
               Console.WriteLine($"No character found! Skipping to next slot...\n");
            }
            else
            {
               Console.WriteLine($"Character found!\n");
               Console.WriteLine($"Supercharging Character #{i + 1} at Offset {characterOffsets[i]}...\n");

               _stream.Position = characterOffsets[i];

               ParseCharacter(_stream, characters[i]);
               PrintCharacter(characters[i]);

               ModifyChunkUInt24(characters[i].xpChunk, "XP", 5000);
               ModifyChunkUInt16(characters[i].gemsChunk, "Gems", 200);
               ModifyChunkUInt24(characters[i].goldChunk, "Gold", 5000);

               Console.WriteLine($"Writing new values back to {FILE_NAME}. Are you sure?");
               Console.ReadLine();

               WriteChunk(_stream, characters[i].xpChunk, characters[i].xpOffset);
               WriteChunk(_stream, characters[i].gemsChunk, characters[i].gemsOffset);
               WriteChunk(_stream, characters[i].goldChunk, characters[i].goldOffset);
            }
         }

         Console.WriteLine("\nAll done! have fun. Press ENTER to get back to the menu.");
         Console.ReadLine();
      }

      static void EditCharacter(FileStream _stream, Character _char)
      {
         if (_char.exists)
         {

            ConsoleKeyInfo input;

            _stream.Position = _char.offset;

            ParseCharacter(_stream, _char);

            do
            {
               bool isValueChanged = false;
               PrintCharacter(_char);
               Console.WriteLine("\nEdit (N)ame, (S)ex, A(l)ingment, (R)ace, (C)lass, E(X)perience, (G)old, G(E)ms\nor (A)LL values. Press ESC to go back.");

               input = Console.ReadKey(true);

               switch (input.Key)
               {
                  case ConsoleKey.N:
                     ModifyNameChunk(_char.nameChunk);
                     isValueChanged = true;
                     break;
                  case ConsoleKey.S:
                     ModifyChunkUInt8(_char.sexChunk, "Sex", 1, 2);
                     isValueChanged = true;
                     break;
                  case ConsoleKey.L:
                     ModifyChunkUInt8(_char.alignmentChunk, "Alignment", 1, 3);
                     isValueChanged = true;
                     break;
                  case ConsoleKey.R:
                     ModifyChunkUInt8(_char.raceChunk, "Race", 1, 5);
                     isValueChanged = true;
                     break;
                  case ConsoleKey.C:
                     ModifyChunkUInt8(_char.classChunk, "Class", 1, 5);
                     isValueChanged = true;
                     break;
                  case ConsoleKey.X:
                     ModifyChunkUInt24(_char.xpChunk, "XP");
                     isValueChanged = true;
                     break;
                  case ConsoleKey.E:
                     ModifyChunkUInt16(_char.gemsChunk, "Gems");
                     isValueChanged = true;
                     break;
                  case ConsoleKey.G:
                     ModifyChunkUInt24(_char.goldChunk, "Gold");
                     isValueChanged = true;
                     break;
                  case ConsoleKey.Q:
                     ModifyChunkUInt8(_char.questChunk1, "Quest", 0, 2);
                     break;
                  case ConsoleKey.A:
                     ModifyNameChunk(_char.nameChunk);
                     ModifyChunkUInt8(_char.sexChunk, "Sex", 1, 2);
                     ModifyChunkUInt8(_char.alignmentChunk, "Alignment", 1, 3);
                     ModifyChunkUInt8(_char.raceChunk, "Race", 1, 5);
                     ModifyChunkUInt8(_char.classChunk, "Class", 1, 5);
                     ModifyChunkUInt24(_char.xpChunk, "XP");
                     ModifyChunkUInt16(_char.gemsChunk, "Gems");
                     ModifyChunkUInt24(_char.goldChunk, "Gold");
                     isValueChanged = true;

                     break;
               }

               if (isValueChanged)
               {
                  // Write chunks back to the file
                  Console.WriteLine($"Writing new value(s) back to {FILE_NAME}. Are you sure? Press ESC to abort.");
                  input = Console.ReadKey(true);

                  if (input.Key != ConsoleKey.Escape)
                  {
                     WriteChunk(_stream, _char.nameChunk, _char.nameOffset);
                     WriteChunk(_stream, _char.sexChunk, _char.sexOffset);
                     WriteChunk(_stream, _char.alignmentChunk, _char.alignmentOffset);
                     WriteChunk(_stream, _char.raceChunk, _char.raceOffset);
                     WriteChunk(_stream, _char.classChunk, _char.classOffset);
                     WriteChunk(_stream, _char.xpChunk, _char.xpOffset);
                     WriteChunk(_stream, _char.gemsChunk, _char.gemsOffset);
                     WriteChunk(_stream, _char.goldChunk, _char.goldOffset);
                     WriteChunk(_stream, _char.questChunk1, _char.questOffset);
                  }

               }

            } while (input.Key != ConsoleKey.Escape);
         }

      }

      static void EditAllCharacters(FileStream _stream)
      {
         ReadCharacterSlots(_stream);

         // We parse, modify and write back parameters for each character
         for (int i = 0; i < characters.Length; i++)
         {

            Console.WriteLine($"\nReading Slot #{i + 1}...");

            if (!characters[i].exists)
            {
               Console.WriteLine($"No character found! Skipping to next slot...\n");
            }
            else
            {
               Console.WriteLine($"Character found!\n");
               Console.WriteLine($"Reading Character #{i + 1} at Offset {characterOffsets[i]}...\n");

               EditCharacter(_stream, characters[i]);

               Console.WriteLine($"\nCharacter #{i + 1} done!\n");
            }
         }

         Console.WriteLine("\nAll done!");
         Console.ReadLine();
      }

      static void ModifyChunkUInt8(byte[] _chunk, string _chunkName, uint _min, uint _max)
      {
         byte input;
         byte[] newArray = new byte[1];

         Console.Write($"\nEnter new {_chunkName} value ({_min}-{_max}): ");

         while (!byte.TryParse(Console.ReadLine(), out input) || input < _min || input > _max)
         {
            Console.Write($"Enter a valid numerical value between {_min} and {_max}! New value: ");
         }

         newArray[0] = input;

         Array.Clear(_chunk, 0, _chunk.Length);
         Array.Copy(newArray, _chunk, newArray.Length);
      }

      // Set by input
      static void ModifyChunkUInt16(byte[] _chunk, string _chunkName)
      {
         ushort input;

         Console.Write($"\nEnter new {_chunkName} value (0-65535): ");

         while (!UInt16.TryParse(Console.ReadLine(), out input))
         {
            Console.Write($"Enter a valid numerical value between 0 and 65535! New value: ");
         }

         byte[] newArray = BitConverter.GetBytes(input);

         Array.Clear(_chunk, 0, _chunk.Length);
         Array.Copy(newArray, _chunk, 2);
      }

      // Set directly
      static void ModifyChunkUInt16(byte[] _chunk, string _chunkName, ushort _amount)
      {
         ushort newVal = _amount;

         byte[] newArray = BitConverter.GetBytes(newVal);

         Array.Clear(_chunk, 0, _chunk.Length);
         Array.Copy(newArray, _chunk, 2);
      }

      // Set by input
      static void ModifyChunkUInt24(byte[] _chunk, string _chunkName)
      {
         uint input;
         var uint24RangeMax = 16777215;

         Console.Write($"\nEnter new {_chunkName} value (0-{uint24RangeMax}): ");

         // Should be a UInt24, but there's none in C# by default.
         // Read this as a UInt32 and later just copy three bytes.
         while (!UInt32.TryParse(Console.ReadLine(), out input) || input > uint24RangeMax)
         {
            Console.Write($"Enter a valid numerical value between 0 and {uint24RangeMax}! New value: ");
         }

         // Create an intermediate byte array where we copy to and from
         // This is because BitConverter.GetBytes() can't return UInt24
         // which would result in a too long chunk (byte array). 
         byte[] newArray = BitConverter.GetBytes(input);

         Array.Clear(_chunk, 0, _chunk.Length);

         Array.Copy(newArray, _chunk, 3); // Remember to copy just three bytes > UInt24
      }

      // Set directly
      static void ModifyChunkUInt24(byte[] _chunk, string _chunkName, uint _amount)
      {;
         // Should be a UInt24, but there's none in C# by default.
         // Read this as a UInt32 and later just copy three bytes.
         uint newVal = _amount;

         // Create an intermediate byte array where we copy to and from
         // This is because BitConverter.GetBytes() can't return UInt24
         // which would result in a too long chunk (byte array). 

         byte[] newArray = BitConverter.GetBytes(newVal);

         Array.Clear(_chunk, 0, _chunk.Length);
         
         Array.Copy(newArray, _chunk, 3); // Remember to copy just three bytes > UInt24
      }

      static void ModifyNameChunk(byte[] _name)
      {
         bool isNameValid = false;
         byte[] newName = new byte[_name.Length];

         // Check if the name contains only uppercase latin characters and numerals from 0-9
         Regex rx = new Regex("^[A-Z0-9]*$");

         while(!isNameValid)
         {
            //ask for and get new name from input and save it into a byte array
            Console.Write("\nEnter new Name (Max 15 characters, A-Z, 0-9): ");
            string nameInput = Console.ReadLine().ToUpper();

            // Truncate input above 15 characters
            nameInput = nameInput.Substring(0, Math.Min(15, nameInput.Length));

            if (rx.IsMatch(nameInput) && !String.IsNullOrEmpty(nameInput))
            {
               Console.Write($"Name '{nameInput}' accepted.\n");
               newName = Encoding.ASCII.GetBytes(nameInput);

               isNameValid = true;
            }
            else
            {
               Console.Write($"Name '{nameInput}' contains invalid characters! Try again.\n");
               isNameValid = false;
            }
         }

         // we clear the old array first so we can just copy the new one in it - keeps array size the same
         Array.Clear(_name, 0, _name.Length);
         Array.Copy(newName, _name, newName.Length);
      }

      // Write chunk back from byte array
      static void WriteChunk(Stream _stream, byte[] _chunk, int _offset)
      {
         _stream.Seek(_offset, SeekOrigin.Begin);
         _stream.Write(_chunk, 0, _chunk.Length);
      }

      // Write chunk back from single byte
      static void WriteChunk(Stream _stream, byte _byte, int _offset)
      {
         _stream.Seek(_offset, SeekOrigin.Begin);
         _stream.WriteByte(_byte);
      }

      static void ParseCharacter(FileStream _stream, Character _char)
      {
         _stream.Read(_char.nameChunk, 0, _char.nameChunk.Length);                       // Character Name 0x0 - 0xE
         _stream.Position += 1;                                                          // UNKNOWN 0xF

         _stream.Read(_char.sexChunk, 0, _char.sexChunk.Length);                         // Sex 0x10
         _stream.Position += 1;                                                          // UNKNOWN 0x11

         _stream.Read(_char.alignmentChunk, 0, _char.alignmentChunk.Length);             // Alignment 0x12
         _stream.Read(_char.raceChunk, 0, _char.raceChunk.Length);                       // Race 0x13
         _stream.Read(_char.classChunk, 0, _char.classChunk.Length);                     // Character Class - 0x14
         _stream.Read(_char.statsChunk, 0, _char.statsChunk.Length);                     // Stats - 0x15 - 0x22

         _stream.Read(_char.levelChunk1, 0, _char.levelChunk1.Length);                   // Level - 0x23
         _stream.Read(_char.levelChunk2, 0, _char.levelChunk2.Length);                   // Level - 0x24
         _stream.Read(_char.ageChunk, 0, _char.ageChunk.Length);                         // Age Offset 37=0x25
         _stream.Position += 1;                                                          // UNKNOWN - 0x26

         _stream.Read(_char.xpChunk, 0, _char.xpChunk.Length);                           // Experience - UInt24 0x27 - 0x29
         _stream.Position += 1;                                                          // UNKNOWN - 0x2A

         _stream.Read(_char.magicPointsCurrentChunk, 0, _char.magicPointsCurrentChunk.Length); // Magic Points - 0x2B - 0x2C
         _stream.Read(_char.magicPointsMaxChunk, 0, _char.magicPointsMaxChunk.Length);   // Magic Points Max - 0x2D - 0x2E
         _stream.Read(_char.spellLevelChunk, 0, _char.spellLevelChunk.Length);           // Spell Level - 0x2F - 0x30

         _stream.Read(_char.gemsChunk, 0, _char.gemsChunk.Length);                       // Gems - ushort  0x31 - 0x32

         _stream.Read(_char.healthCurrentChunk, 0, _char.healthCurrentChunk.Length);     // Health Points Current - 0x33 - 0x34
         _stream.Read(_char.healthModifiedChunk, 0, _char.healthModifiedChunk.Length);   // Health Points Modified 0x35 - 0x36
         _stream.Read(_char.healthMaxChunk, 0, _char.healthMaxChunk.Length);             // Health Points Max - 0x37 - 0x38

         _stream.Read(_char.goldChunk, 0, _char.goldChunk.Length);                       // Gold - 0x39 - 0x3B
         _stream.Position += 1;                                                          // UNKNOWN 0x3C

         _stream.Read(_char.armorClassChunk, 0, _char.armorClassChunk.Length);           // Armor Class - 0x3D
         _stream.Read(_char.foodChunk, 0, _char.foodChunk.Length);                       // Food - 0x3E
         _stream.Read(_char.conditionChunk, 0, _char.conditionChunk.Length);             // Condition - 0x3F

         _stream.Read(_char.equippedWeaponChunk, 0, _char.equippedWeaponChunk.Length);   // Equipped Weapon - 0x40
         _stream.Read(_char.equippedGearChunk, 0, _char.equippedGearChunk.Length);       // Other equipment - 0x41 - 0x45
         _stream.Read(_char.inventoryChunk, 0, _char.inventoryChunk.Length);             // Inventory - 0x46 - 0x4B
         _stream.Read(_char.equipmentChargesChunk, 0, _char.equipmentChargesChunk.Length); // Equipment Charges - 0x4C - 0x57

         _stream.Read(_char.resistancesChunk, 0, _char.resistancesChunk.Length);         // Resistances 0x58 - 0x67

         _stream.Position += 8;                                                          // UNKNOWN 0x68 - 0x6F

         _stream.Read(_char.questChunk1, 0, _char.questChunk1.Length);                   // Quest #1 Progress? 0x70 - 0x7D

         _stream.Position += 13;                                                         // UNKNOWN 0x71 - 0x7D

         _stream.Read(_char.indexChunk, 0, _char.indexChunk.Length);                     // Character Index number - 0x7E
      }

      static string GetSexFromChunk(Character _char)
      {
         var s = BitConverter.ToString(_char.sexChunk);

         switch (s)
         {
            case "01": return "M";
            case "02": return "F";
            default:
               throw new Exception($"\nUnknown Sex: {s}");
         }
      }

      static string GetAlignmentFromChunk(Character _char)
      {
         var s = BitConverter.ToString(_char.alignmentChunk);

         switch (s)
         {
            case "01": return "Good";
            case "02": return "Neutral";
            case "03": return "Evil";
            default:
               throw new Exception($"Unknown alignment: {s}");
         }
      }

      static string GetRaceFromChunk(Character _char)
      {
         var s = BitConverter.ToString(_char.raceChunk);

         switch (s)
         {
            case "01": return "Human";
            case "02": return "Elf";
            case "03": return "Dwarf";
            case "04": return "Gnome";
            case "05": return "Half-Orc";
            default:
               throw new Exception($"Unknown race: {s}");
         }
      }

      static string GetClassFromChunk(Character _char)
      {
         var s = BitConverter.ToString(_char.classChunk);

         switch (s)
         {
            case "01": return "Knight";
            case "02": return "Paladin";
            case "03": return "Archer";
            case "04": return "Cleric";
            case "05": return "Sorcerer";
            case "06": return "Robber";
            default:
               throw new Exception($"Unknown class: {s}");
         }
      }

      static string GetConditionFromChunk(Character _char)
      {
         var s = BitConverter.ToString(_char.conditionChunk);

         switch (s)
         {
            case "00": return "Good";
            case "01": return "01";
            case "02": return "02";
            case "03": return "03";
            case "04": return "04";
            case "05": return "05";
            default:
               throw new Exception($"Unknown condition: {s}");
         }
      }

      static string GetTownName(Character _char)
      {
         string[] townNames = { "DELETED", "Sorpigal", "Portsmith", "Algary", "Dusk", "Erliquin" };

         var townName = townNames[_char.locationNum];

         return townName;
      }

      static void PrintCharacterHeader()
      {
         Console.WriteLine("#  Name            Sex Alignm. Race     Class    Age Cond. Lvl (XP) Town    ");
         Console.WriteLine("-- --------------- --- ------- -------- -------- --- ----- -------- --------");
      }

      static void PrintCharacterShort(Character _char)
      {
         Console.WriteLine($"{_char.indexNum + 1}  {Encoding.Default.GetString(_char.nameChunk)} {GetSexFromChunk(_char).PadRight(3)} {GetAlignmentFromChunk(_char).PadRight(7)} {GetRaceFromChunk(_char).PadRight(8)} {GetClassFromChunk(_char).PadRight(8)} {_char.ageNum}  {GetConditionFromChunk(_char).PadRight(5)} {_char.levelNum} ({_char.xpNum}) {GetTownName(_char)}");
      }

      static void PrintCharacter(Character _char)
      {
         Console.Clear();

         PrintCharacterHeader();
         PrintCharacterShort(_char);
         Console.WriteLine();

         // HP Current (Modified) / Max
         Console.WriteLine($"Health: {BitConverter.ToUInt16(_char.healthCurrentChunk, 0)} ({BitConverter.ToUInt16(_char.healthModifiedChunk, 0)}) / {BitConverter.ToUInt16(_char.healthMaxChunk, 0)} (AC: {_char.acNum})");

         // MP Current / Max (Spell Level)
         Console.WriteLine($"Magic Points: {BitConverter.ToUInt16(_char.magicPointsCurrentChunk, 0)}/{BitConverter.ToUInt16(_char.magicPointsMaxChunk, 0)} (Spell Level: {_char.spellLvlNum})");

         // Stats 
         Console.WriteLine($"Stats: INT {_char.statsIntellect1}/{_char.statsIntellect2}  MGT {_char.statsMight1}/{_char.statsMight2}  PER {_char.statsPersonality1}/{_char.statsPersonality2}\n       END {_char.statsEndurance1}/{_char.statsEndurance2}  SPD {_char.statsSpeed1}/{_char.statsSpeed2}  ACC {_char.statsAccuracy1}/{_char.statsAccuracy2}  LCK {_char.statsLuck1}/{_char.statsLuck2}");
         Console.WriteLine();

         // Gold Gems Food
         Console.WriteLine($"Gold: {_char.goldNum} / Gems: {BitConverter.ToUInt16(_char.gemsChunk, 0)} / Food: {_char.foodNum}");
         Console.WriteLine();

         // Equipped Weapon
         Console.WriteLine($"Equipped Weapon: {BitConverter.ToString(_char.equippedWeaponChunk)}");

         // Other equipment
         Console.WriteLine($"Equipment: {BitConverter.ToString(_char.equippedGearChunk)}");

         // Backpack
         Console.WriteLine($"Backpack: {BitConverter.ToString(_char.inventoryChunk)}");

         // Equipment Charges 
         Console.WriteLine($"Equipment Charges: {BitConverter.ToString(_char.equipmentChargesChunk)}");
         Console.WriteLine();

         // Resistances 0x58 - 0x67
         Console.WriteLine($"Resistances: Magic  {_char.resMagic1}%/{_char.resMagic2}%  Fire   {_char.resFire1}%/{_char.resFire2}%  Cold   {_char.resCold1}%/{_char.resCold2}%  Elec   {_char.resElec1}%/{_char.resElec2}%\n             Acid   {_char.resAcid1}%/{_char.resAcid2}% Fear   {_char.resFear1}%/{_char.resFear2}% Poison {_char.resPoison1}%/{_char.resPoison2}% Sleep  {_char.resSleep1}%/{_char.resSleep2}%");

         Console.WriteLine($"Quest #1 Progress: {BitConverter.ToString(_char.questChunk1)}");

         Console.WriteLine("----------------------------------------------------------------------------");
      }

      static void PrintCharacterDebug(Character _char)
      {
         
         // Character Name 0x0 - 0xE
         Console.WriteLine($"Name: {Encoding.Default.GetString(_char.nameChunk)}");

         // Sex 0x10
         Console.WriteLine($"\nSex: {GetSexFromChunk(_char)}");

         // UNKNOWN 0x11

         // Alignment 0x12
         Console.WriteLine($"Alignment: {GetAlignmentFromChunk(_char)}");

         // Race 0x13
         Console.WriteLine($"Race: {GetRaceFromChunk(_char)}");

         // Character Class - 0x14
         Console.WriteLine($"Class: {GetClassFromChunk(_char)}");

         // Stats - 0x15 - 0x22
         Console.WriteLine($"Stats\n INT: {_char.statsIntellect1}/{_char.statsIntellect2}  MGT: {_char.statsMight1}/{_char.statsMight2}  PER: {_char.statsPersonality1}/{_char.statsPersonality2}\n END: {_char.statsEndurance1}/{_char.statsEndurance2}  SPD: {_char.statsSpeed1}/{_char.statsSpeed2}  ACC: {_char.statsAccuracy1}/{_char.statsAccuracy2}  LCK: {_char.statsLuck1}/{_char.statsLuck2}");

         // Level - 0x23 - 0x24
         Console.WriteLine($"Level: {_char.levelNum} [{BitConverter.ToString(_char.levelChunk1)}]");

         // Age Offset 37=0x25
         Console.WriteLine($"Age: {_char.ageNum} [{BitConverter.ToString(_char.ageChunk)}]");

         // UNKNOWN - 0x26

         // Experience - Stored as a little-endian UInt24 0x27 - 0x29
         Console.WriteLine($"Experience: {_char.xpNum} [{BitConverter.ToString(_char.xpChunk).Replace("-", " ")}] (UInt24, Length: {_char.xpChunk.Length})");

         // UNKNOWN - 0x2A

         // ---- MAGIC ----------------------------------- //
         // Magic Points - 0x2B - 0x2C
         Console.WriteLine($"Magic Points: {BitConverter.ToUInt16(_char.magicPointsCurrentChunk, 0)} [{BitConverter.ToString(_char.magicPointsCurrentChunk).Replace("-", " ")}]");

         // Magic Points Max - 0x2D - 0x2E
         Console.WriteLine($"Magic Points Max: {BitConverter.ToUInt16(_char.magicPointsMaxChunk, 0)} [{BitConverter.ToString(_char.magicPointsMaxChunk).Replace("-", " ")}]");

         // Spell Level - 0x2F - 0x30
         Console.WriteLine($"Spell Level: {_char.spellLvlNum} [{BitConverter.ToString(_char.spellLevelChunk).Replace("-", " ")}]");
         // ---------------------------------------------- //

         // Gems - Stored as a little-endian ushort  0x31 - 0x32
         Console.WriteLine($"Gems: {BitConverter.ToInt16(_char.gemsChunk, 0)} [{BitConverter.ToString(_char.gemsChunk)}] (ushort, Length: {_char.gemsChunk.Length})");

         // ---- HEALTH ----------------------------------- //
         // Health Points Current - 0x33 - 0x34
         Console.WriteLine($"Health: " + BitConverter.ToUInt16(_char.healthCurrentChunk, 0) + " [" + BitConverter.ToString(_char.healthCurrentChunk) + "]" + " (ushort, Length: " + _char.healthCurrentChunk.Length + ")");

         // Health Points Modified 0x35 - 0x36
         Console.WriteLine($"Health Mod: " + BitConverter.ToUInt16(_char.healthModifiedChunk, 0) + " [" + BitConverter.ToString(_char.healthModifiedChunk) + "]" + " (ushort, Length: " + _char.healthModifiedChunk.Length + ")");

         // Health Points Max - 0x37 - 0x38
         Console.WriteLine($"Max Health: {BitConverter.ToUInt16(_char.healthMaxChunk, 0)} [{BitConverter.ToString(_char.healthMaxChunk)}] (ushort, Length: {_char.healthMaxChunk.Length})");
         // ----------------------------------------------- //

         // Gold - 0x39 - 0x3B
         Console.WriteLine($"Gold: {_char.goldNum} [{BitConverter.ToString(_char.goldChunk).Replace("-", " ")}] (UInt24, Length: {_char.goldChunk.Length})");

         // UNKNOWN 0x3C

         // Armor Class - 0x3D
         Console.WriteLine($"AC: {_char.acNum} [{BitConverter.ToString(_char.armorClassChunk)}]");

         // Food - 0x3E
         Console.WriteLine($"Food: {_char.foodNum} [{BitConverter.ToString(_char.foodChunk)}]");

         // Condition - 0x3F
         Console.WriteLine($"Condition: {GetConditionFromChunk(_char)}");

         // Equipped Weapon - 0x40
         Console.WriteLine($"Equipped Weapon: {BitConverter.ToString(_char.equippedWeaponChunk)}");

         // Other equipment - 0x41 - 0x45
         Console.WriteLine($"Equipment: {BitConverter.ToString(_char.equippedGearChunk)}");

         // Inventory - 0x46 - 0x4B
         Console.WriteLine($"Inventory: {BitConverter.ToString(_char.inventoryChunk)}");

         // Equipment Charges - 0x4C - 0x57
         Console.WriteLine($"Equipment Charges: {BitConverter.ToString(_char.equipmentChargesChunk)}");

         // Resistances 0x58 - 0x67
         Console.WriteLine($"Resistances\n Magic  {_char.resMagic1}%/{_char.resMagic2}%  Fire   {_char.resFire1}%/{_char.resFire2}%  Cold   {_char.resCold1}%/{_char.resCold2}%  Elec   {_char.resElec1}%/{_char.resElec2}%\n Acid   {_char.resAcid1}%/{_char.resAcid2}% Fear   {_char.resFear1}%/{_char.resFear2}% Poison {_char.resPoison1}%/{_char.resPoison2}% Sleep  {_char.resSleep1}%/{_char.resSleep2}%");

         // UNKNOWN 0x68 - 0x7D

         // Character Index number - 0x7E
         Console.WriteLine($"\nCharacter Index: {BitConverter.ToString(_char.indexChunk)}");
      }
   }
}
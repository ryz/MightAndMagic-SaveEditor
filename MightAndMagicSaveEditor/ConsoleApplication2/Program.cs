using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace MightAndMagicSaveEditor
{
   class Program
   {
      static ConsoleKeyInfo userInput;

      static string FILE_NAME = "ROSTER.DTA";
      static string VERSION_NUMBER = "v0.4";

      // Initialize stuff, mostly all the "chunks" as byte arrays

      // There are 18 characters in the file, each one 127 bytes long
      static int[] characterOffset = { 0, 127, 254, 381, 508, 635, 762, 889, 1016, 1143, 1270, 1397, 1524, 1651, 1778, 1905, 2032, 2159 };

      // These are the 18 "character slots" at the very end of the ROSTER.DTA file. They indicate if a character exists (value is 1) or not (value is 0).
      static byte[] characterSlotsChunk = new byte[18]; // Offset 2286=0x8EE
      static int characterSlotsOffset = 2286;

      static string[] characterSlotTowns = { "DELETED", "Sorpigal", "Portsmith", "Algary", "Dusk", "Erliquin" };

      static bool isNewNameValid = false;

      static Character[] characters = new Character[18];



   static void Main(string[] args)
      {

         // create all the character constructors 
         for (int i = 0; i < characters.Length; i++)
         {
            characters[i] = new Character();
            characters[i].offset = characterOffset[i];
         }

         Console.Write($"Opening {FILE_NAME}... ");

         if (File.Exists(FILE_NAME))
         {
            using (var stream = File.Open(FILE_NAME, FileMode.Open, FileAccess.ReadWrite))
            {
               Console.WriteLine("Success!\n");

               do
               {
                  DisplayMenu();
                  userInput = Console.ReadKey(false);

                  switch (userInput.KeyChar.ToString())
                  {
                     case "1":
                        EditAllCharacters(stream);
                        break;
                     case "2":
                        QuickStartPackage(stream, characters[0]);
                        break;
                     case "3":
                     default:
                        break;
                  }
                  Console.Clear();
               }
               while (userInput.Key != ConsoleKey.Escape);
            }
         }
         else
         {
            Console.WriteLine($"\nFile {FILE_NAME} not found! Make sure it's in the same folder as this program.\nAborting.");
            Console.ReadLine();
         }
      }

      public static void DisplayMenu()
      {
         Console.WriteLine($"Might and Magic 1 Save Game Editor ({VERSION_NUMBER}) by ryz");
         Console.WriteLine();
         Console.WriteLine("1. Edit all characters");
         Console.WriteLine("2. Quick Start Package - Give each character XP, Gold and Gems");
         Console.WriteLine();
         Console.WriteLine("Press ESC to exit.");
      }


      // gives each character 5000 XP, 200 Gems and 5000 Gold
      public static void QuickStartPackage(FileStream _stream, Character _char)
      {
         _stream.Position = characterOffset[0];

         // We need to read this chunk first, because we need to check if a given character exists before reading his data.
         _stream.Position = characterSlotsOffset;
         _stream.Read(characterSlotsChunk, 0, characterSlotsChunk.Length);

         // We parse, modify and write back parameters for each character
         for (int i = 0; i < characterOffset.Length; i++)
         {
            Console.WriteLine($"\nReading Slot #{i + 1}...");

            if (characterSlotsChunk[i] == 0)
            {
               Console.WriteLine($"No character found! Skipping to next slot...\n");
            }
            else
            {
               Console.WriteLine($"Character found!\n");
               Console.WriteLine($"Supercharging Character #{i + 1} at Offset {characterOffset[i]}...\n");

               _stream.Position = characterOffset[i];

               ParseCharacter(_stream, characters[i]);
               PrintCharacter(characters[i]);

               ModifyChunkUInt24(characters[i].xpChunk, "XP", 5000);
               ModifyChunkUInt16(characters[i].gemsChunk, "Gems", 200);
               ModifyChunkUInt24(characters[i].goldChunk, "Gold", 5000);


               Console.WriteLine($"Writing new values back to {FILE_NAME}. Are you sure?");
               Console.ReadLine();

               WriteChunk(_stream, "XP", characters[i].xpChunk, characters[i].xpOffset);
               WriteChunk(_stream, "Gems", characters[i].gemsChunk, characters[i].gemsOffset);
               WriteChunk(_stream, "Gold", characters[i].goldChunk, characters[i].goldOffset);


            }
         }

         Console.WriteLine("\nAll done! have fun. Press ENTER to get back to the menu.");
         Console.ReadLine();
      }

      public static void EditAllCharacters(FileStream _stream)
      {

         // We need to read this chunk first, because we need to check if a given character exists before reading his data.
         _stream.Position = characterSlotsOffset;
         _stream.Read(characterSlotsChunk, 0, characterSlotsChunk.Length);

         // We parse, modify and write back parameters for each character
         for (int i = 0; i < characterOffset.Length; i++)
         {

            // First we read the character slot-byte at the end of the file to see if the character exists in the first place.
            // This prevents nasty errors for characters which have been wiped in-game, as the game literally sets every byte of that character to zero in this case.
            Console.WriteLine($"\nReading Slot #{i + 1}...");

            if (characterSlotsChunk[i] == 0)
            {
               Console.WriteLine($"No character found! Skipping to next slot...\n");
            }
            else
            {
               Console.WriteLine($"Character found!\n");
               Console.WriteLine($"Reading Character #{i + 1} at Offset {characterOffset[i]}...\n");

               var town = characterSlotTowns[characterSlotsChunk[i]];
               Console.WriteLine($"This character is located at the Inn of {town}");

               _stream.Position = characterOffset[i];

               ParseCharacter(_stream, characters[0]);
               PrintCharacter(characters[0]);


               // do work on the chunks

               ModifyNameChunk(characters[0].nameChunk, ref isNewNameValid);

               ModifyChunkUInt8(characters[0].sexChunk, "Sex", 1, 2);
               ModifyChunkUInt8(characters[0].alignmentChunk, "Alignment", 1, 3);
               ModifyChunkUInt8(characters[0].raceChunk, "Race", 1, 5);
               ModifyChunkUInt8(characters[0].classChunk, "Class", 1, 5);
               ModifyChunkUInt24(characters[0].xpChunk, "XP");
               ModifyChunkUInt16(characters[0].gemsChunk, "Gems");
               ModifyChunkUInt24(characters[0].goldChunk, "Gold");

               // Write chunks back to the file

               Console.WriteLine($"Writing new values back to {FILE_NAME}. Are you sure?");
               Console.ReadLine();

               if (isNewNameValid)
               {
                  WriteChunk(_stream, "Name", characters[0].nameChunk, characters[0].nameOffset);
               }

               WriteChunk(_stream, "Sex", characters[0].sexChunk, characters[0].sexOffset);
               WriteChunk(_stream, "Alignment", characters[0].alignmentChunk, characters[0].alignmentOffset);
               WriteChunk(_stream, "Race", characters[0].raceChunk, characters[0].raceOffset);
               WriteChunk(_stream, "Class", characters[0].classChunk, characters[0].classOffset);
               WriteChunk(_stream, "XP", characters[0].xpChunk, characters[0].xpOffset);
               WriteChunk(_stream, "Gems", characters[0].gemsChunk, characters[0].gemsOffset);
               WriteChunk(_stream, "Gold", characters[0].goldChunk, characters[0].goldOffset);

               Console.WriteLine($"\nCharacter #{i + 1} done!\n");
            }


         }

         Console.WriteLine("\nAll done!");
         Console.ReadLine();
      }

      public static void ModifyChunkUInt8(byte[] _chunk, string _chunkName, uint _min, uint _max)
      {
         Console.Write($"\nEnter new {_chunkName} value ({_min}-{_max}): ");
         byte newVal = Byte.Parse(Console.ReadLine());

         byte[] newArray = new byte[1];

         newArray[0] = newVal;

         Array.Clear(_chunk, 0, _chunk.Length);
         Array.Copy(newArray, _chunk, newArray.Length);
      }

      // Set by input
      public static void ModifyChunkUInt16(byte[] _chunk, string _chunkName)
      {
         Console.Write($"\nEnter new {_chunkName} value (UInt16): ");
         ushort newVal = UInt16.Parse(Console.ReadLine());

         byte[] newArray = BitConverter.GetBytes(newVal);

         Array.Clear(_chunk, 0, _chunk.Length);
         Array.Copy(newArray, _chunk, 2);
      }

      // Set directly
      public static void ModifyChunkUInt16(byte[] _chunk, string _chunkName, ushort _amount)
      {
         ushort newVal = _amount;

         byte[] newArray = BitConverter.GetBytes(newVal);

         Array.Clear(_chunk, 0, _chunk.Length);
         Array.Copy(newArray, _chunk, 2);
      }

      // Set by input
      public static void ModifyChunkUInt24(byte[] _chunk, string _chunkName)
      {
         Console.Write($"\nEnter new {_chunkName} value (UInt24): ");
         
         // Although this should be a UInt24, we read this as a UInt32 and later just copy three bytes
         uint newVal = UInt32.Parse(Console.ReadLine());

         // We have to create an intermediate byte array here where we copy to and from
         // This is because BitConverter.GetBytes() can't return UInt24
         // which would result in a too long chunk (byte array). 

         byte[] newArray = BitConverter.GetBytes(newVal);

         Array.Clear(_chunk, 0, _chunk.Length);

         // Remember to copy just three bytes > UInt24
         Array.Copy(newArray, _chunk, 3);
      }

      // Set directly
      public static void ModifyChunkUInt24(byte[] _chunk, string _chunkName, uint _amount)
      {;
         // Although this should be a UInt24, we read this as a UInt32 and later just copy three bytes
         uint newVal = _amount;

         // We have to create an intermediate byte array here where we copy to and from
         // This is because BitConverter.GetBytes() can't return UInt24
         // which would result in a too long chunk (byte array). 

         byte[] newArray = BitConverter.GetBytes(newVal);

         Array.Clear(_chunk, 0, _chunk.Length);

         // Remember to copy just three bytes > UInt24
         Array.Copy(newArray, _chunk, 3);
      }

      // Write chunk back from byte array
      public static void WriteChunk(Stream _stream, string _chunkName, byte[] _chunk, int _offset)
      {
         _stream.Seek(_offset, SeekOrigin.Begin);
         _stream.Write(_chunk, 0, _chunk.Length);
      }

      // Write chunk back from single byte
      public static void WriteChunk(Stream _stream, string _chunkName, byte _byte, int _offset)
      {
         _stream.Seek(_offset, SeekOrigin.Begin);
         _stream.WriteByte(_byte);
      }


      public static void ModifyNameChunk(byte[] _name, ref bool _nameValid)
      {
         byte[] newName = new byte[_name.Length];

         //ask for and get new name from input and save it into a byte array
         Console.Write("\nEnter a new Name: ");
         string nameInput = Console.ReadLine().ToUpper();

         // Truncate input above 15 characters
         nameInput = nameInput.Substring(0, Math.Min(15, nameInput.Length));

         // Check that the name contains only uppercase latin characters and numerals from 0-9
         Regex rx = new Regex("^[A-Z0-9]*$");

         if (rx.IsMatch(nameInput))
         {
            Console.Write($"Name '{nameInput}' accepted.\n");
            newName = Encoding.ASCII.GetBytes(nameInput);

            _nameValid = true;
         }
         else
         {
            Console.Write($"Name '{nameInput}' contains invalid characters! Name has not been changed.\n");
            _nameValid = false;
         }

         // we clear the old array first so we can just copy the new one in it - keeps array size the same
         Array.Clear(_name, 0, _name.Length);
         Array.Copy(newName, _name, newName.Length);
      }

      public static void ParseCharacter(FileStream _stream, Character _char)
      {
         _stream.Read(_char.nameChunk, 0, _char.nameChunk.Length);                       // Character Name 0x0 - 0xE
         _stream.Position += 1;                                              // UNKNOWN 0xF

         _stream.Read(_char.sexChunk, 0, _char.sexChunk.Length);                         // Sex 0x10
         _stream.Position += 1;                                              // UNKNOWN 0x11

         _stream.Read(_char.alignmentChunk, 0, _char.alignmentChunk.Length);             // Alignment 0x12
         _stream.Read(_char.raceChunk, 0, _char.raceChunk.Length);                       // Race 0x13
         _stream.Read(_char.classChunk, 0, _char.classChunk.Length);                     // Character Class - 0x14
         _stream.Read(_char.statsChunk, 0, _char.statsChunk.Length);                     // Stats - 0x15 - 0x22
         _stream.Read(_char.levelChunk1, 0, _char.levelChunk1.Length);                   // Level - 0x23 - 0x24
         _stream.Position += 1;
         _stream.Read(_char.ageChunk, 0, _char.ageChunk.Length);                         // Age Offset 37=0x25
         _stream.Position += 1;                                              // UNKNOWN - 0x26

         _stream.Read(_char.xpChunk, 0, _char.xpChunk.Length);                           // Experience - UInt24 0x27 - 0x29
         _stream.Position += 1;                                              // UNKNOWN - 0x2A

         _stream.Read(_char.magicPointsCurrentChunk, 0, _char.magicPointsCurrentChunk.Length); // Magic Points - 0x2B - 0x2C
         _stream.Read(_char.magicPointsMaxChunk, 0, _char.magicPointsMaxChunk.Length);   // Magic Points Max - 0x2D - 0x2E
         _stream.Read(_char.spellLevelChunk, 0, _char.spellLevelChunk.Length);           // Spell Level - 0x2F - 0x30

         _stream.Read(_char.gemsChunk, 0, _char.gemsChunk.Length);                       // Gems - ushort  0x31 - 0x32

         _stream.Read(_char.healthCurrentChunk, 0, _char.healthCurrentChunk.Length);     // Health Points Current - 0x33 - 0x34
         _stream.Read(_char.healthModifiedChunk, 0, _char.healthModifiedChunk.Length);   // Health Points Modified 0x35 - 0x36
         _stream.Read(_char.healthMaxChunk, 0, _char.healthMaxChunk.Length);             // Health Points Max - 0x37 - 0x38

         _stream.Read(_char.goldChunk, 0, _char.goldChunk.Length);                       // Gold - 0x39 - 0x3B
         _stream.Position += 1;                                              // UNKNOWN 0x3C

         _stream.Read(_char.armorClassChunk, 0, _char.armorClassChunk.Length);           // Armor Class - 0x3D
         _stream.Read(_char.foodChunk, 0, _char.foodChunk.Length);                       // Food - 0x3E
         _stream.Read(_char.conditionChunk, 0, _char.conditionChunk.Length);             // Condition - 0x3F

         _stream.Read(_char.equippedWeaponChunk, 0, _char.equippedWeaponChunk.Length);   // Equipped Weapon - 0x40
         _stream.Read(_char.equippedGearChunk, 0, _char.equippedGearChunk.Length);       // Other equipment - 0x41 - 0x45
         _stream.Read(_char.inventoryChunk, 0, _char.inventoryChunk.Length);             // Inventory - 0x46 - 0x4B
         _stream.Read(_char.equipmentChargesChunk, 0, _char.equipmentChargesChunk.Length); // Equipment Charges - 0x4C - 0x57

         _stream.Read(_char.resistancesChunk, 0, _char.resistancesChunk.Length);         // Resistances 0x58 - 0x67
         _stream.Position += 22;                                             // UNKNOWN 0x68 - 0x7D

         _stream.Read(_char.characterIndexChunk, 0, _char.characterIndexChunk.Length);   // Character Index number - 0x7E
      }

      public static void PrintCharacter(Character _char)
      {
         // Character Name 0x0 - 0xE
         //encode and print the byte array to a string so that we can debug it
         Console.WriteLine($"Name: {Encoding.Default.GetString(_char.nameChunk)} (Length: {_char.nameChunk.Length})");

         // UNKNOWN 0xF, 

         // Sex 0x10

         var sexS = BitConverter.ToString(_char.sexChunk);

         switch (sexS)
         {
            case "01":
               Console.WriteLine($"\nSex: Male ({sexS})");
               break;
            case "02":
               Console.WriteLine($"\nSex: Female ({sexS})");
               break;
            default:
               throw new Exception($"\nUnknown Sex: {sexS}");
         }

         // UNKNOWN 0x11

         // Alignment 0x12

         var alignmentS = BitConverter.ToString(_char.alignmentChunk);

         switch (alignmentS)
         {
            case "01":
               Console.WriteLine($"Alignment: Good ({alignmentS})");
               break;
            case "02":
               Console.WriteLine($"Alignment: Neutral ({alignmentS})");
               break;
            case "03":
               Console.WriteLine($"Alignment: Evil ({alignmentS})");
               break;
            default:
               //throw new Exception($"Unknown alignment: {alignmentS}");
               Console.WriteLine($"Unknown alignment: {alignmentS}");
               break;

         }
         // Race 0x13

         var raceS = BitConverter.ToString(_char.raceChunk);

         switch (raceS)
         {
            case "01":
               Console.WriteLine($"Race: Human ({raceS})");
               break;
            case "02":
               Console.WriteLine($"Race: Elf ({raceS})");
               break;
            case "03":
               Console.WriteLine($"Race: Dwarf ({raceS})");
               break;
            case "04":
               Console.WriteLine($"Race: Gnome ({raceS})");
               break;
            case "05":
               Console.WriteLine($"Race: Half-Orc ({raceS})");
               break;
            default:
               throw new Exception($"\nUnknown race: {raceS}");

         }

         // Character Class - 0x14

         var classS = BitConverter.ToString(_char.classChunk);

         switch (classS)
         {
            case "01":
               Console.WriteLine($"Class: Knight ({classS})");
               break;
            case "02":
               Console.WriteLine($"Class: Paladin ({classS})");
               break;
            case "03":
               Console.WriteLine($"Class: Archer ({classS})");
               break;
            case "04":
               Console.WriteLine($"Class: Cleric ({classS})");
               break;
            case "05":
               Console.WriteLine($"Class: Sorcerer ({classS})");
               break;
            case "06":
               Console.WriteLine($"Class: Robber ({classS})");
               break;
            default:
               throw new Exception($"Unknown class: {classS}");
         }

         // Stats - 0x15 - 0x22

         int statsIntellect1 = _char.statsChunk[0];
         int statsIntellect2 = _char.statsChunk[1];
         int statsMight1 = _char.statsChunk[2];
         int statsMight2 = _char.statsChunk[3];
         int statsPersonality1 = _char.statsChunk[4];
         int statsPersonality2 = _char.statsChunk[5];
         int statsEndurance1 = _char.statsChunk[6];
         int statsEndurance2 = _char.statsChunk[7];
         int statsSpeed1 = _char.statsChunk[8];
         int statsSpeed2 = _char.statsChunk[9];
         int statsAccuracy1 = _char.statsChunk[10];
         int statsAccuracy2 = _char.statsChunk[11];
         int statsLuck1 = _char.statsChunk[12];
         int statsLuck2 = _char.statsChunk[13];

         Console.WriteLine($"Stats\n INT: {statsIntellect1}/{statsIntellect2}  MGT: {statsMight1}/{statsMight2}  PER: {statsPersonality1}/{statsPersonality2}\n END: {statsEndurance1}/{statsEndurance2}  SPD: {statsSpeed1}/{statsSpeed2}  ACC: {statsAccuracy1}/{statsAccuracy2}  LCK: {statsLuck1}/{statsLuck2}");

         // Level - 0x23 - 0x24
         int levelNum = _char.levelChunk1[0];
         Console.WriteLine($"Level: {levelNum} [{BitConverter.ToString(_char.levelChunk1)}]");

         // Age Offset 37=0x25
         int ageNum = _char.ageChunk[0];
         Console.WriteLine($"Age: {ageNum} [{BitConverter.ToString(_char.ageChunk)}]");

         // UNKNOWN - 0x26

         // Experience - Stored as a little-endian UInt24 0x27 - 0x29
         int expNum = (_char.xpChunk[2] << 16) | (_char.xpChunk[1] << 8) | _char.xpChunk[0];
         Console.WriteLine($"Experience: {expNum} [{BitConverter.ToString(_char.xpChunk).Replace("-", " ")}] (UInt24, Length: {_char.xpChunk.Length})");

         // UNKNOWN - 0x2A

         // ---- MAGIC ----------------------------------- //
         // Magic Points - 0x2B - 0x2C
         Console.WriteLine($"Magic Points: {BitConverter.ToUInt16(_char.magicPointsCurrentChunk, 0)} [{BitConverter.ToString(_char.magicPointsCurrentChunk).Replace("-", " ")}]");

         // Magic Points Max - 0x2D - 0x2E
         Console.WriteLine($"Magic Points Max: {BitConverter.ToUInt16(_char.magicPointsMaxChunk, 0)} [{BitConverter.ToString(_char.magicPointsMaxChunk).Replace("-", " ")}]");

         // Spell Level - 0x2F - 0x30
         int spellLvlNum = _char.spellLevelChunk[0];
         Console.WriteLine($"Spell Level: {spellLvlNum} [{BitConverter.ToString(_char.spellLevelChunk).Replace("-", " ")}]");
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
         int goldNum = (_char.goldChunk[2] << 16) | (_char.goldChunk[1] << 8) | _char.goldChunk[0];
         Console.WriteLine($"Gold: {goldNum} [{BitConverter.ToString(_char.goldChunk).Replace("-", " ")}] (UInt24, Length: {_char.goldChunk.Length})");

         // UNKNOWN 0x3C

         // Armor Class - 0x3D
         int acNum = _char.armorClassChunk[0];
         Console.WriteLine($"AC: {acNum} [{BitConverter.ToString(_char.armorClassChunk)}]");

         // Food - 0x3E
         int foodNum = _char.foodChunk[0];
         Console.WriteLine($"Food: {foodNum} [{BitConverter.ToString(_char.foodChunk)}]");

         // Condition - 0x3F
         var conditionS = BitConverter.ToString(_char.conditionChunk);

         switch (conditionS)
         {
            case "00":
               Console.WriteLine($"Condition: Good ({conditionS})");
               break;
            case "01":
               Console.WriteLine($"Condition: 01 ({conditionS})");
               break;
            case "02":
               Console.WriteLine($"Condition: 02 ({conditionS})");
               break;
            case "03":
               Console.WriteLine($"Condition: 03 ({conditionS})");
               break;
            case "04":
               Console.WriteLine($"Condition: 04 ({conditionS})");
               break;
            case "05":
               Console.WriteLine($"Condition: 05 ({conditionS})");
               break;
            default:
               throw new Exception($"Unknown condition: {conditionS}");
         }

         // Equipped Weapon - 0x40
         Console.WriteLine($"Equipped Weapon: {BitConverter.ToString(_char.equippedWeaponChunk)}");

         // Other equipment - 0x41 - 0x45
         Console.WriteLine($"Equipment: {BitConverter.ToString(_char.equippedGearChunk)}");

         // Inventory - 0x46 - 0x4B
         Console.WriteLine($"Inventory: {BitConverter.ToString(_char.inventoryChunk)}");

         // Equipment Charges - 0x4C - 0x57
         Console.WriteLine($"Equipment Charges: {BitConverter.ToString(_char.equipmentChargesChunk)}");

         // Resistances 0x58 - 0x67

         int resMagic1 = _char.resistancesChunk[0];
         int resMagic2 = _char.resistancesChunk[1];
         int resFire1 = _char.resistancesChunk[2];
         int resFire2 = _char.resistancesChunk[3];
         int resCold1 = _char.resistancesChunk[4];
         int resCold2 = _char.resistancesChunk[5];
         int resElec1 = _char.resistancesChunk[6];
         int resElec2 = _char.resistancesChunk[7];
         int resAcid1 = _char.resistancesChunk[8];
         int resAcid2 = _char.resistancesChunk[9];
         int resFear1 = _char.resistancesChunk[10];
         int resFear2 = _char.resistancesChunk[11];
         int resPoison1 = _char.resistancesChunk[12];
         int resPoison2 = _char.resistancesChunk[13];
         int resSleep1 = _char.resistancesChunk[14];
         int resSleep2 = _char.resistancesChunk[15];

         Console.WriteLine($"Resistances\n Magic  {resMagic1}%/{resMagic2}%  Fire   {resFire1}%/{resFire2}%  Cold   {resCold1}%/{resCold2}%  Elec   {resElec1}%/{resElec2}%\n Acid   {resAcid1}%/{resAcid2}% Fear   {resFear1}%/{resFear2}% Poison {resPoison1}%/{resPoison2}% Sleep  {resSleep1}%/{resSleep2}%");

         // UNKNOWN 0x68 - 0x7D

         // Character Index number - 0x7E
         Console.WriteLine($"\nCharacter Index: {BitConverter.ToString(_char.characterIndexChunk)}");
      }
   }
   class Character
   {
      public int offset { get; set; } = 0;
      public byte[] nameChunk { get; set; } = new byte[15]; // Offset 0=0x0

      public int nameOffset
      {
         get
         {
            return offset;
         }
         set
         {
            offset = value;
         }
      }


      public byte[] unknownChunk1 { get; set; } = new byte[1]; // Offset 15=0xF

      public byte[] sexChunk { get; set; } = new byte[1]; // Offset 16=0x10

      public int sexOffset
      {
         get
         {
            return offset + 16;
         }
         set
         {
            offset = value - 16;
         }
      }

      public byte[] unknownChunk2 { get; set; } = new byte[1]; // Offset 17=0x11

      public byte[] alignmentChunk { get; set; } = new byte[1]; // Offset 18=0x12
      public int alignmentOffset
      {
         get
         {
            return offset + 18;
         }
         set
         {
            offset = value - 18;
         }
      }

      public byte[] raceChunk { get; set; } = new byte[1]; // Offset 19=0x13
      public int raceOffset
      {
         get
         {
            return offset + 19;
         }
         set
         {
            offset = value - 19;
         }
      }

      public byte[] classChunk { get; set; } = new byte[1]; // Offset 20=0x14
      public int classOffset
      { get
         {
            return offset + 20;
         }
         set
         {
            offset = value - 20;
         }
      }

      // Stats, there are seven statistics for each character, two bytes each.
      public byte[] statsChunk { get; set; } = new byte[14]; // Offset 21=0x15

      public byte[] levelChunk1 { get; set; } = new byte[1]; // Offset 35=0x23
      public byte[] levelChunk2 { get; set; } = new byte[1]; // Offset 36=0x24

      public byte[] ageChunk { get; set; } = new byte[1]; // Offset 37=0x25

      public byte[] unknownChunk3 { get; set; } = new byte[1];  // Offset 38=0x26

      // XP, stored as UInt24
      public byte[] xpChunk { get; set; } = new byte[3]; // Offset 39=0x27
      public int xpOffset
      {
         get
         {
            return offset + 39;
         }
         set
         {
            offset = value - 39;
         }
      }

      public byte[] unknownChunk4 { get; set; } = new byte[1]; // Offset 42=0x2A

      public byte[] magicPointsCurrentChunk { get; set; } = new byte[2]; // Offset 43=0x2B
      public byte[] magicPointsMaxChunk { get; set; } = new byte[2]; // Offset 45=0x2D

      public byte[] spellLevelChunk { get; set; } = new byte[2]; // Offset 47=0x2F

      public byte[] gemsChunk { get; set; } = new byte[2]; // Offset 49=0x31
      public int gemsOffset
      {
         get
         {
            return offset + 49;
         }
         set
         {
            offset = value - 49;
         }
      }

      public byte[] healthCurrentChunk { get; set; } = new byte[2]; // Offset 51=0x33
      public byte[] healthModifiedChunk { get; set; } = new byte[2]; // Offset 53
      public byte[] healthMaxChunk { get; set; } = new byte[2]; // Offset 55

      public byte[] goldChunk { get; set; } = new byte[3];  // Offset 57=0x39
      public int goldOffset
      {
         get
         {
            return offset + 57;
         }
         set
         {
            offset = value - 57;
         }
      }


      public byte[] unknownChunk7 { get; set; } = new byte[1]; // Offset 58=0x3A

      public byte[] armorClassChunk { get; set; } = new byte[1]; // Offset 62=0x3D

      public byte[] foodChunk { get; set; } = new byte[1]; // Offset 62=0x3E

      public byte[] conditionChunk { get; set; } = new byte[1]; // Offset 63=0x3F

      public byte[] equippedWeaponChunk { get; set; } = new byte[1]; // Offset 64=0x40
      public byte[] equippedGearChunk { get; set; } = new byte[5]; // Offset 65=0x41
      public byte[] inventoryChunk { get; set; } = new byte[6]; // Offset 70=0x46

      public byte[] equipmentChargesChunk { get; set; } = new byte[12];// Offset 76=0x4C 

      public byte[] resistancesChunk { get; set; } = new byte[16]; // Offset 88=0x58

      public byte[] unknownChunk8 { get; set; } = new byte[22]; // Offset 104=0x68 - biggest chunk, probably contains various progress/quest-related data

      public byte[] characterIndexChunk { get; set; } = new byte[1]; // Offset 126=0x7E

   }
}
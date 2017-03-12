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

      // Initialize stuff, mostly all the "chunks" as byte arrays

      // There are 18 characters in the file, each one 127 bytes long
      static uint[] characterOffset = { 0, 127, 254, 381, 508, 635, 762, 889, 1016, 1143, 1270, 1397, 1524, 1651, 1778, 1905, 2032, 2159 };

      // Character Name
      static byte[] nameChunk = new byte[15]; // Offset 0=0x0
      static int nameOffset = 0;
      static bool isNewNameValid = false;

      static byte[] unknownChunk1 = new byte[1]; // Offset 15=0xF

      static byte[] sexChunk = new byte[1]; // Offset 16=0x10
      static int sexOffset = 16;

      static byte[] unknownChunk2 = new byte[1]; // Offset 17=0x11

      static byte[] alignmentChunk = new byte[1]; // Offset 18=0x12
      static int alignmentOffset = 18;

      static byte[] raceChunk = new byte[1]; // Offset 19=0x13
      static int raceOffset = 19;

      static byte[] classChunk = new byte[1]; // Offset 20=0x14
      static int classOffset = 20;

      // Stats, there are seven statistics for each character, two bytes each.
      static byte[] statsChunk = new byte[14]; // Offset 21=0x15

      static byte[] levelChunk1 = new byte[1]; // Offset 35=0x23
      static byte[] levelChunk2 = new byte[1]; // Offset 36=0x24

      static byte[] ageChunk = new byte[1]; // Offset 37=0x25

      static byte[] unknownChunk3 = new byte[1];  // Offset 38=0x26

      // XP, stored as UInt24
      static byte[] xpChunk = new byte[3]; // Offset 39=0x27
      static int xpOffset = 39;

      static byte[] unknownChunk4 = new byte[1]; // Offset 42=0x2A

      static byte[] magicPointsCurrentChunk = new byte[2]; // Offset 43=0x2B
      static byte[] magicPointsMaxChunk = new byte[2]; // Offset 45=0x2D

      static byte[] spellLevelChunk = new byte[2]; // Offset 47=0x2F

      static byte[] gemsChunk = new byte[2]; // Offset 49=0x31
      static int gemsOffset = 49;

      static byte[] healthCurrentChunk = new byte[2]; // Offset 51=0x33
      static byte[] healthModifiedChunk = new byte[2]; // Offset 53
      static byte[] healthMaxChunk = new byte[2]; // Offset 55

      static byte[] goldChunk = new byte[3];  // Offset 57=0x39
      static int goldOffset = 57;

      static byte[] unknownChunk7 = new byte[1]; // Offset 58=0x3A

      static byte[] armorClassChunk = new byte[1]; // Offset 62=0x3D

      static byte[] foodChunk = new byte[1]; // Offset 62=0x3E

      static byte[] conditionChunk = new byte[1]; // Offset 63=0x3F

      static byte[] equippedWeaponChunk = new byte[1]; // Offset 64=0x40
      static byte[] equippedGearChunk = new byte[5]; // Offset 65=0x41
      static byte[] inventoryChunk = new byte[6]; // Offset 70=0x46

      static byte[] equipmentChargesChunk = new byte[12];// Offset 76=0x4C 

      static byte[] resistancesChunk = new byte[16]; // Offset 88=0x58

      static byte[] unknownChunk8 = new byte[22]; // Offset 104=0x68 - biggest chunk, probably contains various progress/quest-related data

      static byte[] characterIndexChunk = new byte[1]; // Offset 126=0x7E

      // These are the 18 "character slots" at the very end of the ROSTER.DTA file. They indicate if a character exists (value is 1) or not (value is 0).
      static byte[] characterSlotsChunk = new byte[18]; // Offset 2286=0x8EE
      static int characterSlotsOffset = 2286;

      static string[] characterSlotTowns = { "DELETED", "Sorpigal", "Portsmith", "Algary", "Dusk", "Erliquin" };

      static void Main(string[] args)
      {
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
         Console.WriteLine("Might and Magic 1 Save Game Editor (v0.3) by ryz");
         Console.WriteLine();
         Console.WriteLine("1. Edit all characters");
         Console.WriteLine();
         Console.WriteLine("Press ESC to exit.");
      }

      public static void EditAllCharacters(FileStream _stream)
      {
         _stream.Position = characterOffset[0];

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

               ParseCharacter(_stream);
               PrintCharacter();


               // do work on the chunks

               ModifyNameChunk(nameChunk, ref isNewNameValid);

               ModifyChunkUInt8(sexChunk, "Sex", 1, 2);
               ModifyChunkUInt8(alignmentChunk, "Alignment", 1, 3);
               ModifyChunkUInt8(raceChunk, "Race", 1, 5);
               ModifyChunkUInt8(classChunk, "Class", 1, 5);
               ModifyChunkUInt24(xpChunk, "XP");
               ModifyChunkUInt16(gemsChunk, "Gems");
               ModifyChunkUInt24(goldChunk, "Gold");

               // Write chunks back to the file

               if (isNewNameValid)
               {
                  WriteChunk(_stream, "Name", nameChunk, nameOffset, FILE_NAME);
               }

               WriteChunk(_stream, "Sex", sexChunk, sexOffset, FILE_NAME);
               WriteChunk(_stream, "Alignment", alignmentChunk, alignmentOffset, FILE_NAME);
               WriteChunk(_stream, "Race", raceChunk, raceOffset, FILE_NAME);
               WriteChunk(_stream, "Class", classChunk, classOffset, FILE_NAME);
               WriteChunk(_stream, "XP", xpChunk, xpOffset, FILE_NAME);
               WriteChunk(_stream, "Gems", gemsChunk, gemsOffset, FILE_NAME);
               WriteChunk(_stream, "Gold", goldChunk, goldOffset, FILE_NAME);

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

      public static void ModifyChunkUInt16(byte[] _chunk, string _chunkName)
      {
         Console.Write($"\nEnter new {_chunkName} value (UInt16): ");
         ushort newVal = UInt16.Parse(Console.ReadLine());

         byte[] newArray = BitConverter.GetBytes(newVal);

         Array.Clear(_chunk, 0, _chunk.Length);
         Array.Copy(newArray, _chunk, 2);


      }

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

      // Write chunk back from byte array
      public static void WriteChunk(Stream _stream, string _chunkName, byte[] _chunk, int _offset, string _filename)
      {
         Console.WriteLine($"Writing new {_chunkName} value back to {_filename}. Are you sure?");
         Console.ReadLine();
         _stream.Seek(_offset, SeekOrigin.Begin);
         _stream.Write(_chunk, 0, _chunk.Length);
      }

      // Write chunk back from single byte
      public static void WriteChunk(Stream _stream, string _chunkName, byte _byte, int _offset, string _filename)
      {
         Console.WriteLine($"Writing new {_chunkName} value back to {_filename}. Are you sure?");
         Console.ReadLine();
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

      public static void ParseCharacter(FileStream _stream)
      {
         _stream.Read(nameChunk, 0, nameChunk.Length);                       // Character Name 0x0 - 0xE
         _stream.Position += 1;                                              // UNKNOWN 0xF

         _stream.Read(sexChunk, 0, sexChunk.Length);                         // Sex 0x10
         _stream.Position += 1;                                              // UNKNOWN 0x11

         _stream.Read(alignmentChunk, 0, alignmentChunk.Length);             // Alignment 0x12
         _stream.Read(raceChunk, 0, raceChunk.Length);                       // Race 0x13
         _stream.Read(classChunk, 0, classChunk.Length);                     // Character Class - 0x14
         _stream.Read(statsChunk, 0, statsChunk.Length);                     // Stats - 0x15 - 0x22
         _stream.Read(levelChunk1, 0, levelChunk1.Length);                   // Level - 0x23 - 0x24
         _stream.Position += 1;
         _stream.Read(ageChunk, 0, ageChunk.Length);                         // Age Offset 37=0x25
         _stream.Position += 1;                                              // UNKNOWN - 0x26

         _stream.Read(xpChunk, 0, xpChunk.Length);                           // Experience - UInt24 0x27 - 0x29
         _stream.Position += 1;                                              // UNKNOWN - 0x2A

         _stream.Read(magicPointsCurrentChunk, 0, magicPointsCurrentChunk.Length); // Magic Points - 0x2B - 0x2C
         _stream.Read(magicPointsMaxChunk, 0, magicPointsMaxChunk.Length);   // Magic Points Max - 0x2D - 0x2E
         _stream.Read(spellLevelChunk, 0, spellLevelChunk.Length);           // Spell Level - 0x2F - 0x30

         _stream.Read(gemsChunk, 0, gemsChunk.Length);                       // Gems - ushort  0x31 - 0x32

         _stream.Read(healthCurrentChunk, 0, healthCurrentChunk.Length);     // Health Points Current - 0x33 - 0x34
         _stream.Read(healthModifiedChunk, 0, healthModifiedChunk.Length);   // Health Points Modified 0x35 - 0x36
         _stream.Read(healthMaxChunk, 0, healthMaxChunk.Length);             // Health Points Max - 0x37 - 0x38

         _stream.Read(goldChunk, 0, goldChunk.Length);                       // Gold - 0x39 - 0x3B
         _stream.Position += 1;                                              // UNKNOWN 0x3C

         _stream.Read(armorClassChunk, 0, armorClassChunk.Length);           // Armor Class - 0x3D
         _stream.Read(foodChunk, 0, foodChunk.Length);                       // Food - 0x3E
         _stream.Read(conditionChunk, 0, conditionChunk.Length);             // Condition - 0x3F

         _stream.Read(equippedWeaponChunk, 0, equippedWeaponChunk.Length);   // Equipped Weapon - 0x40
         _stream.Read(equippedGearChunk, 0, equippedGearChunk.Length);       // Other equipment - 0x41 - 0x45
         _stream.Read(inventoryChunk, 0, inventoryChunk.Length);             // Inventory - 0x46 - 0x4B
         _stream.Read(equipmentChargesChunk, 0, equipmentChargesChunk.Length); // Equipment Charges - 0x4C - 0x57

         _stream.Read(resistancesChunk, 0, resistancesChunk.Length);         // Resistances 0x58 - 0x67
         _stream.Position += 22;                                             // UNKNOWN 0x68 - 0x7D

         _stream.Read(characterIndexChunk, 0, characterIndexChunk.Length);   // Character Index number - 0x7E
      }

      public static void PrintCharacter()
      {
         // Character Name 0x0 - 0xE
         //encode and print the byte array to a string so that we can debug it
         Console.WriteLine($"Name: {Encoding.Default.GetString(nameChunk)} (Length: {nameChunk.Length})");

         // UNKNOWN 0xF, 

         // Sex 0x10

         var sexS = BitConverter.ToString(sexChunk);

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

         var alignmentS = BitConverter.ToString(alignmentChunk);

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

         var raceS = BitConverter.ToString(raceChunk);

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

         var classS = BitConverter.ToString(classChunk);

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

         int statsIntellect1 = statsChunk[0];
         int statsIntellect2 = statsChunk[1];
         int statsMight1 = statsChunk[2];
         int statsMight2 = statsChunk[3];
         int statsPersonality1 = statsChunk[4];
         int statsPersonality2 = statsChunk[5];
         int statsEndurance1 = statsChunk[6];
         int statsEndurance2 = statsChunk[7];
         int statsSpeed1 = statsChunk[8];
         int statsSpeed2 = statsChunk[9];
         int statsAccuracy1 = statsChunk[10];
         int statsAccuracy2 = statsChunk[11];
         int statsLuck1 = statsChunk[12];
         int statsLuck2 = statsChunk[13];

         Console.WriteLine($"Stats\n INT: {statsIntellect1}/{statsIntellect2}  MGT: {statsMight1}/{statsMight2}  PER: {statsPersonality1}/{statsPersonality2}\n END: {statsEndurance1}/{statsEndurance2}  SPD: {statsSpeed1}/{statsSpeed2}  ACC: {statsAccuracy1}/{statsAccuracy2}  LCK: {statsLuck1}/{statsLuck2}");

         // Level - 0x23 - 0x24
         int levelNum = levelChunk1[0];
         Console.WriteLine($"Level: {levelNum} [{BitConverter.ToString(levelChunk1)}]");

         // Age Offset 37=0x25
         int ageNum = ageChunk[0];
         Console.WriteLine($"Age: {ageNum} [{BitConverter.ToString(ageChunk)}]");

         // UNKNOWN - 0x26

         // Experience - Stored as a little-endian UInt24 0x27 - 0x29
         int expNum = (xpChunk[2] << 16) | (xpChunk[1] << 8) | xpChunk[0];
         Console.WriteLine($"Experience: {expNum} [{BitConverter.ToString(xpChunk).Replace("-", " ")}] (UInt24, Length: {xpChunk.Length})");

         // UNKNOWN - 0x2A

         // ---- MAGIC ----------------------------------- //
         // Magic Points - 0x2B - 0x2C
         Console.WriteLine($"Magic Points: {BitConverter.ToUInt16(magicPointsCurrentChunk, 0)} [{BitConverter.ToString(magicPointsCurrentChunk).Replace("-", " ")}]");

         // Magic Points Max - 0x2D - 0x2E
         Console.WriteLine($"Magic Points Max: {BitConverter.ToUInt16(magicPointsMaxChunk, 0)} [{BitConverter.ToString(magicPointsMaxChunk).Replace("-", " ")}]");

         // Spell Level - 0x2F - 0x30
         int spellLvlNum = spellLevelChunk[0];
         Console.WriteLine($"Spell Level: {spellLvlNum} [{BitConverter.ToString(spellLevelChunk).Replace("-", " ")}]");
         // ---------------------------------------------- //

         // Gems - Stored as a little-endian ushort  0x31 - 0x32
         Console.WriteLine($"Gems: {BitConverter.ToInt16(gemsChunk, 0)} [{BitConverter.ToString(gemsChunk)}] (ushort, Length: {gemsChunk.Length})");

         // ---- HEALTH ----------------------------------- //
         // Health Points Current - 0x33 - 0x34
         Console.WriteLine($"Health: " + BitConverter.ToUInt16(healthCurrentChunk, 0) + " [" + BitConverter.ToString(healthCurrentChunk) + "]" + " (ushort, Length: " + healthCurrentChunk.Length + ")");

         // Health Points Modified 0x35 - 0x36
         Console.WriteLine($"Health Mod: " + BitConverter.ToUInt16(healthModifiedChunk, 0) + " [" + BitConverter.ToString(healthModifiedChunk) + "]" + " (ushort, Length: " + healthModifiedChunk.Length + ")");

         // Health Points Max - 0x37 - 0x38
         Console.WriteLine($"Max Health: {BitConverter.ToUInt16(healthMaxChunk, 0)} [{BitConverter.ToString(healthMaxChunk)}] (ushort, Length: {healthMaxChunk.Length})");
         // ----------------------------------------------- //


         // Gold - 0x39 - 0x3B
         int goldNum = (goldChunk[2] << 16) | (goldChunk[1] << 8) | goldChunk[0];
         Console.WriteLine($"Gold: {goldNum} [{BitConverter.ToString(goldChunk).Replace("-", " ")}] (UInt24, Length: {goldChunk.Length})");

         // UNKNOWN 0x3C

         // Armor Class - 0x3D
         int acNum = armorClassChunk[0];
         Console.WriteLine($"AC: {acNum} [{BitConverter.ToString(armorClassChunk)}]");

         // Food - 0x3E
         int foodNum = foodChunk[0];
         Console.WriteLine($"Food: {foodNum} [{BitConverter.ToString(foodChunk)}]");

         // Condition - 0x3F
         var conditionS = BitConverter.ToString(conditionChunk);

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
         Console.WriteLine($"Equipped Weapon: {BitConverter.ToString(equippedWeaponChunk)}");

         // Other equipment - 0x41 - 0x45
         Console.WriteLine($"Equipment: {BitConverter.ToString(equippedGearChunk)}");

         // Inventory - 0x46 - 0x4B
         Console.WriteLine($"Inventory: {BitConverter.ToString(inventoryChunk)}");

         // Equipment Charges - 0x4C - 0x57
         Console.WriteLine($"Equipment Charges: {BitConverter.ToString(equipmentChargesChunk)}");

         // Resistances 0x58 - 0x67

         int resMagic1 = resistancesChunk[0];
         int resMagic2 = resistancesChunk[1];
         int resFire1 = resistancesChunk[2];
         int resFire2 = resistancesChunk[3];
         int resCold1 = resistancesChunk[4];
         int resCold2 = resistancesChunk[5];
         int resElec1 = resistancesChunk[6];
         int resElec2 = resistancesChunk[7];
         int resAcid1 = resistancesChunk[8];
         int resAcid2 = resistancesChunk[9];
         int resFear1 = resistancesChunk[10];
         int resFear2 = resistancesChunk[11];
         int resPoison1 = resistancesChunk[12];
         int resPoison2 = resistancesChunk[13];
         int resSleep1 = resistancesChunk[14];
         int resSleep2 = resistancesChunk[15];

         Console.WriteLine($"Resistances\n Magic  {resMagic1}%/{resMagic2}%  Fire   {resFire1}%/{resFire2}%  Cold   {resCold1}%/{resCold2}%  Elec   {resElec1}%/{resElec2}%\n Acid   {resAcid1}%/{resAcid2}% Fear   {resFear1}%/{resFear2}% Poison {resPoison1}%/{resPoison2}% Sleep  {resSleep1}%/{resSleep2}%");

         // UNKNOWN 0x68 - 0x7D

         // Character Index number - 0x7E
         Console.WriteLine($"\nCharacter Index: {BitConverter.ToString(characterIndexChunk)}");
      }
   }
}

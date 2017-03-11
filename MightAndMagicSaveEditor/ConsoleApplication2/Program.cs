using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using GSF;

namespace MightAndMagicSaveEditor
{
   class Program
   {
      static void Main(string[] args)
      {
         string FILE_NAME = "ROSTER.DTA";

         Console.WriteLine("Might and Magic 1 Save Game Editor (v0.1) by ryz\n");
         Console.WriteLine($"Opening {FILE_NAME}...");

         if (File.Exists(FILE_NAME))
         {
            using (var stream = File.Open(FILE_NAME, FileMode.Open, FileAccess.ReadWrite))
            {

               // There are 18 characters in the file, each one 127 bytes long
               uint[] characterOffset = { 0, 127, 254, 381, 508, 635, 762, 889, 1016, 1143, 1270, 1397, 1524, 1651, 1778, 1905, 2032, 2159 };

               stream.Position = characterOffset[0];

               // Initialize stuff, mostly all the "chunks" as byte arrays

               // Character Name
               var nameChunk = new byte[15]; // Offset 0=0x0
               var nameOffset = 0;
               bool isNewNameValid = false;

               var unknownChunk1 = new byte[1]; // Offset 15=0xF

               var sexChunk = new byte[1]; // Offset 16=0x10
               var sexOffset = 16;

               var unknownChunk2 = new byte[1]; // Offset 17=0x11

               var alignmentChunk = new byte[1]; // Offset 18=0x12
               var alignmentOffset = 18;

               var raceChunk = new byte[1]; // Offset 19=0x13
               var raceOffset = 19;

               var classChunk = new byte[1]; // Offset 20=0x14
               var classOffset = 20;

               // Stats, there are seven statistics for each character, two bytes each.
               var statsChunk = new byte[14]; // Offset 21=0x15

               byte statsIntellect1 = statsChunk[0];
               byte statsIntellect2 = statsChunk[1];
               byte statsMight1 = statsChunk[2];
               byte statsMight2 = statsChunk[3];
               byte statsPersonality1 = statsChunk[4];
               byte statsPersonality2 = statsChunk[5];
               byte statsEndurance1 = statsChunk[6];
               byte statsEndurance2 = statsChunk[7];
               byte statsSpeed1 = statsChunk[8];
               byte statsSpeed2 = statsChunk[9];
               byte statsAccuracy1 = statsChunk[10];
               byte statsAccuracy2 = statsChunk[11];
               byte statsLuck1 = statsChunk[12];
               byte statsLuck2 = statsChunk[13];

               var levelChunk1 = new byte[1]; // Offset 35=0x23
               var levelChunk2 = new byte[1]; // Offset 36=0x24

               var ageChunk = new byte[1]; // Offset 37=0x25

               var unknownChunk3 = new byte[1];  // Offset 38=0x26

               // XP, stored as UInt24
               var xpChunk = new byte[3]; // Offset 39=0x27
               var xpOffset = 39;

               var unknownChunk4 = new byte[3]; // Offset 41=0x29

               var spellPointsChunk = new byte[2]; // Offset 45=0x2D

               var unknownChunk5 = new byte[2]; // Offset 47=0x2F

               var gemsChunk = new byte[2]; // Offset 49=0x31
               var gemsOffset = 49;

               var healthCurrentChunk = new byte[2]; // Offset 51=0x33
               var unknownChunkA = new byte[2]; // Offset 53

               var healthMaxChunk = new byte[2]; // Offset 55

               var goldChunk = new byte[3];  // Offset 57=0x39
               var goldOffset = 57;

               var unknownChunk7 = new byte[1]; // Offset 58=0x3A

               var armorClassChunk = new byte[1]; // Offset 62=0x3D

               var foodChunk = new byte[1]; // Offset 62=0x3E

               var conditionChunk = new byte[1]; // Offset 63=0x3F

               var equippedWeaponChunk = new byte[1]; // Offset 64=0x40
               var equippedGearChunk = new byte[5]; // Offset 65=0x41
               var inventoryChunk = new byte[6]; // Offset 70=0x46

               var unknownChunk8 = new byte[50]; // Offset 76=0x4C - biggest chunk, probably contains various progress/quest-related data

               var characterSlotChunk = new byte[1]; // Offset 126=0x7E


               // We parse, modify and write back parameters for each character
               for (int i = 0; i < characterOffset.Length; i++)
               {
                  stream.Position = characterOffset[i];

                  Console.WriteLine($"Reading Character #{i + 1} at Offset {characterOffset[i]}...\n");

                  ParseCharacter(stream, nameChunk, sexChunk, alignmentChunk, raceChunk, classChunk, statsChunk, levelChunk1, ageChunk, xpChunk, gemsChunk, healthCurrentChunk, healthMaxChunk, goldChunk, armorClassChunk, foodChunk, conditionChunk, equippedWeaponChunk, equippedGearChunk, inventoryChunk, characterSlotChunk);

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
                     WriteChunk(stream, "Name", nameChunk, nameOffset, FILE_NAME);
                  }

                  WriteChunk(stream, "Sex", sexChunk, sexOffset, FILE_NAME);
                  WriteChunk(stream, "Alignment", alignmentChunk, alignmentOffset, FILE_NAME);
                  WriteChunk(stream, "Race", raceChunk, raceOffset, FILE_NAME);
                  WriteChunk(stream, "Class", classChunk, classOffset, FILE_NAME);
                  WriteChunk(stream, "XP", xpChunk, xpOffset, FILE_NAME);
                  WriteChunk(stream, "Gems", gemsChunk, gemsOffset, FILE_NAME);
                  WriteChunk(stream, "Gold", goldChunk, goldOffset, FILE_NAME);

                  Console.WriteLine($"\nCharacter #{i + 1} done!\n");
               }

               Console.WriteLine("\nAll done!");
               Console.ReadLine();
            }
         } else
         {
            Console.WriteLine($"\nFile {FILE_NAME} not found! Make sure it's in the same folder as this program. Aborting.");
            Console.ReadLine();
         }
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
         uint newVal = UInt24.Parse(Console.ReadLine());

         // We have to create an intermediate byte array here where we copy to and from
         // This is because BitConverter.GetBytes() can't return UInt24
         // which would result in a too long chunk (byte array). 

         byte[] newArray = BitConverter.GetBytes(newVal);

         Array.Clear(_chunk, 0, _chunk.Length);
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

      public static void ParseCharacter(
         FileStream _stream,
         byte[] _name,
         byte[] _sex,
         byte[] _alignment,
         byte[] _race,
         byte[] _class,
         byte[] _statsChunk,
         byte[] _level,
         byte[] _age,
         byte[] _exp,
         byte[] _gems,
         byte[] _health,
         byte[] _healthMax,
         byte[] _gold,
         byte[] _armorClass,
         byte[] _food,
         byte[] _condition,
         byte[] _equippedWeapon,
         byte[] _equipment,
         byte[] _inventory,
         byte[] _characterSlot
         )

      {

         // Character Name 0x0 - 0xE
         _stream.Read(_name, 0, _name.Length);

         //encode and print the byte array to a string so that we can debug it
         Console.WriteLine($"Name: {Encoding.Default.GetString(_name)} (Length: {_name.Length})");

         // Magic Byte 0xF, 
         _stream.Position += 1;

         // Sex 0x10
         _stream.Read(_sex, 0, _sex.Length);

         var sexS = BitConverter.ToString(_sex);

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

         // Unknown 0x11
         _stream.Position += 1;

         // Alignment 0x12
         _stream.Read(_alignment, 0, _alignment.Length);

         var alignmentS = BitConverter.ToString(_alignment);

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
         _stream.Read(_race, 0, _race.Length);

         var raceS = BitConverter.ToString(_race);

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
         _stream.Read(_class, 0, _class.Length);

         var classS = BitConverter.ToString(_class);

         switch (classS)
         {
            case "01":
               Console.WriteLine($"Class: KNIGHT ({classS})");
               break;
            case "02":
               Console.WriteLine($"Class: PALADIN ({classS})");
               break;
            case "03":
               Console.WriteLine($"Class: ARCHER ({classS})");
               break;
            case "04":
               Console.WriteLine($"Class: CLERIC ({classS})");
               break;
            case "05":
               Console.WriteLine($"Class: SORCERER ({classS})");
               break;
            case "06":
               Console.WriteLine($"Class: ROBBER ({classS})");
               break;
            default:
               throw new Exception($"Unknown class: {classS}");
         }

         // Stats - 0x15 - 0x22
         _stream.Read(_statsChunk, 0, _statsChunk.Length);

         Console.WriteLine($"Stats (I/M/P/E/S/A/L): {BitConverter.ToString(_statsChunk).Replace("-", " ")} (Length: {_statsChunk.Length})");

         // Intellect
         // Might
         // Perception
         // Endurance
         // Speed
         // Accuracy
         // Luck

         // Level - 0x23 - 0x24
         _stream.Read(_level, 0, _level.Length);
         int levelNum = _level[0];
         Console.WriteLine($"Level: {levelNum} [{BitConverter.ToString(_level)}]");

         _stream.Position += 1;

         // Age Offset 37=0x25
         _stream.Read(_age, 0, _age.Length);
         int ageNum = _age[0];
         Console.WriteLine($"Age: {ageNum} [{BitConverter.ToString(_age)}]");

         // Advance the stream - 0x26
         _stream.Position += 1;

         // Experience - Stored as a little-endian UInt24 0x27 - 0x29
         _stream.Read(_exp, 0, _exp.Length);
         int expNum = (_exp[2] << 16) | (_exp[1] << 8) | _exp[0];
         Console.WriteLine($"Experience : {expNum} [{BitConverter.ToString(_exp).Replace("-", " ")}] (UInt24, Length: {_exp.Length})");


         // Advance the stream - 0x2A - 0x2C
         _stream.Position += 3;

         // Spell Points - 0x2D - 0x2E
         _stream.Position += 2;

         // Advance the stream - 0x2F - 0x30
         _stream.Position += 2;

         // Gems - Stored as a little-endian ushort  0x31 - 0x32
         _stream.Read(_gems, 0, _gems.Length);
         Console.WriteLine($"Gems: {BitConverter.ToInt16(_gems, 0)} [{BitConverter.ToString(_gems)}] (ushort, Length: {_gems.Length})");

         // Health Points - 0x33 - 0x34
         _stream.Read(_health, 0, _health.Length);
         Console.WriteLine($"Health: " + BitConverter.ToUInt16(_health, 0) + " [" + BitConverter.ToString(_health) + "]" + " (ushort, Length: " + _health.Length + ")");

         // ?? 0x37 - 0x38
         _stream.Position += 2;

         // Max HP - 0x35 - 0x36
         _stream.Read(_healthMax, 0, _healthMax.Length);
         Console.WriteLine($"Max Health: {BitConverter.ToUInt16(_healthMax, 0)} [{BitConverter.ToString(_healthMax)}] (ushort, Length: {_healthMax.Length})");



         // Gold - 0x39 - 0x3B
         _stream.Read(_gold, 0, _gold.Length);
         int goldNum = (_gold[2] << 16) | (_gold[1] << 8) | _gold[0];
         Console.WriteLine($"Gold: {goldNum} [{BitConverter.ToString(_gold).Replace("-", " ")}] (UInt24, Length: {_gold.Length})");


         // Advance the stream 0x3C
         _stream.Position += 1;

         // Armor Class - 0x3D
         _stream.Read(_armorClass, 0, _armorClass.Length);
         int acNum = _armorClass[0];
         Console.WriteLine($"AC: {acNum} [{BitConverter.ToString(_armorClass)}]");

         // Food - 0x3E
         _stream.Read(_food, 0, _food.Length);
         int foodNum = _food[0];
         Console.WriteLine($"Food: {foodNum} [{BitConverter.ToString(_food)}]");

         // Condition - 0x3F
         _stream.Read(_condition, 0, _condition.Length);

         var conditionS = BitConverter.ToString(_condition);

         switch(conditionS)
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
         _stream.Read(_equippedWeapon, 0, _equippedWeapon.Length);
         Console.WriteLine($"Equipped Weapon: {BitConverter.ToString(_equippedWeapon)}");

         // Other equipment - 0x41 - 0x45
         _stream.Read(_equipment, 0, _equipment.Length);
         Console.WriteLine($"Equipment: {BitConverter.ToString(_equipment)}");

         // Inventory - 0x46 - 0x4B
         _stream.Read(_inventory, 0, _inventory.Length);
         Console.WriteLine($"Inventory: {BitConverter.ToString(_inventory)}");

         // Advance the stream - 0x4C - 0x7D
         _stream.Position += 50;

         // Character slot number - 0x7E
         _stream.Read(_characterSlot, 0, _characterSlot.Length);
         Console.WriteLine($"Character Slot: {BitConverter.ToString(_characterSlot)}");
      }



   }
}

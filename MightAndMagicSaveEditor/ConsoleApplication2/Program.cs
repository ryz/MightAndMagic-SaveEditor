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
      static void Main(string[] args)
      {
         string FILE_NAME = "ROSTER.DTA";

         Console.WriteLine("Might and Magic 1 Save Game Editor (v0.1)\n");
         Console.WriteLine("Opening " + FILE_NAME + "...");

         if (File.Exists(FILE_NAME))
         {
            using (var stream = File.Open(FILE_NAME, FileMode.Open, FileAccess.ReadWrite))
            {

               bool isNewNameValid = false;
               // 18 characters, each one 127 byte
               uint[] characterOffset = { 0, 127, 254, 381, 508, 635, 762, 889, 1016, 1143, 1270, 1397, 1524, 1651, 1778, 1905, 2032, 2159 };

               stream.Position = characterOffset[0];

               // Initialize all the chunks as byte arrays
               var nameChunk = new byte[15]; // Offset 0=0x0

               var offsetOfNameChunkInFile = stream.Position;
               byte[] newName = new byte[nameChunk.Length];


               var unknownChunk1 = new byte[1]; // Offset 15=0xF

               var sexChunk = new byte[1]; // Offset 16=0x10
               var alignmentChunk = new byte[1]; // Offset 17=0x11
               var raceChunk = new byte[1]; // Offset 18=0x12

               var unknownChunk2 = new byte[1]; // Offset 19=0x13

               var classChunk = new byte[1]; // Offset 20=0x14
               var offsetClassChunkInFile = nameChunk.Length + unknownChunk1.Length + sexChunk.Length + alignmentChunk.Length + raceChunk.Length + unknownChunk2.Length;

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

               // XP, stored as ushort/UInt16
               var xpChunk = new byte[2]; // Offset 39=0x27
               var offsetXpChunkInFile = nameChunk.Length + unknownChunk1.Length + sexChunk.Length + alignmentChunk.Length + raceChunk.Length + unknownChunk2.Length + classChunk.Length + statsChunk.Length + levelChunk1.Length + levelChunk2.Length + ageChunk.Length + unknownChunk3.Length;

               var unknownChunk4 = new byte[4]; // Offset 41=0x29

               var spellPointsChunk = new byte[2]; // Offset 45=0x2D

               var unknownChunk5 = new byte[2]; // Offset 47=0x2F

               var gemsChunk = new byte[1]; // Offset 49=0x31

               var unknownChunk6 = new byte[1]; // Offset 50=0x32

               var healthChunk = new byte[6]; // Offset 51=0x33

               var goldChunk = new byte[1];  // Offset 57=0x39

               var unknownChunk7 = new byte[3]; // Offset 58=0x3A

               var armorClassChunk = new byte[1]; // Offset 62=0x3D

               var foodChunk = new byte[1]; // Offset 62=0x3E

               var conditionChunk = new byte[1]; // Offset 63=0x3F

               var equippedWeaponChunk = new byte[1]; // Offset 64=0x40

               var equippedGearChunk = new byte[5]; // Offset 65=0x41

               var inventoryChunk = new byte[6]; // Offset 70=0x46

               var unknownChunk8 = new byte[50]; // Offset 76=0x4C - biggest chunk, probably contains various progress/quest-related data

               var characterSlotChunk = new byte[1]; // Offset 126=0x7E


               for (int i = 0; i < characterOffset.Length; i++)
               {
                  stream.Position = characterOffset[i];

                  Console.WriteLine($"Reading Character #{i + 1} at Offset {characterOffset[i]}...\n");

                  ParseCharacter(stream, nameChunk, sexChunk, alignmentChunk, raceChunk, classChunk, statsChunk, levelChunk1, xpChunk, gemsChunk, healthChunk, goldChunk, armorClassChunk, foodChunk, conditionChunk, equippedWeaponChunk, equippedGearChunk, inventoryChunk, characterSlotChunk);

                  GetName(nameChunk, newName, ref isNewNameValid);

                  // do work on the chunks

                  Console.Write("\nEnter a new class (1-5): ");
                  byte classInput = Byte.Parse(Console.ReadLine());

                  Console.Write("\nEnter new XP value (0-9999): ");
                  ushort newXP = UInt16.Parse(Console.ReadLine());
                  xpChunk = BitConverter.GetBytes(newXP);

                  //write it back to the file

                  //name
                  if (isNewNameValid)
                  {
                     Console.WriteLine($"\nWriting new name back to {FILE_NAME}. Are you sure?");
                     Console.ReadLine();
                     stream.Seek(offsetOfNameChunkInFile, SeekOrigin.Begin);
                     stream.Write(nameChunk, 0, nameChunk.Length);
                  }

                  //class
                  Console.WriteLine($"\nWriting new Class back to {FILE_NAME}. Are you sure?");
                  Console.ReadLine();
                  stream.Seek(offsetClassChunkInFile, SeekOrigin.Begin);
                  stream.WriteByte(classInput);

                  //exp
                  Console.WriteLine($"\nWriting new XP value back to {FILE_NAME} . Are you sure?");
                  Console.ReadLine();
                  stream.Seek(offsetXpChunkInFile, SeekOrigin.Begin);
                  stream.Write(xpChunk, 0, xpChunk.Length);




                  Console.WriteLine($"\nCharacter #{i + 1} done!\n");
               }

               Console.WriteLine("\nAll done!");
               Console.ReadLine();
            }
         } else
         {
            Console.WriteLine($"\nFile {FILE_NAME} not found! Aborting.");
            Console.ReadLine();
         }
      }

      public static void GetName(byte[] _name, byte[] _newName, ref bool _nameValid)
      {

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
            _newName = Encoding.ASCII.GetBytes(nameInput);
            _nameValid = true;
         }
         else
         {
            Console.Write($"Name '{nameInput}' contains invalid characters! Name has not been changed.\n");
            _nameValid = false;
         }

         // we clear the old array first so we can just copy the new one in it - keeps array size the same
         Array.Clear(_name, 0, _name.Length);
         Array.Copy(_newName, _name, _newName.Length);
      }

      public static void ParseCharacter(
         FileStream _stream, 
         byte[] _name, 
         byte[] _sex, 
         byte[] _alignment, 
         byte[] _race, 
         byte[] _characterClassNum, 
         byte[] _statsChunk, 
         byte[] _level, 
         byte[] _exp, 
         byte[] _gems, 
         byte[] _health, 
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
         Console.WriteLine($"Name: " + Encoding.Default.GetString(_name) + " (Length: " + _name.Length + ")");
         
         //Console.WriteLine("Name Array As Hex: " + BitConverter.ToString(_name).Replace("-", " ") + " (Length: " + _name.Length + ")");

         // Magic Byte 0xF, 
         _stream.Position += 1;
         //_stream.Read(_magicByte, 0, _magicByte.Length);

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

         // Alignment 0x11
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
               throw new Exception($"\nUnknown alignment: {alignmentS}");

         }
         // Race 0x12
         _stream.Read(_race, 0, _race.Length);

         var raceS = BitConverter.ToString(_race);

         switch (raceS)
         {
            case "01":
               Console.WriteLine($"Race: Human? ({raceS})");
               break;
            case "02":
               Console.WriteLine($"Race: 02 ({raceS})");
               break;
            case "03":
               Console.WriteLine($"Race: 03 ({raceS})");
               break;
            case "04":
               Console.WriteLine($"Race: 04 ({raceS})");
               break;
            case "05":
               Console.WriteLine($"Race: 05 ({raceS})");
               break;
            default:
               throw new Exception($"\nUnknown race: {raceS}");

         }

         // Unknown 0x13
         //_stream.Read(_unknownSection1, 0, _unknownSection1.Length);
         _stream.Position += 1;

         // Character Class - 0x14
         _stream.Read(_characterClassNum, 0, _characterClassNum.Length);
         var characterClassString = BitConverter.ToString(_characterClassNum);

         switch (characterClassString)
         {
            case "01":
               Console.WriteLine($"Class: KNIGHT ({characterClassString})");
               break;
            case "02":
               Console.WriteLine($"Class: PALADIN ({characterClassString})");
               break;
            case "03":
               Console.WriteLine($"Class: ARCHER ({characterClassString})");
               break;
            case "04":
               Console.WriteLine($"Class: CLERIC ({characterClassString})");
               break;
            case "05":
               Console.WriteLine($"Class: SORCERER ({characterClassString})");
               break;
            case "06":
               Console.WriteLine($"Class: ROBBER ({characterClassString})");
               break;
            default:
               throw new Exception($"Unknown class: {characterClassString}");
         }

         // Stats - 0x15 - 0x22
         _stream.Read(_statsChunk, 0, _statsChunk.Length);

         Console.WriteLine("Stats (I/M/P/E/S/A/L): " + BitConverter.ToString(_statsChunk).Replace("-", " ") + " (Length: " + _statsChunk.Length + ")");

         // Intellect

         // Might

         // Perception

         // Endurance

         // Speed

         // Accuracy

         // Luck


         // Level - 0x23 - 0x24
         _stream.Read(_level, 0, _level.Length);
         Console.WriteLine("Level: " + BitConverter.ToString(_level).Replace("-", " ") + " (Length: " + _level.Length + ")");

         _stream.Position += 1;

         // Advance the stream - 0x25 - 0x26
         _stream.Position += 2;

         // Experience - Stored as a little-endian ushort 0x27 - 0x28
         _stream.Read(_exp, 0, _exp.Length);
         Console.WriteLine("Experience : " + BitConverter.ToInt16(_exp, 0) + " [" + BitConverter.ToString(_exp).Replace("-", " ") + "]" + " (ushort, Length: " + _exp.Length + ")");

         // Advance the stream - 0x29 - 0x2C
         _stream.Position += 4;

         // Spell Points - 0x2D - 0x2E
         _stream.Position += 2;

         // Advance the stream - 0x2F - 0x30
         _stream.Position += 2;

         // Gems - 0x31
         _stream.Read(_gems, 0, _gems.Length);
         Console.WriteLine("Gems: " + BitConverter.ToString(_gems));

         // Advance the stream - 0x32
         _stream.Position += 1;

         // Health Points - 0x33 - 0x34
         // Max HP - 0x35 - 0x36 & 0x37 - 0x38
         _stream.Read(_health, 0, _health.Length);
         Console.WriteLine("Health: " + BitConverter.ToString(_health));


         // Gold - 0x39
         _stream.Read(_gold, 0, _gold.Length);
         Console.WriteLine("Gold: " + BitConverter.ToString(_gold));

         // Advance the stream 0x3A - 0x3C
         _stream.Position += 3;

         // Armor Class - 0x3D
         _stream.Read(_armorClass, 0, _armorClass.Length);
         Console.WriteLine("AC: " + BitConverter.ToString(_armorClass));

         // Food - 0x3E
         _stream.Read(_food, 0, _food.Length);
         Console.WriteLine("Food: " + BitConverter.ToString(_food));

         // Condition - 0x3F
         _stream.Read(_condition, 0, _condition.Length);
         Console.WriteLine("Condition: " + BitConverter.ToString(_condition));

         // Equipped Weapon - 0x40
         _stream.Read(_equippedWeapon, 0, _equippedWeapon.Length);
         Console.WriteLine("Equipped Weapon: " + BitConverter.ToString(_equippedWeapon));

         // Other equipment - 0x41 - 0x45
         _stream.Read(_equipment, 0, _equipment.Length);
         Console.WriteLine("Equipment: " + BitConverter.ToString(_equipment));

         // Inventory - 0x46 - 0x4B
         _stream.Read(_inventory, 0, _inventory.Length);
         Console.WriteLine("Inventory: " + BitConverter.ToString(_inventory));

         // Advance the stream - 0x4C - 0x7D
         _stream.Position += 50;

         // Character slot number - 0x7E
         _stream.Read(_characterSlot, 0, _characterSlot.Length);
         Console.WriteLine("Character Slot: " + BitConverter.ToString(_characterSlot));
      }
         
   }
}

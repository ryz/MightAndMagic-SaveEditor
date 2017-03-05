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

         Console.WriteLine("Might and Magic 1 Save Game Editor (v0.02)\n");
         Console.WriteLine("Opening " + FILE_NAME + "...");

         using (var stream = File.Open(FILE_NAME, FileMode.Open, FileAccess.ReadWrite))
         {
            var nameChunk = new byte[15];
            var offsetOfNameChunkInFile = stream.Position;

            byte[] newName = new byte[15];
            bool isNewNameValid = false;

            var magicByte = new byte[1];

            var sex = new byte[1];
            var alignment = new byte[1];
            var race = new byte[1];

            var unknownSection1 = new byte[1];

            var characterClassNum = new byte[1];
            var offsetOfCharacterClassChunkInFile = nameChunk.Length + magicByte.Length + sex.Length + alignment.Length + race.Length + unknownSection1.Length;


            // Stats Chunk
            var statsAll = new byte[14]; 

            var characterLevel1 = new byte[1];
            var characterLevel2 = new byte[1];

            var age = new byte[1]; // Offset 37=0x25

            var unknownByte2 = new byte[1];  // Offset 38=0x26

            // XP, stored as ushort/UInt16
            var experience = new byte[2]; // Offset 39=0x27
            var offsetExperienceChunkInFile = nameChunk.Length + magicByte.Length + sex.Length + alignment.Length + race.Length + unknownSection1.Length + characterClassNum.Length + statsAll.Length + characterLevel1.Length + characterLevel2.Length + age.Length + unknownByte2.Length;


            ParseCharacter(stream, nameChunk, sex, alignment, race, characterClassNum, statsAll, characterLevel1, experience);

            byte statsIntellect1 = statsAll[0];
            byte statsIntellect2 = statsAll[1];
            byte statsMight1 = statsAll[2];
            byte statsMight2 = statsAll[3];
            byte statsPersonality1 = statsAll[4];
            byte statsPersonality2 = statsAll[5];
            byte statsEndurance1 = statsAll[6];
            byte statsEndurance2 = statsAll[7];
            byte statsSpeed1 = statsAll[8];
            byte statsSpeed2 = statsAll[9];
            byte statsAccuracy1 = statsAll[10];
            byte statsAccuracy2 = statsAll[11];
            byte statsLuck1 = statsAll[12];
            byte statsLuck2 = statsAll[13];


            //ask and get new name from input and save it into a byte array
            Console.Write("\nEnter a new Name: ");
            string nameInput = Console.ReadLine().ToUpper();

            // Truncate input above 15 characters
            nameInput = nameInput.Substring(0, Math.Min(15, nameInput.Length));

            // Check that the name contains only uppercase latin characters and numerals from 0-9
            Regex rx = new Regex("^[A-Z0-9]*$");

            if(rx.IsMatch(nameInput))
            {
               Console.Write("Name '" + nameInput + "' accepted.\n");
               newName = Encoding.ASCII.GetBytes(nameInput);
               isNewNameValid = true;
            }
            else
            {
               Console.Write("Name contains invalid characters! Name has not been changed.\n");
               isNewNameValid = false;
            }


            /*
            // debug
            var newNameString = Encoding.Default.GetString(newName);
            Console.WriteLine("\nIntermediate Name: " + newNameString + " (Length: " + newNameString.Length + ")");
            Console.WriteLine("Intermediate Name Array As Hex: " + BitConverter.ToString(newName).Replace("-", " ") + " (Length: " + newName.Length + ")");
            */

            // do work on the chunk
            // we clear old array first so we can just copy the new one in it - keeps array size the same
            Array.Clear(nameChunk, 0, nameChunk.Length);
            Array.Copy(newName, nameChunk, newName.Length);

            /*
            // debug - print new name
            nameString = Encoding.Default.GetString(nameChunk);
            Console.WriteLine("\nNew Name: " + nameString + " (Length: " + nameString.Length + ")");
            Console.WriteLine("New Name Array As Hex: " + BitConverter.ToString(nameChunk).Replace("-", " ") + " (Length: " + nameString.Length + ")");
            */

            Console.Write("\nEnter a new class (1-5): ");
            byte classInput = Byte.Parse(Console.ReadLine());

            Console.Write("\nEnter new XP value (0-9999): ");
            ushort newXP = UInt16.Parse(Console.ReadLine());
            experience = BitConverter.GetBytes(newXP);

            //write it back to the file

            //name
            if (isNewNameValid)
            {
               Console.WriteLine("\nWriting new name back to " + FILE_NAME + ". Are you sure?");
               Console.ReadLine();
               stream.Seek(offsetOfNameChunkInFile, SeekOrigin.Begin);
               stream.Write(nameChunk, 0, nameChunk.Length);
            }

            //class
            Console.WriteLine("\nWriting new Class back to " + FILE_NAME + ". Are you sure?");
            Console.ReadLine();
            stream.Seek(offsetOfCharacterClassChunkInFile, SeekOrigin.Begin);
            stream.WriteByte(classInput);

            //exp
            Console.WriteLine("\nWriting new XP value back to " + FILE_NAME + ". Are you sure?");
            Console.ReadLine();
            stream.Seek(offsetExperienceChunkInFile, SeekOrigin.Begin);
            stream.Write(experience, 0, experience.Length);



            Console.ReadLine();
         }
      }
      public static void ParseCharacter(FileStream _stream, byte[] _name, byte[] _sex, byte[] _alignment, byte[] _race, byte[] _characterClassNum, byte[] _statsAll, byte[] _level, byte[] _exp)
      {
         Console.WriteLine("Reading first Character...\n");

         // Character Name 0x0 - 0xE
         _stream.Read(_name, 0, _name.Length);

         //encode and print the byte array to a string so that we can debug it
         Console.WriteLine("Name: " + Encoding.Default.GetString(_name) + " (Length: " + _name.Length + ")");
         Console.WriteLine("Name Array As Hex: " + BitConverter.ToString(_name).Replace("-", " ") + " (Length: " + _name.Length + ")");

         // Magic Byte 0xF, 
         _stream.Position += 1;
         //_stream.Read(_magicByte, 0, _magicByte.Length);

         // Sex 0x10
         _stream.Read(_sex, 0, _sex.Length);

         var sexS = BitConverter.ToString(_sex);

         switch (sexS)
         {
            case "01":
               Console.WriteLine("\nSex: Male " + "(" + sexS + ")");
               break;
            case "02":
               Console.WriteLine("\nSex: Female " + "(" + sexS + ")");
               break;
            default:
               throw new Exception("\nUnknown sex: " + sexS);
         }

         // Alignment 0x11
         _stream.Read(_alignment, 0, _alignment.Length);

         var alignmentS = BitConverter.ToString(_alignment);

         switch (alignmentS)
         {
            case "01":
               Console.WriteLine("Alignment: Good " + "(" + alignmentS + ")");
               break;
            case "02":
               Console.WriteLine("Alignment: Neutral " + "(" + alignmentS + ")");
               break;
            case "03":
               Console.WriteLine("Alignment: Evil " + "(" + alignmentS + ")");
               break;
            default:
               throw new Exception("\nUnknown alignment: " + alignmentS);

         }
         // Race 0x12
         _stream.Read(_race, 0, _race.Length);

         var raceS = BitConverter.ToString(_race);

         switch (raceS)
         {
            case "01":
               Console.WriteLine("Race: Human? " + "(" + raceS + ")");
               break;
            case "02":
               Console.WriteLine("Race: 02 " + "(" + raceS + ")");
               break;
            case "03":
               Console.WriteLine("Race: 03 " + "(" + raceS + ")");
               break;
            case "04":
               Console.WriteLine("Race: 04 " + "(" + raceS + ")");
               break;
            case "05":
               Console.WriteLine("Race: 05 " + "(" + raceS + ")");
               break;
            default:
               throw new Exception("\nUnknown race: " + raceS);

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
               Console.WriteLine("Class: KNIGHT " + "(" + characterClassString + ")");
               break;
            case "02":
               Console.WriteLine("Class: PALADIN " + "(" + characterClassString + ")");
               break;
            case "03":
               Console.WriteLine("Class: ARCHER " + "(" + characterClassString + ")");
               break;
            case "04":
               Console.WriteLine("Class: CLERIC " + "(" + characterClassString + ")");
               break;
            case "05":
               Console.WriteLine("Class: SORCERER " + "(" + characterClassString + ")");
               break;
            case "06":
               Console.WriteLine("Class: ROBBER " + "(" + characterClassString + ")");
               break;
            default:
               throw new Exception("Unknown class: " + characterClassString);
         }

         // Stats - 0x15 - 0x22
         _stream.Read(_statsAll, 0, _statsAll.Length);

         Console.WriteLine("Stats (I/M/P/E/S/A/L): " + BitConverter.ToString(_statsAll).Replace("-", " ") + " (Length: " + _statsAll.Length + ")");

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

         // Advance the stream - 0x2F - 0x30
         _stream.Position += 2;

         // Gems - 0x31

         // Advance the stream - 0x32
         _stream.Position += 1;

         // Health Points - 0x33 - 0x34

         // Max HP - 0x35 - 0x36 & 0x37 - 0x38

         // Gold - 0x39

         // Advance the stream 0x3A - 0x3C
         _stream.Position += 3;

         // Armor Class - 0x3D

         // Food - 0x3E

         // Condition - 0x3F

         // Equipped Weapon - 0x40

         // Other equipment - 0x41 - 0x45

         // Inventory - 0x46 - 0x4B

         // Advance the stream - 0x4C - 0x7D

         // Character slot number - 0x7E
      }
         
   }
}

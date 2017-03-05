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
            var unknownSection1 = new byte[4];

            var characterClassNum = new byte[1];
            var offsetOfCharacterClassChunkInFile = nameChunk.Length + magicByte.Length + unknownSection1.Length;


            // Stats
            var statsAll = new byte[14]; 

            var statsIntellect1 = new byte[1];
            var statsIntellect2 = new byte[1];
            var statsMight1 = new byte[1];
            var statsMight2 = new byte[1];
            var statsPersonality1 = new byte[1];
            var statsPersonality2 = new byte[1];
            var statsEndurance1 = new byte[1];
            var statsEndurance2 = new byte[1];
            var statsSpeed1 = new byte[1];
            var statsSpeed2 = new byte[1];
            var statsAccuracy1 = new byte[1];
            var statsAccuracy2 = new byte[1];
            var statsLuck1 = new byte[1];
            var statsLuck2 = new byte[1];

            var characterLevel1 = new byte[1];
            var characterLevel2 = new byte[1];

            var experience1 = new byte[1];
            var experience2 = new byte[2];


            ParseCharacter(stream, nameChunk, characterClassNum, statsAll, characterLevel1, experience1);

            //ask and get new name from input and save it into a byte array
            Console.Write("\nEnter a new Name: ");
            string nameInput = Console.ReadLine().ToUpper();
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

            //write it back to the file

            //name
            if (isNewNameValid)
            {
               Console.WriteLine("\nWriting new name back to " + FILE_NAME + ". Are you sure?");
               Console.ReadLine();
               stream.Seek(offsetOfNameChunkInFile, SeekOrigin.Begin);
               stream.Write(nameChunk, 0, nameChunk.Length);
               Console.WriteLine("New name: Manfred");
            }

            //class
            Console.WriteLine("\nWriting new Class back to " + FILE_NAME + ". Are you sure?");
            Console.ReadLine();
            stream.Seek(offsetOfCharacterClassChunkInFile, SeekOrigin.Begin);
            stream.WriteByte(classInput);
            Console.WriteLine("New Class: ARCHER " + "(03)");

            Console.ReadLine();
         }
      }
      public static void ParseCharacter(FileStream _stream, byte[] _name, byte[] _characterClassNum, byte[] _statsAll, byte[] _level, byte[] _exp)
      {
         //read dat shit 0x0 - 0xE
         _stream.Read(_name, 0, _name.Length);

         Console.WriteLine("Reading first Character...\n");

         //encode the byte array to a string so that we can debug it
         var nameString = Encoding.Default.GetString(_name);
         Console.WriteLine("Name: " + nameString + " (Length: " + nameString.Length + ")");
         Console.WriteLine("Name Array As Hex: " + BitConverter.ToString(_name).Replace("-", " ") + " (Length: " + _name.Length + ")");

         // Advance the stream 0xF, 0x10 - 0x13
         _stream.Position += 5;
         //_stream.Read(_magicByte, 0, _magicByte.Length);
         //_stream.Read(_unknownSection1, 0, _unknownSection1.Length);

         // Read character class - 0x14
         _stream.Read(_characterClassNum, 0, _characterClassNum.Length);
         var characterClassString = BitConverter.ToString(_characterClassNum);

         switch (characterClassString)
         {
            case "01":
               Console.WriteLine("\nClass: KNIGHT " + "(" + characterClassString + ")");
               break;
            case "02":
               Console.WriteLine("\nClass: PALADIN " + "(" + characterClassString + ")");
               break;
            case "03":
               Console.WriteLine("\nClass: ARCHER " + "(" + characterClassString + ")");
               break;
            case "04":
               Console.WriteLine("\nClass: CLERIC " + "(" + characterClassString + ")");
               break;
            case "05":
               Console.WriteLine("\nClass: SORCERER " + "(" + characterClassString + ")");
               break;
            case "06":
               Console.WriteLine("\nClass: ROBBER " + "(" + characterClassString + ")");
               break;
            default:
               throw new Exception("\nUnknown class: " + characterClassString);
         }

         //Read stats - 0x15 - 0x22
         _stream.Read(_statsAll, 0, _statsAll.Length);

         Console.WriteLine("Stats (I/M/P/E/S/A/L): " + BitConverter.ToString(_statsAll).Replace("-", " ") + " (Length: " + _statsAll.Length + ")");

         // Intellect

         // Might

         // Perception

         // Endurance

         // Speed

         // Accuracy

         // Luck


         // Read level - 0x23 - 0x24
         _stream.Read(_level, 0, _level.Length);
         Console.WriteLine("Level: " + BitConverter.ToString(_level).Replace("-", " ") + " (Length: " + _level.Length + ")");

         _stream.Position += 1;

         // Advance the stream - 0x25 - 0x26
         _stream.Position += 2;

         // Read Experience - 0x27 - 0x28
         _stream.Read(_exp, 0, _exp.Length);
         Console.WriteLine("Experience: " + BitConverter.ToString(_exp).Replace("-", " ") + " (Length: " + _exp.Length + ")");

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

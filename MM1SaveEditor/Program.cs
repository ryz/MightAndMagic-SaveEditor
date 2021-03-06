﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;


namespace MM1SaveEditor
{
   class Program
   {
      static string ROSTER_FILE_NAME = "ROSTER.DTA";
      static string VERSION_NUMBER = "v0.6";

      static Character[] characters = new Character[18];
      static int[] characterOffsets = new int[18];

      static byte[] characterSlotsChunk = new byte[18]; // See ReadCharacterSlots()
      static int characterSlotsOffset = 2286;

      static Dumper dumper = new Dumper();
      static MazeViewer mazeViewer = new MazeViewer();

      static void Main(string[] args)
      {
         if (File.Exists(ROSTER_FILE_NAME))
         {
            Console.Write($"Opening {ROSTER_FILE_NAME}... ");

            using (var stream = File.Open(ROSTER_FILE_NAME, FileMode.Open, FileAccess.ReadWrite))
            {
               Console.WriteLine("Success!\n");
               Console.OutputEncoding = System.Text.Encoding.UTF8;

               mazeViewer.Init();

               dumper.Init(); // If MM.exe is found, initialize and parse item & monster data
               
               InitializeCharacters();
               MainMenu(stream);
            }
         }
         else
         {
            Console.WriteLine($"File {ROSTER_FILE_NAME} not found! Make sure it's in the same folder as this program.\nAborting.");
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
            userInput = Console.ReadKey(true);

            switch (userInput.KeyChar.ToString())
            {
               case "1":
                  CharacterListMenu(_stream);
                  break;
               case "2":
                  QuickStartPackage(_stream, characters[0]);
                  break;
               case "9":
                  if (Dumper.isInitialized)
                  {
                     dumper.DumpAllItemsToTxtFile(_stream);
                     dumper.DumpAllItemsToJsonFile();
                     Console.ReadLine();
                  }
                  else
                  {
                     Console.Write("Dumper not initialized! Nothing has been dumped. Press any key to continue.");
                     Console.ReadLine();
                  }
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
         Console.Clear();
         Console.WriteLine($"Might and Magic 1 Save Game Editor ({VERSION_NUMBER}) by ryz");
         Console.WriteLine();
         Console.WriteLine("1. List (and edit) characters");
         Console.WriteLine("2. Quick Start Package - Give each character XP, Gold and Gems");
         Console.WriteLine();
         Console.WriteLine("9. Dump Items to JSON/TXT file");
         Console.WriteLine();
         Console.WriteLine("Press ESC to exit.");
      }

      static void DisplayCharacterListMenu(FileStream _stream)
      {
         Console.Clear();
         Console.WriteLine("Character List");
         Console.WriteLine();

         ReadCharacterSlots(_stream);
         PrintCharacterHeader();

         int characterCounter = 0;

         for (int i = 0; i < characters.Length; i++)
         {
            if (!characters[i].exists)
            {
               Console.WriteLine($"-- ---EMPTY-SLOT-- --- ------- -------- -------- --- ----- -- ------- --------");
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
         if (characterCounter <= 10)
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

         ReadCharacterSlots(_stream);
         ConsoleKeyInfo input;

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

               ModifyChunkUInt32(characters[i].xpChunk, "XP", 32001);
               ModifyChunkUInt16(characters[i].gemsChunk, "Gems", 200);
               ModifyChunkUInt24(characters[i].goldChunk, "Gold", 5000);

               characters[i].sexChunk[0] = 2; // Set to female

               for (int j = 0; j < characters[i].statsChunk.Length; j++)
               {
                  characters[i].statsChunk[j] = 18; // give perfect rollable stats
               }

               //Console.WriteLine($"Writing new values back to {ROSTER_FILE_NAME}. Are you sure?");
               //Console.ReadLine();

               WriteChunk(_stream, characters[i].xpChunk, characters[i].xpOffset);
               WriteChunk(_stream, characters[i].gemsChunk, characters[i].gemsOffset);
               WriteChunk(_stream, characters[i].goldChunk, characters[i].goldOffset);
               WriteChunk(_stream, characters[i].statsChunk, characters[i].statsOffset);
               WriteChunk(_stream, characters[i].sexChunk, characters[i].sexOffset);
            }
         }

         Console.WriteLine("\nAll done! have fun. Press ENTER to get back to the menu.");
         Console.ReadLine();
      }

      static void EditCharacter(FileStream _stream, Character _char)
      {
         ReadCharacterSlots(_stream); // We read this here to see if a character changed town

         if (_char.exists)
         {

            ConsoleKeyInfo input;

            _stream.Position = _char.offset;

            ParseCharacter(_stream, _char);

            do
            {
               bool isValueChanged = false;
               PrintCharacter(_char);
               Console.WriteLine("\nEdit (N)ame, (S)ex, (A)lignment, (R)ace, (C)lass, E(X)perience, (G)old, G(E)ms\n(F)ood, (B)ackpack or (W)arp to town. Press ESC to go back.");

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
                  case ConsoleKey.A:
                     ModifyChunkUInt8(_char.alignmentCurrentChunk, "Alignment", 1, 3);
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
                     ModifyChunkUInt32(_char.xpChunk, "XP");
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
                  case ConsoleKey.F:
                     ModifyChunkUInt8(_char.foodChunk, "Food", 0, 255);
                     isValueChanged = true;
                     break;
                  case ConsoleKey.Q:
                     ModifyChunkUInt8(_char.questMainAct1Chunk, "Quest", 0, 255);
                     isValueChanged = true;
                     break;
                  case ConsoleKey.B:
                     EditBackpackOfCharacter(_stream, _char);
                     isValueChanged = true;
                     break;
                  case ConsoleKey.W:
                     WarpCharacterToTown(_stream, _char);
                     break;
               }

               if (isValueChanged)
               {
                  // Write chunks back to the file
                  
                  //Console.WriteLine($"Writing new value(s) back to {ROSTER_FILE_NAME}. Are you sure? Press ESC to abort.");
                  //input = Console.ReadKey(true);

                  //if (input.Key != ConsoleKey.Escape)
                  //{
                     WriteChunk(_stream, _char.nameChunk, _char.nameOffset);
                     WriteChunk(_stream, _char.sexChunk, _char.sexOffset);
                     WriteChunk(_stream, _char.alignmentCurrentChunk, _char.alignmentCurrentOffset);
                     WriteChunk(_stream, _char.raceChunk, _char.raceOffset);
                     WriteChunk(_stream, _char.classChunk, _char.classOffset);
                     WriteChunk(_stream, _char.xpChunk, _char.xpOffset);
                     WriteChunk(_stream, _char.gemsChunk, _char.gemsOffset);
                     WriteChunk(_stream, _char.goldChunk, _char.goldOffset);
                     WriteChunk(_stream, _char.foodChunk, _char.foodOffset);
                     WriteChunk(_stream, _char.questMainAct1Chunk, _char.questOffset);
                  //}

               }

            } while (input.Key != ConsoleKey.Escape);
         }

      }

      static void WarpCharacterToTown(FileStream _stream, Character _char)
      {
         _stream.Position = _char.offset;
         ParseCharacter(_stream, _char);
         PrintCharacter(_char);

         Console.WriteLine("Select new town (1. Sorpigal, 2. Portsmith, 3. Algary, 4. Dusk, 5. Erliquin) ");
         byte input = Convert.ToByte(Console.ReadLine());

         characterSlotsChunk[_char.indexNum] = input;

         WriteChunk(_stream, characterSlotsChunk, characterSlotsOffset);

         ReadCharacterSlots(_stream); // Read the slots here again to reflect the changes
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

      static void SetItemInBackpackSlot(FileStream _stream, Character _char, byte _itemId, uint _slotNum)
      {
         // "Gives" a character a certain item, defined by it's ID and then 
         // sets the charges corresponding to the charge value parsed from the game (exe).

         _char.backpackChunk[_slotNum] = _itemId;

         if (_itemId > 0)
         {
            _char.backpackChargesChunk[_slotNum] = (byte)Dumper.items[_itemId - 1].charges; // -1 because the item array starts at 0
         }
         else
         {
            _char.backpackChargesChunk[_slotNum] = 0;
         }

      }

      static void EditBackpackOfCharacter(FileStream _stream, Character _char)
      {
         ConsoleKeyInfo input;

         _stream.Position = _char.offset;
         ParseCharacter(_stream, _char);
         PrintCharacter(_char);

         Console.WriteLine("Select slot to edit (1-6)");

         input = Console.ReadKey();
         byte itemId;

         switch (input.Key)
         {
            case ConsoleKey.D1:
               Console.WriteLine($"Slot #1 contains: {GetItemName(_char.backpackSlot1)} ({_char.backpackSlot1}). Enter new item ID (0-255): ");
               itemId = Convert.ToByte(Console.ReadLine());
               SetItemInBackpackSlot(_stream, _char, itemId, 0);
               break;
            case ConsoleKey.D2:
               Console.WriteLine($"Slot #2 contains: {GetItemName(_char.backpackSlot2)} ({_char.backpackSlot2}). Enter new item ID (0-255): ");
               itemId = Convert.ToByte(Console.ReadLine());
               SetItemInBackpackSlot(_stream, _char, itemId, 1);
               break;
            case ConsoleKey.D3:
               Console.WriteLine($"Slot #3 contains: {GetItemName(_char.backpackSlot3)} ({_char.backpackSlot3}). Enter new item ID (0-255): ");
               itemId = Convert.ToByte(Console.ReadLine());
               SetItemInBackpackSlot(_stream, _char, itemId, 2);
               break;
            case ConsoleKey.D4:
               Console.WriteLine($"Slot #4 contains: {GetItemName(_char.backpackSlot4)} ({_char.backpackSlot4}). Enter new item ID (0-255): ");
               itemId = Convert.ToByte(Console.ReadLine());
               SetItemInBackpackSlot(_stream, _char, itemId, 3);
               break;
            case ConsoleKey.D5:
               Console.WriteLine($"Slot #5 contains: {GetItemName(_char.backpackSlot5)} ({_char.backpackSlot5}). Enter new item ID (0-255): ");
               itemId = Convert.ToByte(Console.ReadLine());
               SetItemInBackpackSlot(_stream, _char, itemId, 4);
               break;
            case ConsoleKey.D6:
               Console.WriteLine($"Slot #6 contains: {GetItemName(_char.backpackSlot6)} ({_char.backpackSlot6}). Enter new item ID (0-255): ");
               itemId = Convert.ToByte(Console.ReadLine());
               SetItemInBackpackSlot(_stream, _char, itemId, 5);
               break;
            default:
               break;
         }
         WriteChunk(_stream, _char.backpackChunk, _char.backpackOffset);
         WriteChunk(_stream, _char.backpackChargesChunk, _char.backpackChargesOffset);

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
      {
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

      // Set by input
      static void ModifyChunkUInt32(byte[] _chunk, string _chunkName)
      {
         uint input;

         Console.Write($"\nEnter new {_chunkName} value (0-9999999999): ");

         while (!UInt32.TryParse(Console.ReadLine(), out input))
         {
            Console.Write($"Enter a valid numerical value between 0 and 99999999999! New value: ");
         }

         byte[] newArray = BitConverter.GetBytes(input);

         Array.Clear(_chunk, 0, _chunk.Length);
         Array.Copy(newArray, _chunk, newArray.Length);
      }

      // Set directly
      static void ModifyChunkUInt32(byte[] _chunk, string _chunkName, uint _amount)
      {
         uint newVal = _amount;

         byte[] newArray = BitConverter.GetBytes(newVal);

         Array.Clear(_chunk, 0, _chunk.Length);
         Array.Copy(newArray, _chunk, newArray.Length);
      }

      static void ModifyNameChunk(byte[] _name)
      {
         bool isNameValid = false;
         byte[] newName = new byte[_name.Length];

         // Check if the name contains only uppercase latin characters and numerals from 0-9
         Regex rx = new Regex("^[A-Z0-9]*$");

         while (!isNameValid)
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

         _stream.Read(_char.unknownChunk1, 0, _char.unknownChunk1.Length);               // UNKNOWN #1 - 0xF

         _stream.Read(_char.sexChunk, 0, _char.sexChunk.Length);                         // Sex 0x10
         _stream.Read(_char.alignmentOriginalChunk, 0, _char.alignmentOriginalChunk.Length); // Original Alignment (on creation) 0x11
         _stream.Read(_char.alignmentCurrentChunk, 0, _char.alignmentCurrentChunk.Length); // Current Alignment (can be altered through actions) 0x12
         _stream.Read(_char.raceChunk, 0, _char.raceChunk.Length);                       // Race 0x13
         _stream.Read(_char.classChunk, 0, _char.classChunk.Length);                     // Character Class - 0x14
         _stream.Read(_char.statsChunk, 0, _char.statsChunk.Length);                     // Stats - 0x15 - 0x22

         _stream.Read(_char.levelChunk1, 0, _char.levelChunk1.Length);                   // Level - 0x23
         _stream.Read(_char.levelChunk2, 0, _char.levelChunk2.Length);                   // Level - 0x24
         _stream.Read(_char.ageChunk, 0, _char.ageChunk.Length);                         // Age Offset 37=0x25

         _stream.Read(_char.timesRestedChunk, 0, _char.timesRestedChunk.Length);         // Counts how often this character has rested - 0x26

         _stream.Read(_char.xpChunk, 0, _char.xpChunk.Length);                           // Experience - UInt32 0x27 - 0x2A

         _stream.Read(_char.magicPointsCurrentChunk, 0, _char.magicPointsCurrentChunk.Length); // Magic Points - 0x2B - 0x2C
         _stream.Read(_char.magicPointsMaxChunk, 0, _char.magicPointsMaxChunk.Length);   // Magic Points Max - 0x2D - 0x2E
         _stream.Read(_char.spellLevelChunk, 0, _char.spellLevelChunk.Length);           // Spell Level - 0x2F - 0x30

         _stream.Read(_char.gemsChunk, 0, _char.gemsChunk.Length);                       // Gems - ushort  0x31 - 0x32

         _stream.Read(_char.healthCurrentChunk, 0, _char.healthCurrentChunk.Length);     // Health Points Current - 0x33 - 0x34
         _stream.Read(_char.healthModifiedChunk, 0, _char.healthModifiedChunk.Length);   // Health Points Modified 0x35 - 0x36
         _stream.Read(_char.healthMaxChunk, 0, _char.healthMaxChunk.Length);             // Health Points Max - 0x37 - 0x38

         _stream.Read(_char.goldChunk, 0, _char.goldChunk.Length);                       // Gold - 0x39 - 0x3B

         _stream.Read(_char.armorClassFromItemsChunk, 0, _char.armorClassFromItemsChunk.Length); // Armor Class from Items - 0x3C
         _stream.Read(_char.armorClassTotalChunk, 0, _char.armorClassTotalChunk.Length);         // Armor Class - 0x3D

         _stream.Read(_char.foodChunk, 0, _char.foodChunk.Length);                       // Food - 0x3E
         _stream.Read(_char.conditionChunk, 0, _char.conditionChunk.Length);             // Condition - 0x3F

         _stream.Read(_char.equipmentChunk, 0, _char.equipmentChunk.Length);               // equipment - 0x40 - 0x45
         _stream.Read(_char.backpackChunk, 0, _char.backpackChunk.Length);                 // Inventory - 0x46 - 0x4B

         _stream.Read(_char.equipmentChargesChunk, 0, _char.equipmentChargesChunk.Length); // Equipment Charges - 0x4C - 0x57
         _stream.Read(_char.backpackChargesChunk, 0, _char.backpackChargesChunk.Length);   // Backpack Charges - 0x51 - 0x57

         _stream.Read(_char.resistancesChunk, 0, _char.resistancesChunk.Length);         // Resistances 0x58 - 0x67

         _stream.Read(_char.unknownChunk2, 0, _char.unknownChunk2.Length);               // UNKNOWN #2 0x68 - 0x6C

         _stream.Read(_char.questSideChunk, 0, _char.questSideChunk.Length);             // Currently active sidequest - 0x6D

         _stream.Read(_char.unknownChunk3, 0, _char.unknownChunk3.Length);               // UNKNOWN #3 0x6E - 0x6F

         _stream.Read(_char.questMainAct1Chunk, 0, _char.questMainAct1Chunk.Length);     // Main Quest Act 1 0x70

         _stream.Read(_char.unknownChunk4, 0, _char.unknownChunk4.Length);               // UNKNOWN #4 0x71 - 0x73

         _stream.Read(_char.progress1Chunk, 0, _char.progress1Chunk.Length);             // Misc Progress Tracker #1 0x74

         _stream.Read(_char.questLocationVisitChunk, 0, _char.questLocationVisitChunk.Length); // (Quest?) locations visited - 0x75

         _stream.Read(_char.unknownChunk5, 0, _char.unknownChunk5.Length);               // UNKNOWN #5 0x76 - 0x77

         _stream.Read(_char.questCompletedChunk, 0, _char.questCompletedChunk.Length);   // Quests completed - 0x78

         _stream.Read(_char.unknownChunk6, 0, _char.unknownChunk6.Length);               // UNKNOWN #6 0x79 - 0x7C

         _stream.Read(_char.questMainAct2Chunk, 0, _char.questMainAct2Chunk.Length);     // Main Quest Act 2 0x7D

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
         var s = BitConverter.ToString(_char.alignmentCurrentChunk);

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
            // Characters can have multiple conditions at the same time (not all)
            // The maximum is 7 conditions at the same time (0x7F)
            case "00": return "Good";
            case "01": return "Asleep";
            case "02": return "Blinded";
            case "03": return "Blinded, Asleep";
            case "04": return "Silenced";
            case "05": return "Silenced, Asleep";
            case "06": return "Silenced, Blinded";
            case "07": return "Silenced, Blinded, Asleep";
            case "08": return "Diseased";
            case "09": return "Diseased, Asleep";
            case "0A": return "Diseased, Blinded";
            case "0B": return "Diseased, Blinded, Asleep";
            case "0C": return "Diseased, Silenced";
            case "0D": return "Diseased, Silenced, Asleep";
            case "0E": return "Diseased, Silenced, Blinded";
            case "0F": return "Diseased, Silenced, Blinded, Asleep";
            case "10": return "Poisoned";
            case "11": return "Poisoned, Asleep";
            case "12": return "Poisoned, Blinded";
            case "13": return "Poisoned, Blinded, Asleep";
            case "14": return "Poisoned, Silenced";
            case "18": return "Poisoned, Diseased";
            case "20": return "Paralyzed";
            case "21": return "Paralyzed, Asleep";
            case "22": return "Paralyzed, Blinded";
            case "23": return "Paralyzed, Blinded, Asleep";
            case "28": return "Paralyzed, Diseased";
            case "29": return "Paralyzed, Diseased, Asleep";
            case "2B": return "Paralyzed, Diseased, Blinded, Asleep";
            case "30": return "Paralyzed, Poisoned";
            case "38": return "Paralyzed, Poisoned, Diseased";
            case "3A": return "Paralyzed, Poisoned, Diseased, Blinded";
            case "3B": return "Paralyzed, Poisoned, Diseased, Blinded, Asleep";
            case "3F": return "Paralyzed, Poisoned, Diseased, Silenced, Blinded, Asleep";
            case "40": return "Unconscious";
            case "50": return "Unconscious, Poisoned";
            case "60": return "Unconscious, Paralyzed";
            case "7F": return "Unconscious, Paralyzed, Poisoned, Diseased, Silenced, Blinded, Asleep";
            case "A0": return "Stone";
            case "C0": return "Dead";
            case "F0": return "Dead, Stone";
            case "FF": return "Eradicated";
            default: return $"Unknown ({s})";
         }
      }

      static string GetMainQuestAct1Progress(Character _char)
      {
         var s = BitConverter.ToString(_char.questMainAct1Chunk);

         switch (s)
         {
            case "00": return "Not begun"; // 0

            case "01": return "Talked to Old Man in Sorpigal Dungeon / Got Vellum Scroll";      // 1, gives quest item "Vellum Scroll" (0xE7), only if it's not in the backpack already
            case "02": return "Talked to Wizard Agar behind Erliquin Inn";                      // 2, needs "Vellum Scroll" (0xE7) in backpack, +1000 XP
            case "04": return "Talked to Telgoran (Robed Elf) in Dusk";                         // 4, needs "Vellum Scroll" (0xE7) in backpack, removes it, +2500 XP and +1500 Gold

            case "0C": return "Talked to Zom in Algary";                                        // 12, gives clue #1: "1-15"
            case "14": return "Talked to Zam in Portsmith";                                     // 20, gives clue #2: "C-15"
            case "1C": return "Talked to both Zom and Zam";                                     // 28, both clues: "C1; 15-15"

            case "24": return "Found Ruby Whistle";                                             // 36, gives quest item "Ruby Whistle" (0xE8), 2k gold and note: "Stronghold at B-3 14,2 - blow 2x"
            case "64": return "Entered Minotaur Stronghold in the Enchanted Forest (B-3 14,2)"; // 100 (+64)
            case "80": return "Found Dog Statue in Minotaur Stronghold";                        // 128 (+28) This is the last step of Act 1; also gives 10k XP (Search statue to get the Gold Key (0xEF))

            default: return $"Unknown ({s})";
         }
      }

      static string GetMainQuestAct2Progress(Character _char)
      {
         var s = BitConverter.ToString(_char.questMainAct2Chunk);

         switch (s)
         {
            case "00": return "Not begun"; // 0x0  (default, act 2 not started)

            case "01": return "Astral Projector #1 activated";                                  // 1 (1) // Astral Maze - Projectors, five in total, can be activated in any order
            case "02": return "Astral Projector #2 activated";                                  // 2 (2)
            case "03": return "Astral Projector #1 and #2 activated";                           // 3 (1+2)
            case "04": return "Astral Projector #3 activated";                                  // 4 (4)
            case "05": return "Astral Projector #1 and #3 activated";                           // 5 (1+4)
            case "06": return "Astral Projector #2 and #3 activated";                           // 6 (2+4)
            case "07": return "Astral Projector #1, #2 and #3 activated";                       // 7 (1+2+4)
            case "08": return "Astral Projector #4 activated";                                  // 8 (8)
            case "09": return "Astral Projector #1 and #4 activated";                           // 9 (1+8)
            case "0A": return "Astral Projector #2 and #4 activated";                           // 10 (2+8)
            case "0B": return "Astral Projector #1, #2 and #4 activated";                       // 11 (1+2+8)
            case "0C": return "Astral Projector #3 and #4 activated";                           // 12 (4+8)
            case "0D": return "Astral Projector #1, #3 and #4 activated";                       // 13 (1+4+8)
            case "0E": return "Astral Projector #2, #3 and #4 activated";                       // 14 (2+4+8)
            case "0F": return "Astral Projector #1, #2, #3 and #4 activated";                   // 15 (1+2+4+8)
            case "1F": return "All five Astral Projectors activated";                           // 16 (1+2+4+8+16)

            case "40": return "Soul Maze completed (False King Sheltem exposed)";               // 0x40 (64) Soul Maze has been completed (Sheltem answered)
            
            case "41": return "Soul Maze done & Astral Projector #1 activated";                 // 1 (1) // Astral Maze - Projectors, five in total, can be activated in any order
            case "42": return "Soul Maze done & Astral Projector #2 activated";                 // 2 (2)
            case "43": return "Soul Maze done & Astral Projector #1 and #2 activated";          // 3 (1+2)
            case "44": return "Soul Maze done & Astral Projector #3 activated";                 // 4 (4)
            case "45": return "Soul Maze done & Astral Projector #1 and #3 activated";          // 5 (1+4)
            case "46": return "Soul Maze done & Astral Projector #2 and #3 activated";          // 6 (2+4)
            case "47": return "Soul Maze done & Astral Projector #1, #2 and #3 activated";      // 7 (1+2+4)
            case "48": return "Soul Maze done & Astral Projector #4 activated";                 // 8 (8)
            case "49": return "Soul Maze done & Astral Projector #1 and #4 activated";          // 9 (1+8)
            case "4A": return "Soul Maze done & Astral Projector #2 and #4 activated";          // 10 (2+8)
            case "4B": return "Soul Maze done & Astral Projector #1, #2 and #4 activated";      // 11 (1+2+8)
            case "4C": return "Soul Maze done & Astral Projector #3 and #4 activated";          // 12 (4+8)
            case "4D": return "Soul Maze done & Astral Projector #1, #3 and #4 activated";      // 13 (1+4+8)
            case "4E": return "Soul Maze done & Astral Projector #2, #3 and #4 activated";      // 14 (2+4+8)
            case "4F": return "Soul Maze done & Astral Projector #1, #2, #3 and #4 activated";  // 15 (1+2+4+8)
            case "5F": return "Soul Maze done & All five Astral Projectors activated";          // 16 (1+2+4+8+16)

            case "FF": return "Game Completed! Inner Sanctum / Data Keeper reached";            // This triggers a different dialogue at B-1 4-15 hinting at M&M2 (also gives 500k XP) 

            default: return $"Unknown ({s})";
         }
      }

      static string GetTownName(Character _char)
      {
         string[] townNames = { "DELETED", "Sorpigal", "Portsmith", "Algary", "Dusk", "Erliquin" };

         var townName = townNames[_char.locationNum];

         return townName;
      }

      static string GetItemName(int _slot)
      {
         if (Dumper.isInitialized)
         {
            if (_slot == 0)
            {
               return "".PadRight(14);
            }

            return Dumper.items[_slot - 1].name;
         }

         var s = $"Item ID: ({_slot.ToString()})";

         return s.PadRight(14);

      }

      static string PrintChargesIfItemIsMagic(int _slot, int _chargesslot)
      {
         if (Dumper.isInitialized)
         {
            if (_slot == 0)
            {
               return "".PadLeft(3);
            }
            else if (Dumper.items[_slot - 1].isMagic)
            {
               string s = $"({_chargesslot.ToString().PadLeft(3)})";
               return s;
            }
            else
            {
               return "( N )".PadLeft(3);
            }
         }

         return "".PadLeft(3);
      }

      static void PrintCharacterHeader()
      {
         Console.WriteLine("#  Name            Sex Alignm. Race     Class    Age Cond. Lv XP      Town    ");
         Console.WriteLine("-- --------------- --- ------- -------- -------- --- ----- -- ------- --------");
      }

      static void PrintCharacterShort(Character _char)
      {
         Console.WriteLine($"{_char.indexNum + 1}  {_char.name} {GetSexFromChunk(_char).PadRight(3)} {GetAlignmentFromChunk(_char).PadRight(7)} {GetRaceFromChunk(_char).PadRight(8)} {GetClassFromChunk(_char).PadRight(8)} {_char.ageNum}  {GetConditionFromChunk(_char).PadRight(5)} {_char.levelNum.ToString().PadLeft(2)} {_char.xpNum.ToString().PadLeft(7)} {GetTownName(_char)}");
      }

      static void PrintCharacter(Character _char)
      {
         Console.Clear();

         PrintCharacterHeader();
         PrintCharacterShort(_char);
         Console.WriteLine();

         // HP Current (Modified) / Max & MP Current / Max (Spell Level)
         Console.WriteLine($"Health: {BitConverter.ToUInt16(_char.healthCurrentChunk, 0)} ({BitConverter.ToUInt16(_char.healthModifiedChunk, 0)}) / {BitConverter.ToUInt16(_char.healthMaxChunk, 0)}     Magic Points: {BitConverter.ToUInt16(_char.magicPointsCurrentChunk, 0)}/{BitConverter.ToUInt16(_char.magicPointsMaxChunk, 0)} (Spell Level: {_char.spellLvlNum})");

         // Stats 
         Console.WriteLine($"Stats: INT {_char.statIntellect}/{_char.statIntellectTemp}  MGT {_char.statMight}/{_char.statMightTemp}  PER {_char.statPersonality}/{_char.statPersonalityTemp}\n       END {_char.statEndurance}/{_char.statEnduranceTemp}  SPD {_char.statSpeed}/{_char.statSpeedTemp}  ACC {_char.statAccuracy}/{_char.statAccuracyTemp}  LCK {_char.statLuck}/{_char.statLuckTemp}");
         Console.WriteLine();

         // Equipment & Backpack
         Console.WriteLine($"Equipment".PadRight(14) + "(Charges)".PadRight(12) + "Backpack".PadRight(14) + "(Charges)".PadRight(12) + "Other");
         Console.WriteLine($"1. {GetItemName(_char.equipSlot1)} {PrintChargesIfItemIsMagic(_char.equipSlot1, _char.equipChargesSlot1).PadRight(5)} | 1. {GetItemName(_char.backpackSlot1)} {PrintChargesIfItemIsMagic(_char.backpackSlot1, _char.backpackChargesSlot1).PadRight(5)} | Gold: {_char.goldNum} ");
         Console.WriteLine($"2. {GetItemName(_char.equipSlot2)} {PrintChargesIfItemIsMagic(_char.equipSlot2, _char.equipChargesSlot2).PadRight(5)} | 2. {GetItemName(_char.backpackSlot2)} {PrintChargesIfItemIsMagic(_char.backpackSlot2, _char.backpackChargesSlot2).PadRight(5)} | Gems: {BitConverter.ToUInt16(_char.gemsChunk, 0)}");
         Console.WriteLine($"3. {GetItemName(_char.equipSlot3)} {PrintChargesIfItemIsMagic(_char.equipSlot3, _char.equipChargesSlot3).PadRight(5)} | 3. {GetItemName(_char.backpackSlot3)} {PrintChargesIfItemIsMagic(_char.backpackSlot3, _char.backpackChargesSlot3).PadRight(5)} | Food: {_char.foodNum}");
         Console.WriteLine($"4. {GetItemName(_char.equipSlot4)} {PrintChargesIfItemIsMagic(_char.equipSlot4, _char.equipChargesSlot4).PadRight(5)} | 4. {GetItemName(_char.backpackSlot4)} {PrintChargesIfItemIsMagic(_char.backpackSlot4, _char.backpackChargesSlot4).PadRight(5)} |");
         Console.WriteLine($"5. {GetItemName(_char.equipSlot5)} {PrintChargesIfItemIsMagic(_char.equipSlot5, _char.equipChargesSlot5).PadRight(5)} | 5. {GetItemName(_char.backpackSlot5)} {PrintChargesIfItemIsMagic(_char.backpackSlot5, _char.backpackChargesSlot5).PadRight(5)} |");
         Console.WriteLine($"6. {GetItemName(_char.equipSlot6)} {PrintChargesIfItemIsMagic(_char.equipSlot6, _char.equipChargesSlot6).PadRight(5)} | 6. {GetItemName(_char.backpackSlot6)} {PrintChargesIfItemIsMagic(_char.backpackSlot6, _char.backpackChargesSlot6).PadRight(5)} |");
         Console.WriteLine();

         // Resistances 0x58 - 0x67
         Console.WriteLine($"Resistances: Magic  {_char.resMagic1}%/{_char.resMagic2}%  Fire   {_char.resFire1}%/{_char.resFire2}%  Cold   {_char.resCold1}%/{_char.resCold2}%  Elec   {_char.resElec1}%/{_char.resElec2}%\n             Acid   {_char.resAcid1}%/{_char.resAcid2}% Fear   {_char.resFear1}%/{_char.resFear2}% Poison {_char.resPoison1}%/{_char.resPoison2}% Sleep  {_char.resSleep1}%/{_char.resSleep2}%");

         Console.WriteLine($"Main Quest Progress");
         Console.WriteLine($"Act 1: {GetMainQuestAct1Progress(_char)} (0x{BitConverter.ToString(_char.questMainAct1Chunk)})");
         Console.WriteLine($"Act 2: {GetMainQuestAct2Progress(_char)} (0x{BitConverter.ToString(_char.questMainAct2Chunk)})");

         Console.WriteLine($"DEBUG - Unknown Chunks: [{BitConverter.ToString(_char.unknownChunk1)}] [{BitConverter.ToString(_char.unknownChunk2)}] [{BitConverter.ToString(_char.unknownChunk3)}] [{BitConverter.ToString(_char.unknownChunk4)}] [{BitConverter.ToString(_char.unknownChunk5)}] [{BitConverter.ToString(_char.unknownChunk6)}]");

         Console.WriteLine("----------------------------------------------------------------------------");
      }
   }
}
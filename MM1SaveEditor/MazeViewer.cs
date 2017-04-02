using System;
using System.IO;


namespace MM1SaveEditor
{
   class MazeViewer
   {
      static string MAZEDATA_FILE_NAME = "MAZEDATA.DTA";
      static string VERSION_NUMBER = "v0.1";

      static int mazeAmount = 28160 / 256;

      static Maze[] mazes = new Maze[mazeAmount];
      static int[] mazeOffsets = new int[mazeAmount];

      public void Init()
      {
         if (File.Exists(MAZEDATA_FILE_NAME))
         {
            Console.Write($"Opening {MAZEDATA_FILE_NAME}... ");

            using (var stream = File.Open(MAZEDATA_FILE_NAME, FileMode.Open, FileAccess.Read))
            {
               Console.WriteLine("Success!\n");
               Console.WriteLine($"Might and Magic 1 Maze Viewer ({VERSION_NUMBER}) by ryz\n");

               InitializeMazeData();

               for (int i = 0; i < mazes.Length; i++)
               {
                  ParseMaze(stream, mazes[i]);
               }

               PrintMaze(mazes[3]);

               Console.ReadLine();
            }
         }
         else
         {
            Console.WriteLine($"File {MAZEDATA_FILE_NAME} not found! Make sure it's in the same folder as this program.\nAborting.");
         }
      }

      static void InitializeMazeData()
      {
         for (int i = 0; i < mazes.Length; i++)
         {
            int mazeChunkSize = 256;

            mazeOffsets[i] = i * mazeChunkSize;

            mazes[i] = new Maze();
            mazes[i].offset = mazeOffsets[i];
            mazes[i].id = i;
         }
      }

      static void ParseMaze(FileStream _stream, Maze _maze)
      {
         _stream.Position = _maze.offset;

         _stream.Read(_maze.dataChunk, 0, _maze.dataChunk.Length);

         if (BitConverter.IsLittleEndian)
         {
            Array.Reverse(_maze.dataChunk);
         }

      }

      static void PrintMaze(Maze _maze)
      {
         //Console.WriteLine($"{BitConverter.ToString(_maze.dataChunk)}");

         Array.Copy(_maze.dataChunk, 0, _maze.dataLine1, 0, _maze.dataLine1.Length);
         Array.Copy(_maze.dataChunk, 16, _maze.dataLine2, 0, _maze.dataLine2.Length);
         Array.Copy(_maze.dataChunk, 16 * 2, _maze.dataLine3, 0, _maze.dataLine3.Length);
         Array.Copy(_maze.dataChunk, 16 * 3, _maze.dataLine4, 0, _maze.dataLine4.Length);
         Array.Copy(_maze.dataChunk, 16 * 4, _maze.dataLine5, 0, _maze.dataLine5.Length);
         Array.Copy(_maze.dataChunk, 16 * 5, _maze.dataLine6, 0, _maze.dataLine6.Length);
         Array.Copy(_maze.dataChunk, 16 * 6, _maze.dataLine7, 0, _maze.dataLine7.Length);
         Array.Copy(_maze.dataChunk, 16 * 7, _maze.dataLine8, 0, _maze.dataLine8.Length);
         Array.Copy(_maze.dataChunk, 16 * 8, _maze.dataLine9, 0, _maze.dataLine9.Length);
         Array.Copy(_maze.dataChunk, 16 * 9, _maze.dataLine10, 0, _maze.dataLine10.Length);
         Array.Copy(_maze.dataChunk, 16 * 10, _maze.dataLine11, 0, _maze.dataLine11.Length);
         Array.Copy(_maze.dataChunk, 16 * 11, _maze.dataLine12, 0, _maze.dataLine12.Length);
         Array.Copy(_maze.dataChunk, 16 * 12, _maze.dataLine13, 0, _maze.dataLine13.Length);
         Array.Copy(_maze.dataChunk, 16 * 13, _maze.dataLine14, 0, _maze.dataLine14.Length);
         Array.Copy(_maze.dataChunk, 16 * 14, _maze.dataLine15, 0, _maze.dataLine15.Length);
         Array.Copy(_maze.dataChunk, 16 * 15, _maze.dataLine16, 0, _maze.dataLine16.Length);

         Array.Reverse(_maze.dataLine1);
         Array.Reverse(_maze.dataLine2);
         Array.Reverse(_maze.dataLine3);
         Array.Reverse(_maze.dataLine4);
         Array.Reverse(_maze.dataLine5);
         Array.Reverse(_maze.dataLine6);
         Array.Reverse(_maze.dataLine7);
         Array.Reverse(_maze.dataLine8);
         Array.Reverse(_maze.dataLine9);
         Array.Reverse(_maze.dataLine10);
         Array.Reverse(_maze.dataLine11);
         Array.Reverse(_maze.dataLine12);
         Array.Reverse(_maze.dataLine13);
         Array.Reverse(_maze.dataLine14);
         Array.Reverse(_maze.dataLine15);
         Array.Reverse(_maze.dataLine16);

         Console.WriteLine();
         Console.WriteLine($"Maze:");
         Console.WriteLine("+" + "-".PadRight(47).Replace(" ", "-") + "+");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine1))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine2))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine3))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine4))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine5))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine6))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine7))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine8))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine9))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine10))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine11))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine12))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine13))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine14))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine15))}|");
         Console.WriteLine($"|{MazeGfx(BitConverter.ToString(_maze.dataLine16))}|");
         Console.WriteLine("+" + "-".PadRight(47).Replace(" ", "-") + "+");

      }

      static string MazeGfx(string _s)
      {
         return _s.Replace("-", " ").Replace("11", " ║").Replace("80", " ╬").Replace("00", " ╬").Replace("55", " #").Replace("44", " ═").Replace("C4", " ═").Replace("95", " U").Replace("15", " U").Replace("14", " ╝").Replace("C1", " ╔").Replace("41", " ╔").Replace("81", " ╟").Replace("01", " ╠").Replace("05", " ╚").Replace("85", " ╚").Replace("50", " ╗").Replace("40", " ╦").Replace("C0", " ╦").Replace("45", " [").Replace("C5", " [").Replace("54", " ]").Replace("D4", " ]").Replace("51", " П").Replace("D1", " П").Replace("91", " П").Replace("04", " ╩").Replace("84", " ╩").Replace("90", " ╣").Replace("10", " ╣").Replace(" ", "");
      }
   }
}
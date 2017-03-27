using System;
using System.Text;

namespace MM1SaveEditor
{
   class Monster
   {
      // Offset 0x1B312 - 0x1CB71 (6240 byte)
      // 32 Byte per Monster, 6240 / 32 = 195 monsters total

      // Missing info: HP, "Bonus on Touch", "Special Ability", "Magic Resistance"
      // Slither Beast: 7, N,N,N
      // Mutant Larva: 4-8, Y,N,N
      // Battle Rat: 6, Y,N,N
      // Gnome: 5, N,N,Y

      public int offset { get; set; }
      public int id { get; set; }

      public byte[] nameChunk { get; set; } = new byte[15];
      public string name { get { return Encoding.Default.GetString(nameChunk); } }

      public byte[] dataChunk { get; set; } = new byte[2]; // 3

      public byte[] healthChunk { get; set; } = new byte[1];
      public int healthMin { get { return healthChunk[0] + 1; } }
      public int healthMax { get { return healthChunk[0] + 8; } }

      public byte[] acChunk { get; set; } = new byte[1];
      public int ac { get { return acChunk[0]; } }

      public byte[] damageChunk { get; set; } = new byte[1];
      public int damage { get { return damageChunk[0]; } }

      public byte[] attacksChunk { get; set; } = new byte[1];
      public int attacks { get { return attacksChunk[0]; } }

      public byte[] speedChunk { get; set; } = new byte[1];
      public int speed { get { return speedChunk[0]; } }

      public byte[] xpChunk { get; set; } = new byte[2];
      public int xp { get { return BitConverter.ToUInt16(xpChunk, 0); } }

      public byte[] dataChunk2 { get; set; } = new byte[8];

   }
}

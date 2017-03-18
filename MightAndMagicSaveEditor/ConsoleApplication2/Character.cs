using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MightAndMagicSaveEditor
{
   class Character
   {
      public int offset { get; set; } = 0;
      public byte[] nameChunk { get; set; } = new byte[15]; // Offset 0=0x0

      public int nameOffset { get { return offset; } }


      public byte[] unknownChunk1 { get; set; } = new byte[1]; // Offset 15=0xF

      public byte[] sexChunk { get; set; } = new byte[1]; // Offset 16=0x10

      public int sexOffset { get { return offset + 16; }
      }

      public byte[] unknownChunk2 { get; set; } = new byte[1]; // Offset 17=0x11

      public byte[] alignmentChunk { get; set; } = new byte[1]; // Offset 18=0x12
      public int alignmentOffset { get { return offset + 18; } }

      public byte[] raceChunk { get; set; } = new byte[1]; // Offset 19=0x13
      public int raceOffset { get { return offset + 19; } }

      public byte[] classChunk { get; set; } = new byte[1]; // Offset 20=0x14
      public int classOffset { get { return offset + 20; } }

      // Stats, there are seven statistics for each character, two bytes each.
      public byte[] statsChunk { get; set; } = new byte[14]; // Offset 21=0x15

      public int statsIntellect1   { get { return statsChunk[0]; } }
      public int statsIntellect2   { get { return statsChunk[1]; } }
      public int statsMight1       { get { return statsChunk[2]; } }
      public int statsMight2       { get { return statsChunk[3]; } }
      public int statsPersonality1 { get { return statsChunk[4]; } }
      public int statsPersonality2 { get { return statsChunk[5]; } }
      public int statsEndurance1   { get { return statsChunk[6]; } }
      public int statsEndurance2   { get { return statsChunk[7]; } }
      public int statsSpeed1       { get { return statsChunk[8]; } }
      public int statsSpeed2       { get { return statsChunk[9]; } }
      public int statsAccuracy1    { get { return statsChunk[10]; } }
      public int statsAccuracy2    { get { return statsChunk[11]; } }
      public int statsLuck1        { get { return statsChunk[12]; } }
      public int statsLuck2        { get { return statsChunk[13]; } }

      public byte[] levelChunk1 { get; set; } = new byte[1]; // Offset 35=0x23
      public byte[] levelChunk2 { get; set; } = new byte[1]; // Offset 36=0x24

      public byte[] ageChunk { get; set; } = new byte[1]; // Offset 37=0x25

      public byte[] unknownChunk3 { get; set; } = new byte[1];  // Offset 38=0x26

      // XP, stored as UInt24
      public byte[] xpChunk { get; set; } = new byte[3]; // Offset 39=0x27
      public int xpOffset { get { return offset + 39; } }

      public byte[] unknownChunk4 { get; set; } = new byte[1]; // Offset 42=0x2A

      public byte[] magicPointsCurrentChunk { get; set; } = new byte[2]; // Offset 43=0x2B
      public byte[] magicPointsMaxChunk { get; set; } = new byte[2]; // Offset 45=0x2D

      public byte[] spellLevelChunk { get; set; } = new byte[2]; // Offset 47=0x2F

      public byte[] gemsChunk { get; set; } = new byte[2]; // Offset 49=0x31
      public int gemsOffset { get { return offset + 49; } }

      public byte[] healthCurrentChunk { get; set; } = new byte[2]; // Offset 51=0x33
      public byte[] healthModifiedChunk { get; set; } = new byte[2]; // Offset 53
      public byte[] healthMaxChunk { get; set; } = new byte[2]; // Offset 55

      public byte[] goldChunk { get; set; } = new byte[3];  // Offset 57=0x39
      public int goldOffset { get { return offset + 57; } }


      public byte[] unknownChunk7 { get; set; } = new byte[1]; // Offset 58=0x3A

      public byte[] armorClassChunk { get; set; } = new byte[1]; // Offset 62=0x3D

      public byte[] foodChunk { get; set; } = new byte[1]; // Offset 62=0x3E

      public byte[] conditionChunk { get; set; } = new byte[1]; // Offset 63=0x3F

      public byte[] equippedWeaponChunk { get; set; } = new byte[1]; // Offset 64=0x40
      public byte[] equippedGearChunk { get; set; } = new byte[5]; // Offset 65=0x41
      public byte[] inventoryChunk { get; set; } = new byte[6]; // Offset 70=0x46

      public byte[] equipmentChargesChunk { get; set; } = new byte[12];// Offset 76=0x4C 

      public byte[] resistancesChunk { get; set; } = new byte[16]; // Offset 88=0x58

      public int resMagic1  { get { return resistancesChunk[0]; } }
      public int resMagic2  { get { return resistancesChunk[1]; } }
      public int resFire1   { get { return resistancesChunk[2]; } }
      public int resFire2   { get { return resistancesChunk[3]; } }
      public int resCold1   { get { return resistancesChunk[4]; } }
      public int resCold2   { get { return resistancesChunk[5]; } }
      public int resElec1   { get { return resistancesChunk[6]; } }
      public int resElec2   { get { return resistancesChunk[7]; } }
      public int resAcid1   { get { return resistancesChunk[8]; } }
      public int resAcid2   { get { return resistancesChunk[9]; } }
      public int resFear1   { get { return resistancesChunk[10]; } }
      public int resFear2   { get { return resistancesChunk[11]; } }
      public int resPoison1 { get { return resistancesChunk[12]; } }
      public int resPoison2 { get { return resistancesChunk[13]; } }
      public int resSleep1  { get { return resistancesChunk[14]; } }
      public int resSleep2  { get { return resistancesChunk[15]; } }

      public byte[] unknownChunk8 { get; set; } = new byte[22]; // Offset 104=0x68 - biggest chunk, probably contains various progress/quest-related data

      public byte[] characterIndexChunk { get; set; } = new byte[1]; // Offset 126=0x7E
   }

}

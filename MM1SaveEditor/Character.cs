﻿using System;
using System.Text;

namespace MM1SaveEditor
{
   class Character
   {
      public bool exists = false;

      public int offset                      { get; set; } = 0;

      public byte[] nameChunk                { get; set; } = new byte[15]; // Offset 0=0x0
      public int nameOffset                  { get { return offset; } }
      public string name                     { get { return Encoding.Default.GetString(nameChunk); } }

      public byte[] unknownChunk1            { get; set; } = new byte[1]; // Offset 15=0xF, quite sure this is a null terminator for the name string, is always 0x0

      public byte[] sexChunk                 { get; set; } = new byte[1]; // Offset 16=0x10
      public int sexOffset                   { get { return offset + 0x10; } }

      public byte[] alignmentOriginalChunk   { get; set; } = new byte[1]; // Offset 17=0x11
      public byte[] alignmentCurrentChunk    { get; set; } = new byte[1]; // Offset 18=0x12
      public int alignmentCurrentOffset      { get { return offset + 0x12; } }

      public byte[] raceChunk                { get; set; } = new byte[1]; // Offset 19=0x13
      public int raceOffset                  { get { return offset + 0x13; } }

      public byte[] classChunk               { get; set; } = new byte[1]; // Offset 20=0x14
      public int classOffset                 { get { return offset + 0x14; } }

      // Stats, there are seven statistics for each character, two bytes each.
      // Temp stats are always the "active" ones and are reset to the given normal stat after resting.
      public byte[] statsChunk               { get; set; } = new byte[14]; // Offset 21=0x15
      public int statsOffset                 { get { return offset + 0x15; } }

      public int statIntellect               { get { return statsChunk[0]; } }
      public int statIntellectTemp           { get { return statsChunk[1]; } }
      public int statMight                   { get { return statsChunk[2]; } }
      public int statMightTemp               { get { return statsChunk[3]; } }
      public int statPersonality             { get { return statsChunk[4]; } }
      public int statPersonalityTemp         { get { return statsChunk[5]; } }
      public int statEndurance               { get { return statsChunk[6]; } }
      public int statEnduranceTemp           { get { return statsChunk[7]; } }
      public int statSpeed                   { get { return statsChunk[8]; } }
      public int statSpeedTemp               { get { return statsChunk[9]; } }
      public int statAccuracy                { get { return statsChunk[10]; } }
      public int statAccuracyTemp            { get { return statsChunk[11]; } }
      public int statLuck                    { get { return statsChunk[12]; } }
      public int statLuckTemp                { get { return statsChunk[13]; } }

      public byte[] levelChunk1              { get; set; } = new byte[1]; // Offset 35=0x23
      public byte[] levelChunk2              { get; set; } = new byte[1]; // Offset 36=0x24
      public int levelNum                    { get { return levelChunk1[0]; } }

      public byte[] ageChunk                 { get; set; } = new byte[1]; // Offset 37=0x25
      public int ageNum                      { get { return ageChunk[0]; } }

      // This byte counts the number of times this character has rested; resting increases the value by one
      // Resting while this is 0xFF resets it to 0x0 and increments the character's age by one
      // This aims to roughly simulate aging in the form of "passing time"
      // as 255 is "close" to one year (365 days) and thus the character slowly ages over time
      public byte[] timesRestedChunk         { get; set; } = new byte[1];  // Offset 38=0x26

      // XP, stored as UInt32
      public byte[] xpChunk                  { get; set; } = new byte[4]; // Offset 39=0x27
      public int xpOffset                    { get { return offset + 0x27; } }
      public uint xpNum                      { get { return BitConverter.ToUInt32(xpChunk, 0); } }

      public byte[] magicPointsCurrentChunk  { get; set; } = new byte[2]; // Offset 43=0x2B
      public byte[] magicPointsMaxChunk      { get; set; } = new byte[2]; // Offset 45=0x2D

      public byte[] spellLevelChunk          { get; set; } = new byte[2]; // Offset 47=0x2F
      public int spellLvlNum                 { get { return spellLevelChunk[0]; } }

      public byte[] gemsChunk                { get; set; } = new byte[2]; // Offset 49=0x31
      public int gemsOffset                  { get { return offset + 0x31; } }

      public byte[] healthCurrentChunk       { get; set; } = new byte[2]; // Offset 51=0x33
      public byte[] healthModifiedChunk      { get; set; } = new byte[2]; // Offset 53
      public byte[] healthMaxChunk           { get; set; } = new byte[2]; // Offset 55

      public byte[] goldChunk                { get; set; } = new byte[3];  // Offset 57=0x39
      public int goldOffset                  { get { return offset + 0x39; } }
      public int goldNum                     { get { return (goldChunk[2] << 16) | (goldChunk[1] << 8) | goldChunk[0]; } }

      // The innate AC depends on the class and is set on character creation.
      // Total AC is always this plus AC from items.
      public byte[] armorClassFromItemsChunk { get; set; } = new byte[1]; // Offset 61=0x3C
      public byte[] armorClassTotalChunk     { get; set; } = new byte[1]; // Offset 62=0x3D
      public int acFromItemsNum              { get { return armorClassFromItemsChunk[0]; } }
      public int acTotalNum                  { get { return armorClassTotalChunk[0]; } }

      public byte[] foodChunk                { get; set; } = new byte[1]; // Offset 62=0x3E
      public int foodOffset                  { get { return offset + 0x3E; } }
      public int foodNum                     { get { return foodChunk[0]; } }

      public byte[] conditionChunk           { get; set; } = new byte[1]; // Offset 63=0x3F

      public byte[] equipmentChunk           { get; set; } = new byte[6]; // Offset 64=0x40

      public int equipSlot1                  { get { return equipmentChunk[0]; } }
      public int equipSlot2                  { get { return equipmentChunk[1]; } }
      public int equipSlot3                  { get { return equipmentChunk[2]; } }
      public int equipSlot4                  { get { return equipmentChunk[3]; } }
      public int equipSlot5                  { get { return equipmentChunk[4]; } }
      public int equipSlot6                  { get { return equipmentChunk[5]; } }

      public byte[] backpackChunk            { get; set; } = new byte[6]; // Offset 70=0x46
      public int backpackOffset               { get { return offset + 0x46; } }

      public int backpackSlot1               { get { return backpackChunk[0]; } }
      public int backpackSlot2               { get { return backpackChunk[1]; } }
      public int backpackSlot3               { get { return backpackChunk[2]; } }
      public int backpackSlot4               { get { return backpackChunk[3]; } }
      public int backpackSlot5               { get { return backpackChunk[4]; } }
      public int backpackSlot6               { get { return backpackChunk[5]; } }

      public byte[] equipmentChargesChunk    { get; set; } = new byte[6];// Offset 76=0x4C 

      public int equipChargesSlot1           { get { return equipmentChargesChunk[0]; } }
      public int equipChargesSlot2           { get { return equipmentChargesChunk[1]; } }
      public int equipChargesSlot3           { get { return equipmentChargesChunk[2]; } }
      public int equipChargesSlot4           { get { return equipmentChargesChunk[3]; } }
      public int equipChargesSlot5           { get { return equipmentChargesChunk[4]; } }
      public int equipChargesSlot6           { get { return equipmentChargesChunk[5]; } }

      public byte[] backpackChargesChunk     { get; set; } = new byte[6];// Offset 82=0x51 
      public int backpackChargesOffset       { get { return offset + 0x51; } }

      public int backpackChargesSlot1        { get { return backpackChargesChunk[0]; } }
      public int backpackChargesSlot2        { get { return backpackChargesChunk[1]; } }
      public int backpackChargesSlot3        { get { return backpackChargesChunk[2]; } }
      public int backpackChargesSlot4        { get { return backpackChargesChunk[3]; } }
      public int backpackChargesSlot5        { get { return backpackChargesChunk[4]; } }
      public int backpackChargesSlot6        { get { return backpackChargesChunk[5]; } }

      public byte[] resistancesChunk         { get; set; } = new byte[16]; // Offset 88=0x58

      public int resMagic1                   { get { return resistancesChunk[0]; } }
      public int resMagic2                   { get { return resistancesChunk[1]; } }
      public int resFire1                    { get { return resistancesChunk[2]; } }
      public int resFire2                    { get { return resistancesChunk[3]; } }
      public int resCold1                    { get { return resistancesChunk[4]; } }
      public int resCold2                    { get { return resistancesChunk[5]; } }
      public int resElec1                    { get { return resistancesChunk[6]; } }
      public int resElec2                    { get { return resistancesChunk[7]; } }
      public int resAcid1                    { get { return resistancesChunk[8]; } }
      public int resAcid2                    { get { return resistancesChunk[9]; } }
      public int resFear1                    { get { return resistancesChunk[10]; } }
      public int resFear2                    { get { return resistancesChunk[11]; } }
      public int resPoison1                  { get { return resistancesChunk[12]; } }
      public int resPoison2                  { get { return resistancesChunk[13]; } }
      public int resSleep1                   { get { return resistancesChunk[14]; } }
      public int resSleep2                   { get { return resistancesChunk[15]; } }

      public byte[] unknownChunk2            { get; set; } = new byte[5]; // Offset 104=0x68 - probably contains various progress/quest-related data

      // This is _probably_ the sidequest byte. Can't have multiple quests active
      // Quest steps:
      // 00 (no quest active)
      //
      // Lord Inspectron, Castle Blackridge North
      // 08 - Q1 active (Find the Ancient Ruins in the Quivering Forest) 
      // 09 - Q2 active (Visit Blithes Peak and Report)
      // 0A - Q3 active (Get Cactus Nectar by trading with Nomads in Desert)
      // 0B - Q4 active (Find Shrine of Okzar in Dusk Dungeon)
      //
      // Lord Hacker, Castle Blackridge South
      // 0F - Q1 active (
      //
      // False King Alamar
      // FF - Q1 active (false quest which can't be completed, has to be removed via spell: C3/7)
      public byte[] questSideChunk           { get; set; } = new byte[1]; // Offset 0x6D  

      public byte[] unknownChunk3            { get; set; } = new byte[2]; // Offset 0x6E - probably contains various progress/quest-related data

      // This tracks the progress of main quest, act 1
      public byte[] questMainAct1Chunk              { get; set; } = new byte[1]; // Offset 0x70
      public int questOffset                 { get { return offset + 0x70; } }

      public byte[] unknownChunk4            { get; set; } = new byte[3]; // Offset 0x71 - probably contains various progress/quest-related data

      // Answering the signs (given by the Gypsy) correctly at A-4,4-6 adds 0x80 (128) to this (each character individually)
      // Also observed values 0x0B, 0x0E, 0x0F
      public byte[] progress1Chunk           { get; set; } = new byte[1]; // Offset 0x74

      // This is _probably_ the "(quest) locations visited" byte. Set if...
      // 0x00 (none)
      // 0x01 (Wizard’s Lair, B-1, at 13,5. Just enter and leave. Inspectron Q1, adds 0x1
      // 0x03 (Blithe's Peak, B-3 at 9,6.) Inspectron Q2, adds 0x2 
      // 0x07 (Cactus Nectar given to Inspectron) Inspectron Q3, adds 0x4 (only to character who carries it)
      public byte[] questLocationVisitChunk  { get; set; } = new byte[1]; // Offset 0x75

      public byte[] unknownChunk5            { get; set; } = new byte[2]; // Offset 0x76 - probably contains various progress/quest-related data

      // This is _probably_ the "side quest completed" byte
      // 0x00 (none)
      // 0x01 (Inspectron Q1 complete) adds 0x1
      // 0x03 (Inspectron Q2 complete) adds 0x2
      // 0x07 (Inspectron Q2 complete) adds 0x4 (all characters)
      public byte[] questCompletedChunk { get; set; } = new byte[1]; // Offset 0x78

      public byte[] unknownChunk6            { get; set; } = new byte[4]; // Offset 0x79 - probably contains various progress/quest-related data

      // This tracks the progress of the Main Quest, Act 2
      public byte[] questMainAct2Chunk              { get; set; } = new byte[1]; // Offset 0x7D

      public byte[] indexChunk               { get; set; } = new byte[1]; // Offset 0x7E
      public int indexNum                    { get { return indexChunk[0]; } }

      public int locationNum                 { get; set; } = 0;

   }

}

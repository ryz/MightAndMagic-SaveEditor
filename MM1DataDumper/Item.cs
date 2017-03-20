using System;

namespace MM1DataDumper
{
   class Item
   {
      public int offset { get; set; }

      public byte[] nameChunk { get; set; } = new byte[14];
      public byte[] classChunk { get; set; } = new byte[1]; // Mask which determines who can equip this item
      public byte[] specialChunk { get; set; } = new byte[1];
      public byte[] specialAmountChunk { get; set; } = new byte[1];
      public byte[] isMagicChunk { get; set; } = new byte[1]; //
      public byte[] unknownChunk { get; set; } = new byte[1]; // ???
      public byte[] chargesChunk { get; set; } = new byte[1]; // Charges if it's magic.
      public byte[] valueChunk { get; set; } = new byte[2]; // Price if bought. sell price is half of that. Stored as big endian.
      public byte[] damageChunk { get; set; } = new byte[1]; // Only used by weapons
      public byte[] bonusChunk { get; set; } = new byte[1]; // Either flat damage or AC bonus, depending on item type


   }
}

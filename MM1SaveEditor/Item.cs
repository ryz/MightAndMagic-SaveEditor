using System;
using System.Text;

namespace MM1SaveEditor
{
   class Item
   {
      public int offset { get; set; }
      public int id { get; set; }

      public byte[] nameChunk { get; set; } = new byte[14];
      public string name { get { return Encoding.Default.GetString(nameChunk); } }

      public byte[] classChunk { get; set; } = new byte[1]; // Mask which determines who can equip this item

      public byte[] specialChunk { get; set; } = new byte[1];
      public byte[] specialAmountChunk { get; set; } = new byte[1];
      public int specialAmount { get { return specialAmountChunk[0]; } }

      public byte[] magicStateChunk { get; set; } = new byte[1]; // indicates if an item is magic and what type it is
      public bool isMagic
      { get
         {
            if (magicStateChunk[0] == 0)
            {
               return false;
            }
            else
            {
               return true;
            }
         }
      }

      public byte[] magicEffectChunk { get; set; } = new byte[1]; // 
      public byte[] chargesChunk { get; set; } = new byte[1]; // Charges if it's magic.
      public int charges { get { return chargesChunk[0]; } }

      public byte[] valueChunk { get; set; } = new byte[2]; // Price if bought. sell price is half of that. Stored as big endian.
      public int value { get { return BitConverter.ToUInt16(valueChunk, 0); } }

      public byte[] damageChunk { get; set; } = new byte[1]; // Only used by weapons
      public int damage { get { return damageChunk[0]; } }

      public byte[] bonusChunk { get; set; } = new byte[1]; // Either flat damage or AC bonus, depending on item type
      public int bonus { get { return bonusChunk[0]; } }

      public string category { get; set; }
   }
}

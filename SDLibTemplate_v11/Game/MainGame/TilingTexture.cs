using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SDLibTemplate_v11.Game.MainGame
{
    public class TilingTexture
    {
        public static Dictionary<string, Texture2D> SpriteSheets { get; private set; }
        public static Rectangle GetRectanlge(int id, int textureScale)
        {
            Point[] shifts = new Point[] { 
            new Point(0,0),
            new Point(1,0),
            new Point(2,0),
            new Point(0,1),
            new Point(1,1),
            new Point(2,1),
            new Point(0,2),
            new Point(1,2),
            new Point(2,2),

            new Point(1,3),
            new Point(0,4),
            new Point(1,4),
            new Point(2,4),
            new Point(1,5),

            };
            Rectangle rect = new Rectangle(shifts[id].X * textureScale/3,
                shifts[id].Y * textureScale/3, textureScale / 3, textureScale/3);
            return rect;
        }
        public static int ConvertToIndex(bool groundLeft, bool groundTop, bool groundRight, bool groundBottom)
        {
            if (groundLeft && groundTop && !groundRight && !groundBottom)
                return 0;
            if (!groundLeft && groundTop && !groundRight && !groundBottom)
                return 1;
            if (!groundLeft && groundTop && groundRight && !groundBottom)
                return 2;
            if (groundLeft && !groundTop && !groundRight && !groundBottom)
                return 3;
            if (!groundLeft && !groundTop && !groundRight && !groundBottom)
                return 4;
            if (!groundLeft && !groundTop && groundRight && !groundBottom)
                return 5;
            if (groundLeft && !groundTop && !groundRight && groundBottom)
                return 6;
            if (!groundLeft && !groundTop && !groundRight && groundBottom)
                return 7;
            if (!groundLeft && !groundTop && groundRight && groundBottom)
                return 8;

            if (groundLeft && groundTop && groundRight && !groundBottom)
                return 9;
            if (groundLeft && groundTop && !groundRight && groundBottom)
                return 10;
            if (groundLeft && groundTop && groundRight && groundBottom)
                return 11;
            if (!groundLeft && groundTop && groundRight && groundBottom)
                return 12;
            if (groundLeft && !groundTop && groundRight && groundBottom)
                return 13;
            return 4;
            /*
             *111
             *111
             *111
             *010
             *111
             *010
             *
             */
        }

    }
}
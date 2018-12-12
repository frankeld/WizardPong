using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WizardPong
{
    public class Text //Simplifies drawing
    {
        string font;
        SpriteFont sFont;

        public Text(string f)
        {
            font = "Fonts\\" + f;
        }

        public void LoadContent(ContentManager c)
        {
            sFont = c.Load<SpriteFont>(font);
        }

        public void Draw(SpriteBatch s, string text, Vector2 pos, Color color)
        {
            Vector2 textSize = sFont.MeasureString(text);

            s.DrawString(sFont, text, new Vector2(pos.X - textSize.X / 2, pos.Y - textSize.Y / 2), color); //Bases text size for previous assuming current is the same

        }

        public void Draw(SpriteBatch s, string text, Vector2 pos, Color color, int lines)
        {
            Vector2 textSize = sFont.MeasureString(text);

            s.DrawString(sFont, text, new Vector2(pos.X - textSize.X / 2, pos.Y - textSize.Y / 2 + textSize.Y * lines), color);

        }

        public void DrawRightAlgnWithSpace(SpriteBatch s, string text, Vector2 pos, Color color, int lines) //Uses for labels
        {
            Vector2 textSize = sFont.MeasureString(text);

            s.DrawString(sFont, text, new Vector2(pos.X - textSize.X - 20, pos.Y - textSize.Y / 2 + textSize.Y * lines), color);

        }

        public void DrawControls(SpriteBatch s, string text, Vector2 pos, Color color, int lines, bool noncenter) //Uses for controls
        {
            Vector2 textSize = sFont.MeasureString(text);

            s.DrawString(sFont, text, new Vector2(pos.X, pos.Y - textSize.Y / 2 + textSize.Y * lines), color);

        }



    }
}

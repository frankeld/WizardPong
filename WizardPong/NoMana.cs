using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace WizardPong
{
    public class NoMana : Spell  //Treats NoMana as spell so it updates position and time left after each frame
    {
        int player;
        int frameCount;
        Text emptyMana;
        Vector2 position;

        public NoMana(int num)
        {
            player = num;
            frameCount = 0;
            Game1.activeSpells.Add(this);
        }
        public void LoadContent(ContentManager c)
        {
            emptyMana = new Text("fontType3");
            emptyMana.LoadContent(c);

        }
        public override void Update(Player playerOne, Player playerTwo)
        {
            if (frameCount == 30 * 4)
            {
                return;
            }
            frameCount++;
            if (player == 1) //Updates the position of the text to follow the player
            {
                position.X = playerOne.BoundingBox().Center.ToVector2().X;
                position.Y = playerOne.BoundingBox().Top;
            }
            else if (player == 2)
            {
                position.X = playerTwo.BoundingBox().Center.ToVector2().X;
                position.Y = playerTwo.BoundingBox().Top;
            }
            position.Y -= frameCount; //Moves the text up over time
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (frameCount == 30 * 4) //Does not draw if at end
            {
                return;
            }
            emptyMana.Draw(spriteBatch, "Not enough mana!", position, Color.Red);
        }
    }
}

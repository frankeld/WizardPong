using Microsoft.Xna.Framework.Graphics;

namespace WizardPong
{
    abstract public class Spell
    {
        public Spell()
        {

        }

        abstract public void Draw(SpriteBatch s);
        abstract public void Update(Player playerOne, Player playerTwo);

    }
}

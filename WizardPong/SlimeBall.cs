using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WizardPong
{
    class SlimeBall : Spell
    {
        public enum SpellState
        {
            Seeking,
            Active,
            Gone
        }

        Texture2D image;
        int caster;
        float frameCount;
        Vector2 position;
        SpellState spellState;
        static int cost = 15;
        SoundEffect squish;
        SoundEffect castSound;
        Ball target;


        public SlimeBall(int cast, Vector2 pos, Ball b)
        {
            caster = cast;
            frameCount = 0;
            Game1.activeSpells.Add(this);
            position = new Vector2(pos.X, pos.Y);
            spellState = SpellState.Seeking;
            target = b;
        }


        public void LoadContent(ContentManager c)
        {
            squish = c.Load<SoundEffect>("Sounds\\shimmerSound");
            image = c.Load<Texture2D>("Spells\\slimeBall");
            castSound = c.Load<SoundEffect>("Sounds\\Cast\\castSlimeBall");
            castSound.Play(Game1.soundEffectsOn ? Game1.volume : 0f, 0.0f, 0.0f);//Plays when first created
        }

        public override void Update(Player playerOne, Player playerTwo)
        {
            if (frameCount == 30 * 13)
            {
                if (spellState == SpellState.Seeking)
                {
                    return;
                }
                else if (spellState == SpellState.Active)
                {
                    spellState = SpellState.Gone;
                    target.Slime(false, caster);

                    return;
                }
                else if (spellState == SpellState.Gone)
                {
                    return;
                }

            }
            if (spellState == SpellState.Active)
            {
                frameCount++;
                target.Slime(true, caster); //needs to run constantly to check which side of the court has the ball
                return;
            }
            frameCount++;

            Vector2 targetPos = new Vector2(0, 0);

            targetPos = new Vector2(target.BoundingBox().X, target.BoundingBox().Y);
            if (target.BoundingBox().Intersects(BoundingBox()))
            {
                frameCount = 0;
                spellState = SpellState.Active;
                target.Slime(true, caster);
                squish.Play(Game1.soundEffectsOn ? Game1.volume : 0f, 0.0f, 0.0f);

            }

            Vector2 getTo = Vector2.Subtract(targetPos, position);

            getTo.Normalize();
            getTo = Vector2.Multiply(getTo, 15);
            position = Vector2.Add(position, getTo);
        }

        public override void Draw(SpriteBatch s)
        {
            if (spellState == SpellState.Seeking)
            {

                s.Draw(image, BoundingBox(), null, Color.White);

            }

        }

        Rectangle BoundingBox()
        {
            return new Rectangle((int)position.X, (int)position.Y, image.Width, image.Height);

        }


        public static int Cost
        {
            get
            {
                return cost;
            }
        }

    }
}
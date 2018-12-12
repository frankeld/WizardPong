using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WizardPong
{
    class PortalTrap : Wall
    {
        Rectangle boundingBox;
        Texture2D image;
        float frameCount;
        int caster;
        static int cost = 50; //Objects havent been made yet, so do this here
        Player casterPlay;
        SoundEffect castSound;
        bool fail;

        public PortalTrap(int cast, Player casterP, Ball ball)
        {
            caster = cast;
            casterPlay = casterP;
            frameCount = 0;
            Rectangle casterBox = Game1.walls[caster].BoundingBox();
            Rectangle box = new Rectangle(casterBox.X, casterBox.Y, casterBox.Width, casterBox.Height);
            if (caster == 1)
            {
                box.X += 25;
            }
            else if (caster == 2)
            {
                box.X -= 25;
            }


            if (box.Intersects(ball.BoundingBox())) //Prevents force field from being created on top of the ball
            {
                box = new Rectangle();
                fail = true;
            }
            else
            {
                fail = false;

                if (caster == 1)
                {
                    box.X += -5;
                }
                else if (caster == 2)
                {
                    box.X -= -5;
                }
            }
            boundingBox = box;

        }


        public void LoadContent(ContentManager c)
        {
            image = c.Load<Texture2D>("Spells\\portalTrap");
            castSound = c.Load<SoundEffect>("Sounds\\Cast\\castPortalTrap");
            castSound.Play(Game1.soundEffectsOn ? Game1.volume : 0f, 0.0f, 0.0f);//Plays when first created
            Game1.walls.Add(this);
        }

        public override void Draw(SpriteBatch s)
        {
            s.Draw(image, boundingBox, null, Color.White);

        }

        public override void Update()
        {
            if (frameCount == 30 * 10) //Removes portal trap after alloted time
            {
                boundingBox = new Rectangle();
                frameCount++;
                return;
            }
            if (frameCount <= 2 * 30 * 10) //But player freeze lasts even longer
            {
                casterPlay.Freeze(true);
            }
            else
            {
                casterPlay.Freeze(false);
                return;
            }
            frameCount++;
        }

        public override Rectangle BoundingBox()
        {
            return boundingBox;

        }
        public static int Cost
        {
            get
            {
                return cost;
            }
        }

        internal bool IsFail()
        {
            return fail;
        }

    }

}

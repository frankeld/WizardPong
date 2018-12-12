using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WizardPong
{
    public class ForceField : Wall
    {

        Rectangle boundingBox;
        Texture2D image;
        float frameCount;
        int playerNum;
        int playerWidth;
        static int cost = 40;
        bool fail;
        SoundEffect castSound;

        public ForceField(int player, int width)
        {
            playerNum = player;
            playerWidth = width;
            frameCount = 0;
        }


        public void LoadContent(ContentManager c, Vector2 corner, Ball ball)
        {

            if (playerNum == 1)
            {
                image = c.Load<Texture2D>("Spells\\forceField");
                boundingBox = new Rectangle((int)corner.X + playerWidth, (int)corner.Y, image.Width, image.Height);
            }
            else
            {
                image = c.Load<Texture2D>("Spells\\forceFieldFlip");
                boundingBox = new Rectangle((int)corner.X - image.Width, (int)corner.Y, image.Width, image.Height);
            }

            if (boundingBox.Intersects(ball.BoundingBox())) //Prevents force field from being created on top of the ball
            {
                boundingBox = new Rectangle();
                fail = true;
                return; //New code to just end the loading if the force field is on top of the ball
            }
            else
            {
                fail = false;
            }

            castSound = c.Load<SoundEffect>("Sounds\\Cast\\castForceField");
            castSound.Play(Game1.soundEffectsOn ? Game1.volume : 0f, 0.0f, 0.0f);//Plays when first created

            Game1.walls.Add(this);
        }

        public override void Draw(SpriteBatch s)
        {

            s.Draw(image, boundingBox, null, Color.White);

        }

        public override void Update()
        {
            if (frameCount == 30 * 10)
            {
                boundingBox = new Rectangle();
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

using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WizardPong
{
    class SlowTime : Spell
    {
        int caster;
        float frameCount;
        bool active;
        static int cost = 50;
        SoundEffect castSound;

        public SlowTime(int cast)
        {
            caster = cast;
            frameCount = 0;
            Game1.activeSpells.Add(this);
            active = true;
        }
        internal void LoadContent(ContentManager content)
        {
            castSound = content.Load<SoundEffect>("Sounds\\Cast\\castSlowTime");
            castSound.Play(Game1.soundEffectsOn ? Game1.volume : 0f, 0.0f, 0.0f);//Plays when first created
        }
        public override void Update(Player playerOne, Player playerTwo)
        {
            if (frameCount == 30 * 10) //Same method setup as in PortalTrap
            {

                if (active)
                {
                    if (caster == 1)
                    {
                        playerTwo.Slow(false);
                    }
                    else if (caster == 2)
                    {
                        playerOne.Slow(false);
                    }
                    active = false;
                }
                return;
            }

            frameCount++;

            if (caster == 1)
            {
                playerTwo.Slow(true);
            }
            else if (caster == 2)
            {
                playerOne.Slow(true);
            }
        }
        public override void Draw(SpriteBatch s)
        {


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

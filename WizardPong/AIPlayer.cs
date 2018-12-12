using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace WizardPong
{
    class AIPlayer : Player
    {
        bool starter = true;
        Random randomizer;
        Player enemy;
        int ballStuckCounter = 0; //Resets AI pos if the counter reaches 30

        public AIPlayer(int num, Player playerOne) : base(num)
        {
            randomizer = new Random();
            enemy = playerOne;
        }



        public override void Move(Ball ball)
        {
            if (ball.stuckBall)
            {
                ballStuckCounter++;
            } else
            {
                ballStuckCounter = 0;
            }
            if (ballStuckCounter > 30 && Game1.walls[0].BoundingBox().X < ball.BoundingBox().X) //If the ball is stuck, resets position but only if on the right side (AI side) of the screen
            {
                position = new Vector2(3200 - 400, 2000 / 2);
            }

            bool changeState = false;
            if (starter)
            {
                starter = false;
                position.X -= 250;
            }
            //want to go to the right of the ball, not left
            Vector2 getToCenter = ball.BoundingBox().Center.ToVector2();
            getToCenter = Vector2.Subtract(getToCenter, BoundingBox().Center.ToVector2());


            if (position.X < ball.BoundingBox().X)
            {
                getToCenter.Y = 0;
                getToCenter.X = 10;
            }


            getToCenter.Normalize();
            getToCenter = Vector2.Multiply(getToCenter, velocity);

            //if ball is going towards goal
            if (ball.TowardsAIGoal())
            {
                if (CheckCollision(position.X + getToCenter.X, position.Y + getToCenter.Y, ball))
                {

                }
                else
                {
                    position += getToCenter;
                    changeState = true;
                }
            }
            if (changeState)
            {
                animState = AnimationState.Walk;
            }
            else
            {
                animState = AnimationState.Idle;
            }


        }

        public override void Cast(ContentManager content, Ball ball, GameTime gameTime)
        {
            double seconds = gameTime.TotalGameTime.TotalSeconds;
            if (-0.01 < seconds - (int)seconds && seconds - (int)seconds < 0.01)
            {
                if (mana < 100)
                {
                    mana++;
                }
            }
            if (-0.01 < seconds - ((int)seconds + 0.5) && seconds - ((int)seconds + 0.5) < 0.01)
            {
                if (mana < 100)
                {
                    mana++;
                }
            }

            //If ball is near goal, random chance to cast Portal Trap
            Rectangle casterBox = Game1.walls[2].BoundingBox();
            Rectangle box = new Rectangle(casterBox.X, casterBox.Y, casterBox.Width, casterBox.Height);
            box.X -= 60;
            box.Y -= 20;
            box.Width += 60;
            box.Height += 40;

            if (ball.BoundingBox().Intersects(box))
            {

                if (randomizer.Next(1, 6) <= 2)
                {
                    if (mana > PortalTrap.Cost)
                    {
                        PortalTrap temp = new PortalTrap(2, this, ball);
                        temp.LoadContent(content);

                        if (!temp.IsFail())
                        {
                            animState = AnimationState.Attack;
                            attackOverride = true;
                            mana -= PortalTrap.Cost;
                        }
                        else
                        {
                            //This should not happen because it should be trigged before it enters the goal or not at all.
                            //Or should it happen as the ball continually intersects it?
                        }

                    }
                }

            }
            //Randomly casts Force Field
            if (randomizer.Next(1, 15) == 4 && mana >= 78)
            {
                ForceField temp = new ForceField(2, wizardSprites[10].Width); //has to be the largest attack to prevent issues IMPORTANT
                temp.LoadContent(content, new Vector2(position.X, position.Y), ball);
                if (!temp.IsFail())
                {
                    animState = AnimationState.Attack;
                    attackOverride = true;
                    mana -= ForceField.Cost;
                }
            }

            //if ball is near, cast slime ball
            if (randomizer.Next(1, 350) == 2 && mana >= 50 && Vector2.Distance(ball.BoundingBox().Center.ToVector2(), BoundingBox().Center.ToVector2()) < 500)
            {
                SlimeBall temp = new SlimeBall(2, new Vector2(position.X + wizardSprites[currentTexture].Width / 2, position.Y + wizardSprites[currentTexture].Height / 2), ball);
                temp.LoadContent(content);
                mana -= SlimeBall.Cost;
                animState = AnimationState.Attack;
                attackOverride = true;
            }
            //if other play is near center line, cast slowtime
            if (randomizer.Next(1, 4) == 2 && mana >= 60 && Math.Abs(Game1.walls[0].BoundingBox().X - enemy.BoundingBox().X) < 500 && !ball.TowardsAIGoal())
            {
                SlowTime temp = new SlowTime(2);
                temp.LoadContent(content);
                mana -= SlowTime.Cost;
                animState = AnimationState.Attack;
                attackOverride = true;
            }


        }

    }
}

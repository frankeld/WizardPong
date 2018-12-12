using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace WizardPong
{
    public class Ball
    {
        Vector2 position;
        Vector2 velocity;
        Texture2D image;
        float maxSpeed = 18f;
        SoundEffect playerThud;
        SoundEffect wallThud;
        Color slime;
        internal bool stuckBall = false; //Needs to be accessed by Game1 to draw STOP
        List<Vector2> pastVel;

        public Ball()
        {
            Random rand = new Random();
            int rander = rand.Next(5);

            velocity = new Vector2(8f, 8f);

            if (rander == 1) //Randomizes starting direction
            {
                velocity.X *= -1;
            }
            else if (rander == 2)
            {
                velocity.X *= -1;
                velocity.Y *= -1;
            }
            else if (rander == 3)
            {
                velocity.Y *= -1;
            }
            else if (rander == 4)
            {
                //nothing
            }

            slime = Color.White;

            pastVel = new List<Vector2>(); //Stores past pos for stuckBall checker

        }
        
        public void LoadContent(ContentManager c, Vector2 start)
        {
            image = c.Load<Texture2D>("Main Graphics\\Ball");
            playerThud = c.Load<SoundEffect>("Sounds\\playerThud");
            wallThud = c.Load<SoundEffect>("Sounds\\wallThud");
            position = new Vector2(start.X - (image.Width / 2), start.Y - (image.Height / 2));
        }

        internal bool TowardsAIGoal()
        {
            if (velocity.X > 0)
            {
                return true;
            }
            return false;
        }


        public void Slime(bool effect, int caster)
        {
            if (effect)
            {
                if (position.X < Game1.walls[0].BoundingBox().Center.X && caster == 2)
                {
                    slime = Color.Purple * 0f; //Makes invisible
                }
                else if (caster == 2)
                {
                    slime = Color.White * 0.5f; //Makes the effect visible even on the caster side, but to a lesser degree
                }
                if (position.X > Game1.walls[0].BoundingBox().Center.X && caster == 1)
                {
                    slime = Color.Purple * 0f;
                }
                else if (caster == 1)
                {
                    slime = Color.White * 0.5f;
                }

            }
            else if (!effect)
            {
                slime = Color.White;
            }
        }

        public void Draw(SpriteBatch s)
        {
            s.Draw(image, position, slime);
        }

        public Rectangle BoundingBox()
        {
            return new Rectangle((int)position.X, (int)position.Y, image.Width, image.Height);
        }

        public void Move(Player playerOne, Player playerTwo)
        {
            if (!BallCollision(playerOne, playerTwo))//Moves if no ball collision
            {
                position += velocity;
            }

        }

        public void Update(Player playerOne, Player playerTwo)
        {
            Move(playerOne, playerTwo);
            stuckBall = CheckForBallStuck(); //TODO: Draw issue message if ball is stuck
        }

        private bool CheckForBallStuck()
        {
            int BallChangesVelocity = 0; //Keeps track of how many changes in the last 30 frames //TODO: make 30 frames a property
            pastVel.Insert(0, velocity);
            if (pastVel.Count > 30) //Deletes if over alloted past history rememberence
            {
                pastVel.RemoveAt(pastVel.Count - 1);
            }

            for (int i = 0; i < pastVel.Count-1; i++) //must be -1 to prevent issues with pastVel(i+1)
            {
                if (pastVel[i]!= pastVel[i + 1])
                {
                    BallChangesVelocity++;
                }
            }

            if (BallChangesVelocity > 15)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void reflect(Wall wallHit, Player playerHit)
        {
            Rectangle interSect;

            if (wallHit == null)
            {
                interSect = Rectangle.Intersect(BoundingBox(), playerHit.BoundingBox());
                if (Math.Abs(velocity.X) < maxSpeed)
                {
                    velocity *= 1.3f; //increases speed for each time it hits a player
                }

            }
            else if (playerHit == null)
            {
                interSect = Rectangle.Intersect(BoundingBox(), wallHit.BoundingBox());
            }
            else
            {
                interSect = new Rectangle(0, 0, 0, 0);
            }

            Vector2 subtraction = Vector2.Subtract(BoundingBox().Center.ToVector2(), interSect.Center.ToVector2());

            double headingAngle = Math.Atan2(subtraction.Y, subtraction.X);
            //Debug.WriteLine(headingAngle);

            position -= velocity; //Prevents moving in the new direction to be part of the old intersection
            if ((headingAngle >= 0 && headingAngle <= Math.PI / 4) || (headingAngle <= 0 && headingAngle >= Math.PI / -4))
            {
                //1st piece
                velocity.X *= -1;
                position += velocity;
            }
            else if (headingAngle >= Math.PI / 4 && headingAngle < 3 * Math.PI / 4)
            {
                //2nd piece
                velocity.Y *= -1;
                position += velocity;
            }
            else if ((headingAngle >= 3 * Math.PI / 4 && headingAngle <= Math.PI) || (headingAngle >= -1 * Math.PI && headingAngle <= -3 * Math.PI / 4))
            {
                //3rd piece
                velocity.X *= -1;
                position += velocity;
            }
            else if (headingAngle > -3 * Math.PI / 4 && headingAngle < Math.PI / -4)
            {
                //4th piece
                velocity.Y *= -1;
                position += velocity;
            }
        }

        private bool BallCollision(Player playerOne, Player playerTwo)
        {
            if (playerTwo is AIPlayer)
            {
                Game1.CurrentGameLength += 0.05f; //Increases score for every frame that the ball is moving and the second player is AI,
                //TODO: To be honest, this is a weird way
            }

            for (int i = 3; i < Game1.walls.Count; i++)
            {
                if (Game1.walls[i].BoundingBox().Intersects(BoundingBox()))
                {
                    reflect(Game1.walls[i], null);
                    wallThud.Play(Game1.soundEffectsOn ? Game1.volume : 0f, 0.0f, 0.0f);
                    return true;

                }

            }

            if (BoundingBox().Intersects(playerOne.BoundingBox()))
            {
                reflect(null, playerOne);
                playerThud.Play(Game1.soundEffectsOn ? Game1.volume : 0f, 0.0f, 0.0f);
                return true;
            }

            if (BoundingBox().Intersects(playerTwo.BoundingBox()))
            {
                reflect(null, playerTwo);
                playerThud.Play(Game1.soundEffectsOn ? Game1.volume : 0f, 0.0f, 0.0f);
                return true;
            }

            return false;

        }

    }
}

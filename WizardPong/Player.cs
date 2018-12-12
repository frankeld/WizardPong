using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace WizardPong
{
    public class Player
    {
        protected enum AnimationState
        {
            Idle,
            Walk,
            Attack
        }
        protected AnimationState animState;
        protected Vector2 position;
        protected bool isAI;
        protected List<Keys> control;
        protected List<string> XBoxControl;
        protected List<Texture2D> wizardSprites;
        protected int currentTexture;
        protected int score;
        int playerNum;
        double castCounter;
        protected float velocity;
        protected int mana; //TODO: Make mana bar
        Color color;
        protected bool attackOverride = false;

        public Player(int num)
        {
            playerNum = num;
            castCounter = 0.1;
            velocity = 10f;
            color = Color.White;
            currentTexture = 0;
            wizardSprites = new List<Texture2D>();
        }

        public void LoadContent(ContentManager c, Vector2 start)
        {
            score = 0;
            mana = 50;

            wizardSprites.Add(c.Load<Texture2D>("Main Graphics\\Wiz" + playerNum.ToString() + "\\idle_1")); //no longer using idle animation (disregard)
            wizardSprites.Add(c.Load<Texture2D>("Main Graphics\\Wiz" + playerNum.ToString() + "\\idle_2")); //TODO: Remove or fix idle
            wizardSprites.Add(c.Load<Texture2D>("Main Graphics\\Wiz" + playerNum.ToString() + "\\idle_3"));
            wizardSprites.Add(c.Load<Texture2D>("Main Graphics\\Wiz" + playerNum.ToString() + "\\idle_4"));
            wizardSprites.Add(c.Load<Texture2D>("Main Graphics\\Wiz" + playerNum.ToString() + "\\walk_1"));
            wizardSprites.Add(c.Load<Texture2D>("Main Graphics\\Wiz" + playerNum.ToString() + "\\walk_2"));
            wizardSprites.Add(c.Load<Texture2D>("Main Graphics\\Wiz" + playerNum.ToString() + "\\walk_3"));
            wizardSprites.Add(c.Load<Texture2D>("Main Graphics\\Wiz" + playerNum.ToString() + "\\walk_4"));
            wizardSprites.Add(c.Load<Texture2D>("Main Graphics\\Wiz" + playerNum.ToString() + "\\attack_1"));
            wizardSprites.Add(c.Load<Texture2D>("Main Graphics\\Wiz" + playerNum.ToString() + "\\attack_2"));
            wizardSprites.Add(c.Load<Texture2D>("Main Graphics\\Wiz" + playerNum.ToString() + "\\attack_3"));

            position = new Vector2(start.X - (wizardSprites[currentTexture].Width / 2), start.Y - (wizardSprites[currentTexture].Height / 2));
            control = new List<Keys>();
            if (playerNum == 1)
            {
                control.Add(Keys.W);
                control.Add(Keys.S);
                control.Add(Keys.A);
                control.Add(Keys.D);
                control.Add(Keys.F); //force field
                control.Add(Keys.T); //slow time
                control.Add(Keys.R); //slime ball
                control.Add(Keys.E); //portal trap
            }
            if (playerNum == 2)
            {
                control.Add(Keys.Up);
                control.Add(Keys.Down);
                control.Add(Keys.Left);
                control.Add(Keys.Right);
                control.Add(Keys.J); //force field
                control.Add(Keys.K); //slow time
                control.Add(Keys.L); //slime ball
                control.Add(Keys.I); //portal trap
            }
            XBoxControl = new List<string>();
            XBoxControl.Add("Up");
            XBoxControl.Add("Down");
            XBoxControl.Add("Left");
            XBoxControl.Add("Right");
            XBoxControl.Add("X"); //force field
            XBoxControl.Add("Y"); //slow time
            XBoxControl.Add("A"); //slime ball
            XBoxControl.Add("B"); //portal trap

        }

        public void Draw(SpriteBatch s)
        {


            s.Draw(wizardSprites[currentTexture], position, color);

        }

        public Rectangle BoundingBox()
        {

            return new Rectangle((int)position.X, (int)position.Y, wizardSprites[currentTexture].Width, wizardSprites[currentTexture].Height);

        }

        public virtual void Move(Ball ball)
        {


            bool changeState = false;

            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState padState = GamePad.GetState(PlayerIndex.One); //prevents issues with a value that doesn't exist
            if (playerNum == 1)
            {
                padState = GamePad.GetState(PlayerIndex.One);
            }
            else if (playerNum == 2)
            {
                padState = GamePad.GetState(PlayerIndex.Two);
            }

            if (keyboardState.IsKeyDown(control[0]) || padState.ThumbSticks.Left.Y > 0.5f)
            {
                if (CheckCollision(position.X, position.Y - velocity, ball))
                {

                }
                else
                {
                    position.Y -= velocity;
                    changeState = true;
                }

            }
            if (keyboardState.IsKeyDown(control[1]) || padState.ThumbSticks.Left.Y < -0.5f)
            {
                if (CheckCollision(position.X, position.Y + velocity, ball))
                {

                }
                else
                {
                    position.Y += velocity;

                    changeState = true;
                }
            }
            if (keyboardState.IsKeyDown(control[2]) || padState.ThumbSticks.Left.X < -0.5f)
            {
                if (CheckCollision(position.X - velocity, position.Y, ball))
                {

                }
                else
                {
                    position.X -= velocity;

                    changeState = true;
                }
            }
            if (keyboardState.IsKeyDown(control[3]) || padState.ThumbSticks.Left.X > 0.5f)
            {
                if (CheckCollision(position.X + velocity, position.Y, ball))
                {

                }
                else
                {
                    position.X += velocity;

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

        protected bool CheckCollision(float v, float v2, Ball package)
        {
            Rectangle modifiedBox = new Rectangle(0, 0, wizardSprites[currentTexture].Width, wizardSprites[currentTexture].Height);
            modifiedBox.X = (int)v;
            modifiedBox.Y = (int)v2;
            for (int i = 0; i < Game1.walls.Count; i++)
            {
                if (modifiedBox.Intersects(Game1.walls[i].BoundingBox()))
                {
                    return true;
                }
            }
            if (modifiedBox.Intersects(package.BoundingBox()))
            {
                return true;
            }

            return false;
        }


        public virtual void Cast(ContentManager content, Ball ball, GameTime gameTime)
        {

            castCounter += 0.1;
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState padState = GamePad.GetState(PlayerIndex.One); //prevents issues with a value that doesn't exist
            if (playerNum == 1)
            {
                padState = GamePad.GetState(PlayerIndex.One);
            }
            else if (playerNum == 2)
            {
                padState = GamePad.GetState(PlayerIndex.Two);
            }
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

            if (castCounter >= 1)
            {
                if (keyboardState.IsKeyDown(control[4]) || (padState.IsButtonDown(Buttons.A) && !Game1.lastGamePadOne.IsButtonDown(Buttons.A)))
                {
                    if (mana > ForceField.Cost)
                    {

                        ForceField temp = new ForceField(playerNum, wizardSprites[10].Width); //has to be the largest attack image to prevent issues IMPORTANT, and if player two then needs to change corner
                        temp.LoadContent(content, new Vector2(playerNum == 1 ? position.X : position.X - (wizardSprites[10].Width - wizardSprites[0].Width), position.Y), ball); //and if player two then needs to change corner because opens to the left


                        if (!temp.IsFail()) //Did not fail due to exceptions
                        {
                            animState = AnimationState.Attack;
                            attackOverride = true;
                            mana -= ForceField.Cost;
                        }
                    }
                    else
                    {
                        NoMana temp = new NoMana(playerNum);
                        temp.LoadContent(content);
                    }

                }
                else if (keyboardState.IsKeyDown(control[5]) || (padState.IsButtonDown(Buttons.X) && !Game1.lastGamePadOne.IsButtonDown(Buttons.X)))
                {
                    if (mana > SlowTime.Cost)
                    {
                        SlowTime temp = new SlowTime(playerNum);
                        temp.LoadContent(content);
                        mana -= SlowTime.Cost;
                        animState = AnimationState.Attack;
                        attackOverride = true;
                    }
                    else
                    {
                        NoMana temp = new NoMana(playerNum);
                        temp.LoadContent(content);
                    }
                }
                else if (keyboardState.IsKeyDown(control[6]) || (padState.IsButtonDown(Buttons.Y) && !Game1.lastGamePadOne.IsButtonDown(Buttons.Y)))
                {
                    if (mana > SlimeBall.Cost)
                    {
                        SlimeBall temp = new SlimeBall(playerNum, new Vector2(position.X + wizardSprites[currentTexture].Width / 2, position.Y + wizardSprites[currentTexture].Height / 2), ball);
                        temp.LoadContent(content);
                        mana -= SlimeBall.Cost;
                        animState = AnimationState.Attack;
                        attackOverride = true;
                    }
                    else
                    {
                        NoMana temp = new NoMana(playerNum);
                        temp.LoadContent(content);
                    }

                }
                else if (keyboardState.IsKeyDown(control[7]) || (padState.IsButtonDown(Buttons.B) && !Game1.lastGamePadOne.IsButtonDown(Buttons.B)))
                {
                    if (mana > PortalTrap.Cost)
                    {
                        PortalTrap temp = new PortalTrap(playerNum, this, ball);
                        temp.LoadContent(content);

                        if (!temp.IsFail())
                        {
                            mana -= PortalTrap.Cost;
                            animState = AnimationState.Attack;
                            attackOverride = true;
                        }

                    }
                    else
                    {
                        NoMana temp = new NoMana(playerNum);
                        temp.LoadContent(content);
                    }

                }
                else
                {
                    return; //needs to break to prevent cast counter reset
                }
                castCounter = 0;
            }
        }

        internal void DrawUpdate(GameTime time)
        {
            if (attackOverride)
            {
                animState = AnimationState.Attack;
            }

            double miliSeconds = time.TotalGameTime.Milliseconds;
            int switchTime;
            if (animState == AnimationState.Idle)
            {
                switchTime = 1;
            }
            else if (animState == AnimationState.Walk)
            {
                switchTime = 40;
            }
            else if (animState == AnimationState.Attack)
            {
                switchTime = 70;
            }
            else
            {
                switchTime = 1;
            }



            if ((int)(miliSeconds % switchTime) == 0)
            {

                if (animState == AnimationState.Idle)
                {
                    //no idle animation anymore
                }
                else if (animState == AnimationState.Walk)
                {
                    if (currentTexture >= 4 && currentTexture <= 7)
                    {
                        if (currentTexture == 7)
                        {
                            currentTexture = 4;
                        }
                        else
                        {
                            currentTexture++;
                        }
                    }
                    else
                    {
                        currentTexture = 4;
                    }

                    if (velocity == 0)
                    {
                        currentTexture = 0; //Prevents walking motion when frozen
                    }
                }
                else if (animState == AnimationState.Attack)
                {
                    if (currentTexture >= 8 && currentTexture <= 10)
                    {
                        if (currentTexture == 10)
                        {
                            attackOverride = false;
                            currentTexture = 0; //should reset after spell is over

                        }
                        else
                        {

                            currentTexture++;
                        }
                    }
                    else
                    {
                        currentTexture = 8;
                    }
                }
            }
            else
            {
                //prevents animation from being to fast
            }
        }

        internal void Update(Ball package, ContentManager content, GameTime gameTime)
        {
            Move(package);
            Cast(content, package, gameTime);
            DrawUpdate(gameTime);
        }

        public void Slow(bool effect)
        {

            if (effect)
            {
                color = Color.Blue;
                velocity = 1f;
            }
            else if (effect == false)
            {
                color = Color.White;
                velocity = 10f;
            }
        }

        public void Freeze(bool effect)
        {
            if (effect)
            {
                velocity = 0;
                color = Color.AliceBlue;
            }
            else if (!effect)
            {
                velocity = 10f;
                color = Color.White;
            }
        }

        public string CrlSpt(int spot)
        {

            if (Game1.XBoxGame) //Returns different controls to help menu if it is a XBox game
            {
                return XBoxControl[spot];
            }
            return control[spot].ToString();
        }

        public int Score
        {
            get
            {
                return score;
            }
            set
            {
                score = value;
            }
        }

        public int Mana
        {
            get
            {
                return mana;
            }
        }


    }
}
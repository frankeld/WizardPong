using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Media;

namespace WizardPong
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        enum GameState //Controls the state of the game
        {
            MainMenu,
            Help,
            Game,
            Pause,
            Score,
            EnterName,
            Over
        }

        GameState gameState = GameState.MainMenu; //Sets starting screen

        //Default class properties
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Create main objects for stage
        Texture2D background;
        Text scoreFont;
        Text menuFont;
        Point mainFrame;

        //Creates main active objects
        Player playerOne;
        Player playerTwo;
        Ball package;

        IsolatedStorageFile store;

        internal static List<Wall> walls; //Stores all the still walls, force fields, or portal traps
        int basicWallCount = 0; //Stores the count for static walls so NewGame doesn't have to regenerate them
        internal static List<Spell> activeSpells; //Stores all active spell, except for force field

        public static float volume = 1.0f; //Starts volume at full
        public static bool soundEffectsOn = true;

        SoundEffectInstance scoreSound;
        SoundEffectInstance winGameSound; //Sound for end of game. Must be instance to enable stopping
        SoundEffectInstance scoreLoseSound;
        SoundEffectInstance gameLoseSound;
        Song backgroundMusic;

        //Stores all keyboard and gamepad states
        KeyboardState keyboardState;
        KeyboardState lastKeyboard;
        GamePadState padStateOne;
        GamePadState padStateTwo;
        internal static GamePadState lastGamePadOne; //Needs to be accessed by Player.Cast()
        internal static GamePadState lastGamePadTwo;

        //Might use these later
        Color colorOptionOne;
        Color colorOptionTwo;

        //Makes a publically accesible view of game type
        internal static bool XBoxGame = false;

        //Scaling info
        public static Matrix SpriteScale; //TODO: Prevent streching
        int VirtualWidthScale = 3200;
        int VirtualHeightScale = 2000;

        int displayVolumeCounter; //-1 means nothing

        string version = "vF2";//For developing reasons

        bool pauseMode = false; //changes help screen base on this mode

        //High score storage
        static string ScoreFileName = "highscore";


        public static float CurrentGameLength = 0f;

        //Used for entering highscores
        int wheel = 1;
        int firstWheel = 0;
        int secondWheel = 0;
        int thirdWheel = 0;
        string winnerName = "";
        char[] alphabet;

        int recentWinner = 0;

        //Score animations
        Texture2D scoreAnim;
        int scoreAnimColCount = 9;
        double scoreAnimPos = 0;

        Texture2D overAnim;
        int overAnimColCount = 17;
        double overAnimPos = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //Initializes all the starting objects
            playerOne = new Player(1);
            playerTwo = new Player(2);
            package = new Ball();

            walls = new List<Wall>();
            activeSpells = new List<Spell>();

            scoreFont = new Text("fontType1");
            menuFont = new Text("fontType2");

            colorOptionOne = Color.Cyan;
            colorOptionTwo = Color.LightCyan;

            alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            //Default
            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Loads font and text classes
            background = Content.Load<Texture2D>("Main Graphics\\mainBackground");
            scoreFont.LoadContent(Content);
            menuFont.LoadContent(Content);
            scoreAnim = Content.Load<Texture2D>("Main Graphics\\scoreAnimGIF");
            overAnim = Content.Load<Texture2D>("Main Graphics\\overAnimGIF");
            //Building scaling as percent of virutal size
            SpriteScale = Matrix.CreateScale((float)Window.ClientBounds.Width / VirtualWidthScale, (float)Window.ClientBounds.Height / VirtualHeightScale, 1.0f);

            //Keeps mainFrame using virtual to correctly size texture positions
            mainFrame = new Point(VirtualWidthScale, VirtualHeightScale - 150); //150 moves it above taskbar


            //Loads everything at various positions
            playerOne.LoadContent(Content, new Vector2(400, mainFrame.Y / 2));
            playerTwo.LoadContent(Content, new Vector2(mainFrame.X - 400, mainFrame.Y / 2));
            package.LoadContent(Content, new Vector2(mainFrame.X / 2, mainFrame.Y / 2));

            Wall bottomWall = new Wall();
            Wall topWall = new Wall();
            Wall sideLeftTopWall = new Wall();
            Wall sideLeftBottomWall = new Wall();
            Wall sideRightTopWall = new Wall();
            Wall sideRightBottomWall = new Wall();
            Wall goalLeftWall = new Wall();
            Wall goalRightWall = new Wall();
            Wall centerLine = new Wall();

            centerLine.LoadContent(Content, new Rectangle((mainFrame.X / 2) - 5, 0, 5, mainFrame.Y), 1);
            goalLeftWall.LoadContent(Content, new Rectangle(0, mainFrame.Y / 5, 10, mainFrame.Y * 3 / 5), 2);
            goalRightWall.LoadContent(Content, new Rectangle(mainFrame.X - 10, mainFrame.Y / 5, 10, mainFrame.Y * 3 / 5), 2);

            bottomWall.LoadContent(Content, new Rectangle(0, mainFrame.Y - 100, mainFrame.X, 100), 3);
            topWall.LoadContent(Content, new Rectangle(0, 0, mainFrame.X, 100), 4);
            sideLeftTopWall.LoadContent(Content, new Rectangle(0, 0, 100, mainFrame.Y / 5), 5);
            sideLeftBottomWall.LoadContent(Content, new Rectangle(0, mainFrame.Y * 4 / 5, 100, mainFrame.Y / 5), 5);
            sideRightTopWall.LoadContent(Content, new Rectangle(mainFrame.X - 100, 0, 100, mainFrame.Y / 5), 6);
            sideRightBottomWall.LoadContent(Content, new Rectangle(mainFrame.X - 100, mainFrame.Y * 4 / 5, 100, mainFrame.Y / 5), 6);

            basicWallCount = walls.Count;//Used to prevent unnecessary wall re-creation


            //Creates instance version from SoundEffect parent (only keeps second cheer instance for later code)
            winGameSound = Content.Load<SoundEffect>("Sounds\\winGameSound").CreateInstance();
            scoreSound = Content.Load<SoundEffect>("Sounds\\scoreSound").CreateInstance();
            gameLoseSound = Content.Load<SoundEffect>("Sounds\\gameLoseSound").CreateInstance();
            scoreLoseSound = Content.Load<SoundEffect>("Sounds\\scoreLoseSound").CreateInstance();
            backgroundMusic = Content.Load<Song>("Sounds\\BackgroundMusic");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(backgroundMusic);

            //ClearHighScoreData(); //Run to clear out old highscores
            StartHighScoreData();

            //Gets store for use by high score functions
            store = IsolatedStorageFile.GetUserStoreForApplication();
        }



        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>

        protected override void Update(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White); //Clears past frame and starts screen at white

            //Collects states and checks for exit button presses
            keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            GamePadCapabilities padOne = GamePad.GetCapabilities(PlayerIndex.One);
            if (padOne.IsConnected)
            {
                XBoxGame = true;
            }

            if (XBoxGame)
            {
                padStateOne = GamePad.GetState(PlayerIndex.One);
                padStateTwo = GamePad.GetState(PlayerIndex.Two);
            }
            if (padStateOne.IsButtonDown(Buttons.Start))
            {
                Exit();
            }
            if (padStateTwo.IsButtonDown(Buttons.Start))
            {
                Exit();
            }

            if (gameState != GameState.EnterName && XBoxGame) //Prevents the volume from being adjusted when the name wheels are being adjusted
            {
                if (padStateOne.IsButtonDown(Buttons.DPadUp) || padStateTwo.IsButtonDown(Buttons.DPadUp))
                {
                    volume += 0.01f;
                    volume = Math.Min(volume, 1.0f); //Prevents issue with tiny over or under amounts
                    displayVolumeCounter = 50; //makes volume last 50 frames
                }
                else if (padStateOne.IsButtonDown(Buttons.DPadDown) || padStateTwo.IsButtonDown(Buttons.DPadDown))
                {
                    volume -= 0.01f;
                    volume = Math.Max(volume, 0.0f);
                    displayVolumeCounter = 50;
                }
                else
                {
                    if (displayVolumeCounter >= 0) //Lower volume counter if not changing volume (stops at 0)
                    {
                        displayVolumeCounter--;
                    }
                }
            }
            MediaPlayer.Volume = volume;

            switch (gameState)
            {
                case GameState.MainMenu:
                    if ((keyboardState.IsKeyDown(Keys.H) && !lastKeyboard.IsKeyDown(Keys.H)) || (padStateOne.IsButtonDown(Buttons.X) && !lastGamePadOne.IsButtonDown(Buttons.X)) || (padStateTwo.IsButtonDown(Buttons.X) && !lastGamePadTwo.IsButtonDown(Buttons.X)))
                    {
                        gameState = GameState.Help;
                    }
                    else if (keyboardState.IsKeyDown(Keys.B) || (padStateOne.IsButtonDown(Buttons.Y) && !lastGamePadOne.IsButtonDown(Buttons.Y)) || (padStateTwo.IsButtonDown(Buttons.Y) && !lastGamePadTwo.IsButtonDown(Buttons.Y)))
                    {
                        gameState = GameState.Game;
                        NewGame(0, 0);
                    }
                    else if (keyboardState.IsKeyDown(Keys.G) || (padStateOne.IsButtonDown(Buttons.A) && !lastGamePadOne.IsButtonDown(Buttons.A)) || (padStateTwo.IsButtonDown(Buttons.A) && !lastGamePadTwo.IsButtonDown(Buttons.A)))
                    {
                        gameState = GameState.Game;
                        NewGame(0, 0);
                        playerTwo = new AIPlayer(2, playerOne);
                        playerTwo.LoadContent(Content, new Vector2(mainFrame.X - 400, mainFrame.Y / 2));
                    }
                    break;
                case GameState.Help:
                    if ((keyboardState.IsKeyDown(Keys.H) && !lastKeyboard.IsKeyDown(Keys.H)) || (padStateOne.IsButtonDown(Buttons.X) && !lastGamePadOne.IsButtonDown(Buttons.X)) || (padStateTwo.IsButtonDown(Buttons.X) && !lastGamePadTwo.IsButtonDown(Buttons.X)))
                    {
                        if (!pauseMode)
                        {
                            gameState = GameState.MainMenu;
                        }
                        else
                        {
                            pauseMode = false;
                            gameState = GameState.Pause;
                        }

                    }

                    if ((keyboardState.IsKeyDown(Keys.M) && !lastKeyboard.IsKeyDown(Keys.M)) || (padStateOne.IsButtonDown(Buttons.DPadLeft) && !lastGamePadOne.IsButtonDown(Buttons.DPadLeft)) || (padStateTwo.IsButtonDown(Buttons.DPadLeft) && !lastGamePadTwo.IsButtonDown(Buttons.DPadLeft)))
                    {
                        if (MediaPlayer.State == MediaState.Stopped) //If the DPadLeft is hit, will pause or play the music
                        {
                            MediaPlayer.Play(backgroundMusic);
                        }
                        else
                        {
                            MediaPlayer.Stop();
                        }
                    }
                    if ((keyboardState.IsKeyDown(Keys.S) && !lastKeyboard.IsKeyDown(Keys.S)) || (padStateOne.IsButtonDown(Buttons.DPadRight) && !lastGamePadOne.IsButtonDown(Buttons.DPadRight)) || (padStateTwo.IsButtonDown(Buttons.DPadRight) && !lastGamePadTwo.IsButtonDown(Buttons.DPadRight)))
                    {
                        soundEffectsOn = !soundEffectsOn; //Changes the value of sound effects on if DPad Right
                    }
                    break;
                case GameState.Game:
                    if ((keyboardState.IsKeyDown(Keys.P) && !lastKeyboard.IsKeyDown(Keys.P)) || (padStateOne.IsButtonDown(Buttons.LeftStick) && !lastGamePadOne.IsButtonDown(Buttons.LeftStick)) || (padStateTwo.IsButtonDown(Buttons.LeftStick) && !lastGamePadTwo.IsButtonDown(Buttons.LeftStick)))
                    {
                        gameState = GameState.Pause;
                    }

                    package.Update(playerOne, playerTwo);

                    playerOne.Update(package, Content, gameTime);
                    playerTwo.Update(package, Content, gameTime);

                    for (int i = 0; i < activeSpells.Count; i++)
                    {
                        activeSpells[i].Update(playerOne, playerTwo);
                    }

                    for (int i = 1; i < walls.Count; i++) //Same thing as in NewGame thing
                    {
                        walls[i].Update();
                    }

                    if (package.BoundingBox().Intersects(walls[2].BoundingBox())) //Do not flip these 2 and 1s!!!
                    {
                        playerOne.Score++;
                        recentWinner = 1;
                        if (playerTwo is AIPlayer && playerOne.Score == 3 && ScoreToBeat() > CurrentGameLength)
                        {
                            winGameSound.Volume = soundEffectsOn ? volume : 0f; winGameSound.Play();
                            gameState = GameState.EnterName;
                        }
                        else if (playerOne.Score == 3)
                        {
                            winGameSound.Volume = soundEffectsOn ? volume : 0f; winGameSound.Play();
                            gameState = GameState.Over;
                        }
                        else
                        {
                            gameState = GameState.Score;
                            scoreSound.Volume = soundEffectsOn ? volume : 0f; scoreSound.Play();
                        }
                    }
                    else if (package.BoundingBox().Intersects(walls[1].BoundingBox()))
                    {
                        playerTwo.Score++;
                        recentWinner = 2;
                        if (playerTwo.Score == 3 && playerTwo is AIPlayer)
                        {
                            //Plays loser sound if single player
                            gameLoseSound.Volume = soundEffectsOn ? volume : 0f; gameLoseSound.Play();
                            gameState = GameState.Over;
                        }
                        else if (playerTwo.Score == 3)
                        {
                            winGameSound.Volume = soundEffectsOn ? volume : 0f; winGameSound.Play();
                            gameState = GameState.Over;
                        }
                        else
                        {
                            gameState = GameState.Score;
                            if (playerTwo is AIPlayer)
                            {
                                //Plays loser sound if single player
                                scoreLoseSound.Volume = soundEffectsOn ? volume : 0f; scoreLoseSound.Play();
                            }
                            else
                            {
                                scoreSound.Volume = soundEffectsOn ? volume : 0f; scoreSound.Play();
                            }

                        }
                    }

                    break;
                case GameState.Pause:
                    if ((keyboardState.IsKeyDown(Keys.P) && !lastKeyboard.IsKeyDown(Keys.P)) || (padStateOne.IsButtonDown(Buttons.LeftStick) && !lastGamePadOne.IsButtonDown(Buttons.LeftStick)) || (padStateTwo.IsButtonDown(Buttons.LeftStick) && !lastGamePadTwo.IsButtonDown(Buttons.LeftStick)))
                    {
                        gameState = GameState.Game;
                    }
                    if ((keyboardState.IsKeyDown(Keys.H) && !lastKeyboard.IsKeyDown(Keys.H)) || (padStateOne.IsButtonDown(Buttons.X) && !lastGamePadOne.IsButtonDown(Buttons.X)) || (padStateTwo.IsButtonDown(Buttons.X) && !lastGamePadTwo.IsButtonDown(Buttons.X)))
                    {
                        gameState = GameState.Help;
                        pauseMode = true;
                    }
                    if ((keyboardState.IsKeyDown(Keys.M) && !lastKeyboard.IsKeyDown(Keys.M)) || (padStateOne.IsButtonDown(Buttons.B) && !lastGamePadOne.IsButtonDown(Buttons.B)) || (padStateTwo.IsButtonDown(Buttons.B) && !lastGamePadTwo.IsButtonDown(Buttons.B)))
                    {
                        gameState = GameState.MainMenu;
                    }
                    break;
                case GameState.Score:
                    if (playerTwo is AIPlayer && recentWinner == 2)
                    {
                        scoreAnimPos = scoreAnimColCount + 1; //No animation should play if loses to AI
                    }
                    //scoreAnimPos = 0 means that no animation should play, while one is the begining (not zero based)
                    if (scoreAnimPos < scoreAnimColCount)
                    {
                        scoreAnimPos += 0.1; //Slows animation
                    }
                    else
                    {
                        scoreAnimPos = scoreAnimColCount + 1;
                    }

                    if (keyboardState.IsKeyDown(Keys.G) && !lastKeyboard.IsKeyDown(Keys.G) || (padStateOne.IsButtonDown(Buttons.A) && !lastGamePadOne.IsButtonDown(Buttons.A)) || (padStateTwo.IsButtonDown(Buttons.A) && !lastGamePadTwo.IsButtonDown(Buttons.A)))
                    {
                        scoreSound.Stop(); //Stops the scoreSound if the player leaves the score page and starts a new round
                        if (playerTwo is AIPlayer)
                        {
                            int scoreSave = playerTwo.Score;
                            NewGame(playerOne.Score, scoreSave);
                            playerTwo = new AIPlayer(2, playerOne);
                            playerTwo.LoadContent(Content, new Vector2(mainFrame.X - 400, mainFrame.Y / 2));
                            playerTwo.Score = scoreSave; //Must re-create score after erased through AI initialization
                        }
                        else
                        {
                            NewGame(playerOne.Score, playerTwo.Score);
                        }

                        gameState = GameState.Game;
                    }
                    break;
                case GameState.EnterName:
                    //(playerOne.Score > playerTwo.Score) ? padStateOne : padStateTwo
                    if (XBoxGame)
                    {
                        if (wheel == 1)
                        {
                            if ((padStateOne.IsButtonDown(Buttons.DPadDown) && !lastGamePadOne.IsButtonDown(Buttons.DPadDown)) || (padStateTwo.IsButtonDown(Buttons.DPadDown) && !lastGamePadTwo.IsButtonDown(Buttons.DPadDown)))
                            {
                                if (firstWheel == alphabet.Length - 1)
                                {
                                    firstWheel = 0;
                                }
                                else
                                {
                                    firstWheel++;
                                }

                            }
                            if ((padStateOne.IsButtonDown(Buttons.DPadUp) && !lastGamePadOne.IsButtonDown(Buttons.DPadUp)) || (padStateTwo.IsButtonDown(Buttons.DPadUp) && !lastGamePadTwo.IsButtonDown(Buttons.DPadUp)))
                            {
                                if (firstWheel == 0)
                                {
                                    firstWheel = alphabet.Length - 1;
                                }
                                else
                                {
                                    firstWheel--;
                                }

                            }
                            if ((padStateOne.IsButtonDown(Buttons.A) && !lastGamePadOne.IsButtonDown(Buttons.A)) || (padStateTwo.IsButtonDown(Buttons.A) && !lastGamePadTwo.IsButtonDown(Buttons.A)))
                            {
                                wheel = 2;
                            }
                        }
                        else if (wheel == 2)
                        {
                            if ((padStateOne.IsButtonDown(Buttons.DPadDown) && !lastGamePadOne.IsButtonDown(Buttons.DPadDown)) || (padStateTwo.IsButtonDown(Buttons.DPadDown) && !lastGamePadTwo.IsButtonDown(Buttons.DPadDown)))
                            {
                                if (secondWheel == alphabet.Length - 1)
                                {
                                    secondWheel = 0;
                                }
                                else
                                {
                                    secondWheel++;
                                }

                            }
                            if ((padStateOne.IsButtonDown(Buttons.DPadUp) && !lastGamePadOne.IsButtonDown(Buttons.DPadUp)) || (padStateTwo.IsButtonDown(Buttons.DPadUp) && !lastGamePadTwo.IsButtonDown(Buttons.DPadUp)))
                            {
                                if (secondWheel == 0)
                                {
                                    secondWheel = alphabet.Length - 1;
                                }
                                else
                                {
                                    secondWheel--;
                                }

                            }
                            if ((padStateOne.IsButtonDown(Buttons.A) && !lastGamePadOne.IsButtonDown(Buttons.A)) || (padStateTwo.IsButtonDown(Buttons.A) && !lastGamePadTwo.IsButtonDown(Buttons.A)))
                            {
                                wheel = 3;
                            }
                        }
                        else if (wheel == 3)
                        {
                            if ((padStateOne.IsButtonDown(Buttons.DPadDown) && !lastGamePadOne.IsButtonDown(Buttons.DPadDown)) || (padStateTwo.IsButtonDown(Buttons.DPadDown) && !lastGamePadTwo.IsButtonDown(Buttons.DPadDown)))
                            {
                                if (thirdWheel == alphabet.Length - 1)
                                {
                                    thirdWheel = 0;
                                }
                                else
                                {
                                    thirdWheel++;
                                }

                            }
                            if ((padStateOne.IsButtonDown(Buttons.DPadUp) && !lastGamePadOne.IsButtonDown(Buttons.DPadUp)) || (padStateTwo.IsButtonDown(Buttons.DPadUp) && !lastGamePadTwo.IsButtonDown(Buttons.DPadUp)))
                            {
                                if (thirdWheel == 0)
                                {
                                    thirdWheel = alphabet.Length - 1;
                                }
                                else
                                {
                                    thirdWheel--;
                                }

                            }
                            if ((padStateOne.IsButtonDown(Buttons.A) && !lastGamePadOne.IsButtonDown(Buttons.A)) || (padStateTwo.IsButtonDown(Buttons.A) && !lastGamePadTwo.IsButtonDown(Buttons.A)))
                            {
                                winnerName = alphabet[firstWheel].ToString() + alphabet[secondWheel].ToString() + alphabet[thirdWheel].ToString();
                                DoSaveGame();
                                gameState = GameState.Over;
                            }
                        }

                    }
                    else
                    {
                        if (keyboardState.IsKeyDown(Keys.Enter) && !lastKeyboard.IsKeyDown(Keys.Enter))
                        {
                            DoSaveGame();
                            gameState = GameState.Over;
                        }
                        Keys[] pressed = keyboardState.GetPressedKeys();
                        if (pressed.Length > 0 && !lastKeyboard.IsKeyDown(pressed[0]))
                        {
                            if (pressed[0] == Keys.Back && winnerName.Length > 0)
                            {
                                winnerName = winnerName.Insert(winnerName.Length - 1, ", ");
                                string[] newName = winnerName.Split(new Char[] { ',', ' ' });
                                winnerName = newName[0];
                            }
                            else
                            {
                                for (int i = 0; i < alphabet.Length; i++) //Only allows characters in the alphabet
                                {
                                    if (pressed[0].ToString().Equals(alphabet[i].ToString()))
                                    {
                                        winnerName += pressed[0].ToString();
                                    }
                                }

                            }

                        }
                    }

                    break;
                case GameState.Over:
                    if (playerTwo is AIPlayer & playerTwo.Score == 3) //Does not play win animation if AI wins
                    {
                        overAnimPos = overAnimColCount + 1;
                    }
                    if (overAnimPos < overAnimColCount)
                    {
                        overAnimPos += 0.1; //Slows animation
                    }
                    else
                    {
                        overAnimPos = overAnimColCount + 1;
                    }

                    if (keyboardState.IsKeyDown(Keys.M) && !lastKeyboard.IsKeyDown(Keys.M) || (padStateOne.IsButtonDown(Buttons.B) && !lastGamePadOne.IsButtonDown(Buttons.B)) || (padStateTwo.IsButtonDown(Buttons.B) && !lastGamePadTwo.IsButtonDown(Buttons.B)))
                    {
                        gameState = GameState.MainMenu;
                        winGameSound.Stop();
                        gameLoseSound.Stop();
                    }
                    else if (keyboardState.IsKeyDown(Keys.G) && !lastKeyboard.IsKeyDown(Keys.G) || (padStateOne.IsButtonDown(Buttons.A) && !lastGamePadOne.IsButtonDown(Buttons.A)) || (padStateTwo.IsButtonDown(Buttons.A) && !lastGamePadTwo.IsButtonDown(Buttons.A)))
                    {
                        winGameSound.Stop();
                        gameLoseSound.Stop();
                        if (playerTwo is AIPlayer)
                        {
                            NewGame(0, 0);
                            playerTwo = new AIPlayer(2, playerOne);
                            playerTwo.LoadContent(Content, new Vector2(mainFrame.X - 400, mainFrame.Y / 2));
                        }
                        else
                        {
                            NewGame(0, 0);
                        }
                        gameState = GameState.Game;
                    }
                    break;
            }

            //Stores the current presses as last before looping. Used to prevent key repeat loops
            lastKeyboard = keyboardState;
            lastGamePadOne = padStateOne;
            lastGamePadTwo = padStateTwo;

            base.Update(gameTime);

        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, SpriteScale);
            spriteBatch.Draw(background, new Rectangle(new Point(0, 0), mainFrame), Color.White);



            switch (gameState)
            {
                case GameState.MainMenu:

                    scoreFont.Draw(spriteBatch, version, new Vector2(mainFrame.X - 100, 50), Color.White);

                    menuFont.Draw(spriteBatch, "Welcome to Wizard Pong!", new Vector2(mainFrame.X / 2, 300), colorOptionOne);
                    scoreFont.Draw(spriteBatch, "To access the help screen, press " + (!XBoxGame ? "H" : "X") + ". To start a new two player game, press " + (!XBoxGame ? "B" : "Y") + ". To start a single player game, press " + (!XBoxGame ? "G" : "A") + ".", new Vector2(mainFrame.X / 2, 500), colorOptionTwo);
                    scoreFont.Draw(spriteBatch, "To quit the game, press " + (!XBoxGame ? "escape" : "start") + ".", new Vector2(mainFrame.X / 2, mainFrame.Y - 100), colorOptionTwo);

                    break;
                case GameState.Pause:
                    for (int i = 1; i < walls.Count; i++) //Same thing as in NewGame thing
                    {
                        walls[i].Draw(spriteBatch);
                    }
                    //package.Draw(spriteBatch); //Looks bad if also drawing

                    playerOne.Draw(spriteBatch);
                    playerTwo.Draw(spriteBatch);

                    for (int i = 0; i < activeSpells.Count; i++)
                    {
                        activeSpells[i].Draw(spriteBatch);
                    }

                    scoreFont.Draw(spriteBatch, playerOne.Score.ToString(), new Vector2(mainFrame.X / 2 - 70, 60), Color.White);
                    scoreFont.Draw(spriteBatch, playerTwo.Score.ToString(), new Vector2(mainFrame.X / 2 + 70, 60), Color.White);
                    scoreFont.Draw(spriteBatch, playerOne.Mana.ToString(), new Vector2(mainFrame.X / 2 - 200, 60), Color.White);
                    scoreFont.Draw(spriteBatch, playerTwo.Mana.ToString(), new Vector2(mainFrame.X / 2 + 200, 60), Color.White);

                    menuFont.Draw(spriteBatch, "Pause", new Vector2(mainFrame.X / 2, 300), colorOptionOne);
                    scoreFont.Draw(spriteBatch, "To continue the game, press " + (!XBoxGame ? "P" : "the left joystick") + ".", new Vector2(mainFrame.X / 2, 500), colorOptionTwo);
                    scoreFont.Draw(spriteBatch, "To access the help screen, press " + (!XBoxGame ? "H" : "X") + ".", new Vector2(mainFrame.X / 2, 1000), colorOptionTwo);
                    scoreFont.Draw(spriteBatch, "To exit this game, press " + (!XBoxGame ? "M" : "B") + ".", new Vector2(mainFrame.X / 2, mainFrame.Y - 500), colorOptionTwo);

                    break;

                case GameState.Game:

                    for (int i = 0; i < walls.Count; i++)
                    {
                        walls[i].Draw(spriteBatch);
                    }
                    playerOne.Draw(spriteBatch);
                    playerTwo.Draw(spriteBatch);

                    package.Draw(spriteBatch);
                    if (package.stuckBall)
                    {
                        menuFont.Draw(spriteBatch, "Please stop. It is just annoying.", new Vector2(mainFrame.X / 2, mainFrame.Y / 2), Color.Red, 0);
                    }

                    for (int i = 0; i < activeSpells.Count; i++)
                    {
                        activeSpells[i].Draw(spriteBatch);
                    }

                    scoreFont.Draw(spriteBatch, playerOne.Score.ToString(), new Vector2(mainFrame.X / 2 - 70, 60), Color.White);
                    scoreFont.Draw(spriteBatch, playerTwo.Score.ToString(), new Vector2(mainFrame.X / 2 + 70, 60), Color.White);
                    scoreFont.Draw(spriteBatch, playerOne.Mana.ToString(), new Vector2(mainFrame.X / 2 - 200, 60), Color.White);
                    scoreFont.Draw(spriteBatch, playerTwo.Mana.ToString(), new Vector2(mainFrame.X / 2 + 200, 60), Color.White);


                    break;
                case GameState.Help:

                    menuFont.DrawControls(spriteBatch, "Help:", new Vector2(mainFrame.X / 15, 100), colorOptionOne, 0, true);
                    scoreFont.DrawControls(spriteBatch, "P1", new Vector2(mainFrame.X / 7, 200), colorOptionTwo, 0, true);
                    scoreFont.DrawControls(spriteBatch, "P2", new Vector2(mainFrame.X / 7 + 120, 200), colorOptionTwo, 0, true);
                    List<string> CrlLabel = new List<string>();
                    List<string> spellInfo = new List<string>();
                    CrlLabel.Add("Move up:"); CrlLabel.Add("Move down:"); CrlLabel.Add("Move left:"); CrlLabel.Add("Move right:"); CrlLabel.Add("Force field:");
                    CrlLabel.Add("Slow time:"); CrlLabel.Add("Slime ball:"); CrlLabel.Add("Portal trap:");
                    spellInfo.Add(""); spellInfo.Add(""); spellInfo.Add(""); spellInfo.Add(""); spellInfo.Add("Creates a temporary wall to block the ball. Costs: " + ForceField.Cost.ToString());
                    spellInfo.Add("Slows the opposing player. Costs: " + SlowTime.Cost.ToString()); spellInfo.Add("Makes the ball invisible on the enemy side. Costs: " + SlimeBall.Cost.ToString()); spellInfo.Add("Creates a portal trap in front of the goal, but causes the player to be stuck. Costs: " + PortalTrap.Cost.ToString());
                    for (int i = 0; i < CrlLabel.Count; i++)
                    {
                        scoreFont.DrawRightAlgnWithSpace(spriteBatch, CrlLabel[i], new Vector2(mainFrame.X / 7 - 10, 200), colorOptionTwo, i + 1);
                        scoreFont.DrawControls(spriteBatch, playerOne.CrlSpt(i), new Vector2(mainFrame.X / 7, 200), colorOptionTwo, i + 1, true);
                        scoreFont.DrawControls(spriteBatch, playerTwo.CrlSpt(i), new Vector2(mainFrame.X / 7 + 120, 200), colorOptionTwo, i + 1, true);
                        scoreFont.DrawControls(spriteBatch, spellInfo[i], new Vector2(mainFrame.X / 6 + 500, 200), colorOptionTwo, i + 1, true);
                    }

                    //TODO: Put spell icons on help menu


                    scoreFont.DrawControls(spriteBatch, "Some rules of the game:", new Vector2(mainFrame.X / 15, 200), colorOptionTwo, CrlLabel.Count + 3, true);
                    scoreFont.DrawControls(spriteBatch, "Each player starts with 50 mana, which automatically regenerates over time. It has maximum value of 100.", new Vector2(mainFrame.X / 15, 200), colorOptionTwo, CrlLabel.Count + 4, true);
                    scoreFont.DrawControls(spriteBatch, "Each game is up to 5 rounds. The first player to three wins. Press " + (!XBoxGame ? "P" : "the left joystick") + " to pause during the game.", new Vector2(mainFrame.X / 15, 200), colorOptionTwo, CrlLabel.Count + 5, true);
                    scoreFont.DrawControls(spriteBatch, "To exit the help menu, press " + (!XBoxGame ? "H" : "X") + ".", new Vector2(mainFrame.X / 15, mainFrame.Y * 6 / 7), colorOptionTwo, 0, true);

                    //Only shows volume related things when on a XBox because a computer would display its volume manager
                    scoreFont.DrawControls(spriteBatch, !XBoxGame ? "" : "To change the volume, use the DPad left and right arrows. It is currently at: " + ((int)(volume * 100)).ToString() + "%", new Vector2(mainFrame.X / 15, 200), colorOptionTwo, CrlLabel.Count + 6, true);

                    scoreFont.DrawControls(spriteBatch, "To turn " + (MediaPlayer.State == MediaState.Stopped ? "on" : "off") + " background music, press " + (!XBoxGame ? "M" : "DPadLeft") + ".", new Vector2(mainFrame.X / 15, 200), colorOptionTwo, CrlLabel.Count + 7, true);
                    scoreFont.DrawControls(spriteBatch, "To turn " + (!soundEffectsOn ? "on" : "off") + " sound effects, press " + (!XBoxGame ? "S" : "DPadRight") + ".", new Vector2(mainFrame.X / 15, 200), colorOptionTwo, CrlLabel.Count + 8, true);



                    break;
                case GameState.EnterName:
                    if (XBoxGame)
                    {
                        scoreFont.Draw(spriteBatch, "Using the DPad Up and Down to enter your initials. When you are done with a alphabet wheel,", new Vector2(mainFrame.X / 2, mainFrame.Y / 2 - 400), colorOptionTwo);
                        scoreFont.Draw(spriteBatch, "use A to advance to the next wheel (or save the name).", new Vector2(mainFrame.X / 2, mainFrame.Y / 2 - 300), colorOptionTwo);
                        scoreFont.Draw(spriteBatch, alphabet[firstWheel].ToString(), new Vector2(mainFrame.X / 2 - 100, mainFrame.Y / 2), colorOptionTwo);
                        scoreFont.Draw(spriteBatch, alphabet[secondWheel].ToString(), new Vector2(mainFrame.X / 2, mainFrame.Y / 2), colorOptionTwo);
                        scoreFont.Draw(spriteBatch, alphabet[thirdWheel].ToString(), new Vector2(mainFrame.X / 2 + 100, mainFrame.Y / 2), colorOptionTwo);
                    }
                    else
                    {
                        scoreFont.Draw(spriteBatch, "Please enter your name for the high score list. Press enter when you are done.", new Vector2(mainFrame.X / 2, 300), colorOptionTwo);
                        scoreFont.Draw(spriteBatch, winnerName, new Vector2(mainFrame.X / 2, 500), colorOptionTwo);
                    }
                    break;
                case GameState.Score:
                    scoreFont.Draw(spriteBatch, playerOne.Score.ToString(), new Vector2(mainFrame.X / 2 - 70, 60), Color.White);
                    scoreFont.Draw(spriteBatch, playerTwo.Score.ToString(), new Vector2(mainFrame.X / 2 + 70, 60), Color.White);

                    if (recentWinner == 1)
                    {
                        scoreFont.Draw(spriteBatch, "Player One won the round.", new Vector2(mainFrame.X / 2, mainFrame.Y / 2), colorOptionTwo);
                        scoreFont.Draw(spriteBatch, "To continue playing, press " + (!XBoxGame ? "G" : "A") + ".", new Vector2(mainFrame.X / 2, mainFrame.Y / 2 + 100), colorOptionTwo);
                    }
                    else if (recentWinner == 2)
                    {
                        scoreFont.Draw(spriteBatch, "Player Two won the round.", new Vector2(mainFrame.X / 2, mainFrame.Y / 2), colorOptionTwo);
                        scoreFont.Draw(spriteBatch, "To continue playing, press " + (!XBoxGame ? "G" : "A") + ".", new Vector2(mainFrame.X / 2, mainFrame.Y / 2 + 100), colorOptionTwo);
                    }

                    spriteBatch.Draw(scoreAnim, new Rectangle(mainFrame.X / 4 - scoreAnim.Width / (2 * scoreAnimColCount), mainFrame.Y / 2 - scoreAnim.Height / 2, scoreAnim.Width / scoreAnimColCount, scoreAnim.Height), new Rectangle(scoreAnim.Width / scoreAnimColCount * (int)scoreAnimPos, 0, scoreAnim.Width / scoreAnimColCount, scoreAnim.Height), Color.White);
                    spriteBatch.Draw(scoreAnim, new Rectangle(mainFrame.X * 3 / 4 - scoreAnim.Width / (2 * scoreAnimColCount), mainFrame.Y / 2 - scoreAnim.Height / 2, scoreAnim.Width / scoreAnimColCount, scoreAnim.Height), new Rectangle(scoreAnim.Width / scoreAnimColCount * (int)scoreAnimPos, 0, scoreAnim.Width / scoreAnimColCount, scoreAnim.Height), Color.White);

                    break;
                case GameState.Over:
                    if (playerOne.Score > playerTwo.Score)
                    {
                        scoreFont.Draw(spriteBatch, "Player One Wins!", new Vector2(mainFrame.X / 2, mainFrame.Y / 2), colorOptionTwo);
                    }
                    else if (playerOne.Score < playerTwo.Score)
                    {
                        scoreFont.Draw(spriteBatch, "Player Two Wins!", new Vector2(mainFrame.X / 2, mainFrame.Y / 2), colorOptionTwo);
                    }

                    scoreFont.Draw(spriteBatch, "Press " + (!XBoxGame ? "G" : "A") + " to start a new game, or press " + (!XBoxGame ? "M" : "B") + " to go to the main menu.", new Vector2(mainFrame.X / 2, mainFrame.Y / 2 + 100), colorOptionTwo);

                    HighScoreData scores = DoLoadGame();

                    if (playerTwo is AIPlayer) //Only shows high scores if after a game that could get on them
                    {
                        scoreFont.Draw(spriteBatch, "HIGHSCORES:", new Vector2(mainFrame.X / 2, 200), colorOptionTwo, 0);
                        for (int i = 0; i < scores.PlayerNames.Count; i++)
                        {
                            scoreFont.Draw(spriteBatch, scores.PlayerNames[i].ToString() + "   " + scores.Scores[i].ToString(), new Vector2(mainFrame.X / 2, 200), colorOptionTwo, i + 2); //i+2 to give space to title

                        }
                    }

                    spriteBatch.Draw(overAnim, new Rectangle(200, mainFrame.Y / 2 - overAnim.Height / 2, overAnim.Width / overAnimColCount, overAnim.Height), new Rectangle(overAnim.Width / overAnimColCount * (int)overAnimPos, 0, overAnim.Width / overAnimColCount, overAnim.Height), Color.White);
                    spriteBatch.Draw(overAnim, new Rectangle((mainFrame.X - 200) - (overAnim.Width / overAnimColCount), mainFrame.Y / 2 - overAnim.Height / 2, overAnim.Width / overAnimColCount, overAnim.Height), new Rectangle(overAnim.Width / overAnimColCount * (int)overAnimPos, 0, overAnim.Width / overAnimColCount, overAnim.Height), Color.White);

                    break;
            }

            if (displayVolumeCounter > 0 && XBoxGame)
            {
                scoreFont.Draw(spriteBatch, "Vol: " + ((int)(volume * 100)).ToString() + "%", new Vector2(150, 50), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void StartHighScoreData()
        {
            //Checks to see if the high score data exits, and creates it if doesn't exist yet. This should only run once per machine.
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (!store.FileExists(ScoreFileName))
            {
                using (IsolatedStorageFileStream fs = store.OpenFile(ScoreFileName, FileMode.CreateNew))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        // Convert the object to XML data and put it in the stream.
                        XmlSerializer serializer = new XmlSerializer(typeof(HighScoreData));
                        HighScoreData data = new HighScoreData();
                        data.PlayerNames.Add("play1"); data.Scores.Add(100);
                        data.PlayerNames.Add("play2"); data.Scores.Add(101);
                        data.PlayerNames.Add("play3"); data.Scores.Add(102);
                        data.PlayerNames.Add("play4"); data.Scores.Add(103);
                        data.PlayerNames.Add("play5"); data.Scores.Add(104);
                        serializer.Serialize(fs, data);
                    }
                }
            }
        }

        private void DoSaveGame()
        {

            if (store.FileExists(ScoreFileName))
            {
                using (IsolatedStorageFileStream fs = store.OpenFile(ScoreFileName, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            // Convert the object to XML data and put it in the stream.
                            XmlSerializer serializer = new XmlSerializer(typeof(HighScoreData));
                            HighScoreData savedData = new HighScoreData();
                            try
                            {
                                savedData = (HighScoreData)serializer.Deserialize(fs);
                            }
                            catch (InvalidOperationException e)
                            {
                                Debug.WriteLine("An error occurred: '{0}'", e);
                                Debug.WriteLine("ISSUE! THE SAVE DATA WILL NOW BE CLEARED AND RECREATED");
                                fs.Dispose(); //Prevents the file from throwing an in-use exception
                                store.DeleteFile(ScoreFileName);
                                StartHighScoreData();
                                return;
                            }
                            for (int i = 0; i < savedData.PlayerNames.Count; i++)
                            {
                                if (CurrentGameLength < savedData.Scores[i])
                                {
                                    savedData.Scores.RemoveAt(savedData.Scores.Count - 1);//Removes last item in list
                                    savedData.PlayerNames.RemoveAt(savedData.PlayerNames.Count - 1);
                                    savedData.Scores.Insert(i, (int)CurrentGameLength);
                                    savedData.PlayerNames.Insert(i, winnerName);

                                    fs.SetLength(0);//Erases the old data
                                    serializer.Serialize(fs, savedData);
                                    return;
                                }
                            }

                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("The file was never created.");
            }
        }

        private int ScoreToBeat()
        {
            // Check to see whether the save exists.
            if (!store.FileExists(ScoreFileName))
            {
                throw new InvalidOperationException("The file was never created.");
            }

            using (IsolatedStorageFileStream fs = store.OpenFile(ScoreFileName, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(HighScoreData));
                    HighScoreData data = (HighScoreData)serializer.Deserialize(fs);
                    return data.Scores[data.Scores.Count - 1];
                }
            }

        }

        private HighScoreData DoLoadGame()
        {
            // Check to see whether the save exists.
            if (!store.FileExists(ScoreFileName))
            {
                throw new InvalidOperationException("The file was never created.");
            }

            using (IsolatedStorageFileStream fs = store.OpenFile(ScoreFileName, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(HighScoreData));
                    HighScoreData data = (HighScoreData)serializer.Deserialize(fs);
                    return data;
                }
            }

        }

        private void ClearHighScoreData()
        {
            store.DeleteFile(ScoreFileName);
        }

        protected void NewGame(int playerOneScore, int playerTwoScore)
        {
            if (playerOne.Score == 3 || playerTwo.Score == 3)
            {
                CurrentGameLength = 0;//Resets at end of round only before the players are recreated
            }

            playerOne = new Player(1);
            playerTwo = new Player(2);

            package = new Ball();
            while (walls.Count > basicWallCount) //removes any wall spells, as the first nine in the List are native walls
            {
                walls.RemoveAt(walls.Count - 1);
            }

            playerOne.LoadContent(Content, new Vector2(400, mainFrame.Y / 2));
            playerTwo.LoadContent(Content, new Vector2(mainFrame.X - 400, mainFrame.Y / 2));
            package.LoadContent(Content, new Vector2(mainFrame.X / 2, mainFrame.Y / 2));

            playerOne.Score = playerOneScore;
            playerTwo.Score = playerTwoScore;

            activeSpells = new List<Spell>();


            wheel = 1;
            firstWheel = 0;
            secondWheel = 0;
            thirdWheel = 0;
            winnerName = "";
            recentWinner = 0;

            scoreAnimPos = 0;
            overAnimPos = 0;
        }

    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using NewTemplate.Collisions;
using NewTemplate.SpawnableObjects;
using NewTemplate.ParticleSystem;
using System;

namespace NewTemplate
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteBatch _spriteBatch2;

        private Song titleMusic;
        private Song gameplayMusic;
        //private ExplosionParticleSystem _explosions;
        private FireworkParticleSystem _fireworks1;
        private FireworkParticleSystem _fireworks2;
        private FireworkParticleSystem _fireworks3;

        private bool menuActive;
        private bool gameActive;
        private bool gamePaused;
        private bool instructions;
        private bool hazardCollide;
        private bool _shaking = false;
        private bool _fireworksActive = false;

        private float scrollingSpeed = 150f;
        private float _shakeTime = 0;
        private float _fireworksTime = 0;
        //private float _explosionTime = 0;
        private double randomSpawnTime = 0;
        private int score = 0;

        #region Assets and Textures
        private Texture2D title;
        private SpriteFont start;
        private SpriteFont exit;
        private SpriteFont _instructions;
        private SpriteFont _score;
        private Texture2D ball;
        private Texture2D rectangle;
        private Texture2D logo;
        private Texture2D background;
        private ScrollingFloor floor;
        private List<BowlingPin> bowlingPins;
        private List<Hazard> hazards;
        private Ball dumball;
        #endregion Assets and Textures

        readonly System.Random rand = new System.Random();

        private KeyboardState keyboardState;
        private KeyboardState previousKeyboardState;

        private GamePadState gamePadState;
        private GamePadState previousGamePadSate;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.PreferredBackBufferWidth = Constants.GAME_WIDTH;
            _graphics.PreferredBackBufferHeight = Constants.GAME_HEIGHT;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            bowlingPins = new List<BowlingPin>
            {
                new BowlingPin(new Vector2(-100, 0), 0),
                new BowlingPin(new Vector2(-100, 0), 0),
                new BowlingPin(new Vector2(-100, 0), 0),
                new BowlingPin(new Vector2(-100, 0), 0),
                new BowlingPin(new Vector2(-100, 0), 0),
                new BowlingPin(new Vector2(-100, 0), 0)
            };

            hazards = new List<Hazard>
            {
                new Hazard(new Vector2(-100, 0), 0),
                new Hazard(new Vector2(-100, 0), 0),
                new Hazard(new Vector2(-100, 0), 0),
                new Hazard(new Vector2(-100, 0), 0),
                new Hazard(new Vector2(-100, 0), 0),
                new Hazard(new Vector2(-100, 0), 0)
            };

            dumball = new Ball(new Vector2(20, Constants.GAME_HEIGHT - 135));

            _score = Content.Load<SpriteFont>("File");
            _instructions = Content.Load<SpriteFont>("Prompt");

            //_explosions = new ExplosionParticleSystem(this, 20);
            //Components.Add(_explosions);

            _fireworks1 = new FireworkParticleSystem(this, 20);
            Components.Add(_fireworks1);
            _fireworks2 = new FireworkParticleSystem(this, 20);
            Components.Add(_fireworks2);
            _fireworks3 = new FireworkParticleSystem(this, 20);
            Components.Add(_fireworks3);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteBatch2 = new SpriteBatch(GraphicsDevice);
            dumball.LoadContent(Content);

            foreach (var pin in bowlingPins)
            {
                pin.LoadContent(Content);
            }

            foreach (var hazard in hazards)
            {
                hazard.LoadContent(Content);
            }

            logo = Content.Load<Texture2D>("Dumball_Logo");
            background = Content.Load<Texture2D>("BGG");

            ball = Content.Load<Texture2D>("circle");
            rectangle = Content.Load<Texture2D>("Rectangle");

            floor = new ScrollingFloor(Content.Load<Texture2D>("GroundTile"), scrollingSpeed);

            menuActive = true;

            titleMusic = Content.Load<Song>("Music/BALLGAMETITLE");
            gameplayMusic = Content.Load<Song>("Music/BALLGAMEPLAY");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(titleMusic);
        }

        public void RandomizeSpawn()
        {
            randomSpawnTime = (rand.NextDouble() * 2) + 1;
            double spawn = rand.NextDouble();

            if (spawn < Constants.HAZARD_SPAWN)
            {
                for (int i = 0; i < hazards.Count; i++)
                {
                    if (hazards[i].isOffScreen)
                    {
                        hazards[i].SpawnPosition(new Vector2(Constants.GAME_WIDTH + 20, Constants.GAME_HEIGHT - 125));
                        hazards[i].UpdateSpeed(scrollingSpeed);
                        i = hazards.Count;
                    }
                }
            }
            else
            {
                for (int i = 0; i < bowlingPins.Count; i++)
                {
                    if (bowlingPins[i].isOffScreen)
                    {
                        bowlingPins[i].SpawnPosition(new Vector2(Constants.GAME_WIDTH + 20, Constants.GAME_HEIGHT - 115));
                        bowlingPins[i].UpdateSpeed(scrollingSpeed);
                        i = bowlingPins.Count;
                    }
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            previousGamePadSate = gamePadState;
            previousKeyboardState = keyboardState;

            gamePadState = GamePad.GetState(PlayerIndex.One);
            keyboardState = Keyboard.GetState();

            #region Menu Updates
            if (menuActive)
            {
                if (instructions)
                {
                    if ((gamePadState.Buttons.Start == ButtonState.Pressed && previousGamePadSate.Buttons.Start != ButtonState.Pressed) ||
                    (keyboardState.IsKeyDown(Keys.Enter)) && !previousKeyboardState.IsKeyDown(Keys.Enter))
                        instructions = false;
                }
                else
                {
                    scrollingSpeed = 150f;
                    if ((gamePadState.Buttons.Start == ButtonState.Pressed && previousGamePadSate.Buttons.Start != ButtonState.Pressed) ||
                        (keyboardState.IsKeyDown(Keys.Enter)) && !previousKeyboardState.IsKeyDown(Keys.Enter))
                    {
                        MediaPlayer.Play(gameplayMusic);
                        score = 0;
                        gameActive = true;
                        dumball.GameOver = false;
                        floor.Activated = true;
                        menuActive = false;
                    }
                    if (gamePadState.Buttons.Y == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Space))
                    {
                        instructions = true;
                    }
                    if ((gamePadState.Buttons.Back == ButtonState.Pressed && previousGamePadSate.Buttons.Back != ButtonState.Pressed) ||
                        (keyboardState.IsKeyDown(Keys.Escape)) && !previousKeyboardState.IsKeyDown(Keys.Escape))
                        Exit();
                }                
            }
            #endregion Menu Updates

            #region Game Play Updates
            else if (gameActive)
            {
                if (((gamePadState.Buttons.Start == ButtonState.Pressed && previousGamePadSate.Buttons.Start != ButtonState.Pressed) || 
                    (keyboardState.IsKeyDown(Keys.Enter)) && !previousKeyboardState.IsKeyDown(Keys.Enter)) && !dumball.GameOver && dumball.Active)
                {
                    MediaPlayer.Pause();
                    gamePaused = true;
                    gameActive = false;
                }
                if (!dumball.Active && !dumball.GameOver)
                {
                    if (dumball.Bounds.Center.X <= 62) dumball.Active = true;
                    else
                    {
                        dumball.Update(gameTime);
                        floor.Update(gameTime);
                    }
                }
                else if (dumball.Active)
                {
                    randomSpawnTime -= gameTime.ElapsedGameTime.TotalSeconds;

                    if (randomSpawnTime <= 0)
                    {
                        RandomizeSpawn();
                    }
                    scrollingSpeed += 0.05f;

                    floor.UpdateSpeed(scrollingSpeed);

                    dumball.Update(gameTime);

                    if (!dumball.GameOver)
                    {
                        floor.Update(gameTime);

                        #region Hazard Updates
                        foreach (Hazard hazard in hazards)
                        {
                            if (!hazard.isOffScreen) hazard.UpdateSpeed(scrollingSpeed);
                            else hazard.UpdateSpeed(0);

                            if (!hazard.isOffScreen) hazard.Update(gameTime);

                            if (dumball.Bounds.CollidesWith(hazard.Bounds))
                            {
                                if (!hazardCollide) hazard.PlaySound();
                                hazardCollide = true;
                                dumball.GameOver = true;
                                scrollingSpeed = 150f;
                                _shaking = true;
                            }
                            else hazardCollide = false;
                        }
                        #endregion Hazard Updates

                        #region Bowling Pin Updates

                        foreach (BowlingPin pin in bowlingPins)
                        {
                            if (!pin.isOffScreen) pin.UpdateSpeed(scrollingSpeed);
                            else pin.UpdateSpeed(0);

                            if (!pin.isOffScreen) pin.Update(gameTime);

                            if (dumball.Bounds.CollidesWith(pin.Bounds))
                            {
                                dumball.PinHit = true;
                                pin.PlaySound();
                                pin.SpawnPosition(new Vector2(-151, Constants.GAME_HEIGHT - 115));
                                pin.UpdateSpeed(0);
                                score += 10;
                            }
                        }


                        #endregion Bowling Pin Updates   
                    }
                }
                else
                {
                    foreach (var pin in bowlingPins)
                    {
                        pin.SpawnPosition(new Vector2(-100, 0));
                    }
                    foreach (var hazard in hazards)
                    {
                        hazard.SpawnPosition(new Vector2(-100, 0));
                    }

                    gameActive = false;
                }
            }
            #endregion Game Play Updates

            /// Transition Function between Game Play and Menu
            else if (!gameActive && dumball.GameOver && !menuActive && !_shaking)
            {
                menuActive = true;
                _shakeTime = 0;
                MediaPlayer.Play(titleMusic);
            }

            #region Pause Menu Updates
            else if (gamePaused)
            {
                floor.Activated = false;
                dumball.Active = false;

                if ((gamePadState.Buttons.Start == ButtonState.Pressed && previousGamePadSate.Buttons.Start != ButtonState.Pressed) ||
                    (keyboardState.IsKeyDown(Keys.Enter)) && !previousKeyboardState.IsKeyDown(Keys.Enter))
                {
                    MediaPlayer.Resume();
                    gameActive = true;
                    floor.Activated = true;
                    dumball.Active = true;
                    gamePaused = false;
                }
                if ((gamePadState.Buttons.Back == ButtonState.Pressed && previousGamePadSate.Buttons.Back != ButtonState.Pressed) ||
                    (keyboardState.IsKeyDown(Keys.Escape)) && !previousKeyboardState.IsKeyDown(Keys.Escape))
                {
                    gamePaused = false;
                    menuActive = true;
                    hazardCollide = true;
                    dumball.Reset(new Vector2(20, Constants.GAME_HEIGHT - 135));
                    dumball.GameOver = true;
                    scrollingSpeed = 150f;
                    foreach (var pin in bowlingPins)
                    {
                        pin.SpawnPosition(new Vector2(-100, 0));
                    }
                    foreach (var hazard in hazards)
                    {
                        hazard.SpawnPosition(new Vector2(-100, 0));
                    }
                }                
            }
            #endregion Pause Menu Updates

            #region Firework Function
            if (score % 50 == 0 && _fireworksTime < 1200 && score != 0)
            {
                _fireworksTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                _fireworksActive = true;

                if (score % 250 == 0)
                {
                    _fireworks1.PlaceExplosion(new Vector2(_graphics.PreferredBackBufferWidth * 0.5f, 85));
                    _fireworks2.PlaceExplosion(new Vector2(_graphics.PreferredBackBufferWidth / 6, 120));
                    _fireworks3.PlaceExplosion(new Vector2((_graphics.PreferredBackBufferWidth * 5) / 6, 120));
                }
                else if (score % 100 == 0)
                {
                    _fireworks2.PlaceExplosion(new Vector2(_graphics.PreferredBackBufferWidth / 6, 120));
                    _fireworks3.PlaceExplosion(new Vector2((_graphics.PreferredBackBufferWidth * 5) / 6, 120));
                }
                else _fireworks1.PlaceExplosion(new Vector2(_graphics.PreferredBackBufferWidth * 0.5f, 85));
            }
            else if (_fireworksTime >= 1200)
            {
                _fireworksActive = false;
                if (!(score % 50 == 0)) _fireworksTime = 0;
            }
            #endregion Firework Function

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (_fireworksTime < 600 && _fireworksActive) GraphicsDevice.Clear(Color.Thistle);

            Matrix shakeTransform = Matrix.Identity;
            if (_shaking)
            {
                _shakeTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                shakeTransform = Matrix.CreateTranslation(3 * MathF.Sin(_shakeTime), 3 * MathF.Cos(_shakeTime), 0);
                if (_shakeTime > 1000) _shaking = false;
            }

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, shakeTransform);
            _spriteBatch.Draw(background, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
            floor.Draw(gameTime, _spriteBatch);
            foreach (var pin in bowlingPins)
            {
                if (!pin.isOffScreen) pin.Draw(gameTime, _spriteBatch);
            }
            foreach (var hazard in hazards)
            {
                if (!hazard.isOffScreen) hazard.Draw(gameTime, _spriteBatch);
            }
            dumball.Draw(gameTime, _spriteBatch);
            /*#region Collision Drawing
            var rect = new Rectangle((int)(dumball.Bounds.Center.X - dumball.Bounds.Radius),
                                         (int)(dumball.Bounds.Center.Y - dumball.Bounds.Radius),
                                         (int)(2 * dumball.Bounds.Radius), (int)(2 * dumball.Bounds.Radius));
            _spriteBatch.Draw(ball, rect, Color.White);
            var rect2 = new Rectangle();
            foreach (var pin in bowlingPins)
            {
                rect2 = new Rectangle((int)pin.Bounds.Left, (int)pin.Bounds.Top,
                                         (int)(pin.Bounds.Width), (int)(pin.Bounds.Height));
                _spriteBatch.Draw(rectangle, rect2, Color.White);
            }

            var rect3 = new Rectangle();
            foreach (var hazard in hazards)
            {
                rect3 = new Rectangle((int)(hazard.Bounds.Center.X - hazard.Bounds.Radius),
                                         (int)(hazard.Bounds.Center.Y - hazard.Bounds.Radius),
                                         (int)(2 * hazard.Bounds.Radius), (int)(2 * hazard.Bounds.Radius));

                _spriteBatch.Draw(ball, rect3, Color.White);
            }
            #endregion Collision Drawing*/
            _spriteBatch.End();

            #region Menu and Prompt Spritebatch
            _spriteBatch2.Begin();
            if (!instructions) _spriteBatch2.DrawString(_score, $"Score: {score}", new Vector2(20, 20), Color.Gold);
            if (gamePaused) _spriteBatch2.DrawString(_instructions, "  - Press Start or Enter to Resume -\n- Press Back or Esc to Return to Menu -", new Vector2(_graphics.PreferredBackBufferWidth * 0.5f - 225,
                _graphics.PreferredBackBufferHeight / 2), Color.White);
            if (menuActive && !instructions)
            {
                _spriteBatch2.DrawString(_instructions, " - Press Start or Enter to Play  -\nPress Y or Space Bar for Instructions\n - Press Back or Esc to Exit Game -",
                new Vector2(_graphics.PreferredBackBufferWidth * 0.5f - 205, 270), Color.White);

                _spriteBatch2.Draw(logo, new Vector2(_graphics.PreferredBackBufferWidth * 0.5f - 150, 133), new Rectangle(0, 0, 306, 94), Color.White);
            }
            if (instructions)
            {
                _spriteBatch2.DrawString(_instructions, "- Move with Left or Right on Keyboard,\n  or on Controller DPad or Left Stick\n- Press Space bar or A button to Jump\n- Press Start or Enter to pause the game\n- Avoid Spike balls and Collect Pins for points",
                new Vector2(124, 120), Color.White);

                _spriteBatch2.DrawString(_score, "- Press Enter or Start to Return to Menu -", new Vector2(60, 305), Color.Gold);
            }
            _spriteBatch2.End();
            #endregion Menu and Prompt Spritebatch

            base.Draw(gameTime);
        }
    }
}

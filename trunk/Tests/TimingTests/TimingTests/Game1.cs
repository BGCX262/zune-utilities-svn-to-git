using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;

namespace TimingTests
{
    /// <summary>
    /// This class is poorly written, completely undocumented, and in my opinion, quite useless.
    /// Ok kidding.  The point of this project is to explore how XNA timings work, and see what the
    /// maximum workloads are (approximately) on the zune.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        int[] arr1 = new int[35000];
        int draws = 0;
        int updates = 0;
        bool copyarr = false;
        Random r = new Random();
        Vector2 drawLoc = new Vector2(10, 20);

        GamePadState lastState;
        PlayerIndex pl1 = PlayerIndex.One;

        TimeSpan lastDrawGame;
        TimeSpan lastUpdateGame;
        TimeSpan lastDrawReal;
        TimeSpan lastUpdateReal;
        TimeSpan[] drTimeGame = new TimeSpan[40];
        TimeSpan[] upTimeGame = new TimeSpan[40];
        TimeSpan[] drTimeReal = new TimeSpan[40];
        TimeSpan[] upTimeReal = new TimeSpan[40];
        int drIxGm = 0;
        int drIxRl = 0;
        int upIxGm = 0;
        int upIxRl = 0;
        int NextIndex(ref int index)
        {
            index = Index(index + 1);
            return index;
        }
        int Index(int index)
        {
            if (index < 0) return 39;
            if (index > 39) return 0;
            return index;
        }
        TimeSpan avgTime(TimeSpan[] times)
        {
            TimeSpan total = TimeSpan.Zero;
            for (int i = 0; i < times.Length; i++)
                total += times[i];
            return new TimeSpan(total.Ticks / times.Length);
        }
        float fps(TimeSpan[] times)
        {
            TimeSpan avg = avgTime(times);
            return (float)(1000 / avg.TotalMilliseconds);
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("arial");
            base.Initialize();
            lastState = GamePad.GetState(PlayerIndex.One);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            GamePadState state = GamePad.GetState(pl1);
            // Allows the game to exit
            if (state.Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (state.Buttons.A == ButtonState.Released && lastState.Buttons.A == ButtonState.Pressed)
                copyarr = !copyarr;
            if (state.DPad.Up == ButtonState.Released && lastState.DPad.Up == ButtonState.Pressed)
            {
                this.IsFixedTimeStep = !this.IsFixedTimeStep;
            }
            if (state.DPad.Down == ButtonState.Pressed)
                updates = draws = 0;
            lastState = state;

            if (copyarr)
            {
                for (int i = 0; i < arr1.Length; i++)
                    arr1[i] = r.Next();
            }

            updates++;

            upTimeGame[NextIndex(ref upIxGm)] = gameTime.TotalGameTime - lastUpdateGame;
            upTimeReal[NextIndex(ref upIxRl)] = gameTime.TotalRealTime - lastUpdateReal;

            lastUpdateGame = gameTime.TotalGameTime;
            lastUpdateReal = gameTime.TotalRealTime;
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            draws++;
            String text = "";
            text += "Draws: " + draws + "   Updates: " + updates + "\n";
            text += "Array Copy: " + (copyarr ? "ON " : "OFF") + "  Fixed Time: " +
                (IsFixedTimeStep ? "YES" : "NO ") + "\n";
            if (gameTime.IsRunningSlowly)
                text += "Running Slow\n";
            else
                text += "Running OK\n";
            text += "\nDraw fps:\n   Game: " + fps(drTimeGame) + "\n   Real: " + fps(drTimeReal);
            text += "\nUpdate fps:\n   Game: " + fps(upTimeGame) + "\n   Real: " + fps(upTimeReal);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, text, drawLoc, Color.Black);
            spriteBatch.End();

            drTimeGame[NextIndex(ref drIxGm)] = gameTime.TotalGameTime - lastDrawGame;
            drTimeReal[NextIndex(ref drIxRl)] = gameTime.TotalRealTime - lastDrawReal;

            lastDrawGame = gameTime.TotalGameTime;
            lastDrawReal = gameTime.TotalRealTime;

        
            base.Draw(gameTime);
        }
    }
}

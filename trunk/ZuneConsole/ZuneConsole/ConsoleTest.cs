using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using ZuneUtils;

namespace ZuneConsole
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ConsoleTest : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GamePadState lastState;
        Random r = new Random(7);
        ZConsole console;
        RenderTarget2D trg;
        bool isPortrait = true;

        public ConsoleTest()
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
            lastState = GamePad.GetState(PlayerIndex.One);
            console = new ZConsole(this, "arial");
            Components.Add(console);
            base.Initialize();
            console.Write("Welcome to the Zune Debug Console\nby omegagames\n\n");
            console.Write("-Press the center button to write 40\ncharacters of random text to the console.\n\n");
            console.Write("-Press Up to toggle between portrait and\nlandscape display.\n\n");

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            updateRenderTarget();
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
            GamePadState state = GamePad.GetState(PlayerIndex.One);
            // Allows the game to exit
            if (state.Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (state.IsButtonUp(Buttons.A) && lastState.IsButtonDown(Buttons.A))
            {
                String s = "";
                for (int i = 0; i < 40; i++)
                    s += (char)(r.Next((int)' ', (int)'~'));

                console.Write(s);
            }
            if (state.IsButtonUp(Buttons.DPadUp) && lastState.IsButtonDown(Buttons.DPadUp))
            {
                isPortrait = !isPortrait;
                updateRenderTarget();
            }

            lastState = state;
            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetRenderTarget(0, trg);
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
            GraphicsDevice.SetRenderTarget(0, null);
            spriteBatch.Begin();
            Vector2 center = new Vector2(120, 160);
            Vector2 offset = new Vector2(120, 160);
            if (!isPortrait)
                center = new Vector2(160, 120);
            
            spriteBatch.Draw(trg.GetTexture(), offset, null, Color.White,
                (isPortrait ? 0 : MathHelper.PiOver2), center, 1f, SpriteEffects.None, 0);
            spriteBatch.End();
        }

        void updateRenderTarget()
        {
            trg = new RenderTarget2D(GraphicsDevice,
                isPortrait ? 240 : 320, isPortrait ? 320 : 240, 0, SurfaceFormat.Color);
            console.DisplayPortrait = isPortrait;
        }
    }
}

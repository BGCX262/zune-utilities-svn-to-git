using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;

namespace Zune
{
    /// <summary>
    /// Enumeration of different orientations the device.
    /// </summary>
    public enum ZuneOrientations
    {
        /// <summary>
        /// The Zune's normal orientation
        /// </summary>
        Portrait,
        /// <summary>
        /// The Zune oriented in landscape with the buttons on the right of the screen.
        /// </summary>
        Landscape,
        /// <summary>
        /// The Zune oriented in landscape with the buttons on the left of the screen.
        /// </summary>
        LandscapeLefty
    }

    /// <summary>
    /// Specifies the buttons present on all zune models
    /// </summary>
    public enum ZuneButtons
    {
        /// <summary>
        /// The DPad button Vector2ing Up, when accounting for orientation
        /// </summary>
        Up = 0,
        /// <summary>
        /// The DPad button Vector2ing Down, when accounting for orientation
        /// </summary>
        Down = 1,
        /// <summary>
        /// The DPad button Vector2ing Left, when accounting for orientation
        /// </summary>
        Left = 2,
        /// <summary>
        /// The DPad button Vector2ing Right, when accounting for orientation
        /// </summary>
        Right = 3,
        /// <summary>
        /// The button in the center of the DPad
        /// </summary>
        Select = 4,
        /// <summary>
        /// The Back button, regardless of device's orientation
        /// </summary>
        Back = 5,
        /// <summary>
        /// The Play/Pause button, regardless of device's orientation
        /// </summary>
        PlayPause = 6
    }


    public class Zune : GameComponent
    {
        public static readonly int ZunePixelRows = 320;
        public static readonly int ZunePixelColumns = 240;
        public static readonly Vector2 ZuneScreenCenter = new Vector2(120, 160);

        #region Private members
        private RenderTarget2D rendTrg;
        private SpriteBatch spb;
        private ZuneOrientations orientation;
        private bool drawingIsOpen = false;
        private GamePadState state1, state2;
        private MediaLibrary ml = new MediaLibrary();
        MediaManager media = new MediaManager();
        #endregion

        #region Public constructors
        /// <summary>
        /// Initializes a new instance of the Zune class.
        /// </summary>
        /// <param name="game">The Game object that this should be attached to</param>
        public Zune(Game game)
            : this(game, ZuneOrientations.Portrait)
        { }

        /// <summary>
        /// Initializes a new instance of the Zune class with the screen orientation specified.
        /// </summary>
        /// <param name="game">The Game object that this should be attached to</param>
        /// <param name="orientation">The screen orientation used to render</param>
        public Zune(Game game, ZuneOrientations orientation)
            : base(game)
        {
            this.orientation = orientation;
        }
        #endregion


        #region Public Input events
        /// <summary>
        /// An event that fires when a Zune button is initially pressed.
        /// </summary>
        public event ZuneButtonEventHandler ButtonDown;
        /// <summary>
        /// An event that fires when a previously pressed Zune button is released.
        /// </summary>
        public event ZuneButtonEventHandler ButtonUp;
        /// <summary>
        /// An event that fires whenever a Zune button is down.  If the button is held, 
        /// this event will fire on every call to Update().
        /// </summary>
        public event ZuneButtonEventHandler ButtonPressed;
        #endregion

        #region Public override methods
        public override void Initialize()
        {
            base.Initialize();
            rendTrg = new RenderTarget2D(Game.GraphicsDevice, ZunePixelColumns,
                ZunePixelRows, 0, SurfaceFormat.Color);
            spb = new SpriteBatch(Game.GraphicsDevice);
            state1 = state2 = GamePad.GetState(PlayerIndex.One);
        }

        public override void Update(GameTime gameTime)
        {
            state2 = state1;
            state1 = GamePad.GetState(PlayerIndex.One);
            CheckInputEvents(gameTime);
            media.Update();
            base.Update(gameTime);
        }

        #endregion

        #region Public Methods/Properties: Drawing
        public ZuneOrientations Orientation
        {
            get { return orientation; }
            set
            {
                AssertNotDrawing();
                ZuneOrientations old = orientation;
                orientation = value;
                if (old != orientation)
                    ChangeOrientation();
            }
        }

        public Vector2 Size
        {
            get
            {
                return orientation == ZuneOrientations.Portrait
                    ? new Vector2(ZunePixelColumns, ZunePixelRows)
                    : new Vector2(ZunePixelRows, ZunePixelColumns);
            }
        }

        public void BeginDrawing()
        {
            drawingIsOpen = true;
            Game.GraphicsDevice.SetRenderTarget(0, rendTrg);
        }

        public void Draw()
        {
            AssertNotDrawing();
            Texture2D rendtex = rendTrg.GetTexture();
            Vector2 texCenter = (Orientation == ZuneOrientations.Portrait ?
                new Vector2(ZunePixelColumns / 2, ZunePixelRows / 2) :
                new Vector2(ZunePixelRows / 2, ZunePixelColumns / 2));
            float angle = 0.0f;
            if (Orientation == ZuneOrientations.Landscape)
                angle = MathHelper.PiOver2;
            else if (Orientation == ZuneOrientations.LandscapeLefty)
                angle = -MathHelper.PiOver2;

            spb.Begin();
            spb.Draw(
                rendtex,
                ZuneScreenCenter,
                null,
                Color.White,
                angle,
                texCenter,
                1.0f,
                SpriteEffects.None,
                0);
            spb.End();
        }

        public void EndDrawing()
        {
            Game.GraphicsDevice.SetRenderTarget(0, null);
            drawingIsOpen = false;
        }
        #endregion

        #region Public Methods/Properties: Input
        public bool IsPressed(ZuneButtons zuneButton)
        {
            Buttons b = ButtonFromZuneButton(zuneButton);
            return state1.IsButtonDown(b);
        }

        public bool IsNewPress(ZuneButtons zuneButton)
        {
            Buttons b = ButtonFromZuneButton(zuneButton);
            bool down1 = state1.IsButtonDown(b);
            bool down2 = state2.IsButtonDown(b);
            return ((down1 != down2) && down1);
        }
        public ZuneButtons[] GetAllPressed()
        {
            List<ZuneButtons> btns = new List<ZuneButtons>();
            if (IsPressed(ZuneButtons.Select)) btns.Add(ZuneButtons.Select);
            if (IsPressed(ZuneButtons.PlayPause)) btns.Add(ZuneButtons.PlayPause);
            if (IsPressed(ZuneButtons.Back)) btns.Add(ZuneButtons.Back);
            if (IsPressed(ZuneButtons.Up)) btns.Add(ZuneButtons.Up);
            if (IsPressed(ZuneButtons.Down)) btns.Add(ZuneButtons.Down);
            if (IsPressed(ZuneButtons.Left)) btns.Add(ZuneButtons.Left);
            if (IsPressed(ZuneButtons.Right)) btns.Add(ZuneButtons.Right);
            return btns.ToArray();
        }
        #endregion

        #region Public Methods/Properties: Media
        public MediaLibrary MediaLibrary
        {
            get { return ml; }
        }
        public MediaManager Media
        {
            get { return media; }
        }

        #endregion

        #region Private Methods/Properties: Drawing
        private void ChangeOrientation()
        {
            int w = (int)Size.X,
                h = (int)Size.Y;
            rendTrg = new RenderTarget2D(Game.GraphicsDevice, w, h, 0, SurfaceFormat.Color);
        }

        private void AssertNotDrawing()
        {
            if (drawingIsOpen)
                throw new Exception("EndDrawing() must be called before this operation can be completed.");
        }
        private void AssertDrawing()
        {
            if (!drawingIsOpen)
                throw new Exception("BeginDrawing() must be called before this operation can be completed.");
        }
        #endregion

        #region Private Methods/Properties: Input
        private void CheckInputEvents(GameTime gameTime)
        {
            bool isDown;
            bool wasDown;
            Buttons btn;
            for (int i = 0; i <= 6; i++)
            {
                btn = ButtonFromZuneButton((ZuneButtons)i);
                isDown = state1.IsButtonDown(btn);
                wasDown = state2.IsButtonDown(btn);
                if (ButtonDown != null && (isDown && !wasDown))
                    ButtonDown(this, new ZuneButtonEventArgs((ZuneButtons)i, gameTime));
                else if (ButtonUp != null && (!isDown && wasDown))
                    ButtonUp(this, new ZuneButtonEventArgs((ZuneButtons)i, gameTime));

                if (ButtonPressed != null && (isDown))
                    ButtonPressed(this, new ZuneButtonEventArgs((ZuneButtons)i, gameTime));
            }
        }

        private Buttons ButtonFromZuneButton(ZuneButtons zBtn)
        {
            switch (zBtn)
            {
                case ZuneButtons.Up:
                    if (orientation == ZuneOrientations.Portrait)
                        return Buttons.DPadUp;
                    else if (orientation == ZuneOrientations.Landscape)
                        return Buttons.DPadRight;
                    else
                        return Buttons.DPadLeft;

                case ZuneButtons.Down:
                    if (orientation == ZuneOrientations.Portrait)
                        return Buttons.DPadDown;
                    else if (orientation == ZuneOrientations.Landscape)
                        return Buttons.DPadLeft;
                    else
                        return Buttons.DPadRight;

                case ZuneButtons.Left:
                    if (orientation == ZuneOrientations.Portrait)
                        return Buttons.DPadLeft;
                    else if (orientation == ZuneOrientations.Landscape)
                        return Buttons.DPadUp;
                    else
                        return Buttons.DPadDown;

                case ZuneButtons.Right:
                    if (orientation == ZuneOrientations.Portrait)
                        return Buttons.DPadRight;
                    else if (orientation == ZuneOrientations.Landscape)
                        return Buttons.DPadDown;
                    else
                        return Buttons.DPadUp;

                case ZuneButtons.Select:
                    return Buttons.A;

                case ZuneButtons.Back:
                    return Buttons.Back;

                case ZuneButtons.PlayPause:
                    return Buttons.B;

                default:
                    throw new ArgumentException("zBtn was not one of the values in ZuneButtons", "zBtn");
            }
        }
        #endregion

    }


    public delegate void ZuneButtonEventHandler(object sender, ZuneButtonEventArgs e);

    /// <summary>
    /// Contains data for the Zune button events
    /// </summary>
    public class ZuneButtonEventArgs : EventArgs
    {
        private ZuneButtons button;
        private GameTime gameTime;

        /// <summary>
        /// Initializes a new instance of the ZuneButtonEventArgs class.
        /// </summary>
        /// <param name="button">The ZuneButton associated with this event</param>
        /// <param name="gameTime">The GameTime given when an Update() call generated the event</param>
        public ZuneButtonEventArgs(ZuneButtons button, GameTime gameTime)
        {
            this.button = button;
            this.gameTime = gameTime;
        }

        /// <summary>
        /// Gets the ZuneButton associated with this event.
        /// </summary>
        public ZuneButtons Button { get { return button; } }

        /// <summary>
        /// Gets the GameTime associated with this event.
        /// </summary>
        public GameTime GameTime { get { return gameTime; } }
    }
}
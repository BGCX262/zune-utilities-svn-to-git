using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using System.Collections;

namespace ZuneUtils
{

    /// <summary>
    /// An object which allows writing text to the screen.
    /// </summary>
    public class ZConsole : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Private Members
        Color color;
        SpriteFont font = null;
        String fontRescource;
        bool isPortrait = true;
        SpriteBatch sb;
        LineQueue lq = new LineQueue();
        int PortraitWidth = 240 - 2;
        int LandscapeWidth = 320 - 2;
        #endregion

        #region Public Constructors
        /// <summary>
        /// Create a new Console using the specified font.
        /// </summary>
        /// <param name="game">The Game this GameComponent belongs to</param>
        /// <param name="fontResourceName">The Resource Name of the included SpriteFont resource used for rendering text</param>
        public ZConsole(Game game, String fontResourceName)
            : this(game, fontResourceName, new Color(20, 20, 20))
        { }

        /// <summary>
        /// Create a new Console using the specified font and color.
        /// </summary>
        /// <param name="game">The Game this GameComponent belongs to</param>
        /// <param name="fontResourceName">The Resource Name of the included SpriteFont resource used for rendering text</param>
        /// <param name="color">The console's display text color</param>
        public ZConsole(Game game, String fontResourceName, Color color)
            : base(game)
        {
            fontRescource = fontResourceName;
            this.color = color;
        }
        #endregion

        #region Public Properties / Methods
        /// <summary>
        /// Gets or sets the color used to render text.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
        /// <summary>
        /// Detrmines whether the console is rendered in Portrait mode.
        /// </summary>
        public bool DisplayPortrait
        {
            get { return isPortrait; }
            set 
            {
                if (isPortrait == value)
                    return;
                isPortrait = value;
                SetupLines();
            }
        }
        /// <summary>
        /// Detrmines whether the console is rendered in Landscape mode.
        /// </summary>
        public bool DisplayLandscape
        {
            get { return !isPortrait; }
            set 
            {
                if (isPortrait == !value)
                    return;
                isPortrait = !value;
                SetupLines();
            }
        }

        /// <summary>
        /// Writes the specified text to the screen and moves to the next line
        /// </summary>
        /// <param name="text">The text to write</param>
        public void WriteLine(string text)
        {
            if (text == null) text = "";
            Write(text + "\n");
        }

        /// <summary>
        /// Writes the specified text to the screen
        /// </summary>
        /// <param name="text">The text to write</param>
        public void Write(String text)
        {
            if (font == null)
                LoadContent();
            if (lq[0] != null)
                text = lq[0] + text; //prepend previous line.
            String[] str = text.Split('\n');
            for (int i = 0; i < str.Length; i++)
            {
                if (font.MeasureString(str[i]).X > (isPortrait ? PortraitWidth : LandscapeWidth))
                {
                    WriteWrapString(str[i],(i == 0));
                }
                else if (i == 0)
                    lq.ReplaceFirstLine(str[i]);
                else
                    lq.Add(str[i]);
            }
        }

        /// <summary>
        /// Clears the console of text
        /// </summary>
        public void Clear()
        {
            lq.Clear();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to Draw itself to the screen
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            if (font == null)
                LoadContent();
            String output = "";
            for (int i = lq.Count - 1; i >= 0; i--)
            {
                if (lq[i] == null) continue;
                output += lq[i] + "\n";
            }
            if (color.A < byte.MaxValue)
                sb.Begin(SpriteBlendMode.AlphaBlend);
            else
                sb.Begin();
            sb.DrawString(font, output, Vector2.Zero, color);
            sb.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }
        #endregion

        protected override void LoadContent()
        {
            sb = new SpriteBatch(Game.GraphicsDevice);
            font = Game.Content.Load<SpriteFont>(fontRescource);
            SetupLines();
            base.LoadContent();
        }

        #region Private Functions
        private void WriteWrapString(string p, bool appendFirstLine)
        {
            int ixSt = 0, length = 5;
            while (ixSt < p.Length)
            {
                while ((length + ixSt) < p.Length && font.MeasureString(
                    p.Substring(ixSt, length)).X < (isPortrait ? PortraitWidth : LandscapeWidth))
                    length += 5;
                length -= 5;
                while ((length + ixSt) < p.Length && font.MeasureString(
                    p.Substring(ixSt, length)).X < (isPortrait ? PortraitWidth : LandscapeWidth))
                    length++;
                length--;
                if (ixSt + length > p.Length) length = p.Length - ixSt;
                if (!appendFirstLine)
                    lq.Add(p.Substring(ixSt, length));
                else
                {
                    lq.ReplaceFirstLine(p.Substring(ixSt, length));
                    appendFirstLine = false;
                }
                ixSt += length;
            }
        }

        private void SetupLines()
        {
            if (font == null)
                LoadContent();
            int screenHeight = (isPortrait ? LandscapeWidth : PortraitWidth);
            int maxLines = 1;
            string str = ".";
            while (font.MeasureString(str).Y <= screenHeight)
            {
                str += "\n.";
                maxLines++;
            }

            lq.Resize(maxLines);
        }
        #endregion

        #region LineQueue class
        private class LineQueue
        {
            String[] queue;
            int _ix = 0;
            private int index
            {
                get { return _ix; }
                set
                {
                    _ix = value;
                    while (_ix < 0) 
                        _ix += queue.Length;
                    _ix = _ix % queue.Length;
                }
            }

            public LineQueue() 
            {
                queue = new String[1];
            }

            public String this[int index]
            {
                get 
                {
                    if (index < 0 || index >= queue.Length)
                        throw new ArgumentOutOfRangeException("index");
                    return queue[(this.index - index + queue.Length) % queue.Length]; 
                }
            }

            public int Count { get { return queue.Length; } }

            public void Add(String line)
            {
                if (line == null)
                    throw new ArgumentNullException("line","line cannot be null");
                index++;
                queue[index] = line;
            }
            public void ReplaceFirstLine(String line)
            {
                if (line == null)
                    throw new ArgumentNullException("line", "line cannot be null");
                queue[index] = line;
            }

            public void Clear()
            {
                for (int i = 0; i < queue.Length; i++)
                    queue[i] = null;
            }

            public void Resize(int size)
            {
                if (size <= 0)
                    throw new ArgumentOutOfRangeException("size", "size must be greater than 0");
                String[] newQ = new String[size];
                int startIx = index;
                int i = 0;
                newQ[i++] = queue[index++];
                if (size < queue.Length)
                    index += (queue.Length - size);
                else
                    i += size - queue.Length;
                i = i % size;
                while (index != startIx)
                {
                    newQ[i++] = queue[index++];
                    i %= size;
                }
                index = 0;
                queue = newQ;
            }

        }
        #endregion
    }
}
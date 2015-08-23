using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;

namespace Zune
{
    public class MediaManager
    {
        private static Random Rnd = new Random();

        Collection<Song> nowPlaying = new Collection<Song>();
        int songIndex = -1;
        TimeSpan playPos = TimeSpan.Zero;
        bool repeating = false;

        /// <summary>
        /// Creates a new MediaManager
        /// </summary>
        public MediaManager()
        {
            if (MediaPlayer.Queue.Count < 1) return;
            for (int i = 0; i < MediaPlayer.Queue.Count; i++)
                nowPlaying.Add(MediaPlayer.Queue[i]);
            songIndex = MediaPlayer.Queue.ActiveSongIndex;
        }

        public void Update()
        {
            if (MediaPlayer.State != MediaState.Playing) return;
            if (MediaPlayer.PlayPosition > CurrentSong.Duration - TimeSpan.FromMilliseconds(300))
                NextSong();
        }

        #region Public MediaPlayer Command Methods
        public void Play()
        {
            if (MediaPlayer.State == MediaState.Playing) return; //A job well done.
            if (CurrentSong == null) return; //Nothing to play.
            bool wasPaused = (MediaPlayer.State == MediaState.Paused);
                MediaPlayer.Play(CurrentSong);            
            if (wasPaused)
                MediaPlayer.PlayPosition = playPos;
        }

        public void Pause()
        {
            if (MediaPlayer.State != MediaState.Playing) return;
            playPos = MediaPlayer.PlayPosition;
            MediaPlayer.Pause();
        }

        public void Stop()
        {
            MediaPlayer.Stop();
        }

        public void Next()
        {
            NextSong();
        }

        public void Previous()
        {
            PreviousSong();
        }

        public void ShuffleTracks()
        {
            Song tmp;
            int index;
            for (int i = 0; i < Count; i++)
            {
                index = Rnd.Next(Count);
                if (index == i) continue;
                if (i == songIndex) 
                    songIndex = index;
                else if (index == songIndex) 
                    songIndex = i;
                tmp = nowPlaying[i];
                nowPlaying[i] = nowPlaying[index];
                nowPlaying[index] = tmp;
            }
        }

        #endregion

        #region Public Properties
        public float Volume
        {
            get { return MediaPlayer.Volume; }
            set { MediaPlayer.Volume = value; }
        }

        public bool Loop
        {
            get { return repeating; }
            set { repeating = value; }
        }

        public Song this[int index]
        {
            get { return nowPlaying[index]; }
            set
            {
                if (index < 0 || index > nowPlaying.Count)
                    throw new IndexOutOfRangeException("index was out of range");
                if (value == null)
                    throw new ArgumentNullException("Song", "Song cannot be null");
                nowPlaying[index] = value;
                if (index == songIndex)
                {
                    if (MediaPlayer.State == MediaState.Playing)
                        MediaPlayer.Play(CurrentSong);
                    else if (MediaPlayer.State == MediaState.Paused)
                        MediaPlayer.Stop();
                }
            }
        }

        public int Count { get { return nowPlaying.Count; } }

        public Song CurrentSong
        {
            get
            {
                if (songIndex < 0) return null;
                return nowPlaying[songIndex];
            }
        }

        public int CurrentIndex
        {
            get { return songIndex; }
            set
            {
                if (value >= nowPlaying.Count)
                    throw new ArgumentOutOfRangeException("CurrentIndex");
                if (value < -1) value = -1;
                songIndex = value;
                if (value < 0 || MediaPlayer.State == MediaState.Paused)
                {
                    playPos = TimeSpan.Zero;
                    MediaPlayer.Stop();
                }
                else
                    MediaPlayer.Play(CurrentSong);
            }
        }
        #endregion

        #region Queue Manipulation Methods
        #region Add/Insert Overloads
        public void Add(Song song)
        {
            Insert(song, nowPlaying.Count);
        }
        public void Add(Artist artist)
        {
            foreach (Song s in artist.Songs)
                Add(s);
        }
        public void Add(Album album)
        {
            foreach (Song s in album.Songs)
                Add(s);
        }
        public void Add(Genre genre)
        {
            foreach (Song s in genre.Songs)
                Add(s);
        }
        public void Add(Playlist playlist)
        {
            foreach (Song s in playlist.Songs)
                Add(s);
        }

        public void Insert(Song song, int index)
        {
            InsertSong(song, index);
        }
        public void Insert(Artist artist, int index)
        {
            foreach (Song s in artist.Songs)
                Insert(s, index++);
        }
        public void Insert(Album album, int index)
        {
            foreach (Song s in album.Songs)
                Insert(s, index++);
        }
        public void Insert(Genre genre, int index)
        {
            foreach (Song s in genre.Songs)
                Insert(s, index++);
        }
        public void Insert(Playlist playlist, int index)
        {
            foreach (Song s in playlist.Songs)
                Insert(s, index++);
        }
        #endregion

        public void Remove(int index)
        {
            RemoveSong(index);
        }

        public void Remove(Song song)
        {
            int index = -1;
            while ((index = nowPlaying.IndexOf(song)) >= 0)
                Remove(index);
        }

        public void Clear()
        {
            nowPlaying.Clear();
            songIndex = -1;
            MediaPlayer.Stop();
        }
        #endregion

        #region Private Functions
        private void NextSong()
        {
            if (songIndex < nowPlaying.Count - 1)
            {
                MediaPlayer.Play(nowPlaying[songIndex++]);
                return;
            }
            songIndex = 0;
            if (repeating)
                MediaPlayer.Play(nowPlaying[songIndex]);
            else
                MediaPlayer.Stop();
        }

        private void PreviousSong()
        {
            songIndex = songIndex - 1;
            if (songIndex < 0) songIndex = nowPlaying.Count - 1;
            MediaPlayer.Play(nowPlaying[songIndex]);
        }

        private void InsertSong(Song s, int index)
        {
            if (s == null)
                throw new ArgumentNullException("song", "song cannot be null");
            if (index <= songIndex)
                songIndex++;
            nowPlaying.Insert(index, s);
        }

        private void RemoveSong(int index)
        {
            if (index == songIndex) NextSong();
            if (index < songIndex) songIndex--;
            nowPlaying.RemoveAt(index);
            if (nowPlaying.Count < 1)
            {
                songIndex = -1;
                MediaPlayer.Stop();
            }
        }
        #endregion
    }
}
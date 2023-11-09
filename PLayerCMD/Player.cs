using NAudio.Wave;
using Player;

namespace PLayerCMD
{

    public class PlayerInterface
    {
        Dictionary<ConsoleKey, Action> playerCommands ;
        DrawContent asciiInterface;
        PlayMp3 mp3;
        int playlist_Controller;
        bool playlistExit = false;

        public bool PlaylistExit
        {
            get => playlistExit;
        }

        public int Playlist_Controller
        {
            get => playlist_Controller;
            set => playlist_Controller = value;
        }

        public PlayerInterface()
        {
            this.playlist_Controller = 1;
            this.playerCommands = new Dictionary<ConsoleKey, Action>()
            {
                {ConsoleKey.Spacebar, () =>
                {
                    if (mp3.IsPlaying)
                    {
                        mp3.Pause();
                    }
                    else
                        mp3.Play();
                }
                },
                {ConsoleKey.LeftArrow, () =>
                {
                    mp3.SkipBack(10);
                }
                },
                {ConsoleKey.RightArrow,() =>
                {
                    mp3.Skip(10);
                }
                },
                {ConsoleKey.DownArrow,() =>
                {
                    mp3.DecraseVolume(0.1f);
                    this.asciiInterface.VolumeBarDraw(mp3.MusicVolume);
                }
                },
                {ConsoleKey.UpArrow,() =>
                {
                    mp3.RaiseVolume(0.1f);
                    this.asciiInterface.VolumeBarDraw(mp3.MusicVolume);
                }
                },
                {ConsoleKey.Escape, () =>
                {
                    mp3.EndMusic = true;
                    this.playlistExit = true;
                }
                },
                {ConsoleKey.PageUp, () =>
                {
                    this.playlist_Controller = 1;
                    this.mp3.EndMusic = true;
                }
                },
                {ConsoleKey.PageDown, () =>
                {
                    this.playlist_Controller = -1;
                    this.mp3.EndMusic = true;
                }
                }
            };
        }
        public PlayerInterface(string directory)
        {
            this.mp3 = new PlayMp3(directory);
            this.playerCommands = new Dictionary<ConsoleKey, Action>()
            {
                {ConsoleKey.Spacebar, () =>
                {
                    if (mp3.IsPlaying)
                    {
                        mp3.Pause();
                    }
                    else
                        mp3.Play();
                }
                },
                {ConsoleKey.LeftArrow, () =>
                {
                    mp3.SkipBack(10);
                }
                },
                {ConsoleKey.RightArrow,() =>
                {
                    mp3.Skip(10);
                }
                },
                {ConsoleKey.DownArrow,() =>
                {
                    mp3.DecraseVolume(0.1f);
                    this.asciiInterface.VolumeBarDraw(mp3.MusicVolume);
                }
                },
                {ConsoleKey.UpArrow,() =>
                {
                    mp3.RaiseVolume(0.1f);
                    this.asciiInterface.VolumeBarDraw(mp3.MusicVolume);
                }
                },
                {ConsoleKey.Escape, () =>
                {
                    mp3.EndMusic = true;
                    this.playlistExit = true;
                }
                },
                {ConsoleKey.PageUp, () =>
                {
                    this.playlist_Controller = 1;
                    this.mp3.EndMusic = true;
                } 
                },
                {ConsoleKey.PageDown, () =>
                {
                    this.playlist_Controller = -1;
                    this.mp3.EndMusic = true;
                }
                }
            };
            this.asciiInterface = new DrawContent(mp3.MusicDuration, mp3.MusicName,mp3.MusicVolume);
            this.playlist_Controller = 1;
        }


        public void Controls()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey().Key;
                if (this.playerCommands.ContainsKey(key))
                {
                    this.playerCommands[key]();
                }
            }
        }

        public void UpdateMusic(string directory)
        {
            this.mp3 = new PlayMp3(directory);
            this.asciiInterface = new DrawContent(mp3.MusicDuration, mp3.MusicName, mp3.MusicVolume);

        }

        public void Stop()
        {
            this.mp3.Stop();
            this.asciiInterface.Dispose();
        }
        public void Play()
        {
            this.mp3.Play();
            Console.CursorVisible = false;
            Console.Clear();
            while (!this.mp3.EndMusic)
            {
                Controls();
                this.asciiInterface.Update(this.mp3.MusicCurrentTime);
            }
            Stop();
        }

        
    }

    internal class DrawContent
    {
        AsciiGenerator asciiMusicName;
        double durationTime;
        int[] screenSize;
        int currentDash = 1;
        double currentTime;
        Thread drawThread;
        bool isplaying;
        int framesAwaitTime;


        List<string> reprodutionBarDetail = new List<string>();
        List<string> reprodutionBar = new List<string>();
        List<double> reprodutionBarTime = new List<double>();
        string[] volumeBar = new string[12];
        string[] volumeBarDetail = new string[12];


        internal DrawContent(double duration, string name, float volume)
        {
            Console.SetWindowSize(200, 40);
            int reprodutionBarLenght = Console.WindowWidth - 1;
            double timePerDash;
            double time;

            this.screenSize = new int[] { Console.WindowWidth, Console.WindowHeight };
            this.reprodutionBarTime = new List<double>();
            this.framesAwaitTime = 620;         
            this.drawThread = new Thread(Draw);
            this.asciiMusicName = new AsciiGenerator(name, true);
            this.durationTime = duration;

            timePerDash = duration / reprodutionBarLenght;
            time = timePerDash;

            for(int i = 0;i < reprodutionBarLenght; i++)
            {
                reprodutionBarTime.Add(time);
                time += timePerDash;
            }

            for (int i = 0; i < reprodutionBarLenght; i++)
            {
                this.reprodutionBar.Add(" ");
                this.reprodutionBarDetail.Add("=");
            }

            for (int i = 0; i < 12; i++)
            {
                this.volumeBar[i] = "|";
                this.volumeBarDetail[i] = "H";
            }

            for (int i = 0;i < this.volumeBar.Length; i++)
            {
                this.volumeBar[i] = "|";
            }

            this.volumeBar[0] = "=";
            this.volumeBar[(int)((1.1f - volume) * 10)] = "+";
            this.volumeBar[11] = "=";

            isplaying = true;
            this.drawThread.Start();
        }

        private string Concat(List<string> list)
        {
            string text = "";
            try
            {
                foreach (var t in list)
                {
                    text += t;
                }
            }
            catch
            {
                text = string.Empty;
            }
            return text;
        }

        public void Dispose()
        {
            
            this.isplaying = false;
            Thread.Sleep(this.framesAwaitTime);
            this.framesAwaitTime = 0;

        }

        public void VolumeBarDraw(float volume)
        {
            int posX = Console.WindowWidth - 3;
            int posY = Console.WindowHeight / 2 - 5;

            for (int i = 0; i < this.volumeBar.Length; i++)
            {
                this.volumeBar[i] = "|";
            }

            this.volumeBar[0] = "=";
            this.volumeBar[11] = "=";

            this.volumeBar[(int)((1.1f - volume) * 10)] = "+";
            for (int i = 0; i < this.volumeBar.Length; i++)
            {
                Console.SetCursorPosition(posX, posY);
                Console.WriteLine(this.volumeBarDetail[i]);
                posY++;
            }
            posY = Console.WindowHeight / 2 - 5;
            posX++;
            for (int i = 0; i < this.volumeBar.Length; i++)
            {
                Console.SetCursorPosition(posX, posY);
                Console.WriteLine(this.volumeBar[i]);
                posY++;
            }
            posY = Console.WindowHeight / 2 - 5;
            posX++;
            for (int i = 0; i < this.volumeBar.Length; i++)
            {
                Console.SetCursorPosition(posX, posY);
                Console.WriteLine(this.volumeBarDetail[i]);
                posY++;
            }
        }

        private void Draw()
        {
            while (this.isplaying)
            {
                Thread.Sleep(this.framesAwaitTime);
                Console.Clear();
                this.asciiMusicName.Draw(new int[] { 0, Console.WindowHeight / 2 - 5 });
                Console.SetCursorPosition(0, Console.WindowHeight - 3);

                Console.WriteLine(Concat(this.reprodutionBarDetail));

                Console.WriteLine(Concat(this.reprodutionBar));

                Console.WriteLine(Concat(this.reprodutionBarDetail));

            }
            Console.Clear();
            
        }

        private void ResizeInterface()
        {
            int width = Console.WindowWidth;
            if (this.screenSize[0] != Console.WindowWidth)
            {
                int reprodutionBarLenght = Console.WindowWidth-1;
                this.screenSize[0] = Console.WindowWidth;

                this.reprodutionBar.Clear();
                this.reprodutionBarDetail.Clear();
                this.reprodutionBarTime.Clear();

                for (int i = 0; i < reprodutionBarLenght; i++)
                {
                    this.reprodutionBar.Add(" ");
                    this.reprodutionBarDetail.Add("=");
                }
                double timePerDash = this.durationTime / reprodutionBarLenght;
                double time = timePerDash;

                for (int i = 0; i < reprodutionBarLenght; i++)
                {
                    reprodutionBarTime.Add(time);
                    time += timePerDash;
                }
            }

            if (this.screenSize[1] != Console.WindowHeight)
            {
                this.screenSize[1] = Console.WindowHeight;
            }
        }

        private void ReprodutionBarUpdate()
        {
            if (currentDash < this.reprodutionBarTime.Count)
            {
                if (this.currentTime > this.reprodutionBarTime[currentDash])
                {
                    for (int i = 0; i <= currentDash; i++)
                    {
                        this.reprodutionBar[i] = "-";
                    }
                    this.currentDash++;
                }
            }

            double time = this.currentDash - 1 < 0 ? 0 : this.reprodutionBarTime[currentDash - 1];
            if (this.currentTime < time)
            {
                this.reprodutionBar[currentDash] = " ";
                this.currentDash--;
            }

        }

        public void Update(double currentTime)
        {
            this.currentTime = currentTime;
            ResizeInterface();
            ReprodutionBarUpdate();
            
        }
    }

    internal class PlayMp3
    {
        private string musicName;
        private double musicDuration;
        private bool endMusic = false;


        public bool EndMusic
        {
            get => endMusic;
            set => endMusic = value;
        }
        public double MusicCurrentTime
        {
            get { return this.music.CurrentTime.TotalMilliseconds; }
        }
        public double MusicDuration
        {
            get { return musicDuration; }
        }

        public bool IsPlaying
        {
            get { return (this.audioController.PlaybackState == PlaybackState.Playing); }
        }

        public string MusicName
        {
            get { return musicName; }
        }

        public float MusicVolume
        {
            get => this.audioController.Volume;
        }

        AudioFileReader music;
        WaveOutEvent audioController;
        
        internal PlayMp3(string directory)
        {
            this.musicName = directory.Substring(directory.LastIndexOf("\\")+1);
            this.musicName = this.musicName.Remove(this.musicName.IndexOf(".mp3"));
            this.music = new AudioFileReader(directory);
            this.musicDuration = music.TotalTime.TotalMilliseconds;
            this.audioController = new WaveOutEvent();
            this.audioController.Init(music);
        }

        public void Play()
        {
            this.audioController.Play();
            this.audioController.PlaybackStopped += StopMusic;
        }

        public void Stop()
        {
            this.audioController.Stop();
            this.audioController.Dispose();
            this.music.Dispose();
        }

        private void StopMusic(object sender, StoppedEventArgs e)
        {
            this.endMusic = true;
        }

        public void Pause()
        {
            this.audioController.Pause();   
        }

        public void RaiseVolume(float volume)
        {
            if(this.audioController.Volume + volume < 1.0f)
            {
                this.audioController.Volume += volume;
            }
        }

        public void DecraseVolume(float volume)
        {
            if(this.audioController.Volume - volume > 0)
            {
                this.audioController.Volume -= volume;
            }
            
        }

        public void Skip(int seconds)
        {
            this.music.Skip(seconds);
        }

        public void SkipBack(int seconds)
        {
            this.music.Skip(seconds * -1);
        }

    }
}
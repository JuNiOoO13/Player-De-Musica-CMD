
using AngleSharp.Media;
using System.Web;
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Player
{

    static class YoutubeSearch
    {
        static async public Task<string> DownloadAsyncMusicByName(string musicName)
        {
            //Baixa a musica e retorna uma Task com o nome do directorio, onde a musica está 
            //Essa função procura a musica pelo nome e pega o primeiro resultado que encontra
            var youtube = new YoutubeClient();
            VideoSearchResult? videoInfo = null;
            IAsyncEnumerable<VideoSearchResult> results = youtube.Search.GetVideosAsync(musicName);
            await foreach (var result in results)
            {
                videoInfo = result;
                break;
            }

            if (videoInfo != null)
            {
                string nomeMusica = videoInfo.Title;
                nomeMusica = RemoveSpecialCaracters(nomeMusica);
                string directory = Path.Combine(CreateMusicDir(), nomeMusica + ".mp3");
                if (!File.Exists(directory))
                {
                    ValueTask<Video> taks = youtube.Videos.GetAsync(videoInfo.Url);
                    Video video = taks.Result;
                    ValueTask<StreamManifest> manisfests = youtube.Videos.Streams.GetManifestAsync(videoInfo.Url);
                    StreamManifest manisfest = manisfests.Result;
                    IStreamInfo streamInfo = manisfest.Streams[manisfest.Streams.Count - 1];
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, directory);

                }
                return directory;
            }
            else
                return "Not Found";
        }

        static async public Task<string> DownloadAsyncMusicByUrl(string url)
        {
            //Baixa a musica e retorna uma Task com o nome do directorio, onde a musica está 
            //Essa função procura a musica pelo nome e pega o primeiro resultado que encontra
            string nomeMusica;
            string directory;
            YoutubeClient youtube= new YoutubeClient();
            Video music = youtube.Videos.GetAsync(url).Result;
            nomeMusica = music.Title;
            nomeMusica = RemoveSpecialCaracters(nomeMusica);
            directory = Path.Combine(CreateMusicDir(), nomeMusica + ".mp3");
            StreamManifest manifest = youtube.Videos.Streams.GetManifestAsync(music.Url).Result;
            IStreamInfo stream = manifest.Streams[manifest.Streams.Count - 1];
            await youtube.Videos.Streams.DownloadAsync(stream, directory);
            return directory;

        }


        static async public Task<string[]> SearchPlaylistByName(string search)
        {
            //Essa função pesquisa uma playlist e retorna o primeiro resultado a pesquisa
            PlaylistSearchResult link = null;
            YoutubeClient youtube = new YoutubeClient();
            var a = youtube.Search.GetPlaylistsAsync(search);
            await foreach (var item in a)
            {
                link = item;
                break;
            }
            return new string[] {link.Url, link.Title};
            

            
        }

        static async public Task<List<string>> GetPlaylistVideosUrl(string link)
        {
            //Essa função recebe uma playlist, e cria uma lista com os links das musicas
            var youtube = new YoutubeClient();
            var playlist = youtube.Playlists.GetVideosAsync(link);
            List<string> musicasDir = new List<string>();
            await foreach(var videoResult in playlist)
            {
                musicasDir.Add(videoResult.Url);
            }
            return musicasDir;
        }

        static private string RemoveSpecialCaracters(string name)
        {
            //Essa função serve para remover os caracters proibidos na hora de criar o Path para o música
            string specialCaracters = "<>:/\\|?*\"";
            int cont = 0;
            foreach (char specialCaracter in specialCaracters)
            {
                do
                {
                    if (name.IndexOf(specialCaracter, cont) != -1)
                    {
                        name = name.Remove(name.IndexOf(specialCaracter), 1);
                        cont++;
                    }
                    else
                    {
                        cont = 0;
                        break;
                    }
                } while (true);

            }
            return name;

        }

        static private string CreateMusicDir()
        {
            //Cria o directorio caso ele não exista
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "Musicas");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }


    }


}

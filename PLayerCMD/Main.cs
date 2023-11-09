
using AngleSharp.Text;
using Player;
using PLayerCMD;
using System.Diagnostics;

class Program
{
    static string dirProgram = Directory.GetCurrentDirectory();
    static string playlistsDir = Path.Combine(dirProgram, "playlists");
    static string playlistLinksFile = Path.Combine(playlistsDir, "playlistsLinks.txt");
    static string playlistNamesFile = Path.Combine(playlistsDir, "playlistsNames.txt");

    static void Main(string[] args)
    { 

        Console.CursorVisible = false;
        Console.SetWindowSize(122, 30);
        Console.Title = "CMD-Player";
        Options.WriteOptionIni.CreateIniFile();
        if (!Directory.Exists(playlistsDir))
        {
            Directory.CreateDirectory(playlistsDir);
        }
        if (!File.Exists(playlistLinksFile))
        {
            File.CreateText(playlistLinksFile).Dispose();
        }
        if (!File.Exists(playlistNamesFile))
        {
            File.CreateText(playlistNamesFile).Dispose();
        }
        MainMenu();
    }

    static void PlaylistAddMenu()
    {
        string text = Input("Nome da Playlist:",PlaylistsMainMenu);
        Task<string[]> playlist = YoutubeSearch.SearchPlaylistByName(text);
        while (!playlist.IsCompleted);
        string status = WritePlaylist(playlist.Result);
        Console.Clear();
        AsciiGenerator statusAscii = new AsciiGenerator(status);
        statusAscii.Draw(new int[] { 0, 10 });
        Console.ReadKey();
        Console.Clear();
    }

    static void PlaylistsMainMenu()
    {
        AsciiMenu menu = new AsciiMenu("Playlists","Nova Playlist","Remover Playlist");
        menu.ButtonsFunctions(PlaylistsMenu, PlaylistAddMenu, PlaylistRemoveMenu);
        menu.Update(MainMenu);
    }

    static void PlaySingleMusic()
    {
        AsciiGenerator loadingText = new AsciiGenerator("Loading");
        PlayerInterface player;
        string userInput = Input("Nome da Musica :",MainMenu);
        Console.Clear();
        var directory = YoutubeSearch.DownloadAsyncMusicByName(userInput);
        loadingText.Draw(new int[] { 30, 10 });
        while (!directory.IsCompleted);
        player = new PlayerInterface(directory.Result);
        player.Play();
    }

    static void RemovePlaylist(string filePath, string NameToRemove)
    {
        List<string> FileBuffer = new List<string>();
        using (StreamReader file = new StreamReader(filePath))
        {
            string row;
            while((row = file.ReadLine()) != null)
            {
                if(row != NameToRemove)
                {
                    FileBuffer.Add(row);
                }
            }
        }
        File.WriteAllText(filePath, string.Empty);
        using (StreamWriter file = new StreamWriter(filePath,true))
        {
            foreach(string row in FileBuffer)
            {
                file.WriteLine(row);
            }
        }
    }

    static void Play_Playlist(string link)
    {
        Console.Clear();
        AsciiGenerator loadingText = new AsciiGenerator("Loading...");
        loadingText.Draw(new int[] {30,10});
        int cont = 0;
        bool mainMenu = false;
        PlayerInterface player = new PlayerInterface();
        Task<string> directory;
        var urls = YoutubeSearch.GetPlaylistVideosUrl(link);
        while (true)
        {
            if (cont > urls.Result.Count || mainMenu)
                break;


            directory = YoutubeSearch.DownloadAsyncMusicByUrl(urls.Result[cont]);
            while (!directory.IsCompleted) ;
            player.UpdateMusic(directory.Result);
            player.Play();
            mainMenu = player.PlaylistExit;
            cont = player.Playlist_Controller + cont < 0 ? 0 : player.Playlist_Controller + cont;
        }
        MainMenu();
    }

    static void MainMenu()
    {
        Console.Clear();
        AsciiMenu mainMenu = new AsciiMenu("Reproduzir Uma Música", "Reproduzir Playlist", "Configs");
        mainMenu.ButtonsFunctions(PlaySingleMusic, PlaylistsMainMenu, SettingsScreen);
        mainMenu.Update();


    }

    static void toggleCheckbox()
    {
        bool state = Options.WriteOptionIni.Read("AutoRemoveMusic").ToBoolean();
        if (state == false)
        {
            state = true;
        }
        else
        {

            state = false;
        }
        Options.WriteOptionIni.Write("AutoRemoveMusic", state);
        SettingsScreen();
    }

    static void SettingsScreen()
    {
        bool autoRemoveMusic = Options.WriteOptionIni.Read("AutoRemoveMusic").ToBoolean();
        char autoRemoveMusicChecker = autoRemoveMusic ? 'X' : ' ';
        AsciiMenu menu = new AsciiMenu($"Reproduzir e Excluir | {autoRemoveMusicChecker} |");
        menu.ButtonsFunctions(toggleCheckbox);
        menu.Update(MainMenu);

    }

    static void PlaylistRemoveMenu()
    {
        string[] playlistLinks = ReadPlaylist(playlistLinksFile).ToArray();
        string[] playlistNames = ReadPlaylist(playlistNamesFile).ToArray();

        if (playlistNames.Length != 0)
        {
            int key;
            DinamicAsciiMenu Menu = new DinamicAsciiMenu(playlistNames);
            while (true)
            {
                Menu.Update(PlaylistsMainMenu);
                key = Menu.GetButtonPressed();
                if (key != -1)
                {
                    break;
                }
            }
            RemovePlaylist(playlistLinksFile, playlistLinks[key]);
            RemovePlaylist(playlistNamesFile, playlistNames[key]);
            Console.Clear();
        }
        else
        {
            Console.Clear();
            AsciiGenerator text = new AsciiGenerator("Vazio");
            text.Draw(new int[] { 25, 10 });
            Console.ReadKey();
            Console.Clear() ;
        }
    }

    static void PlaylistsMenu()
    {

        string[] playlistLinks = ReadPlaylist(playlistLinksFile).ToArray();
        string[] playlistNames = ReadPlaylist(playlistNamesFile).ToArray();

        if(playlistNames.Length != 0)
        {
            int key;
            DinamicAsciiMenu Menu = new DinamicAsciiMenu(playlistNames);
            while (true)
            {
                Menu.Update(PlaylistsMainMenu);
                key = Menu.GetButtonPressed();
                if(key != -1)
                    break;
                
            }
            Play_Playlist(playlistLinks[key]);
        }
        else
        {
            AsciiGenerator text = new AsciiGenerator("esta Vazio");
            text.Draw(new int[] {25,10});
            Console.ReadKey();
            Console.Clear();
        }
    }

    static private bool Checker(string playlist)
    {
        using (StreamReader arqTxt = new StreamReader(playlistLinksFile))
        {
            string row;
            while ((row = arqTxt.ReadLine()) != null)
            {
                if (playlist == row)
                {
                    return false;
                }
            }
        }
        return true;
    }

    static string WritePlaylist(string[] playlist)
    {
        if (Checker(playlist[0]))
        {

            using (StreamWriter arqTxt = new StreamWriter(playlistLinksFile, true))
            {
                arqTxt.WriteLine(playlist[0]);
            }
            using (StreamWriter arqTxt = new StreamWriter(playlistNamesFile, true))
            {
                arqTxt.WriteLine(playlist[1]);
            }
            return "Sucesso";

        }
        else
            return "Ja existe essa playlist";
        
    }

    static List<string> ReadPlaylist(string filePath)
    {
        List<string> results = new List<string>();
        using(StreamReader arqTxt = new StreamReader(filePath))
        {
            string row;
            while((row = arqTxt.ReadLine()) != null)
            {
                results.Add(row);
            }
            
            
        }
        return results;
    }

    static string Input(string infoText, Action quitFunc)
    {
        string text = "";
        ConsoleKeyInfo key;
        AsciiGenerator infoTextAscii = new AsciiGenerator(infoText);
        AsciiGenerator userTextAscii = new AsciiGenerator("");
        while (true)
        {
            infoTextAscii.Draw(new int[] { 0, 0 });
            key = Console.ReadKey();
            if (key.KeyChar.IsLetter() || key.KeyChar.IsDigit())
            {
                text += key.KeyChar;
                userTextAscii.UpdateText(text.ToLower()+" ");
            }
            else if(key.Key == ConsoleKey.Spacebar)
            {
                text += "  ";
            }
            else if(key.Key == ConsoleKey.Backspace && text.Length != 0)
            {
                text = text.Remove(text.Length-1);
                userTextAscii.UpdateText(text.ToLower() + " ") ;
                Console.Clear();
            }
            else if(key.Key == ConsoleKey.Enter && text.Trim() != "")
            {
                Console.WriteLine(text.Length);
                return text;
            }
            else if (key.Key == ConsoleKey.LeftArrow)
            {
                Console.Clear();
                quitFunc();
            }
            userTextAscii.Draw(new int[] {0,8});
            

        }


    }

}
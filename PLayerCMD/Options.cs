


using System.Runtime.InteropServices;
using System.Text;

namespace Player
{
    internal class Options
    {
        public class WriteOptionIni
        {
            static string path = Path.Combine(Directory.GetCurrentDirectory(), "Configs");
            static string filepath = Path.Combine(path,"configs.ini");
            static string section = "options";
            [DllImport("kernel32")]
            private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder returnValue, int size, string filePath);


            public static void Write(string key, bool value)
            {
                WritePrivateProfileString(section, key, value.ToString(), filepath);
            }

            public static string Read(string key,string defaultvalue = "")
            {
                CreateIniFile();
                try
                {
                    StringBuilder stringBuilder = new StringBuilder(255);
                    GetPrivateProfileString(section, key, defaultvalue, stringBuilder, 255, filepath);
                    return stringBuilder.ToString();
                }
                catch
                {
                    Console.WriteLine("Chave invalida");
                    return false.ToString();
                }
                
            }

            public static void CreateIniFile()
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!File.Exists(filepath))
                {
                    File.Create(filepath);
                    Write("AutoRemoveMusic", false);
                }

            }
        }

        public class CreateTxt
        {

        }
        readonly int consoleMaxWidth = Console.WindowWidth;
        readonly int consoleMaxHeight = Console.WindowHeight;

       
    }

    
}

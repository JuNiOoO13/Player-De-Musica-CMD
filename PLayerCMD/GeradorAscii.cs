using Figgle;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace Player
{
    public class AsciiGenerator
    {
        private string[] asciiArt;
        private string text;
        private bool adjustOnScreen;
        public AsciiGenerator(string text,bool adjustOnScreen = false)
        {
            this.text = text;
            this.adjustOnScreen = adjustOnScreen;
            this.asciiArt = FiggleFonts.Big.Render(text).Split("\n");
            if (adjustOnScreen)
            {
                ResizeOnScreen();
            }
        }

        public void Draw(int[] pos)
        {
            foreach (string word in asciiArt)
            {
                Console.SetCursorPosition(pos[0], pos[1]);
                Console.WriteLine(word);
                pos[1]++;
            }
            Console.WriteLine();
        }

        public void UpdateText(string newText)
        {
            this.asciiArt = FiggleFonts.Big.Render(newText).Split("\n");
            if (adjustOnScreen)
            {
                ResizeOnScreen();
            }
        }

        public void ResetToOriginalText()
        {
            this.asciiArt = FiggleFonts.Big.Render(this.text).Split("\n");
            if (adjustOnScreen)
            {
                ResizeOnScreen();
            }
        }
        public string GetOriginalText()
        {
            return this.text;
        }

        private void ResizeOnScreen()
        {
            
            int maxWidth = Console.WindowWidth;
            for(int i = 0; i < this.asciiArt.Length;i++)
            {
                if (this.asciiArt[i].Length > maxWidth)
                {
                    this.asciiArt[i]= this.asciiArt[i].Substring(0, maxWidth);
                }
            }

        }
    }
}

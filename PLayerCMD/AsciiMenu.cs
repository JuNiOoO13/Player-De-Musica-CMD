

using System.Security.AccessControl;

namespace Player
{
    public class AsciiMenu
    {
        private List<AsciiGenerator> asciiArts = new List<AsciiGenerator>();
        private int pointerPos = 0;
        private bool enterclick = false;
        private Dictionary<ConsoleKey, Action> keysCommands;
        private Action[] buttonFunctions;
        
        public AsciiMenu(params string[] texts)
        {
            keysCommands = new Dictionary<ConsoleKey, Action>()
            {
                {ConsoleKey.UpArrow,() => {this.pointerPos = this.pointerPos - 1 < 0 ? 0 : this.pointerPos-1;}},
                {ConsoleKey.DownArrow,() => {this.pointerPos = this.pointerPos + 1 > this.asciiArts.Count-1? this.pointerPos : this.pointerPos + 1;}},
                {ConsoleKey.Enter,() => 
                {
                    Console.Clear();
                    this.buttonFunctions[this.pointerPos]();
                }
                }
            };
            foreach (string text in texts)
            {
                this.asciiArts.Add(new AsciiGenerator(text));
            }
            this.asciiArts[0].UpdateText("->" + this.asciiArts[0].GetOriginalText());
        }

        public void ButtonsFunctions(params Action[] buttonFunctions)
        {
            this.buttonFunctions = buttonFunctions;
        }

        private void Draw()
        {
            Console.Clear();
            int posY = 0;
            for (int i = 0; i < asciiArts.Count; i++)
            {
                this.asciiArts[i].Draw(new int[] { 0, posY });
                posY += 8;
            }

        }

        private void Controllers(Action exitFunc)
        {
            ConsoleKeyInfo key = Console.ReadKey();
            if (this.keysCommands.ContainsKey(key.Key))
            {
                this.keysCommands[key.Key]();
            }
            else if (key.Key == ConsoleKey.LeftArrow && exitFunc != null)
            {
                exitFunc();
            }
        }

        private void UpdatePos()
        {


            for (int i = 0; i < this.asciiArts.Count; i++)
            {
                if (i == this.pointerPos)
                {
                    this.asciiArts[i].UpdateText("->" + this.asciiArts[i].GetOriginalText());
                }
                else
                {
                    this.asciiArts[i].ResetToOriginalText();
                }

            }

        }

        public void ChangeValue(int index, string newText)
        {
            this.asciiArts[index] = new AsciiGenerator(newText);
            this.asciiArts[index].UpdateText("->" + this.asciiArts[index].GetOriginalText());
        }

        public string GetOriginalValue(int index)
        {
            return this.asciiArts[index].GetOriginalText();
        }

        public void Update(Action? exitFunc = null)
        {
            while (true)
            {
                this.Draw();
                this.Controllers(exitFunc);
                this.UpdatePos();
            }
            

        }
    }

    public class DinamicAsciiMenu
    {
        AsciiGenerator[,] pages;
        int currentPage;
        int pointerPos;
        bool enterClick;
        int currentRowLenght;
        Dictionary<ConsoleKey, Action> commands;

        public DinamicAsciiMenu(params string[] texts)
        {
            this.pointerPos = 0;
            this.currentPage = 0;
            this.enterClick = false;
            this.commands = new Dictionary<ConsoleKey, Action>()
            {
                {ConsoleKey.DownArrow, () =>
                {
                    if(this.pointerPos < this.currentRowLenght-1)
                    {
                        this.pointerPos++;
                    }
                }
                },
                { ConsoleKey.UpArrow, () =>
                {
                    if(this.pointerPos > 0)
                    {
                        this.pointerPos--;
                    }
                }
                },
                {ConsoleKey.PageUp, () =>
                {
                    if(this.currentPage < this.pages.GetLength(0)-1)
                    {
                        RemovePointer();
                        this.pointerPos = 0;
                        this.currentPage++;
                    }
                }
                },
                {ConsoleKey.PageDown,() =>
                {
                    if(this.currentPage > 0)
                    {
                        RemovePointer();
                        this.pointerPos = 0;
                        this.currentPage--;
                    }
                }
                },
                {ConsoleKey.Enter, () => this.enterClick = true }
            };

            int quantPages = texts.Length / 3 + (texts.Length % 3 > 0 ? 1 : 0);
            this.pages = new AsciiGenerator[quantPages, 3];

            string[] opcs = new string[3];
            int count = 0;
            for (int i = 0; i < quantPages; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (count <= texts.Length - 1)
                    {
                        pages[i, j] = new AsciiGenerator(texts[count], true);
                    }
                    else
                    {
                        break;
                    }
                    count++;
                }
            }
            this.pages[0, 0].UpdateText("=>" + this.pages[0, 0].GetOriginalText());

        }

        private void UpdatePointerPos()
        {
            for (int i = 0; i < this.currentRowLenght; i++)
            {
                if (this.pointerPos == i)
                {
                    this.pages[this.currentPage, this.pointerPos].UpdateText("=>" + this.pages[this.currentPage, this.pointerPos].GetOriginalText());
                }
                else
                {
                    if(this.pages[this.currentPage, i] != null)
                    {
                        this.pages[this.currentPage, i].ResetToOriginalText();
                    }
                    
                }
            }
        }

        private void RemovePointer()
        {
            for (int i = 0; i < this.currentRowLenght; i++)
            {

                if (this.pages[this.currentPage, i] != null)
                {
                    this.pages[this.currentPage, i].ResetToOriginalText();
                }

            }
        }

        private void Draw()
        {
            Console.Clear();
            Console.SetCursorPosition(38, 0);
            Console.Write($"Pagina : {this.currentPage+1} de {this.pages.GetLength(0)} [Trocar Página :PgUp+ PgDown-] ");
            int DrawPosY = 2;
            for (int i = 0; i < this.currentRowLenght; i++)
            {
                if(this.pages[this.currentPage, i] != null)
                {
                    this.pages[this.currentPage, i].Draw(new int[] {0 , DrawPosY });
                    DrawPosY += 8;
                }
                
            }
        }

        private void CheckRowLenght()
        {
            this.currentRowLenght = 0;
            for (int i = 0; i < 3; i++)
            {
                if (this.pages[this.currentPage, i] != null)
                {
                    this.currentRowLenght++;
                }
            }
            
        }

        private void Controllers(Action quitFunc)
        {
            
            ConsoleKey key = Console.ReadKey().Key;
            if (this.commands.ContainsKey(key))
            {
                this.commands[key]();
            }
            if(key == ConsoleKey.LeftArrow)
            {
                Console.Clear();
                quitFunc();
                
            }

        }

        public int GetButtonPressed()
        {
            if (this.enterClick)
            {
                return (this.currentPage) * 3 + (this.pointerPos);
            }
            else
            {
                return -1;
            }
        }

        public void Update(params Action[] Funcs)
        {
            CheckRowLenght();
            Draw();
            Controllers(Funcs[Funcs.Length - 1]);
            UpdatePointerPos();
            
            
            
        }
    }
} 
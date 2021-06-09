using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Integrated_250ML_Manager
{
    class Program
    {
        static void Main(string[] args)
        {
            Cue[] cues = null;
            while(true)
            {
                UserInterface.MainMenu(ref cues);
            }
        }
    }

    static class UserInterface
    {
        public static void MainMenu(ref Cue[] cues)
        {
            string[] mainMenuOptions = new string[] { "load cue file from desk", "load cue file from tsv table", "export loaded cues to desk", "fast export tsv to desk", "preview cue details"};

            string loadedCuesInformation = "no cue file loaded";
            if (cues != null)
            {
                loadedCuesInformation = cues.Count() + " cues loaded";
                for (int i = 0; i < 10 && i < cues.Count(); i++)
                {
                    loadedCuesInformation += "\n" + cues[i].DeskNumberString();
                    loadedCuesInformation += "                            ".Substring(0, 10 - cues[i].DeskNumberString().Length);
                    loadedCuesInformation += cues[i].name;
                }
            }

            int selectionIndex = HighlightMenuSelect("select module", mainMenuOptions, loadedCuesInformation);
            switch (selectionIndex)
            {
                case 0:
                    // load cue file from desk
                    cues = LoadCueFileFromDesk();
                    break;
                case 1:
                    // load cue file from tsv table
                    cues = LoadCueFileFromTsvTable();
                    break;
                case 2:
                    // export loaded cues to desk
                    ExportCueFileToDesk(ref cues);
                    break;
                case 3:
                    // fast export tsv to desk
                    FastExportTsvToDesk();
                    break;
                case 4:
                    CueViewer(ref cues);
                    break;
            }
        }

        public static void FastExportTsvToDesk()
        {
            Console.Clear();
            Console.WriteLine("Enter location of tsv file");
            string directory = Console.ReadLine().Replace("\"", " ");
            Cue[] cues = CueIO.TSVTableIngest(directory);
            Console.WriteLine("cues imported");
            CueIO.CueListExport(cues);
            Console.WriteLine("cues exported to same directory as this .exe");
        }

        public static void ExportCueFileToDesk(ref Cue[] cues)
        {
            CueIO.CueListExport(cues);
        }

        public static Cue[] LoadCueFileFromDesk()
        {
            Console.WriteLine("enter location of Cue file");
            string directory = Console.ReadLine().Replace("\"", " ");
            return CueIO.CueListIngest(directory);
        }

        public static Cue[] LoadCueFileFromTsvTable()
        {
            Console.WriteLine("enter location of Cue file");
            string directory = Console.ReadLine().Replace("\"", " ");
            return CueIO.TSVTableIngest(directory);
        }

        public static int HighlightMenuSelect(string header, string[] options, string footer)
        {
            Console.Clear();
            int selectIndex = 0;
            while(true)
            {
                // printing options
                Console.Clear();
                Console.WriteLine(header + "\n");
                for (int i = 0; i < options.Count(); i++)
                {
                    if (i == selectIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.WriteLine(options[i]);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.WriteLine(options[i]);
                    }
                }
                Console.WriteLine("\n" + footer);

                // getting user selection
                ConsoleKey key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if(selectIndex > 0)
                            selectIndex--;
                        break;
                    case ConsoleKey.RightArrow:
                        if (selectIndex > 0)
                            selectIndex--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (selectIndex < options.Count() - 1)
                            selectIndex++;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (selectIndex < options.Count() - 1)
                            selectIndex++;
                        break;
                    case ConsoleKey.Enter:
                        return selectIndex;
                        // break; does not need to be there apparently
                }
            }
        }

        public static void CueViewer(ref Cue[] cues)
        {
            if(cues == null)
            {
                E.rror("no cue file loaded");
                Thread.Sleep(500);
                return;
            }

            Console.Clear();
            Console.WindowHeight = 31;
            Console.WindowWidth = 24 * 5 + 1;
            // writing to screen

            // introduction
            int currentCueIndex = 0;
            int loadCueIndex = 1;
            Console.WriteLine("COMMANDS:\n" +
                "L = LOAD\n" +
                "ENTER / G / Space = GO\n" +
                "UP / RIGHT ARROWS TO LOAD NEXT CUE\n" +
                "DOWN / LEFT ARROWS TO LOAD PREVIOUS CUE\n" +
                "ESC TO RETURN TO MAIN MENU\n" +
                "/n" +
                "TYPE NUMBER AND PRESS GO TO LOAD DISTANT CUE\n\n" +
                "");

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("New value is less than the current cue");

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("new value is more than the current cue");

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("New value is the same as the current cue");


            Console.ReadKey();
            Console.Clear();

            bool showDeltaOnLoaded = false;

            // User interaction section

            string keyChars = "";
            PrintCue(cues, currentCueIndex, currentCueIndex, false);
            PrintCue(cues, loadCueIndex, currentCueIndex, showDeltaOnLoaded);
            while (true) // loop for displaying cues
            {
                // user input management 
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                if (consoleKeyInfo.Key == ConsoleKey.Backspace)
                {
                    keyChars = keyChars.Substring(0, keyChars.Length - 1);
                    Console.CursorLeft = 0;
                    Console.Write(keyChars);
                }
                else if(consoleKeyInfo.Key == ConsoleKey.Escape)
                {
                    return;
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Enter || consoleKeyInfo.Key == ConsoleKey.G || consoleKeyInfo.Key == ConsoleKey.Spacebar) // progression to next cue case
                {
                    int oldLoadCueIndex = loadCueIndex;
                    if (keyChars != "") // case for a standard next cue iteration
                    {
                        //Int32.TryParse(keyChars, out loadCueIndex);
                        loadCueIndex = GetCueIndexFromDeskNumber(cues, keyChars);
                        if (loadCueIndex >= cues.Count() || loadCueIndex < 0)
                        {
                            loadCueIndex = oldLoadCueIndex;
                        }
                        else
                        {
                            Console.Clear();
                            PrintCue(cues, currentCueIndex, currentCueIndex, false);
                            PrintCue(cues, loadCueIndex, currentCueIndex, showDeltaOnLoaded);
                            //loadCueIndex--;
                        }
                    }
                    else // case for a direct cue number entry (desk numbers, not Cue indices)
                    {
                        currentCueIndex = loadCueIndex;
                        Console.Clear();
                        PrintCue(cues, currentCueIndex, currentCueIndex, false);
                        loadCueIndex++;
                        if (loadCueIndex > cues.Count() - 1)
                        {
                            loadCueIndex--;
                        }
                        PrintCue(cues, loadCueIndex, currentCueIndex, true);
                    }
                    keyChars = "";
                }
                else if (consoleKeyInfo.Key == ConsoleKey.UpArrow || consoleKeyInfo.Key == ConsoleKey.RightArrow) // advance preview cue
                {
                    if (keyChars != "")
                    {
                        Int32.TryParse(keyChars, out int keyCharsValue);
                        keyChars = (keyCharsValue + 1) + "";
                    }
                    else
                    {
                        if (loadCueIndex < cues.Count() - 1)
                        {
                            loadCueIndex++;
                        }
                        Console.Clear();
                        PrintCue(cues, currentCueIndex, currentCueIndex, false);
                        PrintCue(cues, loadCueIndex, currentCueIndex, showDeltaOnLoaded);
                    }
                }
                else if (consoleKeyInfo.Key == ConsoleKey.DownArrow || consoleKeyInfo.Key == ConsoleKey.LeftArrow) // reverse preview cue
                {
                    if (keyChars != "")
                    {
                        Int32.TryParse(keyChars, out int keyCharsValue);
                        keyChars = (keyCharsValue - 1) + "";
                    }
                    else
                    {
                        if (loadCueIndex > 0)
                        {
                            loadCueIndex--;
                        }
                        Console.Clear();
                        PrintCue(cues, currentCueIndex, currentCueIndex, false);
                        PrintCue(cues, loadCueIndex, currentCueIndex, false);
                    }
                }
                else // case for entering a cue number rather than an immediate action
                {
                    keyChars += consoleKeyInfo.KeyChar;
                }
            }
        }

        static private int GetCueIndexFromDeskNumber (Cue[] cues, string deskNumber)
        {
            for(int i = 0; i < cues.Count(); i++)
            {
                if (deskNumber == cues[i].DeskNumberString())
                {
                    return i;
                }
                else if (cues[i].DeskNumberString().Split('.').Count() == 2)
                {
                    if (cues[i].DeskNumberString().Split('.')[1] == "0" && cues[i].DeskNumberString().Split('.')[0] == deskNumber)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        static void PrintCueORIGINAL(Cue[] cues, int cueIndex, int previousCueIndex, bool showDelta)
        {
            int channelsPerRow = (int)Math.Floor((Console.WindowWidth - 1) / 5f);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Cue:");
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            //Console.Write(cueIndex + 1);
            Console.Write(cues[cueIndex].DeskNumberString()/* + " " + cueIndex*/);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.CursorLeft = 10;
            Console.Write(cues[cueIndex].name);
            if (cueIndex == cues.Count() - 1)
            {
                Console.CursorLeft = 25;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("FINAL CUE OF SHOW");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Fade Up  : ");
            Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine(cues[cueIndex].fadeUp[0] + ":" + cues[cueIndex].fadeUp[1]);
            if (cues[cueIndex].FadeUpInt() == 0)
            {
                Console.WriteLine("SNAP");
            }
            else
            {
                Console.WriteLine(cues[cueIndex].FadeUpInt());
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Fade Down: ");
            Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine(cues[cueIndex].fadeDown[0] + ":" + cues[cueIndex].fadeDown[1]);
            if (cues[cueIndex].FadeDownInt() == 0)
            {
                Console.WriteLine("SNAP");
            }
            else
            {
                Console.WriteLine(cues[cueIndex].FadeDownInt());
            }

            for (int i = 0; i < 4; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                for (int channel = 0; channel < channelsPerRow; channel++)
                {
                    int pos = channel % channelsPerRow;

                    Console.Write((channel + (i * channelsPerRow) + 1).ToString().PadLeft(5));
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkYellow;

                for (int channel = 0; channel < channelsPerRow; channel++)
                {
                    int pos = channel % channelsPerRow;
                    int value = cues[cueIndex].channels[channel + (i * channelsPerRow)];
                    int previousValue = cues[previousCueIndex].channels[channel + (i * channelsPerRow)];

                    // colour formatting
                    if (previousValue - value > 0) // if new value is less than old value
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.DarkMagenta;
                    }
                    else if (previousValue - value < 0) // if new value is greater than old value
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.White;
                    }
                    else // if new value is equal to the old value
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                    }

                    // printing
                    if (value == 255)
                    {
                        Console.Write("FL".ToString().PadLeft(5));
                    }
                    else if (value == 0 && value == previousValue)
                    {
                        Console.Write("  ".ToString().PadLeft(5));
                    }
                    else
                    {
                        int dec = (int)Math.Round((100.0f * (float)value) / 255.0f);
                        Console.Write(dec.ToString().PadLeft(5));
                    }
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine();
                // printing of delta values
                if (showDelta)
                {
                    for (int channel = 0; channel < channelsPerRow; channel++)
                    {
                        int value = cues[cueIndex].channels[channel + (i * channelsPerRow)];
                        int previousValue = cues[previousCueIndex].channels[channel + (i * channelsPerRow)];

                        //Cue value = cues[cueIndex];
                        //Cue previousValue = cues[previousCueIndex];

                        if (value != previousValue)
                        {
                            //Console.Write(((value + previousValue) + "").PadLeft(5));
                            //Console.Write(((channel + (i * channelsPerRow)) + "").PadLeft(5));
                        }
                    }
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
            }

            //Console.WriteLine("\n");
        }

        static void PrintCue(Cue[] cues, int cueIndex, int previousCueIndex, bool showDelta)
        {
            int channelsPerRow = (int)Math.Floor((Console.WindowWidth - 1) / 5f);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Cue:");
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            //Console.Write(cueIndex + 1);
            Console.Write(cues[cueIndex].DeskNumberString()/* + " " + cueIndex*/);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.CursorLeft = 10;
            Console.Write(cues[cueIndex].name);
            if (cueIndex == cues.Count() - 1)
            {
                Console.CursorLeft = 25;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("FINAL CUE OF SHOW");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Fade Up  : ");
            Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine(cues[cueIndex].fadeUp[0] + ":" + cues[cueIndex].fadeUp[1]);
            if (cues[cueIndex].FadeUpInt() == 0)
            {
                Console.WriteLine("SNAP");
            }
            else
            {
                Console.WriteLine(cues[cueIndex].FadeUpInt());
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Fade Down: ");
            Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine(cues[cueIndex].fadeDown[0] + ":" + cues[cueIndex].fadeDown[1]);
            if (cues[cueIndex].FadeDownInt() == 0)
            {
                Console.WriteLine("SNAP");
            }
            else
            {
                Console.WriteLine(cues[cueIndex].FadeDownInt());
            }

            for (int i = 0; i < 4; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                for (int channel = 0; channel < channelsPerRow; channel++)
                {
                    int pos = channel % channelsPerRow;

                    Console.Write((channel + (i * channelsPerRow) + 1).ToString().PadLeft(5));
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkYellow;

                for (int channel = 0; channel < channelsPerRow; channel++)
                {
                    int pos = channel % channelsPerRow;
                    int value = cues[cueIndex].channels[channel + (i * channelsPerRow)];
                    int previousValue = cues[previousCueIndex].channels[channel + (i * channelsPerRow)];

                    // colour formatting
                    if (previousValue - value > 0) // if new value is less than old value
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.DarkMagenta;
                    }
                    else if (previousValue - value < 0) // if new value is greater than old value
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.White;
                    }
                    else // if new value is equal to the old value
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                    }

                    // printing
                    if (value == 255)
                    {
                        Console.Write("FL".ToString().PadLeft(5));
                    }
                    else if (value == 0 && value == previousValue)
                    {
                        Console.Write("  ".ToString().PadLeft(5));
                    }
                    else
                    {
                        int dec = (int)Math.Round((100.0f * (float)value) / 255.0f);
                        Console.Write(dec.ToString().PadLeft(5));
                    }
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine();
                // printing of delta values
                if (showDelta)
                {
                    for (int channel = 0; channel < channelsPerRow; channel++)
                    {
                        int value = cues[cueIndex].channels[channel + (i * channelsPerRow)];
                        int previousValue = cues[previousCueIndex].channels[channel + (i * channelsPerRow)];

                        //Cue value = cues[cueIndex];
                        //Cue previousValue = cues[previousCueIndex];

                        if (value != previousValue)
                        {
                            //Console.Write(((value + previousValue) + "").PadLeft(5));
                            //Console.Write(((channel + (i * channelsPerRow)) + "").PadLeft(5));
                        }
                    }
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
            }

            //Console.WriteLine("\n");
        }
    }

    static class E
    {
        public static void rror(string message)
        {
            ConsoleColor originalForegroundColour = Console.ForegroundColor;
            ConsoleColor originalBackgroundColour = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = originalForegroundColour;
            Console.BackgroundColor = originalBackgroundColour;
        }
    } // easy error message class

    static class CueIO
    {
        public static Cue[] CueListIngest(string sourceDirectory)
        {
            // read raw data from file
            byte[] rawData;
            using (BinaryReader br = new BinaryReader(File.Open(sourceDirectory, FileMode.Open)))
            {
                Console.WriteLine("source file opened");
                rawData = br.ReadBytes((int)br.BaseStream.Length);
                Console.WriteLine("source file read");
            }

            // checking file length is valid, getting cue count
            Cue[] cues;
            if((rawData.Count() - 10240) % 2048 == 0)
            {
                Console.WriteLine("source file is of valid length");
                cues = new Cue[(rawData.Count() - 10240) / 2048];
                Console.WriteLine("empty cue array created");
            }
            else // file length is not round value when divided by cue length
            {
                E.rror("source file is of invalid length");
                if((int)Math.Floor((rawData.Count() - 10240) / 2048f) > 0) // file is long enough to contain at least one valid cue
                {
                    Console.WriteLine("ignoring file length error, last cue may not exist");
                    cues = new Cue[(int)Math.Floor((rawData.Count() - 10240) / 2048f)];
                }
                else // file is too short to contain any cues
                {
                    E.rror("no cues could be read, canceling operation");
                    return null;
                }
            }
            Console.WriteLine(cues.Count() + " cues");

            // looping through cues, filling cue array with values
            Console.WriteLine("filling cue array");
            for(int cueIndex = 0; cueIndex < cues.Count(); cueIndex++) // loop once for each cue, fill out entire cue each loop
            {
                // creating instance of current cue
                cues[cueIndex] = new Cue();

                // desk number
                cues[cueIndex].deskNumber = new byte[] { rawData[cueIndex * 2], rawData[cueIndex * 2 + 1] };

                // cue name
                for(int i = 0; i < 10; i++)
                {
                    cues[cueIndex].name += (char)rawData[10240 + (2048 * cueIndex) + i];
                }

                // fade up
                cues[cueIndex].fadeUp = new byte[] { rawData[10240 + (2048 * cueIndex) + 12], rawData[10240 + 2048 * (cueIndex) + 13] };

                // fade down
                cues[cueIndex].fadeDown = new byte[] { rawData[10240 + (2048 * cueIndex) + 14], rawData[10240 + (2048 * cueIndex) + 15] };

                byte[] fadeDown = cues[cueIndex].fadeDown;

                // channel values
                for(int channelIndex = 0; channelIndex < 1024; channelIndex++)
                {
                    cues[cueIndex].channels[channelIndex] = rawData[10240 + (2048 * cueIndex) + 20 + channelIndex];
                }
            }
            Console.WriteLine("cue array complete");
            return cues;
        }

        public static void CueListExport(Cue[] cues)
        {
            byte[] exportData = new byte[10240 + 2048 * cues.Count()];

            Console.WriteLine("copying cue data into correct file format");

            // add data to exportData on a cue by cue loop basis
            for(int cueIndex = 0; cueIndex < cues.Count(); cueIndex++)
            {
                // desk number into header section
                exportData[cueIndex * 2] = cues[cueIndex].deskNumber[0];
                exportData[cueIndex * 2 + 1] = cues[cueIndex].deskNumber[1];

                // name
                for(int i = 0; i < 10; i++)
                {
                    exportData[10240 + (2048 * cueIndex) + i] = (byte)cues[cueIndex].name[i];
                }

                // fade up
                exportData[10240 + (2048 * cueIndex) + 12] = cues[cueIndex].fadeUp[0];
                exportData[10240 + (2048 * cueIndex) + 13] = cues[cueIndex].fadeUp[1];

                // fade down
                exportData[10240 + (2048 * cueIndex) + 14] = cues[cueIndex].fadeDown[0];
                exportData[10240 + (2048 * cueIndex) + 15] = cues[cueIndex].fadeDown[1];

                // channels
                for(int i = 0; i < 1024; i++)
                {
                    exportData[10240 + (2048 * cueIndex) + 20 + i] = cues[cueIndex].channels[i];
                }
            }

            Console.WriteLine("file formatting complete");
            Console.WriteLine("opening export file");

            using (BinaryWriter bw = new BinaryWriter(File.Create("Cue")))
            {
                Console.WriteLine("export file created");
                Console.WriteLine("writing data to file...");
                for (int i = 0; i < exportData.Count(); i++)
                {
                    bw.Write(exportData[i]);
                }
                Console.WriteLine("data written successfully");
            }
            Console.WriteLine("export complete");
        }

        public static Cue[] TSVTableIngest(string sourceDirectory)
        {
            string[,] TSVTable; // locations go X then Y, so [across, down]
            int cueCount = 0;
            int channelCount = 0;
            int[] cornerShiftXY = new int[2];

            // ingesting TSV data from file, converting into proper table format. Completes TSVTable variable
            {
                // reading and formatting TSV file
                string rawTSVTable;
                if(File.Exists(sourceDirectory))
                {
                    Console.WriteLine("opening TSV file");
                    using (StreamReader sr = new StreamReader(sourceDirectory))
                    {
                        rawTSVTable = sr.ReadToEnd();
                    }
                }
                else
                {
                    E.rror("file does not exist, cancelling");
                    Thread.Sleep(1000);
                    return null;
                }

                // converting raw string data into table
                string[] rowSplitTSVTable = rawTSVTable.Split('\n');
                for(int i = 0; i < rowSplitTSVTable.Count(); i++)
                {
                    rowSplitTSVTable[i] = rowSplitTSVTable[i].Split('\n')[0].Split('\r')[0];
                }

                // checking for the row with the most tabs
                int maxRowWidth = -1;
                for(int i = 0; i < rowSplitTSVTable.Count(); i++)
                {
                    if(rowSplitTSVTable[i].Split('\t').Count() > maxRowWidth)
                    {
                        maxRowWidth = rowSplitTSVTable[i].Split('\t').Count();
                    }
                }

                // converting rowSplitTSVTable into TSVTable
                TSVTable = new string[maxRowWidth, rowSplitTSVTable.Count()];
                for (int y = 0; y < rowSplitTSVTable.Count(); y++)
                {
                    string[] currentRowSplitAndColumnSplit = rowSplitTSVTable[y].Split('\t');
                    for (int x = 0; x < currentRowSplitAndColumnSplit.Count(); x++)
                    {
                        TSVTable[x, y] = currentRowSplitAndColumnSplit[x];
                    }
                }
            }

            // getting correct metadata values from file
            {
                // getting corner cell location
                bool cornerCellFound = false;
                for(int y = 0; y < TSVTable.GetLength(1) && !cornerCellFound; y++)
                {
                    for(int x = 0; x < TSVTable.GetLength(0) && !cornerCellFound; x++)
                    {
                        if(TSVTable[x,y].ToLower() == "CORNER_CELL_250ML".ToLower())
                        {
                            cornerShiftXY = new int[] { x, y };
                            cornerCellFound = true;
                        }
                    }
                }
                if(cornerCellFound)
                {
                    Console.WriteLine("corner cell found at " + cornerShiftXY[0] + ", " + cornerShiftXY[1]);
                }
                else
                {
                    E.rror("could not find corner cell, cancelling");
                    Thread.Sleep(1000);
                    return null;
                }

                // getting number of cues
                for(int x = cornerShiftXY[0] + 2; x < TSVTable.GetLength(0); x++)
                {
                    if(Int32.TryParse(TSVTable[x, cornerShiftXY[1] + 1], out int parsedInt))
                    {
                        cueCount++;
                    }
                    else
                    {
                        break;
                    }
                }
                Console.WriteLine(cueCount + " cues found");

                // getting number of channels
                for(int y = cornerShiftXY[1] + 2; y < TSVTable.GetLength(1); y++)
                {
                    if(Int32.TryParse(TSVTable[cornerShiftXY[0] + 1, y], out int parsedInt))
                    {
                        channelCount++;
                    }
                    else
                    {
                        break;
                    }
                }
                Console.WriteLine(channelCount + " channels found");
            }

            // copying data from table into cue array

            Console.WriteLine("copying data from table to cues");
            Cue[] cues = new Cue[cueCount];
            {
                for (int cueIndex = 0; cueIndex < cueCount; cueIndex++)
                {
                    // initializing cue[] index
                    cues[cueIndex] = new Cue();

                    // fade up 
                    if(Int32.TryParse(TSVTable[cornerShiftXY[0] + 2 + cueIndex, cornerShiftXY[1] + 2 + channelCount], out int fadeUp))
                    {
                        cues[cueIndex].fadeUp = Endian.IntToLittle(fadeUp * 10 * 2);
                    }
                    else
                    {
                        cues[cueIndex].fadeUp = new byte[] { 60, 0 };
                        //E.rror("fade down on cue " + cueIndex + " is blank / invalid, using 3s as default");
                    }

                    // fade down
                    if(Int32.TryParse(TSVTable[cornerShiftXY[0] + 2 + cueIndex, cornerShiftXY[1] + 3 + channelCount], out int fadeDown))
                    {
                        cues[cueIndex].fadeDown = Endian.IntToLittle(fadeDown * 10 * 2);
                    }
                    else
                    {
                        cues[cueIndex].fadeDown = new byte[] { 60, 0 };
                        //E.rror("fade down on cue " + cueIndex + " is blank / invalid, using 3s as default");
                    }

                    // cue name
                    cues[cueIndex].name = (TSVTable[cornerShiftXY[0] + 2 + cueIndex, cornerShiftXY[1] + 4 + channelCount] + "          ").Substring(0, 10);

                    // channel data
                    for(int channelRowIndex = 0; channelRowIndex < channelCount; channelRowIndex++)
                    {
                        bool channelIndexParseResult = int.TryParse(TSVTable[cornerShiftXY[0] + 1, cornerShiftXY[1] + 2 + channelRowIndex], out int channelIndex);
                        if(!channelIndexParseResult)
                        {
                            E.rror("failed to parse integer from DMX channel column. This error should never occur, it can only be produced by an internal error. cancelling");
                            Thread.Sleep(1000);
                            return null;
                        }
                        byte channelValue = 0;
                        if(TSVTable[cornerShiftXY[0] + 2 + cueIndex, cornerShiftXY[1] + 2 + channelRowIndex].ToLower() == "fl")
                        {
                            channelValue = 255;
                        }
                        else
                        {
                            bool channelValueParseResult = byte.TryParse(TSVTable[cornerShiftXY[0] + 2 + cueIndex, cornerShiftXY[1] + 2 + channelRowIndex], out channelValue);
                            if (!channelIndexParseResult)
                            {
                                E.rror("failed to parse integer from cue number column (actual luminaire luminosity value). This error should never occur, it can only be produced by an internal error. cancelling");
                                Thread.Sleep(1000);
                                return null;
                            }
                            channelValue = (byte)(channelValue * (255 / 100f));
                        }
                        
                        cues[cueIndex].channels[channelIndex] = channelValue;
                    }

                    // desk number
                    cues[cueIndex].deskNumber = Endian.IntToLittle((short)(Convert.ToDecimal(TSVTable[cornerShiftXY[0] + 2 + cueIndex, cornerShiftXY[1] + 1]) * 10));
                }
            }
            return cues;
        }
    }

    class Endian
    {
        public static byte[] IntToLittle(int input)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.GetBytes(input);
            }
            return (byte[])BitConverter.GetBytes(input).Reverse();
        }
        public static byte[] IntToBig(int input)
        {
            return (byte[])IntToLittle(input).Reverse();
        }
        public static byte[] BigToLittle(byte[] input)
        {
            return (byte[])input.Reverse();
        }
        public static byte[] LittleToBig(byte[] input)
        {
            return (byte[])input.Reverse();
        }
        public static int LittleToInt(byte[] input)
        {
            int output = 0;
            for (int i = 0; i < input.Count(); i++)
            {
                output += input[i] * (int)Math.Pow(i, 255);
            }
            return output;
        }
        public static int BigToInt(byte[] input)
        {
            int output = 0;
            for (int i = 0; i < input.Count(); i++)
            {
                output += input[i] * (int)Math.Pow(input.Count() - i - 1, 255);
            }
            return output;
        }
    }

    class Cue
    {
        public string name;
        public byte[] channels = new byte[1024];
        public byte[] deskNumber = new byte[2];     // keep little endian order
        public byte[] fadeUp = new byte[2];         // keep little endian order
        public byte[] fadeDown = new byte[2];       // keep little endian order

        // conversions from stored data to human readable forms

        public int FadeUpInt()
        {
            int output = 0;
            for (int i = 0; i < fadeUp.Count(); i++)
            {
                output += fadeUp[i] * (int)Math.Pow(255, i);
            }
            return (int)Math.Round(output / 20f);
        }

        public int FadeDownInt()
        {
            int output = 0;
            for (int i = 0; i < fadeDown.Count(); i++)
            {
                output += fadeDown[i] * (int)Math.Pow(255, i);
            }
            return (int)Math.Round(output / 20f);
        }

        public string DeskNumberString()
        {
            int output = 0;
            for (int i = 0; i < deskNumber.Count(); i++)
            {
                output += deskNumber[i] * (int)Math.Pow(255, i);
            }
            return (output / 10 + "." + output % 10);
        }
    }
}
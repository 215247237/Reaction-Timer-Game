using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SimpleReactionMachine
{
    class SimpleReactionMachine
    {
        const string TOP_LEFT_JOINT = "┌";
        const string TOP_RIGHT_JOINT = "┐";
        const string BOTTOM_LEFT_JOINT = "└";
        const string BOTTOM_RIGHT_JOINT = "┘";
        const string LEFT_JOINT = "├";
        const string RIGHT_JOINT = "┤";
        const char HORIZONTAL_LINE = '─';
        const string VERTICAL_LINE = "│";
        const string MENU_FORMAT = "{0}{1}{2}";
        const string PROMPT_FORMAT = "{0,-20}";

        static private IController? controller;

        static void Main(string[] args)
        {
            // Make a menu
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(MENU_FORMAT, TOP_LEFT_JOINT, new string(HORIZONTAL_LINE, 50), TOP_RIGHT_JOINT);
            Console.WriteLine(MENU_FORMAT, VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine(MENU_FORMAT, VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine(MENU_FORMAT, VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine(MENU_FORMAT, LEFT_JOINT, new string(HORIZONTAL_LINE, 50), RIGHT_JOINT);
            Console.WriteLine(MENU_FORMAT, VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine(MENU_FORMAT, VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine(MENU_FORMAT, VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine(MENU_FORMAT, VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine(MENU_FORMAT, VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine(MENU_FORMAT, BOTTOM_LEFT_JOINT, new string(HORIZONTAL_LINE, 50), BOTTOM_RIGHT_JOINT);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.SetCursorPosition(5, 6);
            Console.Write(PROMPT_FORMAT, "- For Insert Coin press SPACE");
            Console.SetCursorPosition(5, 7);
            Console.Write(PROMPT_FORMAT, "- For Go/Stop action press ENTER");
            Console.SetCursorPosition(5, 8);
            Console.Write(PROMPT_FORMAT, "- For Exit press ESC");

            // Create a time for Tick event
            Timer timer = new Timer(10);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;

            // Connect GUI with the Controller and vice versa
            controller = new EnhancedReactionController();
            IGui gui = new Gui();
            gui.Connect(controller);
            controller.Connect(gui, new RandomGenerator());

            //Reset the GUI
            gui.Init();
            // Start the timer
            timer.Enabled = true;

            // Run the menu
            bool quitePressed = false;
            while (!quitePressed)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        controller.GoStopPressed();
                        break;
                    case ConsoleKey.Spacebar:
                        controller.CoinInserted();
                        break;
                    case ConsoleKey.Escape:
                        quitePressed = true;
                        break;
                }
            }
        }

        // This event occurs every 10 msec
        private static void OnTimedEvent(Object? source, ElapsedEventArgs? e)
        {
            controller!.Tick();
        }

        // Internal implementation of Random Generator
        private sealed class RandomGenerator : IRandom
        {
            private readonly Random rnd = new Random(100);

            public int GetRandom(int from, int to)
            {
                return rnd.Next(to - from) + from;
            }
        }

        // Internal implementation of GUI
        private sealed class Gui : IGui
        {
            public void Connect(IController controller)
            {

            }

            public void Init()
            {
                SetDisplay("Start your game!");
            }

            public void SetDisplay(string s)
            {
                PrintUserInterface(s);
            }

            private static void PrintUserInterface(string s)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.SetCursorPosition(15, 2);
                Console.Write(PROMPT_FORMAT, s);
                Console.SetCursorPosition(0, 10);
            }
        }
    }
}
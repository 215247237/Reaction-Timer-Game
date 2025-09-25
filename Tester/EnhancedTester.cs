using System;

namespace SimpleReactionMachine
{
    class EnhancedTester
    {
        private static IController controller;
        private static IGui gui;
        private static string displayText;
        private static int randomNumber;
        private static int passed = 0;

        static void Main(string[] args)
        {
            RunTests();
            Console.WriteLine("\n=====================================\nSummary: {0} tests passed out of 72", passed);
        }

        private static void RunTests()
        {
            controller = new EnhancedReactionController();
            gui = new DummyGui();
            gui.Connect(controller);
            controller.Connect(gui, new RndGenerator());
            gui.Init();


            /* CONTROLLER TESTS
               - One behaviour test per line to focus on one issue at a time
               - Tests only focus on new implementations
               - NOTE: All states excluding TimeReactionState use: tickCount = ticks elapsed
                Only TimeReactionState uses: (tickCount - 1) = ticks elapsed, due to first tick setting timer to 0.00
            */

            // TEST 1: Successfully completing three rounds
            // Round 1
            randomNumber = 100;
            DoInsertCoin("1a", controller, "Press GO!");         // Coin inserted; transitions from IdleState to ReadyState
            DoGoStop("1b", controller, "Wait...");               // GO/STOP pressed; transitions from ReadyState to DelayState
            DoTicks("1c", controller, randomNumber, "0.00");     // Transitions from DelayState (lasts randomNumber ticks) to TimeReactionState after random delay; displays 0.00 as the first tick in TimeReactionState
            DoTicks("1d", controller, 31, "0.30");               // 31 ticks: tickCount = 30. Displays "0.30" (1 tick displays 0.00)
            DoGoStop("1e", controller, "0.30");                  // GO/STOP pressed in TimeReactionState, transitioning into DisplayResultState
            DoTicks("1f", controller, 299, "0.30");              // Still in DisplayResultState (lasts 300 ticks), 1 tick before transition to DelayState
            DoTicks("1g", controller, 1, "Wait...");             // + 1 tick; transitions from DisplayResultState to DelayState for round 2

            // Round 2
            DoTicks("1h", controller, randomNumber, "0.00");     // Transitions from DelayState back to TimeReactionState
            DoTicks("1i", controller, 55, "0.54");               // 55 ticks: tickCount = 54. Displays "0.54" (1 tick displays 0.00)
            DoGoStop("1j", controller, "0.54");                  // GO/STOP pressed in TimeReactionState, transitioning back into DisplayResultState
            DoTicks("1k", controller, 300, "Wait...");           // DisplayResultState completes 300 tick duration; final tick transitions back to DelayState for round 3 (final round)
            DoTicks("1l", controller, 99, "Wait...");            // Still in DelayState (lasts randomNumber ticks), 1 tick before transition to TimeReactionState

            // Round 3
            DoTicks("1m", controller, 1, "0.00");                // + 1 tick; transitions from DelayState back to TimeReactionState        
            DoTicks("1n", controller, 5, "0.04");                // 5 ticks: tickCount = 4. Displays "0.04", (1 tick displays 0.00)
            DoGoStop("1o", controller, "0.04");                  // GO/STOP pressed in TimeReactionState, transitioning back into DisplayResultState for the last time
            DoTicks("1p", controller, 299, "0.04");              // Still in DisplayResultState (lasts 300 ticks), 1 tick before transition to DisplayAverageState
            DoTicks("1q", controller, 1, "Average = 0.29");      // + 1 tick; transitions from DisplayResultState to DisplayAverageState
            DoTicks("1r", controller, 499, "Average = 0.29");    // Still in DisplayAverageState (lasts 500 ticks), 1 tick before transition to IdleState
            DoTicks("1s", controller, 1, "Insert coin");         // + 1 tick; game over. Transitions from DisplayAverageState to IdleState



            // TEST 2: GO/STOP not pressed in ReadyState before 10 second timeout (reset)
            randomNumber = 150;
            DoInsertCoin("2a", controller, "Press GO!");    // Coin inserted; transitions from IdleState to ReadyState
            DoTicks("2b", controller, 999, "Press GO!");    // Still in ReadyState (lasts 1000 ticks), 1 tick before 10 second timeout and transition to IdleState
            DoTicks("2c", controller, 1, "Insert coin");    // + 1 tick; 10 second timeout reached and game is over. Transitions from ReadyState back to IdleState



            // TEST 3: Pressing GO/STOP early while in DisplayResultState (skip time display)
            // Pressed during Round 1 results; jumps to next round
            randomNumber = 200;
            DoInsertCoin("3a", controller, "Press GO!");                      // Coin inserted; transitions from IdleState to ReadyState
            DoGoStop("3b", controller, "Wait...");                            // GO/STOP pressed; transitions from ReadyState to DelayState
            DoTicks("3c", controller, randomNumber + 37, "0.36");             // Transitions from DelayState (lasts randomNumber ticks) to 37 ticks into TimeReactionState
            DoGoStop("3d", controller, "0.36");                               // GO/STOP pressed in TimeReactionState, transitioning into DisplayResultState
            DoTicks("3e", controller, 200, "0.36");                           // 200 ticks into 300 tick display duration in DisplayResultState
            DoGoStop("3f", controller, "Wait...");                            // GO/STOP pressed while in DisplayResultState. Transitions to DelayState to start next round

            // Pressed during Round 2 results; jumps to next round
            DoTicks("3g", controller, randomNumber + 46, "0.45");             // Transitions from DelayState (lasts randomNumber ticks) to 46 ticks into TimeReactionState
            DoGoStop("3h", controller, "0.45");                               // GO/STOP pressed in TimeReactionState, transitioning into DisplayResultState
            DoTicks("3i", controller, 56, "0.45");                            // 56 ticks into 300 tick display duration in DisplayResultState
            DoGoStop("3j", controller, "Wait...");                            // GO/STOP pressed while in DisplayResultState. Transitions to DelayState to start final round

            // Pressed during Round 3 results; jumps to average result
            DoTicks("3k", controller, randomNumber, "0.00");                  // Transitions from DelayState (lasts randomNumber ticks) to first tick into TimeReactionState
            DoTicks("3l", controller, 6, "0.05");                             // 6 ticks: tickCount = 5. Displays "0.05" (1 tick displays 0.00)
            DoGoStop("3m", controller, "0.05");                               // GO/STOP pressed in TimeReactionState, transitioning into DisplayResultState
            DoTicks("3n", controller, 100, "0.05");                           // 100 ticks into 300 tick DisplayResultState duration
            DoGoStop("3o", controller, "Average = 0.29");                     // GO/STOP pressed while in DisplayResultState. Transitions to DisplayAverageState to show average results

            // Pressed during average results; jumps to starting screen
            DoTicks("3p", controller, 350, "Average = 0.29");                 // Still in DisplayAverageState (lasts 500 ticks), 150 ticks before transition to IdleState
            DoGoStop("3q", controller, "Insert coin");                        // GO/STOP pressed while in DisplayAverageState. Transitions to IdleState



            // TEST 4: GO/STOP not pressed in TimeReactionState before 2 second timeout (2/3 rounds)
            // Successfully pressed during round 1
            randomNumber = 250;
            DoInsertCoin("4a", controller, "Press GO!");                        // Coin inserted; transitions from IdleState to ReadyState
            DoGoStop("4b", controller, "Wait...");                              // GO/STOP pressed; transitions from ReadyState to DelayState
            DoTicks("4c", controller, randomNumber + 151, "1.50");              // Transitions from DelayState (lasts randomNumber ticks) to 151 ticks into TimeReactionState
            DoGoStop("4d", controller, "1.50");                                 // GO/STOP pressed in TimeReactionState, transitioning into DisplayResultState

            // Not pressed during round 2
            DoTicks("4e", controller, 300 + (randomNumber - 1), "Wait...");     // 300 tick DisplayResultState + randomNumber ticks DelayState - 1: 1 tick before transition to TimeReactionState
            DoTicks("4f", controller, 1, "0.00");                               // + 1 tick; adding back the removed tick, transitioning from DelayState to TimeReactionState
            DoTicks("4g", controller, 200, "1.99");                             // 1 tick before 201 tick (2.00 on timer) TimeReactionState timeout
            DoTicks("4h", controller, 1, "2.00");                               // + 1 tick; transitions from TimeReactionState to DisplayResultState
            DoTicks("4i", controller, 299, "2.00");                             // Still in DisplayResultState (lasts 300 ticks), 1 tick before transition to DelayState
            DoTicks("4j", controller, 1, "Wait...");                            // + 1 tick; transitions from DisplayResultState to DelayState

            // Not pressed during round 3; average result includes timeout times
            DoTicks("4k", controller, randomNumber, "0.00");                    // Transitions from DelayState (lasts randomNumber ticks) to 1 tick into TimeReactionState
            DoTicks("4l", controller, 251, "2.00");                             // 251 ticks: tickCount = 250; last tick of TimeReactionState being tick 201 (2.00 on timer). Transitions 50 ticks into DisplayResultState (lasts 300 ticks)
            DoTicks("4m", controller, 500, "Average = 1.83");                   // 500 ticks completes remaining 250 ticks in DisplayResultState, transitioning 250 ticks into DisplayAverageState (lasts 500 ticks)



            // TEST 5: GO/STOP not pressed in TimeReactionState before 2 second timeout (3/3 rounds)
            DoTicks("5a", controller, 250, "Insert coin");                              // Completes previous game's remaining 250 ticks, transitioning from DisplayAverageState to IdleState
            DoInsertCoin("5b", controller, "Press GO!");                                // Coin inserted; transitions from IdleState to ReadyState
            DoGoStop("5c", controller, "Wait...");                                      // GO/STOP pressed; transitions from ReadyState to DelayState
            DoTicks("5d", controller, randomNumber + 201 + 300, "Wait...");             // Transitions from DelayState (lasts randomNumber ticks), to TimeReactionState (times out at 201 ticks/2.00 on timer), to DisplayResultState (lasts 300 ticks) completing round 1, back to DelayState
            DoTicks("5e", controller, randomNumber + 201 + 300, "Wait...");             // Repeat process to timeout round 2
            DoTicks("5f", controller, randomNumber + 201 + 300, "Average = 2.00");      // Repeat process to timeout final round, transitioning from DisplayResultState to DisplayAverageState instead



            // TEST 6: Pressing GO/STOP early while in DelayState (reset for attempted cheating)
            // Game 1: Cheating in round 1
            randomNumber = 175;
            gui.Init();
            DoReset("6a", controller, "Insert coin");                                               // Reset game to IdleState
            DoInsertCoin("6b", controller, "Press GO!");                                            // Coin inserted; transitions from IdleState to ReadyState
            DoGoStop("6c", controller, "Wait...");                                                  // GO/STOP pressed; transitions from ReadyState to DelayState
            DoTicks("6d", controller, 15, "Wait...");                                               // 15 ticks into DelayState (lasts randomNumber ticks)
            DoGoStop("6e", controller, "Insert coin");                                              // GO/STOP pressed in DelayState, aborting game and transitioning back to IdleState

            // Game 2: Cheating in round 2
            DoInsertCoin("6f", controller, "Press GO!");                                            // Coin inserted; transitions from IdleState to ReadyState
            DoGoStop("6g", controller, "Wait...");                                                  // GO/STOP pressed; transitions from ReadyState to DelayState
            DoTicks("6h", controller, randomNumber + 201 + 300 + 50, "Wait...");                    // Round 1 completed (DelayState for randomNumber ticks, TimeReactionState for 201 ticks, DisplayResultState for 300 ticks, 51 ticks into DelayState)
            DoGoStop("6i", controller, "Insert coin");                                              // GO/STOP pressed in DelayState, aborting game and transitioning back to IdleState

            // Game 3: Cheating in round 3
            DoInsertCoin("6j", controller, "Press GO!");                                            // Coin inserted; transitions from IdleState to ReadyState
            DoGoStop("6k", controller, "Wait...");                                                  // GO/STOP pressed; transitions from ReadyState to DelayState
            DoTicks("6l", controller, randomNumber + 201 + 300, "Wait...");                         // Round 1 completed (DelayState for randomNumber ticks, TimeReactionState for 201 ticks, DisplayResultState for 300 ticks, first tick into DelayState)
            DoTicks("6m", controller, randomNumber + 201 + 300 + (randomNumber - 1), "Wait...");    // Repeat process to complete round 2. 1 tick before transition to TimeReactionState
            DoGoStop("6n", controller, "Insert coin");                                              // GO/STOP pressed in DelayState, aborting game and transitioning back to IdleState
        }

        private static void DoReset(string testID, IController controller, string msg)
        {
            try
            {
                controller.Init();
                GetMessage(testID, msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("test {0}: failed with exception {1})", testID, ex.Message);
            }
        }

        private static void DoGoStop(string testID, IController controller, string msg)
        {
            try
            {
                controller.GoStopPressed();
                GetMessage(testID, msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("test {0}: failed with exception {1})", testID, ex.Message);
            }
        }

        private static void DoInsertCoin(string testID, IController controller, string msg)
        {
            try
            {
                controller.CoinInserted();
                GetMessage(testID, msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("test {0}: failed with exception {1})", testID, ex.Message);
            }
        }

        private static void DoTicks(string testID, IController controller, int n, string msg)
        {
            try
            {
                for (int t = 0; t < n; t++) controller.Tick();
                GetMessage(testID, msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("test {0}: failed with exception {1})", testID, ex.Message);
            }
        }

        private static void GetMessage(string testID, string msg)
        {
            if (msg.ToLower() == displayText.ToLower())
            {
                Console.WriteLine("test {0}: passed successfully", testID);
                passed++;
            }
            else
            {
                Console.WriteLine("test {0}: failed with message ( expected {1} | received {2})", testID, msg, displayText);
            }
        }

        private class DummyGui : IGui
        {
            private IController controller;

            public void Connect(IController controller)
            {
                this.controller = controller;
            }

            public void Init()
            {
                displayText = "?reset?";
            }

            public void SetDisplay(string msg)
            {
                displayText = msg;
            }
        }

        private class RndGenerator : IRandom
        {
            public int GetRandom(int from, int to)
            {
                return randomNumber;
            }
        }
    }
}

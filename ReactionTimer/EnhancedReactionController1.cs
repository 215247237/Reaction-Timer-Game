using System.Runtime.ConstrainedExecution;

namespace SimpleReactionMachine
{
    public class EnhancedReactionController : IController
    {
        private const int MinDelayTicks = 100;
        private const int MaxDelayTicks = 250;
        private const int MaxReactionTicks = 200;
        private const int MaxDisplayReactionTicks = 300;    // Tick duration for DisplayResultState after each round
        private const int MaxDisplayAverageTicks = 500;     // Tick duration for DisplayAverageState after final round
        private const double TickInterval = 0.01;
        private const int MaxReadyTicks = 1000;             // Tick duration before timeout in ReadyState
        private const int MaxRounds = 3;                    // Max number of rounds in a single game

        private int roundsCompleted = 0;                    // Tracks number of rounds completed
        private double totalReactionTime = 0;               // Tracks total reaction time of completed rounds
        private int readyTickCount = 0;                     // Tracks completed ticks in ReadyState
        private IGui? gui;
        private IRandom? rng;
        private int tickCount = 0;
        private int delayDuration = 0;
        private int displayReactionTimeDuration = 0;

        private GameState currentState = new IdleState();
        public void Tick() => currentState.Tick(this);
        public void GoStopPressed() => currentState.GoStopPressed(this);
        public void CoinInserted() => currentState.CoinInserted(this);

        private static class StateHelper // Moved from end of program
        {
            public static void TransitionState(EnhancedReactionController ctrl, ref int counter, GameState newState, string message)
            {
                counter = 0;
                ctrl.currentState = newState;
                ctrl.gui.SetDisplay(message);
            }
        }

        public void Connect(IGui gui, IRandom rng)
        {
            this.gui = gui;
            this.rng = rng;
        }

        public void Init() // Updated to perform ReadyState timeout
        {
            readyTickCount = 0; // Resets ReadyState tick counter
            StateHelper.TransitionState(this, ref tickCount, new IdleState(), "Insert coin");
        }

        private void ProceedAfterRoundOver() // Determines which state to transition to based on number of rounds completed
        {
            if (roundsCompleted < MaxRounds) // If three rounds have not been completed
            {
                delayDuration = rng.GetRandom(MinDelayTicks, MaxDelayTicks);                    // Get random delay time for DelayState duration
                StateHelper.TransitionState(this, ref tickCount, new DelayState(), "Wait...");  // Transition to DelayState to start next round
            }
            else // If third and final round has been completed
            {
                string averageTime = (totalReactionTime / MaxRounds).ToString("0.00");  // Calculate average reaction time
                StateHelper.TransitionState(this, ref displayReactionTimeDuration,
                new DisplayAverageState(), "Average = " + averageTime);                 // Transition to DisplayAverageState to display average time
            }
        }

        private abstract class GameState
        {
            public virtual void Tick(EnhancedReactionController ctrl)
            {

            }

            public virtual void GoStopPressed(EnhancedReactionController ctrl)
            {

            }

            public virtual void CoinInserted(EnhancedReactionController ctrl)
            {

            }
        }

        private sealed class IdleState : GameState
        {
            public override void CoinInserted(EnhancedReactionController ctrl)
            {
                ctrl.currentState = new ReadyState();
                ctrl.gui.SetDisplay("Press GO!");
            }
        }

        private sealed class ReadyState : GameState // Tick added and GoStopPressed updated
        {
            public override void Tick(EnhancedReactionController ctrl) // Added to implement 10 second timeout
            {
                ctrl.readyTickCount++;

                if (ctrl.readyTickCount >= MaxReadyTicks) // If 10 seconds pass without GO/STOP being pressed
                {
                    ctrl.Init();    // Reset game to IdleState
                }
            }

            public override void GoStopPressed(EnhancedReactionController ctrl) // Updated to include failsafe
            {
                ctrl.readyTickCount = 0;    // Reset counter in case Init method not called
                ctrl.delayDuration = ctrl.rng.GetRandom(MinDelayTicks, MaxDelayTicks);
                StateHelper.TransitionState(ctrl, ref ctrl.tickCount, new DelayState(), "Wait...");
            }
        }

        private sealed class DelayState : GameState // GoStopPressed updated
        {
            public override void Tick(EnhancedReactionController ctrl)
            {
                ctrl.tickCount++;

                if (ctrl.tickCount >= ctrl.delayDuration)
                {
                    StateHelper.TransitionState(ctrl, ref ctrl.tickCount, new TimeReactionState(), "0.00");
                }
            }

            public override void GoStopPressed(EnhancedReactionController ctrl) // Updated to implement full reset
            {
                ctrl.roundsCompleted = 0;               // Reset all counters and game progress
                ctrl.totalReactionTime = 0;
                ctrl.displayReactionTimeDuration = 0;
                ctrl.Init();                            // Reset game to IdleState
            }
        }

        private sealed class TimeReactionState : GameState // Tick and GoStopPressed updated
        {
            public override void Tick(EnhancedReactionController ctrl) // Updated to play multiple rounds
            {
                if (ctrl.tickCount >= MaxReactionTicks) // If GO/STOP is not pressed before 2 second timeout is reached
                {
                    double finalTime = MaxReactionTicks * TickInterval;     // Use timeout value as reaction time
                    ctrl.totalReactionTime += finalTime;                    // Add the reaction time to total reaction time
                    ctrl.roundsCompleted++;                                 // Increment round completion count

                    string result = finalTime.ToString("0.00");
                    StateHelper.TransitionState(ctrl, ref
                        ctrl.displayReactionTimeDuration, new DisplayResultState(), result);    // Transition to DisplayResultState to display round's reaction time                                
                    return;                                                                     // Exit method
                }

                double liveTime = ctrl.tickCount * TickInterval;        // Calculate live timer value using tickCount
                ctrl.gui.SetDisplay(liveTime.ToString("0.00"));         // Display live timer
                ctrl.tickCount++;                                       // tickCount incrementation moved from start of method so first tick reads 0.00
            }

            public override void GoStopPressed(EnhancedReactionController ctrl) // Updated to play multiple rounds
            {
                double finalTime = (ctrl.tickCount - 1) * TickInterval;     // GO/STOP is pressed before 2 second timeout is reached, calculate reaction time using tickCount - 1 (NOTE: first tick is 0.00)
                ctrl.totalReactionTime += finalTime;                        // Add the reaction time to total reaction time 
                ctrl.roundsCompleted++;                                     // Increment round completion count

                string result = finalTime.ToString("0.00");
                StateHelper.TransitionState(ctrl, ref
                    ctrl.displayReactionTimeDuration, new DisplayResultState(), result);  // Transition to DisplayResultState to display round's reaction time                                     
            }
        }

        private sealed class DisplayResultState : GameState // Handles all post-round behaviour, replacing redundant GameOverState class
        {
            public override void Tick(EnhancedReactionController ctrl)
            {
                ctrl.displayReactionTimeDuration++;

                if (ctrl.displayReactionTimeDuration >= MaxDisplayReactionTicks) // 3 seconds pass
                {
                    ctrl.ProceedAfterRoundOver();   // Go to next round or display average time
                }
            }

            public override void GoStopPressed(EnhancedReactionController ctrl) // GO/STOP is pressed during result display
            {
                ctrl.ProceedAfterRoundOver();   // Skip ahead to next round
            }
        }

        private sealed class DisplayAverageState : GameState // Handles all post-game behaviour, replacing redundant GameOverState class
        {
            public override void Tick(EnhancedReactionController ctrl)
            {
                ctrl.displayReactionTimeDuration++;

                if (ctrl.displayReactionTimeDuration >= MaxDisplayAverageTicks) // 5 seconds pass
                {
                    EndGame(ctrl);  // Reset counters and transition to IdleState, ending game
                }
            }

            public override void GoStopPressed(EnhancedReactionController ctrl) // GO/STOP is pressed during average result display
            {
                EndGame(ctrl);  // Skip average result display, reset counters, and transition to IdleState
            }

            private static void EndGame(EnhancedReactionController ctrl) // End current game and ready system for new game   
            {
                ctrl.roundsCompleted = 0;               // Reset all counters
                ctrl.totalReactionTime = 0;
                ctrl.displayReactionTimeDuration = 0;
                ctrl.Init();                            // Transition to IdleState
            }
        }
    }
}
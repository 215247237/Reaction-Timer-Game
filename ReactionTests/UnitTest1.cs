using NUnit.Framework;
using SimpleReactionMachine;

namespace ReactionTests
{
    public class ReactionControllerTests
    {
        private EnhancedReactionController controller;
        private DummyGui gui;
        private DummyRandom rng;

        [SetUp]
        public void Setup()
        {
            gui = new DummyGui();
            rng = new DummyRandom();
            controller = new EnhancedReactionController();
            controller.Connect(gui, rng);
            controller.Init();
        }

        [Test]
        public void CoinInserted_ShowsPressGoMessage()
        {
            controller.CoinInserted();
            Assert.That(gui.LastDisplay, Is.EqualTo("Press GO!"));
        }

        [Test]
        public void ReadyState_TimesOutAfter10Seconds()
        {
            controller.CoinInserted(); // go to ReadyState
            for (int i = 0; i < 1000; i++) controller.Tick();
            Assert.That(gui.LastDisplay, Is.EqualTo("Insert coin"));
        }

        [Test]
        public void GoStopPressed_TransitionsToDelayState()
        {
            controller.CoinInserted(); // ReadyState
            controller.GoStopPressed();
            Assert.That(gui.LastDisplay, Is.EqualTo("Wait..."));
        }


        private class DummyGui : IGui
        {
            public string LastDisplay { get; private set; } = "";
            public void Connect(IController controller) { }
            public void Init() { }
            public void SetDisplay(string s) { LastDisplay = s; }
        }

        private class DummyRandom : IRandom
        {
            private int value = 100;
            public void SetRandom(int v) => value = v;
            public int GetRandom(int from, int to) => value;
        }
    }
}

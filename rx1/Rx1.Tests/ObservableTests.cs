namespace Rx1.Tests
{
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The observable tests.
    /// </summary>
    [TestClass]
    public class ObservableTests
    {
        /// <summary>
        /// The first test.
        /// </summary>
        [TestMethod]
        [Owner("robrodi")]
        public void FirstTest()
        {
            // Arrange
            const int events = 10000;
            var oneThousandRandomEvents = Enumerable.Range(0, events).Select(_ => GameState.GenerateEvent(_)).ToArray();
            var numKills = oneThousandRandomEvents.Where(e => e.Victim != e.Killer).Count();
            var numSuicides = oneThousandRandomEvents.Where(e => e.Victim == e.Killer).Count();

            // Act
            using (var processor = new StatsProcessorObservable(oneThousandRandomEvents.ToObservable()))
            {
                Trace.WriteLine(string.Format("Events: {0} Kills: {1} Suicides: {2}", events, processor.Kills, processor.Suicides));

                // Assert
                Assert.AreEqual(events, processor.TotalEvents, "Total Events");
                Assert.AreEqual(numKills, processor.Kills, "Kills");
                Assert.AreEqual(numSuicides, processor.Suicides, "Suicides");
            }
        }

        [TestMethod]
        [Owner("robrodi")]
        public void KillsByPlayer()
        {
            const int events = 10000;
            var oneThousandRandomEvents = Enumerable.Range(0, events).Select(_ => GameState.GenerateEvent(_)).ToArray();
            var expectPlayer0Kills = oneThousandRandomEvents.Count(e => e.Killer == 0);
            var expectPlayer0Deaths = oneThousandRandomEvents.Count(e => e.Victim == 0);
            
            // Act
            using (var processor = new StatsProcessorObservable(oneThousandRandomEvents.ToObservable()))
            {
                // Assert
                Assert.AreEqual(expectPlayer0Deaths, processor.PlayerDeathCounts[0], "Deaths");
                Assert.AreEqual(expectPlayer0Kills, processor.PlayerKillCounts[0], "Kills");
            }
        }
    }
}
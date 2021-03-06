﻿namespace Rx1.Tests
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
            const int events = 1000;
            var oneThousandRandomEvents = Enumerable.Range(0, events).Select(_ => GameState.GenerateEvent(_)).ToArray();
            var numKills = oneThousandRandomEvents.Where(e => e.Killed != e.Killer).Count();
            var numSuicides = oneThousandRandomEvents.Where(e => e.Killed == e.Killer).Count();

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
    }
}
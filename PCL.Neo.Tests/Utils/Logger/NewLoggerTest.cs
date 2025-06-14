using PCL.Neo.Core.Utils.Logger;
using System;

namespace PCL.Neo.Tests.Utils.Logger
{
    [TestFixture]
    [TestOf(typeof(NewLogger))]
    public class NewLoggerTest
    {
        [Test]
        public void LoggerTest()
        {
            var logger = NewLogger.Logger;
            var ex = new Exception("test exception");

            logger.OnDebugLogEvent += argvs =>
            {
                Console.WriteLine(argvs.Message);
                Console.WriteLine(argvs.Timestamp.ToString("O"));
                Console.WriteLine("OnDebugLog");
            };

            logger.LogDebug("Test Debug", level: NewLogger.LogLevel.Debug);
            logger.LogInformation("Hello World!");
            logger.LogWarning("Test Warning");
            logger.LogError("Test Error");
            logger.LogFatal("Test Fatal", ex);
        }
    }
}
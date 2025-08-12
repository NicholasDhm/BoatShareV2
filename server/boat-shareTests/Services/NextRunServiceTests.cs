namespace boat_share.Services.Tests
{
	[TestClass()]
	public class NextRunServiceTests
    {
        [TestMethod()]
        public void GetTimeToNextRun1MinuteBeforeTest()
        {
            TimeToNextRunCommonTest(23, 58, "03:01:00");
        }

        [TestMethod()]
        public void GetTimeToNextRun1MinuteAfterTest()
        {
            TimeToNextRunCommonTest(0, 0, "02:59:00");
        }

        public void TimeToNextRunCommonTest(int hour, int min, string expected)
        {
            var now = new DateTime(2025, 1, 18, hour, min, 0, 0, DateTimeKind.Utc);
            var testValue = NextRunService.GetTimeToNextRun(now);
            Assert.AreEqual(expected, testValue.ToString(), "for " + hour + ":" + min);
        }

        [TestMethod()]
        public void UberTest()
        {
            TimeToNextRunCommonTest(0, 0, "02:59:00");
            TimeToNextRunCommonTest(1, 0, "01:59:00");
            TimeToNextRunCommonTest(2, 0, "00:59:00");
            TimeToNextRunCommonTest(3, 0, "23:59:00");
            //for (var i = 0; i < 24; i++)
            //{
            //    TimeToNextRunCommonTest(i, 0, "02:59:00");
            //}
        }
    }
}
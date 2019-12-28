    using System;

namespace IpScanner
{
    public class TimeClass
    {
        public static Tuple<int, int, int> CurrentTime()
        {
            return Tuple.Create(Convert.ToInt32(DateTime.Now.Hour),
                Convert.ToInt32(DateTime.Now.Minute), Convert.ToInt32(DateTime.Now.Second));
        }

        private static Tuple <int, int, int> AtWhichHour(int hour, int minute, int second)
        {
            int wH, wM, wS;
            var currentTime = CurrentTime();

            wH = currentTime.Item1 + hour;
            wM = currentTime.Item2 + minute;
            wS = currentTime.Item3 + second;

            if (wS > 59)
            {
                wM++;
                wS = Math.Abs(60 - wS);
            }

            if (wM > 59)
            {
                wH++;
                wM = Math.Abs(60 - wM);
            }

            if (wH > 23) wH = Math.Abs(24 - wH);

            return Tuple.Create(wH, wM, wS);
        }

        public static Tuple <string, string, string> CurrentTimeResult(int hour, int minute, int second)
        {
            var time = CurrentTime();

            string resultHour = time.Item1.ToString(), resultMinute = time.Item2.ToString(), resultSecond = time.Item3.ToString();

            if (time.Item1 < 10) resultHour = "0" + time.Item1.ToString();
            if (time.Item2 < 10) resultMinute = "0" + time.Item2.ToString();
            if (time.Item3 < 10) resultSecond = "0" + time.Item3.ToString();

            return Tuple.Create(resultHour, resultMinute, resultSecond);
        }

        public static Tuple <string, string, string> Result(int hour, int minute, int second)
        {
            var time = AtWhichHour(hour, minute, second);
            string resultHour = time.Item1.ToString(), resultMinute = time.Item2.ToString(), resultSecond = time.Item3.ToString();

            if (time.Item1 < 10) resultHour = "0" + time.Item1.ToString();
            if (time.Item2 < 10) resultMinute = "0" + time.Item2.ToString();
            if (time.Item3 < 10) resultSecond = "0" + time.Item3.ToString();

            return Tuple.Create(resultHour, resultMinute, resultSecond);
        }
    }
}
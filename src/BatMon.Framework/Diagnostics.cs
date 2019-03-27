using System.Diagnostics;


namespace BatMon.Framework
{
    public class Diagnostics
    {
        [Conditional("DEBUG")]
        private static void setDebug() { _isRelease = false; }
        private static bool _isRelease = true;

        public static bool isRelease()
        {
            setDebug();
            return _isRelease;
        }
        public static bool isDebug()
        {
            return !isRelease();
        }

    }
}

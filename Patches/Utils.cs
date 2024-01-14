using UnityEngine;

namespace MotionTracker
{
    class Utils
    {
        public class Main
        {
            public static bool IsIngame()
            {
                GameObject env = GameObject.Find("Environment");
                if (env != null)
                {
                    return true;
                }
                return false;
            }
        }
    }
}

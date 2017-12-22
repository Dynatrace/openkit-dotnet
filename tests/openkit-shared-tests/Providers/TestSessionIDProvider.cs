namespace Dynatrace.OpenKit.Providers
{
    public class TestSessionIDProvider : ISessionIDProvider
    {
        private int initialIntegerOffset = 0;

        private static readonly object syncLock = new object();

        public int GetNextSessionID()
        {
            if (initialIntegerOffset == int.MaxValue)
            {
                initialIntegerOffset = 0;
            }
            initialIntegerOffset += 1;

            return initialIntegerOffset;
        }
    }
}

/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Johannes Baeuerle
 */
using System;

namespace Dynatrace.OpenKit.Providers
{
    /// <summary>
    ///  Class for providing a session ids
    /// </summary>
    public class DefaultSessionIDProvider : ISessionIDProvider
    {
        private int initialIntegerOffset = 0;

        private static readonly object syncLock = new object();

        public DefaultSessionIDProvider()
        {
            Random randomGenerator = new Random();
            initialIntegerOffset = randomGenerator.Next();
        }

        public int GetNextSessionID()
        {
            lock(syncLock)
            {
                if(initialIntegerOffset == int.MaxValue)
                {
                    initialIntegerOffset = 0;
                }
                initialIntegerOffset += 1;
                return initialIntegerOffset;
            }
        }
    }
}

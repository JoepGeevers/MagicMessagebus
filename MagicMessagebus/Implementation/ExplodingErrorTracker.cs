namespace MagicMessagebus.Implementation
{
    using System;

    using Contract;

    internal class ExplodingErrorTracker : IErrorTracker
    {
        public void Track(Exception e)
        {
            throw e;
        }
    }
}
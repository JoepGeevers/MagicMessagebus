namespace MagicMessagebus.Contract
{
    using System;

    public interface IErrorTracker
    {
        void Track(Exception e);
    }
}
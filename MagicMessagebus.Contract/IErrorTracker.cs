using System;

namespace MagicMessagebus.Contract
{
    public interface IErrorTracker
    {
        void Track(Exception e);
    }
}

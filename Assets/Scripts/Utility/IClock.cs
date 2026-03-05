using System;

namespace Assets.Scripts.Utility
{
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}

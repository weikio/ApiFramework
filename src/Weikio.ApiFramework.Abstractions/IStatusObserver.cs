using System;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IStatusObserver<T> where T : Enum
    {
        void Observe(StatusLog<T> newStatus);
    }
}

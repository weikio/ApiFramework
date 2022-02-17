using System;
using System.Globalization;

namespace Weikio.ApiFramework.Abstractions
{
    public class StatusLog<T> where T : Enum
    {
        public StatusLog(T previousStatus, T newStatus, DateTime logTime, string message)
        {
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            LogTime = logTime;
            Message = message;
        }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public T PreviousStatus { get; }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public T NewStatus { get; }

        public DateTime LogTime { get; }
        public string Message { get; }
        
        public override string ToString()
        {
            return $"{LogTime.ToLocalTime().ToString(CultureInfo.InvariantCulture)} - {Message}";
        }
    }
}

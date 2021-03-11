using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Weikio.ApiFramework.Abstractions
{
    public abstract class StatusBase<T> where T : Enum
    {
        protected StatusBase()
        {
            var log = new StatusLog<T>(PreviousStatus, Status, DateTime.UtcNow, "Created");

            Messages.Add(log);
        }

        public virtual void UpdateStatus(T status, string message)
        {
            var updateTime = DateTime.UtcNow;

            PreviousStatus = Status;
            Status = status;
            LastStatusUpdate = updateTime;

            var log = new StatusLog<T>(PreviousStatus, Status, updateTime, message);

            Messages.Add(log);
        }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public T Status { get; private set; }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public T PreviousStatus { get; private set; }

        public DateTime LastStatusUpdate { get; private set; }

        public List<StatusLog<T>> Messages { get; } = new List<StatusLog<T>>();

        public override string ToString()
        {
            var lastMessage = Messages.Last();

            return $"{Status}: {lastMessage.LogTime.ToLocalTime().ToString(CultureInfo.InvariantCulture)} - {lastMessage.Message}";
        }
    }
}

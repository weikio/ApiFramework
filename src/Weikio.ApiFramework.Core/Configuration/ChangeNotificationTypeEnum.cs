namespace Weikio.ApiFramework.Core.Configuration
{
    public enum ChangeNotificationTypeEnum
    {
        /// <summary>
        /// Every endpoint change is automatically handled. 
        /// </summary>
        Single,

        /// <summary>
        /// Endpoint change notifications are automatically batched. Batching notifications can provide a better performance but on the other hand, if multiple endpoints are initialized at the same time, one slow initializer means that the other faster endpoints can't be called.
        /// </summary>
        Batch,

        /// <summary>
        /// Change notifications are not provided automatically. 
        /// </summary>
        Manual
    }
}

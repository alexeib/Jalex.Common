using System.Threading.Tasks;

namespace Jalex.Infrastructure.Messaging
{
    public interface IMessagePipe<in T>
    {
        /// <summary>
        /// Asynchronously sends a message of type T to any subscribed consumers
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>Task to notify when message was fully consumed</returns>
        Task SendAsync(T message);
    }
}
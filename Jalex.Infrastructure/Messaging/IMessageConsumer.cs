using System.Threading.Tasks;

namespace Jalex.Infrastructure.Messaging
{
    public interface IMessageConsumer<in T>
    {
        /// <summary>
        /// Consumes a message asynchronously
        /// </summary>
        /// <param name="message">Message to consume</param>
        /// <returns>Task to let caller know when the message was consumed</returns>
        Task ConsumeAsync(T message);
    }
}
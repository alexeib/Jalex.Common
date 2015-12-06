using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Jalex.Concurrency.Dataflow
{
    public class FilterBlock<T> : IPropagatorBlock<T, T>
    {
        private readonly Func<T, bool> _predicate;
        private readonly IPropagatorBlock<T, T> _bufferBlock;

        public FilterBlock(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            _predicate = predicate;
            _bufferBlock = new BufferBlock<T>();
        }

        #region Implementation of IDataflowBlock

        /// <summary>
        /// Offers a message to the <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/>, giving the target the opportunity to consume or postpone the message.
        /// </summary>
        /// <returns>
        /// The status of the offered message. If the message was accepted by the target, <see cref="F:System.Threading.Tasks.Dataflow.DataflowMessageStatus.Accepted"/> is returned, and the source should no longer use the offered message, because it is now owned by the target. If the message was postponed by the target, <see cref="F:System.Threading.Tasks.Dataflow.DataflowMessageStatus.Postponed"/> is returned as a notification that the target may later attempt to consume or reserve the message; in the meantime, the source still owns the message and may offer it to other blocks.If the target would have otherwise postponed message, but <paramref name="source"/> was null, <see cref="F:System.Threading.Tasks.Dataflow.DataflowMessageStatus.Declined"/> is instead returned. If the target tried to accept the message but missed it due to the source delivering the message to another target or simply discarding it, <see cref="F:System.Threading.Tasks.Dataflow.DataflowMessageStatus.NotAvailable"/> is returned. If the target chose not to accept the message, <see cref="F:System.Threading.Tasks.Dataflow.DataflowMessageStatus.Declined"/> is returned. If the target chose not to accept the message and will never accept another message from this source, <see cref="F:System.Threading.Tasks.Dataflow.DataflowMessageStatus.DecliningPermanently"/> is returned.
        /// </returns>
        /// <param name="messageHeader">A <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> instance that represents the header of the message being offered.</param><param name="messageValue">The value of the message being offered.</param><param name="source">The <see cref="T:System.Threading.Tasks.Dataflow.ISourceBlock`1"/> offering the message. This may be null.</param><param name="consumeToAccept">Set to true to instruct the target to call <see cref="M:System.Threading.Tasks.Dataflow.ISourceBlock`1.ConsumeMessage()"/> synchronously during the call to <see cref="M:System.Threading.Tasks.Dataflow.ITargetBlock`1.OfferMessage()"/>, prior to returning <see cref="F:System.Threading.Tasks.Dataflow.DataflowMessageStatus.Accepted"/>, in order to consume the message.</param><exception cref="T:System.ArgumentException">The <paramref name="messageHeader"/> is not valid.-or-<paramref name="consumeToAccept"/> may only be true if provided with a non-null <paramref name="source"/>.</exception>
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, bool consumeToAccept)
        {
            bool passesFilter = _predicate(messageValue);
            if (passesFilter)
            {
                return _bufferBlock.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
            }

            return DataflowMessageStatus.Accepted;
        }

        /// <summary>
        /// Signals to the <see cref="T:System.Threading.Tasks.Dataflow.IDataflowBlock"/> that it should not accept nor produce any more messages nor consume any more postponed messages.
        /// </summary>
        public void Complete()
        {
            _bufferBlock.Complete();
        }

        /// <summary>
        /// Causes the <see cref="T:System.Threading.Tasks.Dataflow.IDataflowBlock"/> to complete in a <see cref="F:System.Threading.Tasks.TaskStatus.Faulted"/> state.
        /// </summary>
        /// <param name="exception">The <see cref="T:System.Exception"/> that caused the faulting.</param><exception cref="T:System.ArgumentNullException">The <paramref name="exception"/> is null.</exception>
        public void Fault(Exception exception)
        {
            _bufferBlock.Fault(exception);
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.Tasks.Task"/> that represents the asynchronous operation and completion of the dataflow block.
        /// </summary>
        /// <returns>
        /// The task.
        /// </returns>
        public Task Completion { get { return _bufferBlock.Completion; } }

        #endregion

        #region Implementation of ISourceBlock<out T>

        /// <summary>
        /// Links the <see cref="T:System.Threading.Tasks.Dataflow.ISourceBlock`1"/> to the specified <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/>.
        /// </summary>
        /// <returns>
        /// An IDisposable that, upon calling Dispose, will unlink the source from the target.
        /// </returns>
        /// <param name="target">The <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> to which to connect this source.</param><param name="linkOptions">A <see cref="T:System.Threading.Tasks.Dataflow.DataflowLinkOptions"/> instance that configures the link.</param><exception cref="T:System.ArgumentNullException"><paramref name="target"/> is null (Nothing in Visual Basic) or <paramref name="linkOptions"/> is null (Nothing in Visual Basic).</exception>
        public IDisposable LinkTo(ITargetBlock<T> target, DataflowLinkOptions linkOptions)
        {
            return _bufferBlock.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Called by a linked <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> to accept and consume a <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> previously offered by this <see cref="T:System.Threading.Tasks.Dataflow.ISourceBlock`1"/>.
        /// </summary>
        /// <returns>
        /// The value of the consumed message. This may correspond to a different <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> instance than was previously reserved and passed as the <paramref name="messageHeader"/> to <see cref="M:System.Threading.Tasks.Dataflow.ISourceBlock`1.ConsumeMessage()"/>. The consuming <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> must use the returned value instead of the value passed as messageValue through <see cref="M:System.Threading.Tasks.Dataflow.ITargetBlock`1.OfferMessage()"/>.If the message requested is not available, the return value will be null.
        /// </returns>
        /// <param name="messageHeader">The <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> of the message being consumed.</param><param name="target">The <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> consuming the message.</param><param name="messageConsumed">true if the message was successfully consumed; otherwise, false.</param><exception cref="T:System.ArgumentException">The <paramref name="messageHeader"/> is not valid.</exception><exception cref="T:System.ArgumentNullException">The <paramref name="target"/> is null.</exception>
        public T ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target, out bool messageConsumed)
        {
            return _bufferBlock.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <summary>
        /// Called by a linked <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> to reserve a previously offered <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> by this <see cref="T:System.Threading.Tasks.Dataflow.ISourceBlock`1"/>.
        /// </summary>
        /// <returns>
        /// true if the message was successfully reserved; otherwise, false.
        /// </returns>
        /// <param name="messageHeader">The <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> of the message being reserved.</param><param name="target">The <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> reserving the message.</param><exception cref="T:System.ArgumentException">The <paramref name="messageHeader"/> is not valid.</exception><exception cref="T:System.ArgumentNullException">The <paramref name="target"/> is null.</exception>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        {
            return _bufferBlock.ReserveMessage(messageHeader, target);
        }

        /// <summary>
        /// Called by a linked <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> to release a previously reserved <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> by this <see cref="T:System.Threading.Tasks.Dataflow.ISourceBlock`1"/>.
        /// </summary>
        /// <param name="messageHeader">The <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> of the reserved message being released.</param><param name="target">The <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> releasing the message it previously reserved.</param><exception cref="T:System.ArgumentException">The <paramref name="messageHeader"/> is not valid.</exception><exception cref="T:System.ArgumentNullException">The <paramref name="target"/> is null.</exception><exception cref="T:System.InvalidOperationException">The <paramref name="target"/> did not have the message reserved.</exception>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        {
            _bufferBlock.ReleaseReservation(messageHeader, target);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Jalex.Infrastructure.Concurrency.Dataflow
{
    public sealed class EmptyBlock<T> : IReceivableSourceBlock<T>
    {
        #region Implementation of IDataflowBlock

        /// <summary>
        /// Signals to the <see cref="T:System.Threading.Tasks.Dataflow.IDataflowBlock"/> that it should not accept nor produce any more messages nor consume any more postponed messages.
        /// </summary>
        public void Complete()
        {
            throw new InvalidOperationException("Empty block cannot complete");
        }

        /// <summary>
        /// Causes the <see cref="T:System.Threading.Tasks.Dataflow.IDataflowBlock"/> to complete in a <see cref="F:System.Threading.Tasks.TaskStatus.Faulted"/> state.
        /// </summary>
        /// <param name="exception">The <see cref="T:System.Exception"/> that caused the faulting.</param><exception cref="T:System.ArgumentNullException">The <paramref name="exception"/> is null.</exception>
        public void Fault(Exception exception)
        {
            throw new InvalidOperationException("Empty block cannot fault");
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.Tasks.Task"/> that represents the asynchronous operation and completion of the dataflow block.
        /// </summary>
        /// <returns>
        /// The task.
        /// </returns>
        public Task Completion => Task.FromResult(0);

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
            throw new InvalidOperationException("Empty block cannot be linked");
        }

        /// <summary>
        /// Called by a linked <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> to accept and consume a <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> previously offered by this <see cref="T:System.Threading.Tasks.Dataflow.ISourceBlock`1"/>.
        /// </summary>
        /// <returns>
        /// The value of the consumed message. This may correspond to a different <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> instance than was previously reserved and passed as the <paramref name="messageHeader"/> to <see cref="M:System.Threading.Tasks.Dataflow.ISourceBlock`1.ConsumeMessage()"/>. The consuming <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> must use the returned value instead of the value passed as <paramref name="messageValue"/> through <see cref="M:System.Threading.Tasks.Dataflow.ITargetBlock`1.OfferMessage()"/>.If the message requested is not available, the return value will be null.
        /// </returns>
        /// <param name="messageHeader">The <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> of the message being consumed.</param><param name="target">The <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> consuming the message.</param><param name="messageConsumed">true if the message was successfully consumed; otherwise, false.</param><exception cref="T:System.ArgumentException">The <paramref name="messageHeader"/> is not valid.</exception><exception cref="T:System.ArgumentNullException">The <paramref name="target"/> is null.</exception>
        public T ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target, out bool messageConsumed)
        {
            throw new InvalidOperationException("Empty block cannot consume messages");
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
            throw new InvalidOperationException("Empty block cannot reserve message");
        }

        /// <summary>
        /// Called by a linked <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> to release a previously reserved <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> by this <see cref="T:System.Threading.Tasks.Dataflow.ISourceBlock`1"/>.
        /// </summary>
        /// <param name="messageHeader">The <see cref="T:System.Threading.Tasks.Dataflow.DataflowMessageHeader"/> of the reserved message being released.</param><param name="target">The <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> releasing the message it previously reserved.</param><exception cref="T:System.ArgumentException">The <paramref name="messageHeader"/> is not valid.</exception><exception cref="T:System.ArgumentNullException">The <paramref name="target"/> is null.</exception><exception cref="T:System.InvalidOperationException">The <paramref name="target"/> did not have the message reserved.</exception>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        {
            throw new InvalidOperationException("Empty block cannot release reservation");
        }

        #endregion

        #region Implementation of IReceivableSourceBlock<T>

        /// <summary>
        /// Attempts to synchronously receive an available output item from the<see cref="T:System.Threading.Tasks.Dataflow.IReceivableSourceBlock`1"/>.
        /// </summary>
        /// <returns>
        /// true if an item could be received; otherwise, false.
        /// </returns>
        /// <param name="filter">The predicate value must successfully pass in order for it to be received. <paramref name="filter"/> may be null, in which case all items will pass.</param><param name="item">The item received from the source.</param>
        public bool TryReceive(Predicate<T> filter, out T item)
        {
            item = default(T);
            return false;
        }

        /// <summary>
        /// Attempts to synchronously receive all available items from the <see cref="T:System.Threading.Tasks.Dataflow.IReceivableSourceBlock`1"/>.
        /// </summary>
        /// <returns>
        /// true if one or more items could be received; otherwise, false.
        /// </returns>
        /// <param name="items">The items received from the source.</param>
        public bool TryReceiveAll(out IList<T> items)
        {
            items = null;
            return false;
        }

        #endregion
    }
}

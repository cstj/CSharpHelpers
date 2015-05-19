using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace Helpers.MVVMHelpers
{

    public class ObservableCollectionGUISafe<T> : ObservableCollection<T>
    {
        public enum LockTypeEnum
        {
            SpinWait,
            Lock
        }

        private bool _lockObjWasTaken;
        private readonly object _lockObj;
        private int _lock; // 0=unlocked		1=locked

        private readonly LockTypeEnum _lockType;
        public LockTypeEnum LockType
        {
            get
            {
                return _lockType;
            }
        }

        public ObservableCollectionGUISafe(LockTypeEnum lockType)
            : base()
		{
			_lockType = lockType;
			_lockObj = new object();
		}

        #region Dispatch Helpers
        protected Dispatcher GetDispatcher()
        {
            return Dispatcher.FromThread(Thread.CurrentThread);
        }

        protected void WaitForCondition(Func<bool> condition)
        {
            var dispatcher = GetDispatcher();

            if (dispatcher == null)
            {
                switch (LockType)
                {
                    case LockTypeEnum.SpinWait:
                        SpinWait.SpinUntil(condition); // spin baby... 
                        break;
                    case LockTypeEnum.Lock:
                        var isLockTaken = false;
                        Monitor.Enter(_lockObj, ref isLockTaken);
                        _lockObjWasTaken = isLockTaken;
                        break;
                }
                return;
            }

            _lockObjWasTaken = true;
            PumpWait_PumpUntil(dispatcher, condition);
        }

        protected void PumpWait_PumpUntil(Dispatcher dispatcher, Func<bool> condition)
        {
            var frame = new DispatcherFrame();
            BeginInvokePump(dispatcher, frame, condition);
            Dispatcher.PushFrame(frame);
        }

        private static void BeginInvokePump(Dispatcher dispatcher, DispatcherFrame frame, Func<bool> condition)
        {
            dispatcher.BeginInvoke
                (
                DispatcherPriority.DataBind,
                (Action)
                    (
                    () =>
                    {
                        frame.Continue = !condition();

                        if (frame.Continue)
                            BeginInvokePump(dispatcher, frame, condition);
                    }
                    )
                );
        }

        public void DoEvents()
        {
            var dispatcher = GetDispatcher();
            if (dispatcher == null)
            {
                return;
            }

            var frame = new DispatcherFrame();
            dispatcher.BeginInvoke(DispatcherPriority.DataBind, new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        private static object ExitFrame(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }

        protected bool TryLock()
        {
            switch (LockType)
            {
                case LockTypeEnum.SpinWait:
                    return Interlocked.CompareExchange(ref _lock, 1, 0) == 0;
                case LockTypeEnum.Lock:
                    return Monitor.TryEnter(_lockObj);
            }

            return false;
        }

        protected void Lock()
        {
            switch (LockType)
            {
                case LockTypeEnum.SpinWait:
                    WaitForCondition(() => Interlocked.CompareExchange(ref _lock, 1, 0) == 0);
                    break;
                case LockTypeEnum.Lock:
                    WaitForCondition(() => Monitor.TryEnter(_lockObj));
                    break;
            }
        }

        protected void Unlock()
        {
            switch (LockType)
            {
                case LockTypeEnum.SpinWait:
                    _lock = 0;
                    break;
                case LockTypeEnum.Lock:
                    if (_lockObjWasTaken)
                        Monitor.Exit(_lockObj);
                    _lockObjWasTaken = false;
                    break;
            }
        }
        #endregion

        #region INotifyCollectionChanged

        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            var notifyCollectionChangedEventHandler = CollectionChanged;

            if (notifyCollectionChangedEventHandler == null)
                return;

            foreach (NotifyCollectionChangedEventHandler handler in notifyCollectionChangedEventHandler.GetInvocationList())
            {
                var dispatcherObject = handler.Target as DispatcherObject;

                if (dispatcherObject != null && !dispatcherObject.CheckAccess())
                {
                    dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, args);
                }
                else
                    handler(this, args);
            }
        }

        protected void RaiseNotifyCollectionChanged()
        {
            RaiseNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected void RaiseNotifyCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            RaisePropertyChanged("Count");
            RaisePropertyChanged("Item[]");
            OnCollectionChanged(args);
        }

        #endregion INotifyCollectionChanged

        #region INotifyPropertyChanged

        protected override event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            var propertyChangedEventHandler = PropertyChanged;

            if (propertyChangedEventHandler != null)
            {
                propertyChangedEventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged


    }
}
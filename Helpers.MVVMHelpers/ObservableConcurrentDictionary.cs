using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;

namespace Helpers.MVVMHelpers
{
    public enum LockTypeEnum
    {
        SpinWait,
        Lock
    }

    public class ObservableConcurrentDictionary<TKey, TVal> : ConcurrentDictionary<TKey, TVal>, INotifyCollectionChanged, INotifyPropertyChanged, IDictionary<TKey, TVal>, ICollection<KeyValuePair<TKey, TVal>>
    {
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

        public ObservableConcurrentDictionary(LockTypeEnum lockType)
            : base()
        {
            ConcurrentDictionary<TKey, TVal> t = new ConcurrentDictionary<TKey, TVal>();
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

        #region Dictionary Updates with Notification
        public TVal GetOrAddWithNote(TKey key, Func<TKey, TVal> valueFactory)
        {
            // Stores the value
            TVal value;
            // If key exists
            if (base.ContainsKey(key))
                // Get value
                value = base.GetOrAdd(key, valueFactory);
            // Else if key does not exist
            else
            {
                // Add value and raise event
                value = base.GetOrAdd(key, valueFactory);
                RaiseNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
            }
            // Return value
            return value;
        }

        public TVal GetOrAddWithNote(TKey key, TVal value)
        {
            // If key exists
            if (base.ContainsKey(key))
                // Get value
                base.GetOrAdd(key, value);
            // Else if key does not exist
            else
            {
                // Add value and raise event
                base.GetOrAdd(key, value);
                RaiseNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
            }
            // Return value
            return value;
        }

        public bool TryAddWithNote(TKey key, TVal value)
        {
            // Stores tryAdd
            bool tryAdd;
            // If added
            if (tryAdd = base.TryAdd(key, value))
                // Raise event
                RaiseNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
            // Return tryAdd
            return tryAdd;
        }

        public bool TryRemoveWithNote(TKey key, out TVal value)
        {
            // Stores tryRemove
            bool tryRemove;
            // If removed
            if (tryRemove = base.TryRemove(key, out value))
                // Raise event
                RaiseNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));
            // Return tryAdd
            return tryRemove;
        }

        public bool TryUpdateWithNote(TKey key, TVal newValue)
        {
            if (base.ContainsKey(key)) return TryUpdateWithNote(key, newValue, base[key]);
            else return TryAddWithNote(key, newValue);
        }

        public bool TryUpdateWithNote(TKey key, TVal newValue, TVal comparisonValue)
        {
            // Stores tryUpdate
            bool tryUpdate;
            // If updated
            if (tryUpdate = base.TryUpdate(key, newValue, comparisonValue))
                // Raise event
                RaiseNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newValue));
            // Return tryUpdate
            return tryUpdate;
        }
        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members
        void ICollection<KeyValuePair<TKey, TVal>>.Add(KeyValuePair<TKey, TVal> item)
        {
            this.TryAddWithNote(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<TKey, TVal>>.Clear()
        {
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, this));
        }

        bool ICollection<KeyValuePair<TKey, TVal>>.Remove(KeyValuePair<TKey, TVal> item)
        {
            TVal temp;
            return this.TryRemoveWithNote(item.Key, out temp);
        }
        #endregion

        #region IDictionary<TKey,TValue> Members
        public void Add(TKey key, TVal value)
        {
            this.TryAddWithNote(key, value);
        }

        public bool Remove(TKey key)
        {
            TVal temp;
            return this.TryRemoveWithNote(key, out temp);
        }

        public new TVal this[TKey key]
        {
            get { return base[key]; }
            set { TryUpdateWithNote(key, value); }
        }
        #endregion

        #region INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
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
            OnPropertyChanged("Count");
            OnPropertyChanged("Keys");
            OnPropertyChanged("Values");
            OnCollectionChanged(args);
        }

        #endregion INotifyCollectionChanged

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
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
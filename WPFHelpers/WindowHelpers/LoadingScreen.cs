using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers.WPFHelpers.WindowHelpers
{
    public class LoadingScreen<T> : IDisposable where T : System.Windows.Window
    {
        T windowLoading;
        System.Windows.Threading.Dispatcher windowLoadingDispatcher;
        private bool disposed = false;
        private System.Windows.Window _owner;

        public LoadingScreen(System.Windows.Window owner)
        {
            _owner = owner;
            CreateLoadingWindow();
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                CloseLoadingWindow();
            }
            disposed = true;
        }

        ~LoadingScreen()
        {
            Dispose(false);
        }
        #endregion

        private T GetInstance()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        public void CreateLoadingWindow()
        {
            if (windowLoading == null)
            {
                double ParentHor = 0;
                double ParentVer = 0;
                if (_owner != null)
                {
                    ParentHor = _owner.Left + (_owner.Width / 2);
                    ParentVer = _owner.Top + (_owner.Height / 2);
                }
                System.Threading.ManualResetEvent reset = new System.Threading.ManualResetEvent(false);
                reset.Reset();
                System.Threading.Thread t = new System.Threading.Thread(() =>
                {
                    windowLoading = GetInstance();
                    if (_owner != null)
                    {
                        windowLoading.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                        //Set to center of parent window
                        windowLoading.Top = ParentVer - (windowLoading.Height / 2);
                        windowLoading.Left = ParentHor - (windowLoading.Width / 2);
                    }
                    else
                    {
                        windowLoading.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    }
                    windowLoadingDispatcher = windowLoading.Dispatcher;
                    reset.Set();
                    System.Windows.Threading.Dispatcher.Run();

                });
                t.IsBackground = true;
                t.SetApartmentState(System.Threading.ApartmentState.STA);
                t.Start();
                reset.WaitOne();
            }
            windowLoadingDispatcher.Invoke(new Action(() =>
                windowLoading.Show()));
        }

        public void CloseLoadingWindow()
        {
            windowLoadingDispatcher.Invoke(new Action(() => windowLoading.Close()));
            windowLoading = null;
            windowLoadingDispatcher = null;
        }
    }
}
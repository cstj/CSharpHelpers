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
        bool disposed = false;

        public LoadingScreen()
        {
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
                System.Threading.ManualResetEvent reset = new System.Threading.ManualResetEvent(false);
                reset.Reset();
                System.Threading.Thread t = new System.Threading.Thread(() =>
                {
                    windowLoading = GetInstance();
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
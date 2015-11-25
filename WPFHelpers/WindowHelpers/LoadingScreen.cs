using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers.WPFHelpers.WindowHelpers
{
    /// <summary>
    /// This function loads a splash screen and hands back control to the caller.  
    /// It the splash can then be closed by calling CloseLoadingWindow.
    /// It takes a WPF window as a splash.  Usually a window with no Chrome or interactable controls.
    /// </summary>
    /// <typeparam name="T">WPF window to load as a splash.</typeparam>
    public class LoadingScreen<T> : IDisposable where T : System.Windows.Window
    {
        T windowLoading;
        System.Windows.Threading.Dispatcher windowLoadingDispatcher;
        private bool disposed = false;
        private System.Windows.Window _owner;
        double ParentHor = 0;
        double ParentVer = 0;

        System.Threading.Thread thread;
        string _ThreadName;

        /// <summary>
        /// Creates a loading window on an off thread wihtout blocking.
        /// </summary>
        /// <param name="owner">Parent window to open in the center of.</param>
        public LoadingScreen(System.Windows.Window owner)
        {
            _ThreadName = "Splash Screen";
            _owner = owner;
            CreateLoadingWindow();
        }

        /// <summary>
        /// Creates a loading window on an off thread wihtout blocking.
        /// </summary>
        /// <param name="owner">Parent window to open in the center of.</param>
        /// <param name="ThreadName">The name of the off thread.</param>
        public LoadingScreen(System.Windows.Window owner, string ThreadName)
        {
            _owner = owner;
            _ThreadName = ThreadName;
            CreateLoadingWindow();
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose Implementation.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                CloseLoadingWindow();
            }
            disposed = true;
        }

        /// <summary>
        /// Destructor.  Fires disposal.
        /// </summary>
        ~LoadingScreen()
        {
            Dispose(false);
        }
        #endregion

        /// <summary>
        /// Returns an instance of hte splash window.
        /// </summary>
        /// <returns></returns>
        private T GetInstance()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        /// <summary>
        /// Creates the splash window it self.  Non blocking.
        /// </summary>
        private void CreateLoadingWindow()
        {
            if (windowLoading == null)
            {
                //If we have a parent get its center.
                
                if (_owner != null)
                {
                    ParentHor = _owner.Left + (_owner.Width / 2);
                    ParentVer = _owner.Top + (_owner.Height / 2);
                }
                //Create an event to trigger when the dispatcher is ready (so we can hand back 
                //control to the calling function and know our loading screen will close when 
                //instructed).
                System.Threading.ManualResetEvent reset = new System.Threading.ManualResetEvent(false);
                reset.Reset();

                //Create thread to run splash window.
                thread = new System.Threading.Thread(() =>
                {
                    InitLoadingWindow(reset);
                });
                //Name our thread.  Make it background
                thread.Name = _ThreadName;
                thread.IsBackground = true;
                //Set its apartment state to STA (So we can make a GUI on this thread).
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                //Start it!
                thread.Start();
                //Wait for the dispatcher to start.
                reset.WaitOne();
                System.Threading.Thread.Sleep(100);
                //Show the window.
                windowLoadingDispatcher.Invoke(new Action(() =>
                    {
                        windowLoading.Show();
                    }));
            }
        }

        private void InitLoadingWindow(System.Threading.ManualResetEvent reset)
        {
            System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Threading.Dispatcher.CurrentDispatcher));

            //Get an instance of the splash window.
            windowLoading = GetInstance();
            
            //If we have an owner set the start location manually
            if (_owner != null)
            {
                windowLoading.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                //Set to center of parent window
                windowLoading.Top = ParentVer - (windowLoading.Height / 2);
                windowLoading.Left = ParentHor - (windowLoading.Width / 2);
            }
            else
            {
                //Otherwise start in the center of the screen.
                windowLoading.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            
            //Set our event and run the dispatcher waiting for instructions.
            windowLoading.Closed += (s, a) => { System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Background); };
            //Signal ready and run the dispatcher
            reset.Set();

            //Get our dispatcher for use outside of this thread and start it
            windowLoadingDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            System.Windows.Threading.Dispatcher.Run();
        }

        public void CloseLoadingWindow()
        {
            //Close our window using the dispatcher.
            windowLoadingDispatcher.Invoke(new Action(() =>
            {
                windowLoading.Close();
            }));

            //Join the threads.
            thread.Join();

            //Clean up.
            windowLoading = null;
            windowLoadingDispatcher = null;
        }
    }
}
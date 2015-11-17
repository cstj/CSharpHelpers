using System;
using System.ComponentModel;

namespace Helpers.MVVMHelpers.ResourceLocator
{
    #region Opening Class for main ViewModel
    /// <summary>
    /// Landing point that inmplements the main control for the program.  Make properties to objects for views here.
    /// </summary>
    public class Main
    {
        /// <summary>
        /// The main opening place for the main class.
        /// </summary>
        /// <param name="_windowFactory"></param>
        public Main(IWindowFactory _windowFactory)
        {
        }

        public Menu Menu { get; }
    }

    /// <summary>
    /// Dummy implementation.
    /// </summary>
    public class Menu
    {
    }
 
    /// <summary>
    /// Window Factory Interface so viewmodels can open windows.
    /// </summary>
    public interface IWindowFactory
    {
        /// <summary>
        /// Create Window
        /// </summary>
        void CreateWindow();
    }
    #endregion

    #region Goes with Views in main GUI program
    //GUI implementation of IWindowFactory
    public class ProductionWindowFactory : IWindowFactory
    {
        void CreateWindow
        {

        }
    }

    /// <summary>
    /// Link to main viewmodel class.
    /// </summary>
    public class MainViewModel
    {
        ProductionWindowFactory windowFactory;

        /// <summary>
        /// Create required resources
        /// </summary>
        public MainViewModel()
        {
            //Initialise the main viewmodel resource and window factory.
            windowFactory = new ProductionWindowFactory();
            MainResource = new Main(windowFactory);
        }

        private Main _MainResource;
        public Main MainResource
        {
            internal set { _MainResource = value; }
            get { return _MainResource; }
        }
    }

    /* This goes in the main App.xaml
    <Application.Resources>
        <local:MainViewModel x:Key="Locator"/>
    </Application.Resources>
     
    Then in each View:
    <Window.DataContext>
        <Binding Path="MainResource.Menu" Source="{StaticResource Locator}"/>
    </Window.DataContext>
     
     */
    #endregion
}

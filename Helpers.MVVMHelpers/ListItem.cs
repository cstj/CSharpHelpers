using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Helpers.MVVMHelpers
{
    /// <summary>
    /// Event Args to hold Item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListItemEventArgs<T> : EventArgs
    {
        public ListItem<T> item { get; set; }
        public ListItemEventArgs(ListItem<T> in_item)
        {
            item = in_item;
        }
    }

    public class ListItem<T> : INotifyPropertyChanged
    {
        #region Property Change Handleing
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            VerifyPropertyName(name);
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void VerifyPropertyName(string propertyName)
        {
            var myType = this.GetType();
            if (myType.GetProperty(propertyName) == null)
            {
                throw new ArgumentException("Property not found", propertyName);
            }
        }
        #endregion

        #region Constructor
        public ListItem()
        {
            //Connect Click Commands / Events etc
            DoubleClickCommand = new RelayCommand(() => DoubleClickExecute());
            ClickCommand = new RelayCommand(() => ClickExecute());
        }
        #endregion

        #region Operators
        public override bool Equals(object obj)
        {
            //If null ira not equal
            if (obj == null) return false;

            //If types are diffrent then its not equal
            if (obj.GetType() != this.GetType())
                return false;
            else
            {
                ListItem<T> t = (ListItem<T>)obj;
                //If we have the same object return true;
                if (t == this) return true;
                //if any properties are diffrent its not equal
                foreach (System.Reflection.PropertyInfo property in this.GetType().GetProperties())
                {
                    if (property.GetValue(this, null) != property.GetValue(t, null)) return false;
                }
                return true;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #region Properties
        #region Functions to find Selected Items
        /// <summary>
        /// Returns the first Selected Item
        /// </summary>
        /// <returns></returns>
        public ListItem<T> FindFirstSelcted()
        {
            //Run Find select finding only the first item
            List<ListItem<T>> tmp = FindSelected(true);
            if (tmp.Count == 1) return tmp[0];
            else return null;
        }

        /// <summary>
        /// Returns All Selected Items
        /// </summary>
        /// <returns></returns>
        public List<ListItem<T>> FindSelected()
        {
            // Finds all selected items
            return FindSelected(false);
        }

        /// <summary>
        /// Private function taht returns selected items or only first
        /// </summary>
        /// <param name="OnlyFirst">Defines if this is only returning the first found or all</param>
        /// <returns>Selected Items</returns>
        private List<ListItem<T>> FindSelected(bool OnlyFirst)
        {
            List<ListItem<T>> itemList = new List<ListItem<T>>();
            //Is the current item selected?
            if (this.IsSelected)
            {
                itemList.Add(this);
                //If we only want the first item, return it
                if (OnlyFirst) return itemList; 
            }

            //What children are selected?
            var childSelected = from i in this.Children
                                where i.IsSelected
                                select i;
            
            //if we have a return and we only want the first return the frist child found
            if (OnlyFirst && childSelected != null)
            {
                itemList.Add(childSelected.First());
                return itemList;
            }
            //Add selected children
            if (childSelected != null) itemList.AddRange(childSelected.ToList());

            //Cycle through children looking for selected
            foreach (ListItem<T> l in this.Children)
            {
                itemList.AddRange(l.FindSelected());
            }
            return itemList;
        }
        #endregion
        #region Title Property
        public const string TitleName = "Title";
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                if (_Title == value) return;
                _Title = value;
                OnPropertyChanged(TitleName);
            }
        }
        #endregion
        #region ColumnData Property
        public const string ColumnDataName = "ColumnData";
        private Dictionary<string,string> _ColumnData;
        public Dictionary<string,string> ColumnData
        {
            get { return _ColumnData; }
            set
            {
                if (_ColumnData == value) return;
                _ColumnData = value;
                OnPropertyChanged(ColumnDataName);
            }
        }
        #endregion
        #region Value Property
        public const string ValueName = "Value";
        private T _Value;
        public T Value
        {
            get { return _Value; }
            set
            {
                if (_Value != null)
                {
                    if (_Value.Equals(value)) return;
                }
                _Value = value;
                OnPropertyChanged(ValueName);
            }
        }
        #endregion
        #region Children Property
        public const string ChildrenName = "Children";
        private ICollection<ListItem<T>> _Children;
        public ICollection<ListItem<T>> Children
        {
            get { return _Children; }
            set
            {
                if (_Children == value) return;
                //Unsubscribe from events of children
                if (_Children != null) foreach (var c in Children) c.PropertyChanged -= Children_PropertyChanged;
                _Children = value;
                //Subscribe to new child events
                foreach (var c in Children) c.PropertyChanged += Children_PropertyChanged;
                OnPropertyChanged(ChildrenName);
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler ChildChanged;
        protected virtual void OnChildChanged(System.ComponentModel.PropertyChangedEventArgs e, object sender)
        {
            if (ChildChanged != null)
            {
                ChildChanged(sender, e);
            }
        }

        /// <summary>
        /// Handler for when a child changes.  Raises the property changed event for the next level up.  Inception style
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Children_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.ComponentModel.PropertyChangedEventArgs ags = new System.ComponentModel.PropertyChangedEventArgs(ChildrenName);
            OnChildChanged(ags, sender);
        }
        #endregion
        #region FontWeight Property
        public const string FontWeightName = "FontWeight";
        private string _FontWeight;
        public string FontWeight
        {
            get { return _FontWeight; }
            set
            {
                if (_FontWeight == value) return;
                _FontWeight = value;
                OnPropertyChanged(FontWeightName);
            }
        }
        #endregion
        #region Background Property
        public const string BackgroundName = "Background";
        private string _Background;
        public string Background
        {
            get { return _Background; }
            set
            {
                if (_Background == value) return;
                _Background = value;
                OnPropertyChanged(BackgroundName);
            }
        }
        #endregion
        #region IsSelected Property
        public const string IsSelectedName = "IsSelected";
        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (_IsSelected == value) return;
                _IsSelected = value;
                OnPropertyChanged(IsSelectedName);
            }
        }
        #endregion
        #region IsExpanded Property
        public const string IsExpandedName = "IsExpanded";
        private bool _IsExpanded;
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                if (_IsExpanded == value) return;
                _IsExpanded = value;
                OnPropertyChanged(IsExpandedName);
            }
        }
        #endregion
        #endregion
        
        /// <summary>
        /// Expand / Collapse all this items children
        /// </summary>
        /// <param name="Expanded"></param>
        /// <param name="ExpandChildren"></param>
        public void ExpandCollapseCollection(bool Expanded, bool ExpandChildren)
        {
            IsExpanded = Expanded;
            if (Children != null)
            {
                foreach (ListItem<T> l in Children) l.ExpandCollapseCollection(Expanded, ExpandChildren);
            }
        }


        #region Events
        #region Double Click
        public event EventHandler<ListItemEventArgs<T>> DoubleClicked;
        protected virtual void OnDoubleClick(ListItemEventArgs<T> e)
        {
            if (DoubleClicked != null)
            {
                DoubleClicked(this, e);
            }
        }
        public RelayCommand DoubleClickCommand { get; private set; }
        private void DoubleClickExecute()
        {
            OnDoubleClick(new ListItemEventArgs<T>(this));
        }
        #endregion
        #region Click
        public event EventHandler<ListItemEventArgs<T>> Clicked;
        protected virtual void OnClick(ListItemEventArgs<T> e)
        {
            if (Clicked != null)
            {
                Clicked(this, e);
            }
        }
        public RelayCommand ClickCommand { get; private set; }
        private void ClickExecute()
        {
            OnClick(new ListItemEventArgs<T>(this));
        }
        #endregion
        #endregion
    }
}

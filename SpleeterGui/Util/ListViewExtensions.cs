using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SpleeterGui.Util
{
    /// <summary>
    /// https://stackoverflow.com/questions/31176949/binding-selecteditems-of-listview-to-viewmodel/31178953#31178953
    /// </summary>
    public class ListViewExtensions
    {
        private static SelectedItemsBinder GetSelectedValueBinder(DependencyObject obj)
        {
            return (SelectedItemsBinder)obj.GetValue(SelectedValueBinderProperty);
        }

        private static void SetSelectedValueBinder(DependencyObject obj, SelectedItemsBinder items)
        {
            obj.SetValue(SelectedValueBinderProperty, items);
        }

        private static readonly DependencyProperty SelectedValueBinderProperty = DependencyProperty.RegisterAttached("SelectedValueBinder", typeof(SelectedItemsBinder), typeof(ListViewExtensions));


        public static readonly DependencyProperty SelectedValuesProperty = DependencyProperty.RegisterAttached("SelectedValues", typeof(IList), typeof(ListViewExtensions),
            new FrameworkPropertyMetadata(null, OnSelectedValuesChanged));


        private static void OnSelectedValuesChanged(DependencyObject o, DependencyPropertyChangedEventArgs value)
        {
            var oldBinder = GetSelectedValueBinder(o);
            if (oldBinder != null)
                oldBinder.UnBind();

            SetSelectedValueBinder(o, new SelectedItemsBinder((ListView)o, (IList)value.NewValue));
            GetSelectedValueBinder(o).Bind();
        }

        public static void SetSelectedValues(Selector elementName, IEnumerable value)
        {
            elementName.SetValue(SelectedValuesProperty, value);
        }

        public static IEnumerable GetSelectedValues(Selector elementName)
        {
            return (IEnumerable)elementName.GetValue(SelectedValuesProperty);
        }
    }

    public class SelectedItemsBinder
    {
        private ListView _listView;
        private IList _collection;

        public SelectedItemsBinder(ListView listView, IList collection)
        {
            _listView = listView;
            _collection = collection ?? new object[0];

            _listView.SelectedItems.Clear();

            foreach (var item in _collection)
            {
                _listView.SelectedItems.Add(item);
            }
        }

        public void Bind()
        {
            _listView.SelectionChanged += ListView_SelectionChanged;

            if (_collection is INotifyCollectionChanged)
            {
                var observable = (INotifyCollectionChanged)_collection;
                observable.CollectionChanged += Collection_CollectionChanged;
            }
        }

        public void UnBind()
        {
            if (_listView != null)
                _listView.SelectionChanged -= ListView_SelectionChanged;

            if (_collection != null && _collection is INotifyCollectionChanged)
            {
                var observable = (INotifyCollectionChanged)_collection;
                observable.CollectionChanged -= Collection_CollectionChanged;
            }
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems ?? new object[0])
            {
                if (!_listView.SelectedItems.Contains(item))
                    _listView.SelectedItems.Add(item);
            }
            foreach (var item in e.OldItems ?? new object[0])
            {
                _listView.SelectedItems.Remove(item);
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems ?? new object[0])
            {
                if (!_collection.Contains(item))
                    _collection.Add(item);
            }

            foreach (var item in e.RemovedItems ?? new object[0])
            {
                _collection.Remove(item);
            }
        }
    }
}

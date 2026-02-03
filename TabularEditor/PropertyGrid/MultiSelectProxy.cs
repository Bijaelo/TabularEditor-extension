using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TabularEditor.PropertyGridExtension
{
    internal sealed class MultiSelectProxy : ICustomTypeDescriptor
    {
        private readonly object[] _items;

        public MultiSelectProxy(object[] items)
        {
            _items = items ?? Array.Empty<object>();
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            if (_items.Length == 0) return new PropertyDescriptorCollection(null);

            var first = _items[0];
            var firstProps = TypeDescriptor.GetProperties(first, attributes, true).Cast<PropertyDescriptor>();
            var merged = new List<PropertyDescriptor>();

            foreach (var pd in firstProps)
            {
                var presentOnAll = _items
                    .Skip(1)
                    .All(o => TypeDescriptor.GetProperties(o, attributes, true)
                        .Cast<PropertyDescriptor>()
                        .Any(p => p.Name == pd.Name && p.PropertyType == pd.PropertyType));

                if (presentOnAll)
                {
                    merged.Add(pd);
                    continue;
                }

                if ((pd.Name == "ObjectLevelSecurity" || pd.Name == "RowLevelSecurity") &&
                    _items.Skip(1).All(o =>
                    {
                        var p = TypeDescriptor.GetProperties(o, attributes, true).Find(pd.Name, false);
                        return p != null && p.PropertyType == pd.PropertyType;
                    }))
                {
                    merged.Add(pd);
                }
            }

            return new PropertyDescriptorCollection(merged.ToArray(), true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _items.Length > 0 ? _items[0] : null;
        }

        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(_items.FirstOrDefault());
        public string GetClassName() => TypeDescriptor.GetClassName(_items.FirstOrDefault());
        public string GetComponentName() => TypeDescriptor.GetComponentName(_items.FirstOrDefault());
        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(_items.FirstOrDefault());
        public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(_items.FirstOrDefault());
        public PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(_items.FirstOrDefault());
        public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(_items.FirstOrDefault(), editorBaseType);
        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(_items.FirstOrDefault());
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(_items.FirstOrDefault(), attributes);
    }
}

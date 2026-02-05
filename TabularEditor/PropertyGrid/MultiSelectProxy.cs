using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TabularEditor.PropertyGridExtension
{
    internal sealed class MultiSelectProxy : ICustomTypeDescriptor
    {
        private readonly object[] _items;
        private static readonly HashSet<string> AllowList = new HashSet<string>(StringComparer.Ordinal)
        {
            "ObjectLevelSecurity",
            "RowLevelSecurity",
        };

        public MultiSelectProxy(object[] items)
        {
            _items = items ?? Array.Empty<object>();
        }

        public static bool RequiresProxy(object[] items, Attribute[] attributes = null)
        {
            if (items == null || items.Length <= 1) return false;

            foreach (var name in AllowList)
            {
                if (IsPresentOnAll(items, name, attributes))
                {
                    return true;
                }
            }

            return false;
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
                if (AllowList.Contains(pd.Name))
                {
                    if (IsPresentOnAll(_items, pd.Name, attributes))
                    {
                        merged.Add(new MergedPropertyDescriptor(pd, _items, attributes));
                    }
                    continue;
                }

                if (IsMergeCompatibleOnAll(_items, pd, attributes))
                {
                    merged.Add(new MergedPropertyDescriptor(pd, _items, attributes));
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

        private static bool IsPresentOnAll(object[] items, string name, Attribute[] attributes)
        {
            var first = items[0];
            var firstPd = FindProperty(first, name, attributes);
            if (firstPd == null) return false;

            return items
                .Skip(1)
                .All(o =>
                {
                    var p = FindProperty(o, name, attributes);
                    return p != null && p.PropertyType == firstPd.PropertyType;
                });
        }

        private static bool IsMergeCompatibleOnAll(object[] items, PropertyDescriptor firstPd, Attribute[] attributes)
        {
            return items
                .Skip(1)
                .All(o =>
                {
                    var other = FindProperty(o, firstPd.Name, attributes);
                    return other != null && AreMergeCompatible(firstPd, other);
                });
        }

        private static PropertyDescriptor FindProperty(object instance, string name, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(instance, attributes, true).Find(name, false);
        }

        private static bool AreMergeCompatible(PropertyDescriptor first, PropertyDescriptor other)
        {
            if (first.PropertyType != other.PropertyType) return false;
            if (first.ComponentType != other.ComponentType) return false;
            if (first.IsReadOnly != other.IsReadOnly) return false;

            var a = first.Attributes;
            var b = other.Attributes;
            if (a.Count != b.Count) return false;

            foreach (Attribute attr in a)
            {
                var key = attr.TypeId as Type ?? attr.GetType();
                var otherAttr = b[key];
                if (otherAttr == null) return false;
                if (!attr.Equals(otherAttr)) return false;
            }

            return true;
        }
    }

    internal sealed class MergedPropertyDescriptor : PropertyDescriptor
    {
        private readonly PropertyDescriptor _inner;
        private readonly object[] _items;
        private readonly Attribute[] _attributes;
        private readonly MultiValueTypeConverter _converter;
        private bool _isMixed;

        public MergedPropertyDescriptor(PropertyDescriptor inner, object[] items, Attribute[] attributes)
            : base(inner)
        {
            _inner = inner;
            _items = items ?? Array.Empty<object>();
            _attributes = attributes;
            _converter = new MultiValueTypeConverter(inner.Converter, this);
        }

        public PropertyDescriptor InnerDescriptor => _inner;
        public bool IsMixed => _isMixed;

        public override Type ComponentType => _inner.ComponentType;
        public override Type PropertyType => _inner.PropertyType;
        public override bool IsReadOnly => _items.Any(i => GetDescriptor(i)?.IsReadOnly ?? true);
        public override TypeConverter Converter => _converter;

        public override bool CanResetValue(object component) => _inner.CanResetValue(component);

        public override object GetValue(object component)
        {
            if (_items.Length == 0)
            {
                _isMixed = false;
                return null;
            }

            var firstDescriptor = GetDescriptor(_items[0]);
            var firstValue = firstDescriptor?.GetValue(_items[0]);
            _isMixed = false;

            for (var i = 1; i < _items.Length; i++)
            {
                var d = GetDescriptor(_items[i]);
                var v = d?.GetValue(_items[i]);
                if (!Equals(firstValue, v))
                {
                    _isMixed = true;
                    break;
                }
            }

            return firstValue;
        }

        public override void ResetValue(object component)
        {
            foreach (var item in _items)
            {
                var d = GetDescriptor(item);
                d?.ResetValue(item);
            }
        }

        public override void SetValue(object component, object value)
        {
            foreach (var item in _items)
            {
                var d = GetDescriptor(item);
                d?.SetValue(item, value);
            }
        }

        public override bool ShouldSerializeValue(object component) => _inner.ShouldSerializeValue(component);

        public override object GetEditor(Type editorBaseType) => _inner.GetEditor(editorBaseType);

        private PropertyDescriptor GetDescriptor(object item)
        {
            return TypeDescriptor.GetProperties(item, _attributes, true).Find(_inner.Name, false);
        }
    }

    internal sealed class MultiValueTypeConverter : TypeConverter
    {
        private readonly TypeConverter _inner;
        private readonly MergedPropertyDescriptor _descriptor;

        public MultiValueTypeConverter(TypeConverter inner, MergedPropertyDescriptor descriptor)
        {
            _inner = inner ?? TypeDescriptor.GetConverter(descriptor.PropertyType);
            _descriptor = descriptor;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => _inner?.CanConvertTo(context, destinationType) ?? base.CanConvertTo(context, destinationType);

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => _inner?.CanConvertFrom(context, sourceType) ?? base.CanConvertFrom(context, sourceType);

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && _descriptor.IsMixed)
            {
                var innerType = _inner?.GetType();
                if (innerType != null &&
                    innerType.Name == "IndexerConverter" &&
                    innerType.Namespace == "TabularEditor.PropertyGridUI")
                {
                    return "Multiple objects selected.";
                }

                return "Multiple values";
            }

            if (_inner != null)
            {
                return _inner.ConvertTo(context, culture, value, destinationType);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            => _inner?.ConvertFrom(context, culture, value) ?? base.ConvertFrom(context, culture, value);

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            => _inner?.GetStandardValuesSupported(context) ?? base.GetStandardValuesSupported(context);

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            => _inner?.GetStandardValuesExclusive(context) ?? base.GetStandardValuesExclusive(context);

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            => _inner?.GetStandardValues(context) ?? base.GetStandardValues(context);
    }
}

using System;
using System.Reflection;
using Terminal.Gui;

namespace Guit
{
    class Binding
    {
        Lazy<PropertyInfo> property;

        public Binding(
            View control,
            object dataContext,
            string propertyName,
            Func<object, object> convertTo = null,
            Func<object, object> convertFrom = null)
        {
            Control = control;
            DataContext = dataContext;
            PropertyName = propertyName;
            ConvertTo = convertTo;
            ConvertFrom = convertFrom;

            property = new Lazy<PropertyInfo>(() => DataContext.GetType().GetProperty(PropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
        }

        public View Control { get; }

        public object DataContext { get; }

        public string PropertyName { get; }

        public Func<object, object> ConvertTo { get; }

        public Func<object, object> ConvertFrom { get; }

        PropertyInfo Property => property.Value;

        public void Register()
        {
            if (Control is Label)
                return;

            if (Control is TextField textField && textField != null)
                textField.Changed += (sender, e) => SetValue(textField.Text?.ToString());
            else if (Control is CheckBox checkBox && checkBox != null)
                checkBox.Toggled += (sender, e) => SetValue(checkBox.Checked);
            else if (Control is RadioGroup radioGroup && radioGroup != null)
                radioGroup.SelectionChanged = x => SetValue(x);
            else
                throw new NotSupportedException($"Control {Control.GetType().FullName} is not supported");
        }

        public void Run()
        {
            if (Control is TextField textField && textField != null)
                textField.Text = GetValue(string.Empty);
            else if (Control is CheckBox checkBox && checkBox != null)
                checkBox.Checked = GetValue(false);
            else if (Control is RadioGroup radioGroup && radioGroup != null)
                radioGroup.Selected = GetValue(0);
            else if (Control is Label label && label != null)
                label.Text = GetValue(string.Empty);
            else
                throw new NotSupportedException($"Control {Control.GetType().FullName} is not supported");
        }

        T GetValue<T>(T defaultValue)
        {
            var value = Property.GetValue(DataContext);

            if (ConvertTo != null)
                value = ConvertTo(value);

            if (value == null)
                value = defaultValue;

            return (T)value;
        }

        void SetValue(object value) =>
            Property.SetValue(DataContext, ConvertFrom != null ? ConvertFrom(value) : value);
    }
}
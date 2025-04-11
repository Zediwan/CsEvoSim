using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CsEvoSim.Core
{
    public enum SettingType
    {
        Boolean,
        Integer,
        Double,
        String,
        Enum
    }

    public class SystemSetting
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public SettingType Type { get; set; }
        public object Value { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
        public object StepValue { get; set; }
        public string Description { get; set; }
        public Action<object> OnValueChanged { get; set; }

        // Constructor for numeric settings
        public static SystemSetting CreateNumeric<T>(
            string name,
            string displayName,
            T value,
            T minValue,
            T maxValue,
            T stepValue,
            Action<T> onValueChanged,
            string description = null)
        {
            SettingType type = typeof(T) == typeof(int) ? SettingType.Integer : SettingType.Double;

            return new SystemSetting
            {
                Name = name,
                DisplayName = displayName,
                Type = type,
                Value = value,
                MinValue = minValue,
                MaxValue = maxValue,
                StepValue = stepValue,
                Description = description,
                OnValueChanged = v => onValueChanged((T)v)
            };
        }

        // Constructor for boolean settings
        public static SystemSetting CreateBoolean(
            string name,
            string displayName,
            bool value,
            Action<bool> onValueChanged,
            string description = null)
        {
            return new SystemSetting
            {
                Name = name,
                DisplayName = displayName,
                Type = SettingType.Boolean,
                Value = value,
                Description = description,
                OnValueChanged = v => onValueChanged((bool)v)
            };
        }
    }

    public interface ISystemWithSettings : ISystem
    {
        string SettingsGroupName { get; }
        IEnumerable<SystemSetting> GetSettings();
    }

    // Helper class to generate UI elements for settings
    public static class SettingsUIFactory
    {
        public static FrameworkElement CreateUIElement(SystemSetting setting)
        {
            return setting.Type switch
            {
                SettingType.Boolean => CreateBooleanControl(setting),
                SettingType.Integer => CreateNumericControl<int>(setting),
                SettingType.Double => CreateNumericControl<double>(setting),
                SettingType.String => CreateTextControl(setting),
                SettingType.Enum => CreateEnumControl(setting),
                _ => throw new NotSupportedException($"Setting type {setting.Type} is not supported"),
            };
        }

        private static FrameworkElement CreateBooleanControl(SystemSetting setting)
        {
            var checkBox = new CheckBox
            {
                Content = setting.DisplayName,
                IsChecked = (bool)setting.Value,
                Margin = new Thickness(5)
            };

            checkBox.Checked += (s, e) =>
            {
                setting.Value = true;
                setting.OnValueChanged?.Invoke(true);
            };

            checkBox.Unchecked += (s, e) =>
            {
                setting.Value = false;
                setting.OnValueChanged?.Invoke(false);
            };

            if (!string.IsNullOrEmpty(setting.Description))
                checkBox.ToolTip = setting.Description;

            return checkBox;
        }

        private static FrameworkElement CreateNumericControl<T>(SystemSetting setting) where T : struct, IComparable
        {
            var panel = new StackPanel { Margin = new Thickness(5) };

            // Setting name label
            panel.Children.Add(new TextBlock
            {
                Text = setting.DisplayName,
                Margin = new Thickness(0, 0, 0, 3),
                ToolTip = setting.Description
            });

            // Slider for numeric input
            var slider = new Slider
            {
                Minimum = Convert.ToDouble(setting.MinValue),
                Maximum = Convert.ToDouble(setting.MaxValue),
                Value = Convert.ToDouble(setting.Value),
                TickFrequency = Convert.ToDouble(setting.StepValue),
                IsSnapToTickEnabled = true,
                Width = 150,
                Margin = new Thickness(0, 0, 0, 3)
            };

            // Value display
            var valueText = new TextBlock
            {
                Text = FormatValue(setting.Value),
                Margin = new Thickness(0, 0, 0, 5)
            };

            slider.ValueChanged += (s, e) =>
            {
                T newValue;
                if (typeof(T) == typeof(int))
                    newValue = (T)(object)(int)Math.Round(e.NewValue);
                else
                    newValue = (T)(object)Math.Round(e.NewValue, 2);

                setting.Value = newValue;
                valueText.Text = FormatValue(newValue);
                setting.OnValueChanged?.Invoke(newValue);
            };

            panel.Children.Add(slider);
            panel.Children.Add(valueText);

            return panel;
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                int i => i.ToString(),
                double d => d.ToString("0.00"),
                float f => f.ToString("0.00"),
                _ => value.ToString()
            };
        }

        private static FrameworkElement CreateTextControl(SystemSetting setting)
        {
            var panel = new StackPanel { Margin = new Thickness(5) };

            panel.Children.Add(new TextBlock
            {
                Text = setting.DisplayName,
                Margin = new Thickness(0, 0, 0, 3),
                ToolTip = setting.Description
            });

            var textBox = new TextBox
            {
                Text = (string)setting.Value,
                Width = 150,
                Margin = new Thickness(0, 0, 0, 5)
            };

            textBox.TextChanged += (s, e) =>
            {
                setting.Value = textBox.Text;
                setting.OnValueChanged?.Invoke(textBox.Text);
            };

            panel.Children.Add(textBox);
            return panel;
        }

        private static FrameworkElement CreateEnumControl(SystemSetting setting)
        {
            var panel = new StackPanel { Margin = new Thickness(5) };

            panel.Children.Add(new TextBlock
            {
                Text = setting.DisplayName,
                Margin = new Thickness(0, 0, 0, 3),
                ToolTip = setting.Description
            });

            var comboBox = new ComboBox
            {
                Width = 150,
                Margin = new Thickness(0, 0, 0, 5)
            };

            Type enumType = setting.Value.GetType();
            foreach (var value in Enum.GetValues(enumType))
            {
                comboBox.Items.Add(value);
            }

            comboBox.SelectedItem = setting.Value;

            comboBox.SelectionChanged += (s, e) =>
            {
                setting.Value = comboBox.SelectedItem;
                setting.OnValueChanged?.Invoke(comboBox.SelectedItem);
            };

            panel.Children.Add(comboBox);
            return panel;
        }
    }
}

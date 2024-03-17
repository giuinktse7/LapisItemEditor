using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Utilities;

namespace LapisItemEditor.Controls
{
    public class NumberInputValueChangedEventArgs : RoutedEventArgs
    {
        public NumberInputValueChangedEventArgs(RoutedEvent routedEvent, uint oldValue, uint newValue) : base(routedEvent)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public uint OldValue { get; }
        public uint NewValue { get; }
    }

    /// <summary>
    /// Control that represents a TextBox with button spinners that allow incrementing and decrementing numeric values.
    /// </summary>
    public class NumberInput : TemplatedControl
    {
        /// <summary>
        /// Defines the <see cref="Maximum"/> property.
        /// </summary>
        public static readonly StyledProperty<uint> MaximumProperty =
            AvaloniaProperty.Register<NumberInput, uint>(nameof(Maximum), uint.MaxValue, coerce: OnCoerceMaximum);

        /// <summary>
        /// Defines the <see cref="Minimum"/> property.
        /// </summary>
        public static readonly StyledProperty<uint> MinimumProperty =
            AvaloniaProperty.Register<NumberInput, uint>(nameof(Minimum), uint.MinValue, coerce: OnCoerceMinimum);

        /// <summary>
        /// Defines the <see cref="Text"/> property.
        /// </summary>
        public static readonly DirectProperty<NumberInput, string> TextProperty =
            AvaloniaProperty.RegisterDirect<NumberInput, string>(nameof(Text), o => o.Text, (o, v) => o.Text = v,
                defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

        /// <summary>
        /// Defines the <see cref="Value"/> property.
        /// </summary>
        public static readonly DirectProperty<NumberInput, uint> ValueProperty =
            AvaloniaProperty.RegisterDirect<NumberInput, uint>(nameof(Value), updown => updown.Value,
                (updown, v) => updown.Value = v, defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

        /// <summary>
        /// Defines the <see cref="Watermark"/> property.
        /// </summary>
        public static readonly StyledProperty<string> WatermarkProperty =
            AvaloniaProperty.Register<NumericUpDown, string>(nameof(Watermark));

        /// <summary>
        /// Defines the <see cref="HorizontalContentAlignment"/> property.
        /// </summary>
        public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
            ContentControl.HorizontalContentAlignmentProperty.AddOwner<NumberInput>();

        /// <summary>
        /// Defines the <see cref="VerticalContentAlignment"/> property.
        /// </summary>
        public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
            ContentControl.VerticalContentAlignmentProperty.AddOwner<NumberInput>();

        private IDisposable _textBoxTextChangedSubscription;

        private uint _value;
        private string _text;
        private bool _internalValueSet;
        private bool _clipValueToMinMax;
        private bool _isSyncingTextAndValueProperties;
        private bool _isTextChangedFromUI;

        /// <summary>
        /// Gets the TextBox template part.
        /// </summary>
        private TextBox TextBox { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed value.
        /// </summary>
        public uint Maximum
        {
            get { return GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Gets or sets the minimum allowed value.
        /// </summary>
        public uint Minimum
        {
            get { return GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Gets or sets the formatted string representation of the value.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { SetAndRaise(TextProperty, ref _text, value); }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public uint Value
        {
            get { return _value; }
            set
            {
                value = OnCoerceValue(value);
                SetAndRaise(ValueProperty, ref _value, value);
            }
        }

        /// <summary>
        /// Gets or sets the object to use as a watermark if the <see cref="Value"/> is null.
        /// </summary>
        public string Watermark
        {
            get { return GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }


        /// <summary>
        /// Gets or sets the horizontal alignment of the content within the control.
        /// </summary>
        public HorizontalAlignment HorizontalContentAlignment
        {
            get => GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the vertical alignment of the content within the control.
        /// </summary>
        public VerticalAlignment VerticalContentAlignment
        {
            get => GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }

        /// <summary>
        /// Initializes new instance of <see cref="NumberInput"/> class.
        /// </summary>
        public NumberInput()
        {
            Initialized += (sender, e) =>
            {
                if (!_internalValueSet && IsInitialized)
                {
                    SyncTextAndValueProperties(false, null, true);
                }

            };
        }

        /// <summary>
        /// Initializes static members of the <see cref="NumberInput"/> class.
        /// </summary>
        static NumberInput()
        {
            MaximumProperty.Changed.Subscribe(OnMaximumChanged);
            MinimumProperty.Changed.Subscribe(OnMinimumChanged);
            TextProperty.Changed.Subscribe(OnTextChanged);
            ValueProperty.Changed.Subscribe(OnValueChanged);
        }

        /// <inheritdoc />
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            CommitInput();
            base.OnLostFocus(e);
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            if (TextBox != null)
            {
                _textBoxTextChangedSubscription?.Dispose();
            }
            TextBox = e.NameScope.Find<TextBox>("PART_TextBox");
            if (TextBox != null)
            {
                TextBox.AddHandler(InputElement.TextInputEvent, OnPreviewTextInput, RoutingStrategies.Tunnel);
                TextBox.Text = Text;
                _textBoxTextChangedSubscription = TextBox.GetObservable(TextBox.TextProperty)
                .Subscribe(txt => TextBoxOnTextChanged());
            }
        }

        void OnPreviewTextInput(object? sender, RoutedEventArgs e)
        {
            Debug.Assert(e is Avalonia.Input.TextInputEventArgs);
            var ev = (Avalonia.Input.TextInputEventArgs)e;

            // Only allow unsigned integers
            ev.Handled = !UInt32.TryParse(ev.Text, out uint value);
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    var commitSuccess = CommitInput();
                    e.Handled = !commitSuccess;
                    break;
            }
        }

        protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
        {
            base.UpdateDataValidation(property, state, error);
        }

        /// <summary>
        /// Called when the <see cref="Maximum"/> property value changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnMaximumChanged(uint oldValue, uint newValue)
        {
            Value = (uint)MathUtilities.Clamp((float)Value, Minimum, Maximum);
        }

        /// <summary>
        /// Called when the <see cref="Minimum"/> property value changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnMinimumChanged(uint oldValue, uint newValue)
        {
            Value = (uint)MathUtilities.Clamp((float)Value, Minimum, Maximum);
        }

        /// <summary>
        /// Called when the <see cref="Text"/> property value changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnTextChanged(string oldValue, string newValue)
        {
            if (IsInitialized)
            {
                SyncTextAndValueProperties(true, Text);
            }
        }

        /// <summary>
        /// Called when the <see cref="Value"/> property value changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnValueChanged(uint oldValue, uint newValue)
        {
            if (!_internalValueSet && IsInitialized)
            {
                SyncTextAndValueProperties(false, null, true);
            }

            RaiseValueChangedEvent(oldValue, newValue);
        }

        /// <summary>
        /// Called when the <see cref="Maximum"/> property has to be coerced.
        /// </summary>
        /// <param name="baseValue">The value.</param>
        protected virtual uint OnCoerceMaximum(uint baseValue)
        {
            return Math.Max(baseValue, Minimum);
        }

        /// <summary>
        /// Called when the <see cref="Minimum"/> property has to be coerced.
        /// </summary>
        /// <param name="baseValue">The value.</param>
        protected virtual uint OnCoerceMinimum(uint baseValue)
        {
            return Math.Min(baseValue, Maximum);
        }

        /// <summary>
        /// Called when the <see cref="Value"/> property has to be coerced.
        /// </summary>
        /// <param name="baseValue">The value.</param>
        protected virtual uint OnCoerceValue(uint baseValue)
        {
            return baseValue;
        }

        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void RaiseValueChangedEvent(uint oldValue, uint newValue)
        {
            var e = new NumberInputValueChangedEventArgs(ValueChangedEvent, oldValue, newValue);
            RaiseEvent(e);
        }

        /// <summary>
        /// Converts the formatted text to a value.
        /// </summary>
        private uint ConvertTextToValue(string text)
        {
            uint result = 0;

            if (string.IsNullOrEmpty(text))
            {
                return result;
            }

            // Since the conversion from Value to text using a FormatString may not be parsable,
            // we verify that the already existing text is not the exact same value.
            var currentValueText = ConvertValueToText();
            if (Equals(currentValueText, text))
            {
                return Value;
            }

            result = ConvertTextToValueCore(currentValueText, text);

            return (uint)MathUtilities.Clamp((float)result, Minimum, Maximum);
        }

        /// <summary>
        /// Converts the value to formatted text.
        /// </summary>
        /// <returns></returns>
        private string ConvertValueToText()
        {
            //Manage FormatString of type "{}{0:N2} °" (in xaml) or "{0:N2} °" in code-behind.
            return Value.ToString();
        }




        /// <summary>
        /// Called when the <see cref="Maximum"/> property value changed.
        /// </summary>
        /// <param name="e">The event args.</param>
        private static void OnMaximumChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is NumberInput upDown)
            {
                var oldValue = (uint)e.OldValue;
                var newValue = (uint)e.NewValue;
                upDown.OnMaximumChanged(oldValue, newValue);
            }
        }

        /// <summary>
        /// Called when the <see cref="Minimum"/> property value changed.
        /// </summary>
        /// <param name="e">The event args.</param>
        private static void OnMinimumChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is NumberInput upDown)
            {
                var oldValue = (uint)e.OldValue;
                var newValue = (uint)e.NewValue;
                upDown.OnMinimumChanged(oldValue, newValue);
            }
        }

        /// <summary>
        /// Called when the <see cref="Text"/> property value changed.
        /// </summary>
        /// <param name="e">The event args.</param>
        private static void OnTextChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is NumberInput input)
            {
                var oldValue = (string)e.OldValue;
                var newValue = (string)e.NewValue;
                input.OnTextChanged(oldValue, newValue);
            }
        }

        /// <summary>
        /// Called when the <see cref="Value"/> property value changed.
        /// </summary>
        /// <param name="e">The event args.</param>
        private static void OnValueChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is NumberInput upDown)
            {
                var oldValue = (uint)e.OldValue;
                var newValue = (uint)e.NewValue;
                upDown.OnValueChanged(oldValue, newValue);
            }
        }

        private void SetValueInternal(uint value)
        {
            _internalValueSet = true;
            try
            {
                Value = value;
            }
            finally
            {
                _internalValueSet = false;
            }
        }

        private static uint OnCoerceMaximum(AvaloniaObject instance, uint value)
        {
            if (instance is NumberInput upDown)
            {
                return upDown.OnCoerceMaximum(value);
            }

            return value;
        }

        private static uint OnCoerceMinimum(AvaloniaObject instance, uint value)
        {
            if (instance is NumberInput upDown)
            {
                return upDown.OnCoerceMinimum(value);
            }

            return value;
        }

        private void TextBoxOnTextChanged()
        {
            try
            {
                _isTextChangedFromUI = true;
                if (TextBox != null)
                {
                    Text = TextBox.Text;
                }
            }
            finally
            {
                _isTextChangedFromUI = false;
            }
        }

        /// <summary>
        /// Defines the <see cref="ValueChanged"/> event.
        /// </summary>
        public static readonly RoutedEvent<NumberInputValueChangedEventArgs> ValueChangedEvent =
            RoutedEvent.Register<NumberInput, NumberInputValueChangedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);

        /// <summary>
        /// Raised when the <see cref="Value"/> changes.
        /// </summary>
        public event EventHandler<NumberInputValueChangedEventArgs> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        private bool CommitInput()
        {
            return SyncTextAndValueProperties(true, Text);
        }

        /// <summary>
        /// Synchronize <see cref="Text"/> and <see cref="Value"/> properties.
        /// </summary>
        /// <param name="updateValueFromText">If value should be updated from text.</param>
        /// <param name="text">The text.</param>
        private bool SyncTextAndValueProperties(bool updateValueFromText, string text)
        {
            return SyncTextAndValueProperties(updateValueFromText, text, false);
        }

        /// <summary>
        /// Synchronize <see cref="Text"/> and <see cref="Value"/> properties.
        /// </summary>
        /// <param name="updateValueFromText">If value should be updated from text.</param>
        /// <param name="text">The text.</param>
        /// <param name="forceTextUpdate">Force text update.</param>
        private bool SyncTextAndValueProperties(bool updateValueFromText, string text, bool forceTextUpdate)
        {
            if (_isSyncingTextAndValueProperties)
                return true;

            _isSyncingTextAndValueProperties = true;
            var parsedTextIsValid = true;
            try
            {
                if (updateValueFromText)
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        try
                        {
                            var newValue = ConvertTextToValue(text);
                            if (!Equals(newValue, Value))
                            {
                                SetValueInternal(newValue);
                            }
                        }
                        catch
                        {
                            parsedTextIsValid = false;
                        }
                    }
                }

                // Do not touch the ongoing text input from user.
                if (!_isTextChangedFromUI)
                {
                    var keepEmpty = !forceTextUpdate && string.IsNullOrEmpty(Text);
                    if (!keepEmpty)
                    {
                        var newText = ConvertValueToText();
                        if (!Equals(Text, newText))
                        {
                            Text = newText;
                        }
                    }

                    // Sync Text and textBox
                    if (TextBox != null)
                    {
                        TextBox.Text = Text;
                    }
                }
            }
            finally
            {
                _isSyncingTextAndValueProperties = false;
            }
            return parsedTextIsValid;
        }

        private uint ConvertTextToValueCore(string currentValueText, string text)
        {
            if (UInt32.TryParse(text, out uint value))
            {
                return value;
            }

            return Value;
        }

        private void ValidateMinMax(uint value)
        {
            if (value < Minimum)
            {
                throw new ArgumentOutOfRangeException(nameof(value), string.Format("Value must be greater than Minimum value of {0}", Minimum));
            }
            else if (value > Maximum)
            {
                throw new ArgumentOutOfRangeException(nameof(value), string.Format("Value must be less than Maximum value of {0}", Maximum));
            }
        }
    }
}

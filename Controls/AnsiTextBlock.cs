using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Shoebill.Helpers;

namespace Shoebill.Controls;

public class AnsiTextBlock : TextBlock
{
    public static readonly StyledProperty<string> AnsiTextProperty =
        AvaloniaProperty
            .Register<AnsiTextBlock, string>(nameof(AnsiText), defaultBindingMode: BindingMode.OneWay);

    public AnsiTextBlock()
    {
        AnsiTextProperty.Changed.Subscribe(UpdateText);
    }

    public string AnsiText
    {
        get => GetValue(AnsiTextProperty);
        set => SetValue(AnsiTextProperty, value);
    }

    private void UpdateText(AvaloniaPropertyChangedEventArgs<string> text)
    {
        Inlines?.Clear();
        if (string.IsNullOrEmpty(text.NewValue.Value))
            return;
        var segments = AnsiHelper.Parse(text.NewValue.Value);
        foreach (var segment in segments)
        {
            Inlines ??= new InlineCollection();
            Inlines.Add(new Run(
                segment.Text) { Foreground = segment.Foreground });
        }
    }
}
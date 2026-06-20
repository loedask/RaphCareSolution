namespace RaphCare.Mobile.Core.Shared.Controls;

public partial class IconButton : Button
{
    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(IconButton), string.Empty);

    public string Glyph { get => (string)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }

    public IconButton()
    {
        InitializeComponent();
    }
}

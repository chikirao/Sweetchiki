using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Sweetch.App.Wheel;
using Sweetch.Core;
using static Sweetch.App.UI.Controls;
using static Sweetch.Core.Loc;

namespace Sweetch.App.UI;

/// <summary>
/// The Wheels page: pick a wheel and a file type, see that wheel as it will open, star favourites
/// (they always come first, so your hand learns where to drop) and hide slices you never use.
/// </summary>
internal sealed partial class SettingsWindow
{
    private WheelKind _wheel = WheelKind.Convert;
    private int _group;

    /// <summary>File types shown as groups, with sample names used to find which slices apply.</summary>
    internal static (string English, string Russian, string Glyph, string[] Samples)[] Groups =>
    [
        // Two formats per group, so "convert to X" shows up even when one sample already is X.
        ("Images", "Изображения", Glyphs.Photo, ["a.png", "a.jpg", "a.gif"]),
        ("Video", "Видео", Glyphs.Video, ["a.mp4", "a.mov"]),
        ("Audio", "Аудио", Glyphs.Audio, ["a.mp3", "a.wav"]),
        ("PDF", "PDF", Glyphs.Page, ["a.pdf"]),
        ("Documents", "Документы", Glyphs.Document, ["a.docx", "a.md"]),
        ("Slides", "Презентации", Glyphs.Page, ["a.pptx", "a.odp"]),
        ("Spreadsheets", "Таблицы", Glyphs.Table, ["a.xlsx", "a.csv"]),
        ("Subtitles", "Субтитры", Glyphs.Subtitles, ["a.srt", "a.vtt"]),
        ("Archives", "Архивы", Glyphs.Pack, ["a.zip", "a.tar"]),
    ];

    /// <summary>Every group with its slices; slices that work for anything (archive, actions) form the last group.</summary>
    private static List<(string Name, string Glyph, List<Operation> Ops)> GroupedOperations() => Group(Operations?.Invoke() ?? []);

    /// <summary>Splits operations into the file-type groups (each operation once, in the first group it fits).</summary>
    internal static List<(string Name, string Glyph, List<Operation> Ops)> Group(IReadOnlyList<Operation> all)
    {
        var folder = new FileItem(Path.GetTempPath());
        var shown = new HashSet<string>();
        var groups = new List<(string, string, List<Operation>)>();
        foreach (var (english, russian, glyph, samples) in Groups)
        {
            var items = samples.Select(s => new FileItem(s)).ToList();
            var ops = all.Where(o => o.Wheel != WheelKind.Actions && !o.AppliesTo([folder]) && Applies(o, items) && shown.Add(o.Id)).ToList();
            groups.Add((T(english, russian), glyph, ops));
        }
        groups.Add((T("Anything", "Любые файлы"), Glyphs.Folder, all.Where(o => shown.Add(o.Id)).ToList()));
        return groups;
    }

    internal static bool Applies(Operation op, IReadOnlyList<FileItem> samples) =>
        samples.Any(s => op.AppliesTo([s]) || op.AppliesTo([s, s with { Path = "b" + Path.GetExtension(s.Path) }]));

    /// <summary>Shows the wheel page for one wheel and file type (used by the self-test).</summary>
    internal void ShowWheel(WheelKind wheel, int group)
    {
        _wheel = wheel;
        _group = group;
        Page = SettingsPage.Wheels;
    }

    private void WheelsPage(Panel root)
    {
        PageHeader(root, PageName(SettingsPage.Wheels),
            T("Star your favourite slices: they always come first, in the same place. Use the switch to hide a slice.", "Отметьте избранные секторы: они всегда будут первыми, на одном месте. Переключатель скрывает сектор."));

        var top = new DockPanel { Margin = new Thickness(0, 14, 0, 18), LastChildFill = false };
        var wheels = Segmented(
            [.. new[] { WheelKind.Convert, WheelKind.Tools, WheelKind.Actions }.Select(w =>
                (_settings.KeyFor(w) == Input.ModifierKey.None ? AppSettings.WheelName(w) : $"{AppSettings.WheelName(w)} · {AppSettings.KeyName(_settings.KeyFor(w))}", w))],
            _wheel,
            w =>
            {
                _wheel = w;
                _group = 0;
                Rebuild();
            });
        top.Children.Add(wheels);
        var reset = Button(T("Reset to default", "Сбросить настройки"), () =>
        {
            _settings.HiddenOperations.Clear();
            _settings.FavoriteOperations.Clear();
            Save();
            Rebuild();
        }, glyph: Glyphs.Reset);
        DockPanel.SetDock(reset, Dock.Right);
        top.Children.Add(reset);
        root.Children.Add(top);

        var groups = GroupedOperations()
            .Select(g => (g.Name, g.Glyph, Ops: g.Ops.Where(o => o.Wheel == _wheel).ToList()))
            .Where(g => g.Ops.Count > 0)
            .ToList();
        if (groups.Count == 0)
            return;
        _group = Math.Clamp(_group, 0, groups.Count - 1);
        var (name, _, ops) = groups[_group];
        var visible = WheelLayout.Arrange(ops, _settings.HiddenOperations, _settings.FavoriteOperations);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(210) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 300 });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 280 });

        // 1. file types
        var list = new StackPanel();
        list.Children.Add(ColumnLabel(T("1. File type", "1. Тип файла")));
        for (var i = 0; i < groups.Count; i++)
        {
            var g = groups[i];
            var shown = WheelLayout.Arrange(g.Ops, _settings.HiddenOperations, _settings.FavoriteOperations).Count;
            list.Children.Add(GroupItem(g.Name, g.Glyph, $"{shown}", i));
        }
        grid.Children.Add(list);

        // 2. the wheel as it will open
        var middle = new StackPanel();
        middle.Children.Add(ColumnLabel(T("2. How the wheel opens", "2. Способ открытия меню")));
        var key = _settings.KeyFor(_wheel);
        var keyName = key == Input.ModifierKey.None ? AppSettings.WheelName(_wheel) : AppSettings.KeyName(key);
        var preview = WheelWindow.Preview(visible, key, _wheel, keyName, name, $"{visible.Count} / {ops.Count}", _settings.FavoriteOperations.ToHashSet());
        preview.MaxWidth = 420;
        middle.Children.Add(new Border
        {
            Background = Theme.Card,
            CornerRadius = new CornerRadius(CardRadius),
            Padding = new Thickness(10),
            Child = preview,
        });
        var hint = Text(T("Favourite slices are highlighted. Tap the same key again (or press R) to repeat the last action.", "Избранные секторы выделены. Нажмите ту же клавишу ещё раз (или R), чтобы повторить последнее действие."), 12, brush: Theme.Muted);
        hint.Margin = new Thickness(4, 10, 4, 0);
        middle.Children.Add(hint);
        Grid.SetColumn(middle, 2);
        grid.Children.Add(middle);

        // 3. slices
        var right = new StackPanel();
        right.Children.Add(ColumnLabel(T("3. Slices", "3. Секторы")));
        var rows = new StackPanel();
        foreach (var op in WheelLayout.Arrange(ops, [], _settings.FavoriteOperations))
            rows.Children.Add(SliceRow(op, rows.Children.Count > 0));
        right.Children.Add(new Border
        {
            Background = Theme.Card,
            CornerRadius = new CornerRadius(CardRadius),
            Padding = new Thickness(8, 6, 8, 6),
            Child = rows,
        });
        Grid.SetColumn(right, 4);
        grid.Children.Add(right);

        root.Children.Add(grid);
    }

    private static TextBlock ColumnLabel(string text)
    {
        var label = Text(text, 13, brush: Theme.Muted);
        label.Margin = new Thickness(4, 0, 0, 10);
        return label;
    }

    private Border GroupItem(string name, string glyph, string count, int index)
    {
        var active = index == _group;
        var row = new DockPanel();
        var badge = new Border
        {
            CornerRadius = new CornerRadius(9),
            MinWidth = 26,
            Padding = new Thickness(7, 1, 7, 2),
            Background = active ? Theme.Ink : Theme.Track,
            Child = new TextBlock
            {
                Text = count,
                FontSize = 11.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = active ? Theme.OnInk : Theme.Muted,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
            VerticalAlignment = VerticalAlignment.Center,
        };
        DockPanel.SetDock(badge, Dock.Right);
        row.Children.Add(badge);
        var icon = new TextBlock
        {
            Text = glyph,
            FontFamily = Theme.Icons,
            FontSize = 15,
            Foreground = active ? Theme.Ink : Theme.Muted,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0),
        };
        DockPanel.SetDock(icon, Dock.Left);
        row.Children.Add(icon);
        row.Children.Add(Text(name, 14, active ? FontWeights.SemiBold : FontWeights.Normal, wrap: false));
        var item = new Border
        {
            Background = active ? Theme.Selected : Brushes.Transparent,
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(12, 9, 10, 9),
            Margin = new Thickness(0, 0, 0, 2),
            Cursor = Cursors.Hand,
            Child = row,
        };
        if (!active)
        {
            item.MouseEnter += (_, _) => item.Background = Theme.Hover;
            item.MouseLeave += (_, _) => item.Background = Brushes.Transparent;
        }
        item.MouseLeftButtonUp += (_, _) =>
        {
            _group = index;
            Rebuild();
        };
        return item;
    }

    private UIElement SliceRow(Operation op, bool separator)
    {
        var hidden = _settings.HiddenOperations.Contains(op.Id);
        var favorite = _settings.FavoriteOperations.Contains(op.Id);

        var star = new TextBlock
        {
            Text = favorite ? Glyphs.Star : Glyphs.Star,
            FontFamily = Theme.Icons,
            FontSize = 20,
            Foreground = favorite ? Theme.Ink : Theme.Muted,
            Opacity = favorite ? 1 : 0.35,
            Padding = new Thickness(8),
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center,
            ToolTip = T("Favourite – always first", "Избранное — всегда первым"),
        };
        star.MouseLeftButtonUp += (_, _) =>
        {
            if (!_settings.FavoriteOperations.Remove(op.Id))
            {
                _settings.FavoriteOperations.Add(op.Id);
                _settings.HiddenOperations.Remove(op.Id);
            }
            Save();
            Rebuild();
        };

        var toggle = Toggle(!hidden, visible =>
        {
            if (visible)
            {
                _settings.HiddenOperations.Remove(op.Id);
            }
            else if (!_settings.HiddenOperations.Contains(op.Id))
            {
                _settings.HiddenOperations.Add(op.Id);
                _settings.FavoriteOperations.Remove(op.Id);
            }
            Save();
            // Let the switch finish its animation before the wheel redraws.
            Dispatcher.BeginInvoke(Rebuild, System.Windows.Threading.DispatcherPriority.Background);
        });
        toggle.Margin = new Thickness(6, 0, 8, 0);
        toggle.VerticalAlignment = VerticalAlignment.Center;

        var isFormat = op.Wheel == WheelKind.Convert && op.Label.Length <= 6 && op.Label.ToUpperInvariant() == op.Label;
        var icon = new TextBlock
        {
            Text = isFormat ? Glyphs.Convert : op.Glyph,
            FontFamily = Theme.Icons,
            FontSize = 16,
            Foreground = Theme.Text,
            Width = 28,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var label = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        label.Children.Add(Text(op.Label, 14, isFormat ? FontWeights.SemiBold : FontWeights.Normal, wrap: false));
        if (!string.IsNullOrEmpty(op.Subtitle))
            label.Children.Add(Text(op.Subtitle, 11.5, brush: Theme.Muted, wrap: false));

        var row = new DockPanel { Margin = new Thickness(10, 5, 2, 5), Opacity = hidden ? 0.5 : 1 };
        DockPanel.SetDock(icon, Dock.Left);
        DockPanel.SetDock(toggle, Dock.Right);
        DockPanel.SetDock(star, Dock.Right);
        row.Children.Add(icon);
        row.Children.Add(toggle);
        row.Children.Add(star);
        row.Children.Add(label);
        return new Border
        {
            BorderBrush = Theme.PanelBorder,
            BorderThickness = new Thickness(0, separator ? 1 : 0, 0, 0),
            Child = row,
        };
    }
}

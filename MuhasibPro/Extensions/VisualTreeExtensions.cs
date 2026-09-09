using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace MuhasibPro.Extensions;

public static class VisualTreeExtensions
{
    public static T? FindVisualParent<T>(this DependencyObject child) where T : class
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is T typed)
                return typed;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}

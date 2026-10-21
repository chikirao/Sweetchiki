using System.Runtime.InteropServices;
using Sweetch.App.Native;

namespace Sweetch.App.Input;

/// <summary>
/// Tells something being carried – a drag &amp; drop – from the mouse merely being held down.
/// <para>
/// The gesture is "drag a file, tap a key", but a low-level mouse hook sees only a button and a distance:
/// a brush stroke, a selection rectangle, a scrollbar and a window border look exactly the same, and drawing
/// apps keep a modifier key busy the whole time, so the wheel used to open in the middle of a stroke.
/// </para>
/// <para>
/// Who owns the mouse is what differs. OLE's <c>DoDragDrop</c> – Explorer, the desktop, browsers and any
/// WPF or WinForms app carrying a file, a picture or a link – takes the capture for a window of its own,
/// because the drag loop, not the app, handles the mouse. An app that is still using the mouse itself
/// captures the window it is drawing into, or its parent, so the capture stays in the same window tree as
/// the pointer. Capturing the window under the pointer therefore means "the app is working"; capturing
/// anything else means "something is being carried".
/// </para>
/// </summary>
internal static class DragSession
{
    /// <summary>The thread is moving a window or has a menu open: the mouse is busy with the window manager.</summary>
    private const uint Busy = Win32.GUI_INMOVESIZE | Win32.GUI_INMENUMODE | Win32.GUI_SYSTEMMENUMODE | Win32.GUI_POPUPMENUMODE;

    /// <summary>
    /// Whether a drag &amp; drop is under way at this screen point. Asked of the foreground thread: a drag
    /// keeps its source in front, while the wheel it opens is shown without taking focus.
    /// </summary>
    public static bool IsCarrying(Win32.POINT at)
    {
        var thread = Win32.GetWindowThreadProcessId(Win32.GetForegroundWindow(), IntPtr.Zero);
        if (thread == 0)
            return false;
        var info = new Win32.GUITHREADINFO { cbSize = Marshal.SizeOf<Win32.GUITHREADINFO>() };
        if (!Win32.GetGUIThreadInfo(thread, ref info))
            return false;
        return Carries(info.flags, Win32.GetAncestor(info.hwndCapture, Win32.GA_ROOT), Win32.GetAncestor(Win32.WindowFromPoint(at), Win32.GA_ROOT));
    }

    /// <summary>
    /// The decision on its own, apart from Win32, so the cases it has to tell apart can be checked: a capture
    /// that is not the window under the pointer, on a thread that is not moving a window or showing a menu.
    /// </summary>
    internal static bool Carries(uint threadFlags, IntPtr captureRoot, IntPtr pointedRoot) =>
        captureRoot != IntPtr.Zero && pointedRoot != IntPtr.Zero && captureRoot != pointedRoot && (threadFlags & Busy) == 0;
}

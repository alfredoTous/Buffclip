using static X11;

class HotkeyManager
{
    // Simulates key strokes for pasting
    public static void SimulateCtrlShiftV(IntPtr dpy)
    { 
        uint ctrl  = (uint)XKeysymToKeycode(dpy, XK_Control_L);
        uint shift = (uint)XKeysymToKeycode(dpy, XK_Shift_L);
        uint v     = (uint)XKeysymToKeycode(dpy, XK_V);

        XTestFakeKeyEvent(dpy, ctrl, true, 0);   // Ctrl Down
        XTestFakeKeyEvent(dpy, shift, true, 0);  // Shift Down
        XTestFakeKeyEvent(dpy, v, true, 0);      // V Down

        XTestFakeKeyEvent(dpy, v, false, 0);     // V Up
        XTestFakeKeyEvent(dpy, shift, false, 0); // Shift Up
        XTestFakeKeyEvent(dpy, ctrl, false, 0);  // Ctrl Up

        XFlush(dpy);
    }

}

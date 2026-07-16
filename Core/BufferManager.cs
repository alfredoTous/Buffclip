using static X11;


class Buffers
{
    public int NumberOfBuffers;
    public string[] buffers;
    private ClipboardManager clip;

    // Constructor
    public Buffers(int NumberOfBuffers)
    {
        this.NumberOfBuffers = NumberOfBuffers;
        this.buffers = new string[this.NumberOfBuffers];
        this.clip = new ClipboardManager();
    }


    // Get content of PRIMARY selection and save it into indicated buffer
    public void CopyToBuffer(int id_buf)
    {
        string content = this.clip.GetClipBoardContent("PRIMARY");
        this.buffers[id_buf] = content;
    }
    
    // Paste the content of the indicated buffer
    public void PasteFromBuffer(IntPtr dpy, int id_buf)
    {
        // Temporarily save content of CLIPBOARD
        string content = this.clip.GetClipBoardContent("CLIPBOARD");

        // Change content of CLIPBOARD to buffer content
        this.clip.SetClipboardContent("CLIPBOARD", buffers[id_buf]);

        // SimulateCtrlShiftV KeyPress
        this.SimulateCtrlShiftV(dpy);
        Thread.Sleep(20);
        // Return original value
        this.clip.SetClipboardContent("CLIPBOARD", content);
    }


    // Simulates key strokes for pasting
    private void SimulateCtrlShiftV(IntPtr dpy)
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


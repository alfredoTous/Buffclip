using static X11;
using System.Diagnostics;


class HotkeyManager
{

    static IntPtr display = IntPtr.Zero;        // Connection to X server
    static ClipboardManager clipboard = null!;  // ClipboardManager

    // ==== virt key values ====
    static int f1_keycode;
    static int f2_keycode;
    static int f3_keycode;
    static int f4_keycode;
    // =========================

    // =============== auto repeat ===================
    private static KeyAutoRepeat f2AutoRepeat = null!;
    private static KeyAutoRepeat f4AutoRepeat = null!;
    // ===============================================


    // Main Thread
    public static void ListenForKeyPress(NetworkManager network)
    {
        // Manage key press and selection events with X11 APIs

        display = XOpenDisplay(IntPtr.Zero); // Open connection to the X11 server
        if (display == IntPtr.Zero)
            throw new Exception("[-] Error XOpenDisplay failed");


        // ===================== CLIPBOARD =================================================================
        clipboard = new ClipboardManager(display);
        // Subscribe to event
        // This event is trigerred on F1/F3 keypresses by clipboard.HandleSelectionNotify
        clipboard.PrimaryContentReceived += (bufferId, content) =>
        {
            if (!Globals.BuffersManager.IsDifferentContent(content, bufferId)) return; // Avoid actions if content is the same

            Globals.BuffersManager.SetBuf(bufferId, content);
            if (network.IsConnected)
                network.SendUpdateBuffer(bufferId);
        };
        // =================================================================================================

        IntPtr rootWindow = XDefaultRootWindow(display); // Get root window, needed for Global Grab of F1/F2/F3/F4 keys

        f1_keycode = XKeysymToKeycode(display, XK_F1); // Translate from logic virtual key (F1) value to physical keycode
        f2_keycode = XKeysymToKeycode(display, XK_F2); // Translate from logic virtual key (F2) value to physical keycode
        f3_keycode = XKeysymToKeycode(display, XK_F3); // Translate from logic virtual key (F3) value to physical keycode
        f4_keycode = XKeysymToKeycode(display, XK_F4); // Translate from logic virtual key (F4) value to physical keycode

        f2AutoRepeat = new KeyAutoRepeat(f2_keycode, 1, "F2");
        f4AutoRepeat = new KeyAutoRepeat(f4_keycode, 2, "F4");


        // Used to grab combinations of keys such as {key}+CapsLock, etc
        uint[] lockCombos = {
            0,
            LockMask,
            Mod2Mask,
            LockMask | Mod2Mask,
        };
        
        // Keys to grab globally
        int[] used_keys = {
            f1_keycode,
            f2_keycode,
            f3_keycode,
            f4_keycode
        };
       
        // Grab
        foreach (int key in used_keys) {
            foreach (uint mods in lockCombos) {
                XGrabKey(display, key, mods, rootWindow, true, GrabModeAsync, GrabModeAsync);
            }
        }
        XSync(display, false);

        Console.WriteLine("[i] Esperando F1...");
        XEvent ev;
        try {
            // Main loop
            while (true) {
                if (XPending(display) > 0)
                {
                    XNextEvent(display, out ev);
                    HandleXEvent(ev);
                }
                else
                {
                    f2AutoRepeat.ProcessMainLoop(display);
                    f4AutoRepeat.ProcessMainLoop(display);
                    System.Threading.Thread.Sleep(10);
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
        }
    }


    public static void HandleXEvent(XEvent ev)
    {
        switch (ev.type)
        {
            case KeyPress:
            {
                // ========== Check for combo F1+F2+F3+F4 ==========
                UpdateKeyState(ev.xkey, true);

                if (KeyState.CheckCombo())
                {
                    Console.WriteLine("TOGGLE!");
                    return;
                }
                // =================================================


                // ========== Check which key wass pressed =========
                // Buffer 1
                if (ev.xkey.keycode == f1_keycode) 
                { 
                    // Triggers SelectionNotify event (XConvertSelection Api)
                    clipboard.RequestSelectionContent("PRIMARY", 1);
                }
                else if (ev.xkey.keycode == f3_keycode) 
                {
                    clipboard.RequestSelectionContent("PRIMARY", 2);
                }
                else
                {
                    if (f2AutoRepeat.HandleKeyPress(ev.xkey)) return;
                    if (f4AutoRepeat.HandleKeyPress(ev.xkey)) return;
                }
                
                break;
            }

            case KeyRelease:
            {
                if (f2AutoRepeat.HandleKeyRelease(ev.xkey)) return;
                if (f4AutoRepeat.HandleKeyRelease(ev.xkey)) return;

                UpdateKeyState(ev.xkey, false);
                break;
            }

            case SelectionRequest:
            {
                XSelectionRequestEvent request = ev.xselectionrequest; // Get request
                clipboard.HandleSelectionRequest(request);
                break;
            }

            case SelectionNotify:
            {
                XSelectionEvent selection = ev.xselection;
                clipboard.HandleSelectionNotify(selection);
                break;
            }
        }
    }


    private static void UpdateKeyState(XKeyEvent key, bool isPressed)
    {
        if (key.keycode == f1_keycode)
            KeyState.f1Down = isPressed;
        if (key.keycode == f2_keycode) 
            KeyState.f2Down = isPressed; 
        if (key.keycode == f3_keycode) 
            KeyState.f3Down = isPressed;
        if (key.keycode == f4_keycode) 
            KeyState.f4Down = isPressed; 
    }


    public static bool IsPhysicalKeyDown(IntPtr display, int keycode)
    {
        byte[] keys = new byte[32];
        X11.XQueryKeymap(display, keys);
        return (keys[keycode / 8] & (1 << (keycode % 8))) != 0;
    }

    // Simulates key strokes for pasting
    public static void SimulateCtrlShiftV(IntPtr display, byte bufferId)
    { 
        uint ctrl  = (uint)XKeysymToKeycode(display, XK_Control_L);
        uint shift = (uint)XKeysymToKeycode(display, XK_Shift_L);
        uint v     = (uint)XKeysymToKeycode(display, XK_V);
        
        KeyAutoRepeat state = (bufferId == 1) ? f2AutoRepeat : f4AutoRepeat;
        bool wasKeyDown = IsPhysicalKeyDown(display, state.KeyCode);

        if (wasKeyDown)
        {
            state.IgnoreReleaseCount++;
            XTestFakeKeyEvent(display, (uint)state.KeyCode, false, 0); 
        }

        XTestFakeKeyEvent(display, ctrl, true, 0);   // Ctrl Down
        XTestFakeKeyEvent(display, shift, true, 0);  // Shift Down
        XTestFakeKeyEvent(display, v, true, 0);      // V Down

        XTestFakeKeyEvent(display, v, false, 0);     // V Up
        XTestFakeKeyEvent(display, shift, false, 0); // Shift Up
        XTestFakeKeyEvent(display, ctrl, false, 0);  // Ctrl Up

        if (wasKeyDown)
        {
            state.IgnorePressCount++;
            XTestFakeKeyEvent(display, (uint)state.KeyCode, true, 0); 
        }

        XFlush(display);
    }


    private static class KeyState
    {
        public static bool f1Down = false;
        public static bool f2Down = false;
        public static bool f3Down = false;
        public static bool f4Down = false;
       
        // When F1+F2+F3+F4 are pressed we activate/deactivate the key global grabbing
        public static bool combo => (KeyState.f1Down && KeyState.f2Down && KeyState.f3Down && KeyState.f4Down);
        public static bool toggleComboActive = false;

        public static bool CheckCombo()
        {
            if (KeyState.combo)
            {
                if (!KeyState.toggleComboActive)
                {
                    KeyState.toggleComboActive = true;
                    return true;
                }
            }
            else
            {
                KeyState.toggleComboActive = false;
            }
            return false;
        }
    }

    private class KeyAutoRepeat
    {
        public Stopwatch Stopwatch = new Stopwatch();
        public bool IsRepeating = false;
        public int IgnorePressCount = 0;
        public int IgnoreReleaseCount = 0;
        public int KeyCode;
        public byte BufferId;
        private string ActionName;

        public KeyAutoRepeat(int keyCode, byte bufferId, string actionName)
        {
            this.KeyCode = keyCode;
            this.BufferId = bufferId;
            this.ActionName = actionName;
        }

        public void ProcessMainLoop(IntPtr display)
        {
            if (!this.IsRepeating) return;

            bool isPhysicalDown = IsPhysicalKeyDown(display, KeyCode);
            if (!isPhysicalDown && this.Stopwatch.ElapsedMilliseconds > 150)
            {
                this.IsRepeating = false;
                this.Stopwatch.Stop();
            }
            else if (isPhysicalDown && this.Stopwatch.ElapsedMilliseconds > 150)
            {
                if (!clipboard.IsPasting) 
                {
                    clipboard.BeginPasteBufferContent(BufferId);
                    this.Stopwatch.Restart();
                }
            }
        }

        public bool HandleKeyPress(XKeyEvent ev)
        {
            if (ev.keycode != this.KeyCode) return false;

            if (this.IgnorePressCount > 0)
            {
                this.IgnorePressCount--;
                return true; 
            }
            if (clipboard.IsPasting) return true;
            
            if (this.IsRepeating && this.Stopwatch.ElapsedMilliseconds < 120)
                return true;

            clipboard.BeginPasteBufferContent(BufferId);
            this.IsRepeating = true;
            this.Stopwatch.Restart();
            return true;
        }
        
        public bool HandleKeyRelease(XKeyEvent ev)
        {
            if (ev.keycode != this.KeyCode) return false;

            if (this.IgnoreReleaseCount > 0)
            {
                this.IgnoreReleaseCount--;
                return true; 
            }
            return false;
        }
    }

}

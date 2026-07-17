class Program
{

    static void PrintHelp()
    {
        Console.WriteLine(
    @"BuffClip - Network clipboard manager - Inspired by Kitty terminal

    Usage:
        buffclip server
        buffclip client <server-ip>

    Commands:
        server              Start BuffClip in server mode.
        client <server-ip>  Connect to a BuffClip server.

    Examples:
        buffclip server
        buffclip client 192.168.1.100");
    }

    static void InitiateNetworkListener()
    {
        NetworkManager netman = new NetworkManager(4444);
        Thread thread = new Thread(netman.StartServer);
        thread.IsBackground = true;
        thread.Start();
    }

    
    static void Main(string[] args)
    {
        if (args.Length == 0) {
            PrintHelp();
            return;
        }
        switch (args[0].ToLower())
        {
            case "server":
                {
                    InitiateNetworkListener(); // Initiates server at 0.0.0.0:4444 Consider adjusting this via Parameters
                    int NUMBER_OF_BUFFERS = 2;
                    Buffers buffers = new Buffers(NUMBER_OF_BUFFERS); // Initiate buffers
                    HotkeyManager.ListenForKeyPress(buffers); // Waits for KeyPress/KeyRelease Events
                    break;
                }

            case "client":
                {
                    if (args.Length != 2) {
                        Console.WriteLine("Usage: buffclip client <ip>");
                        return;
                    }
                    NetworkManager netman = new NetworkManager(4444);
                    netman.Connect(args[1]);
                    netman.SendFullSyncRequest(1);
                    break;
                }

            default:
                PrintHelp();
                break;
        }
    }
}



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
        BuffclipServer server = new BuffclipServer("0.0.0.0", 4444);        // Default values for now
        Thread thread = new Thread(server.StartServer);
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
                    BuffclipClient client = new BuffclipClient("192.168.1.47", 4444);
                    client.Connect();
                    client.SendFullSyncRequest(1);
                    break;
                }

            default:
                PrintHelp();
                break;
        }
    }
}



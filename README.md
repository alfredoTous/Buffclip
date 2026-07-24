# Buffclip

*A lightweight clipboard manager with persistent clipboard buffers that sync across your virtual machines.*

### Originally inspired by Kitty's clipboard buffers

After using Kitty's clipboard buffers for a long time, I found myself wishing I could use the same workflow outside the terminal. They worked great inside Kitty, but not in my browser, IDE, or virtual machines.

Buffclip started as an attempt to bring persistent clipboard buffers to the entire desktop while seamlessly synchronizing them across multiple machines.

---

# Usage

```text
buffclip

Description:
  Buffclip - Network clipboard manager

Usage:
  buffclip [command] [options]

Commands:
  server    Start Buffclip in server mode.
  client    Connect to a Buffclip server.
  local     Run without networking.

Options:
  -h, --help
  --version
```
Every command provides its own help page.

## Server

```bash
buffclip server
```

By default, Buffclip listens on `0.0.0.0`.

For security, it is recommended to bind only to the interface used by your virtual machines.

## Client

```bash
buffclip client
```

Or:

```bash
buffclip client --ip 192.168.1.10
```

## Local mode

```bash
buffclip local
```

# Default Hotkeys

| Action | Key |
|---------|-----|
| Copy to Buffer 1 | `F1` |
| Paste Buffer 1 | `F2` |
| Copy to Buffer 2 | `F3` |
| Paste Buffer 2 | `F4` |

These are global hotkeys, so Buffclip exclusively grabs them while running. As a result, other applications cannot use these keybindings. Custom keybindings are not supported yet, but a quick toggle to enable or disable the global key grab is planned for an upcoming release.

# Features

- Persistent clipboard buffers
- Global X11 hotkeys
- Clipboard synchronization over TCP
- Automatic server discovery via UDP broadcast
- Multiple client support
- Native X11 implementation
- Background service

# Current Limitations

- X11 only (Wayland is not supported)
- Windows support is not yet available
- Still under active development and testing

# Motivation

I spend a lot of time switching between my host machine, virtual machines and remote systems while doing security research. Traditional clipboards and even terminal clipboard buffers were not enough for my workflow.
BuffClip attempts to make clipboard buffers behave more like shared memory across machines instead of a local operating system feature.


# High Priority TODOs

- [ ] Windows support
- [ ] Automatic client reconnection
- [ ] Packet encryption
- [ ] Logging
- [ ] Easier installation and service setup
- [ ] Wayland support (if feasible)


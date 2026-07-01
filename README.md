# TakobiAI

A behavior tree library for **Godot 4 / C#**, built on native Godot editor APIs

<img width="364" height="151" alt="image" src="https://github.com/user-attachments/assets/12f2e2de-aae2-4fa3-b0ff-85b34b856de9" />

## Features

- **Live Debugger** — a dockable panel (built on Godot's `EditorDebuggerPlugin`) that visualizes tick status and node state in real time while your game runs.

<img width="788" height="433" alt="image" src="https://github.com/user-attachments/assets/3477277f-f1f4-43bd-abb6-faf420bd3958" />


- **Native profiling** — tree counts and per-tree tick times show up directly in Godot's built-in **Monitors** tab, alongside FPS and memory.

<img width="544" height="359" alt="image" src="https://github.com/user-attachments/assets/88bfa48e-1795-4da8-af51-95f056cc8cb2" />


- **Custom inspectors** — method calls, signal emitters/awaiters, and Blackboard comparisons get dedicated dropdown editors instead of raw string fields.

<img width="377" height="429" alt="image" src="https://github.com/user-attachments/assets/113c5812-7155-436c-9d2d-eb606c778201" />
<img width="380" height="457" alt="image" src="https://github.com/user-attachments/assets/5b3e2306-cb07-4216-8388-159f8e02659f" />
  
- **Blackboard binding via `$key`** — bind exported fields on any node to a Blackboard value with a `$` prefix.
- **SubTree composition** — nest and reuse whole trees as a single leaf, with optional Blackboard sharing and circular-reference protection.
- **Zero-allocation tick path** — argument resolution is cached ahead of time, so ticking a tree doesn't generate garbage every frame.

## Installation

1. Copy the `addons/takobi_ai` folder into your project's `addons/` directory.
2. Enable **TakobiAI** in **Project Settings → Plugins**.

## License

MIT

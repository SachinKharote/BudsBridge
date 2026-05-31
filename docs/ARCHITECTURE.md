# Universal TWS Sync Architecture

## Delivery Strategy

Build the product in two layers:

- a WPF desktop shell for operator control and session visibility
- a sync engine that begins as a managed prototype and can later move hot paths to native code if profiling requires it

## Why This Stack

### UI

`C# + WPF + .NET Framework`

Reason:

- buildable in the current environment
- fast for Windows desktop iteration
- strong binding and styling support for the dashboard-heavy UI

Current repository target:

- `.NET Framework 4.0` for local build compatibility in this workspace
- can be retargeted upward once the matching developer pack is installed

### First Audio Prototype

`NAudio + WASAPI`

Reason:

- quick route to loopback capture and render experimentation
- low integration overhead for a Windows-only proof of concept

### Future Performance Layer

`C++ sync engine + WASAPI`

Reason:

- only introduce native complexity once the routing model is proven and actual timing bottlenecks are measured

## Architecture Overview

```text
WPF UI
  -> ViewModels
  -> Device and sync orchestration
  -> Audio services
  -> WASAPI capture and dual render pipelines
  -> Bluetooth audio endpoints
```

## Current Repository Layout

```text
universal-tws-sync/
  docs/
  src/
    UniversalTWSSync.App/
      Commands/
      Infrastructure/
      Models/
      Services/
      ViewModels/
```

## Application Layers

### Presentation

Responsibilities:

- dashboard rendering
- user actions
- latency control
- activity feed
- visual state

### Session Orchestration

Responsibilities:

- scan and selection flow
- connect and disconnect actions
- sync status evaluation
- routing commands to the active engine

### Device Discovery

Current state:

- mock service for predictable UI behavior

Future state:

- enumerate Windows audio endpoints first
- optionally enrich with Bluetooth metadata where available

### Audio Sync Engine

Current state:

- mocked session behavior and quality heuristics

Future state:

- WASAPI loopback capture
- mono downmix
- independent output buffers
- manual delay insertion
- drift monitoring

## Data Flow

1. User scans for devices.
2. Discovery service returns candidate endpoints.
3. User selects device A and device B.
4. Sync service validates the pair and starts a session.
5. System audio is captured and duplicated.
6. Manual delay is applied to the faster path.
7. Dashboard reflects current quality and activity.

## Honest Engineering Constraints

- not every Bluetooth adapter can hold two stable output sessions
- different devices can drift even after startup correction
- telemetry varies by vendor and Windows support level
- true stereo is a later milestone, not an MVP promise

## Next Implementation Steps

1. Replace mock discovery with actual Windows endpoint enumeration.
2. Add session persistence for recently used device pairs.
3. Integrate loopback capture and a local render test.
4. Validate dual-output behavior on at least one hardware matrix.
5. Decide whether native code is necessary after profiling.

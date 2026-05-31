# Universal TWS Sync

Universal TWS Sync is a Windows desktop MVP for routing mono system audio to two separately paired Bluetooth audio endpoints and keeping them aligned with manual latency correction.

This repository currently includes:

- a corrected MVP PRD
- a realistic Windows-first architecture document
- a buildable WPF desktop shell
- mock device discovery and sync services that preserve the intended UX while the low-level audio engine is developed

## Current MVP Direction

The product intent stays the same:

- dark desktop dashboard
- dual device workflow
- latency calibration
- quick actions
- connection health and battery panels

The engineering scope has been tightened to what is realistic on Windows:

- use Windows-paired audio endpoints for MVP
- prioritize tested hardware compatibility over universal claims
- keep mono sync as the first proof point

## Project Structure

- `docs/PRD-MVP.md` corrected product requirements for the first release
- `docs/ARCHITECTURE.md` implementation architecture and delivery phases
- `src/UniversalTWSSync.App` WPF application
- `build.ps1` local MSBuild entry point

## Building

This workspace does not currently expose `dotnet` on `PATH`, and only the `.NET Framework 4.0` reference pack is installed locally, so the current shell targets that framework and builds through Visual Studio Build Tools directly.

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1
```

If the required .NET Framework targeting pack or WPF build components are missing, install them through Visual Studio Build Tools first.

## What Is Implemented Today

- dashboard UI closely aligned to the provided concept
- scan devices action
- left and right device selection
- connect, start sync, stop sync, test audio, recalibrate, and audio settings actions
- latency slider with live status updates
- profile-like state retention during runtime
- activity feed and sync quality messaging

## What Comes Next

- replace mock device discovery with Windows endpoint enumeration
- integrate WASAPI loopback capture
- route duplicated mono output to two render pipelines
- add drift tracking and ongoing compensation
- save profiles to disk

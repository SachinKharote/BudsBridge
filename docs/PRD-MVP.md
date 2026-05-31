# Universal TWS Sync MVP PRD

## Product Vision

Universal TWS Sync helps users reuse independent earbuds by playing the same synchronized mono audio through two separately paired Bluetooth audio devices on Windows.

## Product Positioning

This MVP is an experimental Windows utility focused on proving reliable dual-endpoint playback on tested hardware.

It is not yet a universal Bluetooth pairing layer and should not claim universal compatibility across all adapters, codecs, or earbud brands.

## Problem Statement

Users often lose one earbud from a TWS pair or end up with single working earbuds from different brands. Existing ecosystems rarely allow these devices to be reused together, which increases waste and replacement cost.

## Primary Goal

Prove that two independent Bluetooth audio endpoints can play the same mono system audio stream with acceptable sync for at least 15 minutes on supported Windows hardware.

## MVP Audience

- students and budget-conscious users with partially usable earbuds
- tinkerers and technical users willing to try experimental audio tooling
- users motivated by reuse and e-waste reduction

## In Scope

- Windows desktop app
- list Windows-paired Bluetooth-capable audio endpoints
- select one endpoint for device A and one endpoint for device B
- capture system audio through WASAPI loopback
- convert stereo to mono
- duplicate the stream to two render pipelines
- manual latency adjustment
- status dashboard for connection, sync quality, and activity
- save and restore recently used pairings in a future MVP increment

## Out Of Scope

- pairing brand-new devices directly over Bluetooth
- guaranteed battery, codec, or signal telemetry for every brand
- perfect stereo separation
- microphone and call routing
- ANC coordination
- mobile support
- universal compatibility claims

## Assumptions

- both devices are already paired with Windows
- the Windows adapter and driver stack can maintain simultaneous audio sessions
- different devices may have different startup latency and drift behavior

## Success Criteria

- user can select two different endpoints
- app can start a sync session on tested hardware
- mono playback stays subjectively aligned after manual calibration
- session remains usable for 15 minutes on at least one documented hardware matrix
- UI clearly reports state, limitations, and failure conditions

## Risks

### Windows Bluetooth Limitations

Some adapters or drivers may refuse dual stable streaming or may degrade under load.

### Latency Drift

Initial offset correction may not be enough. Two devices can drift apart over time.

### Telemetry Gaps

Battery, codec, and radio metrics are inconsistent across brands and Windows APIs.

## MVP UX Requirements

- preserve the dual-device dashboard layout
- keep scan, connect, start sync, stop sync, test audio, recalibrate, and audio settings actions visible
- expose manual latency calibration as a first-class control
- communicate experimental status honestly

## Non-Functional Requirements

- start quickly on supported Windows systems
- keep logs for every major session event
- fail gracefully when one device disconnects
- prioritize clarity over aggressive automation

## Release Strategy

### Phase 1

UI and interaction shell with mock services.

### Phase 2

Real endpoint enumeration and WASAPI loopback prototype.

### Phase 3

Dual render pipeline, drift tracking, hardware compatibility matrix, and profile persistence.

Dissonance HLAPI Demo
=====================

This demo demonstrates the core features of Dissonance voice comms with Unity HLAPI networking:

* The Dissonance HLAPI "DissonanceSetup" prefab.
* Global voice channel with push-to-talk.
* Volume-triggered chat rooms, "Room A" and "Room B", with automatic voice activation.
* Proximity chat with other nearby players.
* 3D positional audio.
* Text chat channels.

See the Dissonance [documentation](https://dissonance.readthedocs.io/en/latest/) for more detailed documentation.

Running the demo
================

As this is a multiplayer voice comms demo, we will want to run multiple clients and connect them all into one session.

1. Add both "HLAPI Demo" and "HLAPI Game World" scenes to your project's build settings, and drag "HLAPI Demo" to the top of the list.
4. Click File -> Build and Run.
5. Once the client is running, run the Demo scene in the editor.
6. In editor, click "LAN Host(H)".
7. On the client, click "LAN Client(C)".

The demo scene should load on both instances with both players connected.

Global Chat
===========

Global chat is configured via the "Voice Broadcast Trigger" and "Voice Receipt Trigger" behaviours on the DemoWorld entity.

By default, the broadcast trigger is configured to open a channel to the "Global" room via push to talk, on the "GlobalChat" input axis (you may need to define this axis in Edit -> Project Settings -> Input). While holding down this button, all players in the session will hear you speak.

Global chat does not use 3D positional audio.

Collider Triggered Rooms
========================

Rooms A and B contain broadcast and receipt triggers which are configured to activate when a player is stood within their box colliders. Once the receipt trigger is activated you will hear all voice and text messages sent to the room. Once the broadcast trigger is activated it will transmit voice to that room.

These rooms each use 3D positional audio.

Player Tracking
===============

Each player prefab contains a "Hlapi Player" script. This script tracks the position of the player and enables positional audio playback for Dissonance voices as well as collider triggering for "Voice Broadcast Trigger" and "Voice Receipt Trigger".

Player Proximity
================

Each player prefab contains a "Voice Broadcast Trigger" which transmits directly to that player. This trigger is configured to activate when the local player is stood within a sphere collider attached to the remote player and will automatically transmit when it detects voice.

Proximity chat uses 3D positional audio.

Text Chat
=========

Press "y" to send text messages to all players in the "Global" room.
Press "u" to send text messages to all players stood inside "Room A".
Press "i" to send text messages to all players stood inside "Room B".
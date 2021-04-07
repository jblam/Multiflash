# Multiflash

A GUI tool for flashing Arduino-like devices with Arduino-like firmware.

Only Windows 10 2004 and above is officially supported. Linux and OSX are not supported.

## Installation instructions

Extract the zip file and run `multiflash.exe`.

A valid Multiflash archive must be provided by the microcontroller firmware author.

## What

A GUI which provides a drag-drop UX for flashing precompiled binaries.

## Why

I want to distribute firmware for the Arduino ecosystem, so that colleagues don't have to run the compiler and deal with AVR-GCC's accurate and insightful error messages.

However, neither Arduino IDE nor PlatformIO are set up to enable that. Both really want to build your source code.

The command-line tools they rely on are .. not great for newcomers. I spent 4 hours and roughly 1000 lines documenting it all so far.

## How

I depend on the individual board tools being installed already, and I hardcode wrappers around their CLI for every project that I do.

Presumably there's a nice way of doing this. Somewhere in Arduino IDE / PlatformIO there may be some kind of JSON doc which has all this info.
If I ever find that ¯\\\_(ツ)\_/¯?



# HaremShortcuts

HaremShortcuts is a plugin for HaremMate that adds keyboard shortcuts for various functions during an H Scene.

## Features

- Hide or show all User Interface.
- Move and rotate the character's location in the map.
- Scale the characters to be bigger or smaller.
- Option to remove the Illusion watermark from screenshots.

## Installation

1. Install [BepInEx]( https://github.com/BepInEx/BepInEx )
1. Install [BepinEx Configuration Manager]( https://github.com/BepInEx/BepInEx.ConfigurationManager )
1. Download and copy the [HaremShortcuts]( https://github.com/FrostedAshe/HaremShortcuts/releases/latest ) dll file to the `BepInEx\plugins` folder.

## Usage

During an H Scene, use the following shortcuts:

### General Shortcuts

| Keys                       | Actions                              |
|----------------------------|--------------------------------------|
| Spacebar                   | Hide or Show all UI                  |
| F11                        | Take Screenshot                      |

### Character Location Control

| Keys                       | Actions                              |
|----------------------------|--------------------------------------|
| I                          | Move characters forward              |
| K                          | Move characters backward             |
| J                          | Move characters left                 |
| L                          | Move characters right                |
| Left Control + (I/J/K/L)   | Reset character's location           |
| Y                          | Move characters up                   |
| H                          | Move characters down                 |
| Left Control + (Y/H)       | Reset character's height             |
| U                          | Rotate characters left               |
| O                          | Rotate characters right              |
| Left Control + (U/O)       | Reset character's rotation           |
| Backspace                  | Reset location, rotation and scale   |
| Left Shift                 | Make movement speed slower           |
| Number Pad Plus (+)        | Increase character's scale           |
| Number Pad Minus (-)       | Decrease character's scale           |
| Left Alt + (+/-)           | Reset character's scale              |

## Configuration

Press F1 to open the BepInEx Configuration Manager. Find and open HaremShortcuts, it has options for:

- Changing the default shortcut keys.
- Changing character location movement speed.
- Removing the watermark from screenshots.

## For Developers

### Building

In order to build the plugin:

- Install the [.NET SDK]( https://dotnet.microsoft.com/en-us/download )
- Clone this repository into a folder.
- Create a folder named `lib` in the project folder.
- Copy the file `Assembly-CSharp.dll` from your HaremMate install folder into the project `lib` folder. ( It is located in `HaremMate\data\Managed` )
- From a shell, change into the project folder and type `dotnet build`. The built dll will be placed in the project `bin` folder.

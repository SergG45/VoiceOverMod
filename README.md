
<p align="center">
  <a href="https://github.com/SergG45/VoiceOverMod" rel="noopener">
 <img width=200px height=200px src="https://em-content.zobj.net/source/skype/289/studio-microphone_1f399-fe0f.png" alt="README logo"></a>
</p>

<h1 align="center">VoiceOverMod</h1>

<div align="center">

</div>

---

<p align="center"> MelonLoader mod for "The Murder of Sonic The Hedgehog" game from Sega Social, adding feature of voicelines for each line of story text in-game
    <br> 
</p>

##  Contents
- [About](#about)
- [Getting Started](#getting_started)
- [Building](#building)
- [Issues](#issues)
- [Built using](#built_using)

## About <a name = "about"></a>
The mod adds the ability to load sound files for all lines of text in-game, allowing you to dub the entire game.

## Getting Started <a name = "getting_started"></a>

 1. Install [MelonLoader](https://github.com/LavaGang/MelonLoader) for a game
 2. Download [latest release of the mod](https://github.com/SergG45/VoiceOverMod/releases/)
 3. Extract contents of folder inside of .zip file in ***.../Mods/*** folder of a game
 

### Adding new audio files
Mod has `Story.XML` file, that stores all the lines of text, made using the StoryJSONtoXML program from this repository. The lines are divided into locations, according to the original JSON Ink file from assets of a game, and are numbered from `0000` to `X000`.

To add a sound for a line, save audio file with the corresponding index from the `.xml` file at start of name in /Mods/Audio/ folder. Not all lines need their audiofiles.

Example:
```
0001_Barry.mp3
```
| Supported audio formats |
| - |
| .mp3 |
| .wav |
| .aiff |

### Setting voice volume

Mod adds `Voice Volume` slider in settings menu of the game in upper-left corner of the screen:

[![The-Murder-of-Sonic-The-Hedgehog-q3n-L5gwmq5.png](https://i.postimg.cc/sx5j4Pnd/The-Murder-of-Sonic-The-Hedgehog-q3n-L5gwmq5.png)](https://postimg.cc/jndYsf0Z)

## Building <a name = "building"></a>
### Mod
 1.  [Install MelonLoader](https://melonwiki.xyz/#/README)
 2.  Modify  `file.csproj`  and edit all  
`<Reference Include="Assembly-CSharp"><HintPath>**.dll</HintPath></Reference>` and etc. to link to your TMOSTH directory libraries.
 3.  Compile and move the  `obj\Release\VoiceOverMod.dll`  to the Mod folder inside your TMOSTH directory.

Mod built initially using MelonLoader v0.5.7
### StoryJSONtoXML
Uses Nuget Newtonsoft.Json.13.0.3 package
 1. Compile program from .sln project (.NET Framework 4.7.2)
 2. Extract TextAsset `story` from `sharedassets0.assets` file from Data game directory to `story.txt`, beside .exe of program. (using [UABE](https://github.com/SeriousCache/UABE) or [Asset Studio](https://github.com/Perfare/AssetStudio))
 3. Create `Locations.txt` file inside of program directory with contents of hierarchy from `story.txt` JSON, with each staring fron new line.
Hierarchy is used to determine the chronology of the lines for reading .XML only.

Example:

[![vivaldi-dervz8nj66.png](https://i.postimg.cc/85ym4W05/vivaldi-dervz8nj66.png)](https://postimg.cc/BLKFQ8s9)

 4. Launch program to create `Story.xml` file.

##  Issues <a name="issues"></a>
If current line of dialogue consists of player's name, mod will differentiate needed line fron .XML list using Levenshtein Distance, because it's unknown to author of the mod how to read current knots of [Ink's](https://github.com/inkle/ink) story at the moment.



##  Built Using <a name = "built_using"></a>
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [MelonLoader v0.5.7](https://github.com/LavaGang/MelonLoader)
- [UnityExplorer](https://github.com/sinai-dev/UnityExplorer)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- Unity library's of [The Murder of Sonic the Hedgehog](https://store.steampowered.com/app/2324650/The_Murder_of_Sonic_the_Hedgehog/)

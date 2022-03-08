# MiscToolsForMD

## What is This

A Muse Dash Mod which contains many features.

## What feature does it have

1. Realtime accuracy calculator (a little buggy but I can't fix it now, maybe you can help me.)

2. Key indicator (which key do you press and how many times do you press that key)

3. Realtime lyric display (under developing)

## How to use it

Install mod loader from [here](https://github.com/LavaGang/MelonLoader), install binary file as a mod, start game and you can see changes.

## How to get binary

You can clone this repo, open in Visual Studio and recover the project. Copy `Directory.Build.props.example` to `Directory.Build.props` and edit it, set game folder(for example, `C:\Program Files\Steam\steamapps\common\Muse Dash`). Some libraries are nuget packages and you can recover normally, other libraries are in `%Game Folder%\MelonLoader\Managed` after you started game with MelonLoader at least once but it should be detected correctly if you edit `Directory.Build.props` correctly.  
After recovering project successfully, you can build it and get `MiscToolsForMD.dll` at `%Repo Folder%\MiscToolsForMD\bin\%Build Type%\`

## Contributing

Please creating feature at branch dev, we use branch main as stable version.

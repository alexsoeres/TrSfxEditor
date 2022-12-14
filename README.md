# TrSfxEditor 
#### Author: BlazinDevilify
___
My own library for extracting, packing and encoding sound effects from a Tomb Raider MAIN.SFX file.

**Supported Games: TR2, TR3**

## 1. Setup
All required files are provided in an archive, for ease of use unpack its contents directly in the folder, where the `MAIN.SFX` file is located.
Normally it's located under `<GAME DIR>\data`.

Also it's recommended to back up the existing `MAIN.SFX` file before performing any operation.

## 2.a. Usage with included batch files (recommended)
### Unpack
Run the included `unpack.bat` batch file to extract the wav files into `.\sfx\`. It will create the folder if it doesn't exist. 

**!!! WARNING: overrides existing files !!!**

### Modify Sound Effects
In order to change the sound effects you can simply replace the existing wav file. Just make sure to keep the same file name and do not add / remove any sound effects beyond the ones that were generated in the first place. Otherwise it might cause the game to crash on startup.

### Convert
Before you can repack you need to make sure that all wave files are the correct encoding. To do so you can simply run `convert-trX.bat`. The program will check and convert all wave files to the correct format automatically.

### Pack
Pack will take all the wave files from `.\sfx\` and concatenate its contents into `MAIN.SFX`.

**!!! WARNING: overrides the MAIN.SFX file !!!**

### Launch game
Work's done ^^

## 2.b. Usage in command line (advanced)
| Command                                            | Description                                                   | Args         | Argument Description                                       |
|----------------------------------------------------|---------------------------------------------------------------|--------------|------------------------------------------------------------|
| `unpack <sfxFile> <folder>`                        | Unpack wave files from SFX file into dedicated folder.        | `sfxFile`    | source file *(ex. `MAIN.SFX`)*                             |
|                                                    |                                                               | `folder`     | target folder *(ex. `./sfx/`)*                             |
| `pack <folder> <sfxFile>`                          | Packs the wave files from a dedicated folder into a SFX file. | `folder`     | source folder containing extracted files *(ex. `./sfx/`)*  |
|                                                    |                                                               | `sfxFile`    | target file *(ex. `MAIN.SFX`)*                             |
| `convert <trVersion> <wavFileIn> <wavFileOut>`     | Converts a single wav files to the appropriate format.        | `trVersion`  | `Tr2` / `Tr3`                                              |
|                                                    |                                                               | `wavFileIn`  | source wav file *(ex. `.../Downloads/wilhelm-scream.wav`)* |
|                                                    |                                                               | `wavFileOut` | target wav file *(ex. `./sfx/69.wav`)*                     |
| `convert-all <trVersion> <wavFileIn> <wavFileOut>` | Converts all wav files in a folder to the appropriate format. | `trVersion`  | `Tr2` / `Tr3`                                              |
|                                                    |                                                               | `folder`     | sfx folder *(ex. `./sfx/`)*                                |
| `check <wavFile>`                                  | Displays the metadata of the wav file in the console.         | `wavFile`    | wav file to check *(ex. `.../Desktop/recording.wav`)*      |

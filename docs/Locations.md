
## Locations provided by LeoConsole

 - `data.SavePath` => `$SAVEPATH`
 - `data.DownloadPath` => `$DOWNLOADPATH`

## Folders and files

These folders are used by apkg or are recommended to use for plugin developers.

| Folder                                       | Description                                                 |
|----------------------------------------------|-------------------------------------------------------------|
| `$DOWNLOADPATH/apkg`                         | files are downloaded, extracted and temporarily stored here |
| `$SAVEPATH/plugins`                          | installed `.dll` plugin files                               |
| `$SAVEPATH/share/<plugin-name>`              | additional plugins files                                    |
| `$SAVEPATH/share/docs/<plugin-name>`         | plugin documentation                                        |
| `$SAVEPATH/share/docs/<plugin-name>/LICENSE` | plugin license (auto-installed from plugin repository)      |
| `$SAVEPATH/share/docs/apkg`                  | apkg documentation                                          |
| `$SAVEPATH/var/<plugin-name>`                | plugin configs, databases and other variable data           |
| `$SAVEPATH/var/apkg/config.json`             | apkg's main config file                                     |
| `$SAVEPATH/var/apkg/installed`               | databases of installed files                                |


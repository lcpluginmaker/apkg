
## Locations provided by LeoConsole

 - `data.SavePath` => `$SAVEPATH`
 - `data.DownloadPath` => `$DOWNLOADPATH`

## Folders and files

These folders are used by apkg or are recommended to use for plugin developers.

| Folder                               | Description                                               |
|--------------------------------------|-----------------------------------------------------------|
| `$DOWNLOADPATH/apkg`                 | files are downloaded and temporarily stored here          |
| `$DOWNLOADPATH/apkg/plugin-extract`  | `lcpkg` files are extracted into here                     |
| `$SAVEPATH/plugins`                  | installed `.dll` plugin files                             |
| `$SAVEPATH/share/<plugin-name>`      | additional plugins files                                  |
| `$SAVEPATH/share/docs/<plugin-name>` | plugin documentation                                      |
| `$SAVEPATH/share/docs/apkg`          | apkg documentation                                        |
| `$SAVEPATH/var/<plugin-name>`        | plugins should save their config files and databases here |
| `$SAVEPATH/var/apkg`                 | apkg's config files and databases.                        |
| `$SAVEPATH/var/apkg/config`          | apkg's main config file                                   |
| `$SAVEPATH/var/apkg/installed`       | databases of installed files                              |
| `$SAVEPATH/var/apkg/repos`           | activated repositories                                    |



## Locations provided by LeoConsole

 - `data.SavePath` => `$SAVEPATH`
 - `data.DownloadPath` => `$DOWNLOADPATH`

## Folders and files that apkg uses

| Folder                               | Description                                               |
|--------------------------------------|-----------------------------------------------------------|
| `$DOWNLOADPATH/plugins`              | files are downloaded and temporarily stored here          |
| `$SAVEPATH/pkg/PackageList.txt`      | apkg checks this file for available packages.             |
| `$SAVEPATH/plugins`                  | installed `.dll` plugin files                             |
| `$SAVEPATH/share/<plugin-name>`      | additional plugins files                                  |
| `$SAVEPATH/share/docs/<plugin-name>` | plugin documentation                                      |
| `$SAVEPATH/share/docs/apkg`          | apkg documentation                                        |
| `$SAVEPATH/var/<plugin-name>`        | plugins should save their config files and databases here |
| `$SAVEPATH/var/apkg`                 | apkg's config files and databases.                        |
| `$SAVEPATH/var/apkg/config`          | apkg's main config file                                   |


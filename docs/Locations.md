
## Locations provided by LeoConsole

 - `data.SavePath` => `$SAVEPATH`
 - `data.DownloadPath` => `$DOWNLOADPATH`

## Folders and files that apkg uses

| Folder                          | Description                                                                                    |
|---------------------------------|------------------------------------------------------------------------------------------------|
| `$DOWNLOADPATH/plugins`         | Place where apkg downloads `.dll` files to before installing or clones git repos for building. |
| `$SAVEPATH/pkg/PackageList.txt` | apkg checks this file for available packages.                                                  |
| `$SAVEPATH/plugins`             | This is where installed `.dll` plugin files go.                                                |
| `$SAVEPATH/share/<plugin-name>` | This is where additional plugins files go to.                                                  |
| `$SAVEPATH/var/<plugin-name>`   | This is where plugins should save their config files and databases                             |
| `$SAVEPATH/var/apkg`            | apkg's config files and databases.                                                             |
| `$SAVEPATH/var/apkg/config`     | apkg's main config file                                                                        |


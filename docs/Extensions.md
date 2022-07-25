
# Writing extensions for apkg

An extension is a subcommand for apkg. Extensions are placed in
`share/apkg/extensions` using following scheme:

```text
share/apkg/extensions
 | - installed-number      -> executable binary
 | - installed-number.json -> metadata file
 | ...
```

## Metadata format

```json
{
    "metadataVersion": 1.0,
    "description": "print number of installed packages",
    "author": "alexcoder04@protonmail.com",
    "disabled": false
}
```

## Extension call

When an extension is called, it gets a base64-encoded argument, which contains
following json data:

```json
{
    "args": ["arg1", "arg2"],
    "savePath": "/path/to/leoconsole/savepath"
}
```


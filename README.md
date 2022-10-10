# Slender Fortress Config Rewriter
A required tool for Slender Fortress Modified 1.7.5+ that rewrites boss configs.

## Features
* Supported versions:
  * 1.7.5+
* Splits profiles.cfg
* Splits boss packs (if configured in profiles_packs.cfg)
* Cleans up configs
* Removes unnecessary keys
* Updates configs to be up to date with the most recent version of Slender Fortress Modified

## Requirements
* [.NET Console Runtime 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime)

## Build Requirements
* Visual Studio 2022+

# How To Use
### Single and multiple configs

Take whatever config files you want, copy them and paste them in any empty folder (or leave them in their original server directories). The config rewriter will look for sub folders so be aware about that if you don't want certain configs to be accidentally rewritten. Any config files with more than 1 boss in them will be split up automatically.
![Capture](https://user-images.githubusercontent.com/42941613/192687536-33faff3f-2ae9-421a-95ef-30ad9665ea69.PNG)

### profiles.cfg

Make sure this config name is exactly profiles.cfg and at least contains the following:
```
"Profiles"
{
}
```

### profiles_packs.cfg

Inside of profiles_packs.cfg, look for all "file" key values and make sure they have a .cfg at the end of them if the pack is a giant config file, if the config are already split prior to installing this program, you can safely ignore that key value. Also make sure that the pack configs are at least in (target directory)/profiles/packs/. Any sub directores found in "file" are fine to keep.

![Capture](https://user-images.githubusercontent.com/42941613/192690182-a1e2689d-550c-4962-b0c6-1d98f36ef953.PNG)


## Rewriting the configs

Simply input a directory to where all the config files are located, back up your configs in case anything goes wrong and leave an issue about it if there are any bugs. On average the config rewriter rewrites one config in less than one second.

![Capture](https://user-images.githubusercontent.com/42941613/192690639-b1222f7e-6c02-4e57-9207-48ca486297a7.PNG)

# Submitting an issue

If there is an error in the config rewriter that messed up your config, get the original unwritten config file you tried to rewrite and either upload it in Pastebin or as it's own file in something like Dropbox or MEGA. You don't need to explain whats wrong with the config, but if you can pinpoint the issue it'd help me out a lot in fixing the issue.

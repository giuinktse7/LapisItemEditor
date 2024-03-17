# Publish instructions

Change `OutputType` in `LapisItemEditor/LapisItemEditor.csproj` from `Exe` to `WinExe`

# win-64

```
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false
```

# linux-64

```
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained false
```

# sldl-gui

## Einrichtung

1. App starten
2. Zahnrad-Icon oben rechts klicken
3. Soulseek **Username** und **Password** eingeben
4. Speichern

## Download starten

1. URL in die Suchleiste einfuegen (Spotify, YouTube, Bandcamp, MusicBrainz)
2. Format und Bitrate waehlen (optional)
3. Download-Ordner waehlen (optional)
4. **Start** klicken

## Waehrend dem Download

- Fortschritt wird pro Track angezeigt
- **Stop** bricht alles ab

## Nach dem Download

- Rechtsklick auf einen Track: **Open file** / **Show in Explorer**
- Fehlgeschlagene Tracks: **Retry Failed** Button unten links
- Rechtsklick auf einzelnen Track: **Retry**

## Unterstuetzte URLs

- `https://open.spotify.com/playlist/...`
- `https://open.spotify.com/album/...`
- `https://open.spotify.com/track/...`
- `https://www.youtube.com/playlist?list=...`
- `https://www.youtube.com/watch?v=...`
- `https://bandcamp.com/...`
- `https://musicbrainz.org/release/...`

## Build

```
dotnet publish slsk-batchdl.Gui -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

Output: `slsk-batchdl.Gui\bin\Release\net8.0-windows\win-x64\publish\sldl-gui.exe`

#!/bin/bash
APP_NAME="./MacOS-App-Build/WorldviewShare.app"
PUBLISH_OUTPUT_DIRECTORY="./bin/Release/net8.0/"
# PUBLISH_OUTPUT_DIRECTORY should point to the output directory of your dotnet publish command.
# One example is /path/to/your/csproj/bin/Release/netcoreapp3.1/osx-x64/publish/.
# If you want to change output directories, add `--output /my/directory/path` to your `dotnet publish` command.
INFO_PLIST="./MacOS-App-Build/Info.plist"
ICON_DIRECTORY="./MacOS-App-Build/"
ICON_FILE="WorldviewShare.icns"

if [ -d "$APP_NAME" ]
then
    rm -rf "$APP_NAME"
fi

mkdir "$APP_NAME"

mkdir "$APP_NAME/Contents"
mkdir "$APP_NAME/Contents/MacOS"
mkdir "$APP_NAME/Contents/Resources"
echo "[INFO] Copying Info.plist"
cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
echo "[INFO] Copying icon file"
cp "$ICON_DIRECTORY$ICON_FILE" "$APP_NAME/Contents/Resources/$ICON_FILE"
echo "[INFO] Copying app files"
cp -a "$PUBLISH_OUTPUT_DIRECTORY" "$APP_NAME/Contents/MacOS"
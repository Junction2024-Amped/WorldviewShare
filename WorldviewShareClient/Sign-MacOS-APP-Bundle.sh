#!/bin/bash
APP_NAME="./MacOS-App-Build/WorldviewShare.app"
ENTITLEMENTS="./MacOS-App-Build/WorldviewShareEntitlements.entitlements"
SIGNING_IDENTITY="Keymasterer44" # matches Keychain Access certificate name

find "$APP_NAME/Contents/MacOS/"|while read fname; do
    if [[ -f $fname ]]; then
        echo "[INFO] Signing $fname"
        codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$fname"
    fi
done

echo "[INFO] Signing app file"

codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$APP_NAME"
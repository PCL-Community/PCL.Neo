#!/bin/bash
# 此脚本中签名和hdiutil的部分只能在Mac上运行！

INFO_PLIST="Info.plist"
ICON_FILE="PCL.Neo.icns"
ENTITLEMENTS="PCLNeoEntitlements.entitlements"
# SIGNING_IDENTITY="Developer ID: PCLCommunity"

cd ./PCL.Neo
dotnet publish -r osx-arm64 --configuration Release -p:UseAppHost=true
dotnet publish -r osx-x64 --configuration Release -p:UseAppHost=true

cd ../mac-publish-assets

# 创建输出目录
mkdir -p arm64
mkdir -p x64

# 打包 arm64 版本
APP_NAME_ARM64="arm64/PCL.Neo.app"
PUBLISH_OUTPUT_DIRECTORY_ARM64="../PCL.Neo/bin/Release/net10.0/osx-arm64/publish/."

if [ -d "$APP_NAME_ARM64" ]; then
    rm -rf "$APP_NAME_ARM64"
fi

mkdir -p "$APP_NAME_ARM64/Contents/MacOS"
mkdir -p "$APP_NAME_ARM64/Contents/Resources"

cp "$INFO_PLIST" "$APP_NAME_ARM64/Contents/Info.plist"
cp "$ICON_FILE" "$APP_NAME_ARM64/Contents/Resources/PCL.Neo.icns"
cp -a $PUBLISH_OUTPUT_DIRECTORY_ARM64/* "$APP_NAME_ARM64/Contents/MacOS/"

# find "$APP_NAME_ARM64/Contents/MacOS/"|while read fname; do
#     if [[ -f $fname ]]; then
#         echo "[INFO] Signing $fname"
#         codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$fname"
#     fi
# done

# echo "[INFO] Signing app file"

# codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$APP_NAME"
# hdiutil create -volname "PCL.Neo.Arm64" -srcfolder $APP_NAME_ARM64 -ov -format UDZO arm64/PCL.Neo.dmg

# 打包 x64 版本
APP_NAME_X64="x64/PCL.Neo.app"
PUBLISH_OUTPUT_DIRECTORY_X64="../PCL.Neo/bin/Release/net10.0/osx-x64/publish/."

if [ -d "$APP_NAME_X64" ]; then
    rm -rf "$APP_NAME_X64"
fi

mkdir -p "$APP_NAME_X64/Contents/MacOS"
mkdir -p "$APP_NAME_X64/Contents/Resources"

cp "$INFO_PLIST" "$APP_NAME_X64/Contents/Info.plist"
cp "$ICON_FILE" "$APP_NAME_X64/Contents/Resources/PCL.Neo.icns"
cp -a $PUBLISH_OUTPUT_DIRECTORY_X64/* "$APP_NAME_X64/Contents/MacOS/"
# hdiutil create -volname "PCL.Neo.X64" -srcfolder $APP_NAME_X64 -ov -format UDZO x64/PCL.Neo.dmg

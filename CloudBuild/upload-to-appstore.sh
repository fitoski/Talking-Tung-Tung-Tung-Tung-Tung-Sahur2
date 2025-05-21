#!/bin/bash
set -e

echo "Uploading IPA to App Store Connect…"

# build.json’dan gelen ikinci argüman, build çıktısının olduğu klasör 
BUILD_DIR="$2"

# IPA dosyasının kesin yolu
IPA_PATH="$BUILD_DIR/build.ipa"
echo "IPA found at $IPA_PATH"

# JWT anahtar dosyasını App Store Connect’in beklendiği yere yaz
mkdir -p ~/.appstoreconnect/private_keys
echo "$ASC_API_KEY_CONTENT" > ~/.appstoreconnect/private_keys/AuthKey_${ASC_API_KEY_ID}.p8

# Gerçek upload
xcrun altool --upload-app \
  --type ios \
  --file "$IPA_PATH" \
  --apiKey "$ASC_API_KEY_ID" \
  --apiIssuer "$ASC_ISSUER_ID"

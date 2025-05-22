#!/bin/bash
set -e

echo ">>> PATH: $PATH"
which xcrun || { echo "❌ xcrun komutu bulunamadı"; exit 1; }
ls -R ~/.appstoreconnect || echo "ℹ️ Anahtar klasörü yok veya boş"
echo ">>> Debug bitti"

echo "Uploading IPA to App Store Connect…"

BUILD_DIR="$2"

IPA_PATH="$BUILD_DIR/build.ipa"
echo "IPA found at $IPA_PATH"

mkdir -p ~/.appstoreconnect/private_keys
echo "$ASC_API_KEY_CONTENT" > ~/.appstoreconnect/private_keys/AuthKey_${ASC_API_KEY_ID}.p8

xcrun altool --upload-app \
  --type ios \
  --file "$IPA_PATH" \
  --apiKey "$ASC_API_KEY_ID" \
  --apiIssuer "$ASC_ISSUER_ID"

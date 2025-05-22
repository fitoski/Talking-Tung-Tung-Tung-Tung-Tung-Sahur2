#!/bin/bash
set -e

echo ">>> PATH: $PATH"
which xcrun || { echo "❌ xcrun bulunamadı!"; exit 1; }

echo ">>> ~/.appstoreconnect içeriği (öncesi):"
ls -R ~/.appstoreconnect || echo "❌ Anahtar klasörü yok"

echo "Uploading IPA to App Store Connect…"

BUILD_DIR="$2"
IPA_PATH="$BUILD_DIR/build.ipa"
echo "✅ IPA found at $IPA_PATH"

mkdir -p ~/.appstoreconnect/private_keys
echo "$ASC_API_KEY_CONTENT" > ~/.appstoreconnect/private_keys/AuthKey_${ASC_API_KEY_ID}.p8

echo ">>> ~/.appstoreconnect içeriği (sonrası):"
ls -R ~/.appstoreconnect

xcrun altool --upload-app \
  --type ios \
  --file "$IPA_PATH" \
  --apiKey "$ASC_API_KEY_ID" \
  --apiIssuer "$ASC_ISSUER_ID"

echo "✅ Upload tamamlandı."

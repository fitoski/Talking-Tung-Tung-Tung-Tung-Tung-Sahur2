#!/bin/bash
set -e

echo "🚀 Starting upload to App Store Connect…"

# 1) Build klasörü, Cloud Build tarafından $2 argümanına verilir
BUILD_DIR="$2"
IPA_PATH="$BUILD_DIR/build.ipa"
echo "✅ IPA located at: $IPA_PATH"

# 2) App Store Connect API Key için dizin
KEY_DIR="$HOME/.appstoreconnect/private_keys"
mkdir -p "$KEY_DIR"

# 3) ENV’den gelen “\n” kaçışlarını gerçek satıra çevirerek .p8 dosyasını oluştur
printf '%b' "$ASC_API_KEY_CONTENT" > "$KEY_DIR/AuthKey_${ASC_API_KEY_ID}.p8"
echo "🔑 Wrote key to $KEY_DIR/AuthKey_${ASC_API_KEY_ID}.p8"
ls -l "$KEY_DIR"

# 4) Gerçek upload
xcrun altool --upload-app \
  --type ios \
  --file "$IPA_PATH" \
  --apiKey "$ASC_API_KEY_ID" \
  --apiIssuer "$ASC_ISSUER_ID"

echo "✅ IPA upload completed successfully."

#!/usr/bin/env bash
set -e

IPA="$WORKSPACE/.build/last/$TARGET_NAME/build.ipa"
echo "Uploading IPA: $IPA"

mkdir -p ~/.appstoreconnect/private_keys
printf '%s' "$ASC_API_KEY_CONTENT" > ~/.appstoreconnect/private_keys/AuthKey_${ASC_API_KEY_ID}.p8

xcrun altool --upload-app \
  --type ios \
  --file "$IPA" \
  --apiKey "$ASC_API_KEY_ID" \
  --apiIssuer "$ASC_ISSUER_ID"

#!/bin/bash

echo "Uploading IPA to App Store Connect…"

# 1) locate the IPA
IPA="$WORKSPACE/.build/last/$TARGET_NAME/build.ipa"
echo "IPA found at $IPA"

# 2) make sure App Store Connect key dir exists
mkdir -p ~/.appstoreconnect/private_keys

# 3) write the p8 file into the place altool expects
echo "$ASC_API_KEY_CONTENT" > ~/.appstoreconnect/private_keys/AuthKey_${ASC_API_KEY_ID}.p8

# 4) upload with altool
xcrun altool --upload-app \
  --type ios \
  --file "$IPA" \
  --apiKey "$ASC_API_KEY_ID" \
  --apiIssuer "$ASC_ISSUER_ID"

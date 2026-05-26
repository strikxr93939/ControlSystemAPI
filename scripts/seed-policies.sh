#!/bin/bash
# Seed test policies into the API
# Usage: ./seed-policies.sh [api_url]

API="${1:-http://localhost:5186}"

echo "Seeding policies to $API..."

curl -s -X POST "$API/api/policies" \
  -H "Content-Type: application/json" \
  -d '{
    "programPattern": "notepad",
    "minVersion": "99.0.0.0",
    "blockType": 0,
    "workshop": "Workshop A",
    "message": "Notepad version is outdated",
    "isActive": true,
    "startTime": "2026-01-01T00:00:00Z",
    "exceptions": ""
  }' | python3 -m json.tool

echo ""

curl -s -X POST "$API/api/policies" \
  -H "Content-Type: application/json" \
  -d '{
    "programPattern": "torrent",
    "blockType": 2,
    "workshop": "",
    "message": "Torrent software is forbidden",
    "isActive": true,
    "startTime": "2026-01-01T00:00:00Z",
    "exceptions": ""
  }' | python3 -m json.tool

echo ""
echo "Done. Check: $API/api/policies"

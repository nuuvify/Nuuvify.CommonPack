#!/usr/bin/env bash
set -euo pipefail

sed -nE 's/^.*Pushing (Nuuvify\.CommonPack\.[A-Za-z0-9._-]+)\.([0-9]+\.[0-9]+\.[0-9]+-preview\.[0-9]+)\.nupkg to .*/\1 \2/p'

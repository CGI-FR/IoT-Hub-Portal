#!/bin/bash
# Script to discover all Bicep templates in the repository
# Usage: ./find-bicep-templates.sh [base_directory]
# Output: JSON array of Bicep template paths

set -euo pipefail

# Default to repository root if no argument provided
BASE_DIR="${1:-.}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1" >&2
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1" >&2
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1" >&2
}

# Validate base directory exists
if [ ! -d "${BASE_DIR}" ]; then
    log_error "Directory not found: ${BASE_DIR}"
    exit 1
fi

log_info "Searching for Bicep templates in: ${BASE_DIR}"

# Find all .bicep files, excluding:
# - node_modules directories
# - hidden directories (starting with .)
# - test directories (for now we'll include tests, but can filter later)
BICEP_FILES=$(find "${BASE_DIR}" -type f -name "*.bicep" \
    ! -path "*/node_modules/*" \
    ! -path "*/.*" \
    -print | sort)

# Count templates found
COUNT=$(echo "${BICEP_FILES}" | grep -c . || echo "0")

if [ "${COUNT}" -eq 0 ]; then
    log_warn "No Bicep templates found in ${BASE_DIR}"
    echo "[]"
    exit 0
fi

log_info "Found ${COUNT} Bicep template(s)"

# Convert to JSON array
# We need to properly escape and format paths for JSON
JSON_ARRAY="["
FIRST=true

while IFS= read -r file; do
    if [ -n "${file}" ]; then
        # Remove leading ./ if present
        file="${file#./}"
        
        if [ "${FIRST}" = true ]; then
            JSON_ARRAY="${JSON_ARRAY}\"${file}\""
            FIRST=false
        else
            JSON_ARRAY="${JSON_ARRAY},\"${file}\""
        fi
        
        log_info "  - ${file}"
    fi
done <<< "${BICEP_FILES}"

JSON_ARRAY="${JSON_ARRAY}]"

# Output JSON array to stdout
echo "${JSON_ARRAY}"

log_info "Bicep template discovery complete"

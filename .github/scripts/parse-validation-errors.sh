#!/bin/bash
# Script to parse and format Azure deployment validation error messages
# Usage: ./parse-validation-errors.sh <error_output_file>
# Output: Formatted error messages with resource names, error codes, and descriptions

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
}

print_section() {
    echo -e "${MAGENTA}──────────────────────────────────────────────────────${NC}"
    echo -e "${MAGENTA}$1${NC}"
    echo -e "${MAGENTA}──────────────────────────────────────────────────────${NC}"
}

# Check if input file is provided
if [ $# -lt 1 ]; then
    log_error "Usage: $0 <error_output_file>"
    log_error "No error output file provided"
    exit 1
fi

ERROR_FILE="$1"

# Check if file exists
if [ ! -f "${ERROR_FILE}" ]; then
    log_error "Error file not found: ${ERROR_FILE}"
    exit 1
fi

log_info "Parsing validation errors from: ${ERROR_FILE}"
echo ""

# Read the error file
ERROR_CONTENT=$(cat "${ERROR_FILE}")

# Check if there are any errors
if [ -z "${ERROR_CONTENT}" ]; then
    log_info "No errors found in output"
    exit 0
fi

print_header "DEPLOYMENT VALIDATION ERRORS"
echo ""

# Parse JSON error output if present
# Azure CLI returns errors in JSON format with structure like:
# { "error": { "code": "...", "message": "...", "details": [...] } }
if echo "${ERROR_CONTENT}" | jq . > /dev/null 2>&1; then
    log_info "Parsing JSON formatted errors..."
    
    # Extract error code
    ERROR_CODE=$(echo "${ERROR_CONTENT}" | jq -r '.error.code // "Unknown"' 2>/dev/null || echo "Unknown")
    
    # Extract error message
    ERROR_MESSAGE=$(echo "${ERROR_CONTENT}" | jq -r '.error.message // .error // "No message available"' 2>/dev/null || echo "No message available")
    
    print_section "Error Code"
    echo -e "${RED}${ERROR_CODE}${NC}"
    echo ""
    
    print_section "Error Message"
    echo "${ERROR_MESSAGE}"
    echo ""
    
    # Extract details if available
    DETAILS_COUNT=$(echo "${ERROR_CONTENT}" | jq -r '.error.details // [] | length' 2>/dev/null || echo "0")
    
    if [ "${DETAILS_COUNT}" -gt 0 ]; then
        print_section "Error Details (${DETAILS_COUNT} issue(s) found)"
        echo ""
        
        for i in $(seq 0 $((DETAILS_COUNT - 1))); do
            DETAIL_CODE=$(echo "${ERROR_CONTENT}" | jq -r ".error.details[${i}].code // \"Unknown\"" 2>/dev/null || echo "Unknown")
            DETAIL_MESSAGE=$(echo "${ERROR_CONTENT}" | jq -r ".error.details[${i}].message // \"No message\"" 2>/dev/null || echo "No message")
            DETAIL_TARGET=$(echo "${ERROR_CONTENT}" | jq -r ".error.details[${i}].target // \"\"" 2>/dev/null || echo "")
            
            echo -e "${YELLOW}Issue $((i + 1)):${NC}"
            echo -e "  ${RED}Code:${NC} ${DETAIL_CODE}"
            
            if [ -n "${DETAIL_TARGET}" ]; then
                echo -e "  ${RED}Resource:${NC} ${DETAIL_TARGET}"
            fi
            
            echo -e "  ${RED}Message:${NC}"
            # Indent the message
            echo "${DETAIL_MESSAGE}" | sed 's/^/    /'
            echo ""
        done
    fi
    
    # Try to extract resource-specific errors
    INNER_ERROR=$(echo "${ERROR_CONTENT}" | jq -r '.error.details[]? | select(.code == "InvalidTemplateDeployment") | .message' 2>/dev/null || echo "")
    if [ -n "${INNER_ERROR}" ]; then
        print_section "Deployment Issues"
        echo "${INNER_ERROR}"
        echo ""
    fi
else
    # Plain text error output
    log_info "Parsing plain text errors..."
    echo ""
    
    print_section "Raw Error Output"
    echo "${ERROR_CONTENT}"
    echo ""
    
    # Try to extract common error patterns
    if echo "${ERROR_CONTENT}" | grep -qi "quota"; then
        log_warn "⚠️  Quota-related error detected"
    fi
    
    if echo "${ERROR_CONTENT}" | grep -qi "permission\|authorization\|forbidden"; then
        log_warn "⚠️  Permission/Authorization error detected"
    fi
    
    if echo "${ERROR_CONTENT}" | grep -qi "sku\|tier"; then
        log_warn "⚠️  SKU/Tier-related error detected"
    fi
    
    if echo "${ERROR_CONTENT}" | grep -qi "location\|region"; then
        log_warn "⚠️  Location/Region-related error detected"
    fi
    
    if echo "${ERROR_CONTENT}" | grep -qi "conflict\|already exists"; then
        log_warn "⚠️  Resource conflict detected"
    fi
fi

print_header "END OF ERROR REPORT"
echo ""

log_info "For more information, check Azure portal deployment history or Azure CLI documentation"
log_info "Common fixes:"
echo "  - Verify resource SKUs are available in your region"
echo "  - Check subscription quotas and limits"
echo "  - Ensure service principal has required permissions"
echo "  - Validate parameter values match resource requirements"

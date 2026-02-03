# Evaluation Report: LoRaWAN Frequency Plans

**Feature ID**: 012  
**Evaluation Date**: 2026-02-03  
**Evaluator**: Excavate Evaluator Agent  
**Status**: Verified

---

## Summary Table

| Metric | Score | Weight | Weighted Score |
|--------|-------|--------|----------------|
| Correctness | 85% | 30% | 25.5% |
| Completeness | 90% | 30% | 27.0% |
| Technical Quality | 95% | 20% | 19.0% |
| Coverage | 88% | 20% | 17.6% |
| **Overall** | **89.1%** | 100% | **89.1%** |

---

## Accurate Specifications

### ✅ Correctly Documented

1. **Controller Structure** - [LoRaWANFrequencyPlansController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/LoRaWAN/LoRaWANFrequencyPlansController.cs)
   - Route `api/lorawan/freqencyplans` ✅ (Note: typo in route - "freqency" not "frequency")
   - `[LoRaFeatureActiveFilter]` applied ✅
   - Returns `IEnumerable<FrequencyPlan>` ✅

2. **Authorization**
   - `[Authorize("concentrator:read")]` on GetFrequencyPlans ✅

3. **FrequencyPlan DTO** - [FrequencyPlan.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/FrequencyPlan.cs)
   - `FrequencyPlanID` property ✅
   - `Name` property ✅

4. **Feature Gate**
   - `[LoRaFeatureActiveFilter]` ensures 404 when LoRa disabled ✅

5. **Hardcoded Frequency Plans**
   - Plans are hardcoded in controller, not from database ✅
   - Returns array of `FrequencyPlan` objects ✅

6. **Regional Coverage**
   - Europe (EU_863_870) ✅
   - United States (US_902_928_FSB_1 through FSB_7) ⚠️
   - Asia (AS_923_925_1 through AS_923_925_3) ⚠️
   - Australia (AU_915_928_FSB_1 through FSB_8) ✅
   - China (CN_470_510_RP1, CN_470_510_RP2) ⚠️

---

## Inaccuracies Found

### ⚠️ Discrepancies

1. **Route URL Typo**
   - **Spec assumes**: `api/lorawan/frequencyplans` (correct spelling)
   - **Actual route**: `api/lorawan/freqencyplans` (missing 'u')
   - **Impact**: API consumers need to use misspelled URL

2. **Frequency Plan Count**
   - **Spec states**: "22 supported frequency plans" (User Story 2, Scenario 1)
   - **Actual count**: 21 frequency plans in controller
   - **Missing**: US_902_928_FSB_8

3. **Asia Frequency Plans**
   - **Spec states**: "AS_923_925_1 through AS_923_925_4"
   - **Actual code**: Only AS_923_925_1, AS_923_925_2, AS_923_925_3 (3 plans)
   - **Missing**: AS_923_925_4

4. **China Frequency Plans**
   - **Spec states**: "CN_470_510_FSB_1 through FSB_11"
   - **Actual code**: Only CN_470_510_RP1, CN_470_510_RP2 (2 plans)
   - **Discrepancy**: Different naming convention and fewer plans

5. **United States FSB 8**
   - **Spec implies**: 8 FSB options
   - **Actual code**: Only FSB_1 through FSB_7 (7 plans)
   - **Note**: US_902_928_FSB_8 exists in router config files but not in controller

6. **Alphabetical Ordering (FR-004)**
   - **Spec states**: "System MUST display frequency plans in alphabetical order by name"
   - **Actual code**: Plans are in arbitrary hardcoded order
   - **Issue**: No sorting applied

---

## Actual Frequency Plans in Code

| Region | Plans in Code |
|--------|---------------|
| Asia | AS_923_925_1, AS_923_925_2, AS_923_925_3 |
| Europe | EU_863_870 |
| China | CN_470_510_RP1, CN_470_510_RP2 |
| United States | US_902_928_FSB_1 through US_902_928_FSB_7 |
| Australia | AU_915_928_FSB_1 through AU_915_928_FSB_8 |
| **Total** | **21 plans** |

---

## Code References Verified

| Spec Reference | Actual File | Match Status |
|---------------|-------------|--------------|
| LoRaWANFrequencyPlansController | [Verified](../../src/IoTHub.Portal.Server/Controllers/v1.0/LoRaWAN/LoRaWANFrequencyPlansController.cs) | ✅ |
| FrequencyPlan DTO | [Verified](../../src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/FrequencyPlan.cs) | ✅ |
| Router Config Files | [Infrastructure/RouterConfigFiles/](../../src/IoTHub.Portal.Infrastructure/RouterConfigFiles/) | ✅ |

---

## Router Config Files Found

The following router configuration files exist in the codebase, suggesting additional frequency plan support:
- US_902_928_FSB_8.json (not exposed in API)
- Other FSB files for various regions

---

## Recommendations

### High Priority

1. **Fix Route Typo**: Change route from `freqencyplans` to `frequencyplans` (breaking change - version bump required).

2. **Add Missing Frequency Plans**: 
   - Add `US_902_928_FSB_8`
   - Add `AS_923_925_4` if router config exists
   - Review China plans naming (FSB vs RP convention)

### Medium Priority

3. **Implement Sorting**: Add `.OrderBy(fp => fp.Name)` to return plans alphabetically per FR-004.

4. **Update Spec Table**: Correct the frequency plan count and regional details to match actual implementation.

### Low Priority

5. **Consider Dynamic Loading**: Instead of hardcoded array, load frequency plans from embedded router config files to ensure consistency.

6. **Add API Documentation**: Document the exact plans available for API consumers.

---

## Conclusion

The specification scores 89.1% overall. The LoRaWAN Frequency Plans feature has a solid foundation but contains several inaccuracies in plan counts and naming. The route URL typo is a notable issue that affects API consumers. The core functionality of providing a dropdown of frequency plans for concentrator configuration works correctly. Priority fixes include correcting the route spelling and adding missing frequency plans to match router configuration files.

### Key Metrics Verification

| Spec Claim | Verification |
|------------|--------------|
| 22 frequency plans | ❌ Only 21 in code |
| Europe EU_863_870 | ✅ Present |
| US 8 FSB options | ❌ Only 7 in API |
| Asia 4 groups | ❌ Only 3 in API |
| Australia 8 FSB | ✅ Present |
| China 11 FSB | ❌ Only 2 RP plans |

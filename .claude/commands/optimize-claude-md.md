# Optimize CLAUDE.md

This command helps maintain an optimized CLAUDE.md file by checking its size, analyzing duplication, and suggesting improvements.

## Your Task

1. **Analyze CLAUDE.md:**
   - Read the current CLAUDE.md file
   - Count total lines and identify sections
   - Check for duplication with:
     - `/docs/MODULE_BOUNDARIES.md`
     - `/docs/DESIGN_SYSTEM_CONTRIBUTING.md`
     - `/libs/design-system/README.md`

2. **Report findings:**
   - Current line count
   - Sections that could be condensed
   - Any duplication with supporting docs
   - Broken or missing links

3. **Provide optimization recommendations:**
   - Which sections should be extracted or condensed
   - Suggested file structure changes
   - Link updates needed

4. **If user approves, perform optimization:**
   - Extract duplicated content to appropriate docs
   - Update CLAUDE.md with references
   - Validate all links work correctly
   - Report final line count reduction

## Optimization Principles

- **CLAUDE.md should be ~200-300 lines** - Quick reference hub
- **Extract detailed documentation** to `/docs/` or existing READMEs
- **Use links liberally** to supporting documentation
- **Avoid duplication** - Single source of truth for each topic
- **Keep essential context** in CLAUDE.md:
  - Project overview
  - Architecture fundamentals
  - State management patterns
  - Import path examples
  - Feature creation guidelines

## File Organization Strategy

**CLAUDE.md** (quick reference):
- Project overview
- Quick command reference (common Nx commands)
- Architecture overview
- Module boundaries summary (link to /docs/MODULE_BOUNDARIES.md)
- State management patterns
- Routing structure
- Import paths
- Design system reference (link to /libs/design-system/README.md)
- Development guidelines

**`/docs/MODULE_BOUNDARIES.md`**:
- Detailed ESLint configuration
- Type and scope tag explanations
- Dependency rules with rationale
- Examples of violations and correct patterns

**`/docs/DESIGN_SYSTEM_CONTRIBUTING.md`**:
- Contributing to design system
- Adding components and tokens
- Publishing workflow
- Storybook documentation

**`/libs/design-system/README.md`**:
- Complete design system documentation
- Package architecture
- Usage examples
- Configuration

## Validation Checklist

After optimization, verify:

- [ ] All links in CLAUDE.md resolve correctly
- [ ] No duplication between CLAUDE.md and supporting docs
- [ ] CLAUDE.md is 200-300 lines
- [ ] Supporting docs are organized in `/docs/`
- [ ] Design system docs remain in `/libs/design-system/`
- [ ] Essential context remains in CLAUDE.md
- [ ] Cross-references between docs work correctly

## Example Usage

User runs: `/optimize-claude-md`

Expected output:
1. Analysis report of current state
2. Recommendations for optimization
3. If approved, perform optimizations
4. Final report with line count reduction and changes made

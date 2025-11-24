# Optimize CLAUDE.md (Generic)

This command helps maintain an optimized CLAUDE.md file by analyzing its size, identifying duplication, and suggesting improvements. This is a generic version that works with any project structure.

## Your Task

### 1. Discovery Phase

First, discover the project's documentation structure:

- Read the current CLAUDE.md file
- Search for all documentation files in the repository:
  - `/docs/**/*.md`
  - `/README.md` files in libraries/packages
  - Any `.md` files in project subdirectories
  - Common documentation patterns (`CONTRIBUTING.md`, `ARCHITECTURE.md`, etc.)

### 2. Analysis Phase

Analyze CLAUDE.md for optimization opportunities:

- **Size Analysis:**
  - Count total lines
  - Identify major sections
  - Measure section sizes relative to total

- **Duplication Detection:**
  - Compare content with discovered documentation files
  - Flag verbatim or near-duplicate content
  - Identify topics that appear in multiple places

- **Link Analysis:**
  - Validate all internal links (relative paths)
  - Check for broken or missing references
  - Identify sections without links that could reference detailed docs

- **Content Assessment:**
  - Identify overly detailed sections (implementation details, extensive examples)
  - Find sections that could be summarized with links
  - Look for outdated or redundant information

### 3. Report Findings

Present a clear analysis report:

```
## CLAUDE.md Optimization Report

### Current State
- Total lines: [count]
- Sections: [list major sections with line counts]

### Duplication Found
- [Section name]: Duplicates content in [file path]
- [Section name]: Similar content in [file path]

### Link Issues
- [Broken links found]
- [Missing links suggested]

### Optimization Opportunities
- [Section name]: Could be extracted to [suggested location]
- [Section name]: Could be condensed from [X] to ~[Y] lines
- [Section name]: Add link to [existing doc] instead of full content
```

### 4. Provide Recommendations

Offer specific, actionable recommendations:

- Which sections should be extracted to separate documentation files
- Suggested file names and locations for extracted content
- Which sections should be condensed to summaries with links
- Link updates needed
- Estimated line count after optimization

### 5. Perform Optimization (If Approved)

If the user approves your recommendations:

- Extract verbose content to appropriate documentation files
- Update CLAUDE.md with concise summaries and links
- Ensure all links are correct and functional
- Maintain essential context in CLAUDE.md
- Report final results: line count reduction, files created/modified

## Optimization Principles

### Size Guidelines

- **Target: 200-400 lines** for most projects
  - Small projects (1-2 apps): 150-250 lines
  - Medium projects (3-10 apps): 250-350 lines
  - Large monorepos (10+ apps): 300-500 lines

- CLAUDE.md should be a **quick reference hub**, not exhaustive documentation

### Content Strategy

**KEEP in CLAUDE.md:**
- Project overview (2-3 paragraphs)
- Quick command reference (most common commands)
- High-level architecture (key concepts only)
- Import/path conventions (brief examples)
- Essential development workflows
- Links to detailed documentation

**EXTRACT to separate docs:**
- Detailed architecture explanations
- Step-by-step tutorials and guides
- Extensive configuration details
- Component/library-specific documentation
- Contributing guidelines (if > 50 lines)
- Testing strategies (if > 30 lines)
- Deployment procedures

### Link Strategy

- Use relative links for internal documentation
- Link liberally - better to link than duplicate
- Keep 1-2 sentence summary before each link for context
- Prefer deep links to specific sections when possible

### Single Source of Truth

- Each topic should have ONE authoritative location
- CLAUDE.md references other docs, doesn't replace them
- Avoid "synced sections" that need updates in multiple places

## Documentation Organization Patterns

Suggest organizing extracted content using these common patterns:

### `/docs/` Directory (Recommended)

Standard documentation files:
- `ARCHITECTURE.md` - System design, patterns, principles
- `CONTRIBUTING.md` - Contribution guidelines
- `DEVELOPMENT.md` - Local setup, workflows, debugging
- `TESTING.md` - Testing strategy, patterns, commands
- `DEPLOYMENT.md` - Build and deployment procedures
- `API.md` - API documentation
- `TROUBLESHOOTING.md` - Common issues and solutions

### Library/Package READMEs

For monorepos or projects with libraries:
- `libs/{library}/README.md` - Library-specific documentation
- `packages/{package}/README.md` - Package usage and API

### Decision Records

For architectural decisions:
- `/docs/adr/` - Architecture Decision Records
- Format: `YYYY-MM-DD-short-title.md`

## Validation Checklist

After optimization, verify:

- [ ] All links in CLAUDE.md resolve correctly
- [ ] No significant duplication between CLAUDE.md and other docs
- [ ] CLAUDE.md is within target line count for project size
- [ ] Essential context remains in CLAUDE.md (can stand alone as entry point)
- [ ] Extracted content is in logical locations
- [ ] Each section in CLAUDE.md is concise and actionable
- [ ] Cross-references between docs work correctly
- [ ] No information was lost during extraction

## Example Output Format

When presenting recommendations, use this format:

```markdown
## Optimization Recommendations

### Summary
- Current: 450 lines
- Target: ~300 lines
- Reduction: ~150 lines (33%)

### Recommended Actions

1. **Extract "Testing Strategy" section** (80 lines)
   - Move to: `/docs/TESTING.md`
   - Replace with: 3-line summary + link
   - Savings: ~77 lines

2. **Condense "Module Boundaries" section** (60 lines)
   - Move details to: `/docs/ARCHITECTURE.md`
   - Keep: Summary of key rules (15 lines)
   - Savings: ~45 lines

3. **Extract "Contributing Guidelines"** (40 lines)
   - Move to: `/CONTRIBUTING.md` (root level)
   - Replace with: 2-line summary + link
   - Savings: ~38 lines

### Files to Create/Update
- Create `/docs/TESTING.md` (new file)
- Update `/docs/ARCHITECTURE.md` (add module boundaries section)
- Create `/CONTRIBUTING.md` (new file)
- Update `CLAUDE.md` (condensed version with links)

### Approve to proceed? (yes/no)
```

## Notes

- Always preserve the user's voice and project-specific terminology
- Don't remove essential context even if it makes CLAUDE.md longer
- Some projects legitimately need longer CLAUDE.md files - use judgment
- Focus on readability and maintainability, not just line count
- When in doubt, ask the user about their preferences before extracting content

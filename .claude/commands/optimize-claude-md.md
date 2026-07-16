# Optimize CLAUDE.md

**Automatically optimizes CLAUDE.md** by analyzing size, identifying duplication, extracting verbose content to separate docs, and creating a condensed quick-reference version.

## Your Task

Execute a **COMPLETE OPTIMIZATION** of CLAUDE.md. Do NOT present a plan - execute the optimization immediately and report results.

---

## Phase 1: Discovery & Analysis

### 1.1 Read Current CLAUDE.md
- Count total lines
- Identify major sections and their sizes
- Note current file structure

### 1.2 Discover Existing Documentation
Search for all documentation files:
- `/docs/**/*.md`
- Root-level documentation files (e.g., `/CONTRIBUTING.md`)
- Project-specific documentation patterns

### 1.3 Analyze for Optimization
- **Size Analysis:** Measure section sizes relative to total
- **Duplication Detection:** Compare content with existing docs
- **Content Assessment:** Identify overly detailed sections
- **Link Opportunities:** Find sections that should link to detailed docs

---

## Phase 2: Execute Optimization (Immediately)

### 2.1 Extract Verbose Sections

**ALWAYS extract these if present (>50 lines):**
- Code templates and examples → `/docs/guides/CODE-TEMPLATES.md`
- Step-by-step setup guides → `/docs/guides/SETUP-GUIDE.md`
- Configuration details → `/docs/guides/CONFIGURATION.md`
- Testing strategies → `/docs/guides/TESTING.md`
- Deployment procedures → `/docs/guides/DEPLOYMENT.md`
- API-specific guides → `/docs/<TOPIC>-GUIDE.md`

**Extraction Criteria:**
- Section > 80 lines → MUST extract
- Section > 50 lines → Should extract if detailed/tutorial content
- Section > 30 lines → Consider extracting if mostly examples/config

### 2.2 Create Documentation Files

For each extracted section:
1. **Create new doc file** with comprehensive content
2. **Replace in CLAUDE.md** with:
   - 2-3 sentence summary
   - Key bullet points (3-5 items max)
   - Link to detailed documentation
   - Example (if critical, <10 lines max)

**Template for replacement:**
```markdown
## [Section Name]

[2-3 sentence summary of what this section covers]

**Quick Reference:**
- Key point 1
- Key point 2
- Key point 3

**📖 See [DETAILED-GUIDE.md](./docs/DETAILED-GUIDE.md) for complete [topic] documentation.**
```

### 2.3 Condense Remaining Content

**Reduce these sections aggressively:**
- Project structure: Keep high-level only, remove file-by-file listings
- Implementation phases: Keep checklist format, remove detailed descriptions
- Commands reference: Keep 5-10 most common commands only
- Technology stack: List names only, remove version justifications

---

## Phase 3: Final Optimizations

### 3.1 Add Documentation Index
Create a documentation index at the top:
```markdown
## 📚 Documentation Index

- **[CODE-TEMPLATES.md](./docs/guides/CODE-TEMPLATES.md)** - Code patterns
- **[SETUP-GUIDE.md](./docs/guides/SETUP-GUIDE.md)** - Setup instructions
- **[CONFIGURATION.md](./docs/guides/CONFIGURATION.md)** - Configuration guide
- **[TESTING.md](./docs/guides/TESTING.md)** - Testing strategy
- **[DEPLOYMENT.md](./docs/guides/DEPLOYMENT.md)** - Deployment guide
```

### 3.2 Cross-Link Documents
Ensure all extracted docs:
- Link back to CLAUDE.md
- Link to related docs
- Use relative paths
- Are listed in the documentation index

### 3.3 Verify All Links
Check that all internal links work:
- Relative paths are correct
- Referenced files exist
- Section anchors are valid

---

## Target Line Counts

### By Project Size

**Small projects (1-3 apps):**
- Target: 150-250 lines
- Maximum: 300 lines

**Medium projects (4-10 apps):**
- Target: 250-350 lines
- Maximum: 400 lines

**Large monorepos (10+ apps):**
- Target: 300-450 lines
- Maximum: 500 lines

### If Over Target

**Be MORE aggressive:**
- Extract ALL code examples (keep 1-2 minimal examples max)
- Extract ALL configuration details (keep quick reference only)
- Extract ALL step-by-step guides
- Condense project structure to high-level overview
- Remove implementation details (link to code instead)

---

## Content Strategy

### KEEP in CLAUDE.md (Concise)
- ✅ Project overview (3-4 paragraphs)
- ✅ Quick start (< 20 lines)
- ✅ Architecture decisions summary (key points only)
- ✅ Project structure (high-level tree only)
- ✅ Most common commands (5-10 commands)
- ✅ Implementation phase checklist
- ✅ Links to detailed documentation
- ✅ Troubleshooting (top 3-5 issues only)

### EXTRACT to Separate Docs (Detailed)
- 🔄 All code templates and examples
- 🔄 Step-by-step tutorials
- 🔄 Configuration examples and details
- 🔄 Testing strategy and examples
- 🔄 Deployment procedures
- 🔄 API specifications
- 🔄 Technology comparisons
- 🔄 Best practices guides

---

## Optimization Principles

### 1. Single Source of Truth
- Each topic has ONE authoritative location
- CLAUDE.md is the entry point/index
- Detailed docs are the source of truth
- Never duplicate content

### 2. Progressive Disclosure
- CLAUDE.md shows what's available
- Links guide to deeper information
- Users choose depth based on needs

### 3. Maintainability
- Update once, reference many times
- Links > duplication
- Clear ownership of content

### 4. Scannability
- Use clear section headers
- Bullet points over paragraphs
- Tables for comparisons
- Code examples minimal in CLAUDE.md

---

## Output Format

After optimization, report:

```markdown
## ✅ CLAUDE.md Optimization Complete

### Results
- **Before:** [X] lines
- **After:** [Y] lines
- **Reduction:** [Z] lines ([P]%)

### Files Created/Updated
- ✅ Created `/docs/guides/CODE-TEMPLATES.md` ([lines] lines)
- ✅ Created `/docs/guides/SETUP-GUIDE.md` ([lines] lines)
- ✅ Created `/docs/guides/CONFIGURATION.md` ([lines] lines)
- ✅ Updated `CLAUDE.md` (condensed with links)

### Sections Extracted
1. [Section name] ([X] lines) → `/docs/[FILE].md`
2. [Section name] ([Y] lines) → `/docs/[FILE].md`

### Validation
- ✅ All links verified and working
- ✅ No content lost (all extracted to docs)
- ✅ CLAUDE.md is within target line count
- ✅ Documentation index created
- ✅ Cross-references working
```

---

## Important Guidelines

### DO:
- ✅ Execute optimization immediately (don't ask for approval)
- ✅ Be aggressive with extractions (aim for target line count)
- ✅ Create comprehensive extracted docs
- ✅ Verify all links work
- ✅ Preserve all information (just reorganize)
- ✅ Keep user's voice and terminology
- ✅ Update relative links correctly

### DON'T:
- ❌ Present a plan and wait for approval
- ❌ Do "first iteration" or "Phase 1" - do COMPLETE optimization
- ❌ Remove essential context from CLAUDE.md
- ❌ Break working links
- ❌ Lose any information during extraction
- ❌ Make CLAUDE.md just a table of contents (keep quick reference)

---

## Special Cases

### If CLAUDE.md is Already Optimized
If CLAUDE.md is already < 400 lines and well-organized:
- Report current status
- Suggest minor improvements only
- Don't force changes

### If Project Legitimately Needs Longer CLAUDE.md
Some complex projects need more context:
- Multi-language monorepos
- Complex microservices architectures
- Specialized domain knowledge required

In these cases:
- Target up to 500 lines
- Focus on organization over line count
- Ensure content is highly valuable

### If User Has Custom Documentation Structure
Respect existing documentation patterns:
- Match existing doc naming conventions
- Use established directory structure
- Maintain consistent formatting

---

## Example Transformation

**Before (Verbose):**
```markdown
## Code Templates (200 lines)

### 1. Domain Entity Template
[Full 30-line code example]

### 2. Repository Interface Template
[Full 40-line code example]

### 3. CQRS Command Template
[Full 35-line code example]

[... 150 more lines of examples ...]
```

**After (Concise):**
```markdown
## Code Patterns

All code follows established templates. See **[CODE-TEMPLATES.md](./docs/guides/CODE-TEMPLATES.md)** for complete examples.

**Quick Reference:**
- **Domain Entities:** POCOs with XML docs, zero dependencies
- **Repository Interfaces:** Task-based async methods
- **CQRS Commands:** `IRequest<TResponse>` records
- **Command Handlers:** `IRequestHandler<TRequest, TResponse>`

**📖 See [CODE-TEMPLATES.md](./docs/guides/CODE-TEMPLATES.md) for all 12 templates.**
```

**Result:** 200 lines → 15 lines (185 lines saved)

---

## Execution Checklist

Before reporting completion, verify:

- [ ] CLAUDE.md is within target line count for project size
- [ ] All verbose sections extracted to separate docs
- [ ] Documentation index created at top of CLAUDE.md
- [ ] All internal links verified and working
- [ ] No information lost during extraction
- [ ] Extracted docs are comprehensive and well-organized
- [ ] Cross-references between docs work correctly
- [ ] CLAUDE.md still provides quick reference value
- [ ] File structure is logical and discoverable
- [ ] Commit message prepared (if requested)

---

## Success Criteria

✅ **Optimization is successful if:**
1. CLAUDE.md is ≤ target line count for project size
2. All detailed content extracted to appropriate docs
3. CLAUDE.md remains useful as quick reference
4. All links work correctly
5. No information was lost
6. Documentation is better organized
7. Maintainability improved

---

**Remember:** Execute the COMPLETE optimization immediately. Do NOT present a plan or ask for approval. Report results after completion.

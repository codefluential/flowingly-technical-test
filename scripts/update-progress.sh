#!/bin/bash
# Progress Update Script
# Usage: ./scripts/update-progress.sh <task_id> <status> [test_type] [test_count]
# Example: ./scripts/update-progress.sh task_001 completed
# Example: ./scripts/update-progress.sh task_014 completed unit 7

set -e

TASK_ID="$1"
STATUS="$2"  # pending|in_progress|completed|blocked
TEST_TYPE="${3:-}"  # unit|contract|e2e
TEST_COUNT="${4:-0}"

if [ -z "$TASK_ID" ] || [ -z "$STATUS" ]; then
    echo "Usage: $0 <task_id> <status> [test_type] [test_count]"
    echo "Status: pending|in_progress|completed|blocked"
    echo "Test Type: unit|contract|e2e (optional)"
    exit 1
fi

TASKS_JSON="project-context/implementation/tasks/tasks.json"
PROGRESS_MD="project-context/implementation/PROGRESS.md"
BUILDLOG_MD="project-context/build-logs/BUILDLOG.md"

# Update tasks.json status using jq
jq --arg task "$TASK_ID" --arg status "$STATUS" --arg timestamp "$(date -Iseconds)" '
  # Update task status
  (.tasks[] | select(.id == $task) | .status) = $status |

  # Update timestamps
  if $status == "in_progress" then
    (.tasks[] | select(.id == $task) | .started_at) = $timestamp |
    .progress_tracking.current_task = $task
  elif $status == "completed" then
    (.tasks[] | select(.id == $task) | .completed_at) = $timestamp
  else
    .
  end |

  # Recalculate all metrics from actual task statuses
  .progress_tracking.tasks_completed = ([.tasks[] | select(.status == "completed")] | length) |
  .progress_tracking.tasks_in_progress = ([.tasks[] | select(.status == "in_progress")] | length) |
  .progress_tracking.tasks_blocked = ([.tasks[] | select(.status == "blocked")] | length) |
  .progress_tracking.tasks_pending = ([.tasks[] | select(.status == "pending" or .status == null)] | length) |

  # Update timestamp
  .progress_tracking.last_updated = $timestamp
' "$TASKS_JSON" > "$TASKS_JSON.tmp" && mv "$TASKS_JSON.tmp" "$TASKS_JSON"

# Update test counts if provided
if [ -n "$TEST_TYPE" ] && [ "$TEST_COUNT" -gt 0 ]; then
    jq --arg type "$TEST_TYPE" --argjson count "$TEST_COUNT" '
      .progress_tracking.tests_passing[$type] = $count |
      .progress_tracking.tests_passing.total = (
        .progress_tracking.tests_passing.unit +
        .progress_tracking.tests_passing.contract +
        .progress_tracking.tests_passing.e2e
      )
    ' "$TASKS_JSON" > "$TASKS_JSON.tmp" && mv "$TASKS_JSON.tmp" "$TASKS_JSON"
fi

# Get current stats
COMPLETED=$(jq -r '.progress_tracking.tasks_completed' "$TASKS_JSON")
TOTAL=$(jq -r '.total_tasks' "$TASKS_JSON")
TESTS=$(jq -r '.progress_tracking.tests_passing.total' "$TASKS_JSON")
CURRENT_MILESTONE=$(jq -r '.progress_tracking.current_milestone' "$TASKS_JSON")

# Update PROGRESS.md header
TASK_NAME=$(jq -r ".tasks[] | select(.id == \"$TASK_ID\") | .name" "$TASKS_JSON")
TIMESTAMP=$(date '+%Y-%m-%d %H:%M')
PERCENT=$(( COMPLETED * 100 / TOTAL ))
UNIT_TESTS=$(jq -r '.progress_tracking.tests_passing.unit' "$TASKS_JSON")
CONTRACT_TESTS=$(jq -r '.progress_tracking.tests_passing.contract' "$TASKS_JSON")
E2E_TESTS=$(jq -r '.progress_tracking.tests_passing.e2e' "$TASKS_JSON")

# Update top-level header (standalone format)
sed -i "s/^\*\*Last Updated\*\*:.*/\*\*Last Updated\*\*: $TIMESTAMP/" "$PROGRESS_MD"
sed -i "s/^\*\*Overall Progress\*\*:.*/\*\*Overall Progress\*\*: $COMPLETED\/$TOTAL tasks ($PERCENT%)/" "$PROGRESS_MD"

# Update Quick Status table (table format)
sed -i "s/| \*\*Current Task\*\* | .* |/| **Current Task** | $TASK_ID: $TASK_NAME |/" "$PROGRESS_MD"
sed -i "s/| \*\*Tasks Completed\*\* | .* |/| **Tasks Completed** | $COMPLETED\/$TOTAL ($PERCENT%) |/" "$PROGRESS_MD"
sed -i "s/| \*\*Tests Passing\*\* | .* |/| **Tests Passing** | $TESTS\/45 ($UNIT_TESTS unit, $CONTRACT_TESTS contract, $E2E_TESTS E2E) |/" "$PROGRESS_MD"

# Update task checkbox in PROGRESS.md
TASK_LINE=$(grep -n "\[ \] $TASK_ID:" "$PROGRESS_MD" | cut -d: -f1 | head -1)
if [ -n "$TASK_LINE" ] && [ "$STATUS" == "completed" ]; then
    sed -i "${TASK_LINE}s/\[ \]/[x]/" "$PROGRESS_MD"
fi

# Update milestone section headers with current progress using Python for reliability
python3 << 'PYTHON_SCRIPT'
import json
import re

# Read tasks.json
with open("project-context/implementation/tasks/tasks.json") as f:
    data = json.load(f)

# Calculate milestone progress
milestones = {
    "M0": "Minimal Scaffold",
    "M1": "Core Parsing & Validation",
    "M2": "API Contracts",
    "M3": "UI & E2E Tests"
}

milestone_stats = {}
for ms, name in milestones.items():
    tasks = [t for t in data["tasks"] if t.get("milestone") == ms]
    completed = [t for t in tasks if t.get("status") == "completed"]
    total = len(tasks)
    completed_count = len(completed)
    percent = (completed_count * 100 // total) if total > 0 else 0

    if completed_count == total:
        emoji, status = "✅", "Complete"
    elif completed_count > 0:
        emoji, status = "🔄", "In Progress"
    else:
        emoji, status = "⏳", "Not Started"

    milestone_stats[ms] = {
        "name": name,
        "emoji": emoji,
        "status": status,
        "completed": completed_count,
        "total": total,
        "percent": percent
    }

# Read and update PROGRESS.md
with open("project-context/implementation/PROGRESS.md") as f:
    lines = f.readlines()

output = []
i = 0
while i < len(lines):
    line = lines[i]

    # Check if line is a milestone header
    match = re.match(r'^### [^ ]+ (M\d): (.+?) \(\d+/\d+ tasks - \d+%\)$', line)
    if match:
        ms_id = match.group(1)
        if ms_id in milestone_stats:
            stats = milestone_stats[ms_id]
            # Replace header
            new_header = f"### {stats['emoji']} {ms_id}: {stats['name']} ({stats['completed']}/{stats['total']} tasks - {stats['percent']}%)\n"
            output.append(new_header)

            # Check next line for **Target** line and update it
            if i + 1 < len(lines) and lines[i + 1].startswith("**Target**"):
                target_match = re.match(r'^\*\*Target\*\*: ([^|]+) \| \*\*Status\*\*:', lines[i + 1])
                if target_match:
                    target_time = target_match.group(1).strip()
                    output.append(f"**Target**: {target_time} | **Status**: {stats['status']}\n")
                    i += 2  # Skip both milestone header and target line
                    continue
        else:
            output.append(line)
    else:
        output.append(line)

    i += 1

# Write back
with open("project-context/implementation/PROGRESS.md", "w") as f:
    f.writelines(output)

PYTHON_SCRIPT

# Re-index Serena after each completed task (for fresh code symbols)
# Only when status changes to "completed" (not when marking as in_progress or pending)
if [ "$STATUS" == "completed" ]; then
    echo ""
    echo "🔄 Re-indexing Serena MCP (task completed)..."
    if command -v uvx &> /dev/null; then
        # Run in background to not slow down progress updates
        (uvx --from git+https://github.com/oraios/serena serena project index > /dev/null 2>&1 &)
        echo "   ✓ Re-indexing started in background"
    else
        echo "⚠️  uvx not found, skipping Serena re-indexing"
    fi
fi

# Add to BUILDLOG if milestone completed (only if not already logged)
if [ "$STATUS" == "completed" ] && ([ "$TASK_NUM" == "010" ] || [ "$TASK_NUM" == "030" ] || [ "$TASK_NUM" == "040" ] || [ "$TASK_NUM" == "050" ]); then
    MILESTONE=""
    case "$TASK_NUM" in
        "010") MILESTONE="M0" ;;
        "030") MILESTONE="M1" ;;
        "040") MILESTONE="M2" ;;
        "050") MILESTONE="M3" ;;
    esac

    # Check if milestone already logged in milestones_completed array
    ALREADY_LOGGED=$(jq --arg milestone "$MILESTONE" '.progress_tracking.milestones_completed | contains([$milestone])' "$TASKS_JSON")

    if [ "$ALREADY_LOGGED" == "false" ]; then
        echo "" >> "$BUILDLOG_MD"
        echo "## $(date '+%Y-%m-%d %H:%M') - $MILESTONE Milestone Complete" >> "$BUILDLOG_MD"
        echo "" >> "$BUILDLOG_MD"
        echo "**Changes**:" >> "$BUILDLOG_MD"
        echo "- Completed all $MILESTONE tasks successfully" >> "$BUILDLOG_MD"
        echo "- $MILESTONE DoD verification passed (task_$TASK_NUM)" >> "$BUILDLOG_MD"
        echo "" >> "$BUILDLOG_MD"
        echo "**Testing**:" >> "$BUILDLOG_MD"
        echo "- Tests passing: $TESTS/45" >> "$BUILDLOG_MD"
        echo "" >> "$BUILDLOG_MD"
        echo "**Next Steps**:" >> "$BUILDLOG_MD"
        if [ "$TASK_NUM" != "050" ]; then
            NEXT_TASK=$(printf "task_%03d" $((10#$TASK_NUM + 1)))
            echo "- Proceed to $NEXT_TASK" >> "$BUILDLOG_MD"
        else
            echo "- Phase 1 complete! Product is submittable." >> "$BUILDLOG_MD"
        fi
        echo "" >> "$BUILDLOG_MD"
        echo "---" >> "$BUILDLOG_MD"

        # Update progress_tracking milestones (only if not already present)
        jq --arg milestone "$MILESTONE" '
          .progress_tracking.milestones_completed += [$milestone]
        ' "$TASKS_JSON" > "$TASKS_JSON.tmp" && mv "$TASKS_JSON.tmp" "$TASKS_JSON"
    fi
fi

echo "✅ Progress updated: $TASK_ID → $STATUS"
echo "   Tasks: $COMPLETED/$TOTAL ($(( COMPLETED * 100 / TOTAL ))%)"
echo "   Tests: $TESTS/45"

# Show what to commit
if [ "$STATUS" == "completed" ]; then
    echo ""
    echo "📝 Suggested commit message:"
    echo "   $TASK_ID: $(jq -r ".tasks[] | select(.id == \"$TASK_ID\") | .name" "$TASKS_JSON")"
fi

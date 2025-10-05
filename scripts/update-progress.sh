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
  (.tasks[] | select(.id == $task) | .status) = $status |
  if $status == "in_progress" then
    (.tasks[] | select(.id == $task) | .started_at) = $timestamp |
    .progress_tracking.current_task = $task
  elif $status == "completed" then
    (.tasks[] | select(.id == $task) | .completed_at) = $timestamp |
    .progress_tracking.tasks_completed += 1 |
    .progress_tracking.tasks_pending -= 1
  elif $status == "blocked" then
    .progress_tracking.tasks_blocked += 1 |
    .progress_tracking.tasks_pending -= 1
  else
    .
  end |
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

sed -i "s/^\*\*Last Updated\*\*:.*/\*\*Last Updated\*\*: $TIMESTAMP/" "$PROGRESS_MD"
sed -i "s/^\*\*Current Task\*\*:.*/\*\*Current Task\*\*: $TASK_ID: $TASK_NAME/" "$PROGRESS_MD"
sed -i "s/^\*\*Tasks Completed\*\*: [0-9]*\/[0-9]* .*/\*\*Tasks Completed\*\*: $COMPLETED\/$TOTAL ($PERCENT%)/" "$PROGRESS_MD"
sed -i "s/^\*\*Tests Passing\*\*:.*/\*\*Tests Passing\*\*: $TESTS\/45/" "$PROGRESS_MD"

# Update task checkbox in PROGRESS.md
TASK_LINE=$(grep -n "\[ \] $TASK_ID:" "$PROGRESS_MD" | cut -d: -f1 | head -1)
if [ -n "$TASK_LINE" ] && [ "$STATUS" == "completed" ]; then
    sed -i "${TASK_LINE}s/\[ \]/[x]/" "$PROGRESS_MD"
fi

# Add to BUILDLOG if milestone completed
TASK_NUM=$(echo "$TASK_ID" | sed 's/task_//')
if [ "$STATUS" == "completed" ] && ([ "$TASK_NUM" == "010" ] || [ "$TASK_NUM" == "030" ] || [ "$TASK_NUM" == "040" ] || [ "$TASK_NUM" == "050" ]); then
    MILESTONE=""
    case "$TASK_NUM" in
        "010") MILESTONE="M0" ;;
        "030") MILESTONE="M1" ;;
        "040") MILESTONE="M2" ;;
        "050") MILESTONE="M3" ;;
    esac

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

    # Update progress_tracking milestones
    jq --arg milestone "$MILESTONE" '
      .progress_tracking.milestones_completed += [$milestone]
    ' "$TASKS_JSON" > "$TASKS_JSON.tmp" && mv "$TASKS_JSON.tmp" "$TASKS_JSON"
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

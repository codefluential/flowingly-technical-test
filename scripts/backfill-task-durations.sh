#!/bin/bash
# Backfill duration data for tasks 001-030 from git commit timestamps
# This script analyzes git log to estimate started_at and completed_at times

set -euo pipefail

TASKS_JSON="project-context/implementation/tasks/tasks.json"

# Create a temporary file for the updated JSON
TEMP_JSON=$(mktemp)

# Function to get first commit timestamp for a task (when it started - usually task file creation or first implementation commit)
get_task_start() {
    local task_id="$1"
    # Look for commits that mark task as in_progress or first implementation commit
    git log --all --reverse --format="%ai" --grep="mark ${task_id}" --grep="task ${task_id}" --grep="${task_id}:" -i | head -1
}

# Function to get last commit timestamp for a task (when it completed)
get_task_end() {
    local task_id="$1"
    # Look for commits that mark task as completed or final implementation
    git log --all --format="%ai" --grep="mark ${task_id} completed" --grep="completed.*${task_id}" -i | head -1
}

echo "Analyzing git log to backfill task durations for tasks 001-030..."

# Process tasks 001-030
for i in $(seq -w 1 30); do
    task_id="task_0$i"

    # Get timestamps from git log
    start_time=$(get_task_start "$task_id")
    end_time=$(get_task_end "$task_id")

    if [[ -n "$start_time" && -n "$end_time" ]]; then
        echo "Found timestamps for $task_id:"
        echo "  Started: $start_time"
        echo "  Completed: $end_time"

        # Update the JSON file with jq
        jq --arg task_id "$task_id" \
           --arg started_at "$start_time" \
           --arg completed_at "$end_time" \
           '(.tasks[] | select(.id == $task_id)) |=
            (. + {started_at: $started_at, completed_at: $completed_at})' \
           "$TASKS_JSON" > "$TEMP_JSON"

        mv "$TEMP_JSON" "$TASKS_JSON"
    else
        echo "No complete timestamp data for $task_id (start: ${start_time:-none}, end: ${end_time:-none})"
    fi
done

echo ""
echo "Backfill complete! Updated $TASKS_JSON"
echo ""
echo "Summary of tasks with duration data:"
jq -r '.tasks[] | select(.started_at != null and .completed_at != null) | "\(.id): \(.name)"' "$TASKS_JSON"

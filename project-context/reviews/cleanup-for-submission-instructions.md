# Instructions

Instrcuition of what to clean up for submissions. All this will be preserved in a branch called local_dev - this will not be published to remote rep. The dev and main branch will be cleansed for submission and are in the remote repo.

## Remove from project root the following folders:
- claude
- playwright
- serena

## Remove from scripts folder in project root the files:
-  reindex-serena.sh
- backfill-task-durations.sh
- update-progress.sh 
- FIXES-SUMMARY.md
- TEST-SIGNAL-TRAP.md
- VALIDATION.md
- Once above remove updatethe README.md file in the folder


# clenup of project-context folder
- remove folders:
    - agents
    - archive
    - build-logs
    - codex
    - learnings
    - reviews

# cleanup of /home/adarsh/dev/codefluent/flowingly-technical-test/project-context/implementation folder:
    - remove dashboard folder
    - remove files ENVIRONMENT.md, TRACKING-WORKFLOW.md
    - remove /home/adarsh/dev/codefluent/flowingly-technical-test/project-context/implementation/README.md - as it not opening and says its binary



## Once all files cleaned up

- Update @CLAUDE.md with /init command
- Update /home/adarsh/dev/codefluent/flowingly-technical-test/README.md to reflect the cleansed state and for the pupose of the reviewer to know how to install and run the app and the details pertinent to the review rather than other matters related to the items removed from the project.
- update /home/adarsh/dev/codefluent/flowingly-technical-test/project-context/README.md for items that no longer exist from above cleanup
Addressing the feed on Flowingly Parsing Service (Gaps / divergences / things to tighten) — PRD + Technical Specification (v0.2) document (@project-context/specifications/prd-technical_spec.md):

1. Given the time constraints, lets do as suggested and use SQLite for unit/integration tests only (fast, hermetic) while we are developing and allow it to run locally with this as the back-end for review. Lets push postgres/render to later stages. Ensure we have a way to create the postgres version off the SQLite version when we hit that milestone. This give us best agility to deliver. Ensure you review an updat ethe releavnt ADR document in the @project-context/adr folder.

2. Agreed - add atest for this - update the @project-context/specifications/prd-technical_spec.md

3. Agreed - add in as as proposed into @project-context/specifications/prd-technical_spec.md

4. Agreed - keep it minimal. Add a rule to ignore ambiguous times (e.g., “7.30pm”, “19:30”) if not reliably detectable,

5. API responses should be specific to the input conext - i.e. either contains expense block or other. No combined but separate reponses. Updated releavt section and ADR documents.

6. Keep a backlog note if not essential for core requirements. Update relevant ADR and the prd.

7. Agreed - add the clarification sentence as proposed - “Request taxRate wins over config; if absent, use config; if both absent, fail with MISSING_TAXRATE or fallback to 0.15.” and test both.

8. This is a good idea - will bake it into the demo app and make it reusable to swagger. But this is after core items are deleivered. Need to think about how to implement this (config cases user can choose in UI or execute with swagger)

9. Agreed - Add both sample emails from the brief to ensure parity with the scenarios. These are separate scenerios.

10. Agree ensure the README has a two-minute/quick reviewer path.


Others:

- Also agree with the 'Risk & scope guardrails' as recommened so we should do this.

- Also agree with the 'Concrete implementation tweaks' as recommened so we should do this.



First rvewi the  @project-context/specifications/prd-technical_spec.md and make a plan to update it for the above feedback. Don't modify the ADR yet. we will do thatnext once the prd tech spec is updated.

	

	



# Review notes and feedback for the prd-technical_spec.md ( Flowingly Parsing Service — PRD + Technical Specification (v0.1)):

## Section 3
 - explain the Hexagon architecture - what is is and why chosen and what were the options
 - Domain - explain what you mean by parsing/normalization
 - Command/Query - explain evnet sourcing - what it is and why not used in v1?
 - Ports/Adapters - Explain the items - what these are and what you mean by 'DI-backed implementations'

## Section 4
  - Explain Errors 400 - why 400 - what does it mean?
  
## Section 4.2 
	- Explain for XML Island 'secure XMl settings (no DTD/XXE)
	- For Date and Time - should we also store as UTC? Why or why not? Will the chosen methods suffice?
	
## Section 5
	-  explain CORS  - what it is and why we 'restrict to known origins'
	- Performance - explain ther terminlogy '...200ms p50 samll payloads'
	

## Section 6
	- State & API - any need for Redux - why or why not?
	- Testing is E@E same as integration testing?
	- Accessibility - should we add ability to turn on high contract colours and chnage text size (have controls on UI)
	
## Section 7
	- Stack - please explain wht these are - wht they do - FluentAssertions, FluentValidation, Serilog. Is Serlog free to use?
	- Patterns - explain the Strategy and Pipleine pattern in general and also how it is applied for this solution with examples.
	- Swaager - explain what swahbuckle is and why we use it and how it helps this project.
	
## Section 8 
	- We will use Postgres only (we will use Render platfomr for all asspects)
	- Do we need to specify the relationships, inexes, constraints (FK) and ERD
	- What is the need for the content hash ( for idempotency as stated) - explain what this and why we do this

## Section 13
    - Are there other BDD scenarios we can add to cover the examples and rules we have for core and some edge cases?

## Section 15
    - What's prometehus? Do we need it? is it free?

## sECTION 16
    - let stik to one platform to keep things simple and use Render - It has postgress and web service - use free tier - check put the lmitation there so we document it in the README.md for the reiewer. We will use Render Github Integration and Blueprint for simple deployment
    - Environment - just have one Render environme for Prod which will like to main branch. For Dev we will run locally.

## Section 17 - Answer to Open Questions (for Product/Tech Decisions)

1. **Default tax rate & currency**: Assume NZ GST 0.15 and `NZD`? Yes
2. **Reservation processing**: For Phase 2, which fields constitute MVP (vendor/date/time/party_size)? We do not need to consider Phase 2
3. **Auth**: Should Prod require an **API key** header from day one? What implication does this have and what does it deomstrate. Explain why we need it and what it would entail and why it would be good to have (or otherwise why not needed).
4. **P99 Targets**: Any explicit latency targets and payload size ceilings? State reasonable ones for objective of this demo app.
5. **XSD**: Turn on by default for `<expense>` island, or keep off initially? Yes.
6. **Storage**: OK with Neon Postgres for Prod, SQLite for Dev? Just Postgres only, no SQLite. We will deploy whole stack to Render.
7. **Versioning**: URI-only or also support media-type header versioning? What is most practical. explain what these two options are.
8. **Logging retention**: Any specific policy (days or size)? Set app wide config value for 30 days default.

## Section 20.3 
-  we can ignore these as we only need to deliver phase 1. Esnuire we scope things accordingly.







# Bookify — Project Documentation

Status: Draft — baseline documentation for Bookify.  
Target: .NET 8 (Razor Pages / ASP.NET Core). Backend: `Bookify.Web`, data: `Bookify.Data`.

---

## 1. Project Planning & Management

### Project Proposal
- Overview: Bookify — hotel reservation system to browse room types, select dates, add to cart and checkout (Stripe).
- Objectives: enable reliable reservations, admin management, secure payments (Stripe test keys for dev).
- Scope: search, booking flows, admin CRUD, authentication/roles, seeding, health checks.

### Project Plan (sample milestones)
- Week 1: Requirements & design (ERD, wireframes)
- Week 2: Models, DbContext, Identity
- Week 3: Rooms and RoomTypes CRUD + availability search
- Week 4: Booking, cart, Stripe checkout
- Week 5: Admin dashboard, seeding
- Week 6: Testing, documentation, presentation

### Roles & Task Assignment
- Developer(s): implementation, migrations, tests
- QA: test cases, integration tests
- Product owner / Lecturer: acceptance and grading

### Risks & Mitigation
- DB connectivity issues → provide SQL script + `TrustServerCertificate=True` for dev only.
- Payment integration changes → use Stripe sandbox keys and abstract payment logic.
- Permission issues creating DB → provide manual DB creation script for DBA.

### KPIs
- API response time < 300ms
- Uptime > 99.5%
- Booking success rate target > 95%

---

## 2. Lecturer Review
(Placeholder for lecturer feedback and final grading criteria)
- Suggested improvements and evaluation notes to be added after review.
- Example grading split: Documentation 30% | Implementation 40% | Testing 20% | Presentation 10%

---

## 3. Requirements Gathering

### Stakeholders
- Customers, Admin/Hotel staff, DBA, Lecturer

### User Stories (examples)
- Search rooms by date and type.
- Add room to cart and checkout with Stripe.
- Admin CRUD for room types, rooms, and bookings.
- Users register/login; admins manage data.

### Functional Requirements
- Authentication / roles (Admin, Customer)
- RoomType & Room management
- Availability search and booking
- Stripe checkout & webhooks
- Admin dashboard: bookings, rooms, room types

### Non-functional Requirements
- Security: HTTPS, secure secret storage
- Performance: indexes on booking dates
- Reliability: seeding + migrations at startup (with permissions)
- Maintainability: layered design (Web / Services / Data)

---

## 4. System Analysis & Design

### Architecture
- Monolithic Razor Pages / MVC app with layers:
  - Presentation: `Bookify.Web`
  - Services: `Bookify.Services`
  - Data: `Bookify.Data` (EF Core)
  - Identity: ASP.NET Identity

### Core Entities (summary)
- `ApplicationUser` (AspNetUsers)
- `RoomType` (RoomTypes)
- `Room` (Rooms) — FK -> `RoomType`
- `Booking` (Bookings) — FK -> `Room`, FK -> `ApplicationUser`

ER Diagram (simple)

```
graph LR
AU["ApplicationUser (AspNetUsers)"]
RT["RoomType (RoomTypes)"]
R["Room (Rooms)"]
B["Booking (Bookings)"]

AU -->|1..* Bookings| B
RT -->|1..* Rooms| R
R -->|1..* Bookings| B
```

### DB Indexes & Constraints
- `Rooms.RoomNumber` UNIQUE
- `RoomTypes.Name` indexed
- `Bookings.CheckInDate`, `Bookings.CheckOutDate`, `Bookings.RoomId`, `Bookings.UserId` indexed

---

## 5. Data Flow & Behaviour

### Checkout Sequence (summary)
1. User validates availability.
2. Create booking in Pending state and create Stripe Checkout session.
3. User pays on Stripe; webhook confirms payment.
4. On webhook success: booking marked Confirmed, store Stripe IDs.

Sequence diagram (summary)

```
sequenceDiagram
Customer->>WebApp: Checkout
WebApp->>BookingService: Validate availability
BookingService->>DB: Check existing bookings
WebApp->>Stripe: Create Checkout Session
Stripe-->>WebApp: SessionId
Stripe->>WebApp: Webhook(payment succeeded)
WebApp->>BookingService: ConfirmBooking
BookingService->>DB: Update booking status
```

---

## 6. UI/UX Design & Prototyping
- Bootstrap-based responsive UI.
- Key screens: Home, Rooms search, Room details, Cart, Checkout, Bookings (user), Admin dashboard.
- Accessibility: semantic HTML, labels, color contrast, keyboard navigation.

---

## 7. System Deployment & Integration

### Tech Stack
- .NET 8, ASP.NET Core (Razor Pages/MVC)
- EF Core 8, Microsoft SQL Server
- ASP.NET Identity
- Stripe.net
- Serilog logging

### Deployment Notes
- Connection string in `Bookify.Web\appsettings.json`.
- For development: `TrustServerCertificate=True` is present; remove for production.
- CI/CD: build, run tests, and deploy migrations to target DB.

---

## 8. Implementation & Source Code

### Important files
- `Bookify.Data\ApplicationDbContext.cs` — models & configuration
- `Bookify.Data\Models\{ApplicationUser,RoomType,Room,Booking}.cs`
- `Bookify.Web\Program.cs` — DI, DbContext, Identity, seeding
- `Bookify.Services` — business logic (BookingService, RoomService)

### Running locally (short)
1. Update `Bookify.Web\appsettings.json` if needed.
2. Build and apply migrations:
   - `dotnet ef database update --project Bookify.Data --startup-project Bookify.Web`
3. Run:
   - `dotnet run --project Bookify.Web`

If DB creation fails due to permissions, run the provided SQL ensure script or ask DBA to create `BookifyDb`.

---

## 9. Testing & QA
- Unit tests for services.
- Integration tests with a test DB.
- Manual UAT for booking & payment flows.
- Logging: Serilog writes to `logs/bookify-.txt`.

---

## 10. Deliverables & Presentation
- `docs/Project_Documentation.md` (this file)
- SQL schema script: `Bookify.Sql/create_bookify_schema.sql` (optional)
- README with install and run steps
- Presentation slides

---

## Appendix — Useful Commands
- Apply EF migrations:
  - `dotnet ef migrations list --project Bookify.Data --startup-project Bookify.Web`
  - `dotnet ef database update --project Bookify.Data --startup-project Bookify.Web`
- Ensure DB via T-SQL:
  - `IF DB_ID(N'BookifyDb') IS NULL CREATE DATABASE [BookifyDb];`
	- 
## Team Member
	- Mohamed Mostafa	mohamedmostafabedir@gmail.com
	- Shams Hesham	    shams.hesham05@gmail.com
	- Omar Elshazly	    Omarmelshazly22@gmail.com
	- Saif Adin Ahmed	saifahmedelbattawy@gmail.com
	- Saif Adin Ashraf	seifeldinashraf06@gmail.com
	- Nada Sami	        nadasami141@gmail.com


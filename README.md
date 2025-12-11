# OIG assessment

Please fork the repository to your account, and add a commit message indicating you have started the assessment. Once completed, please let us know so we can schedule the next phase.

---

# User Permissions Management Module

You are required to develop a module for managing users, roles, and an organizational hierarchy.  
The module must include both a backend (REST API, CQRS, DDD) and a simple user interface (Blazor).

---

## Business Entities and Rules

### User
- Has a name, a unique email, and a list of assigned roles.
- Belongs to a specific organization.
- Receives permissions based on the roles available within their organization and all of its descendant organizations.

### Organization
- Has a name (not necessarily unique).
- Is part of a hierarchical structure where an organization may have any number of nested child organizations (unlimited depth).
- Roles available to the user may come from the user’s own organization or any parent organization above it in the hierarchy.

### Role
- Has a name and a list of permissions.
- Belongs to a single organization.
- The list of available permissions is static and defined in the code.

**Examples of basic permissions (not exhaustive):**
- View user list  
- Add user  
- View roles list  
- Add role  

---

## Functional Requirements

You must implement:

### User Editing Page
- Modify user details
- Assign roles according to hierarchical rules

### Role Editing Page
- Change the role name
- Edit the role’s permissions

### Organization Editing Page
- Modify organization details and structure

### User List Page
- Display users belonging to the current organization and all of its nested organizations

### Role List Page

### Organization Navigation Component
- Used on organization and user list pages
- Must allow navigation across hierarchy levels

---

## Search
- Implement basic search by partial name match.
- The server must return only the minimal data required for the current user.

---

## Permission System
- Implement the access control system within this module.
- Access logic must apply to all UI pages and API endpoints.

---

## Deliverables

The candidate must:

- Perform task decomposition.
- Create diagrams for system components, architecture, and data models.
- Design the API and document key endpoints.
- Explain how the module can be integrated into an existing project:
  - which integration pattern to use,
  - which tools are required,
  - how to ensure compatibility with existing business rules.

---

## Technical Requirements

- **API:** REST, CQRS, DDD  
- **Front-end:** Blazor  
- **Validation:** minimal, implemented both client- and server-side  
- **Data storage:** any;  
  - if using NoSQL, the candidate must explain how the relational model would look.

---

## General Guidelines

- The primary focus is on functionality and business rules rather than UI.
- The interface may be as simple as needed.
- If the candidate cannot complete all functionality:
  - they must implement the most critical part,
  - and clearly explain how the remaining parts would be finished in a real project.
- **Additionally:** The candidate must explain how the module’s performance and reliability could be improved in the future if the number of organizations, users, and roles grows significantly.  
  Expected topics include strategies for scaling, optimizing access patterns, caching, and architectural improvements.

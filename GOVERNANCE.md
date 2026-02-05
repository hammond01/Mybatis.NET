# Project Governance

This document outlines the governance model for the MyBatis.NET project.

## 1. Governance Model

MyBatis.NET operates under a "Benevolent Dictator for Life" (BDFL) model with a Core Team structure. This means that while community consensus is highly valued and sought after, the Lead Maintainer has the final say in case of deadlocks or major strategic disagreements.

## 2. Roles and Responsibilities

### Lead Maintainer (BDFL)
*   **Role**: Project Founder & Strategic Lead
*   **Responsibilities**:
    *   Final decision maker on major architectural changes and roadmap.
    *   Administrator of project assets (repo, NuGet packages, domain).
    *   Arbitrator in case of community disputes.
*   **Current Lead**: Hammond

### Core Team
*   **Role**: Active maintainers with commit access
*   **Responsibilities**:
    *   Review and merge Pull Requests.
    *   Triage Issues (labeling, closing, responding).
    *   Maintain documentation.
    *   Release new versions to NuGet.
    *   Participate in strategic discussions.
*   **Access**: Write access to the repository, admin access to Discord/Community channels.

### Contributors
*   **Role**: Community members who contribute code, docs, or support.
*   **Responsibilities**:
    *   Submit Pull Requests.
    *   Report bugs and suggest features.
    *   Help other users in discussions.

## 3. Decision Making Process

### Consensus Seeking
We aim for consensus on all decisions. Most day-to-day decisions (bug fixes, small features, refactoring) are made by the Core Team members involved in the discussion.

### Proposal Process (RFC)
For major changes (breaking changes, new core modules, significant architectural shifts), an RFC (Request for Comments) process is required:
1.  Open a GitHub Discussion/Issue labeled `RFC`.
2.  Describe the proposal in detail.
3.  Allow at least **1 week** for community feedback.
4.  Core Team reviews and votes.

### Voting
When a formal vote is needed among the Core Team:
*   **Simple Majority**: Used for most operational decisions.
*   **Consensus (or BDFL ruling)**: Required for breaking changes or changes to Governance.

## 4. Expected Behavior

All participants in the project governance are expected to abide by the [Code of Conduct](CODE_OF_CONDUCT.md).

## 5. Changes to Governance

Changes to this governance document can be proposed via a Pull Request and require approval from the Lead Maintainer and a majority of the Core Team.

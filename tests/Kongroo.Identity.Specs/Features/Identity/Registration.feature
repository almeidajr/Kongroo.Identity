@webapi @identity
Feature: Identity registration
  Players need an account before they can authenticate and use their game library.

Rule: Valid registration creates a user account

Scenario: User registers with valid account data
  When the visitor registers with username "kongroo", email "kongroo@example.com", password "Sup3rSecure!", and name "Kongroo Cloud Games"
  Then the registration response should be created
  And the registered user profile should contain username "kongroo", email "kongroo@example.com", and name "Kongroo Cloud Games"

Rule: Registration validates account data

Scenario: User registration rejects an invalid email
  When the visitor registers with username "kongroo", email "invalid-email", password "Sup3rSecure!", and name "Kongroo Cloud Games"
  Then the response should be bad request
  And the response should contain problem details

Scenario: User registration rejects a weak password
  When the visitor registers with username "kongroo", email "kongroo@example.com", password "weak", and name "Kongroo Cloud Games"
  Then the response should be bad request
  And the response should contain problem details

Rule: Registration keeps usernames and email addresses unique

Scenario: User registration rejects a duplicate username
  Given a user exists with username "kongroo", email "kongroo@example.com", password "Sup3rSecure!", and name "Kongroo Cloud Games"
  When the visitor registers with username "kongroo", email "another@example.com", password "Sup3rSecure!", and name "Another User"
  Then the response should be conflict

Scenario: User registration rejects a duplicate email
  Given a user exists with username "kongroo", email "kongroo@example.com", password "Sup3rSecure!", and name "Kongroo Cloud Games"
  When the visitor registers with username "another-user", email "kongroo@example.com", password "Sup3rSecure!", and name "Another User"
  Then the response should be conflict

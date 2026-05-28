@webapi @identity
Feature: Identity authentication
  Registered users need bearer tokens to access authenticated platform resources.

Rule: Valid credentials issue a bearer token

Scenario: User logs in with valid credentials
  Given a user exists with username "kongroo", email "kongroo@example.com", password "Sup3rSecure!", and name "Kongroo Cloud Games"
  When the user logs in with username "kongroo" and password "Sup3rSecure!"
  Then the login response should be ok
  And the login response should contain a bearer access token

Scenario: User login rejects invalid credentials
  Given a user exists with username "kongroo", email "kongroo@example.com", password "Sup3rSecure!", and name "Kongroo Cloud Games"
  When the user logs in with username "kongroo" and password "WrongPassword!1"
  Then the response should be unauthorized

Rule: Bearer tokens authenticate the current user

Scenario: Authenticated user retrieves their own profile
  Given a user exists with username "kongroo", email "kongroo@example.com", password "Sup3rSecure!", and name "Kongroo Cloud Games"
  And the user is logged in with username "kongroo" and password "Sup3rSecure!"
  When the authenticated user requests their profile
  Then the profile response should be ok
  And the profile should contain username "kongroo", email "kongroo@example.com", and name "Kongroo Cloud Games"

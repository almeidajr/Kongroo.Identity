@webapi @identity
Feature: Identity health probes
  Kubernetes probes need dedicated liveness and readiness endpoints.

Scenario: Liveness probe reports healthy
  When the "/health/live" probe endpoint is requested
  Then the probe response should be ok

Scenario: Readiness probe reports healthy
  When the "/health/ready" probe endpoint is requested
  Then the probe response should be ok

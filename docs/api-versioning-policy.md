# TMS API Versioning Policy

The TMS API uses versioning to allow the API to evolve without unexpectedly breaking existing clients.

## Breaking Changes

A change is considered breaking when an existing client may stop working or behave differently without changing its code. Breaking changes require a new API version.

Examples include:

* Removing an existing response field.
* Renaming an existing field.
* Changing the meaning or data type of a field.
* Changing an existing HTTP status code.
* Tightening validation rules for previously accepted requests.
* Changing a default sort order or other established default behavior.

## Additive Changes

Additive changes extend the API without changing existing behavior and normally do not require a new API version.

Examples include:

* Adding a new optional response field.
* Adding a new endpoint.
* Adding a new optional query parameter.
* Adding new functionality that does not alter existing contracts.

## Sunset Window

When a replacement API version is released, the previous version will remain available for a minimum of six months.

This migration window gives all clients, including rural training centres operating on quarterly maintenance schedules, sufficient time to test and migrate before the old version is removed.

## Deprecation Communication

From the first day a replacement version is available, deprecated API versions will communicate their migration status through:

* `Deprecation` response headers.
* `Sunset` response headers containing the planned shutdown date.
* `Link` response headers identifying the successor API version.
* A CHANGELOG entry describing the new version and migration requirements.
* Email notification to every team that holds an API key.
* A calendar invitation for the scheduled shutdown date.

## Skipping Versions

Clients are not required to migrate through every intermediate API version.

For example, a client using V1 may migrate directly to V3 if V3 is the appropriate supported version. Each client is responsible for adapting directly to the contract of the version it chooses.

This policy applies to all public TMS API contracts. When there is uncertainty about whether a proposed change is breaking, the change should be treated as breaking until its compatibility has been reviewed.

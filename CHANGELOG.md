<!-- markdownlint-disable no-duplicate-heading -->

# Changelog

All notable changes to this project will be documented in this file.

The format is based on <https://common-changelog.org/>, and this project adheres mostly to Semantic Versioning.

## [0.4.0] - 2024-07-01

### Added

- Users can add variable names after `@query` in a storylet header to have these variables bind to the knot's input parameters.

## [0.3.0] - 2024-06-08

This update focused on making the story controller more general purpose for projects that want to take advantage of the storylet system for dialogue, but who don't need all the additional infrastructure provided by the full framework.

### Added

- Added `Anansi.Story` class as `Ink.Runtime.Story` wrapper to enable people to use storylets without MonoBehaviour components.
- Added `Anansi.DialogueManager` to take on some of the responsibilities that once belonged to the `Anansi.StoryController` class.
- Added skip typewriter effect when space bar is pressed.
- Added `Anansi.SimulationController` class to manage ticking the simulation and updating characters' positions based on their schedules.

### Changed

- Background images and character sprites now live within the UI Canvas hierarchy in the scene. They are no longer presented in world-space.
- Combined various `Get__Input` external functions into a single function.
- Moved action location storylet management to the `GameManager`

### Removed

- `Anansi.StoryController` has been removed, and its responsibilities divided among the `GameManager`, `DialogueManager`, and `Story` classes.

## [0.2.0] - 2024-04-15

### Added

- Dynamic storylet choices using Ink external functions
- Dynamic storylet weights using Ink functions

## [0.1.0] - 2024-03-05

_Initial release._

[0.1.0]: https://github.com/ShiJbey/Anansi/releases/tag/v0.1.0
[0.2.0]: https://github.com/ShiJbey/Anansi/releases/tag/v0.2.0
[0.3.0]: https://github.com/ShiJbey/Anansi/releases/tag/v0.3.0
[0.4.0]: https://github.com/ShiJbey/Anansi/releases/tag/v0.4.0
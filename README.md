# Calypso

![Static Badge](https://img.shields.io/badge/Unity-2022.3-black)
![Static Badge](https://img.shields.io/badge/Version-1.0.0-green)

![calypso-screenshot](https://github.com/ShiJbey/Calypso/assets/11076525/810faa96-66f1-45d8-869d-94a0d7a4ecfc)

**Calypso** is a unity package that facilitates making simulation-like visual novel games using Inkle's scripting language, Ink. It combines Ink's powerful branching narrative capabilities with a storylet infrastructure and an underlying social simulation that tracks character relationships.

Calypso wants to help developers create stories that feel alive and dynamic. It does this using a storylet architecture that breaks up story content into various chunks selected dynamically at runtime. Calypso provides a custom language using Ink tags that allows authors to write reusable templated stories that dynamically cast characters into roles based on their relationship to the player and other characters.

## Features

- ⏰ DateTime system tracks the current week, day, and time of day (morning, noon, evening, etc.)
- 📍 Story locations update their backgrounds based on the time of day
- ❤️ Underlying relationships system to drive content selection
- ✏️ Write storylet content using [Ink](https://www.inklestudios.com/ink/)
- 🗓️ Characters move locations based on daily schedules

## Installation Instructions

The following are instructions for installing `Calypso` into your Unity project. This project uses semantic versioning. So, major version numbers will have breaking changes. Minor version changes will mostly contain new features, but there is always a chance of a breaking change. Please check the [CHANGELOG](./CHANGELOG.md) or release notes to see what changed between releases.

To add Calypso to your Unity project, you must download the latest version of the package from GitHub. Calypso is not available in the Unity Asset store. All releases are listed on the [Calypso GitHub Releases page](https://github.com/ShiJbey/Calypso/releases). Please follow the steps below.

1. Install dependencies
   1. Install [TDRS](https://github.com/ShiJbey/Unity-TDRS), Calypso's relationship system and social engine
2. Go to the Calypso GitHub releases page.
3. Find your desired release.
4. Download the `calypso_<VERSION>.tar.gz` from under the `Assets` dropdown (\<VERSION\> should be the release version you intend to download).
5. Open your project in Unity
6. Navigate to `Window > Package Manager` in the top menu.
7. Click the `+` icon in the top left and select `Add package from tarball...`.
8. Find and select the downloaded tarball
9. You should now see Calypso appear in the Unity Package Manager window with a version number matching your downloaded version.
10. lose the Package Manager window

## Potential future features

Below are features not currently supported by Calypso but are common in other visual novel tools. I will implement them if there is enough interest and time. If you want to try your hand at contributing new features to Calypso, create a new Issue for the feature (if one does not already exist), fork the repository, make your changes, and submit a pull request.

- Dialogue "auto-play"
- Dialogue "skip"
- Dialogue backlog/history
- Save/load
- Rollback (I don't plan to add this due to social simulation complexity)

## Documentation

You can find the latest documentation for Calypso in the [wiki](https://github.com/ShiJbey/Calypso/wiki). If something does not make sense, feels broken, or needs improvement, please create a new GitHub Issue.

## Projects using Calypso

If you built a game using Calypso, we would love to hear about it. Contact us, and we will add an image and link to your game below.

- TBD

## Frequently Asked Questions

### Why use Ink?

[Ink](https://www.inklestudios.com/ink/) is a great scripting language for creating branching narrative experiences. It's been battle-tested in multiple commercial games. It provides enough end-sure flexibility to allow frameworks like Calypso to leverage it as the content-authoring back-end. If you want to become more familiar with Ink, please check out [Inkle Studio's website](https://www.inklestudios.com) and their portfolio of narrative games.

### Can I use my own UI?

Yes. Calypso is UI-agnostic. The prefabs that come with it help you get up and running faster than starting from scratch.

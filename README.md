# Calypso

![Static Badge](https://img.shields.io/badge/Unity-2022.3-black)
![Static Badge](https://img.shields.io/badge/Version-1.0.0-green)

![calypso-screenshot](https://github.com/ShiJbey/Calypso/assets/11076525/810faa96-66f1-45d8-869d-94a0d7a4ecfc)

**Calypso** is a framework for creating social simulation-driven visual novel experiences in Unity. It combines the [Ink](https://www.inklestudios.com/ink/) narrative scripting language, a storylet architecture, and a social simulation that manages non-player character (NPC) schedules, emotions, personality traits, and relationships. Interactions with NPCs affect their feelings toward the player and other NPCs. Additionally, NPCs can reason about their relationship with the player and respond to events that happen to other characters in their social circle. For example, betraying a character will make their friends and family like you less. In short, adding this element of social simulation to visual novels should allow game developers to create dynamic and reactive interactive storytelling experiences more quickly than starting from scratch. Calypso draws most of its inspiration from _Persona 5's_ use of life simulation, time, and character relationships to drive narrative progression.

Calypso is a good solution for people who want to quickly port their Ink games to Unity. Setting up UI and dialogue management can be cumbersome. Calypso provides starting UI prefabs to help with this. Also, since Calypso is built on top of Ink, users can start with a normal Ink story and incrementally add Calypso-specific features into their games. While Calypso can handle almost any Ink story, it really shines with stories that revolve around character relationships and navigation from one location to another.

## Features

- ⏰ DateTime system tracks the current week, day, and time of day (morning, noon, evening, etc.)
- 📍 Story locations update their backgrounds based on the time of day
- ❤️ Underlying relationships system to drive content selection
- ✏️ Write story content using [Ink](https://www.inklestudios.com/ink/)
- 🗓️ Characters move locations based on daily schedules

## Installation Instructions

The following are instructions for installing `Calypso` into your Unity project. This project uses semantic versioning. So, major version numbers will have breaking changes. Minor version changes will mostly contain new features, but there is always a chance of a breaking change. Please check the [CHANGELOG](./CHANGELOG.md) or release notes to see what changed between releases.

Download the latest package version from GitHub to add Calypso to your Unity project. Calypso is not available in the Unity Asset store. All releases are listed on the [Calypso GitHub Releases page](https://github.com/ShiJbey/Calypso/releases). Please follow the steps below.

1. Install dependencies:
   1. Install Calypso's relationship system and social engine, [TDRS](https://github.com/ShiJbey/Unity-TDRS).
2. Go to the Calypso GitHub releases page.
3. Find your desired release.
4. Download the `calypso_<VERSION>.tar.gz` from under the `Assets` dropdown (`<VERSION>` should be the release version you intend to download).
5. Open your project in Unity
6. Navigate to `Window > Package Manager` in the top menu.
7. Click the `+` icon in the top left and select `Add package from tarball...`.
8. Find and select the downloaded tarball
9. You should now see Calypso appear in the Unity Package Manager window with a version number matching your downloaded version.
10. Close the Package Manager window

## Potential future features

Below are features not currently supported by Calypso but are common in other visual novel tools. I will implement them if there is enough interest and time.

- Dialogue "auto-play"
- Dialogue "skip"
- Dialogue backlog/history
- Save/load
- Choice rollback (I don't plan to add this due to social simulation complexity)

If you want to try your hand at contributing new features to Calypso:

1. Create a new Issue for the feature (if one does not already exist)
2. Fork the repository
3. Make your changes
4. Submit a pull request and describe your changes.

## Documentation

You can find the latest documentation for Calypso in the [wiki](https://github.com/ShiJbey/Calypso/wiki). If something does not make sense, feels broken, or needs improvement, please create a new GitHub Issue.

## Projects using Calypso

If you built a game using Calypso, we would love to hear about it. Please get in touch with us, and we will add an image and link to your game below. Also, remember to credit Calypso in your projects. More exposure helps the project grow.

- TBD

## Frequently Asked Questions

### What are storylets?

Storylets are an approach to narrative design where the narrative experience is broken into discrete pieces that are dynamically served to the player based on some preconditions. Emily Short provides a [helpful blog](https://emshort.blog/2019/11/29/storylets-you-want-them/) going into more depth about Storylets and various potential application areas. Also, Max Kreminski provides an [informative survey](https://mkremins.github.io/publications/Storylets_SketchingAMap.pdf) of the design space of storylets in games.

### Why use Ink?

[Ink](https://www.inklestudios.com/ink/) is a great scripting language for creating branching narrative experiences. It's been battle-tested in multiple commercial games. It provides enough end-sure flexibility to allow frameworks like Calypso to leverage it as the content-authoring back-end. If you want to become more familiar with Ink, please check out [Inkle Studio's website](https://www.inklestudios.com) and their portfolio of narrative games.

### Can I use my own UI?

Yes. Calypso is UI-agnostic. The UI prefabs that come with it help you get up and running faster than starting from scratch.

<h1 align="center">
  <img
    width="200"
    height="200"
    src="https://github.com/ShiJbey/Calypso/assets/11076525/46455e67-94b6-49e5-a072-980bd9ca6754"
  >
  <br>
  Anansi: Unity Toolkit for Simulation-Driven Visual Novels
</h1>

![Static Badge](https://img.shields.io/badge/Unity-2022.3-black)
![Static Badge](https://img.shields.io/badge/Version-0.5.0-green)

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/J3J0UNJ1Q)

![calypso-screenshot](https://github.com/ShiJbey/Calypso/assets/11076525/810faa96-66f1-45d8-869d-94a0d7a4ecfc)

**Anansi** is a Unity extension for creating social simulation-driven visual novels. It combines the [Ink](https://www.inklestudios.com/ink/) narrative scripting language, a storylet architecture, and a social simulation that manages non-player character (NPC) schedules, emotions, personality traits, and relationships. Interactions with NPCs affect their feelings toward the player and other NPCs. Additionally, NPCs can reason about their relationship with the player and respond to events that happen to other characters in their social circle. For example, betraying a character will make their friends and family like you less. In short, adding this element of social simulation to visual novels should allow game developers to create dynamic and reactive interactive storytelling experiences more quickly than starting from scratch. Anansi draws most of its inspiration from _Persona 5's_ use of life simulation, time, and character relationships to drive narrative progression.

Anansi is a good solution for people who want to quickly port their Ink games to Unity. Setting up UI and dialogue management can be cumbersome. Anansi provides starting UI prefabs to help with this. Also, since Anansi is built on top of Ink, users can start with a normal Ink story and incrementally add Anansi-specific features into their games. While Anansi can handle almost any Ink story, it really shines with stories that revolve around character relationships and navigation from one location to another.

## 🚀 Features

- ⏰ DateTime system tracks the current week, day, and time of day (morning, noon, evening, etc.)
- 📍 Story locations update their backgrounds based on the time of day
- ❤️ Underlying relationships system to drive content selection
- ✏️ Write story content using [Ink](https://www.inklestudios.com/ink/)
- 🗓️ Characters move locations based on daily schedules

## 📥 Installation Instructions

The following are instructions for installing `Anansi` into your Unity project. This is the recommended installation method because Anansi is not available in the Unity Asset store.

1. Open your project in Unity
2. Open the package manager window by selecting `Window > Package Manager` in the top menu.
3. Click the `+` icon in the top left, and select `Add package from git URL...`.
4. Paste the following URL: `https://github.com/ShiJbey/Anansi.git?path=/Packages/com.shijbey.anansi#v0.5.0`
5. This will install version `v0.5.0` to your project. If you want a different version change `v0.5.0` to any [tagged version](https://github.com/ShiJbey/Anansi/tags)
6. Wait, and you should see `Anansi` appear in the package manager window with a version matching your downloaded version.
7. Close the package manager window.

> [!TIP]
> This project uses semantic versioning. So, major version numbers will have breaking changes. Minor version changes will mostly contain new features, but there is always a chance of a breaking change. All pre-release version (sub 1.0) may contain breaking changes between minor version changes. Please check the [CHANGELOG](./CHANGELOG.md) to see what changed between releases.

## 😎 Potential future features

Below are features not currently supported by Anansi but are common in other visual novel tools. I will implement them if there is enough interest and time.

- Dialogue "auto-play"
- Dialogue "skip"
- Dialogue backlog/history
- Save/load
- Choice rollback (I don't plan to add this due to social simulation complexity)

If you want to try your hand at contributing new features to Anansi:

1. Create a new Issue for the feature (if one does not already exist)
2. Fork the repository
3. Make your changes
4. Submit a pull request and describe your changes.

## 📚 Documentation

You can find the latest documentation for Anansi in the [wiki](https://github.com/ShiJbey/Anansi/wiki). If something does not make sense, feels broken, or needs improvement, please create a new GitHub Issue.

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Please check the [Issues page](https://github.com/ShiJbey/Anansi/issues)

## ⭐️ Show your support

Don't forget to star ⭐️ this repo if this project has helped you or you find it interesting.

I release all my tools as free and open source projects. My graduate schooling has been partially funded by federal grants, paid for using tax-dollars from hard-working, everyday people. Thank you to those who make it possible for student like me to do what I do.

## 📄 License

This project is [MIT](./LICENSE.md) licensed.

## 👀 Projects using Anansi

If you built a game using Anansi, we would love to hear about it. Please get in touch with us, and we will add an image and link to your game below. Also, remember to credit Anansi in your projects. More exposure helps the project grow.

- TBD

## ❓Frequently Asked Questions

### What are storylets?

Storylets are an approach to narrative design where the narrative experience is broken into discrete pieces that are dynamically served to the player based on some preconditions. Emily Short provides a [helpful blog](https://emshort.blog/2019/11/29/storylets-you-want-them/) going into more depth about Storylets and various potential application areas. Also, Max Kreminski provides an [informative survey](https://mkremins.github.io/publications/Storylets_SketchingAMap.pdf) of the design space of storylets in games.

### Why use Ink?

[Ink](https://www.inklestudios.com/ink/) is a great scripting language for creating branching narrative experiences. It's been battle-tested in multiple commercial games. It provides enough end-sure flexibility to allow frameworks like Anansi to leverage it as the content-authoring back-end. If you want to become more familiar with Ink, please check out [Inkle Studio's website](https://www.inklestudios.com) and their portfolio of narrative games.

### Can I use my own UI?

Yes. Anansi is UI-agnostic. The UI prefabs that come with it help you get up and running faster than starting from scratch.

### Wasn't this project named Calypso? Why Anansi?

Yes it was. I had named it after Naomie Harris' character [Tia Dalma/Calypso](https://en.wikipedia.org/wiki/Tia_Dalma) in Pirates of the Caribbean. However, the greek mythology around Calypso, as depicted in the Odyssey, is better known and I didn't want the two that directly associated.

[Anansi](https://en.wikipedia.org/wiki/Anansi) is an Akan storytelling diety, often depicted as a spider. So, the name fits perfectly. Plus Anansi makes for a bangin' logo.

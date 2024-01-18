# Calypso

![Static Badge](https://img.shields.io/badge/WIP-red)
![Static Badge](https://img.shields.io/badge/Unity-2022.3-black)
![Static Badge](https://img.shields.io/badge/Status-Unstable-red)
![Static Badge](https://img.shields.io/badge/Version-0.1.0-yellow)

![calypso-screenshot](https://github.com/ShiJbey/Calypso/assets/11076525/810faa96-66f1-45d8-869d-94a0d7a4ecfc)

**This project is still under development and is not ready for use.**

Calypso is a unity package for creating simulation-driven visual novel games. It uses a storylet architecture that breaks up NPC dialogue content into various chunks that are selected dynamically at runtime. Calypso combines a relationship system with a query syntax that allows authors to write templated stories that dynamically cast characters into roles based on their relationship to the player and other characters. This allows designers to create reusable dynamic dialogue experiences that adapt to player choices without being locked-in to a specific cast of characters. So, as you add characters to your game, they are automatically eligible to engage in previously authored dialogues.

Calypso wants to help developers create stories that feel alive and dynamic. Conversation dialog is affected by the player's actions and the social simulation. Also, actions have cascading effects on character relationships. For example, selecting a dialog option that insults a character might make their friends dislike the player.

## Features

- ⏰ DateTime system tracks the current week, day and time of day (morning, noon, evening, etc.)
- 📍 Story locations update their backgrounds based on the time of day
- ❤️ Trait-driven relationship system
- ✏️ Write storylet dialogue using [Ink](https://www.inklestudios.com/ink/)
- 🗓️ Characters move locations based on individual schedules

## Dynamic dialog using Ink and RePraxis

Conversation dialog is authored using [Ink](https://www.inklestudios.com/ink/), a popular interactive fiction writing tool that was used to make games such as [80 Days](https://www.inklestudios.com/80days/). It allows writers to create branching and reactive dialogue choices that can easily integrate with and take advantage of C#.

We created a query language extension for the Ink called *RePraxis* that allows writers to precondition conversations and dynamically cast information from the game. It is inspired by the *Praxis* logic language used by [*Versu*](https://versu.com/), a social simulation and interactive fiction engine.

Each character in Calypso is given a collection of conversation scripts, and they choose probabilistically from this collection when interacting with the player. Conversations may have preconditions that affect their availability. For example, they might check for if the friendship score between the player and a character is past a given threshold. Also, preconditions can store variable values in Ink. This feature allows us to dynamically cast characters into conversations, creating more dynamic dialog that adjusts to any configuration of NPCs.

## Planned features

- [x] NPC schedule System
- [x] Simulation calendar
- [ ] Tick-update system
- [x] Author new conversation dialogue using Ink
- [x] Weighted-random conversation selection
- [x] Precondition query language
- [ ] Location selection menu
- [x] Relationship system
- [ ] Control the simulation from within conversations

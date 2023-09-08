# Calypso

![Static Badge](https://img.shields.io/badge/WIP-red)
![Static Badge](https://img.shields.io/badge/Unity-2022.3-black)
![Static Badge](https://img.shields.io/badge/Status-Unstable-red)
![Static Badge](https://img.shields.io/badge/Version-0.1.0-yellow)

![calypso-screenshot](https://github.com/ShiJbey/Calypso/assets/11076525/810faa96-66f1-45d8-869d-94a0d7a4ecfc)

**This project is still under development and is not ready for use.**

Calypso is a unity package for creating simulation-driven visual novels. It enhances visual novels' prototypical branching narrative structure by simulating characters' lives outside their conversations with the player and their relationships with other characters. We provide a query language extension for dialog scripting that allows developers to create flexible conversation scripts that adjust to the current cast of characters. This project aims to assist visual novel developers in creating more dynamic and unique stories than can be accomplished solely using conventional branching narrative tools such as [*Twine*](https://twinery.org/). 

## Simulating the characters' lives and relationships

Calypso wants to help developers create stories that feel alive and dynamic. It supports storytelling with an underlying social simulation that handles non-player characters' (NPCs) schedules and social lives outside their conversations with the player. NPCs can move between locations and interact with other present NPCs. Conversation dialog is affected by the player's actions and background simulation. Also, actions have cascading effects on character relationships. For example, selecting a dialog option that insults a character might make their friends dislike the player.

## Dynamic dialog using Ink and RePraxis

Conversation dialog is authored using [Ink](https://www.inklestudios.com/ink/), a popular interactive fiction writing tool that was used to make games such as [80 Days](https://www.inklestudios.com/80days/). It allows writers to create branching and reactive dialogue choices that can easily integrate with and take advantage of C#. 

We created a query language extension for the Ink called *RePraxis* that allows writers to precondition conversations and dynamically cast information from the game. It is inspired by the *Praxis* logic language used by [*Versu*](https://versu.com/), a social simulation and interactive fiction engine. 

Each character in Calypso is given a collection of conversation scripts, and they choose probabilistically from this collection when interacting with the player. Conversations may have preconditions that affect their availability. For example, they might check for if the friendship score between the player and a character is past a given threshold. Also, preconditions can store variable values in Ink. This feature allows us to dynamically cast characters into conversations, creating more dynamic dialog that adjusts to any configuration of NPCs.

## Planned features

- [x] NPC schedule System
- [x] Simulation calendar
- [ ] Tick-update system
- [x] Weighted-random conversation selection
- [x] Precondition query language
- [ ] Location selection menu
- [ ] Relationship system
- [ ] Support for dynamic NPC sprites

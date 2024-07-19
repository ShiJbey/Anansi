// Main.ink - This file is the central entry point for the sample game.

/* ---------------------------------

Including Content
=================

All ink content needs to included into a central Ink file. For this sample,
we use this Main.ink file as the central file passed to the dialogue manager
to create the Anansi Story instance. All file paths are given relative to this
ink file. "INCLUDE path/to/file.ink" is all you need to add for the storylets
inside to be processed.

----------------------------------*/

INCLUDE ./EvelynDialogue.ink
INCLUDE ./SampleDialogue.ink

/* ---------------------------------

Accessing External Functions
============================

Anansi provides various external functions to assist with communicating with
external systems such as the GUI, Story Database, and Time System. By default,
users are provided with functions for database operations (DbInsert, DbDelete,
DbAssert), queueing storylets using an ID, and queueing storylets using tags.

Since the full Anansi library is for creating location-based visual novels
with simulated characters, the game manager provides additional external
functions for changing the location of the player, forwarding time, and
requesting user input.

 ----------------------------------*/

EXTERNAL SetPlayerLocation(locationId)
EXTERNAL DbInsert(statement)
EXTERNAL DbDelete(statement)
EXTERNAL DbAssert(statement)
EXTERNAL AdvanceTime()
EXTERNAL QueueStorylet(storyletId)
EXTERNAL QueueStoryletWithTags(tags, fallback)
EXTERNAL GetInput(dataType, prompt, variableName)
EXTERNAL DivertToStorylet(knot)
EXTERNAL GetEligibleActions()
EXTERNAL GetEligibleLocations()


/* ---------------------------------

Global Variables
================

Below we define some global variables. These are regular Ink variables. When
you want to use a value from the RePraxis database within Ink, you will need
to store it into an Ink variable. Because Ink does not know about these
variables, you will need to define the variable here ahead of time. Global
variables are shared across all storylets.

 ----------------------------------*/

VAR PlayerName = "player"
VAR StudentID = 0

/* ---------------------------------

The Start Storylet
==================

Below, is the "start" storylet. Anansi looks for this storylet when starting the
story. This storylet is not required when using the Anansi.Story class in your
own projects, but it is required when using the entire framework. Inside of this
knot, designers should set the current time of day and location. This will move
all characters to where they need to be. Optionally, you can include prologue
content under this knot to help introduce the player to the game.

 ----------------------------------*/

=== start ===
#---
#===

// Place initial state setup here.

// {DivertToStorylet(-> library)}

-> library

-> DONE

/* ---------------------------------

Location Storylets
==================

Anansi divides storylets into three categories: basic storylets, location
storylets, and action storylets. Below, we have a location storylet. These
storylets correspond to locations the player can travel to the game. They
can have as much or as little content as you wish. However, they should
always include a call to the "SetPlayerLocation" function. This function
updates the player's location in the simulation and updates the background
to match. Location storylets must include "location" in their comma-
separated list of tags. They should also set themselves as hidden using
the "hidden: true" command.

 ----------------------------------*/

=== library ===
#---
# choiceLabel: Go to Library.
# hidden: true
# tags: location
#===

{SetPlayerLocation("library")}

- What do you want to do?
    <- get_actions
    + [Look around] -> examine_library -> library
- -> DONE

=== examine_library

There are a lot of books in here.

->->

=== get_actions
    + [Get Pizza] -> library
    + {test()} [Eat Waffles] -> library
    + {GetEligibleActions()} [Storylet Options]
    + {GetEligibleLocations()} [Storylet Options]
    - -> DONE


=== function test()
    ~return true

/* ---------------------------------

Action Storylets
================

Below is an example of an action storylet. All action storylets must
"action" in their list of tags. Also, we recommend setting the
storylet as hidden to preventing from being selected when queueing
storylets using tags. In this sample, actions are mostly used to
manage character interactions, and giving the player options for
other things to do.

Below, we have an action for taking a nap when the player is within
their dorm room. The "@query" part of the storylet header ensures
this action is only available when the player is in their room. Also,
the storylet calls the "AdvanceTime" function to simulate time
passing while the player naps.

 ----------------------------------*/


=== nap_in_room ===
# ---
# choiceLabel: Take a nap
# @query
# player.location!player_dormroom
# @end
# hidden: true
# tags: action
# ===

You decided to take a nap...

{AdvanceTime()}

Time has advanced.

-> DONE

/* ---------------------------------

Obtaining User Input
====================

Natively, Ink does not to my knowledge have a way of obtaining
input from the user. Anansi's drama manager provides a "GetInput"
external function to help with this. When called, the UI opens
a dialog box containing a prompt and a text entry box. When users
click "submit" the value in the text entry box is stored in the
provided variable name. This is one of the cases where you will
probably want to declare the Ink variable beforehand and give it
a default value.

The "GetInput" function takes three parameters: the data type of
the input, a text prompt, and the name of a variable to store the
result in. The valid input data types are: text, string, int,
float, and number.

 ----------------------------------*/

=== outside_library ===
#---
# choiceLabel: Go to Outside Library.
# hidden: true
# tags: location
#===

{SetPlayerLocation("outside_library")}

{outside_library == 1:
    You're standing outside of the campus library.

    You look at your Student ID. The name says... {GetInput("text", "What is your name?", "PlayerName")}

    It says {PlayerName}. They usually spell it wrong, but the registrar manager had the same name.

    What are the chances of that?

    Your student ID number is...{GetInput("int", "Enter student ID...", "StudentID")}
}

-> DONE

=== dining_hall ===
#---
# choiceLabel: Go to Dining Hall.
# hidden: true
# tags: location
#===

{SetPlayerLocation("dining_hall")}

-> DONE

=== gym ===
#---
# choiceLabel: Go to Gym.
# hidden: true
# tags: location
#===

{SetPlayerLocation("gym")}

-> DONE

=== classroom ===
#---
# choiceLabel: Go to Classroom.
# hidden: true
# tags: location
#===

{SetPlayerLocation("classroom")}

-> DONE

=== campus_walkway ===
#---
# choiceLabel: Go to Campus Walkway.
# hidden: true
# tags: location
#===

{SetPlayerLocation("campus_walkway")}

-> DONE

=== evelyn_dormroom ===
#---
# choiceLabel: Go to Evelyn's room.
# hidden: true
# tags: location
#===

{SetPlayerLocation("evelyn_dormroom")}

{
    - DbAssert("evelyn.location!evelyn_dormroom"):
        You are in Evelyn's room. She is looking looking out the window, listening to music
    - else:
        Evelyn is not here.
}

-> DONE

=== astrid_dormroom ===
#---
# choiceLabel: Go to Astrid's room.
# hidden: true
# tags: location
#===

{SetPlayerLocation("astrid_dormroom")}

{
    - DbAssert("astrid.location!astrid_dormroom"):
        Astrid is studying at her desk.
    - else:
        Astrid is not here.
}

-> DONE

=== player_dormroom ===
#---
# choiceLabel: Go to your room.
# hidden: true
# tags: location
#===

{SetPlayerLocation("player_dormroom")}

You're in your room.

-> DONE

/* ---------------------------------

Queuing Storylets
=================

When you want to dynamically instantiate and jump to another
storylet in the story, you can use the "QueueStorylet" or
"QueueStoryletWithTags" external functions included with the
base Anansi.Story class.

The examples below looks for all storylets that are tagged
as conversations (convo) for specific characters. If no
storylet content is available to jump to, we provide the
ink knot name of a fallback storylet to use. Notice, the
content for these potential storylets is split among multiple
Ink files.

 ----------------------------------*/


=== talk_to_astrid ===
# ---
# choiceLabel: Talk to Astrid
# @query
# astrid.location!?loc
# player.location!?loc
# @end
# hidden: true
# tags: action
# ===

~speaker = "astrid"
{QueueStoryletWithTags("convo, astrid", "conversation_fallback")}

-> DONE

=== talk_to_evelyn ===
# ---
# choiceLabel: Talk to Evelyn
# @query
# evelyn.location!?loc
# player.location!?loc
# @end
# hidden: true
# tags: action
# ===

~speaker = "evelyn"
{QueueStoryletWithTags("evelyn, convo", "conversation_fallback")}

-> DONE

=== talk_to_momo ===
# ---
# choiceLabel: Talk to Momo
# @query
# momo.location!?loc
# player.location!?loc
# @end
# hidden: true
# tags: action
# ===

~speaker = "momo"
{QueueStoryletWithTags("convo, astrid", "conversation_fallback")}

-> DONE

=== talk_to_giyu ===
# ---
# choiceLabel: Talk to Giyu
# @query
# giyu.location!?loc
# player.location!?loc
# @end
# hidden: true
# tags: action
# ===

~speaker = "giyu"
{QueueStoryletWithTags("convo", "conversation_fallback")}

-> DONE


=== conversation_fallback ===
# ---
# ===

They have nothing to say to you.

-> DONE

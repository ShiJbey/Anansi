INCLUDE ./EvelynDialogue.ink
INCLUDE ./SampleDialogue.ink

// Calypso provides various external functions to assist with communicating with external systems such as the GUI, Story Database, and Time System.

EXTERNAL SetLocation(location, tags)
EXTERNAL SetBackgroundOnly(location, tags)
EXTERNAL DbInsert(statement)
EXTERNAL DbDelete(statement)
EXTERNAL DbAssert(statement)
EXTERNAL AdvanceTime()
EXTERNAL QueueStorylet(storyletId)
EXTERNAL QueueStoryletWithTags(tags, fallback)
EXTERNAL GetInput(prompt, variableName, callback)

// Global variables are shared across all storylets because they use the same story instance. Storylet queries may overwrite these values. So, be mindful of your variable usage

// VAR speaker = "not-specified"
// VAR player = "player"
// VAR location = "not-specified"
VAR timesKnocked = 0

// Do not use any top-level diverts. The StoryController will automatically jump to the "start" knot.

=== start ===

// The system will always look for a knot with the name "start" to execute first. Inside of this knot, designers should set the current time of day and location. This will move all characters to where they need to be. Optionally, you can include prologue content under this knot to help introduce the player to the game.

-> location_outside_library

-> DONE

=== action_nap_in_room ===
# ---
# choiceLabel: Take a nap
# @query
# player.location!player_dormroom
# @end
# ===

You decided to take a nap...

{AdvanceTime()}

Time has advanced. 

-> DONE


=== function _knock_evelyns_room() ===
// Storylets can supply an optional precondition function that evaluates if the storylet is eligible to be shown as a potential action. This function serves as a precondition check, but it does not perform the casting operations required to make an instance of the its corresponding storylet.

// The precondition function is the same base name of the storylet/location/action with a leading underscore.

~ return true

=== location_outside_library ===
#---
# choiceLabel: Go to Outside Library.
#===

You're standing outside of the campus library. {SetLocation("outside_library", "")}

-> DONE

=== location_dining_hall ===
#---
# choiceLabel: Go to Dining Hall.
#===

{SetLocation("dining_hall", "")}

-> DONE

=== location_library ===
#---
# choiceLabel: Go to Library.
#===

{SetLocation("library", "")}

-> DONE

=== location_gym ===
#---
# choiceLabel: Go to Gym.
#===

{SetLocation("gym", "")}

-> DONE

=== location_classroom ===
#---
# choiceLabel: Go to Classroom.
#===

{SetLocation("classroom", "")}

-> DONE

=== location_campus_walkway ===
#---
# choiceLabel: Go to Campus Walkway.
#===

{SetLocation("campus_walkway", "")}

-> DONE

=== location_evelyn_dormroom ===
#---
# choiceLabel: Go to Evelyn's room.
#===

{SetLocation("evelyn_dormroom", "")}

{
    - DbAssert("evelyn.location!astrid_dormroom"):
        You are in Evelyn's room. She is looking looking out the window, listening to music
    - else:
        Evelyn is not here.
}

-> DONE

=== location_astrid_dormroom ===
#---
# choiceLabel: Go to Astrid's room.
#===

{SetLocation("astrid_dormroom", "")}

{DbAssert("astrid.location!astrid_dormroom") == false:
    Astrid is not here.
}

-> DONE

=== location_player_dormroom ===
#---
# choiceLabel: Go to your room.
#===

{SetLocation("player_dormroom", "")}

You're in your room. 

-> DONE

=== action_talk_to_astrid ===
# ---
# choiceLabel: Talk to Astrid
# @query
# astrid.location!?loc
# player.location!?loc
# @end
# ===

~speaker = "astrid"
{QueueStoryletWithTags("convo, astrid", "storylet_conversation_fallback")}

-> DONE

=== action_talk_to_evelyn ===
# ---
# choiceLabel: Talk to Evelyn
# @query
# evelyn.location!?loc
# player.location!?loc
# @end
# ===

~speaker = "evelyn"
{QueueStoryletWithTags("evelyn, convo", "storylet_conversation_fallback")}

-> DONE

=== action_talk_to_momo ===
# ---
# choiceLabel: Talk to Momo
# @query
# momo.location!?loc
# player.location!?loc
# @end
# ===

~speaker = "momo"
{QueueStoryletWithTags("convo, astrid", "storylet_conversation_fallback")}

-> DONE

=== action_talk_to_giyu ===
# ---
# choiceLabel: Talk to Giyu
# @query
# giyu.location!?loc
# player.location!?loc
# @end
# ===

~speaker = "giyu"
{QueueStoryletWithTags("convo", "storylet_conversation_fallback")}

-> DONE


=== storylet_conversation_fallback ===

They have nothing to say to you.

-> DONE


-> END

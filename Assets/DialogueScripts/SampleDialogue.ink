// Global variables
VAR speaker = "speaker"
VAR player = "player"
VAR location = "location"
VAR ChoseRedPill = false
VAR HealthPoints = 50
VAR other_character = "other_character"

=== storylet_sample_conversation ===
# ---
# choice_label: 
# tags: convo, astrid
# @query
# ?speaker.relationships.?other.traits.friend
# neq ?other ?player
# ?player.relationships.?other.traits.rival
# @end
# @set other_character to ?other
# ===

{speaker}.happy: Hello! This is the starting knot!

{speaker}: Now, we'll go to knot 2!

-> knot_2
-> DONE


=== knot_2 ===
{speaker}: Hello from knot 2!

{other_character}: Time for a personality test.

{speaker}: Red pill or blue pill?

*** Red pill
~ChoseRedPill = true // update variable value
-> red_pill
*** Blue pill
~HealthPoints -= 20 // update variable value
-> blue_pill

-> DONE

=== red_pill ===

{speaker}: My god, how brave!

-> continue_conversation

-> DONE

=== blue_pill ===

{speaker}: Bold move, my friend

-> continue_conversation

-> DONE

=== continue_conversation ===

{HealthPoints < 50:

    {speaker}: You seem quite weak. I wonder why...
    
}

{speaker}: Alright. You have answered my question.

{ ChoseRedPill:

{other_character}: You chose the red pill. But I'm still not sure I can trust you
}

{not red_pill: -> no_red_pill_comment}

-> DONE

=== no_red_pill_comment ===

{speaker}: You didn't choose the red pill. I'm not sure I can trust you.

-> DONE

-> END
 trust you.

-> DONE

-> END
trust you.

-> DONE

-> END
